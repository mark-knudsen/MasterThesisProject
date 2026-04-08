using System;
using LLVMSharp;
using LLVMSharp.Interop;
using System.Linq;
using System.Reflection; // for using the stopwatch!
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualBasic;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using System.Text;
using System.Reflection.Metadata.Ecma335;

namespace MyCompiler
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)] // Pack 8 is standard for 64-bit pointers
    public struct RuntimeValue
    {
        public Int16 tag;
        // The compiler adds 6 bytes of padding here automatically to align the pointer to 8
        public IntPtr data;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    struct ArrayObject
    {
        public long length;    // i64
        public long capacity;  // i64
        public IntPtr data;    // pointer
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    struct DataframeObject
    {
        public IntPtr columns; // array<string>
        public IntPtr rows;    // array<ptr to record>
        public IntPtr dataTypes;
    }

    public enum ValueTag
    {
        Int = 1,
        Float = 2,
        Bool = 3,
        String = 4,
        Array = 5,
        Record = 6,
        Dataframe = 7,
        None = 0
    }

    public static class LanguageRuntime
    {
        // Import the same malloc your LLVM code uses (msvcrt on Windows, libc on Linux/macOS)
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr malloc(IntPtr size);

        public static IntPtr ReadCsvInternal(IntPtr pathPtr, IntPtr schemaPtr)
        {
            string path = Marshal.PtrToStringAnsi(pathPtr);
            string schema = Marshal.PtrToStringAnsi(schemaPtr);

            if (!File.Exists(path)) return IntPtr.Zero;

            var lines = File.ReadAllLines(path).Skip(1).ToArray();
            int rowCount = lines.Length;

            // --- THE NATIVE WAY ---
            // 1. Allocate Array Header { i64, i64, ptr } (24 bytes)
            IntPtr rowsArrayHeader = malloc((IntPtr)24);

            // 2. Allocate Data Buffer (rowCount * 8 bytes)
            IntPtr rowsDataBuffer = malloc((IntPtr)(rowCount * 8));

            for (int i = 0; i < rowCount; i++)
            {
                string[] parts = lines[i].Split(',');
                // Allocate Record Buffer
                IntPtr recordBuffer = malloc((IntPtr)(parts.Length * 8));

                for (int col = 0; col < parts.Length; col++)
                {
                    char typeCode = col < schema.Length ? schema[col] : 'S';
                    string rawValue = parts[col].Trim();

                    // Allocate value box (8 bytes)
                    IntPtr valuePtr = malloc((IntPtr)8);

                    if (typeCode == 'I')
                    {
                        long.TryParse(rawValue, out long val);
                        Marshal.WriteInt64(valuePtr, val);
                    }
                    else if (typeCode == 'F')
                    {
                        // Add System.Globalization.CultureInfo.InvariantCulture here
                        if (double.TryParse(rawValue, System.Globalization.CultureInfo.InvariantCulture, out double val))
                        {
                            long bits = BitConverter.DoubleToInt64Bits(val);
                            Marshal.WriteInt64(valuePtr, bits);
                        }
                        else
                        {
                            // Fallback for safety
                            Marshal.WriteInt64(valuePtr, 0);
                        }
                    }
                    else if (typeCode == 'B')
                    {
                        bool val = rawValue.ToLower() == "true" || rawValue == "1";
                        Marshal.WriteInt64(valuePtr, val ? 1 : 0);
                    }
                    else
                    { // String
                      // Strings are tricky; StringToHGlobalAnsi uses Marshal's heap.
                      // For a 100% "Real" IR way, you'd malloc + strcpy here too.
                        IntPtr strPtr = Marshal.StringToHGlobalAnsi(rawValue);
                        malloc((IntPtr)8); // dummy to match your previous logic if needed
                        valuePtr = strPtr;
                    }
                    Marshal.WriteIntPtr(recordBuffer, col * 8, valuePtr);
                }
                Marshal.WriteIntPtr(rowsDataBuffer, i * 8, recordBuffer);
            }

            // Initialize Header: Length, Capacity, DataPtr
            Marshal.WriteInt64(rowsArrayHeader, 0, rowCount);
            Marshal.WriteInt64(rowsArrayHeader, 8, rowCount);
            Marshal.WriteIntPtr(rowsArrayHeader, 16, rowsDataBuffer);

            // 3. Wrap in RuntimeValue { i64 tag, ptr data } (16 bytes)
            IntPtr runtimeBox = malloc((IntPtr)16);
            Marshal.WriteInt64(runtimeBox, 0, 7); // Tag 7 = Dataframe
            Marshal.WriteIntPtr(runtimeBox, 8, rowsArrayHeader);

            return runtimeBox;
        }


        public static void ToCsvInternal(IntPtr dfPtr, IntPtr pathPtr)
        {
            string path = Marshal.PtrToStringAnsi(pathPtr);

            // 1. Unpack Dataframe { ptr columns, ptr rows, ptr types }
            IntPtr columnsArrayPtr = Marshal.ReadIntPtr(dfPtr, 0);
            IntPtr rowsArrayPtr = Marshal.ReadIntPtr(dfPtr, 8);
            IntPtr typesArrayPtr = Marshal.ReadIntPtr(dfPtr, 16);

            // 2. Get Metadata (Lengths) from Array Headers
            long colCount = Marshal.ReadInt64(columnsArrayPtr, 0);
            long rowCount = Marshal.ReadInt64(rowsArrayPtr, 0);

            // Get raw buffers
            IntPtr colDataBuf = Marshal.ReadIntPtr(columnsArrayPtr, 16);
            IntPtr rowDataBuf = Marshal.ReadIntPtr(rowsArrayPtr, 16);
            IntPtr typeDataBuf = Marshal.ReadIntPtr(typesArrayPtr, 16);

            using (StreamWriter writer = new StreamWriter(path))
            {
                // 3. Write Header Row
                List<string> colNames = new List<string>();
                for (int i = 0; i < colCount; i++)
                {
                    IntPtr namePtr = Marshal.ReadIntPtr(colDataBuf, i * 8);
                    colNames.Add(Marshal.PtrToStringAnsi(namePtr));
                }
                writer.WriteLine(string.Join(",", colNames));

                // 4. Write Data Rows
                for (int i = 0; i < rowCount; i++)
                {
                    IntPtr recordPtr = Marshal.ReadIntPtr(rowDataBuf, i * 8);
                    List<string> rowValues = new List<string>();

                    for (int j = 0; j < colCount; j++)
                    {
                        IntPtr valBoxPtr = Marshal.ReadIntPtr(recordPtr, j * 8);
                        int typeCode = (int)Marshal.ReadInt64(typeDataBuf, j * 8); // 1=Int, 2=Float, 3=Bool, 4=String

                        string cellValue = "";
                        if (typeCode == 1) // Int
                        {
                            cellValue = Marshal.ReadInt64(valBoxPtr).ToString();
                        }
                        else if (typeCode == 2) // Float (Double)
                        {
                            long bits = Marshal.ReadInt64(valBoxPtr);
                            cellValue = BitConverter.Int64BitsToDouble(bits).ToString(CultureInfo.InvariantCulture);
                        }
                        else if (typeCode == 3) // Bool
                        {
                            cellValue = Marshal.ReadInt64(valBoxPtr) == 1 ? "true" : "false";
                        }
                        else if (typeCode == 4) // String
                        {
                            // Remember: for strings, valBoxPtr IS the pointer to the string
                            cellValue = Marshal.PtrToStringAnsi(valBoxPtr);
                        }
                        rowValues.Add(cellValue);
                    }
                    writer.WriteLine(string.Join(",", rowValues));
                }
            }
            Console.WriteLine($"--- C# Runtime: CSV saved to {path} ---");
        }

        public static void EnsureCapacityInternal(IntPtr arrayHeaderPtr)
        {
            long len = Marshal.ReadInt64(arrayHeaderPtr, 0);
            long cap = Marshal.ReadInt64(arrayHeaderPtr, 8);

            if (len >= cap)
            {
                long newCap = cap == 0 ? 4 : cap * 2;
                IntPtr oldData = Marshal.ReadIntPtr(arrayHeaderPtr, 16);

                // Use C# to handle the resizing logic
                IntPtr newData = Marshal.AllocHGlobal((int)(newCap * 8));
                // Copy old data to new data
                unsafe { Buffer.MemoryCopy((void*)oldData, (void*)newData, newCap * 8, len * 8); }

                Marshal.WriteInt64(arrayHeaderPtr, 8, newCap);
                Marshal.WriteIntPtr(arrayHeaderPtr, 16, newData);
                // Note: You might want to free oldData here, 
                // but be careful if it came from a constant
            }
        }

    }

    public unsafe class CompilerOrc : IExpressionVisitor, ICompiler
    {
        private LLVMModuleRef _module;
        private LLVMBuilderRef _builder;
        private IntPtr _jit; // JIT execution pointer
        private LLVMValueRef _printf;
        private LLVMTypeRef _printfType;
        private LLVMValueRef _trueStr;
        private LLVMValueRef _falseStr;
        private bool _jitInitialized = false;
        private string _funcName;
        private int _replCounter = 0;
        private bool _debug = true;
        LLVMTypeRef _memmoveType;
        LLVMTypeRef _reallocType;
        private Type _lastType; // Store the type of the last expression for auto-printing
        //private Node _lastNode; // Store the last expression for auto-printing
        private LLVMTypeRef _runtimeValueType;
        HashSet<string> _definedGlobals = new(); // wouldn't we use our context for this job?
        private Context _context;
        public Context GetContext() => _context;
        public void ClearContext() => _context = Context.Empty;

        private LLVMTypeRef _mallocType;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MainDelegate();

        private ReadCsvDelegate _readCsvDelegate;
        private ToCsvDelegate _toCsvDelegate;
        public delegate IntPtr ReadCsvDelegate(IntPtr path, IntPtr schema);
        public delegate void ToCsvDelegate(IntPtr data, IntPtr path);

        public CompilerOrc()
        {
            LLVM.InitializeNativeTarget();
            LLVM.InitializeNativeAsmPrinter();
            LLVM.InitializeNativeAsmParser();

            // Initialize the JIT only once
            EnsureJit();

            // Create the module and builder for the entire session
            _module = LLVMModuleRef.CreateWithName("repl_module");
            _builder = _module.Context.CreateBuilder();

            _context = Context.Empty; // stores variables/functions

            // Declare malloc and other required functions
            DeclareValueStruct();
            SetupCsvFunctions();
        }

        private unsafe void RegisterNativeFunc(LLVMOrcOpaqueJITDylib* dylib, string name, IntPtr fnAddr)
        {
            var jitHandle = (LLVMOrcOpaqueLLJIT*)_jit;
            fixed (byte* pName = System.Text.Encoding.UTF8.GetBytes(name + "\0"))
            {
                var internedName = LLVM.OrcLLJITMangleAndIntern(jitHandle, (sbyte*)pName);

                // FIX: In v20 it is LLVMJITEvaluatedSymbol (No 'Orc')
                var symbol = new LLVMJITEvaluatedSymbol
                {
                    Address = (ulong)fnAddr,
                    Flags = new LLVMJITSymbolFlags { GenericFlags = (byte)1 }
                };

                var pair = new LLVMOrcCSymbolMapPair { Name = internedName, Sym = symbol };
                LLVM.OrcJITDylibDefine(dylib, LLVM.OrcAbsoluteSymbols(&pair, 1));
            }
        }

        private unsafe void SetupCsvFunctions()
        {
            var jitHandle = (LLVMOrcOpaqueLLJIT*)_jit;
            var dylib = LLVM.OrcLLJITGetMainJITDylib(jitHandle);

            // Register ReadCsvInternal
            RegisterNativeFunc(dylib, "ReadCsvInternal",
                Marshal.GetFunctionPointerForDelegate(_readCsvDelegate = new ReadCsvDelegate(LanguageRuntime.ReadCsvInternal)));

            // Register ToCsvInternal
            RegisterNativeFunc(dylib, "ToCsvInternal",
                Marshal.GetFunctionPointerForDelegate(_toCsvDelegate = new ToCsvDelegate(LanguageRuntime.ToCsvInternal)));
        }

        private LLVMValueRef GetOrDeclareMalloc()
        {
            var i64 = _module.Context.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);

            // ptr malloc(i64)
            if (_mallocType.Handle == IntPtr.Zero)
            {
                _mallocType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i64 }, false);
            }

            var mallocFn = _module.GetNamedFunction("malloc");
            if (mallocFn.Handle == IntPtr.Zero)
            {
                mallocFn = _module.AddFunction("malloc", _mallocType);
            }
            return mallocFn;
        }

        private void DeclareValueStruct()
        {
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // Use a Named Struct so it's easy to see in IR
            _runtimeValueType = ctx.CreateNamedStruct("RuntimeValue");
            // Ensure this matches your C# 'public Int16 tag; public IntPtr data;'
            _runtimeValueType.StructSetBody(new[] { ctx.Int16Type, i8Ptr }, false);
        }

        private void DeclarePrintf()
        {
            var llvmCtx = _module.Context;
            _printfType = LLVMTypeRef.CreateFunction(
                  llvmCtx.Int32Type,
                new[] { LLVMTypeRef.CreatePointer(llvmCtx.DoubleType, 0) }, // should this be a double?
                true); // varargs

            _printf = _module.AddFunction("printf", _printfType);
        }

        private void DeclareBoolStrings()
        {
            _trueStr = _builder.BuildGlobalStringPtr("True\n", "true_str");
            _falseStr = _builder.BuildGlobalStringPtr("False\n", "false_str");
        }

        private void EnsureJit()
        {
            if (_jitInitialized)
                return;

            // Create JIT compiler (this assumes you are using LLVM's Orc JIT engine)
            ThrowIfError(OrcBindings.LLVMOrcCreateLLJIT(out _jit, IntPtr.Zero));
            _jitInitialized = true;
        }

        private void ThrowIfError(IntPtr err)
        {
            if (err == IntPtr.Zero) return;

            var msgPtr = OrcBindings.LLVMGetErrorMessage(err);
            var message = Marshal.PtrToStringAnsi(msgPtr);
            Console.WriteLine("JIT Error: " + message);
            OrcBindings.LLVMDisposeErrorMessage(msgPtr);
            OrcBindings.LLVMConsumeError(err);
            throw new Exception(message);
        }

        private void DumpIR(LLVMModuleRef module)
        {
            string llvmIR = module.PrintToString();
            Console.WriteLine("Generated LLVM IR:\n");
            Console.WriteLine(llvmIR);
            File.WriteAllText("output_actual_orc.ll", llvmIR);
        }

        private Type PerformSemanticAnalysis(Node expr)
        {
            var checker = new TypeChecker(_context, _debug);
            _lastType = checker.Check(expr);
            _context = checker.UpdatedContext;

            //_lastNode = GetLastExpression(expr);

            var programedResult = GetProgramResult(expr);

            if (programedResult == null) return new VoidType();
            if (programedResult is ExpressionNode exp) return exp.Type;

            return new VoidType();
        }

        // TODO: add functionality

        // show a smaller dataframe from an existing one like x.show(["name", "age"])
        // get the columns back as an array
        // get size of dataframe
        // get record from dataframe
        // if you then have record you can also get the field

        // later add min, max, mean, sum & map/where

        // Problems

        // TODO: fix the problems

        // BROKEN FUNCTIONALITY   
        // record copy is not working
        // making a dataframe takes a repeat of record syntax, could be smoother              
        // doing this in the same command fails: x.add(1); x.length

        // UNIT TESTING
        // create a orc unit test
        // A REPL can do multipe commands in a row and they need to be tested to see if they work correctly, this has to be setup in a integration test
        // we need to not return the struct, we should only print if there is a return value
        // what the compiler returns would if anything be a exit code, 0 for no problem or say 1 for error

        // OTHER
        // currently it also prints out the lenght when calling print, not sure where it happends
        // int is sometimes set to i64, but int is standard i32 in many langauges, it should be consistent and don't think we need int64 as standard for our language
        // FIX: having int be i64 is a massive number, we do not need that kind of precision, it would be better to use i32

        void CreateMain()
        {
            //_funcName = $"main";
            //_funcName = _debug ? "main" : $"main_{_replCounter++}";
            _funcName = $"main_{_replCounter++}";

            // 1 Create a fresh context + module for this command
            var context = LLVMContextRef.Create();
            _module = context.CreateModuleWithName("repl_module");
            var builder = context.CreateBuilder();

            // 2 Create:  define double @__anon_expr_X()
            var funcType = LLVMTypeRef.CreateFunction( // the integration test does not like that the return type is a struct, it cant run
                LLVMTypeRef.CreatePointer(_runtimeValueType, 0), // return type is now a pointer to the boxed struct
                Array.Empty<LLVMTypeRef>(),
                false);

            var function = _module.AddFunction(_funcName, funcType);
            var entry = function.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            _builder = builder;
        }

        public object Run(Node expr, bool debug = false)
        {
            _debug = debug;
            // 1. Semantic analysis
            var prediction = PerformSemanticAnalysis(expr);

            CreateMain();
            DeclarePrintf();

            if (_debug) Console.WriteLine("we code gen");
            LLVMValueRef resultValue = Visit(expr);

            if (_debug) Console.WriteLine("LLVM TYPE: " + resultValue.TypeOf);
            if (_debug) Console.WriteLine("LANG TYPE: " + prediction);

            var boxedPtr = BoxValue(resultValue, prediction);
            _builder.BuildRet(boxedPtr);

            if (_debug) DumpIR(_module);

            // 5 Wrap in ThreadSafeModule
            var tsc = OrcBindings.LLVMOrcCreateNewThreadSafeContext();
            var tsm = OrcBindings.LLVMOrcCreateNewThreadSafeModule(_module.Handle, tsc);

            var dylib = OrcBindings.LLVMOrcLLJITGetMainJITDylib(_jit);
            ThrowIfError(OrcBindings.LLVMOrcLLJITAddLLVMIRModule(_jit, dylib, tsm));

            // 6 Lookup function pointer
            ulong addr;
            ThrowIfError(OrcBindings.LLVMOrcLLJITLookup(_jit, out addr, _funcName));

            // 7 Call it
            var fnPtr = (IntPtr)addr; // the integration test fails here for some reason
            var delegateResult = Marshal.GetDelegateForFunctionPointer<MainDelegate>(fnPtr);

            // Create stopwatch to measure execution time - uncomment if you want to see the stats for each command, but it can be a bit much
            // Stopwatch sw = Stopwatch.StartNew();

            var tempResult = delegateResult();
            if (tempResult == IntPtr.Zero) throw new Exception("JIT execution returned null pointer");
            RuntimeValue result = Marshal.PtrToStructure<RuntimeValue>(tempResult);

            // Print execution stats - uncomment if you want to see the stats for each command, but it can be a bit much
            // sw.Stop();
            // Console.WriteLine("\n--- Execution Stats ---");
            // Console.WriteLine($"Execution Time: {sw.Elapsed.TotalMilliseconds} ms");
            // Console.WriteLine($"Ticks: {sw.ElapsedTicks}");
            // Console.WriteLine("------------------------\n");

            switch ((ValueTag)result.tag)
            {
                case ValueTag.Int:
                    if (_debug) Console.WriteLine("return int");
                    return Marshal.ReadInt64(result.data);

                case ValueTag.Float:
                    if (_debug) Console.WriteLine("return float");
                    return Marshal.PtrToStructure<double>(result.data);

                case ValueTag.String:
                    if (_debug) Console.WriteLine("return string");
                    return Marshal.PtrToStringAnsi(result.data);

                case ValueTag.Bool:
                    if (_debug) Console.WriteLine("return bool");

                    long b = Marshal.ReadByte(result.data);
                    return b != 0;

                case ValueTag.Array:
                    if (_debug) Console.WriteLine("return array");
                    return HandleArray(result.data, prediction);

                case ValueTag.Record:
                    if (_debug) Console.WriteLine("return Record");

                    if (prediction is RecordType recType) // record(["name", "age"],["dan", 100]) 
                    {
                        return HandleRecord(result.data, recType);
                    }
                    return "Record Failure";
                case ValueTag.Dataframe:
                    if (_debug) Console.WriteLine("return Dataframe");

                    if (prediction is DataframeType dfType)
                    {
                        return HandleDataframe(result.data, dfType);
                    }

                    return "Dataframe display error";


                case ValueTag.None:
                    if (_debug) Console.WriteLine("return none");
                    return default;
            }

            return result;
        }

        // record(["name", "age", "is cool", "rating"], ["Hary potter", 9786, true, 10.5585]) 
        // record([ "full_name", "age_in_moons", "is_active_wizard", "power_level_index",  "assigned_house", "patronus_form","wand_core_material", "academic_gpa", "has_invisibility_cloak", "quidditch_position", "total_gold_galleons", "last_sighting_coordinates"],["Harry James Potter",12456,true,98.7742,"Gryffindor","Stag","Phoenix Feather",3.85,true,"Seeker",45200.50,"51.5074° N, 0.1278° W"])

        private string HandleArray(IntPtr arrayObjPtr, Type type)
        {
            if (arrayObjPtr == IntPtr.Zero) return "[]";

            var array = Marshal.PtrToStructure<ArrayObject>(arrayObjPtr);
            var elementType = ((ArrayType)type).ElementType;
            var elements = new List<object>();
            var stride = (elementType is BoolType) ? 1 : 8;

            Console.WriteLine("Array length: " + array.length);
            Console.WriteLine("Array capacity: " + array.capacity);

            for (long i = 0; i < array.length; i++)
            {
                // Calculate offset correctly based on stride
                IntPtr elemPtr = IntPtr.Add(array.data, (int)(i * stride));

                if (elementType is IntType)
                    elements.Add(Marshal.ReadInt64(elemPtr));
                else if (elementType is FloatType)
                    elements.Add(Marshal.PtrToStructure<double>(elemPtr));
                else if (elementType is BoolType)
                    // Read exactly 1 byte
                    elements.Add(Marshal.ReadByte(elemPtr) != 0);
                else if (elementType is StringType)
                    elements.Add(Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(elemPtr)));
                else if (elementType is RecordType recType)
                    elements.Add(HandleRecord(Marshal.ReadIntPtr(elemPtr), recType));
                else if (elementType is ArrayType arrType)
                    elements.Add(HandleArray(Marshal.ReadIntPtr(elemPtr), arrType));
                else if (elementType is NullType)
                    elements.Add("NULL");
                else
                    elements.Add("Unknown Type");
            }

            return "[" + string.Join(", ", elements) + "]";
        }

        private string HandleRecord(IntPtr dataPtr, RecordType record)
        {
            if (dataPtr == IntPtr.Zero) return "{ empty }";

            var result = new Dictionary<string, object>();
            int fieldSize = 8;

            for (int i = 0; i < record.RecordFields?.Count; i++)
            {
                var rec = record.RecordFields[i];
                string label = rec.Label;
                Type recType = rec.Value.Type;

                IntPtr fieldLocation = IntPtr.Add(dataPtr, i * fieldSize);
                IntPtr ptr = Marshal.ReadIntPtr(fieldLocation);

                switch (recType)
                {
                    case IntType:
                        result[label] = Marshal.ReadInt64(ptr);
                        break;
                    case FloatType:
                        result[label] = BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ptr));
                        break;
                    case BoolType:
                        result[label] = Marshal.ReadByte(ptr) != 0;
                        break;
                    case StringType:
                        result[label] = ptr == IntPtr.Zero ? "null" : Marshal.PtrToStringAnsi(ptr);
                        break;
                    case RecordType recT:
                        result[label] = HandleRecord(ptr, recT);
                        break;
                    case ArrayType arrT:
                        result[label] = HandleArray(ptr, arrT);
                        break;
                    default:
                        result[label] = "Unknown Type";
                        break;
                }
            }

            // Standardize formatting here using InvariantCulture
            var entries = result.Select(kv =>
            {
                string valStr = kv.Value is IFormattable f
                    ? f.ToString(null, CultureInfo.InvariantCulture)
                    : kv.Value?.ToString() ?? "null";
                return $"{kv.Key}: {valStr}";
            });

            return "{ " + string.Join(", ", entries) + " }";
        }

        private List<string> ExtractArray(IntPtr arrayObjPtr, ArrayType type)
        {
            var array = Marshal.PtrToStructure<ArrayObject>(arrayObjPtr);


            var elementType = type.ElementType;
            var result = new List<string>(); // Add first column for index

            for (long i = 0; i < array.length; i++)
            {
                IntPtr elemPtr = IntPtr.Add(array.data, (int)(i * 8));
                IntPtr ptr = Marshal.ReadIntPtr(elemPtr);

                if (elementType is StringType)
                    result.Add(Marshal.PtrToStringAnsi(ptr).ToString());

                else
                    result.Add("?");
            }

            return result;
        }


        private string HandleDataframe(IntPtr ptr, DataframeType type)
        {
            if (ptr == IntPtr.Zero) return "dataframe(null)";
            var dfObj = Marshal.PtrToStructure<DataframeObject>(ptr);

            // 1. Get Column Names
            var columnNames = ExtractArray(dfObj.columns, new ArrayType(new StringType()));

            // 2. Get Tags (The 'dataTypes' header in your %dataframe struct)
            var tagsHeader = Marshal.PtrToStructure<ArrayObject>(dfObj.dataTypes);
            List<long> colTags = new List<long>();
            for (long i = 0; i < tagsHeader.length; i++)
            {
                // Read each tag (i64) from the tags array
                colTags.Add(Marshal.ReadInt64(IntPtr.Add(tagsHeader.data, (int)(i * 8))));
            }

            // 3. Get Rows
            var rowsArray = Marshal.PtrToStructure<ArrayObject>(dfObj.rows);
            var rowsData = new List<List<object>>();
            for (long r = 0; r < rowsArray.length; r++)
            {
                IntPtr recordPtr = Marshal.ReadIntPtr(IntPtr.Add(rowsArray.data, (int)(r * 8)));
                // This returns a list of IntPtrs (pointers to the boxed values)
                rowsData.Add(ExtractRecord(recordPtr, type.RowType));
            }

            return FormatTable(columnNames, rowsData, colTags);
        }

        private string FormatTable(List<string> columnNames, List<List<object>> rows, List<long> colTypes)
        {
            int colCount = columnNames.Count;
            var colWidths = new int[colCount];
            var tags = colTypes ?? new List<long>();
            while (tags.Count < colCount) tags.Insert(0, 0); // Handle 'index' column padding

            string GetStringValue(object v, int colIndex)
            {
                if (v == null) return "null";

                // v is usually the IntPtr from ExtractRecord
                IntPtr ptr = (v is IntPtr p) ? p : IntPtr.Zero;
                if (ptr == IntPtr.Zero) return v.ToString();

                // Use the tag provided by the LLVM-generated tags array
                long tag = colIndex < tags.Count ? tags[colIndex] : -1;

                try
                {
                    switch (tag)
                    {
                        case 0: // Index (Int)
                        case 1: // Int
                            if (ptr == IntPtr.Zero) return "0"; // Handle raw 0
                            return Marshal.ReadInt64(ptr).ToString();
                        case 2: // Float (Double)
                            byte[] bytes = new byte[8];
                            Marshal.Copy(ptr, bytes, 0, 8);
                            return BitConverter.ToDouble(bytes, 0).ToString(CultureInfo.InvariantCulture);
                        case 3: // Bool
                            return Marshal.ReadByte(ptr) != 0 ? "True" : "False";
                        case 4: // String
                                // ptr is the address of the pointer to the string. 
                                // We must dereference it once to get the actual char* address.
                            IntPtr actualStringAddr = Marshal.ReadIntPtr(ptr);
                            if (actualStringAddr == IntPtr.Zero) return "";
                            return Marshal.PtrToStringAnsi(actualStringAddr) ?? "";
                        default:
                            return "???";
                    }
                }
                catch
                {
                    return "ERR";
                }
            }

            // 1. Calculate Widths
            for (int c = 0; c < colCount; c++)
            {
                colWidths[c] = columnNames[c].Length;
                foreach (var row in rows)
                {
                    string s = GetStringValue(row[c], c);
                    if (s.Length > colWidths[c]) colWidths[c] = s.Length;
                }
            }

            // 2. Row Selection (Head/Tail)
            int rowCount = rows.Count;
            var rowIndices = new List<int>();
            if (rowCount <= 10) for (int i = 0; i < rowCount; i++) rowIndices.Add(i);
            else
            {
                for (int i = 0; i < 5; i++) rowIndices.Add(i);
                rowIndices.Add(-1);
                for (int i = rowCount - 5; i < rowCount; i++) rowIndices.Add(i);
            }

            // 3. Formatting
            string FormatRow(List<string> data) =>
                string.Join(" | ", data.Select((val, i) => val.PadRight(colWidths[i])));

            string sep = string.Join("-+-", colWidths.Select(w => new string('-', w)));
            var lines = new List<string> { FormatRow(columnNames), sep };

            foreach (var r in rowIndices)
            {
                if (r == -1)
                {
                    lines.Add(string.Join(" | ", colWidths.Select(w => "...".PadRight(w))));
                    continue;
                }
                var rowStrings = rows[r].Select((v, i) => GetStringValue(v, i)).ToList();
                lines.Add(FormatRow(rowStrings));
            }

            return "\nDataframe (" + rowCount + " rows):\n" + string.Join("\n", lines.Select(l => "  " + l));
        }

        private int GetTypeByTag(Type type)
        {
            if (type == null)
            {
                Console.WriteLine("CRITICAL: GetTypeByTag received a NULL type!");
                return (int)ValueTag.None;
            }

            Console.WriteLine($"Resolving Tag for: {type.GetType().Name}");

            return type switch
            {
                IntType => (Int16)ValueTag.Int,
                FloatType => (Int16)ValueTag.Float,
                BoolType => (Int16)ValueTag.Bool,
                StringType => (Int16)ValueTag.String,
                ArrayType => (Int16)ValueTag.Array,
                RecordType => (Int16)ValueTag.Record,
                DataframeType => (Int16)ValueTag.Dataframe,
                _ => (Int16)ValueTag.None
            };
        }
        private LLVMValueRef BoxValue(LLVMValueRef value, Type type)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i16 = ctx.Int16Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            var mallocFunc = GetOrDeclareMalloc();

            // --- THE FIX: Define with Padding ---
            // { i16, [6 x i8], i8* }
            var paddingType = LLVMTypeRef.CreateArray(i8, 6);
            var runtimeValueType = LLVMTypeRef.CreateStruct(new[] { i16, paddingType, i8Ptr }, false);

            // --- THE SHIELD ---
            var boxTypePtr = LLVMTypeRef.CreatePointer(_runtimeValueType, 0);
            if (value.TypeOf == boxTypePtr)
            {
                return value;
            }

            // --- NULL TYPE RECOVERY ---
            if (type == null || type is VoidType)
            {
                // Check the LLVM value itself. In LLVM 20, if we can't rely on ToString(),
                // we rely on the fact that we just passed a Dataframe pointer.

                // Check if the value we are boxing is specifically our newly created df_ptr
                if (value.Name.Contains("df_ptr"))
                {
                    type = new DataframeType(new List<string>(), new List<Type>(), null);
                }
                else
                {
                    Console.WriteLine("Warning: BoxValue received a null/void type. Defaulting to None.");
                    type = new VoidType();
                }
            }

            int tag = GetTypeByTag(type);
            LLVMValueRef dataPtr;

            if (type is IntType || type is FloatType || type is BoolType)
            {
                int size = type switch
                {
                    IntType => 8,
                    FloatType => 8,
                    BoolType => 1,
                    _ => 8
                };

                var mem = _builder.BuildCall2(_mallocType, mallocFunc,
                    new[] { LLVMValueRef.CreateConstInt(i64, (ulong)size) }, "value_mem");

                var castType = type switch
                {
                    IntType => LLVMTypeRef.CreatePointer(i64, 0),
                    FloatType => LLVMTypeRef.CreatePointer(ctx.DoubleType, 0),
                    BoolType => LLVMTypeRef.CreatePointer(ctx.Int1Type, 0),
                    _ => LLVMTypeRef.CreatePointer(i64, 0)
                };

                var cast = _builder.BuildBitCast(mem, castType, "value_cast");
                _builder.BuildStore(value, cast);
                dataPtr = mem;
            }
            else if (type is StringType || type is RecordType || type is ArrayType || type is DataframeType || type.GetType() == typeof(Type))
            {
                // Treat as pointer
                //if (_debug) Console.WriteLine($"Boxing complex type: {type} (Tag: {tag})");
                dataPtr = _builder.BuildBitCast(value, i8Ptr, "boxed_ptr_cast");
            }
            else if (type is VoidType)
            {
                dataPtr = LLVMValueRef.CreateConstPointerNull(i8Ptr);
            }
            else
            {
                throw new Exception($"Unsupported type in BoxValue: {type}");
            }

            // Allocate RuntimeValue (struct { i16 tag, i8* data })
            var objRaw = _builder.BuildCall2(_mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, 16) }, "runtime_obj");

            var obj = _builder.BuildBitCast(objRaw, LLVMTypeRef.CreatePointer(runtimeValueType, 0), "runtime_cast");

            // Store tag
            var tagPtr = _builder.BuildStructGEP2(runtimeValueType, obj, 0, "tag_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i16, (ulong)tag), tagPtr).SetAlignment(8);

            // Store data
            var dataFieldPtr = _builder.BuildStructGEP2(runtimeValueType, obj, 2, "data_ptr");
            _builder.BuildStore(dataPtr, dataFieldPtr).SetAlignment(8);

            return objRaw;
        }




        private ExpressionNode GetProgramResult(Node expr)
        {
            if (expr is SequenceNode seq)
            {
                foreach (var node in seq.Statements)
                {
                    if (node is ExpressionNode exp && !(node is PrintNode))
                        return exp;
                }

                return null;
            }

            if (expr is ExpressionNode exp2 && !(expr is PrintNode))
                return exp2;

            return null;
        }

        private Node GetLastExpression(Node expr)
        {
            if (expr is SequenceNode seq)
            {
                // iterate from last to first
                for (int i = seq.Statements.Count - 1; i >= 0; i--)
                {
                    var last = seq.Statements[i];

                    // Only consider actual value expressions, skip statements like print
                    if (last is ExpressionNode && !(last is PrintNode))
                        return last;

                    // If last is a sequence itself, recurse
                    if (last is SequenceNode nestedSeq)
                    {
                        var nestedLast = GetLastExpression(nestedSeq);
                        if (nestedLast != null) return nestedLast;
                    }
                }
                // No expression found
                return null;
            }

            // Single expression node
            if (expr is ExpressionNode && !(expr is PrintNode))
                return expr;

            return null;
        }

        public LLVMValueRef VisitForLoop(ForLoopNode expr)
        {
            var func = _builder.InsertBlock.Parent;

            // 1. Initialization
            if (expr.Initialization != null) Visit(expr.Initialization);

            // 2. Define the Basic Blocks
            var condBlock = func.AppendBasicBlock("for.cond");
            var bodyBlock = func.AppendBasicBlock("for.body");
            var stepBlock = func.AppendBasicBlock("for.step");
            var endBlock = func.AppendBasicBlock("for.end");

            // Entry jump
            _builder.BuildBr(condBlock);

            // 3. Condition Block
            _builder.PositionAtEnd(condBlock);
            var condition = Visit(expr.Condition);

            // If the condition is a Double (typical for your comparison results), 
            // we need to compare it against 0.0 using FCmp
            if (condition.TypeOf == _module.Context.DoubleType)
            {
                condition = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE,
                    condition, LLVMValueRef.CreateConstReal(_module.Context.DoubleType, 0.0), "fortest_dbl");
            }
            // If it's an i64 or i32, use ICmp
            else if (condition.TypeOf != _module.Context.Int1Type)
            {
                condition = _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE,
                    condition, LLVMValueRef.CreateConstInt(condition.TypeOf, 0), "fortest_int");
            }

            _builder.BuildCondBr(condition, bodyBlock, endBlock);

            // 4. Body Block
            _builder.PositionAtEnd(bodyBlock);
            Visit(expr.Body);
            _builder.BuildBr(stepBlock); // Jump to step, not back to cond!

            // 5. Step Block (Increment)
            _builder.PositionAtEnd(stepBlock);
            if (expr.Step != null) Visit(expr.Step);
            _builder.BuildBr(condBlock); // Jump back to condition

            // 6. End Block
            _builder.PositionAtEnd(endBlock);

            return default;
        }

        public LLVMValueRef VisitForEachLoop(ForEachLoopNode expr)
        {
            var ctx = _module.Context;
            var func = _builder.InsertBlock.Parent;

            // Array pointer and length
            var arrayPtr = Visit(expr.Array);
            var zero = LLVMValueRef.CreateConstInt(ctx.Int32Type, 0);
            var lenPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { zero }, "len_ptr");
            var arrayLen = _builder.BuildLoad2(ctx.Int64Type, lenPtr, "array_len");

            // Stack-local iterator
            var elementType = ((ArrayType)expr.Array.Type).ElementType;
            var iteratorAlloc = _builder.BuildAlloca(GetLLVMType(elementType), expr.Iterator.Name);

            // Add to context
            _context = _context.Add(expr.Iterator.Name, iteratorAlloc, null, elementType);

            // Loop blocks
            var condBlock = func.AppendBasicBlock("foreach.cond");
            var bodyBlock = func.AppendBasicBlock("foreach.body");
            var endBlock = func.AppendBasicBlock("foreach.end");

            // Counter
            var counterAlloc = _builder.BuildAlloca(ctx.Int64Type, "foreach_counter");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int64Type, 0), counterAlloc);
            _builder.BuildBr(condBlock);

            // Condition
            _builder.PositionAtEnd(condBlock);
            var curIdx = _builder.BuildLoad2(ctx.Int64Type, counterAlloc, "cur_idx");
            var cond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, curIdx, arrayLen, "foreach_test");
            _builder.BuildCondBr(cond, bodyBlock, endBlock);

            // Body
            _builder.PositionAtEnd(bodyBlock);
            var two64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 2);
            var memIdx = _builder.BuildAdd(curIdx, two64, "mem_idx");

            var elementPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { memIdx }, "elem_ptr");
            var elementVal = _builder.BuildLoad2(ctx.Int64Type, elementPtr, "elem_val");

            // Store into stack-local iterator
            _builder.BuildStore(elementVal, iteratorAlloc);

            Visit(expr.Body);

            // Increment counter
            var one64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 1);
            var nextIdx = _builder.BuildAdd(curIdx, one64, "next_idx");
            _builder.BuildStore(nextIdx, counterAlloc);
            _builder.BuildBr(condBlock);

            _builder.PositionAtEnd(endBlock);
            return default;
        }
        public LLVMValueRef VisitIncrement(IncrementNode expr)
        {
            // 1. Get the pointer
            LLVMValueRef global = _module.GetNamedGlobal(expr.Id);
            if (global.Handle == IntPtr.Zero)
                throw new Exception($"Variable {expr.Id} not found.");

            // 2. Identify the type (handle both Int64 and Double)
            // We check the Global's type or default to Int64 for loop indices
            LLVMTypeRef type = _module.Context.Int64Type;
            var entry = _context.Get(expr.Id);
            if (entry != null && entry.Type is FloatType)
            {
                type = _module.Context.DoubleType;
            }

            var oldValue = _builder.BuildLoad2(type, global, "inc_load");

            LLVMValueRef newValue;
            // Check the LLVM Kind instead of expr.Type to be safe
            if (type.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
            {
                newValue = _builder.BuildFAdd(oldValue, LLVMValueRef.CreateConstReal(type, 1.0), "inc_add");
            }
            else
            {
                // Default to Integer Add
                newValue = _builder.BuildAdd(oldValue, LLVMValueRef.CreateConstInt(type, 1, false), "inc_add");
            }

            _builder.BuildStore(newValue, global).SetAlignment(8);
            return newValue;
        }

        public LLVMValueRef VisitDecrement(DecrementNode expr)
        {
            LLVMValueRef global = _module.GetNamedGlobal(expr.Id);
            if (global.Handle == IntPtr.Zero)
                throw new Exception($"Variable {expr.Id} not found.");

            LLVMTypeRef type = _module.Context.Int64Type;
            var entry = _context.Get(expr.Id);
            if (entry != null && entry.Type is FloatType)
            {
                type = _module.Context.DoubleType;
            }

            var oldValue = _builder.BuildLoad2(type, global, "dec_load");

            LLVMValueRef newValue;
            if (type.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
            {
                newValue = _builder.BuildFSub(oldValue, LLVMValueRef.CreateConstReal(type, 1.0), "dec_sub");
            }
            else
            {
                newValue = _builder.BuildSub(oldValue, LLVMValueRef.CreateConstInt(type, 1, false), "dec_sub");
            }

            _builder.BuildStore(newValue, global).SetAlignment(8);
            return newValue;
        }

        public LLVMValueRef VisitComparison(ComparisonNode expr)
        {
            var leftPtr = Visit(expr.Left);
            var rightPtr = Visit(expr.Right);

            // Determine the types of the operands
            var leftType = expr.Left.Type;
            var rightType = expr.Right.Type;

            bool lhsIsInt = leftType is IntType;
            bool rhsIsInt = rightType is IntType;
            bool lhsIsBool = leftType is BoolType;
            bool rhsIsBool = rightType is BoolType;
            bool lhsIsString = leftType is StringType;
            bool rhsIsString = rightType is StringType;

            // Case 1: logical AND (&&) or logical OR (||) - only valid if both operands are boolean
            if (expr.Operator == "&&" || expr.Operator == "||")
            {
                if (!(lhsIsBool && rhsIsBool))
                {
                    throw new Exception("Both operands must be booleans for logical AND/OR.");
                }

                // Handle logical AND (&&)
                if (expr.Operator == "&&")
                {
                    var and = _builder.BuildAnd(leftPtr, rightPtr, "and_tmp");
                    return and;
                }

                // Handle logical OR (||)
                if (expr.Operator == "||")
                {
                    var or = _builder.BuildOr(leftPtr, rightPtr, "or_tmp");
                    return or;
                }
            }

            // Case 2: both integers → ICmp
            if (lhsIsInt && rhsIsInt)
            {
                return expr.Operator switch
                {
                    "<" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, leftPtr, rightPtr, "icmp_tmp"),
                    ">" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, leftPtr, rightPtr, "icmp_tmp"),
                    "<=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, leftPtr, rightPtr, "icmp_tmp"),
                    ">=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, leftPtr, rightPtr, "icmp_tmp"),
                    "==" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, leftPtr, rightPtr, "icmp_tmp"),
                    "!=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, leftPtr, rightPtr, "icmp_tmp"),
                    _ => throw new Exception("Unknown operator")
                };
            }

            // Case 3: at least one is float/double → promote integers (not booleans) to double
            if (lhsIsInt)
                leftPtr = _builder.BuildSIToFP(leftPtr, _module.Context.DoubleType, "int2double");
            if (rhsIsInt)
                rightPtr = _builder.BuildSIToFP(rightPtr, _module.Context.DoubleType, "int2double");

            // Case 4: boolean comparison? Only allow equality/inequality
            if (lhsIsBool && rhsIsBool)
            {
                return expr.Operator switch
                {
                    "==" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, leftPtr, rightPtr, "bool_eq"),
                    "!=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, leftPtr, rightPtr, "bool_ne"),
                    _ => throw new Exception("Cannot compare booleans with < > <= >=")
                };
            }

            // --- 5. STRING COMPARISONS (only equality) ---
            if (lhsIsString && rhsIsString)
            {
                // 1. Get or Create the function signature
                var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
                var strcmpType = LLVMTypeRef.CreateFunction(_module.Context.Int32Type, new[] { i8Ptr, i8Ptr }, false);

                var strcmpFunc = _module.GetNamedFunction("strcmp");
                if (strcmpFunc.Handle == IntPtr.Zero)
                {
                    strcmpFunc = _module.AddFunction("strcmp", strcmpType);
                }

                // 2. Ensure we are passing the data, not the address of the variable
                // If 'left' is a local variable from BuildAlloca, you MUST load it:
                // var leftPtr = _builder.BuildLoad2(i8Ptr, left, "left_val");

                // 3. Call strcmp - Note the use of strcmpType (the Function Type)
                var strcmpResult = _builder.BuildCall2(strcmpType, strcmpFunc, new[] { leftPtr, rightPtr }, "strcmp_res");

                // 4. Compare against 0
                var zero = LLVMValueRef.CreateConstInt(_module.Context.Int32Type, 0);

                return expr.Operator switch
                {
                    "==" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, strcmpResult, zero, "str_eq"),
                    "!=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, strcmpResult, zero, "str_ne"),
                    _ => throw new Exception($"Unsupported string operator: {expr.Operator}")
                };
            }

            // Case 5: both are doubles → FCmp
            return expr.Operator switch
            {
                "<" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, leftPtr, rightPtr, "fcmp_tmp"),
                ">" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, leftPtr, rightPtr, "fcmp_tmp"),
                "<=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLE, leftPtr, rightPtr, "fcmp_tmp"),
                ">=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGE, leftPtr, rightPtr, "fcmp_tmp"),
                "==" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, leftPtr, rightPtr, "fcmp_tmp"),
                "!=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, leftPtr, rightPtr, "fcmp_tmp"),
                _ => throw new Exception("Unknown operator")
            };
        }

        public LLVMValueRef VisitIf(IfNode node)
        {
            var condValue = Visit(node.Condition);

            var function = _builder.InsertBlock.Parent;

            var thenBlock = function.AppendBasicBlock("then");
            var elseBlock = function.AppendBasicBlock("else");
            var mergeBlock = function.AppendBasicBlock("ifcont");

            _builder.BuildCondBr(condValue, thenBlock, elseBlock);

            // THEN
            _builder.PositionAtEnd(thenBlock);
            var thenValue = Visit(node.ThenPart);
            _builder.BuildBr(mergeBlock);
            thenBlock = _builder.InsertBlock;

            // ELSE
            _builder.PositionAtEnd(elseBlock);

            LLVMValueRef elseValue;

            if (node.ElsePart != null)
                elseValue = Visit(node.ElsePart);
            else
                elseValue = LLVMValueRef.CreateConstReal(_module.Context.DoubleType, 0);

            _builder.BuildBr(mergeBlock);
            elseBlock = _builder.InsertBlock;

            // MERGE
            _builder.PositionAtEnd(mergeBlock);

            var phi = _builder.BuildPhi(thenValue.TypeOf, "iftmp");

            phi.AddIncoming(
                new[] { thenValue, elseValue },
                new[] { thenBlock, elseBlock },
                2);

            return phi;
        }

        private LLVMValueRef EnsureFloat(LLVMValueRef value, Type currentType)
        {
            if (currentType is FloatType) return value;
            if (currentType is IntType)
                return _builder.BuildSIToFP(value, _module.Context.DoubleType, "cast_tmp");

            return value; // Hope for the best, or throw an error
        }

        public LLVMValueRef VisitRound(RoundNode expr)
        {
            var val = EnsureFloat(Visit(expr.Value), expr.Value.Type);
            var decimals = EnsureFloat(Visit(expr.Decimals), expr.Decimals.Type);

            var doubleType = _module.Context.DoubleType;
            var powType = LLVMTypeRef.CreateFunction(doubleType, new[] { doubleType, doubleType });
            var roundType = LLVMTypeRef.CreateFunction(doubleType, new[] { doubleType });

            // FIX: Change GetFunction to GetNamedFunction
            var powFunc = _module.GetNamedFunction("pow");
            if (powFunc.Handle == IntPtr.Zero) // Check if it exists
                powFunc = _module.AddFunction("pow", powType);

            var roundFunc = _module.GetNamedFunction("round");
            if (roundFunc.Handle == IntPtr.Zero)
                roundFunc = _module.AddFunction("round", roundType);

            // multiplier = pow(10.0, decimals)
            var ten = LLVMValueRef.CreateConstReal(doubleType, 10.0);
            var multiplier = _builder.BuildCall2(powType, powFunc, new[] { ten, decimals }, "multiplier");

            // temp = val * multiplier
            var temp = _builder.BuildFMul(val, multiplier, "temp");

            // roundedTemp = round(temp)
            var roundedTemp = _builder.BuildCall2(roundType, roundFunc, new[] { temp }, "roundedTemp");

            // result = roundedTemp / multiplier
            return _builder.BuildFDiv(roundedTemp, multiplier, "rounded_final");
        }

        public LLVMValueRef VisitBinary(BinaryOpNode expr)
        {
            // 1. Visit sides to get LLVM values
            var lhs = Visit(expr.Left);
            var rhs = Visit(expr.Right);

            // Get the semantic types (MyType) from the nodes
            var leftType = expr.Left.Type;
            var rightType = expr.Right.Type;

            // 2. Handle Strings (Concatenation)
            // We check MyType because LLVM 'ptr' can be confusing
            if (expr.Operator == "+" && (leftType is StringType || rightType is StringType))
            {
                // BuildStringConcat handles Int/Float -> String conversion internally
                return BuildStringConcat(lhs, leftType, rhs, rightType);
            }

            // 3. Check for logical operations (AND/OR)
            if (expr.Operator == "&&" || expr.Operator == "||")
            {
                // Both operands must be booleans (LLVM integer type with width 1, i1)
                bool lhsIsBool = leftType is BoolType;
                bool rhsIsBool = rightType is BoolType;

                if (!(lhsIsBool && rhsIsBool))
                {
                    throw new InvalidOperationException($"Cannot perform {expr.Operator} on non-boolean types ({leftType}, {rightType})");
                }

                // Handle logical AND (&&)
                if (expr.Operator == "&&")
                {
                    return _builder.BuildAnd(lhs, rhs, "andtmp");
                }

                // Handle logical OR (||)
                if (expr.Operator == "||")
                {
                    return _builder.BuildOr(lhs, rhs, "ortmp");
                }
            }

            // 4. Check for numeric types (Int/Float)
            bool lhsIsInt = leftType is IntType;
            bool rhsIsInt = rightType is IntType;
            bool lhsIsFloat = leftType is FloatType;
            bool rhsIsFloat = rightType is FloatType;

            // 5. Integer arithmetic
            if (lhsIsInt && rhsIsInt)
            {
                return expr.Operator switch
                {
                    "+" => _builder.BuildAdd(lhs, rhs, "addtmp"),
                    "-" => _builder.BuildSub(lhs, rhs, "subtmp"),
                    "*" => _builder.BuildMul(lhs, rhs, "multmp"),
                    "/" => _builder.BuildSDiv(lhs, rhs, "divtmp"),
                    _ => throw new InvalidOperationException($"Unsupported operator {expr.Operator} for Int")
                };
            }

            // 6. Mixed or Floating-point arithmetic
            if ((lhsIsInt || lhsIsFloat) && (rhsIsInt || rhsIsFloat))
            {
                // Promote lhs to double if it's currently an int
                if (lhsIsInt)
                    lhs = _builder.BuildSIToFP(lhs, _module.Context.DoubleType, "int2double");

                // Promote rhs to double if it's currently an int
                if (rhsIsInt)
                    rhs = _builder.BuildSIToFP(rhs, _module.Context.DoubleType, "int2double");

                return expr.Operator switch
                {
                    "+" => _builder.BuildFAdd(lhs, rhs, "faddtmp"),
                    "-" => _builder.BuildFSub(lhs, rhs, "fsubtmp"),
                    "*" => _builder.BuildFMul(lhs, rhs, "fmultmp"),
                    "/" => _builder.BuildFDiv(lhs, rhs, "fdivtmp"),
                    _ => throw new InvalidOperationException($"Unsupported operator {expr.Operator} for Float")
                };
            }

            throw new InvalidOperationException($"Cannot perform {expr.Operator} on {leftType} and {rightType}");
        }

        public LLVMValueRef VisitLogicalOp(LogicalOpNode expr)
        {
            var lhs = Visit(expr.Left);
            var rhs = Visit(expr.Right);

            return expr.Operator switch
            {
                "&&" => _builder.BuildAnd(lhs, rhs, "andtmp"),
                "||" => _builder.BuildOr(lhs, rhs, "ortmp"),
                _ => throw new Exception($"Unknown logical operator {expr.Operator}")
            };
        }

        private (LLVMTypeRef Type, LLVMValueRef Func) GetMalloc()
        {
            // Look for it in the CURRENT module
            var existing = _module.GetNamedFunction("malloc");

            // Define the type: i8* malloc(i64)
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            var mallocType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { _module.Context.Int64Type }, false);

            if (existing.Handle != IntPtr.Zero)
                return (mallocType, existing);

            return (mallocType, _module.AddFunction("malloc", mallocType));
        }

        private LLVMValueRef GetOrDeclareRealloc()
        {
            var ctx = _module.Context;
            var i8ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            var type = LLVMTypeRef.CreateFunction(
                i8ptr,
                new[] { i8ptr, ctx.Int64Type },
                false
            );

            _reallocType = type;

            var fn = _module.GetNamedFunction("realloc");

            if (fn.Handle != IntPtr.Zero)
                return fn;

            return _module.AddFunction("realloc", type);
        }

        private LLVMValueRef GetOrDeclareMemmove()
        {
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            _memmoveType = LLVMTypeRef.CreateFunction(
                ctx.VoidType,
                new[] { i8Ptr, i8Ptr, ctx.Int64Type },
                false
            );


            var fn = _module.GetNamedFunction("memmove");
            if (fn.Handle != IntPtr.Zero)
                return fn;

            return _module.AddFunction("memmove", _memmoveType);
        }

        private LLVMValueRef GetSprintf()
        {
            var fn = _module.GetNamedFunction("sprintf");
            if (fn.Handle != IntPtr.Zero) return fn;
            return _module.AddFunction("sprintf", LLVMTypeRef.CreateFunction(
                _module.Context.Int32Type,
                new[] { LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0), LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0) },
                true));
        }

        private LLVMValueRef BuildStringConcat(LLVMValueRef lhs, Type lhsType, LLVMValueRef rhs, Type rhsType)
        {
            var ctx = _module.Context;
            var malloc = GetMalloc();
            var sprintf = GetSprintf();
            var ptrType = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 1. Helper to ensure we have an i8* for sprintf
            LLVMValueRef PrepareArg(LLVMValueRef val, Type type)
            {
                if (type is StringType) return val;

                // Allocate buffer for number conversion
                var buf = _builder.BuildCall2(malloc.Type, malloc.Func, new[] { LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 32) }, "num_buf");
                var fmtStr = type is IntType ? "%ld" : "%g";
                var fmtPtr = _builder.BuildGlobalStringPtr(fmtStr, "fmt_num");

                // sprintf(buf, fmt, val)
                // We must use a variadic signature: i32 (i8*, i8*, ...)
                var sprintfType = LLVMTypeRef.CreateFunction(ctx.Int32Type, new[] { ptrType, ptrType }, true);
                _builder.BuildCall2(sprintfType, sprintf, new[] { buf, fmtPtr, val }, "sprintf_num");
                return buf;
            }

            var arg1 = PrepareArg(lhs, lhsType);
            var arg2 = PrepareArg(rhs, rhsType);

            // 2. Allocate buffer for final result (256 bytes is a bit risky but okay for REPL)
            var concatBuf = _builder.BuildCall2(malloc.Type, malloc.Func, new[] { LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 512) }, "concat_buf");

            // 3. Perform final Concatenation: sprintf(concatBuf, "%s%s", arg1, arg2)
            var fmtConcat = _builder.BuildGlobalStringPtr("%s%s", "fmt_concat");

            // Define correct variadic type: Result is i32, Args are (dest, format, ...)
            var varArgSprintfType = LLVMTypeRef.CreateFunction(ctx.Int32Type, new[] { ptrType, ptrType }, true);

            _builder.BuildCall2(varArgSprintfType, sprintf, new[] { concatBuf, fmtConcat, arg1, arg2 }, "sprintf_result");

            return concatBuf;
        }

        public LLVMValueRef VisitString(StringNode expr)
        {
            return _builder.BuildGlobalStringPtr(expr.Value, "str");
        }
        public LLVMValueRef VisitNumber(NumberNode expr)
        {
            return LLVMValueRef.CreateConstInt(_module.Context.Int64Type, (ulong)expr.Value);
        }
        public LLVMValueRef VisitFloat(FloatNode expr)
        {
            return LLVMValueRef.CreateConstReal(_module.Context.DoubleType, expr.Value);
        }
        public LLVMValueRef VisitBoolean(BooleanNode expr)
        {
            return LLVMValueRef.CreateConstInt(_module.Context.Int1Type, expr.Value ? 1UL : 0UL);
        }
        public LLVMValueRef VisitNull(NullNode expr)
        {
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // Return a null pointer of type i8*
            return LLVMValueRef.CreateConstPointerNull(i8Ptr);
        }
        public LLVMValueRef VisitAssign(AssignNode expr)
        {
            if (_debug) Console.WriteLine($"visiting assignment: {expr.Id}");

            var value = Visit(expr.Expression);
            var storageType = value.TypeOf;
            var module = _module;

            LLVMValueRef global = module.GetNamedGlobal(expr.Id);

            // Determine the natural alignment for this type
            // Bools (i1) = 1, everything else (i64, double, ptr) = 8
            uint alignment = (storageType.Kind == LLVMTypeKind.LLVMIntegerTypeKind && storageType.IntWidth == 1) ? 1u : 8u;

            if (global.Handle == IntPtr.Zero)
            {
                if (!_definedGlobals.Contains(expr.Id))
                {
                    // --- FIRST DEFINITION ---
                    global = module.AddGlobal(storageType, expr.Id);

                    // Set explicit alignment on the global variable definition itself
                    global.SetAlignment(alignment);

                    // Set initial dummy value so it's not "undef"
                    if (storageType.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
                        global.Initializer = LLVMValueRef.CreateConstReal(storageType, 0.0);
                    else if (storageType.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                        global.Initializer = LLVMValueRef.CreateConstInt(storageType, 0);
                    else
                        global.Initializer = LLVMValueRef.CreateConstNull(storageType);

                    global.Linkage = LLVMLinkage.LLVMExternalLinkage;
                    _definedGlobals.Add(expr.Id);
                }
                else
                {
                    // --- RE-DECLARATION (in a new REPL module) ---
                    global = module.AddGlobal(storageType, expr.Id);

                    // Linkage to External WITHOUT Initializer makes it 'extern'
                    global.Linkage = LLVMLinkage.LLVMExternalLinkage;

                    // Ensure the declaration matches the definition's alignment
                    global.SetAlignment(alignment);
                }
            }

            // Perform the store
            var store = _builder.BuildStore(value, global);

            // Set explicit alignment on the store instruction
            store.SetAlignment(alignment);

            return value;
        }

        // Simple helper to map your AST types to Tags
        private short GetTagForType(Type type)
        {
            if (type is IntType) return (short)ValueTag.Int;
            if (type is FloatType) return (short)ValueTag.Float;
            if (type is StringType) return (short)ValueTag.String;
            if (type is ArrayType) return (short)ValueTag.Array;
            if (type is RecordType) return (short)ValueTag.Record;
            return (short)ValueTag.None;
        }

        public LLVMValueRef VisitRandom(RandomNode expr)
        {
            var llvmCtx = _module.Context;
            var i64 = llvmCtx.Int64Type;

            // 1. Get or declare rand()
            var randFunc = _module.GetNamedFunction("rand");
            if (randFunc.Handle == IntPtr.Zero)
            {
                var randType = LLVMTypeRef.CreateFunction(i64, Array.Empty<LLVMTypeRef>(), false);
                randFunc = _module.AddFunction("rand", randType);
            }

            var randValue = _builder.BuildCall2(
                LLVMTypeRef.CreateFunction(i64, Array.Empty<LLVMTypeRef>()),
                randFunc, Array.Empty<LLVMValueRef>(), "randcall");

            if (expr.MinValue != null && expr.MaxValue != null)
            {
                var minVal = Visit(expr.MinValue);
                var maxVal = Visit(expr.MaxValue);

                // Ensure we are working with Integers for rand math
                if (minVal.TypeOf == _module.Context.DoubleType) minVal = _builder.BuildFPToSI(minVal, i64, "min_i");
                if (maxVal.TypeOf == _module.Context.DoubleType) maxVal = _builder.BuildFPToSI(maxVal, i64, "max_i");

                // --- THE "VISIT IF" STYLE ---
                var func = _builder.InsertBlock.Parent;
                var thenBB = func.AppendBasicBlock("rand.correct"); // min <= max
                var elseBB = func.AppendBasicBlock("rand.swap");    // min > max
                var mergeBB = func.AppendBasicBlock("rand.cont");

                // Create a local variable to store the result (since blocks can't "return")
                var resultPtr = _builder.BuildAlloca(i64, "rand_result_ptr");

                var cond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, minVal, maxVal, "order_check");
                _builder.BuildCondBr(cond, thenBB, elseBB);

                // "Then" Part (Correct Order)
                _builder.PositionAtEnd(thenBB);
                var diff1 = _builder.BuildSub(maxVal, minVal, "diff1");
                var range1 = _builder.BuildAdd(diff1, LLVMValueRef.CreateConstInt(i64, 1), "range1");
                var res1 = _builder.BuildAdd(_builder.BuildSRem(randValue, range1, "mod1"), minVal, "res1");
                _builder.BuildStore(res1, resultPtr).SetAlignment(8); // res1 is i64, requires 8-byte alignment

                _builder.BuildBr(mergeBB);

                // "Else" Part (Wrong Order - Swap logic)
                _builder.PositionAtEnd(elseBB);
                var diff2 = _builder.BuildSub(minVal, maxVal, "diff2"); // again this says i64 -99
                var range2 = _builder.BuildAdd(diff2, LLVMValueRef.CreateConstInt(i64, 1), "range2");
                var res2 = _builder.BuildAdd(_builder.BuildSRem(randValue, range2, "mod2"), maxVal, "res2");
                _builder.BuildStore(res2, resultPtr).SetAlignment(8);

                _builder.BuildBr(mergeBB);

                // Merge
                _builder.PositionAtEnd(mergeBB);
                var finalInt = _builder.BuildLoad2(i64, resultPtr, "final_rand_int");
                return _builder.BuildSIToFP(finalInt, _module.Context.Int64Type, "final_rand_dbl");
            }

            return _builder.BuildSIToFP(randValue, _module.Context.Int64Type, "rand_simple");
        }

        private LLVMValueRef AddImplicitPrint(LLVMValueRef valueToPrint, Type type)
        {
            if (_debug) Console.WriteLine("prints type: " + type);
            if (_debug) Console.WriteLine("value to print type: " + valueToPrint.TypeOf);

            LLVMValueRef finalArg;
            LLVMValueRef formatStr;
            var llvmCtx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0);

            // SAFETY: fallback on actual LLVM type if semantic type fails
            if (valueToPrint.TypeOf == llvmCtx.Int64Type)
            {
                finalArg = valueToPrint;
                formatStr = _builder.BuildGlobalStringPtr("%ld\n", "fmt_int_raw");
            }
            else if (valueToPrint.TypeOf == llvmCtx.DoubleType)
            {
                finalArg = valueToPrint;
                formatStr = _builder.BuildGlobalStringPtr("%f\n", "fmt_float_raw");
            }
            else
            {
                switch (type)
                {
                    case IntType:
                        finalArg = valueToPrint;
                        formatStr = _builder.BuildGlobalStringPtr("%ld\n", "fmt_int");
                        break;

                    case FloatType:
                        finalArg = valueToPrint;
                        formatStr = _builder.BuildGlobalStringPtr("%f\n", "fmt_float");
                        break;

                    case BoolType:
                        DeclareBoolStrings();

                        LLVMValueRef boolCond;

                        if (valueToPrint.TypeOf == llvmCtx.Int1Type)
                        {
                            boolCond = valueToPrint;
                        }
                        else if (valueToPrint.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                        {
                            var zero = LLVMValueRef.CreateConstInt(valueToPrint.TypeOf, 0);
                            boolCond = _builder.BuildICmp(
                                LLVMIntPredicate.LLVMIntNE,
                                valueToPrint,
                                zero,
                                "boolcond");
                        }
                        else if (valueToPrint.TypeOf == llvmCtx.DoubleType)
                        {
                            var zero = LLVMValueRef.CreateConstReal(llvmCtx.DoubleType, 0.0);
                            boolCond = _builder.BuildFCmp(
                                LLVMRealPredicate.LLVMRealONE,
                                valueToPrint,
                                zero,
                                "boolcond");
                        }
                        else
                        {
                            throw new Exception("Unsupported bool representation");
                        }

                        var selectedStr = _builder.BuildSelect(boolCond, _trueStr, _falseStr, "boolstr");

                        return _builder.BuildCall2(
                            _printfType,
                            _printf,
                            new[] { selectedStr },
                            "print_bool");

                    case StringType:
                        finalArg = valueToPrint;
                        formatStr = _builder.BuildGlobalStringPtr("%s\n", "fmt_str");
                        break;

                    case ArrayType:
                        // Cast to i64* to read header
                        var arrPtr = _builder.BuildBitCast(
                            valueToPrint,
                            LLVMTypeRef.CreatePointer(llvmCtx.Int64Type, 0),
                            "arr_len_ptr");

                        var arrLen = _builder.BuildLoad2(llvmCtx.Int64Type, arrPtr, "arr_len");
                        arrLen.SetAlignment(8);

                        finalArg = arrLen;
                        formatStr = _builder.BuildGlobalStringPtr("Array(len=%ld)\n", "fmt_array");
                        break;

                    case RecordType:
                        // Cast to i64* to read header
                        var recPtr = _builder.BuildBitCast(
                            valueToPrint,
                            LLVMTypeRef.CreatePointer(llvmCtx.Int64Type, 0),
                            "arr_len_ptr");

                        var recLen = _builder.BuildLoad2(llvmCtx.Int64Type, recPtr, "arr_len");
                        recLen.SetAlignment(8);

                        finalArg = recLen;
                        formatStr = _builder.BuildGlobalStringPtr("Array(len=%ld)\n", "fmt_array");
                        break;

                    default:
                        throw new Exception($"Unsupported type for printing: {type}");
                }
            }

            //  Unified printf call
            return _builder.BuildCall2(
                _printfType,
                _printf,
                new[] { formatStr, finalArg },
                "printf_call");
        }

        public LLVMValueRef VisitPrint(PrintNode expr) // x.addRange([{index: 5, name: "voldemort", age: 80}, {index: 6, name: "dumbledore", age: 70}, {index: 7, name: "MERLIN", age: 101}])
        {
            var valueToPrint = Visit(expr.Expression);
            return AddImplicitPrint(valueToPrint, expr.Expression.Type); // x.add({name: "Hary potter2", age: 201})
        } // x=dataframe(["name", "age"], type=[string, int])

        //  x=record({name: "Hary potter", age: 30, rating: 10.5585})  
        //  for(i=0; i<100000; i++) x.add({name: "Hary potter", age: 10 + random(1,100)})
        //  x=dataframe(["name", "age"], [{name: "dan", age: 30}, {name: "alice", age: 25}])
        //  x.where(d => d.age > 20)
        //  x.map(d => d.age + 10)
        //  x=record({name: "Hary potter", age: 30, rating: 10.5585}) 
        
        //  for(i=0; i<520000; i++) x.add({name: "Hary potter", age: 10 + random(1,100)})
        public LLVMValueRef VisitWhere(WhereNode expr) // x.where(d=> d.age > 90)
        {
            var sourceType = expr.SourceExpr.Type;
            var program = new SequenceNode();

            // Handle source array type and different element types
            if (sourceType is ArrayType arrType)
                program = WhereForArray(sourceType, expr);
            else if (sourceType is DataframeType recType)
                program = WhereForDataframe(sourceType, expr);

            PerformSemanticAnalysis(program);

            return VisitSequence(program);
        }

        public SequenceNode WhereForDataframe(Type sourceType, WhereNode expr)
        {
            var srcVar = "__where_src";
            var resultVar = "__where_result"; // 10 000 002
            var iVar = "__where_i";

            if (sourceType is not DataframeType dfType)
                throw new Exception("Where only supports dataframe");

            // --- Construct empty result dataframe (same schema) ---
            var columnsArray = new ArrayNode(
                dfType.ColumnNames.Select(c => (ExpressionNode)new StringNode(c)).ToList()
            );

            var recType = sourceType as DataframeType;
            var types = new List<ExpressionNode>();
            foreach (var item in recType.DataTypes)
            {
                if (item is IntType) types.Add(new NumberNode(0));
                else if (item is FloatType) types.Add(new FloatNode(0));
                else if (item is BoolType) types.Add(new BooleanNode(false));
                else if (item is StringType) types.Add(new StringNode(""));
            }

            var typeArray = new ArrayNode(types);

            var resultDf = new DataframeNode(new List<NamedArgumentNode>
            {
                new NamedArgumentNode("columns", columnsArray),
                new NamedArgumentNode("type", typeArray)
            });

            // --- Assignments ---
            var srcAssign = new AssignNode(srcVar, expr.SourceExpr);
            var resultAssign = new AssignNode(resultVar, resultDf);
            var indexInit = new AssignNode(iVar, new NumberNode(0));

            // --- Loop condition ---
            var cond = new ComparisonNode(
                new IdNode(iVar),
                "<",
                new LengthNode(new IdNode(srcVar))
            );

            var step = new IncrementNode(iVar);

            // --- Correct row access ---
            var current = new IndexNode(new IdNode(srcVar), new IdNode(iVar));

            // Replace iterator in predicate
            var predicate = ReplaceIterator(expr.Condition, expr.IteratorId.Name, current);

            // result.add(current)
            var add = new AddNode(new IdNode(resultVar), current);

            var ifBody = new SequenceNode();
            ifBody.Statements.Add(add);

            var ifNode = new IfNode(predicate, ifBody);

            var loopBody = new SequenceNode();
            loopBody.Statements.Add(ifNode);

            var loop = new ForLoopNode(indexInit, cond, step, loopBody);

            var program = new SequenceNode();
            program.Statements.Add(srcAssign);
            program.Statements.Add(resultAssign);
            program.Statements.Add(loop);
            program.Statements.Add(new IdNode(resultVar));
            return program;
        }

        public SequenceNode WhereForArray(Type sourceType, WhereNode expr)
        {
            // Cast here so we can access ElementType
            var arrType = (ArrayType)sourceType;
            var elementType = arrType.ElementType;

            var srcVarName = "__where_src";
            var resultVarName = "__where_result";
            var indexVarName = "__where_i";

            if (elementType is not IntType && elementType is not FloatType &&
                elementType is not StringType && elementType is not BoolType)
                throw new Exception("Unsupported array element type: " + elementType);

            var resultArray = new ArrayNode(new List<ExpressionNode>()) { ElementType = elementType };

            var srcAssign = new AssignNode(srcVarName, expr.SourceExpr);
            var resultAssign = new AssignNode(resultVarName, resultArray);

            // Create the initialization node
            var indexInit = new AssignNode(indexVarName, new NumberNode(0));

            var loopCond = new ComparisonNode(
                new IdNode(indexVarName),
                "<",
                new LengthNode(new IdNode(srcVarName))
            );

            var loopStep = new IncrementNode(indexVarName);
            var currentElement = new IndexNode(new IdNode(srcVarName), new IdNode(indexVarName));

            // CRITICAL: Ensure ReplaceIterator handles LogicalOpNodes!
            ExpressionNode ifCond = ReplaceIterator(expr.Condition, expr.IteratorId.Name, currentElement);

            var addNode = new AddNode(new IdNode(resultVarName), currentElement);

            var ifBody = new SequenceNode();
            ifBody.Statements.Add(addNode);
            var ifNode = new IfNode(ifCond, ifBody);

            var loopBody = new SequenceNode();
            loopBody.Statements.Add(ifNode);

            // Pass the AssignNode (indexInit) here
            var forLoop = new ForLoopNode(indexInit, loopCond, loopStep, loopBody);

            var program = new SequenceNode();
            program.Statements.Add(srcAssign);
            program.Statements.Add(resultAssign);
            program.Statements.Add(forLoop);
            program.Statements.Add(new IdNode(resultVarName));

            return program;
        }



        public LLVMValueRef VisitMap(MapNode expr)
        {
            var sourceType = expr.SourceExpr.Type;

            SequenceNode program;
            if (sourceType is ArrayType)
            {
                Console.WriteLine("ARRAY.........................................................................");
                program = MapForArray(sourceType, expr);
            }
            else if (sourceType is DataframeType)
            {
                Console.WriteLine("DATAFRAME.....................................................................");
                program = MapForDataframe(sourceType, expr); // This helper handles Dataframe result types
            }
            else
            {
                throw new Exception($"Cannot call map on {sourceType}");
            }

            PerformSemanticAnalysis(program);
            return VisitSequence(program);
        }

        public SequenceNode MapForArray(Type sourceType, MapNode expr)
        {
            var arrType = (ArrayType)sourceType;

            // Use 'as' or check type before casting to avoid the crash
            if (expr.SourceExpr.Type is not ArrayType resultArrType)
                throw new Exception($"Map on Array expected to return ArrayType, but got {expr.Type}");

            var srcVarName = "__map_src";
            var resultVarName = "__map_result";
            var indexVarName = "__map_i";

            // Use the element type from the RESOLVED result type
            var emptyResultArray = new ArrayNode(new List<ExpressionNode>())
            {
                ElementType = resultArrType.ElementType
            };

            var srcAssign = new AssignNode(srcVarName, expr.SourceExpr);
            var resultAssign = new AssignNode(resultVarName, emptyResultArray);
            var indexInit = new AssignNode(indexVarName, new NumberNode(0));

            var loopCond = new ComparisonNode(new IdNode(indexVarName), "<", new LengthNode(new IdNode(srcVarName)));
            var loopStep = new IncrementNode(indexVarName);

            var currentElement = new IndexNode(new IdNode(srcVarName), new IdNode(indexVarName));

            // Transform element
            var mappedValue = ReplaceIterator(expr.Assignment, expr.IteratorId.Name, currentElement);

            // Use .add() for dynamic growth, same as 'where'
            var addNode = new AddNode(new IdNode(resultVarName), mappedValue);

            var loopBody = new SequenceNode();
            loopBody.Statements.Add(addNode);

            var forLoop = new ForLoopNode(indexInit, loopCond, loopStep, loopBody);

            var program = new SequenceNode();
            program.Statements.Add(srcAssign);
            program.Statements.Add(resultAssign);
            program.Statements.Add(forLoop);
            program.Statements.Add(new IdNode(resultVarName));

            return program;
        }
        public SequenceNode MapForDataframe(Type sourceType, MapNode expr)
        {
            var dfType = (DataframeType)sourceType;

            var srcVar = "__map_src";
            var resultVar = "__map_result";
            var iVar = "__map_i";

            ExpressionNode resultConstructor;

            // 1. Determine if we are building a new Dataframe or a simple Array
            if (expr.Type is DataframeType resDfType)
            {
                // Case: User returned a Record -> Result is a Dataframe
                var columnsExprs = resDfType.ColumnNames.Select(c => (ExpressionNode)new StringNode(c)).ToList();
                var typeExprs = resDfType.DataTypes.Select(t =>
                {
                    if (t is IntType) return (ExpressionNode)new NumberNode(0);
                    if (t is FloatType) return (ExpressionNode)new FloatNode(0);
                    return (ExpressionNode)new StringNode("");
                }).ToList();

                resultConstructor = new DataframeNode(new List<ExpressionNode> {
            new NamedArgumentNode("columns", new ArrayNode(columnsExprs)),
            new NamedArgumentNode("type", new ArrayNode(typeExprs))
        });
                resultConstructor.SetType(resDfType);
            }
            else if (expr.Type is ArrayType resArrType)
            {
                // Case: User returned a primitive (like x.age + 10) -> Result is an Array
                resultConstructor = new ArrayNode(new List<ExpressionNode>())
                {
                    ElementType = resArrType.ElementType
                };
                resultConstructor.SetType(resArrType);
            }
            else
            {
                throw new Exception($"Unsupported result type for Dataframe map: {expr.Type}");
            }

            // 2. Synthesize Assignments
            var srcAssign = new AssignNode(srcVar, expr.SourceExpr);
            var resultAssign = new AssignNode(resultVar, resultConstructor);
            var indexInit = new AssignNode(iVar, new NumberNode(0));

            // 3. Loop Logic (Generic for both results)
            var cond = new ComparisonNode(new IdNode(iVar), "<", new LengthNode(new IdNode(srcVar)));
            var step = new IncrementNode(iVar);

            // Access the Row from source: src[i]
            var current = new IndexNode(new IdNode(srcVar), new IdNode(iVar));

            // Transform: Replace 'x' with the row access
            var transformedValue = ReplaceIterator(expr.Assignment, expr.IteratorId.Name, current);

            // 4. Append to result (AddNode handles both Array.add and Dataframe.add)
            var addNode = new AddNode(new IdNode(resultVar), transformedValue);

            var loopBody = new SequenceNode();
            loopBody.Statements.Add(addNode);

            var loop = new ForLoopNode(indexInit, cond, step, loopBody);

            // 5. Assemble
            var program = new SequenceNode();
            program.Statements.Add(srcAssign);
            program.Statements.Add(resultAssign);
            program.Statements.Add(loop);
            program.Statements.Add(new IdNode(resultVar));

            return program;
        }





        /* Pseudo code:

        dfcopy = df.copy;
        for(i=0; i< dfcopy.length; i++)
        {
            dfcopy[i].age = dfcopy[i].age + 10;
        }

        return dfcopy;

        */






        public LLVMValueRef VisitCopy(CopyNode expr)
        {
            var value = Visit(expr.Source);
            return EmitDeepCopy(value, expr.Source.Type);
        }

        private LLVMValueRef EmitDeepCopy(LLVMValueRef sourceVal, Type type)
        {
            if (type is ArrayType at)
                return CopyArray(sourceVal, at);

            if (type is RecordType rt)
                return CopyRecord(sourceVal, rt);

            // 3. Dataframes
            if (type is DataframeType dt)
                return CopyDataframe(sourceVal, dt);

            // 4. Primitives (Your existing Boxed Logic)
            if (type is IntType || type is FloatType || type is BoolType)
            {
                var ctx = _module.Context;
                var mallocFunc = GetOrDeclareMalloc();
                var i64 = ctx.Int64Type;
                var newBox = _builder.BuildCall2(_mallocType, mallocFunc, new[] { LLVMValueRef.CreateConstInt(i64, 8) }, "primitive_box_copy");
                var llvmType = GetLLVMType(type);
                var srcTyped = _builder.BuildBitCast(sourceVal, LLVMTypeRef.CreatePointer(llvmType, 0));
                var dstTyped = _builder.BuildBitCast(newBox, LLVMTypeRef.CreatePointer(llvmType, 0));
                var val = _builder.BuildLoad2(llvmType, srcTyped, "box_val");
                _builder.BuildStore(val, dstTyped);
                return newBox;
            }

            return sourceVal;
        }

        private LLVMValueRef CopyDataframe(LLVMValueRef dfPtr, DataframeType dfType)
        {
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // The Dataframe struct layout: { ptr cols, ptr rows, ptr types }
            var dfStructType = LLVMTypeRef.CreateStruct(new[] { i8Ptr, i8Ptr, i8Ptr }, false);

            // 1. Allocate the NEW Dataframe header container
            var newDfPtr = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(),
                new[] { LLVMValueRef.CreateConstInt(ctx.Int64Type, 24) }, "df_copy_header");

            // 2. Deep Copy Columns (Array of Strings)
            var colsPtr = _builder.BuildLoad2(i8Ptr, _builder.BuildStructGEP2(dfStructType, dfPtr, 0), "ld_cols");
            var columnArrayType = new ArrayType(new StringType()); // Columns are always strings
            var newCols = CopyArray(colsPtr, columnArrayType);
            _builder.BuildStore(newCols, _builder.BuildStructGEP2(dfStructType, newDfPtr, 0));

            // 3. Deep Copy Rows (Array of Records)
            var rowsPtr = _builder.BuildLoad2(i8Ptr, _builder.BuildStructGEP2(dfStructType, dfPtr, 1), "ld_rows");
            var rowArrayType = new ArrayType(dfType.RowType); // Uses the RowType from your DataframeType
            var newRows = CopyArray(rowsPtr, rowArrayType);
            _builder.BuildStore(newRows, _builder.BuildStructGEP2(dfStructType, newDfPtr, 1));

            // 4. Deep Copy DataTypes (Array of Integers)
            var typesPtr = _builder.BuildLoad2(i8Ptr, _builder.BuildStructGEP2(dfStructType, dfPtr, 2), "ld_types");
            var typeArrayType = new ArrayType(new IntType()); // Metadata array is always ints
            var newTypes = CopyArray(typesPtr, typeArrayType);
            _builder.BuildStore(newTypes, _builder.BuildStructGEP2(dfStructType, newDfPtr, 2));

            return newDfPtr;
        }

        private LLVMValueRef CopyRecord(LLVMValueRef recordPtr, RecordType recordType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var mallocFunc = GetOrDeclareMalloc();

            // 1. Allocate the new record buffer (exactly like VisitRecord)
            var numFields = (ulong)recordType.RecordFields.Count;
            var newRecordBuffer = _builder.BuildCall2(_mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, numFields * 8) }, "record_copy_buffer");

            for (int i = 0; i < recordType.RecordFields.Count; i++)
            {
                var fieldType = recordType.RecordFields[i].Type;
                var index = LLVMValueRef.CreateConstInt(i64, (ulong)i);

                // 2. Get the address of the pointer in the OLD record
                var srcSlotPtr = _builder.BuildGEP2(i8Ptr, recordPtr, new[] { index }, "src_slot");
                // Load the pointer stored there
                var storedPtr = _builder.BuildLoad2(i8Ptr, srcSlotPtr, "stored_ptr");

                // 3. Perform the Deep Copy
                // Note: For Primitives, your architecture stores a POINTER to the value.
                // We must handle that inside EmitDeepCopy or here.
                var copiedValuePtr = EmitDeepCopy(storedPtr, fieldType);

                // 4. Store the new pointer into the NEW record
                var dstSlotPtr = _builder.BuildGEP2(i8Ptr, newRecordBuffer, new[] { index }, "dst_slot");
                _builder.BuildStore(copiedValuePtr, dstSlotPtr);
            }

            return newRecordBuffer;
        }

        private LLVMValueRef CopyArray(LLVMValueRef srcHeaderPtr, ArrayType arrayType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            var elementType = arrayType.ElementType;

            var arrayStructType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);
            var mallocFunc = GetOrDeclareMalloc();

            // 1. Load Length and Data Pointer
            var srcLenPtr = _builder.BuildStructGEP2(arrayStructType, srcHeaderPtr, 0, "src_len_ptr");
            var srcDataPtrField = _builder.BuildStructGEP2(arrayStructType, srcHeaderPtr, 2, "src_data_ptr_field");
            var length = _builder.BuildLoad2(i64, srcLenPtr, "length");
            var srcDataPtr = _builder.BuildLoad2(i8Ptr, srcDataPtrField, "src_data_ptr");

            // 2. Allocate New Header
            var newHeaderPtr = _builder.BuildCall2(_mallocType, mallocFunc, new[] { LLVMValueRef.CreateConstInt(i64, 24) }, "new_header");

            // 3. Determine Stride
            bool isComplex = elementType is RecordType || elementType is ArrayType;
            var stride = (elementType is BoolType) ? 1 : 8;
            var byteCount = _builder.BuildMul(length, LLVMValueRef.CreateConstInt(i64, (ulong)stride), "byte_count");

            // 4. Allocate New Data Buffer
            var isZero = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, length, LLVMValueRef.CreateConstInt(i64, 0));
            var allocSize = _builder.BuildSelect(isZero, LLVMValueRef.CreateConstInt(i64, (ulong)(4 * stride)), byteCount);
            var newDataPtr = _builder.BuildCall2(_mallocType, mallocFunc, new[] { allocSize }, "new_data");

            // 5. Initialize New Header
            _builder.BuildStore(length, _builder.BuildStructGEP2(arrayStructType, newHeaderPtr, 0));
            _builder.BuildStore(length, _builder.BuildStructGEP2(arrayStructType, newHeaderPtr, 1));
            _builder.BuildStore(newDataPtr, _builder.BuildStructGEP2(arrayStructType, newHeaderPtr, 2));

            // 6. Branching for Copy Logic
            var hasElements = _builder.BuildICmp(LLVMIntPredicate.LLVMIntUGT, length, LLVMValueRef.CreateConstInt(i64, 0));
            var copyBlock = ctx.AppendBasicBlock(_builder.InsertBlock.Parent, "copy.do");
            var endBlock = ctx.AppendBasicBlock(_builder.InsertBlock.Parent, "copy.end");
            _builder.BuildCondBr(hasElements, copyBlock, endBlock);

            _builder.PositionAtEnd(copyBlock);

            if (!isComplex)
            {
                // Simple bitwise copy for primitives
                var memcpyFunc = GetOrDeclareMemmove();
                _builder.BuildCall2(_memmoveType, memcpyFunc, new[] { newDataPtr, srcDataPtr, byteCount }, "");
            }
            else
            {
                // Recursive loop for Records/Arrays/Dataframes
                EmitArrayLoopCopy(length, newDataPtr, srcDataPtr, elementType);
            }

            _builder.BuildBr(endBlock);
            _builder.PositionAtEnd(endBlock);
            return newHeaderPtr;
        }

        private void EmitArrayLoopCopy(LLVMValueRef length, LLVMValueRef newDataPtr, LLVMValueRef srcDataPtr, Type elementType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var llvmElemType = GetLLVMType(elementType);

            // Cast i8* data pointers to the actual element type pointers
            var srcTypedPtr = _builder.BuildBitCast(srcDataPtr, LLVMTypeRef.CreatePointer(llvmElemType, 0));
            var dstTypedPtr = _builder.BuildBitCast(newDataPtr, LLVMTypeRef.CreatePointer(llvmElemType, 0));

            var loopBlock = ctx.AppendBasicBlock(_builder.InsertBlock.Parent, "array.copy.loop");
            var exitBlock = ctx.AppendBasicBlock(_builder.InsertBlock.Parent, "array.copy.exit");

            var indexPtr = _builder.BuildAlloca(i64, "index_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, 0), indexPtr);
            _builder.BuildBr(loopBlock);

            _builder.PositionAtEnd(loopBlock);
            var i = _builder.BuildLoad2(i64, indexPtr, "i");

            // Load from source, Deep Copy, Store to destination
            var srcElemPtr = _builder.BuildGEP2(llvmElemType, srcTypedPtr, new[] { i }, "src_elem_p");
            var srcElemVal = _builder.BuildLoad2(llvmElemType, srcElemPtr, "src_elem_v");

            var copiedVal = EmitDeepCopy(srcElemVal, elementType); // RECURSION

            var dstElemPtr = _builder.BuildGEP2(llvmElemType, dstTypedPtr, new[] { i }, "dst_elem_p");
            _builder.BuildStore(copiedVal, dstElemPtr);

            // Increment index
            var nextI = _builder.BuildAdd(i, LLVMValueRef.CreateConstInt(i64, 1), "next_i");
            _builder.BuildStore(nextI, indexPtr);

            var cond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntULT, nextI, length, "loop_cond");
            _builder.BuildCondBr(cond, loopBlock, exitBlock);

            _builder.PositionAtEnd(exitBlock);
        }


        // public LLVMValueRef VisitCopyRecord(CopyRecordNode expr)
        // {
        //     return VisitCopy(expr);
        // }


        public LLVMValueRef VisitMapExprMutating(MapNode expr) // not in use, maybe use with like an argument, eg: x.map(d => d+2, true) 
        {
            // Temp variable names
            var srcVarName = "__map_src";
            var indexVarName = "__map_i";

            // 1. Store source array
            var srcAssign = new AssignNode(srcVarName, expr.SourceExpr);

            // 2. i = 0
            var indexAssign = new AssignNode(indexVarName, new NumberNode(0));

            // 3. Loop condition: i < src.length
            var loopCond = new ComparisonNode(
                new IdNode(indexVarName),
                "<",
                new LengthNode(new IdNode(srcVarName))
            );

            // x=[1,2,3]
            // newX = x;
            // for (i=0; i<x.length; i++)
            // newX[i] = newX[i] + 2

            // 4. i++
            var loopStep = new IncrementNode(indexVarName);

            // 5. src[i]
            var currentElement = new IndexNode(
                new IdNode(srcVarName),
                new IdNode(indexVarName)
            );

            // 6. Replace iterator (d => ...) with actual element
            var mappedExpr = ReplaceIterator(
                expr.Assignment,
                expr.IteratorId.Name,
                currentElement
            );

            // 7. src[i] = mappedExpr
            var indexAssignNode = new IndexAssignNode(
                new IdNode(srcVarName),
                new IdNode(indexVarName),
                mappedExpr
            );

            // 8. Loop body
            var loopBody = new SequenceNode();
            loopBody.Statements.Add(indexAssignNode);

            var forLoop = new ForLoopNode(
                indexAssign,
                loopCond,
                loopStep,
                loopBody
            );

            // 9. Full sequence
            var program = new SequenceNode();
            program.Statements.Add(srcAssign);
            program.Statements.Add(forLoop);

            // Return the modified array
            program.Statements.Add(new IdNode(srcVarName));

            // Optional semantic check
            PerformSemanticAnalysis(program);

            return VisitSequence(program);
        }

        public ExpressionNode ReplaceIterator(ExpressionNode node, string iteratorName, ExpressionNode replacement)
        {
            if (node == null) return null;

            // 1. Base Case: The ID itself (e.g., $row)
            if (node is IdNode id && id.Name == iteratorName)
            {
                return replacement;
            }

            // 2. RecordFieldNode: This replaces PropertyAccessNode
            // This handles $row.columnName
            if (node is RecordFieldNode rf)
            {
                var newSrc = ReplaceIterator(rf.IdRecord, iteratorName, replacement);
                var newRf = new RecordFieldNode(newSrc, rf.IdField);
                newRf.SetType(rf.Type); // Preserve the type found during initial analysis
                return newRf;
            }

            // 3. NamedArgumentNode
            if (node is NamedArgumentNode arg)
            {
                var newValue = ReplaceIterator(arg.Value, iteratorName, replacement);
                var newArg = new NamedArgumentNode(arg.Name, newValue);
                newArg.SetType(arg.Type);
                return newArg;
            }

            // 4. RecordNode
            if (node is RecordNode rec)
            {
                var newFields = new List<NamedArgumentNode>();
                foreach (var field in rec.Fields)
                {
                    var newValue = ReplaceIterator(field.Value, iteratorName, replacement);
                    var newField = new NamedArgumentNode(field.Label, newValue);
                    newField.SetType(field.Type);
                    newFields.Add(newField);
                }
                var newRec = new RecordNode(newFields);
                newRec.SetType(rec.Type);
                return newRec;
            }

            // 5. Comparison
            if (node is ComparisonNode comp)
            {
                var newComp = new ComparisonNode(
                    ReplaceIterator(comp.Left, iteratorName, replacement),
                    comp.Operator,
                    ReplaceIterator(comp.Right, iteratorName, replacement)
                );
                newComp.SetType(comp.Type);
                return newComp;
            }

            // 6. Binary Operations
            if (node is BinaryOpNode bin)
            {
                var newBin = new BinaryOpNode(
                    ReplaceIterator(bin.Left, iteratorName, replacement),
                    bin.Operator,
                    ReplaceIterator(bin.Right, iteratorName, replacement)
                );
                newBin.SetType(bin.Type);
                return newBin;
            }

            // 7. Indexing
            if (node is IndexNode idx)
            {
                var nSrc = ReplaceIterator(idx.SourceExpression, iteratorName, replacement);
                var nIdx = ReplaceIterator(idx.IndexExpression, iteratorName, replacement);
                var newNode = new IndexNode(nSrc, nIdx);
                newNode.SetType(idx.Type);
                return newNode;
            }

            // 8. Unary
            if (node is UnaryOpNode un)
            {
                var newUn = new UnaryOpNode(un.Operator, ReplaceIterator(un.Operand, iteratorName, replacement));
                newUn.SetType(un.Type);
                return newUn;
            }
            // 9. Logical Operations (and, or)
            if (node is LogicalOpNode log)
            {
                var newLog = new LogicalOpNode(
                    ReplaceIterator(log.Left, iteratorName, replacement),
                    log.Operator, // or log.Op depending on your property name
                    ReplaceIterator(log.Right, iteratorName, replacement)
                );
                newLog.SetType(log.Type);
                return newLog;
            }

            return node;
        }

        public LLVMValueRef VisitArray(ArrayNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            uint count = (uint)expr.Elements.Count;

            bool isBoolArray = expr.ElementType is BoolType;
            int stride = isBoolArray ? 1 : 8; // 1 byte for bool, 8 for others

            var arrayStructType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);
            var mallocFunc = GetOrDeclareMalloc();

            // 1. Header (24 bytes)
            var headerRaw = _builder.BuildCall2(_mallocType, mallocFunc, new[] { LLVMValueRef.CreateConstInt(i64, 24) }, "arr_header");

            // 2. Data Buffer
            var capacity = count > 0 ? count * 2 : 4;
            if (expr.Capacity != null) capacity = (uint)expr.Capacity * 2;
            var dataSize = LLVMValueRef.CreateConstInt(i64, (ulong)capacity * (ulong)stride);
            var dataPtr = _builder.BuildCall2(_mallocType, mallocFunc, new[] { dataSize }, "arr_data");

            // 3. Store Metadata
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, count), _builder.BuildStructGEP2(arrayStructType, headerRaw, 0));
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)capacity), _builder.BuildStructGEP2(arrayStructType, headerRaw, 1));
            _builder.BuildStore(dataPtr, _builder.BuildStructGEP2(arrayStructType, headerRaw, 2));

            // 4. Populate
            for (int i = 0; i < expr.Elements.Count; i++)
            {
                var val = Visit(expr.Elements[i]);
                var idx = LLVMValueRef.CreateConstInt(i64, (ulong)i);

                if (isBoolArray)
                {
                    // Store as i8 (1 byte)
                    var castVal = _builder.BuildZExt(val, i8, "bool_to_i8");
                    var elementPtr = _builder.BuildGEP2(i8, dataPtr, new[] { idx }, "elem_ptr");
                    _builder.BuildStore(castVal, elementPtr);
                }
                else
                {
                    // Store as i8Ptr (8 bytes)
                    var castVal = _builder.BuildBitCast(val, i8Ptr, "val_to_ptr");
                    var elementPtr = _builder.BuildGEP2(i8Ptr, dataPtr, new[] { idx }, "elem_ptr");
                    _builder.BuildStore(castVal, elementPtr);
                }
            }

            return headerRaw;
        }

        public LLVMValueRef VisitIndex(IndexNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            var func = _builder.InsertBlock.Parent;

            // 1. Visit Array (Header Pointer) and Index
            var headerPtr = Visit(expr.SourceExpression);
            var indexVal = Visit(expr.IndexExpression);

            var name = (expr.SourceExpression as IdNode).Name;
            var sourceType = _context.Get(name).Type;

            if (sourceType is ArrayType)
            {
                if (indexVal.TypeOf == ctx.DoubleType)
                    indexVal = _builder.BuildFPToSI(indexVal, i64, "idx_int");

                // Define the Struct Layout: { i64 len, i64 cap, i8* data }
                var arrayStructType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);

                // 2. Load Metadata from Header
                var lenFieldPtr = _builder.BuildStructGEP2(arrayStructType, headerPtr, 0, "len_field_ptr");
                var dataFieldPtr = _builder.BuildStructGEP2(arrayStructType, headerPtr, 2, "data_field_ptr");

                var arrayLen = _builder.BuildLoad2(i64, lenFieldPtr, "array_len");
                var dataPtr = _builder.BuildLoad2(i8Ptr, dataFieldPtr, "data_ptr");

                // 3. --- BOUNDS CHECK ---
                var isNegative = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, indexVal, LLVMValueRef.CreateConstInt(i64, 0));
                var isTooBig = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, indexVal, arrayLen);
                var isInvalid = _builder.BuildOr(isNegative, isTooBig, "is_invalid");

                var failBlock = ctx.AppendBasicBlock(func, "bounds.fail");
                var safeBlock = ctx.AppendBasicBlock(func, "bounds.ok");
                _builder.BuildCondBr(isInvalid, failBlock, safeBlock);

                // --- FAIL BLOCK ---
                _builder.PositionAtEnd(failBlock);
                var errorMsg = _builder.BuildGlobalStringPtr("Runtime Error: Index Out of Bounds!\n", "err_msg");
                _builder.BuildCall2(_printfType, _printf, new[] { errorMsg }, "print_err");
                _builder.BuildRet(LLVMValueRef.CreateConstNull(i8Ptr));

                // --- SAFE BLOCK ---
                _builder.PositionAtEnd(safeBlock);

                // 4. STRIDE-AWARE ACCESS
                // Check if the actual language type is boolean
                bool isBool = ((ArrayType)expr.SourceExpression.Type).ElementType is BoolType;

                // If bool, elements are i8 (1 byte). Otherwise, they are pointers/i64 (8 bytes).
                var gepType = isBool ? i8 : i8Ptr;

                // We index into dataPtr starting at 0 (no +2 offset anymore!)
                var elementPtr = _builder.BuildGEP2(gepType, dataPtr, new[] { indexVal }, "elem_ptr");
                var rawValue = _builder.BuildLoad2(gepType, elementPtr, "raw_val");

                // 5. Transform based on Type
                // Note: If isBool, rawValue is i8. If Int/Float/String, rawValue is i8Ptr (8 bytes).
                return expr.Type switch
                {
                    StringType => _builder.BuildBitCast(rawValue, i8Ptr),
                    FloatType => _builder.BuildBitCast(rawValue, ctx.DoubleType),
                    IntType => _builder.BuildPtrToInt(rawValue, i64),
                    BoolType => _builder.BuildTrunc(rawValue, ctx.Int1Type, "to_bool"), // i8 -> i1
                    RecordType => _builder.BuildBitCast(rawValue, i8Ptr), // it did not have for record type before, but it still worked
                    DataframeType => _builder.BuildBitCast(rawValue, i8Ptr),
                    NullType => _builder.BuildTrunc(rawValue, i8Ptr),
                    _ => rawValue
                };
            }
            else if (sourceType is DataframeType)
            {
                var val = DataframeIndex(headerPtr, indexVal, expr);

                return _builder.BuildBitCast(val, i8Ptr);
            }

            return default;
        }

        private LLVMValueRef DataframeIndex(LLVMValueRef dataframePtr, LLVMValueRef indexValue, IndexNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            var i8PtrPtr = LLVMTypeRef.CreatePointer(i8Ptr, 0);

            var dfType = GetOrCreateDataframeType();
            var arrayType = GetOrCreateArrayType();
            var arrayPtrType = LLVMTypeRef.CreatePointer(arrayType, 0);

            // --- 1. Load rows array ---
            var rowsPtrPtr = _builder.BuildStructGEP2(dfType, dataframePtr, 1, "rows_ptr_ptr");
            var rowsPtr = _builder.BuildLoad2(arrayPtrType, rowsPtrPtr, "rows");

            // --- 2. Load data pointer ---
            var dataPtrPtr = _builder.BuildStructGEP2(arrayType, rowsPtr, 2, "data_ptr_ptr");
            var dataPtr = _builder.BuildLoad2(i8Ptr, dataPtrPtr, "data_ptr");

            // Cast to i8** (array of record pointers)
            var typedDataPtr = _builder.BuildBitCast(dataPtr, i8PtrPtr, "typed_data");

            // --- 3. (Optional but recommended) bounds check ---
            var lenPtr = _builder.BuildStructGEP2(arrayType, rowsPtr, 0, "len_ptr");
            var len = _builder.BuildLoad2(i64, lenPtr, "len");

            var inBounds = _builder.BuildICmp(
                LLVMIntPredicate.LLVMIntULT,
                indexValue,
                len,
                "in_bounds");

            var function = _builder.InsertBlock.Parent;
            var okBlock = ctx.AppendBasicBlock(function, "idx_ok");
            var errBlock = ctx.AppendBasicBlock(function, "idx_err");

            _builder.BuildCondBr(inBounds, okBlock, errBlock);

            // --- ERROR BLOCK ---
            _builder.PositionAtEnd(errBlock);
            // You can replace this with your runtime error handler
            _builder.BuildRet(LLVMValueRef.CreateConstNull(i8Ptr));

            // --- OK BLOCK ---
            _builder.PositionAtEnd(okBlock);

            // --- 4. Direct indexing ---
            var elemPtr = _builder.BuildGEP2(
                i8Ptr,               // element type
                typedDataPtr,
                new[] { indexValue },
                "elem_ptr");

            var recordPtr = _builder.BuildLoad2(i8Ptr, elemPtr, "record");

            return recordPtr;
        }

        private LLVMTypeRef GetOrCreateStructType(RecordType recordType)
        {
            string structName = "struct_" + string.Join("_", recordType.RecordFields.Select(f => f.Label));

            var structType = _module.GetTypeByName(structName);

            if (structType.Handle == IntPtr.Zero)
            {
                var fieldTypes = recordType.RecordFields
                    .Select(f => GetLLVMType(f.Value.Type))
                    .ToArray();

                structType = _module.Context.CreateNamedStruct(structName);
                structType.StructSetBody(fieldTypes, false);
            }
            return structType;
        }

        private LLVMTypeRef GetLLVMType(Type type)
        {
            var ctx = _module.Context;
            return type switch
            {
                FloatType => ctx.DoubleType,
                IntType => ctx.Int64Type,
                BoolType => ctx.Int1Type,
                NullType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), // Represent Null as i8*
                StringType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), // Strings are pointers
                ArrayType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0),  // Arrays  are pointers
                DataframeType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0),  // Dataframes are pointers
                RecordType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0),
                _ => throw new Exception($"Unsupported type: {type}") // it doesn't have a type? how?
            };
        }

        public LLVMValueRef VisitAdd(AddNode expr)
        {
            var name = (expr.SourceExpression as IdNode).Name;
            var sourceType = _context.Get(name).Type;

            if (_debug) Console.WriteLine("semantic type of array being indexed: " + sourceType);

            if (sourceType is ArrayType)
                return AddToArray(expr);
            else if (sourceType is DataframeType dfType)
                return AddToDataframe(expr, dfType); // Pass the type along

            throw new Exception("Add operation is only supported on arrays and dataframes");
        }

        private LLVMValueRef AddToArray(AddNode expr) // x.add({index: 5, name: "Hary potter", age: 35})
        {
            var headerPtr = Visit(expr.SourceExpression);
            ExecuteArrayAddition(headerPtr, Visit(expr.AddExpression), expr.AddExpression.Type);
            return headerPtr;
        }

        public LLVMValueRef AddToDataframe(AddNode expr, DataframeType dfType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // dataframe struct: { cols, rows, types }
            var dfStructType = LLVMTypeRef.CreateStruct(new[] { i8Ptr, i8Ptr, i8Ptr }, false);

            // 1. Get dataframe pointer
            var dfPtr = Visit(expr.SourceExpression);

            // 2. Evaluate the value to add (THIS IS THE IMPORTANT PART)
            // This should already be a record pointer
            var valueToAdd = Visit(expr.AddExpression);

            // Sanity check (optional but very useful while debugging)
            if (!(expr.AddExpression.Type is RecordType))
                throw new Exception("Can only add records to a dataframe");

            // 3. Get rows array pointer: df.rows
            var rowsFieldPtr = _builder.BuildStructGEP2(dfStructType, dfPtr, 1, "rows_field");
            var rowsArrayPtr = _builder.BuildLoad2(i8Ptr, rowsFieldPtr, "rows_array");

            // 4. Append directly to array
            ExecuteArrayAddition(rowsArrayPtr, valueToAdd, dfType.RowType);

            // 5. Return dataframe (so chaining works)
            return dfPtr;
        }

        private void ExecuteArrayAddition(LLVMValueRef headerPtr, LLVMValueRef valueToAdd, Type elementType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            // Array struct: { len, cap, data }
            var arrayStructType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);

            // Always treat elements as pointers
            var elementPtrType = i8Ptr;
            var stride = LLVMValueRef.CreateConstInt(i64, 8); // pointer size

            // Ensure value is a pointer
            var valueToStore = _builder.BuildBitCast(valueToAdd, i8Ptr, "val_to_ptr");

            // 1. Load metadata
            var lenFieldPtr = _builder.BuildStructGEP2(arrayStructType, headerPtr, 0, "len_ptr");
            var capFieldPtr = _builder.BuildStructGEP2(arrayStructType, headerPtr, 1, "cap_ptr");
            var dataFieldPtr = _builder.BuildStructGEP2(arrayStructType, headerPtr, 2, "data_ptr_ptr");

            var length = _builder.BuildLoad2(i64, lenFieldPtr, "len");
            var capacity = _builder.BuildLoad2(i64, capFieldPtr, "cap");
            var dataPtr = _builder.BuildLoad2(i8Ptr, dataFieldPtr, "data_ptr");

            // 2. Check if full
            var isFull = _builder.BuildICmp(LLVMIntPredicate.LLVMIntUGE, length, capacity, "is_full");

            var function = _builder.InsertBlock.Parent;
            var growBlock = ctx.AppendBasicBlock(function, "grow");
            var contBlock = ctx.AppendBasicBlock(function, "add_cont");
            var startBlock = _builder.InsertBlock;

            _builder.BuildCondBr(isFull, growBlock, contBlock);

            // 3. Grow
            _builder.PositionAtEnd(growBlock);

            var zero = LLVMValueRef.CreateConstInt(i64, 0);
            var four = LLVMValueRef.CreateConstInt(i64, 4);
            var two = LLVMValueRef.CreateConstInt(i64, 2);

            var newCap = _builder.BuildSelect(
                _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, capacity, zero),
                four,
                _builder.BuildMul(capacity, two),
                "new_cap"
            );

            var newByteSize = _builder.BuildMul(newCap, stride, "new_byte_size");

            var reallocFunc = GetOrDeclareRealloc();
            var newDataPtr = _builder.BuildCall2(_reallocType, reallocFunc, new[] { dataPtr, newByteSize }, "realloc_ptr");

            _builder.BuildStore(newCap, capFieldPtr);
            _builder.BuildStore(newDataPtr, dataFieldPtr);

            _builder.BuildBr(contBlock);

            // 4. Continue
            _builder.PositionAtEnd(contBlock);

            var finalDataPtr = _builder.BuildPhi(i8Ptr, "final_data_ptr");
            finalDataPtr.AddIncoming(
                new[] { dataPtr, newDataPtr },
                new[] { startBlock, growBlock },
                2
            );

            // CRITICAL FIX: cast buffer to pointer array
            var typedDataPtr = _builder.BuildBitCast(
                finalDataPtr,
                LLVMTypeRef.CreatePointer(i8Ptr, 0), // i8**
                "typed_data_ptr"
            );

            // Now GEP is correct
            var targetPtr = _builder.BuildGEP2(i8Ptr, typedDataPtr, new[] { length }, "target_ptr");

            _builder.BuildStore(valueToStore, targetPtr);

            // 5. Increment length
            var newLen = _builder.BuildAdd(length, LLVMValueRef.CreateConstInt(i64, 1), "next_len");
            _builder.BuildStore(newLen, lenFieldPtr);
        }

        public LLVMValueRef VisitAddRange(AddRangeNode expr)
        {
            Visit(expr.SourceExpression);
            Visit(expr.AddRangeExpression); // visits the array we are on, like x.addRange, then we visit x on this line of code

            var fullSequence = new SequenceNode();

            foreach (var item in ((ArrayNode)expr.AddRangeExpression).Elements)
            {
                var d = new AddNode(expr.SourceExpression, item);
                fullSequence.Statements.Add(d);
            }
            var newRange = Visit(fullSequence);

            return newRange;
        }

        public LLVMValueRef VisitRemove(RemoveNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            // Header: { i64 len, i64 cap, i8* data }
            var arrayStructType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);

            var headerPtr = Visit(expr.SourceExpression);
            var indexVal = Visit(expr.RemoveExpression);

            // 1. Determine if this is a packed (boolean) array
            bool isBool = ((ArrayType)expr.SourceExpression.Type).ElementType is BoolType;

            // This is the CRITICAL part:
            // If bool, we tell GEP the elements are 1-byte (i8).
            // If not, we tell GEP the elements are 8-bytes (i8Ptr).
            var gepStrideType = isBool ? i8 : i8Ptr;
            var strideMultiplier = isBool ? 1 : 8;
            var lenFieldPtr = _builder.BuildStructGEP2(arrayStructType, headerPtr, 0, "len_ptr");
            var dataFieldPtr = _builder.BuildStructGEP2(arrayStructType, headerPtr, 2, "data_ptr_ptr");
            var length = _builder.BuildLoad2(i64, lenFieldPtr, "len");
            var dataPtr = _builder.BuildLoad2(i8Ptr, dataFieldPtr, "data_ptr");

            // 2. Calculate Pointers
            // Use gepStrideType here! Do NOT use dataPtr.TypeOf.
            var dstPtr = _builder.BuildGEP2(gepStrideType, dataPtr, new[] { indexVal }, "dst");

            var nextIdx = _builder.BuildAdd(indexVal, LLVMValueRef.CreateConstInt(i64, 1));
            var srcPtr = _builder.BuildGEP2(gepStrideType, dataPtr, new[] { nextIdx }, "src");

            // 3. Calculate Bytes to Move
            var moveCount = _builder.BuildSub(_builder.BuildSub(length, indexVal), LLVMValueRef.CreateConstInt(i64, 1));
            var bytesToMove = _builder.BuildMul(moveCount, LLVMValueRef.CreateConstInt(i64, (ulong)strideMultiplier), "bytes");

            // 4. Memmove
            var memmoveFunc = GetOrDeclareMemmove();
            _builder.BuildCall2(_memmoveType, memmoveFunc, new[] { dstPtr, srcPtr, bytesToMove }, "");

            // 5. Update Length
            var newLen = _builder.BuildSub(length, LLVMValueRef.CreateConstInt(i64, 1));
            _builder.BuildStore(newLen, lenFieldPtr);

            return headerPtr;
        }

        public LLVMValueRef VisitRemoveRange(RemoveRangeNode expr)
        {
            Visit(expr.SourceExpression);
            Visit(expr.RemoveRangeExpression); // visits the array we are on, like x.addRange, then we visit x on this line of code

            var fullSequence = new SequenceNode();

            var elements = ((ArrayNode)expr.RemoveRangeExpression).Elements;

            // iterate backwards
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                var item = elements[i];
                var removeElement = new RemoveNode(expr.SourceExpression, item);
                fullSequence.Statements.Add(removeElement);
            }

            return Visit(fullSequence);
        }

        public LLVMValueRef VisitLength(LengthNode expr)
        {
            var ctx = _module.Context;
            var sourcePtr = Visit(expr.ArrayExpression); // pointer to array or dataframe
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            if (expr.ArrayExpression.Type is ArrayType)
            {
                var arrayStructType = LLVMTypeRef.CreateStruct(new[] { ctx.Int64Type, ctx.Int64Type, i8Ptr }, false);
                var lenPtr = _builder.BuildStructGEP2(arrayStructType, sourcePtr, 0, "length_ptr");
                var length = _builder.BuildLoad2(ctx.Int64Type, lenPtr, "length");
                length.SetAlignment(8);
                return length;
            }
            else if (expr.ArrayExpression.Type is DataframeType)
            {
                // Dataframe struct: { columns ptr, rows ptr, datatypes ptr }
                var dfStructType = LLVMTypeRef.CreateStruct(new[] { i8Ptr, i8Ptr, i8Ptr }, false);

                // Step 1: get pointer to the 'rows' field
                var rowsFieldPtr = _builder.BuildStructGEP2(dfStructType, sourcePtr, 1, "rows_ptr_field");

                // Step 2: load the pointer to the ArrayObject
                var rowsPtr = _builder.BuildLoad2(i8Ptr, rowsFieldPtr, "rows_ptr");

                // Step 3: cast to ArrayObject*
                var arrayStructType = LLVMTypeRef.CreateStruct(new[] { ctx.Int64Type, ctx.Int64Type, i8Ptr }, false);
                var rowsArrayPtr = _builder.BuildBitCast(rowsPtr, LLVMTypeRef.CreatePointer(arrayStructType, 0), "rows_array_ptr");

                // Step 4: get the 'length' field
                var lenPtr = _builder.BuildStructGEP2(arrayStructType, rowsArrayPtr, 0, "rows_length_ptr");
                var length = _builder.BuildLoad2(ctx.Int64Type, lenPtr, "rows_length");
                length.SetAlignment(8);

                return length;
            }
            else
                throw new Exception("Length operator is only supported on arrays and dataframes");
        }

        public LLVMValueRef VisitColumns(ColumnsNode expr)
        {
            var ctx = _module.Context;
            var sourcePtr = Visit(expr.DataframeExpression); // pointer to dataframe
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            if (expr.DataframeExpression.Type is DataframeType)
            {
                // Dataframe struct: { columns ptr, rows ptr, datatypes ptr }
                var dfStructType = LLVMTypeRef.CreateStruct(new[] { i8Ptr, i8Ptr, i8Ptr }, false);

                // Step 1: get pointer to the 'columns' field
                var columnsFieldPtr = _builder.BuildStructGEP2(dfStructType, sourcePtr, 0, "columns_ptr_field");

                // Step 2: load the pointer to the columns array
                var columnsPtr = _builder.BuildLoad2(i8Ptr, columnsFieldPtr, "columns_ptr");

                return columnsPtr; // Return the pointer to the columns array (i8*)
            }
            else
                throw new Exception("Columns operator is only supported on dataframes");
        }

        private LLVMValueRef GetArrayLength(LLVMValueRef arrayPtr)
        {
            var i64 = _module.Context.Int64Type;
            var lengthGEP = _builder.BuildGEP2(i64, arrayPtr, new[] { LLVMValueRef.CreateConstInt(i64, 0) }, "length_ptr");
            return _builder.BuildLoad2(i64, lengthGEP, "length");  // Load the array length
        }

        private LLVMValueRef GetArrayCapacity(LLVMValueRef arrayPtr)
        {
            var i64 = _module.Context.Int64Type;
            var capacityGEP = _builder.BuildGEP2(i64, arrayPtr, new[] { LLVMValueRef.CreateConstInt(i64, 1) }, "capacity_ptr");
            return _builder.BuildLoad2(i64, capacityGEP, "capacity");  // Load the array capacity
        }

        private LLVMValueRef GetArrayDataPtr(LLVMValueRef arrayPtr)
        {
            var i64 = _module.Context.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            var dataPtrGEP = _builder.BuildGEP2(i64, arrayPtr, new[] { LLVMValueRef.CreateConstInt(i64, 2) }, "data_ptr");
            return _builder.BuildLoad2(i8Ptr, dataPtrGEP, "data_ptr");  // Pointer to array data
        }

        public LLVMValueRef VisitMin(MinNode expr)
        {
            var arrayVar = "__min_array";
            var indexVar = "__min_i";
            var minVar = "__min_val";

            // Save array to temp variable
            var arrayAssign = new AssignNode(arrayVar, expr.ArrayExpression);

            // Initialize loop index i = 1
            var indexAssign = new AssignNode(indexVar, new NumberNode(1));

            // Initialize min value = array[0]
            var firstElement = new IndexNode(new IdNode(arrayVar), new NumberNode(0));
            var minAssign = new AssignNode(minVar, firstElement);

            // Loop condition: i < array.length
            var loopCond = new ComparisonNode(
                new IdNode(indexVar),
                "<",
                new LengthNode(new IdNode(arrayVar))
            );

            // Loop step: i++
            var loopStep = new IncrementNode(indexVar);

            // Current element: array[i]
            var currentElement = new IndexNode(new IdNode(arrayVar), new IdNode(indexVar));

            // Update min if current element < min
            var ifCond = new ComparisonNode(currentElement, "<", new IdNode(minVar));
            var ifBody = new SequenceNode();
            ifBody.Statements.Add(new AssignNode(minVar, currentElement));
            var ifNode = new IfNode(ifCond, ifBody);

            // Loop body
            var loopBody = new SequenceNode();
            loopBody.Statements.Add(ifNode);

            // Build for loop
            var forLoop = new ForLoopNode(indexAssign, loopCond, loopStep, loopBody);

            // Sequence: array assign, min assign, loop
            var program = new SequenceNode();
            program.Statements.Add(arrayAssign);
            program.Statements.Add(minAssign);
            program.Statements.Add(forLoop);

            // Return min value
            program.Statements.Add(new IdNode(minVar));

            // Semantic analysis (optional)
            PerformSemanticAnalysis(program);

            // Visit the AST to generate LLVM IR
            return VisitSequence(program);
        }

        public LLVMValueRef VisitMax(MaxNode expr)
        {
            var arrayVar = "__max_array";
            var indexVar = "__max_i";
            var maxVar = "__max_val";

            var arrayAssign = new AssignNode(arrayVar, expr.ArrayExpression);
            var firstElement = new IndexNode(new IdNode(arrayVar), new NumberNode(0));
            var maxAssign = new AssignNode(maxVar, firstElement);
            var indexAssign = new AssignNode(indexVar, new NumberNode(1));

            var loopCond = new ComparisonNode(
                new IdNode(indexVar),
                "<",
                new LengthNode(new IdNode(arrayVar))
            );

            var loopStep = new IncrementNode(indexVar);
            var currentElement = new IndexNode(new IdNode(arrayVar), new IdNode(indexVar));

            // Update max if current element > max
            var ifCond = new ComparisonNode(currentElement, ">", new IdNode(maxVar));
            var ifBody = new SequenceNode();
            ifBody.Statements.Add(new AssignNode(maxVar, currentElement));
            var ifNode = new IfNode(ifCond, ifBody);

            var loopBody = new SequenceNode();
            loopBody.Statements.Add(ifNode);

            var forLoop = new ForLoopNode(indexAssign, loopCond, loopStep, loopBody);

            var program = new SequenceNode();
            program.Statements.Add(arrayAssign);
            program.Statements.Add(maxAssign);
            program.Statements.Add(forLoop);
            program.Statements.Add(new IdNode(maxVar));

            PerformSemanticAnalysis(program);

            return VisitSequence(program);
        }

        public LLVMValueRef VisitMean(MeanNode expr)
        {
            var arrayVar = "__mean_array";
            var indexVar = "__mean_i";
            var sumVar = "__mean_sum";
            var meanVar = "__mean_val";

            var arrayAssign = new AssignNode(arrayVar, expr.ArrayExpression);
            var sumAssign = new AssignNode(sumVar, new FloatNode(0));
            var indexAssign = new AssignNode(indexVar, new NumberNode(0));

            var loopCond = new ComparisonNode(
                new IdNode(indexVar),
                "<",
                new LengthNode(new IdNode(arrayVar))
            );

            var loopStep = new IncrementNode(indexVar);
            var currentElement = new IndexNode(new IdNode(arrayVar), new IdNode(indexVar));

            // sum += currentElement
            var addAssign = new AssignNode(sumVar,
                new BinaryOpNode(new IdNode(sumVar), "+", currentElement)
            );

            var loopBody = new SequenceNode();
            loopBody.Statements.Add(addAssign);

            var forLoop = new ForLoopNode(indexAssign, loopCond, loopStep, loopBody);

            // mean = sum / array.length
            var meanAssign = new AssignNode(meanVar,
                new BinaryOpNode(new IdNode(sumVar), "/", new LengthNode(new IdNode(arrayVar)))
            );

            var program = new SequenceNode();
            program.Statements.Add(arrayAssign);
            program.Statements.Add(sumAssign);
            program.Statements.Add(forLoop);
            program.Statements.Add(meanAssign);
            program.Statements.Add(new IdNode(meanVar));

            PerformSemanticAnalysis(program);

            return VisitSequence(program);
        }

        public LLVMValueRef VisitSum(SumNode expr)
        {
            var arrayVar = "__sum_array";
            var indexVar = "__sum_i";
            var sumVar = "__sum_val";

            var arrayAssign = new AssignNode(arrayVar, expr.ArrayExpression);
            var sumAssign = new AssignNode(sumVar, new NumberNode(0));
            var indexAssign = new AssignNode(indexVar, new NumberNode(0));

            var loopCond = new ComparisonNode(
                new IdNode(indexVar),
                "<",
                new LengthNode(new IdNode(arrayVar))
            );

            var loopStep = new IncrementNode(indexVar);
            var currentElement = new IndexNode(new IdNode(arrayVar), new IdNode(indexVar));

            // sum += currentElement
            var addAssign = new AssignNode(sumVar,
                new BinaryOpNode(new IdNode(sumVar), "+", currentElement)
            );

            var loopBody = new SequenceNode();
            loopBody.Statements.Add(addAssign);

            var forLoop = new ForLoopNode(indexAssign, loopCond, loopStep, loopBody);

            var program = new SequenceNode();
            program.Statements.Add(arrayAssign);
            program.Statements.Add(sumAssign);
            program.Statements.Add(forLoop);
            program.Statements.Add(new IdNode(sumVar));

            PerformSemanticAnalysis(program);

            return VisitSequence(program);
        }

        public LLVMValueRef VisitId(IdNode expr)
        {
            var entry = _context.Get(expr.Name);
            if (entry == null) throw new Exception($"Variable {expr.Name} not found in context.");

            var llvmType = GetLLVMType(entry.Type);
            if (_debug) Console.WriteLine($"visiting: variable: {expr.Name} (Type: {llvmType})");

            LLVMValueRef ptrToLoad;

            // 1. Resolve the Pointer
            if (entry.Value.Handle != IntPtr.Zero)
            {
                ptrToLoad = entry.Value;
            }
            else
            {
                var global = _module.GetNamedGlobal(expr.Name);
                if (global.Handle == IntPtr.Zero)
                {
                    global = _module.AddGlobal(llvmType, expr.Name);
                    global.Linkage = LLVMLinkage.LLVMExternalLinkage;
                }
                ptrToLoad = global;
            }

            // 2. Determine Alignment
            // i64 (Int) and double (Float) need 8. Bools need 1. Others (pointers) usually 8 on 64-bit.
            uint alignment = 8;
            if (entry.Type is BoolType) alignment = 1;

            // 3. Perform the Load
            LLVMValueRef loadInstruction;

            if (entry.Type is ArrayType || entry.Type is StringType)
            {
                loadInstruction = _builder.BuildLoad2(
                    LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0),
                    ptrToLoad,
                    expr.Name + "_load"
                );
            }
            else
            {
                if (_debug) Console.WriteLine($"visiting: variable: {expr.Name} (Type: {entry.Type}, Ptr: {ptrToLoad})");
                loadInstruction = _builder.BuildLoad2(llvmType, ptrToLoad, expr.Name + "_load");
            }

            // 4. Set the Alignment explicitly
            loadInstruction.SetAlignment(alignment);

            return loadInstruction;
        }

        public LLVMValueRef VisitSequence(SequenceNode expr)
        {
            LLVMValueRef last = default;

            foreach (var stmt in expr.Statements)
            {
                last = Visit(stmt);
            }

            // Use the semantic type from the AST rather than inferring from LLVM types.
            // This avoids misclassifying strings/arrays (both are i8* in LLVM) and prevents
            // MapLLVMTypeToMyType from throwing for pointer types.
            var lastExpr = GetLastExpression(expr) as ExpressionNode;
            if (lastExpr != null && lastExpr.Type is not BoolType)
            {
                //AddImplicitPrint(last, lastExpr.Type);
            }

            return last;
        }

        private LLVMValueRef Visit(Node expr)
        {
            if (_debug)
            {
                var name = expr.GetType().Name;
                //Console.WriteLine("visiting: " + name.Substring(0, name.Length - 4));
            }
            return expr.Accept(this);
        }

        public LLVMValueRef VisitUnaryOp(UnaryOpNode expr)
        {
            if (expr.Operator == "-") return VisitUnaryMinus(expr);
            return default;
        }

        public LLVMValueRef VisitUnaryMinus(UnaryOpNode expr)
        {
            // Visit the operand (the expression on the right of the '-')
            var ctx = _module.Context;
            var value = Visit(expr.Operand);

            // Negate the value, depending on whether it's a float or an integer
            if (value.TypeOf == ctx.DoubleType)
                return _builder.BuildFNeg(value, "fneg");
            else
                return _builder.BuildNeg(value, "neg");
        }

        public LLVMValueRef VisitIndexAssign(IndexAssignNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            var func = _builder.InsertBlock.Parent;

            // 1. Get Header and Index
            var headerPtr = Visit(expr.ArrayExpression);
            var indexVal = Visit(expr.IndexExpression);

            if (indexVal.TypeOf == ctx.DoubleType)
                indexVal = _builder.BuildFPToSI(indexVal, i64, "idx_int");

            // Define Struct Layout: { i64 len, i64 cap, i8* data }
            var arrayStructType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);

            // 2. Load Metadata for Bounds Check and Access
            var lenFieldPtr = _builder.BuildStructGEP2(arrayStructType, headerPtr, 0, "len_ptr");
            var dataFieldPtr = _builder.BuildStructGEP2(arrayStructType, headerPtr, 2, "data_ptr_ptr");

            var arrayLen = _builder.BuildLoad2(i64, lenFieldPtr, "array_len");
            var dataPtr = _builder.BuildLoad2(i8Ptr, dataFieldPtr, "data_ptr");

            // 3. --- BOUNDS CHECK ---
            var isNegative = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, indexVal, LLVMValueRef.CreateConstInt(i64, 0));
            var isTooBig = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, indexVal, arrayLen);
            var isInvalid = _builder.BuildOr(isNegative, isTooBig, "is_invalid");

            var failBlock = ctx.AppendBasicBlock(func, "assign_bounds.fail");
            var safeBlock = ctx.AppendBasicBlock(func, "assign_bounds.ok");
            _builder.BuildCondBr(isInvalid, failBlock, safeBlock);

            // --- FAIL ---
            _builder.PositionAtEnd(failBlock);
            var errorMsg = _builder.BuildGlobalStringPtr("Runtime Error: Index Out of Bounds!\n", "err_msg");
            _builder.BuildCall2(_printfType, _printf, new[] { errorMsg }, "print_err");
            _builder.BuildRet(LLVMValueRef.CreateConstNull(i8Ptr));

            // --- SAFE ---
            _builder.PositionAtEnd(safeBlock);

            // 4. Stride-Aware Value Preparation
            var valueToAssign = Visit(expr.AssignExpression);

            // Check if it's a bool array to determine stride and storage type
            bool isBool = ((ArrayType)expr.ArrayExpression.Type).ElementType is BoolType;
            var gepType = isBool ? i8 : i8Ptr;

            LLVMValueRef finalValueToStore;
            if (isBool) // Convert i1 to i8 (1 byte)
                finalValueToStore = _builder.BuildZExt(valueToAssign, i8, "bool_to_i8");
            else
                finalValueToStore = _builder.BuildBitCast(valueToAssign, i8Ptr, "val_to_ptr");

            // 5. GEP into the separate Data Buffer (No +2 offset)
            var elementPtr = _builder.BuildGEP2(gepType, dataPtr, new[] { indexVal }, "elem_ptr");

            // 6. Store
            _builder.BuildStore(finalValueToStore, elementPtr);

            return valueToAssign;
        }

        public LLVMValueRef VisitRecord(RecordNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var mallocFunc = GetOrDeclareMalloc();

            var numFields = (ulong)expr.Fields.Count;
            // Each field is a pointer (8 bytes)
            var buffer = _builder.BuildCall2(_mallocType, mallocFunc, new[] { LLVMValueRef.CreateConstInt(i64, numFields * 8) }, "record_buffer");

            for (int i = 0; i < expr.Fields.Count; i++)
            {
                var val = Visit(expr.Fields[i].Value);
                LLVMValueRef fieldPtr;

                // CHECK LLVM TYPE: If it's a raw Int or Double, it MUST be boxed
                bool isPrimitive = val.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind ||
                                   val.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind;

                if (isPrimitive)
                {
                    // Allocate 8 bytes for the primitive value
                    var mem = _builder.BuildCall2(_mallocType, mallocFunc, new[] { LLVMValueRef.CreateConstInt(i64, 8) }, "field_mem");

                    // Cast the malloc'd pointer to the correct type (e.g., i64* or double*) so we can store into it
                    var castPtr = _builder.BuildBitCast(mem, LLVMTypeRef.CreatePointer(val.TypeOf, 0), "cast");
                    _builder.BuildStore(val, castPtr);

                    fieldPtr = mem;
                }
                else
                {
                    // It's already a pointer (String, Array, another Record)
                    fieldPtr = _builder.BuildBitCast(val, i8Ptr, "to_i8ptr");
                }

                // Store the pointer (fieldPtr) into the record buffer
                var index = LLVMValueRef.CreateConstInt(i64, (ulong)i);
                var ptr = _builder.BuildGEP2(i8Ptr, buffer, new[] { index }, "field_ptr");
                _builder.BuildStore(fieldPtr, ptr);
            }

            return buffer;
        }

        public LLVMValueRef VisitRecordFieldAssign(RecordFieldAssignNode expr)
        {
            Console.WriteLine("Visiting record field assign");

            var (fieldPtr, fieldType) = GetFieldPointer(expr.IdRecord, expr.IdField);

            var value = Visit(expr.AssignExpression);

            // Optional: type check (recommended)
            if (value.TypeOf != fieldType)
                throw new Exception($"Cannot assign to record field '{expr.IdField}': type mismatch");

            _builder.BuildStore(value, fieldPtr);

            return value;
        }

        private (LLVMValueRef fieldPtr, LLVMTypeRef fieldType) GetFieldPointer(ExpressionNode recordExpr, string fieldName)
        {
            var recordPtr = Visit(recordExpr);
            var recordType = recordExpr.Type as RecordType;
            if (recordType == null) throw new Exception("Expected record type");

            int fieldIndex = GetFieldIndex(fieldName, recordType.RecordFields);

            // EVERY field in your record buffer is a pointer (i8*)
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);

            // Use GEP to find the N-th pointer in the record buffer
            // This is equivalent to: recordPtr + (fieldIndex * 8)
            var fieldPtr = _builder.BuildGEP2(
                i8Ptr,
                recordPtr,
                new[] { LLVMValueRef.CreateConstInt(_module.Context.Int64Type, (ulong)fieldIndex) },
                $"ptr_{fieldName}"
            );

            // We return i8Ptr because the slot contains a pointer
            return (fieldPtr, i8Ptr);
        }

        private int GetFieldIndex(string name, List<RecordField> Fields)
        {
            for (int i = 0; i < Fields.Count; i++)
            {
                if (Fields[i].Label == name)
                    return i;
            }
            throw new Exception($"Field '{name}' not found.");
        }

        public LLVMValueRef VisitRecordField(RecordFieldNode expr)
        {
            var (fieldSlotPtr, _) = GetFieldPointer(expr.IdRecord, expr.IdField);
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // Load the pointer stored in the record slot
            var storedPtr = _builder.BuildLoad2(i8Ptr, fieldSlotPtr, $"load_{expr.IdField}_ptr");

            if (expr.Type is IntType)
                return _builder.BuildLoad2(ctx.Int64Type, storedPtr, $"val_{expr.IdField}");

            if (expr.Type is FloatType)
                return _builder.BuildLoad2(ctx.DoubleType, storedPtr, $"val_{expr.IdField}");

            if (expr.Type is BoolType)
                return _builder.BuildLoad2(ctx.Int1Type, storedPtr, $"val_{expr.IdField}");

            if (expr.Type is StringType)
            {
                // NO GEP HERE. The storedPtr is already the char* (or the pointer to the string)
                return storedPtr;
            }

            return storedPtr;
        }

        public LLVMValueRef VisitAddField(AddFieldNode expr)
        {
            var oldPtr = Visit(expr.Record);
            var oldType = (RecordType)expr.Record.Type;

            // Build new RecordType + Struct with extra field
            var newFields = oldType.RecordFields.Concat(new[]
                { new RecordField { Label = expr.FieldName, Value = expr.Value } }).ToList();

            var newRecordType = new RecordType(newFields);
            var newStructType = GetOrCreateStructType(newRecordType);
            var newPtr = _builder.BuildMalloc(newStructType, "record_add");

            // Copy old values
            for (int i = 0; i < oldType.RecordFields.Count; i++)
            {
                var oldValPtr = _builder.BuildStructGEP2(GetOrCreateStructType(oldType), oldPtr, (uint)i, "old_ptr");
                var val = _builder.BuildLoad2(GetOrCreateStructType(oldType).StructGetTypeAtIndex((uint)i), oldValPtr, "val");
                var newValPtr = _builder.BuildStructGEP2(newStructType, newPtr, (uint)i, "new_ptr");
                _builder.BuildStore(val, newValPtr);
            }

            // Store new field
            var newFieldPtr = _builder.BuildStructGEP2(newStructType, newPtr, (uint)oldType.RecordFields.Count, expr.FieldName);
            var newValue = Visit(expr.Value);
            _builder.BuildStore(newValue, newFieldPtr);

            return newPtr;
        }

        public LLVMValueRef VisitRemoveField(RemoveFieldNode expr)
        {
            var oldPtr = Visit(expr.Record);
            var oldType = expr.Record.Type as RecordType;

            var newFields = oldType.RecordFields.Where(f => f.Label != expr.FieldName).ToList();
            var newRecordType = new RecordType(newFields);
            var newStructType = GetOrCreateStructType(newRecordType);
            var newPtr = _builder.BuildMalloc(newStructType, "record_remove");

            // Copy remaining fields
            int newIndex = 0;
            for (int i = 0; i < oldType.RecordFields.Count; i++)
            {
                if (oldType.RecordFields[i].Label == expr.FieldName)
                    continue;

                var oldValPtr = _builder.BuildStructGEP2(GetOrCreateStructType(oldType), oldPtr, (uint)i, "old_ptr");
                var val = _builder.BuildLoad2(GetOrCreateStructType(oldType).StructGetTypeAtIndex((uint)i), oldValPtr, "val");
                var newValPtr = _builder.BuildStructGEP2(newStructType, newPtr, (uint)newIndex, "new_ptr");
                _builder.BuildStore(val, newValPtr);
                newIndex++;
            }

            return newPtr;
        }

        public LLVMValueRef VisitToCsv(ToCsvNode expr)
        {
            // 1. Visit the Dataframe expression (e.g., the variable 'df2')
            var dfValue = Visit(expr.Expression);

            // 2. Visit the Path expression (the StringNode)
            var pathValue = Visit(expr.FileNameExpr);

            // 3. Setup the Function Type: void ToCsvInternal(ptr, ptr)
            var voidType = _module.Context.VoidType;
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            var toCsvFnType = LLVMTypeRef.CreateFunction(voidType, new[] { i8Ptr, i8Ptr }, false);

            // 4. Get or Declare the function in the LLVM Module
            var toCsvFn = _module.GetNamedFunction("ToCsvInternal");
            if (toCsvFn.Handle == IntPtr.Zero)
            {
                toCsvFn = _module.AddFunction("ToCsvInternal", toCsvFnType);
            }

            // 5. Build the Call
            // Ensure dfValue is cast to i8Ptr if it isn't already (standard for 'ptr' in modern LLVM)
            var dfCast = _builder.BuildBitCast(dfValue, i8Ptr, "df_cast");

            _builder.BuildCall2(toCsvFnType, toCsvFn, new[] { dfCast, pathValue }, "");

            // 6. Return a "None/Null" RuntimeObject as the expression result
            return GenerateNoneResponse();
        }

        // Helper to return a null/none RuntimeValue { i64 0, ptr null }
        private LLVMValueRef GenerateNoneResponse()
        {
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);

            var runtimeObj = _builder.BuildMalloc(_runtimeValueType, "none_obj");
            var tagPtr = _builder.BuildStructGEP2(_runtimeValueType, runtimeObj, 0, "tag_ptr");
            var dataPtr = _builder.BuildStructGEP2(_runtimeValueType, runtimeObj, 1, "data_ptr");

            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int16Type, 0), tagPtr); // Tag 0 = None
            _builder.BuildStore(LLVMValueRef.CreateConstNull(i8Ptr), dataPtr);

            return runtimeObj;
        }

        public LLVMValueRef VisitReadCsv(ReadCsvNode expr)
        {
            // 1. Visit the path (The CSV string)
            var pathValue = Visit(expr.FileNameExpr);

            // 2. Cast the Schema expression to a RecordNode
            // This fixes the CS1503 conversion errors
            var schemaRecord = (RecordNode)expr.SchemaExpr;

            // 3. Convert Schema AST to "ISIBF" string
            var schemaString = GetSchemaString(schemaRecord);
            var schemaValue = _builder.BuildGlobalStringPtr(schemaString, "csv_schema_code");

            // 4. Call the C# Runtime to get the Rows pointer
            var readCsvFn = GetOrDeclareReadCsvInternal();
            var boxedResult = _builder.BuildCall2(_readCsvInternalType, readCsvFn,
                                new[] { pathValue, schemaValue }, "csv_boxed_res");

            // Unbox the Rows Array Pointer { i64 tag, ptr data } from the RuntimeValue
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            var runtimeValueType = LLVMTypeRef.CreateStruct(new[] { ctx.Int16Type, i8Ptr }, false);

            var dataPtrAddr = _builder.BuildStructGEP2(runtimeValueType, boxedResult, 1, "unbox_ptr");
            var rawRowsPtr = _builder.BuildLoad2(i8Ptr, dataPtrAddr, "raw_rows_ptr");

            // 5. REUSE: Finalize the Dataframe Construction
            // Passing the casted schemaRecord and the pointer we got from C#
            return BuildDataframeInternal(schemaRecord, rawRowsPtr);
        }

        // Move your dataframe construction logic into this helper
        private LLVMValueRef BuildDataframeInternal(RecordNode schema, LLVMValueRef rowsPtr)
        {
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            var dfStructType = LLVMTypeRef.CreateStruct(new[] { i8Ptr, i8Ptr, i8Ptr }, false);

            // Use your existing helpers for these (ensure these methods exist in your class)
            var colNamesArray = GenerateColumnNamesArray(schema);
            var dataTypesArray = GenerateDataTypesArray(schema);

            var dfPtr = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(),
                          new[] { LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 24) }, "df_ptr");

            var c1 = _builder.BuildStructGEP2(dfStructType, dfPtr, 0, "cols");
            var c2 = _builder.BuildStructGEP2(dfStructType, dfPtr, 1, "rows");
            var c3 = _builder.BuildStructGEP2(dfStructType, dfPtr, 2, "types");

            _builder.BuildStore(colNamesArray, c1);
            _builder.BuildStore(rowsPtr, c2); // Use the CSV rows or the Literal rows
            _builder.BuildStore(dataTypesArray, c3);

            return dfPtr;
        }

        private LLVMValueRef GenerateColumnNamesArray(RecordNode schema)
        {
            var i64 = _module.Context.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            int count = schema.Fields.Count;

            // 1. Get function AND type
            var mallocFn = GetOrDeclareMalloc();
            // Now _mallocType is guaranteed to be initialized

            // 2. Allocate Header
            var header = _builder.BuildCall2(_mallocType, mallocFn,
                new[] { LLVMValueRef.CreateConstInt(i64, 24) }, "names_header");

            // 3. Allocate Data
            var dataSize = LLVMValueRef.CreateConstInt(i64, (ulong)(count * 8));
            var data = _builder.BuildCall2(_mallocType, mallocFn,
                new[] { dataSize }, "names_data");

            for (int i = 0; i < count; i++)
            {
                var field = schema.Fields[i];
                var nameStr = _builder.BuildGlobalStringPtr(field.Label, $"col_name_{i}");

                var target = _builder.BuildGEP2(i8Ptr, data,
                    new[] { LLVMValueRef.CreateConstInt(i64, (ulong)i) }, $"ptr_{i}");
                _builder.BuildStore(nameStr, target);
            }

            // Initialize Header { len, cap, data }
            var arrayType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)count), _builder.BuildStructGEP2(arrayType, header, 0, "len"));
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)count), _builder.BuildStructGEP2(arrayType, header, 1, "cap"));
            _builder.BuildStore(data, _builder.BuildStructGEP2(arrayType, header, 2, "data"));

            return header;
        }

        private LLVMValueRef GenerateDataTypesArray(RecordNode schema)
        {
            var i64 = _module.Context.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            int count = schema.Fields.Count;

            var header = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(), new[] { LLVMValueRef.CreateConstInt(i64, 24) }, "types_header");
            var data = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(), new[] { LLVMValueRef.CreateConstInt(i64, (ulong)(count * 8)) }, "types_data");

            for (int i = 0; i < count; i++)
            {
                // Map your types to numbers (match your original IR output: 1, 4, 1, 3, 2)
                int typeId = schema.Fields[i].Type switch
                {
                    IntType => 1,
                    FloatType => 2,
                    BoolType => 3,
                    StringType => 4,
                    _ => 0
                };

                var target = _builder.BuildGEP2(i8Ptr, data, new[] { LLVMValueRef.CreateConstInt(i64, (ulong)i) }, "ptr");
                // bitcast integer to ptr for storage in the pointer array
                var valAsPtr = _builder.BuildIntToPtr(LLVMValueRef.CreateConstInt(i64, (ulong)typeId), i8Ptr);
                _builder.BuildStore(valAsPtr, target);
            }

            // Set len=count, cap=count, data=data
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)count), _builder.BuildStructGEP2(LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false), header, 0, ""));
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)count), _builder.BuildStructGEP2(LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false), header, 1, ""));
            _builder.BuildStore(data, _builder.BuildStructGEP2(LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false), header, 2, ""));

            return header;
        }

        private string GetSchemaString(RecordNode schema)
        {
            var sb = new StringBuilder();
            // Assuming 'Fields' is a list of NamedArgumentNode or similar
            foreach (var field in schema.Fields)
            {
                // Adjust the logic below to match how your AST stores the types
                // Example based on your log: Field index resolved to int
                var type = field.Type;

                if (type is IntType) sb.Append('I');
                else if (type is FloatType) sb.Append('F');
                else if (type is BoolType) sb.Append('B');
                else if (type is StringType) sb.Append('S');
            }
            return sb.ToString();
        }

        private LLVMTypeRef _readCsvInternalType;
        private LLVMValueRef GetOrDeclareReadCsvInternal()
        {
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            // ptr ReadCsvInternal(ptr path, ptr schema)
            _readCsvInternalType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i8Ptr, i8Ptr }, false);

            var fn = _module.GetNamedFunction("ReadCsvInternal");
            if (fn.Handle == IntPtr.Zero)
            {
                fn = _module.AddFunction("ReadCsvInternal", _readCsvInternalType);
            }
            return fn;
        }

        private LLVMTypeRef GetOrCreateDataframeType()
        {
            var ctx = _module.Context;

            var dfType = _module.GetTypeByName("dataframe");
            if (dfType.Handle != IntPtr.Zero)
                return dfType;

            // Get array type FIRST
            var arrayType = GetOrCreateArrayType();
            var arrayPtr = LLVMTypeRef.CreatePointer(arrayType, 0);

            // Create named struct: %dataframe
            dfType = ctx.CreateNamedStruct("dataframe");

            dfType.StructSetBody(new[]
            {
                arrayPtr, // columns
                arrayPtr, // rows
                arrayPtr  // types
            }, false);

            return dfType;
        }

        private LLVMTypeRef GetOrCreateArrayType()
        {
            var ctx = _module.Context;

            var arrayType = _module.GetTypeByName("array");
            if (arrayType.Handle != IntPtr.Zero)
                return arrayType;

            // Create named struct: %array
            arrayType = ctx.CreateNamedStruct("array");

            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            arrayType.StructSetBody(new[]
            {
                i64,   // length
                i64,   // capacity
                i8Ptr  // data
            }, false);

            return arrayType;
        }

        private LLVMTypeRef GetArrayPtrType()
        {
            return LLVMTypeRef.CreatePointer(GetOrCreateArrayType(), 0);
        }

        private LLVMTypeRef GetDataframePtrType()
        {
            return LLVMTypeRef.CreatePointer(GetOrCreateDataframeType(), 0);
        }

        public LLVMValueRef VisitDataframe(DataframeNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 1. Get pointers
            var colsPtr = Visit(expr.Columns);
            var rowsPtr = Visit(expr.Rows);

            // 2. Build datatype array from TYPE (not AST)
            var dfType = expr.Type as DataframeType
                ?? throw new Exception("Expected dataframe type.");

            var datatypeNodes = dfType.DataTypes
                .Select(t => new NumberNode(GetTypeByTag(t)))
                .Cast<ExpressionNode>()
                .ToList();

            var datatypeArray = new ArrayNode(datatypeNodes);
            var dataTypesPtr = Visit(datatypeArray);

            // 3. Struct: { cols, rows, types }
            var dfStructType = GetOrCreateDataframeType();
            var dfPtr = _builder.BuildMalloc(dfStructType, "df");

            _builder.BuildStore(colsPtr, _builder.BuildStructGEP2(dfStructType, dfPtr, 0));
            _builder.BuildStore(rowsPtr, _builder.BuildStructGEP2(dfStructType, dfPtr, 1));
            _builder.BuildStore(dataTypesPtr, _builder.BuildStructGEP2(dfStructType, dfPtr, 2));

            return dfPtr;
        }

        public LLVMValueRef VisitNamedArgument(NamedArgumentNode expr) // this is never called?
        {
            // This node is just a wrapper for a regular expression with a name.
            // The name is used for semantic analysis and doesn't affect codegen directly.
            return Visit(expr.Value);
        }

        public LLVMValueRef VisitShowDataframe(ShowDataframeNode expr)
        {
            var sourceType = expr.Source.Type as DataframeType;
            var resultType = expr.Type as DataframeType;
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var dfStructType = LLVMTypeRef.CreateStruct(new[] { i8Ptr, i8Ptr, i8Ptr }, false);
            var arrayStructType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);

            // 1. Source Data
            var sourceDfPtr = Visit(expr.Source);
            var sourceRowsArrayHeader = _builder.BuildLoad2(i8Ptr, _builder.BuildStructGEP2(dfStructType, sourceDfPtr, 1), "src_rows_ptr");
            var lenPtr = _builder.BuildStructGEP2(arrayStructType, sourceRowsArrayHeader, 0, "len_ptr");
            var rowCount = _builder.BuildLoad2(i64, lenPtr, "row_count");

            // 2. New Metadata (Cols and Types)
            var newColsPtr = Visit(new ArrayNode(resultType.ColumnNames.Select(n => (ExpressionNode)new StringNode(n)).ToList()));
            var datatypeNodes = resultType.DataTypes.Select(t => (ExpressionNode)new NumberNode(GetTypeByTag(t))).ToList();
            var newDataTypesPtr = Visit(new ArrayNode(datatypeNodes));

            // 3. Prepare Result Array
            var resultArrayHeader = AllocateArrayHeader(rowCount);
            var itSlot = _builder.BuildAlloca(i8Ptr, "$row_slot");

            // 4. THE LOOP
            BuildLoop(rowCount, (indexRef) =>
            {
                // Get source record
                var sourceDataPtr = _builder.BuildLoad2(i8Ptr, _builder.BuildStructGEP2(arrayStructType, sourceRowsArrayHeader, 2), "src_data");
                var rowPtrPtr = _builder.BuildGEP2(i8Ptr, sourceDataPtr, new[] { indexRef });
                var oldRecordPtr = _builder.BuildLoad2(i8Ptr, rowPtrPtr, "old_rec");

                _builder.BuildStore(oldRecordPtr, itSlot);

                // Update context so RecordFieldNode knows where to look
                _context = _context.Add("$row", itSlot, null!, sourceType.RowType);
                var rowId = new IdNode("$row");
                rowId.SetType(sourceType.RowType);

                // --- CRITICAL: Initialize list INSIDE the loop ---
                var projectedValues = new List<LLVMValueRef>();

                foreach (var colName in resultType.ColumnNames)
                {
                    var fieldAccess = new RecordFieldNode(rowId, colName);
                    var fType = sourceType.RowType.RecordFields.First(f => f.Label == colName).Type;
                    fieldAccess.SetType(fType);

                    // VisitRecordField now returns unboxed raw values (i64, double, or i8* for strings)
                    LLVMValueRef rawValue = VisitRecordField(fieldAccess);
                    projectedValues.Add(rawValue);
                }

                // --- CRITICAL: Build record INSIDE the loop ---
                // This ensures the malloc(size) is called with (3 * 8) = 24, not 0
                var newRecordPtr = BuildRecordFromValues(resultType.RowType, projectedValues);

                var resultDataPtr = _builder.BuildLoad2(i8Ptr, _builder.BuildStructGEP2(arrayStructType, resultArrayHeader, 2), "res_data");
                var resElemPtr = _builder.BuildGEP2(i8Ptr, resultDataPtr, new[] { indexRef });
                _builder.BuildStore(newRecordPtr, resElemPtr);
            });

            // 5. Final Assembly
            var dfPtr = _builder.BuildMalloc(dfStructType, "df_show");
            _builder.BuildStore(newColsPtr, _builder.BuildStructGEP2(dfStructType, dfPtr, 0));
            _builder.BuildStore(resultArrayHeader, _builder.BuildStructGEP2(dfStructType, dfPtr, 1));
            _builder.BuildStore(newDataTypesPtr, _builder.BuildStructGEP2(dfStructType, dfPtr, 2));

            return dfPtr;
        }

        private LLVMValueRef AllocateArrayHeader(LLVMValueRef count)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var arrayStructType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);

            // Allocate 24-byte header
            var header = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(), new[] { LLVMValueRef.CreateConstInt(i64, 24) }, "arr_header");

            // Allocate data buffer (8 bytes per record pointer)
            var dataSize = _builder.BuildMul(count, LLVMValueRef.CreateConstInt(i64, 8), "data_size");
            var dataPtr = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(), new[] { dataSize }, "arr_data");

            // Initialize header fields
            _builder.BuildStore(count, _builder.BuildStructGEP2(arrayStructType, header, 0)); // length
            _builder.BuildStore(count, _builder.BuildStructGEP2(arrayStructType, header, 1)); // capacity
            _builder.BuildStore(dataPtr, _builder.BuildStructGEP2(arrayStructType, header, 2)); // data ptr

            return header;
        }

        // Add this if BuildLoop is missing
        private void BuildLoop(LLVMValueRef count, Action<LLVMValueRef> body)
        {
            var ctx = _module.Context;
            var parentFunc = _builder.InsertBlock.Parent;
            var loopCond = parentFunc.AppendBasicBlock("loop_cond");
            var loopBody = parentFunc.AppendBasicBlock("loop_body");
            var loopEnd = parentFunc.AppendBasicBlock("loop_end");

            var indexPtr = _builder.BuildAlloca(ctx.Int64Type, "index_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int64Type, 0), indexPtr);
            _builder.BuildBr(loopCond);

            _builder.PositionAtEnd(loopCond);
            var index = _builder.BuildLoad2(ctx.Int64Type, indexPtr, "index");
            var cond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, index, count, "cond");
            _builder.BuildCondBr(cond, loopBody, loopEnd);

            _builder.PositionAtEnd(loopBody);
            body(index); // This runs your RecordField extraction
            var nextIndex = _builder.BuildAdd(index, LLVMValueRef.CreateConstInt(ctx.Int64Type, 1), "next_index");
            _builder.BuildStore(nextIndex, indexPtr);
            _builder.BuildBr(loopCond);

            _builder.PositionAtEnd(loopEnd);
        }

        // Internal helper to build a new record from unboxed values
        private LLVMValueRef BuildRecordFromValues(RecordType type, List<LLVMValueRef> values)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // Allocate the record (array of pointers)
            var recordSize = LLVMValueRef.CreateConstInt(i64, (ulong)(values.Count * 8));
            var recordPtr = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(), new[] { recordSize }, "rec_ptr");

            for (int i = 0; i < values.Count; i++)
            {
                var fieldType = type.RecordFields[i].Type;
                LLVMValueRef pointerToStore;

                if (fieldType is IntType || fieldType is FloatType || fieldType is BoolType)
                {
                    // Primitives need to be stored in their own 8-byte allocation
                    var valMem = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(), new[] { LLVMValueRef.CreateConstInt(i64, 8) }, "val_mem");

                    LLVMTypeRef llvmType = fieldType switch
                    {
                        IntType => ctx.Int64Type,
                        FloatType => ctx.DoubleType,
                        _ => ctx.Int1Type
                    };

                    var cast = _builder.BuildBitCast(valMem, LLVMTypeRef.CreatePointer(llvmType, 0));
                    _builder.BuildStore(values[i], cast);
                    pointerToStore = valMem;
                }
                else
                {
                    // Strings and pointers are already pointers, store directly
                    pointerToStore = _builder.BuildBitCast(values[i], i8Ptr);
                }

                // Store the pointer into the record's slot
                var slotPtr = _builder.BuildGEP2(i8Ptr, recordPtr, new[] { LLVMValueRef.CreateConstInt(i64, (ulong)i) });
                _builder.BuildStore(pointerToStore, slotPtr);
            }
            return recordPtr;
        }

        LLVMValueRef GetArrayData(LLVMValueRef arrayPtr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var arrayStructType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);

            return _builder.BuildLoad2(
                i8Ptr,
                _builder.BuildStructGEP2(arrayStructType, arrayPtr, 2),
                "arr_data"
            );
        }

        LLVMValueRef GetArrayElementRaw(LLVMValueRef arrayPtr, LLVMValueRef index, bool isBool)
        {
            var ctx = _module.Context;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var dataPtr = GetArrayData(arrayPtr);

            if (isBool)
            {
                var elemPtr = _builder.BuildGEP2(i8, dataPtr, new[] { index }, "bool_elem_ptr");
                return _builder.BuildLoad2(i8, elemPtr, "bool_val");
            }
            else
            {
                var elemPtr = _builder.BuildGEP2(i8Ptr, dataPtr, new[] { index }, "ptr_elem_ptr");
                return _builder.BuildLoad2(i8Ptr, elemPtr, "ptr_val");
            }
        }

        public LLVMValueRef VisitDataframeColumnAccess(LLVMValueRef dfPtr, LLVMValueRef colIndex)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var dfType = _module.GetTypeByName("dataframe");

            // load df.data (ArrayObject*)
            var dataArrayPtr = _builder.BuildLoad2(
                i8Ptr,
                _builder.BuildStructGEP2(dfType, dfPtr, 1),
                "df_data"
            );

            // get raw pointer array (i8**)
            var dataBuffer = GetArrayData(dataArrayPtr);

            // index into columns
            var colPtrPtr = _builder.BuildGEP2(i8Ptr, dataBuffer, new[] { colIndex }, "col_ptr_ptr");

            // load column (ArrayObject*)
            var colPtr = _builder.BuildLoad2(i8Ptr, colPtrPtr, "col_ptr");

            return colPtr;
        }

        private List<object> ExtractRecord(IntPtr recordPtr, RecordType recordType)
        {
            var result = new List<object>();

            for (int i = 0; i < recordType.RecordFields.Count; i++)
            {
                var field = recordType.RecordFields[i];
                // Handle both Field objects and direct Types
                var fieldType = field.Value?.Type ?? field.Type;

                // 1. Get the pointer stored at this record offset (index i * 8 bytes)
                IntPtr valuePtr = Marshal.ReadIntPtr(IntPtr.Add(recordPtr, i * 8));

                if (valuePtr == IntPtr.Zero)
                {
                    result.Add("null");
                    continue;
                }

                // 2. Dereference based on the known type
                if (fieldType is IntType)
                    result.Add(Marshal.ReadInt64(valuePtr));
                else if (fieldType is FloatType)
                {
                    byte[] bytes = new byte[8];
                    Marshal.Copy(valuePtr, bytes, 0, 8);
                    result.Add(BitConverter.ToDouble(bytes, 0));
                }
                else if (fieldType is BoolType)
                    result.Add(Marshal.ReadByte(valuePtr) != 0);
                else if (fieldType is StringType)
                    result.Add(Marshal.PtrToStringAnsi(valuePtr)); // Direct pointer to C-String
                else
                    result.Add(valuePtr); // Return raw pointer for complex types
            }
            return result;
        }

        public LLVMValueRef VisitTypeLiteral(TypeLiteralNode expr)
        {
            if (_debug) Console.WriteLine("visiting: TypeLiteral (Skipping CodeGen)");
            // Return a dummy null pointer so LLVM doesn't get a completely empty handle
            return LLVMValueRef.CreateConstNull(LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0));
        }

        /*  Command example of how to construct a dataframe in C# that matches the expected memory layout for your LLVM codegen:

        df = read_csv([index: int, name: string, age: int, hasJob: bool, savings: float], "CSV/mytest.csv")
        to_csv(df, "CSV/mytest.csv")
m
        df2 = dataframe(columns=["name", "age", "hasJob", "savings"],data=[{name:"Bob", age: 23, hasJob: true, savings: 230500.00},{name:"Alice", age: 23, hasJob: true, savings: 100500.55},{name:"John", age: 87, hasJob: false, savings: 1209000.02},{name:"Mary", age: 29, hasJob: false, savings: 10700.25}])         
        df2 = dataframe(["name", "age", "hasJob", "savings"],[{name:"Bob", age: 23, hasJob: true, savings: 230500.00},{name:"Alice", age: 23, hasJob: true, savings: 100500.55},{name:"John", age: 87, hasJob: false, savings: 1209000.02},{name:"Mary", age: 29, hasJob: false, savings: 10700.25}])         
        
        df2 = dataframe(columns=["name", "age", "hasJob", "savings"],data=[{name:"Bob", age: 23, hasJob: true, savings: 230500.00},{name:"Alice", age: 23, hasJob: true, savings: 100500.55},{name:"John", age: 87, hasJob: false, savings: 1209000.02},{name:"Mary", age: 29, hasJob: false, savings: 10700.25}])         
        
            record(["name", "age", "is cool", "rating"], ["Hary potter", 9786, true, 10.5585]) 
        */
    }
}