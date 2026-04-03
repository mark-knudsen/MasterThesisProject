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

namespace MyCompiler
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)] // Pack 8 is standard for 64-bit pointers
    public struct RuntimeValue
    {
        public Int16 tag;
        // The compiler adds 6 bytes of padding here automatically to align the pointer to 8
        public IntPtr data;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ArrayObject
    {
        public long length;    // i64
        public long capacity;  // i64
        public IntPtr data;    // pointer
    }

    [StructLayout(LayoutKind.Sequential)]
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
        public static IntPtr ReadCsvInternal(IntPtr pathPtr, IntPtr schemaPtr) // Added second param
        {
            Console.WriteLine("--- C# Runtime: ReadCsvInternal Started ---");
            string path = Marshal.PtrToStringAnsi(pathPtr);
            Console.WriteLine($"Loading: {path}");
            // schemaDescriptor could be a string like "S,I,F" (String, Int, Float)
            string schema = Marshal.PtrToStringAnsi(schemaPtr);

            var lines = File.ReadAllLines(path).Skip(1).ToArray(); // Skip CSV header
            int rowCount = lines.Length;

            // 1. Allocate the Dataframe Header { ptr columns, ptr rows, ptr name }
            IntPtr dfHeader = Marshal.AllocHGlobal(24);

            // 2. Allocate the Rows Array Header { i64 len, i64 cap, ptr data }
            IntPtr rowsArrayHeader = Marshal.AllocHGlobal(24);
            IntPtr rowsDataBuffer = Marshal.AllocHGlobal(rowCount * 8); // Buffer of pointers

            for (int i = 0; i < rowCount; i++)
            {
                string[] parts = lines[i].Split(',');

                // 3. Create a Record (Buffer of Pointers to Boxed Values)
                // Assume 3 columns for this example: [index, name, age]
                IntPtr recordBuffer = Marshal.AllocHGlobal(parts.Length * 8);

                for (int col = 0; col < parts.Length; col++)
                {
                    // Get the character from the schema string for this column
                    char typeCode = col < schema.Length ? schema[col] : 'S';
                    IntPtr boxedVal = BoxValue(parts[col].Trim(), typeCode);
                    Marshal.WriteIntPtr(recordBuffer, col * 8, boxedVal);
                }

                Marshal.WriteIntPtr(rowsDataBuffer, i * 8, recordBuffer);
            }

            // Link headers
            Marshal.WriteInt64(rowsArrayHeader, 0, rowCount);
            Marshal.WriteIntPtr(rowsArrayHeader, 16, rowsDataBuffer);
            Marshal.WriteIntPtr(dfHeader, 8, rowsArrayHeader); // Store in df.rows slot

            return dfHeader;
        }

        public static void ToCsvInternal(IntPtr arrayHeaderPtr, IntPtr pathPtr)
        {
            if (arrayHeaderPtr == IntPtr.Zero || pathPtr == IntPtr.Zero) return;
            string path = Marshal.PtrToStringAnsi(pathPtr);

            // Ensure ArrayObject is defined in your namespace
            var array = Marshal.PtrToStructure<ArrayObject>(arrayHeaderPtr);

            using var writer = new System.IO.StreamWriter(path);
            for (int i = 0; i < array.length; i++)
            {
                IntPtr elemPtr = IntPtr.Add(array.data, i * 8);
                long rawBits = Marshal.ReadInt64(elemPtr);

                if ((ulong)rawBits > 0x1000000)
                {
                    try
                    {
                        string s = Marshal.PtrToStringAnsi((IntPtr)rawBits);
                        writer.WriteLine(s);
                    }
                    catch
                    {
                        writer.WriteLine(rawBits);
                    }
                }
                else if (rawBits > 1000000)
                {
                    writer.WriteLine(BitConverter.Int64BitsToDouble(rawBits));
                }
                else
                {
                    writer.WriteLine(rawBits);
                }
            }
        }

        private static IntPtr BoxValue(string rawValue, char typeCode)
        {
            // Allocate 8 bytes for the value (LLVM expects i64/ptr size)
            IntPtr valPtr = Marshal.AllocHGlobal(8);

            switch (typeCode)
            {
                case 'I': // Integer
                    long.TryParse(rawValue, out long iVal);
                    Marshal.WriteInt64(valPtr, iVal);
                    break;
                case 'F': // Float/Double
                    double.TryParse(rawValue, out double dVal);
                    long bits = BitConverter.DoubleToInt64Bits(dVal);
                    Marshal.WriteInt64(valPtr, bits);
                    break;
                case 'S': // String
                    IntPtr sPtr = Marshal.StringToHGlobalAnsi(rawValue);
                    // For strings, we store the pointer to the string bytes
                    Marshal.WriteIntPtr(valPtr, sPtr);
                    break;
                case 'B': // Boolean
                    bool.TryParse(rawValue, out bool bVal);
                    Marshal.WriteInt64(valPtr, bVal ? 1 : 0);
                    break;
                default:
                    Marshal.WriteInt64(valPtr, 0);
                    break;
            }
            return valPtr;
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
        //private NodeExpr _lastNode; // Store the last expression for auto-printing
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
            var mallocFunc = _module.GetNamedFunction("malloc");
            if (mallocFunc.Handle != IntPtr.Zero) return mallocFunc;

            // Define: ptr malloc(i64)
            _mallocType = LLVMTypeRef.CreateFunction(
                LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0),
                new[] { _module.Context.Int64Type }
            );

            return _module.AddFunction("malloc", _mallocType);
        }

        private void DeclareValueStruct()
        {
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // Use a Named Struct so it's easy to see in IR
            _runtimeValueType = ctx.CreateNamedStruct("RuntimeValue");
            // Ensure this matches your C# 'public Int16 tag; public IntPtr data;'
            _runtimeValueType.StructSetBody(new[] { ctx.Int64Type, i8Ptr }, false);
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

        private Type PerformSemanticAnalysis(NodeExpr expr)
        {
            var checker = new TypeChecker(_context, _debug);
            _lastType = checker.Check(expr);
            _context = checker.UpdatedContext;

            //_lastNode = GetLastExpression(expr);

            var programedResult = GetProgramResult(expr);

            if (programedResult == null) return new VoidType();
            if (programedResult is ExpressionNodeExpr exp) return exp.Type;

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

        public object Run(NodeExpr expr, bool debug = false)
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

            var array = Marshal.PtrToStructure<ArrayObject>(arrayObjPtr); // we fail right here with dataframe, is it not the correct array pointer?
            var elementType = ((ArrayType)type).ElementType;
            var elements = new List<object>();
            var stride = (elementType is BoolType) ? 1 : 8;

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

        private List<IntPtr> ExtractRecordRaw(IntPtr recordPtr, RecordType recordType)
        {
            var result = new List<IntPtr>();
            int fieldSize = 8;

            for (int i = 0; i < recordType.RecordFields.Count; i++)
            {
                IntPtr fieldLocation = IntPtr.Add(recordPtr, i * fieldSize);
                result.Add(Marshal.ReadIntPtr(fieldLocation));
            }

            return result;
        }

        private string HandleDataframe(IntPtr ptr, DataframeType type)
        {
            if (ptr == IntPtr.Zero) return "dataframe(null)";

            // 1. Use your dedicated struct
            var dfObj = Marshal.PtrToStructure<DataframeObject>(ptr);

            // 2. Extract Columns using existing HandleArray logic (but returning List)
            // Note: We need the column names as strings for the header


            var columnNames = ExtractArray(dfObj.columns, new ArrayType(new StringType()));

            if (columnNames == null) return "Dataframe error: null columns";

            // 3. Load Rows
            // We manually navigate the rows ArrayObject
            var rowsArray = Marshal.PtrToStructure<ArrayObject>(dfObj.rows);
            var colsArray = Marshal.PtrToStructure<ArrayObject>(dfObj.columns);

            var rowsData = new List<List<object>>();

            for (long r = 0; r < rowsArray.length; r++)
            {
                IntPtr recordPtrAddr = IntPtr.Add(rowsArray.data, (int)(r * 8));
                IntPtr recordPtr = Marshal.ReadIntPtr(recordPtrAddr);

                if (recordPtr == IntPtr.Zero)
                {
                    rowsData.Add(Enumerable.Repeat<object>("null", (int)colsArray.length).ToList());
                    continue;
                }
                // Extract using the RowType defined in your DataframeType
                rowsData.Add(ExtractRecord(recordPtr, type.RowType));
            }

            // --- 4. Formatting (Standard Format) ---
            return FormatTable(columnNames, rowsData);
        }


        // dataframe(["name", "age", "score", "isCool"], [["dan", 30, 1.1, true]])
        // dataframe(["name", "age"], [["dan", 30], ["alice", 25]])
        // x = dataframe(["name", "title", "age", "score"], [["dan", "prince", 30, 1.1], ["alice", "princess", 25, 2.2], ["bob", "knight", 40, 0.5], ["charlie", "lord", 22, 958.1], ["harry", "wizard", 52, 924.8]])
        // x = dataframe(["name", "title", "age", "score"], [["dan", "prince", 30, 1.1], ["alice", "princess", 25, 2.2], ["bob", "knight", 40, 0.5], ["charlie", "lord", 22, 958.1]])

        // this one wont work
        // dataframe(["name", "age", "score", "isCool"], [["dan", 30, 1.1, true], ["alice", 25, 2.2, false],["alice", 25, 2.2, false]])

        // public string HandleDataframe2(IntPtr dfPtr, DataframeType dfType) // this one also prints the types and stuff
        // {
        //     if (dfPtr == IntPtr.Zero) return "{ empty dataframe }"; // dataframe(["name", "age", "score", "isCool"], [["dan", 30, 1.1, true], ["alice", 25, 2.2, false],["alice", 25, 2.2, false]])

        //     var df = Marshal.PtrToStructure<DataframeObject>(dfPtr);
        //     var columnDataArray = HandleArray(df.columnData, new ArrayType(new StringType()));
        //     var dataTypes = HandleArray(df.datatypesData, new ArrayType(new StringType()));
        //     var data = HandleArray(df.dataPointersData, new ArrayType(new StringType()));

        //     return $"Dataframe: \n - Columns: {columnDataArray}\n - DataTypes: {dataTypes}\n - Data: {data}";
        // }

        private int GetTypeByTag(Type type)
        {
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
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var mallocFunc = GetOrDeclareMalloc();
            var mallocType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i64 }, false);
            var runtimeValueType = LLVMTypeRef.CreateStruct(new[] { i16, i8Ptr }, false);

            // --- THE SHIELD ---
            // If the type is already a pointer to our RuntimeValue struct, DO NOT BOX AGAIN.    
            var boxTypePtr = LLVMTypeRef.CreatePointer(_runtimeValueType, 0);
            if (value.TypeOf == boxTypePtr)
            {
                return value;
            }

            int tag = GetTypeByTag(type);

            LLVMValueRef dataPtr;

            if (type is IntType || type is FloatType || type is BoolType)
            {
                // allocate raw memory and store value
                int size = type switch
                {
                    IntType => 8,
                    FloatType => 8,
                    BoolType => 1,
                    _ => 8
                };

                var mem = _builder.BuildCall2(mallocType, mallocFunc,
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
            else if (type is StringType || type is RecordType || type is ArrayType || type is DataframeType)
            {
                System.Console.WriteLine("Boxing pointer type: " + type);
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

            // Allocate RuntimeValue
            var objRaw = _builder.BuildCall2(mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, 16) }, "runtime_obj");

            var obj = _builder.BuildBitCast(objRaw, LLVMTypeRef.CreatePointer(runtimeValueType, 0), "runtime_cast");

            // Store tag
            var tagPtr = _builder.BuildStructGEP2(runtimeValueType, obj, 0, "tag_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i16, (ulong)tag), tagPtr);

            // Store data
            var dataFieldPtr = _builder.BuildStructGEP2(runtimeValueType, obj, 1, "data_ptr");
            _builder.BuildStore(dataPtr, dataFieldPtr);

            return objRaw;
        }

        private ExpressionNodeExpr GetProgramResult(NodeExpr expr)
        {
            if (expr is SequenceNodeExpr seq)
            {
                foreach (var node in seq.Statements)
                {
                    if (node is ExpressionNodeExpr exp && !(node is PrintNodeExpr))
                        return exp;
                }

                return null;
            }

            if (expr is ExpressionNodeExpr exp2 && !(expr is PrintNodeExpr))
                return exp2;

            return null;
        }

        private NodeExpr GetLastExpression(NodeExpr expr)
        {
            if (expr is SequenceNodeExpr seq)
            {
                // iterate from last to first
                for (int i = seq.Statements.Count - 1; i >= 0; i--)
                {
                    var last = seq.Statements[i];

                    // Only consider actual value expressions, skip statements like print
                    if (last is ExpressionNodeExpr && !(last is PrintNodeExpr))
                        return last;

                    // If last is a sequence itself, recurse
                    if (last is SequenceNodeExpr nestedSeq)
                    {
                        var nestedLast = GetLastExpression(nestedSeq);
                        if (nestedLast != null) return nestedLast;
                    }
                }
                // No expression found
                return null;
            }

            // Single expression node
            if (expr is ExpressionNodeExpr && !(expr is PrintNodeExpr))
                return expr;

            return null;
        }

        public LLVMValueRef VisitForLoopExpr(ForLoopNodeExpr expr)
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

        public LLVMValueRef VisitForEachLoopExpr(ForEachLoopNodeExpr expr)
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

        public LLVMValueRef VisitIncrementExpr(IncrementNodeExpr expr)
        {
            LLVMValueRef global = _module.GetNamedGlobal(expr.Id);

            // If not in this module, but we know it exists globally
            if (global.Handle == IntPtr.Zero && _definedGlobals.Contains(expr.Id))
            {
                LLVMTypeRef llvmType = (expr.Type is FloatType) ? _module.Context.DoubleType : _module.Context.Int64Type;
                global = _module.AddGlobal(llvmType, expr.Id);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage; // Extern
            }

            if (global.Handle == IntPtr.Zero)
                throw new Exception($"Variable {expr.Id} not found.");

            // Explicit type for LLVM 20
            LLVMTypeRef type = (expr.Type is FloatType) ? _module.Context.DoubleType : _module.Context.Int64Type;

            var oldValue = _builder.BuildLoad2(type, global, "inc_load");

            LLVMValueRef newValue;
            if (expr.Type is IntType)
                newValue = _builder.BuildAdd(oldValue, LLVMValueRef.CreateConstInt(type, 1), "inc_add");
            else
                newValue = _builder.BuildFAdd(oldValue, LLVMValueRef.CreateConstReal(type, 1.0), "inc_add");

            _builder.BuildStore(newValue, global).SetAlignment(8);

            return newValue;
        }

        public LLVMValueRef VisitDecrementExpr(DecrementNodeExpr expr)
        {
            LLVMValueRef global = _module.GetNamedGlobal(expr.Id);

            // If not in this module, but we know it exists globally
            if (global.Handle == IntPtr.Zero && _definedGlobals.Contains(expr.Id))
            {
                LLVMTypeRef llvmType = (expr.Type is FloatType) ? _module.Context.DoubleType : _module.Context.Int64Type;
                global = _module.AddGlobal(llvmType, expr.Id);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage; // Extern
            }

            if (global.Handle == IntPtr.Zero)
                throw new Exception($"Variable {expr.Id} not found.");

            // Explicit type for LLVM 20
            LLVMTypeRef type = (expr.Type is FloatType) ? _module.Context.DoubleType : _module.Context.Int64Type;

            var oldValue = _builder.BuildLoad2(type, global, "dec_load");

            LLVMValueRef newValue;
            if (expr.Type is IntType)
                newValue = _builder.BuildSub(oldValue, LLVMValueRef.CreateConstInt(type, 1), "dec_sub");
            else
                newValue = _builder.BuildFSub(oldValue, LLVMValueRef.CreateConstReal(type, 1.0), "dec_sub");

            _builder.BuildStore(newValue, global).SetAlignment(8);

            return newValue;
        }

        public LLVMValueRef VisitComparisonExpr(ComparisonNodeExpr expr)
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

        public LLVMValueRef VisitIfExpr(IfNodeExpr node)
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

        public LLVMValueRef VisitRoundExpr(RoundNodeExpr expr)
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

        public LLVMValueRef VisitBinaryExpr(BinaryOpNodeExpr expr)
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

        public LLVMValueRef VisitLogicalOpExpr(LogicalOpNodeExpr expr)
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

        public LLVMValueRef VisitStringExpr(StringNodeExpr expr)
        {
            return _builder.BuildGlobalStringPtr(expr.Value, "str");
        }
        public LLVMValueRef VisitNumberExpr(NumberNodeExpr expr)
        {
            return LLVMValueRef.CreateConstInt(_module.Context.Int64Type, (ulong)expr.Value);
        }
        public LLVMValueRef VisitFloatExpr(FloatNodeExpr expr)
        {
            return LLVMValueRef.CreateConstReal(_module.Context.DoubleType, expr.Value);
        }
        public LLVMValueRef VisitBooleanExpr(BooleanNodeExpr expr)
        {
            return LLVMValueRef.CreateConstInt(_module.Context.Int1Type, expr.Value ? 1UL : 0UL);
        }
        public LLVMValueRef VisitNullExpr(NullNodeExpr expr)
        {
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // Return a null pointer of type i8*
            return LLVMValueRef.CreateConstPointerNull(i8Ptr);
        }
        public LLVMValueRef VisitAssignExpr(AssignNodeExpr expr)
        {
            if (_debug) Console.WriteLine($"visiting assignment: {expr.Id}");

            var value = Visit(expr.Expression);
            var storageType = value.TypeOf;
            var module = _module;

            LLVMValueRef global = module.GetNamedGlobal(expr.Id);

            if (global.Handle == IntPtr.Zero)
            {
                if (!_definedGlobals.Contains(expr.Id))
                {
                    // FIRST DEFINITION
                    global = module.AddGlobal(storageType, expr.Id);

                    // Set initial dummy value so it's not "undef"
                    if (storageType.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
                        global.Initializer = LLVMValueRef.CreateConstReal(storageType, 0.0);
                    else if (storageType.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                        global.Initializer = LLVMValueRef.CreateConstInt(storageType, 0);
                    else
                        global.Initializer = LLVMValueRef.CreateConstNull(storageType);

                    // Use ExternalLinkage so it's visible to the JIT symbol table
                    global.Linkage = LLVMLinkage.LLVMExternalLinkage;
                    _definedGlobals.Add(expr.Id);
                }
                else
                {
                    // RE-DECLARATION (in a new module)
                    global = module.AddGlobal(storageType, expr.Id);

                    // IMPORTANT: Setting Linkage to External WITHOUT an Initializer 
                    // makes this an 'extern' declaration.
                    global.Linkage = LLVMLinkage.LLVMExternalLinkage;
                }
            }

            var store = _builder.BuildStore(value, global);
            store.SetAlignment(8);

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

        public LLVMValueRef VisitRandomExpr(RandomNodeExpr expr)
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
                _builder.BuildStore(res1, resultPtr).SetAlignment(4); // it says %res1 = add i32 %mod1, i64 1   // what is the i64 1?

                _builder.BuildBr(mergeBB);

                // "Else" Part (Wrong Order - Swap logic)
                _builder.PositionAtEnd(elseBB);
                var diff2 = _builder.BuildSub(minVal, maxVal, "diff2"); // again this says i64 -99
                var range2 = _builder.BuildAdd(diff2, LLVMValueRef.CreateConstInt(i64, 1), "range2");
                var res2 = _builder.BuildAdd(_builder.BuildSRem(randValue, range2, "mod2"), maxVal, "res2");
                _builder.BuildStore(res2, resultPtr).SetAlignment(4);

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

        public LLVMValueRef VisitPrintExpr(PrintNodeExpr expr)
        {
            var valueToPrint = Visit(expr.Expression);
            return AddImplicitPrint(valueToPrint, expr.Expression.Type);
        }

        public LLVMValueRef VisitWhereExpr(WhereNodeExpr expr)
        {
            // 1 Allocate local variables for source and result
            var srcVarName = "__where_src";
            var resultVarName = "__where_result";
            var indexVarName = "__where_i";

            // Save source array into a temp variable
            var srcAssign = new AssignNodeExpr(srcVarName, expr.SourceExpr);

            // Allocate result array
            var resultAssign = new AssignNodeExpr(resultVarName, new ArrayNodeExpr(new List<ExpressionNodeExpr>()));

            // Initialize loop index
            var indexAssign = new AssignNodeExpr(indexVarName, new NumberNodeExpr(0));

            // Loop condition: i < src.length
            var loopCond = new ComparisonNodeExpr(
                new IdNodeExpr(indexVarName),
                "<",
                new LengthNodeExpr(new IdNodeExpr(srcVarName))
            );

            // Loop step: i++
            var loopStep = new IncrementNodeExpr(indexVarName);

            // Current element: src[i]
            var currentElement = new IndexNodeExpr(new IdNodeExpr(srcVarName), new IdNodeExpr(indexVarName));

            // Rewrite the iterator variable inside the condition
            ExpressionNodeExpr ifCond = ReplaceIterator(expr.Condition, expr.IteratorId.Name, currentElement);

            // Add element to result array if condition is true
            var addNode = new AddNodeExpr(new IdNodeExpr(resultVarName), currentElement);

            var ifBody = new SequenceNodeExpr();
            ifBody.Statements.Add(addNode);
            var ifNode = new IfNodeExpr(ifCond, ifBody);

            // Loop body
            var loopBody = new SequenceNodeExpr();
            loopBody.Statements.Add(ifNode);

            var forLoop = new ForLoopNodeExpr(indexAssign, loopCond, loopStep, loopBody);

            // Sequence
            var program = new SequenceNodeExpr();
            program.Statements.Add(srcAssign);
            program.Statements.Add(resultAssign);
            program.Statements.Add(forLoop);

            // Return the filtered array
            program.Statements.Add(new IdNodeExpr(resultVarName));

            // Perform semantic checks if you have them
            PerformSemanticAnalysis(program);

            return VisitSequenceExpr(program);
        }

        public LLVMValueRef VisitMapExpr(MapNodeExpr expr)
        {
            var srcVarName = "__map_src";
            var resultVarName = "__map_result";
            var indexVarName = "__map_i";

            // 1 Clone source array
            var srcAssign = new AssignNodeExpr(srcVarName, new CopyArrayNodeExpr(expr.SourceExpr));

            // 2 Allocate new array for result (length = src.length)
            var resultAssign = new AssignNodeExpr(resultVarName, new CopyArrayNodeExpr(new IdNodeExpr(srcVarName)));

            // 3 Loop index
            var indexAssign = new AssignNodeExpr(indexVarName, new NumberNodeExpr(0));

            // 4 Loop condition: i < src.length
            var loopCond = new ComparisonNodeExpr(
                new IdNodeExpr(indexVarName),
                "<",
                new LengthNodeExpr(new IdNodeExpr(srcVarName))
            );

            // 5 Loop step: i++
            var loopStep = new IncrementNodeExpr(indexVarName);

            // 6 Current element: src[i]
            var currentElement = new IndexNodeExpr(new IdNodeExpr(srcVarName), new IdNodeExpr(indexVarName));

            // 7 Map expression: replace iterator with current element
            var mapExpr = ReplaceIterator(expr.Assignment, expr.IteratorId.Name, currentElement);

            // 8 Assign mapped value into result array
            var indexAssignNode = new IndexAssignNodeExpr(new IdNodeExpr(resultVarName), new IdNodeExpr(indexVarName), mapExpr);

            // 9 Loop body
            var loopBody = new SequenceNodeExpr();
            loopBody.Statements.Add(indexAssignNode);

            // 10 For-loop
            var forLoop = new ForLoopNodeExpr(indexAssign, loopCond, loopStep, loopBody);

            // 11 Sequence of statements
            var program = new SequenceNodeExpr();
            program.Statements.Add(srcAssign);
            program.Statements.Add(resultAssign);
            program.Statements.Add(forLoop);
            program.Statements.Add(new IdNodeExpr(resultVarName)); // return result

            PerformSemanticAnalysis(program);
            return VisitSequenceExpr(program);
        }

        public LLVMValueRef VisitCopyExpr(CopyNodeExpr expr)
        {
            var value = Visit(expr.Source); // LLVM value
            var type = expr.Source.Type;    // language type

            var elementType = ((ArrayType)expr.Source.Type).ElementType;

            if (type is ArrayType)
                return CopyArray(value, elementType);

            if (type is RecordType t)
                return CopyRecord(value, t);

            throw new Exception($"Cannot call copy on type {type}");
        }

        public LLVMValueRef VisitCopyArrayExpr(CopyArrayNodeExpr expr)
        {
            return VisitCopyExpr(expr);
        }

        public LLVMValueRef VisitCopyRecordExpr(CopyRecordNodeExpr expr)
        {
            return VisitCopyExpr(expr);
        }

        private LLVMValueRef CopyArray(LLVMValueRef srcHeaderPtr, Type elementType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            // Define the Header Layout: { i64 len, i64 cap, i8* data }
            var arrayStructType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);
            var mallocFunc = GetOrDeclareMalloc();

            // 1. Load Length and Data Pointer from Source Header
            var srcLenPtr = _builder.BuildStructGEP2(arrayStructType, srcHeaderPtr, 0, "src_len_ptr");
            var srcDataPtrField = _builder.BuildStructGEP2(arrayStructType, srcHeaderPtr, 2, "src_data_ptr_field");

            var length = _builder.BuildLoad2(i64, srcLenPtr, "length");
            var srcDataPtr = _builder.BuildLoad2(i8Ptr, srcDataPtrField, "src_data_ptr");

            // 2. Allocate NEW Header (24 bytes)
            var newHeaderPtr = _builder.BuildCall2(_mallocType, mallocFunc, new[] { LLVMValueRef.CreateConstInt(i64, 24) }, "new_header");

            // 3. Determine Stride (1 for bool, 8 for others)
            bool isBool = elementType is BoolType;
            var stride = isBool ? 1 : 8;

            // 4. Allocate NEW Data Buffer
            // We allocate exactly what is needed for the current length (or use length for capacity)
            var byteCount = _builder.BuildMul(length, LLVMValueRef.CreateConstInt(i64, (ulong)stride), "byte_count");

            // Ensure we allocate at least something if length is 0
            var isZero = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, length, LLVMValueRef.CreateConstInt(i64, 0));
            var allocSize = _builder.BuildSelect(isZero, LLVMValueRef.CreateConstInt(i64, (ulong)(4 * stride)), byteCount);

            var newDataPtr = _builder.BuildCall2(_mallocType, mallocFunc, new[] { allocSize }, "new_data");

            // 5. Initialize New Header Fields
            var dstLenPtr = _builder.BuildStructGEP2(arrayStructType, newHeaderPtr, 0);
            var dstCapPtr = _builder.BuildStructGEP2(arrayStructType, newHeaderPtr, 1);
            var dstDataPtrField = _builder.BuildStructGEP2(arrayStructType, newHeaderPtr, 2);

            _builder.BuildStore(length, dstLenPtr);
            _builder.BuildStore(length, dstCapPtr); // Capacity matches length on copy
            _builder.BuildStore(newDataPtr, dstDataPtrField);

            // 6. Bulk Copy Data using Memmove/Memcpy (Faster than a loop)
            // memcpy(dest, src, length * stride)
            var memcpyFunc = GetOrDeclareMemmove(); // You can use memmove for safety

            // Only copy if length > 0
            var hasElements = _builder.BuildICmp(LLVMIntPredicate.LLVMIntUGT, length, LLVMValueRef.CreateConstInt(i64, 0));
            var copyBlock = ctx.AppendBasicBlock(_builder.InsertBlock.Parent, "copy.do");
            var endBlock = ctx.AppendBasicBlock(_builder.InsertBlock.Parent, "copy.end");
            _builder.BuildCondBr(hasElements, copyBlock, endBlock);

            _builder.PositionAtEnd(copyBlock);
            _builder.BuildCall2(_memmoveType, memcpyFunc, new[] { newDataPtr, srcDataPtr, byteCount }, "");
            _builder.BuildBr(endBlock);

            _builder.PositionAtEnd(endBlock);
            return newHeaderPtr;
        }

        public LLVMValueRef VisitMapExprMutating(MapNodeExpr expr) // not in use, maybe use with like an argument, eg: x.map(d => d+2, true) 
        {
            // Temp variable names
            var srcVarName = "__map_src";
            var indexVarName = "__map_i";

            // 1. Store source array
            var srcAssign = new AssignNodeExpr(srcVarName, expr.SourceExpr);

            // 2. i = 0
            var indexAssign = new AssignNodeExpr(indexVarName, new NumberNodeExpr(0));

            // 3. Loop condition: i < src.length
            var loopCond = new ComparisonNodeExpr(
                new IdNodeExpr(indexVarName),
                "<",
                new LengthNodeExpr(new IdNodeExpr(srcVarName))
            );

            // x=[1,2,3]
            // newX = x;
            // for (i=0; i<x.length; i++)
            // newX[i] = newX[i] + 2

            // 4. i++
            var loopStep = new IncrementNodeExpr(indexVarName);

            // 5. src[i]
            var currentElement = new IndexNodeExpr(
                new IdNodeExpr(srcVarName),
                new IdNodeExpr(indexVarName)
            );

            // 6. Replace iterator (d => ...) with actual element
            var mappedExpr = ReplaceIterator(
                expr.Assignment,
                expr.IteratorId.Name,
                currentElement
            );

            // 7. src[i] = mappedExpr
            var indexAssignNode = new IndexAssignNodeExpr(
                new IdNodeExpr(srcVarName),
                new IdNodeExpr(indexVarName),
                mappedExpr
            );

            // 8. Loop body
            var loopBody = new SequenceNodeExpr();
            loopBody.Statements.Add(indexAssignNode);

            var forLoop = new ForLoopNodeExpr(
                indexAssign,
                loopCond,
                loopStep,
                loopBody
            );

            // 9. Full sequence
            var program = new SequenceNodeExpr();
            program.Statements.Add(srcAssign);
            program.Statements.Add(forLoop);

            // Return the modified array
            program.Statements.Add(new IdNodeExpr(srcVarName));

            // Optional semantic check
            PerformSemanticAnalysis(program);

            return VisitSequenceExpr(program);
        }

        public ExpressionNodeExpr ReplaceIterator(ExpressionNodeExpr node, string iteratorName, ExpressionNodeExpr replacement)
        {
            if (node is IdNodeExpr id && id.Name == iteratorName)
            {
                return replacement;
            }

            if (node is ComparisonNodeExpr comp)
            {
                comp.Left = ReplaceIterator(comp.Left, iteratorName, replacement);
                comp.Right = ReplaceIterator(comp.Right, iteratorName, replacement);
            }
            else if (node is LogicalOpNodeExpr logical)
            {
                // Added Logical op
                logical.Left = ReplaceIterator(logical.Left, iteratorName, replacement);
                logical.Right = ReplaceIterator(logical.Right, iteratorName, replacement);
            }
            else if (node is BinaryOpNodeExpr bin)
            {
                bin.Left = ReplaceIterator(bin.Left, iteratorName, replacement);
                bin.Right = ReplaceIterator(bin.Right, iteratorName, replacement);
            }
            else if (node is UnaryOpNodeExpr un)
            {
                ReplaceIterator(un.Operand, iteratorName, replacement);
            }
            else if (node is IndexNodeExpr idx)
            {
                ReplaceIterator(idx.SourceExpression, iteratorName, replacement);
                ReplaceIterator(idx.IndexExpression, iteratorName, replacement);
            }

            return node;
        }

        public LLVMValueRef VisitArrayExpr(ArrayNodeExpr expr)
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
            var capacity = count > 0 ? count : 4;
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

        public LLVMValueRef VisitIndexExpr(IndexNodeExpr expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type; //  x=dataframe(["name", "age"], [{name: "dan", age: 30}, {name: "alice", age: 25}])
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            var func = _builder.InsertBlock.Parent;

            // 1. Visit Array (Header Pointer) and Index
            var headerPtr = Visit(expr.SourceExpression);
            var indexVal = Visit(expr.IndexExpression);

            var name = (expr.SourceExpression as IdNodeExpr).Name;
            var sourceType = _context.Get(name).Type;

            Console.WriteLine("semantic type of array being indexed: " + sourceType);

            if (sourceType is ArrayType arrType)
            {
                Console.WriteLine("Indexing array");

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
            else if (sourceType is DataframeType dfType)
            {
                var val = HandleDataframeIndexing(headerPtr, indexVal, expr);

                return _builder.BuildBitCast(val, i8Ptr);
            }

            return default;
        }

        private LLVMValueRef HandleDataframeIndexing(
            LLVMValueRef dataframePtr,
            LLVMValueRef indexPtr,
            IndexNodeExpr expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            var i8PtrPtr = LLVMTypeRef.CreatePointer(i8Ptr, 0); // i8**

            Console.WriteLine("Indexing dataframe (NO RUNTIME WRAP)");

            // --- Load rows pointer ---
            var dfType = _module.GetTypeByName("dataframe");
            if (dfType.Handle == IntPtr.Zero)
            {
                dfType = _module.Context.CreateNamedStruct("dataframe");
                dfType.StructSetBody(new[] { i8Ptr, i8Ptr, i8Ptr }, false);
            }

            var rowsPtrPtr = _builder.BuildStructGEP2(dfType, dataframePtr, 1, "rows_ptr_ptr");
            var rowsPtr = _builder.BuildLoad2(i8Ptr, rowsPtrPtr, "rows_ptr");

            // ArrayObject = { i64 length, i64 capacity, i8* data }
            var arrayStructType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);
            var arrayPtrType = LLVMTypeRef.CreatePointer(arrayStructType, 0);
            var rowsHeaderPtr = _builder.BuildBitCast(rowsPtr, arrayPtrType, "rows_header_ptr");

            var lengthPtr = _builder.BuildStructGEP2(arrayStructType, rowsHeaderPtr, 0, "len_ptr");
            var length = _builder.BuildLoad2(i64, lengthPtr, "len");

            var dataPtrPtr = _builder.BuildStructGEP2(arrayStructType, rowsHeaderPtr, 2, "data_ptr_ptr");
            var dataPtr = _builder.BuildLoad2(i8Ptr, dataPtrPtr, "data_ptr");

            // 🔥 CRITICAL: cast to i8**
            var typedDataPtr = _builder.BuildBitCast(dataPtr, i8PtrPtr, "typed_data");

            // --- Blocks ---
            var function = _builder.InsertBlock.Parent;

            var entryBlock = _builder.InsertBlock;
            var loopBlock = ctx.AppendBasicBlock(function, "loop");
            var bodyBlock = ctx.AppendBasicBlock(function, "loop_body");
            var continueBlock = ctx.AppendBasicBlock(function, "continue");
            var foundBlock = ctx.AppendBasicBlock(function, "found");
            var endBlock = ctx.AppendBasicBlock(function, "end");

            // result = null
            var resultAlloca = _builder.BuildAlloca(i8Ptr, "result");
            _builder.BuildStore(LLVMValueRef.CreateConstNull(i8Ptr), resultAlloca);

            _builder.BuildBr(loopBlock);

            // --- LOOP ---
            _builder.PositionAtEnd(loopBlock);

            var indexPhi = _builder.BuildPhi(i64, "i");
            indexPhi.AddIncoming(
                new[] { LLVMValueRef.CreateConstInt(i64, 0) },
                new[] { entryBlock },
                1
            );

            var cond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntULT, indexPhi, length, "cond");
            _builder.BuildCondBr(cond, bodyBlock, endBlock);

            // --- BODY ---
            _builder.PositionAtEnd(bodyBlock);

            // rows[i]
            var elemPtr = _builder.BuildGEP2(i8Ptr, typedDataPtr, new[] { indexPhi }, "elem_ptr");
            var recordPtr = _builder.BuildLoad2(i8Ptr, elemPtr, "record");

            // 🔥 Cast record → i8**
            var typedRecord = _builder.BuildBitCast(recordPtr, i8PtrPtr, "typed_record");

            // record[0]
            var zero = LLVMValueRef.CreateConstInt(i64, 0);
            var fieldPtr = _builder.BuildGEP2(i8Ptr, typedRecord, new[] { zero }, "field0_ptr");
            var fieldValPtr = _builder.BuildLoad2(i8Ptr, fieldPtr, "field0");

            // load index (i64)
            var i64Ptr = LLVMTypeRef.CreatePointer(i64, 0);
            var castPtr = _builder.BuildBitCast(fieldValPtr, i64Ptr, "cast");
            var fieldVal = _builder.BuildLoad2(i64, castPtr, "index_val");
            fieldVal.SetAlignment(8);

            // compare
            var cmp = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, fieldVal, indexPtr, "cmp");
            _builder.BuildCondBr(cmp, foundBlock, continueBlock);

            // --- FOUND ---
            _builder.PositionAtEnd(foundBlock);
            _builder.BuildStore(recordPtr, resultAlloca);
            _builder.BuildBr(endBlock);

            // --- CONTINUE ---
            _builder.PositionAtEnd(continueBlock);

            var next = _builder.BuildAdd(indexPhi, LLVMValueRef.CreateConstInt(i64, 1), "next");

            indexPhi.AddIncoming(
                new[] { next },
                new[] { continueBlock },
                1
            );

            _builder.BuildBr(loopBlock);

            // --- END ---
            _builder.PositionAtEnd(endBlock);

            var result = _builder.BuildLoad2(i8Ptr, resultAlloca, "result_val");

            // 🚀 RETURN RAW POINTER (no wrapping!)
            return result;
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

        private Type MapLLVMTypeToMyType(LLVMTypeRef llvmType)
        {
            if (llvmType.Equals(_module.Context.Int64Type))
                return new IntType();
            if (llvmType.Equals(_module.Context.Int32Type))
                return new IntType();

            if (llvmType.Equals(_module.Context.DoubleType))
                return new FloatType();

            if (llvmType.Equals(_module.Context.Int1Type)) // boolean type
                return new BoolType();

            // LLVM uses i8* for both C-strings and our array representation.
            // Use this mapping mainly for string literal cases; array cases should be handled
            // based on semantic types instead of relying on LLVM type introspection.
            if (llvmType.Kind == LLVMTypeKind.LLVMPointerTypeKind &&
                llvmType.ElementType.Equals(_module.Context.Int8Type))
                return new StringType();

            // Default to None to avoid throwing for pointer/complex types we don't handle.
            return new VoidType();
        }

        public LLVMValueRef VisitAddExpr(AddNodeExpr expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            var arrayStructType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);

            var headerPtr = Visit(expr.ArrayExpression);
            var valueToAdd = Visit(expr.AddExpression);

            // 1. Determine Type and Stride
            bool isBool = expr.AddExpression.Type is BoolType;
            var elementType = isBool ? i8 : i8Ptr; // i8 for bool, i8* for others

            // 2. Prepare the value to be stored
            LLVMValueRef valueToStore;
            if (isBool) // Truncate/Extend to i8
                valueToStore = _builder.BuildZExt(valueToAdd, i8, "bool_to_i8");
            else
                valueToStore = _builder.BuildBitCast(valueToAdd, i8Ptr, "val_to_ptr");

            // 3. Load Header Metadata
            var lenFieldPtr = _builder.BuildStructGEP2(arrayStructType, headerPtr, 0, "len_ptr");
            var capFieldPtr = _builder.BuildStructGEP2(arrayStructType, headerPtr, 1, "cap_ptr");
            var dataFieldPtr = _builder.BuildStructGEP2(arrayStructType, headerPtr, 2, "data_ptr_ptr");

            var length = _builder.BuildLoad2(i64, lenFieldPtr, "len");
            var capacity = _builder.BuildLoad2(i64, capFieldPtr, "cap");
            var dataPtr = _builder.BuildLoad2(i8Ptr, dataFieldPtr, "data_ptr");

            // 4. Growth Logic (Stride aware)
            var isFull = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, length, capacity, "is_full");
            var function = _builder.InsertBlock.Parent;
            var growBlock = ctx.AppendBasicBlock(function, "grow");
            var contBlock = ctx.AppendBasicBlock(function, "add_cont");
            var startBlock = _builder.InsertBlock;

            _builder.BuildCondBr(isFull, growBlock, contBlock);

            _builder.PositionAtEnd(growBlock);
            var newCap = _builder.BuildSelect(
                _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, capacity, LLVMValueRef.CreateConstInt(i64, 0)),
                LLVMValueRef.CreateConstInt(i64, 4),
                _builder.BuildMul(capacity, LLVMValueRef.CreateConstInt(i64, 2)),
                "new_cap");

            // Calculate bytes: newCap * (1 or 8)
            var strideVal = LLVMValueRef.CreateConstInt(i64, (ulong)(isBool ? 1 : 8));
            var newByteSize = _builder.BuildMul(newCap, strideVal, "new_byte_size");

            var reallocFunc = GetOrDeclareRealloc();
            var newDataPtr = _builder.BuildCall2(_reallocType, reallocFunc, new[] { dataPtr, newByteSize }, "realloc_ptr");

            _builder.BuildStore(newCap, capFieldPtr);
            _builder.BuildStore(newDataPtr, dataFieldPtr);
            _builder.BuildBr(contBlock);

            // 5. Final Store
            _builder.PositionAtEnd(contBlock);
            var finalDataPtr = _builder.BuildPhi(i8Ptr, "final_data_ptr");
            finalDataPtr.AddIncoming(new[] { dataPtr, newDataPtr }, new[] { startBlock, growBlock }, 2);

            // GEP uses elementType to determine jump size automatically
            var targetPtr = _builder.BuildGEP2(elementType, finalDataPtr, new[] { length }, "target_ptr");
            _builder.BuildStore(valueToStore, targetPtr);

            _builder.BuildStore(_builder.BuildAdd(length, LLVMValueRef.CreateConstInt(i64, 1)), lenFieldPtr);

            return headerPtr;
        }

        public LLVMValueRef VisitAddRangeExpr(AddRangeNodeExpr expr)
        {
            Visit(expr.ArrayExpression);
            Visit(expr.AddRangeExpression); // visits the array we are on, like x.addRange, then we visit x on this line of code

            var fullSequence = new SequenceNodeExpr();

            foreach (var item in ((ArrayNodeExpr)expr.AddRangeExpression).Elements)
            {
                var d = new AddNodeExpr(expr.ArrayExpression, item);
                fullSequence.Statements.Add(d);
            }
            var newRange = Visit(fullSequence);

            return newRange;
        }
        public LLVMValueRef VisitRemoveExpr(RemoveNodeExpr expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            // Header: { i64 len, i64 cap, i8* data }
            var arrayStructType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);

            var headerPtr = Visit(expr.ArrayExpression);
            var indexVal = Visit(expr.RemoveExpression);

            // 1. Determine if this is a packed (boolean) array
            bool isBool = ((ArrayType)expr.ArrayExpression.Type).ElementType is BoolType;

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

        public LLVMValueRef VisitRemoveRangeExpr(RemoveRangeNodeExpr expr)
        {
            Visit(expr.ArrayExpression);
            Visit(expr.RemoveRangeExpression); // visits the array we are on, like x.addRange, then we visit x on this line of code

            var fullSequence = new SequenceNodeExpr();

            var elements = ((ArrayNodeExpr)expr.RemoveRangeExpression).Elements;

            // iterate backwards
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                var item = elements[i];
                var removeElement = new RemoveNodeExpr(expr.ArrayExpression, item);
                fullSequence.Statements.Add(removeElement);
            }

            return Visit(fullSequence);
        }

        public LLVMValueRef VisitLengthExpr(LengthNodeExpr expr)
        {
            var ctx = _module.Context;
            var arrayPtr = Visit(expr.ArrayExpression); // LLVMValueRef pointing to the array struct

            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var arrayStructType = LLVMTypeRef.CreateStruct(
                new[] { ctx.Int64Type, ctx.Int64Type, i8Ptr }, false
            );

            var lenPtr = _builder.BuildStructGEP2(arrayStructType, arrayPtr, 0);
            var length = _builder.BuildLoad2(ctx.Int64Type, lenPtr, "length");  // why does this work, should we not go into the struct to retrieve the value?
            length.SetAlignment(8); // make sure alignment matches allocation

            return length;
        }

        private LLVMValueRef GetArrayDataPtr(LLVMValueRef arrayPtr)
        {
            var i64 = _module.Context.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            var dataPtrGEP = _builder.BuildGEP2(i64, arrayPtr, new[] { LLVMValueRef.CreateConstInt(i64, 2) }, "data_ptr");
            return _builder.BuildLoad2(i8Ptr, dataPtrGEP, "data_ptr");  // Pointer to array data
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

        public LLVMValueRef VisitMinExpr(MinNodeExpr expr)
        {
            var arrayVar = "__min_array";
            var indexVar = "__min_i";
            var minVar = "__min_val";

            // Save array to temp variable
            var arrayAssign = new AssignNodeExpr(arrayVar, expr.ArrayExpression);

            // Initialize loop index i = 1
            var indexAssign = new AssignNodeExpr(indexVar, new NumberNodeExpr(1));

            // Initialize min value = array[0]
            var firstElement = new IndexNodeExpr(new IdNodeExpr(arrayVar), new NumberNodeExpr(0));
            var minAssign = new AssignNodeExpr(minVar, firstElement);

            // Loop condition: i < array.length
            var loopCond = new ComparisonNodeExpr(
                new IdNodeExpr(indexVar),
                "<",
                new LengthNodeExpr(new IdNodeExpr(arrayVar))
            );

            // Loop step: i++
            var loopStep = new IncrementNodeExpr(indexVar);

            // Current element: array[i]
            var currentElement = new IndexNodeExpr(new IdNodeExpr(arrayVar), new IdNodeExpr(indexVar));

            // Update min if current element < min
            var ifCond = new ComparisonNodeExpr(currentElement, "<", new IdNodeExpr(minVar));
            var ifBody = new SequenceNodeExpr();
            ifBody.Statements.Add(new AssignNodeExpr(minVar, currentElement));
            var ifNode = new IfNodeExpr(ifCond, ifBody);

            // Loop body
            var loopBody = new SequenceNodeExpr();
            loopBody.Statements.Add(ifNode);

            // Build for loop
            var forLoop = new ForLoopNodeExpr(indexAssign, loopCond, loopStep, loopBody);

            // Sequence: array assign, min assign, loop
            var program = new SequenceNodeExpr();
            program.Statements.Add(arrayAssign);
            program.Statements.Add(minAssign);
            program.Statements.Add(forLoop);

            // Return min value
            program.Statements.Add(new IdNodeExpr(minVar));

            // Semantic analysis (optional)
            PerformSemanticAnalysis(program);

            // Visit the AST to generate LLVM IR
            return VisitSequenceExpr(program);
        }

        public LLVMValueRef VisitMaxExpr(MaxNodeExpr expr)
        {
            var arrayVar = "__max_array";
            var indexVar = "__max_i";
            var maxVar = "__max_val";

            var arrayAssign = new AssignNodeExpr(arrayVar, expr.ArrayExpression);
            var firstElement = new IndexNodeExpr(new IdNodeExpr(arrayVar), new NumberNodeExpr(0));
            var maxAssign = new AssignNodeExpr(maxVar, firstElement);
            var indexAssign = new AssignNodeExpr(indexVar, new NumberNodeExpr(1));

            var loopCond = new ComparisonNodeExpr(
                new IdNodeExpr(indexVar),
                "<",
                new LengthNodeExpr(new IdNodeExpr(arrayVar))
            );

            var loopStep = new IncrementNodeExpr(indexVar);
            var currentElement = new IndexNodeExpr(new IdNodeExpr(arrayVar), new IdNodeExpr(indexVar));

            // Update max if current element > max
            var ifCond = new ComparisonNodeExpr(currentElement, ">", new IdNodeExpr(maxVar));
            var ifBody = new SequenceNodeExpr();
            ifBody.Statements.Add(new AssignNodeExpr(maxVar, currentElement));
            var ifNode = new IfNodeExpr(ifCond, ifBody);

            var loopBody = new SequenceNodeExpr();
            loopBody.Statements.Add(ifNode);

            var forLoop = new ForLoopNodeExpr(indexAssign, loopCond, loopStep, loopBody);

            var program = new SequenceNodeExpr();
            program.Statements.Add(arrayAssign);
            program.Statements.Add(maxAssign);
            program.Statements.Add(forLoop);
            program.Statements.Add(new IdNodeExpr(maxVar));

            PerformSemanticAnalysis(program);

            return VisitSequenceExpr(program);
        }

        public LLVMValueRef VisitMeanExpr(MeanNodeExpr expr)
        {
            var arrayVar = "__mean_array";
            var indexVar = "__mean_i";
            var sumVar = "__mean_sum";
            var meanVar = "__mean_val";

            var arrayAssign = new AssignNodeExpr(arrayVar, expr.ArrayExpression);
            var sumAssign = new AssignNodeExpr(sumVar, new FloatNodeExpr(0));
            var indexAssign = new AssignNodeExpr(indexVar, new NumberNodeExpr(0));

            var loopCond = new ComparisonNodeExpr(
                new IdNodeExpr(indexVar),
                "<",
                new LengthNodeExpr(new IdNodeExpr(arrayVar))
            );

            var loopStep = new IncrementNodeExpr(indexVar);
            var currentElement = new IndexNodeExpr(new IdNodeExpr(arrayVar), new IdNodeExpr(indexVar));

            // sum += currentElement
            var addAssign = new AssignNodeExpr(sumVar,
                new BinaryOpNodeExpr(new IdNodeExpr(sumVar), "+", currentElement)
            );

            var loopBody = new SequenceNodeExpr();
            loopBody.Statements.Add(addAssign);

            var forLoop = new ForLoopNodeExpr(indexAssign, loopCond, loopStep, loopBody);

            // mean = sum / array.length
            var meanAssign = new AssignNodeExpr(meanVar,
                new BinaryOpNodeExpr(new IdNodeExpr(sumVar), "/", new LengthNodeExpr(new IdNodeExpr(arrayVar)))
            );

            var program = new SequenceNodeExpr();
            program.Statements.Add(arrayAssign);
            program.Statements.Add(sumAssign);
            program.Statements.Add(forLoop);
            program.Statements.Add(meanAssign);
            program.Statements.Add(new IdNodeExpr(meanVar));

            PerformSemanticAnalysis(program);

            return VisitSequenceExpr(program);
        }

        public LLVMValueRef VisitSumExpr(SumNodeExpr expr)
        {
            var arrayVar = "__sum_array";
            var indexVar = "__sum_i";
            var sumVar = "__sum_val";

            var arrayAssign = new AssignNodeExpr(arrayVar, expr.ArrayExpression);
            var sumAssign = new AssignNodeExpr(sumVar, new NumberNodeExpr(0));
            var indexAssign = new AssignNodeExpr(indexVar, new NumberNodeExpr(0));

            var loopCond = new ComparisonNodeExpr(
                new IdNodeExpr(indexVar),
                "<",
                new LengthNodeExpr(new IdNodeExpr(arrayVar))
            );

            var loopStep = new IncrementNodeExpr(indexVar);
            var currentElement = new IndexNodeExpr(new IdNodeExpr(arrayVar), new IdNodeExpr(indexVar));

            // sum += currentElement
            var addAssign = new AssignNodeExpr(sumVar,
                new BinaryOpNodeExpr(new IdNodeExpr(sumVar), "+", currentElement)
            );

            var loopBody = new SequenceNodeExpr();
            loopBody.Statements.Add(addAssign);

            var forLoop = new ForLoopNodeExpr(indexAssign, loopCond, loopStep, loopBody);

            var program = new SequenceNodeExpr();
            program.Statements.Add(arrayAssign);
            program.Statements.Add(sumAssign);
            program.Statements.Add(forLoop);
            program.Statements.Add(new IdNodeExpr(sumVar));

            PerformSemanticAnalysis(program);

            return VisitSequenceExpr(program);
        }

        public LLVMValueRef VisitIdExpr(IdNodeExpr expr)
        {
            var entry = _context.Get(expr.Name);
            if (entry == null) throw new Exception($"Variable {expr.Name} not found in context.");

            var llvmType = GetLLVMType(entry.Type);
            if (_debug) Console.WriteLine($"visiting: variable: {expr.Name} (Type: {llvmType})");

            LLVMValueRef ptrToLoad;

            // Check if the variable exists in the current context
            if (entry.Value.Handle != IntPtr.Zero)
            {
                // Use the pointer stored in context (stack-local or previously allocated global)
                ptrToLoad = entry.Value;
            }
            else
            {
                // Otherwise, treat it as a REPL/global variable
                var global = _module.GetNamedGlobal(expr.Name);

                if (global.Handle == IntPtr.Zero)
                {
                    // Allocate global in this module (not external)
                    global = _module.AddGlobal(llvmType, expr.Name);
                    global.Linkage = LLVMLinkage.LLVMExternalLinkage;

                    // Optional: initialize
                    //_builder.BuildStore(LLVMValueRef.CreateConstInt(llvmType, 0), global);
                }

                ptrToLoad = global;
            }

            if (entry.Type is ArrayType || entry.Type is StringType)
            {
                return _builder.BuildLoad2(
                    LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0),
                    ptrToLoad,
                    expr.Name + "_load"
                );
            }

            if (_debug) Console.WriteLine($"visiting: variable: {expr.Name} (Type: {entry.Type}, Ptr: {ptrToLoad})");

            // Load the value from the pointer
            //return ptrToLoad;
            return _builder.BuildLoad2(llvmType, ptrToLoad, expr.Name + "_load");
        }

        public LLVMValueRef VisitSequenceExpr(SequenceNodeExpr expr)
        {
            LLVMValueRef last = default;

            foreach (var stmt in expr.Statements)
            {
                last = Visit(stmt);
            }

            // Use the semantic type from the AST rather than inferring from LLVM types.
            // This avoids misclassifying strings/arrays (both are i8* in LLVM) and prevents
            // MapLLVMTypeToMyType from throwing for pointer types.
            var lastExpr = GetLastExpression(expr) as ExpressionNodeExpr;
            if (lastExpr != null && lastExpr.Type is not BoolType)
            {
                //AddImplicitPrint(last, lastExpr.Type);
            }

            return last;
        }

        private LLVMValueRef Visit(NodeExpr expr)
        {
            if (_debug)
            {
                var name = expr.GetType().Name;
                Console.WriteLine("visiting: " + name.Substring(0, name.Length - 8));
            }
            return expr.Accept(this);
        }

        public LLVMValueRef VisitUnaryOpExpr(UnaryOpNodeExpr expr)
        {
            if (expr.Operator == "-") return VisitUnaryMinus(expr);
            return default;
        }

        public LLVMValueRef VisitUnaryMinus(UnaryOpNodeExpr expr)
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

        public LLVMValueRef VisitIndexAssignExpr(IndexAssignNodeExpr expr)
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

        public LLVMValueRef VisitRecordExpr(RecordNodeExpr expr)
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

        public LLVMValueRef VisitRecordFieldAssignExpr(RecordFieldAssignNodeExpr expr)
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

        private (LLVMValueRef fieldPtr, LLVMTypeRef fieldType) GetFieldPointer(ExpressionNodeExpr recordExpr, string fieldName)
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

        public LLVMValueRef VisitRecordFieldExpr(RecordFieldNodeExpr expr)
        {
            // 1. Get the address of the pointer in the record buffer
            // Your IR already does this correctly: %ptr_name = getelementptr ptr, ptr %result_val, i64 1
            var (fieldSlotPtr, _) = GetFieldPointer(expr.IdRecord, expr.IdField);

            // 2. Load the pointer stored in that slot
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            var storedPtr = _builder.BuildLoad2(i8Ptr, fieldSlotPtr, $"load_ptr_{expr.IdField}");

            // 3. GET THE ACTUAL TYPE (from the Record definition)
            var recordType = expr.IdRecord.Type as RecordType;
            var fieldDef = recordType.RecordFields.Find(f => f.Label == expr.IdField);
            var actualType = fieldDef.Value?.Type ?? fieldDef.Type;

            // 4. CONDITIONAL UNBOXING
            if (actualType is IntType)
            {
                // For ints, we need to load the 64-bit value FROM the pointer
                return _builder.BuildLoad2(_module.Context.Int64Type, storedPtr, $"val_{expr.IdField}");
            }

            if (actualType is StringType)
            {
                // For strings, the pointer IS the value. Just return it.
                return storedPtr;
            }
            if (actualType is FloatType)
                return _builder.BuildLoad2(_module.Context.DoubleType, storedPtr, $"val_{expr.IdField}");

            if (actualType is BoolType)
                return _builder.BuildLoad2(_module.Context.Int1Type, storedPtr, $"val_{expr.IdField}");

            // Strings, Arrays, and nested Records are already pointers; return as-is
            return storedPtr;
        }

        private LLVMValueRef CopyRecord(LLVMValueRef recordPtr, RecordType recordType)
        {
            var structType = GetOrCreateStructType(recordType);
            var newPtr = _builder.BuildMalloc(structType, "record_copy");

            // Copy each field
            for (int i = 0; i < recordType.RecordFields.Count; i++)
            {
                var oldFieldPtr = _builder.BuildStructGEP2(structType, recordPtr, (uint)i, "old_ptr");
                var val = _builder.BuildLoad2(structType.StructGetTypeAtIndex((uint)i), oldFieldPtr, "val");
                var newFieldPtr = _builder.BuildStructGEP2(structType, newPtr, (uint)i, "new_ptr");
                _builder.BuildStore(val, newFieldPtr);
            }

            return newPtr;
        }

        public LLVMValueRef VisitAddFieldExpr(AddFieldNodeExpr expr)
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

        public LLVMValueRef VisitRemoveFieldExpr(RemoveFieldNodeExpr expr)
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

        public LLVMValueRef VisitToCsvExpr(ToCsvNodeExpr expr)
        {
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);

            // 1. Visit the Array (This is a variable 'arr', so it IS boxed)
            var boxedArray = Visit(expr.Expression);

            // Extract the raw ArrayObject* from the box
            var arrayDataAddr = _builder.BuildStructGEP2(_runtimeValueType, boxedArray, 1, "array_unbox_gep");
            var rawArrayPtr = _builder.BuildLoad2(i8Ptr, arrayDataAddr, "raw_array_ptr");

            // 2. Visit the Path ("test.csv")
            var pathValue = Visit(expr.FileNameExpr);
            LLVMValueRef rawPathPtr;

            // Check: If pathValue is a literal string, it's already a ptr to i8.
            // If it's a variable, it's a ptr to RuntimeValue.
            if (pathValue.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind &&
                pathValue.TypeOf.ElementType.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                // It's a raw string literal i8*
                rawPathPtr = pathValue;
            }
            else
            {
                // It's a boxed variable (e.g. if you did 'p = "test.csv"' then 'to_csv(arr, p)')
                var pathDataAddr = _builder.BuildStructGEP2(_runtimeValueType, pathValue, 1, "path_unbox_gep");
                rawPathPtr = _builder.BuildLoad2(i8Ptr, pathDataAddr, "raw_path_ptr");
            }

            // 3. Setup the Function
            var toCsvFunc = GetOrDeclareToCsv();
            var toCsvType = LLVMTypeRef.CreateFunction(_module.Context.VoidType, new[] { i8Ptr, i8Ptr }, false);

            // 4. Call with raw pointers
            _builder.BuildCall2(toCsvType, toCsvFunc, new[] { rawArrayPtr, rawPathPtr }, "");

            // 5. Return None box
            return CreateRuntimeObject((short)ValueTag.None, LLVMValueRef.CreateConstPointerNull(i8Ptr));
        }

        private string GetTypeString(RecordType record)
        {
            string desc = "";
            foreach (var field in record.RecordFields)
            {
                var t = field.Value?.Type ?? field.Type;
                if (t is IntType) desc += "I";
                else if (t is FloatType) desc += "F";
                else if (t is StringType) desc += "S";
                else if (t is BoolType) desc += "B";
                else desc += "U"; // Unknown
            }
            return desc;
        }

        public LLVMValueRef VisitReadCsvExpr(ReadCsvNodeExpr expr)
        {
            var path = Visit(expr.FileNameExpr);

            // Create a string describing the types (e.g. "SIF" for String, Int, Float)
            // to help the C# parser know how to parse the text.
            string typeStr = GetTypeString(expr.SchemaExpr.Type as RecordType);
            var schemaDesc = _builder.BuildGlobalStringPtr(typeStr, "schema_desc");

            var readCsvFunc = GetOrDeclareReadCsv(); // Update this to take TWO i8* args
            var rawDfPtr = _builder.BuildCall2(readCsvFunc.TypeOf.ElementType, readCsvFunc,
                new[] { path, schemaDesc }, "csv_df");

            // Tag 7 is your Dataframe tag
            return CreateRuntimeObject(7, rawDfPtr);
        }

        private LLVMValueRef GetOrDeclareReadCsv()
        {
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            var func = _module.GetNamedFunction("ReadCsvInternal");
            if (func.Handle != IntPtr.Zero) return func;

            // Returns a raw pointer (i8*) to an array/record buffer
            var type = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i8Ptr });
            return _module.AddFunction("ReadCsvInternal", type);
        }

        private LLVMValueRef GetOrDeclareToCsv()
        {
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            var func = _module.GetNamedFunction("ToCsvInternal");
            if (func.Handle != IntPtr.Zero) return func;

            // Void return, two i8* (pointer) arguments
            var type = LLVMTypeRef.CreateFunction(_module.Context.VoidType, new[] { i8Ptr, i8Ptr });
            return _module.AddFunction("ToCsvInternal", type);
        }

        private LLVMValueRef CreateRuntimeObject(short tag, LLVMValueRef dataPtr)
        {
            var ctx = _module.Context;
            var i16 = ctx.Int16Type;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);

            // 1. Define the struct type { i16, ptr } 
            // Make sure this matches your C# RuntimeValue struct exactly
            var runtimeObjType = LLVMTypeRef.CreateStruct(new[] { i16, i8Ptr }, false);

            // 2. GET MALLOC (Safety Check)
            // We must look for malloc in the CURRENT module
            var mallocFunc = _module.GetNamedFunction("malloc");
            var mallocType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i64 });

            if (mallocFunc.Handle == IntPtr.Zero)
            {
                // If not found in this module, declare it
                mallocFunc = _module.AddFunction("malloc", mallocType);
            }

            // 3. ALLOCATE 16 bytes
            // This is where your crash was happening because mallocFunc was likely null
            var runtimeObj = _builder.BuildCall2(
                mallocType,
                mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, 16) },
                "runtime_obj"
            );

            // 4. Store the Tag
            var tagPtr = _builder.BuildStructGEP2(runtimeObjType, runtimeObj, 0, "tag_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i16, (ulong)tag), tagPtr);

            // 5. Store the Data Pointer
            var dataFieldPtr = _builder.BuildStructGEP2(runtimeObjType, runtimeObj, 1, "data_ptr");
            _builder.BuildStore(dataPtr, dataFieldPtr);

            return runtimeObj;
        }


        private LLVMValueRef BuildPointerArray(List<LLVMValueRef> elements)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            var malloc = GetOrDeclareMalloc();

            var count = elements.Count;

            var size = LLVMValueRef.CreateConstInt(i64, (ulong)(count * 8));
            var mem = _builder.BuildCall2(_mallocType, malloc, new[] { size }, "arr");

            var cast = _builder.BuildBitCast(mem, LLVMTypeRef.CreatePointer(i8Ptr, 0), "arr_cast");

            for (int i = 0; i < count; i++)
            {
                var ptr = _builder.BuildGEP2(i8Ptr, cast,
                    new[] { LLVMValueRef.CreateConstInt(i64, (ulong)i) },
                    "elem_ptr");

                var val = _builder.BuildBitCast(elements[i], i8Ptr, "to_i8ptr");

                _builder.BuildStore(val, ptr);
            }

            return mem;
        }

        public LLVMValueRef VisitDataframeExpr(DataframeNodeExpr expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var mallocFunc = GetOrDeclareMalloc();

            // DataframeObject Structure: { i64 (colCount), i64 (rowCount), ptr (cols), ptr (rows) }
            var dfStructType = LLVMTypeRef.CreateStruct(new[] { i8Ptr, i8Ptr, i8Ptr }, false);

            // 1. Visit the children to get the Array pointers
            var colsPtr = Visit(expr.Columns); // returns pointer to ArrayObject
            var rowsPtr = Visit(expr.Rows);    // returns pointer to ArrayObject
            List<ExpressionNodeExpr> datatypeIntArray = new List<ExpressionNodeExpr>();

            // Need to store datatypes
            // Fill the DataTypes array
            for (int i = 0; i < expr.DataTypes.Count; i++)
            {
                int typeId = GetTypeByTag(expr.DataTypes[i]);
                datatypeIntArray.Add(new NumberNodeExpr(typeId));
            }
            var datatypeArray = new ArrayNodeExpr(datatypeIntArray);
            var datatypeArrayPtr = Visit(datatypeArray);

            var datatypesCount = LLVMValueRef.CreateConstInt(i64, (ulong)datatypeArray.Elements.Count);
            var datatypesCapacity = LLVMValueRef.CreateConstInt(i64, (ulong)datatypeArray.Elements.Count > 0 ? (ulong)datatypeArray.Elements.Count * 2 : 10);

            var colsCount = LLVMValueRef.CreateConstInt(i64, (ulong)expr.Columns.Elements.Count);
            var colsCapacity = LLVMValueRef.CreateConstInt(i64, (ulong)expr.Columns.Elements.Count > 0 ? (ulong)expr.Columns.Elements.Count * 2 : 10);

            var rowsCount = LLVMValueRef.CreateConstInt(i64, (ulong)expr.Rows.Elements.Count);
            var rowsCapacity = LLVMValueRef.CreateConstInt(i64, (ulong)expr.Rows.Elements.Count > 0 ? (ulong)expr.Rows.Elements.Count * 2 : 10);

            // 2. Extract counts from the Array headers (Length is the first i64 in ArrayObject)
            // We use GEP on the ArrayObject pointers to read the 'length' field
            var arrayStructType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);

            var headerRaw = _builder.BuildCall2(_mallocType, mallocFunc, new[] { LLVMValueRef.CreateConstInt(i64, 24) }, "header_raw");

            // 4. Store Metadata into DataframeObject
            // Struct Type: { i64, i64, i8Ptr, i8Ptr }
            _builder.BuildStore(colsCount, _builder.BuildStructGEP2(arrayStructType, headerRaw, 0, "df_col_count"));
            _builder.BuildStore(colsCapacity, _builder.BuildStructGEP2(arrayStructType, headerRaw, 1, "df_col_capacity"));
            _builder.BuildStore(_builder.BuildBitCast(colsPtr, i8Ptr), _builder.BuildStructGEP2(arrayStructType, headerRaw, 2, "df_cols"));

            _builder.BuildStore(rowsCount, _builder.BuildStructGEP2(arrayStructType, headerRaw, 0, "df_row_count"));
            _builder.BuildStore(rowsCapacity, _builder.BuildStructGEP2(arrayStructType, headerRaw, 1, "df_row_capacity"));
            _builder.BuildStore(_builder.BuildBitCast(rowsPtr, i8Ptr), _builder.BuildStructGEP2(arrayStructType, headerRaw, 2, "df_rows"));

            _builder.BuildStore(datatypesCount, _builder.BuildStructGEP2(arrayStructType, headerRaw, 0, "df_datatypes_count"));
            _builder.BuildStore(datatypesCapacity, _builder.BuildStructGEP2(arrayStructType, headerRaw, 1, "df_datatypes_capacity"));
            _builder.BuildStore(_builder.BuildBitCast(datatypeArrayPtr, i8Ptr), _builder.BuildStructGEP2(arrayStructType, headerRaw, 2, "df_datatypes"));

            var dfType = _module.Context.CreateNamedStruct("dataframe");
            dfType.StructSetBody(new[] { i8Ptr, i8Ptr, i8Ptr }, false);
            var dfPtr = _builder.BuildMalloc(dfType, "df_ptr");

            _builder.BuildStore(colsPtr, _builder.BuildStructGEP2(dfStructType, dfPtr, 0));
            _builder.BuildStore(rowsPtr, _builder.BuildStructGEP2(dfStructType, dfPtr, 1));
            _builder.BuildStore(datatypeArrayPtr, _builder.BuildStructGEP2(dfStructType, dfPtr, 2));

            return dfPtr;
        }

        private string FormatTable(List<string> columnNames, List<List<object>> rows)
        {
            int colCount = columnNames.Count;
            var colWidths = new int[colCount];

            // Calculate Widths
            for (int c = 0; c < colCount; c++)
            {
                colWidths[c] = columnNames[c].Length;
                foreach (var row in rows)
                {
                    string s = row[c] is IFormattable f
                        ? f.ToString(null, CultureInfo.InvariantCulture)
                        : row[c]?.ToString() ?? "null";
                    if (s.Length > colWidths[c]) colWidths[c] = s.Length;
                }
            }

            // Prepare indices for head/tail view
            int rowCount = rows.Count;
            int maxRows = 10;
            var rowIndices = new List<int>();
            if (rowCount <= maxRows)
            {
                for (int i = 0; i < rowCount; i++) rowIndices.Add(i);
            }
            else
            {
                for (int i = 0; i < 5; i++) rowIndices.Add(i);
                rowIndices.Add(-1);
                for (int i = rowCount - 5; i < rowCount; i++) rowIndices.Add(i);
            }

            string FormatRow(List<string> data) =>
                string.Join(" | ", data.Select((val, i) => val.PadRight(colWidths[i])));

            string separator = string.Join("-+-", colWidths.Select(w => new string('-', w)));
            var lines = new List<string> { FormatRow(columnNames), separator };

            foreach (var r in rowIndices)
            {
                if (r == -1)
                {
                    lines.Add(string.Join(" | ", colWidths.Select(w => "...".PadRight(w))));
                    continue;
                }
                var rowStrings = rows[r].Select(v =>
                    v is IFormattable f
                    ? f.ToString(null, CultureInfo.InvariantCulture)
                    : v?.ToString() ?? "null"
                ).ToList();
                lines.Add(FormatRow(rowStrings));
            }

            return "Dataframe:\n" + string.Join("\n", lines.Select(l => "  " + l));
        }



        public LLVMValueRef VisitNamedArgumentExpr(NamedArgumentNodeExpr expr) // this is never called?
        {
            // This node is just a wrapper for a regular expression with a name.
            // The name is used for semantic analysis and doesn't affect codegen directly.
            return Visit(expr.Value);
        }

        public LLVMValueRef VisitShowDataframeExpr(ShowDataframeNodeExpr expr)
        {
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var dfPtr = Visit(expr.Source);

            var dfType = dfPtr.TypeOf.ElementType;

            var columnsPtr = _builder.BuildLoad2(i8Ptr,
                _builder.BuildStructGEP2(dfType, dfPtr, 0), "cols");

            var dataPtr = _builder.BuildLoad2(i8Ptr,
                _builder.BuildStructGEP2(dfType, dfPtr, 1), "data");

            var typesPtr = _builder.BuildLoad2(i8Ptr,
                _builder.BuildStructGEP2(dfType, dfPtr, 2), "types");

            foreach (var colExpr in expr.Columns)
            {
                var nameVal = Visit(colExpr); // LLVMValueRef to string
            }

            throw new NotImplementedException();
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

        public LLVMValueRef VisitDataframeElementAccess(LLVMValueRef dfPtr, LLVMValueRef colIndex, LLVMValueRef rowIndex)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i1 = ctx.Int1Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var dfType = _module.GetTypeByName("dataframe");

            // --- 1. get column pointer ---
            var colPtr = VisitDataframeColumnAccess(dfPtr, colIndex);

            // --- 2. get type info ---
            var typesArrayPtr = _builder.BuildLoad2(
                i8Ptr,
                _builder.BuildStructGEP2(dfType, dfPtr, 2),
                "df_types"
            );

            var typesData = GetArrayData(typesArrayPtr);

            var typePtr = _builder.BuildGEP2(i8Ptr, typesData, new[] { colIndex }, "type_ptr_ptr");
            var typeVal = _builder.BuildLoad2(i8Ptr, typePtr, "type_val");

            // You need a way to identify bool type
            // Example: compare against a global type tag
            // bool isBool = ((ArrayType)expr.ArrayExpression.Type).ElementType is BoolType;
            // var isBool = BuildIsBoolType(typeVal); // YOU implement this
            bool isBool = true; // Placeholder: Assume all columns are bool for this example

            // --- 3. branch ---
            var func = _builder.InsertBlock.Parent;

            // 2. Define the Basic Blocks
            var boolBlock = func.AppendBasicBlock("bool_case");
            var otherBlock = func.AppendBasicBlock("other_case");
            var mergeBlock = func.AppendBasicBlock("merge");

            //_builder.BuildCondBr(isBool, boolBlock, otherBlock);

            // --- BOOL CASE ---
            _builder.PositionAtEnd(boolBlock);

            var boolValRaw = GetArrayElementRaw(colPtr, rowIndex, true);
            var boolVal = _builder.BuildTrunc(boolValRaw, i1, "bool_i1");

            _builder.BuildBr(mergeBlock);
            var boolBlockEnd = _builder.InsertBlock;

            // --- OTHER CASE ---
            _builder.PositionAtEnd(otherBlock);

            var ptrVal = GetArrayElementRaw(colPtr, rowIndex, false);

            _builder.BuildBr(mergeBlock);
            var otherBlockEnd = _builder.InsertBlock;

            // --- MERGE ---
            _builder.PositionAtEnd(mergeBlock);

            var phi = _builder.BuildPhi(i8Ptr, "df_elem");

            phi.AddIncoming(new[] { boolValRaw, ptrVal }, new[] { boolBlockEnd, otherBlockEnd }, 0);

            return phi;
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

        public LLVMValueRef VisitTypeLiteralExpr(TypeLiteralNodeExpr expr)
        {
            if (_debug) Console.WriteLine("visiting: TypeLiteral (Skipping CodeGen)");
            // Return a dummy null pointer so LLVM doesn't get a completely empty handle
            return LLVMValueRef.CreateConstNull(LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0));
        }

        /*  df = dataframe(columns=["name", "age", "hasJob", "savings"], 

        data=[
        {name:"Bob", age: 23, hasJob: true, savings: 230500.00},
        {name:"Alice", age: 23, hasJob: true, savings: 100500.55},
        {name:"John", age: 87, hasJob: false, savings: 1209000.02},
        {name:"Mary", age: 29, hasJob: false, savings: 10700.25}
        ])

        df = read_csv([index: int, name: string, age: int, hasJob: bool, savings: float], "CSV/test.csv")

        */
    }
}