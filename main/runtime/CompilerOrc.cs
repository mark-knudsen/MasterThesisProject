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
using System.Text;
using System.Data;

namespace MyCompiler
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)] // Pack 8 is standard for 64-bit pointers
    public struct RuntimeValue
    {
        public long tag;
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
    public struct DataframeObject
    {
        public IntPtr columnData;         // Array header for column names       
        public IntPtr dataPointersData;   // Array header containing column array pointers        
        public IntPtr datatypesData;      // Array header for datatype names/types       
        public long rowCount;             // Total rows
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

        public static IntPtr ReadCsvInternal(IntPtr pathPtr, IntPtr schemaPtr)
        {
            string path = Marshal.PtrToStringAnsi(pathPtr);
            if (!File.Exists(path))
                return IntPtr.Zero;

            string schema = Marshal.PtrToStringAnsi(schemaPtr);

            var lines = File.ReadAllLines(path).Skip(1).ToArray();
            int rowCount = lines.Length;
            int colCount = schema.Length;

            // ============================
            // COLUMN BUFFERS + ARRAY HEADERS
            // ============================

            IntPtr[] columnBuffers = new IntPtr[colCount];
            IntPtr[] columnHeaders = new IntPtr[colCount];

            for (int c = 0; c < colCount; c++)
            {
                columnBuffers[c] = Marshal.AllocHGlobal(rowCount * 8);
                columnHeaders[c] = Marshal.AllocHGlobal(24);
                Marshal.WriteInt64(columnHeaders[c], 0, rowCount);
                Marshal.WriteInt64(columnHeaders[c], IntPtr.Size, rowCount);
                Marshal.WriteIntPtr(columnHeaders[c], IntPtr.Size * 2, columnBuffers[c]);
            }

            for (int i = 0; i < rowCount; i++)
            {
                string[] parts = lines[i].Split(',');

                for (int c = 0; c < colCount; c++)
                {
                    string raw = parts[c].Trim();
                    int offset = i * 8;
                    char typeCode = schema[c];

                    if (typeCode == 'I')
                    {
                        long.TryParse(raw, out long v);
                        Marshal.WriteInt64(columnBuffers[c], offset, v);
                    }
                    else if (typeCode == 'F')
                    {
                        double.TryParse(raw,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out double v);

                        Marshal.WriteInt64(columnBuffers[c], offset,
                            BitConverter.DoubleToInt64Bits(v));
                    }
                    else if (typeCode == 'B')
                    {
                        bool v = raw == "true" || raw == "1";
                        Marshal.WriteByte(columnBuffers[c], offset, v ? (byte)1 : (byte)0);
                    }
                    else if (typeCode == 'S')
                    {
                        IntPtr strPtr = Marshal.StringToHGlobalAnsi(raw);
                        Marshal.WriteIntPtr(columnBuffers[c], offset, strPtr);
                    }
                }
            }

            // ============================
            // DATA ARRAY (columns of columns)
            // ============================

            IntPtr dataArray = Marshal.AllocHGlobal(colCount * IntPtr.Size);

            for (int i = 0; i < colCount; i++)
                Marshal.WriteIntPtr(dataArray, i * IntPtr.Size, columnHeaders[i]);

            IntPtr dataArrayHeader = Marshal.AllocHGlobal(24);
            Marshal.WriteInt64(dataArrayHeader, 0, colCount);
            Marshal.WriteInt64(dataArrayHeader, IntPtr.Size, colCount);
            Marshal.WriteIntPtr(dataArrayHeader, IntPtr.Size * 2, dataArray);

            // ============================
            // DATAFRAME OBJECT (32 bytes FIXED)
            // ============================

            IntPtr df = Marshal.AllocHGlobal(Marshal.SizeOf<DataframeObject>());

            Marshal.WriteIntPtr(df, 0, IntPtr.Zero);        // column names (unused for now)
            Marshal.WriteIntPtr(df, IntPtr.Size, dataArrayHeader);
            Marshal.WriteIntPtr(df, IntPtr.Size * 2, IntPtr.Zero);
            Marshal.WriteInt64(df, IntPtr.Size * 3, rowCount);

            // ============================
            // WRAP RUNTIME VALUE (16 bytes)
            // ============================

            IntPtr runtime = Marshal.AllocHGlobal(16);
            Marshal.WriteInt64(runtime, 0, 7);
            Marshal.WriteIntPtr(runtime, 8, df);

            return runtime;
        }

        public static void ToCsvInternal(IntPtr dfPtr, IntPtr pathPtr)
        {
            string path = Marshal.PtrToStringAnsi(pathPtr);

            IntPtr df = Marshal.ReadIntPtr(dfPtr, 8);

            IntPtr dataHeader = Marshal.ReadIntPtr(df, IntPtr.Size);
            long colCount = Marshal.ReadInt64(dataHeader, 0);
            IntPtr dataPtr = Marshal.ReadIntPtr(dataHeader, IntPtr.Size * 2);
            long rowCount = Marshal.ReadInt64(df, IntPtr.Size * 3);
            int intColCount = checked((int)colCount);

            using var writer = new StreamWriter(path);

            // header
            writer.WriteLine(string.Join(",", Enumerable.Range(0, intColCount).Select(i => $"col{i}")));

            for (int r = 0; r < rowCount; r++)
            {
                List<string> row = new();

                for (int c = 0; c < colCount; c++)
                {
                    IntPtr col = Marshal.ReadIntPtr(dataPtr, c * IntPtr.Size);
                    int offset = r * 8;

                    long value = Marshal.ReadInt64(col, offset);
                    row.Add(value.ToString());
                }

                writer.WriteLine(string.Join(",", row));
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
        LLVMTypeRef _arrayStruct;
        LLVMTypeRef _dataframeStruct;
        HashSet<string> _definedGlobals = new(); // wouldn't we use our context for this job?
        private Context _context;
        public Context GetContext() => _context;
        public void ClearContext() => _context = Context.Empty;

        private LLVMTypeRef _mallocType;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MainDelegate();

        private ReadCsvDelegate _readCsvDelegate;
        private ToCsvDelegate _toCsvDelegate;

        public delegate IntPtr ReadCsvDelegate(IntPtr pathPtr, IntPtr schemaPtr);
        public delegate void ToCsvDelegate(IntPtr data, IntPtr path);
        private LLVMTypeRef _readCsvInternalType;
        public List<double> compilerTestList = new List<double>();
        public List<double> IRTestList = new List<double>();
        public List<double> RuntimeTestList = new List<double>();
        Stopwatch sw;
        void StartStopWatch() => sw = Stopwatch.StartNew();


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
            DeclareRunTimeValueType();
            DeclareArrayStruct();
            DeclareDataframeStruct();
            SetupCsvFunctions();
        }
        double StopStopWatch(string testName = null)
        {
            sw.Stop();
            if (testName is not null)
                Console.WriteLine("\n--- Execution Stats - " + testName + " ---");
            else
                Console.WriteLine("\n--- Execution Stats ---");

            Console.WriteLine($"Execution Time: {sw.Elapsed.TotalMilliseconds} ms");
            Console.WriteLine($"Ticks: {sw.ElapsedTicks}");
            Console.WriteLine("------------------------\n");

            return sw.Elapsed.TotalMilliseconds;
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

        LLVMTypeRef DeclareRunTimeValueType()
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;

            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            _runtimeValueType = LLVMTypeRef.CreateStruct(new[] { i64, i8Ptr }, false);
            return _runtimeValueType;
        }

        LLVMTypeRef DeclareArrayStruct()
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;

            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            _arrayStruct = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);
            return _arrayStruct;
        }

        LLVMTypeRef DeclareDataframeStruct()
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;

            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            _dataframeStruct = LLVMTypeRef.CreateStruct(
                new LLVMTypeRef[]
                {
            i8Ptr,  // columnData
            i8Ptr,  // dataPointersData
            i8Ptr,  // datatypesData
            i64     // rowCount   ⭐ REQUIRED FIX
                },
                false
            );

            return _dataframeStruct;
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
            // string llvmIR = module.PrintToString();
            // Console.WriteLine("Generated LLVM IR:\n");
            // Console.WriteLine(llvmIR);
            // File.WriteAllText("output_actual_orc.ll", llvmIR);
        }

        private Type PerformSemanticAnalysis(Node expr)
        {
            var checker = new TypeChecker(_context, _debug);
            _lastType = checker.Check(expr);
            _context = checker.UpdatedContext;

            //_lastNode = GetLastExpression(expr);

            var programedResult = GetLastExpression(expr);

            // Console.WriteLine("the last expression node is: " + programedResult as ExpressionNode);

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

        public object Run(Node expr, bool debug = false, bool useStopWatch = false, bool showAllColumns = false, bool showAllRows = false)
        {
            _debug = debug;
            bool _stopwatch = useStopWatch;

            //_showAllColumns = showAllColumns;
            //_showAllRows = showAllRows;


            // 1. Semantic analysis
            var prediction = PerformSemanticAnalysis(expr);

            CreateMain();
            DeclarePrintf();

            if (_debug) Console.WriteLine("we code gen");

            {
                compilerTestList = new List<double>();
                IRTestList = new List<double>();
                RuntimeTestList = new List<double>();

            }

            if (_stopwatch) StartStopWatch();
            LLVMValueRef resultValue = Visit(expr);
            if (_stopwatch) compilerTestList.Add(StopStopWatch("Ran codegen"));

            if (_debug) Console.WriteLine("LLVM TYPE: " + resultValue.TypeOf);
            if (_debug) Console.WriteLine("LANG TYPE: " + prediction);

            var boxedPtr = BoxValue(resultValue, prediction);
            _builder.BuildRet(boxedPtr);

            if (_debug) DumpIR(_module);

            if (_stopwatch) StartStopWatch();

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
            if (_stopwatch) IRTestList.Add(StopStopWatch("Ran IR codegen"));

            // Create stopwatch to measure execution time - uncomment if you want to see the stats for each command, but it can be a bit much
            // Stopwatch sw = Stopwatch.StartNew();

            if (_stopwatch) StartStopWatch();
            var tempResult = delegateResult();
            if (_stopwatch) RuntimeTestList.Add(StopStopWatch("Ran program"));

            if (_stopwatch)
            {
                Console.WriteLine($"\ncompiler list: \n{string.Join(", ", compilerTestList)}");
                Console.WriteLine($"\nIR list: \n{string.Join(", ", IRTestList)}");
                Console.WriteLine($"\nRuntime list: \n{string.Join(", ", RuntimeTestList)}");
            }

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

                    if (prediction is RecordType recType)
                        return HandleRecord(result.data, recType);

                    throw new Exception("Run time object failed for tag record");

                case ValueTag.Dataframe:
                    if (_debug) Console.WriteLine("return Dataframe");
                    if (prediction is DataframeType dfType)
                    {
                        return HandleDataframe(result.data, dfType);
                    }
                    return "Dataframe Failure";

                case ValueTag.None:
                    if (_debug) Console.WriteLine("return none");
                    return default;
            }

            return result;
        }

        private string HandleArray(IntPtr arrayObjPtr, Type type)
        {
            if (arrayObjPtr == IntPtr.Zero) return "[]";

            var array = Marshal.PtrToStructure<ArrayObject>(arrayObjPtr);
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
            if (dataPtr == IntPtr.Zero) return "{ empty record }";

            var result = new Dictionary<string, object>();
            int fieldSize = 8;

            if (_debug) Console.WriteLine("Record type: " + record);
            //if (_debug) Console.WriteLine("Record type count: " + record.RecordFields?.Count);

            for (int i = 0; i < record.RecordFields?.Count; i++)
            {
                var rec = record.RecordFields[i];
                string label = rec.Label;

                Type recType = rec.Type;
                if (_debug) Console.WriteLine($"Label: {label}, type: {recType}");

                IntPtr fieldLocation = IntPtr.Add(dataPtr, i * fieldSize);
                IntPtr ptr = fieldLocation;
                //IntPtr ptr = Marshal.ReadIntPtr(fieldLocation); // the old and bad

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
                        //result[label] = ptr == IntPtr.Zero ? "null" : Marshal.PtrToStringAnsi(ptr);
                        IntPtr strPtr = Marshal.ReadIntPtr(fieldLocation);
                        result[label] = strPtr == IntPtr.Zero ? "null" : Marshal.PtrToStringAnsi(strPtr);
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
                    ? f.ToString()
                    : kv.Value?.ToString() ?? "null";
                return $"{kv.Key}: {valStr}";
            });

            return "{ " + string.Join(", ", entries) + " }";
        }

        private Dictionary<int, object> ExtractArrayIndexed(IntPtr arrayObjPtr, ArrayType type, List<int> indices)
        {
            var elementType = type.ElementType;
            var result = new Dictionary<int, object>();

            if (arrayObjPtr == IntPtr.Zero)
                return result;

            // =====================================================
            // 🔥 MANUAL UNPACK (NO PtrToStructure)
            // =====================================================
            long length = Marshal.ReadInt64(arrayObjPtr, 0);
            long capacity = Marshal.ReadInt64(arrayObjPtr, 8);
            IntPtr data = Marshal.ReadIntPtr(arrayObjPtr, 16);

            if (data == IntPtr.Zero)
                return result;

            const int stride = 8;

            // =====================================================
            // ELEMENT ACCESS
            // =====================================================
            foreach (var i in indices)
            {
                if (i < 0 || i >= length)
                    continue;

                IntPtr elemPtr = IntPtr.Add(data, i * stride);

                object value;

                try
                {
                    if (elementType is IntType)
                    {
                        value = Marshal.ReadInt64(elemPtr);
                    }
                    else if (elementType is FloatType)
                    {
                        long raw = Marshal.ReadInt64(elemPtr);
                        value = BitConverter.Int64BitsToDouble(raw);
                    }
                    else if (elementType is BoolType)
                    {
                        value = Marshal.ReadInt64(elemPtr) != 0;
                    }
                    else if (elementType is StringType)
                    {
                        IntPtr strPtr = Marshal.ReadIntPtr(elemPtr);

                        if (strPtr == IntPtr.Zero)
                            value = "null";
                        else
                        {
                            try
                            {
                                value = Marshal.PtrToStringAnsi(strPtr) ?? "null";
                            }
                            catch
                            {
                                value = "<invalid string>";
                            }
                        }
                    }
                    else
                    {
                        value = "<unknown type>";
                    }
                }
                catch
                {
                    value = "<memory error>";
                }

                result[i] = value;
            }

            return result;
        }

        // public void DebugDataframe(IntPtr dfPtr)
        // {
        //     if (dfPtr == IntPtr.Zero)
        //         return;

        //     IntPtr columnsPtr = Marshal.ReadIntPtr(dfPtr, 0);
        //     IntPtr dataPtr = Marshal.ReadIntPtr(dfPtr, IntPtr.Size);
        //     IntPtr typesPtr = Marshal.ReadIntPtr(dfPtr, IntPtr.Size * 2);

        //     var columns = Marshal.PtrToStructure<ArrayObject>(columnsPtr);
        //     var data = Marshal.PtrToStructure<ArrayObject>(dataPtr);
        //     var types = Marshal.PtrToStructure<ArrayObject>(typesPtr);

        //     DebugArray(columns, "columns");
        //     DebugArray(data, "data", true);
        //     DebugArray(types, "dataTypes");
        // }
        public void DebugDataframe(IntPtr dfPtr)
        {
            if (dfPtr == IntPtr.Zero)
                return;

            long tag = Marshal.ReadInt64(dfPtr, 0);
            IntPtr dataPtr = Marshal.ReadIntPtr(dfPtr, 8);

            Console.WriteLine("=== Dataframe DEBUG ===");
            Console.WriteLine($"tag: {tag}");
            Console.WriteLine($"dataPtr: {dataPtr}");

            if (dataPtr == IntPtr.Zero)
                return;

            IntPtr columns = Marshal.ReadIntPtr(dataPtr, 0);
            IntPtr data = Marshal.ReadIntPtr(dataPtr, IntPtr.Size);
            IntPtr types = Marshal.ReadIntPtr(dataPtr, IntPtr.Size * 2);

            long rowCount = Marshal.ReadInt64(dataPtr, IntPtr.Size * 3);

            Console.WriteLine($"rows: {rowCount}");
            Console.WriteLine($"columns: {columns}");
            Console.WriteLine($"data: {data}");

            if (columns != IntPtr.Zero)
                DebugArrayRaw(columns, "columns");

            if (data != IntPtr.Zero)
                DebugArrayRaw(data, "data", true);

            if (types != IntPtr.Zero)
                DebugArrayRaw(types, "types");
        }



        private void DebugArrayRaw(IntPtr arrayPtr, string name, bool nested = false)
        {
            if (arrayPtr == IntPtr.Zero)
            {
                Console.WriteLine($"{name}: NULL");
                return;
            }

            long length = Marshal.ReadInt64(arrayPtr, 0);
            long capacity = Marshal.ReadInt64(arrayPtr, 8);
            IntPtr data = Marshal.ReadIntPtr(arrayPtr, 16);

            Console.WriteLine($"{name}: len={length}, cap={capacity}");

            if (!nested || length == 0)
                return;

            IntPtr first = Marshal.ReadIntPtr(data);

            Console.WriteLine($"{name}[0] = {first}");
        }


        private void DebugArray(ArrayObject arrayObject, string arrayName, bool isArrayOfArrays = false)
        {
            Console.WriteLine(arrayName + " capacity: " + arrayObject.capacity);
            Console.WriteLine(arrayName + " length: " + arrayObject.length);

            if (isArrayOfArrays && arrayObject.length > 0)
            {
                IntPtr firstColPtr = Marshal.ReadIntPtr(arrayObject.data);
                if (firstColPtr == IntPtr.Zero) return;
                try
                {
                    long firstLen = Marshal.ReadInt64(firstColPtr, 0);
                    int rowCount = (int)firstLen;

                }
                catch
                {
                    Console.WriteLine("not an array of arrays");
                }
            }
            else
            {
                Console.WriteLine(arrayName + " is a primitive array, not an array of arrays");
            }
        }

        // private string HandleDataframe(IntPtr dfPtr, DataframeType type)
        // {
        //     // If dfPtr comes from result.data in Run(), it is already the pointer to the DataframeObject.
        //     if (dfPtr == IntPtr.Zero) return "dataframe(null)";

        //     // =====================================================
        //     // 1. Direct Field Access (No Tag Reading)
        //     // =====================================================
        //     // Layout: { IntPtr columns, IntPtr dataArrays, IntPtr types, long rowCount }
        //     IntPtr columnsArrayPtr = Marshal.ReadIntPtr(dfPtr, 0);
        //     IntPtr dataPointers = Marshal.ReadIntPtr(dfPtr, IntPtr.Size);
        //     IntPtr typesArrayPtr = Marshal.ReadIntPtr(dfPtr, IntPtr.Size * 2);
        //     long rowCount = Marshal.ReadInt64(dfPtr, IntPtr.Size * 3);

        //     int colCount = type.ColumnNames.Count;
        //     int rowCountInt = (int)rowCount;

        //     // =====================================================
        //     // 2. Row selection
        //     // =====================================================
        //     int maxRows = 50;
        //     var rowIndices = new List<int>();

        //     if (rowCountInt <= maxRows)
        //     {
        //         for (int i = 0; i < rowCountInt; i++) rowIndices.Add(i);
        //     }
        //     else
        //     {
        //         for (int i = 0; i < 5; i++) rowIndices.Add(i);
        //         rowIndices.Add(-1); // Indicator for "..."
        //         for (int i = rowCountInt - 5; i < rowCountInt; i++) rowIndices.Add(i);
        //     }

        //     // =====================================================
        //     // 3. Extract data (Reading from the dataPointers buffer)
        //     // =====================================================
        //     var columns = new List<Dictionary<int, object>>();

        //     for (int c = 0; c < colCount; c++)
        //     {
        //         // Read the c-th column pointer from the dataPointers buffer
        //         IntPtr colPtr = Marshal.ReadIntPtr(dataPointers, c * IntPtr.Size);

        //         // Use a modified version of ExtractArrayIndexed that takes the raw buffer and rowCount
        //         var sparse = ExtractRawColumnData(
        //             colPtr,
        //             type.DataTypes[c],
        //             rowCountInt,
        //             rowIndices
        //         );

        //         columns.Add(sparse);
        //     }

        //     // 6. Formatting Logic
        //     var colWidths = new int[colCount];
        //     for (int c = 0; c < colCount; c++)
        //     {
        //         colWidths[c] = type.ColumnNames[c].Length;
        //         foreach (var r in rowIndices)
        //         {
        //             if (r < 0) continue;
        //             if (columns[c].TryGetValue(r, out var valObj))
        //             {
        //                 var s = valObj?.ToString() ?? "null";
        //                 if (s.Length > colWidths[c]) colWidths[c] = s.Length;
        //             }
        //         }
        //     }

        //     int indexWidth = Math.Max("index".Length, rowCountInt > 0 ? (rowCountInt - 1).ToString().Length : 1);

        //     string FormatRow(string index, List<string> row)
        //     {
        //         var paddedIndex = index.PadRight(indexWidth);
        //         var paddedCols = row.Select((val, i) => val.PadRight(colWidths[i]));
        //         return paddedIndex + " | " + string.Join(" | ", paddedCols);
        //     }

        //     string separator = new string('-', indexWidth) + "-+-" +
        //                        string.Join("-+-", colWidths.Select(w => new string('-', w)));

        //     var lines = new List<string>();
        //     lines.Add(FormatRow("index", type.ColumnNames.ToList()));
        //     lines.Add(separator);

        //     foreach (var r in rowIndices)
        //     {
        //         if (r == -1)
        //         {
        //             lines.Add("...".PadRight(indexWidth) + " | " +
        //                       string.Join(" | ", colWidths.Select(w => "...".PadRight(w))));
        //             continue;
        //         }

        //         var rowStrings = new List<string>();
        //         for (int c = 0; c < colCount; c++)
        //         {
        //             if (columns[c].TryGetValue(r, out var valObj))
        //                 rowStrings.Add(valObj?.ToString() ?? "null");
        //             else
        //                 rowStrings.Add("null");
        //         }
        //         lines.Add(FormatRow(r.ToString(), rowStrings));
        //     }

        //     return "Dataframe (" + rowCountInt + " rows):\n" + string.Join("\n", lines.Select(l => "  " + l));
        // }


        private string HandleDataframe(IntPtr dfPtr, DataframeType type)
        {
            if (dfPtr == IntPtr.Zero) return "dataframe(null)";

            // 1. Direct Field Access
            // Layout: { ptr colNames, ptr dataArrays, ptr types, i64 rowCount }
            IntPtr dataArraysField = Marshal.ReadIntPtr(dfPtr, IntPtr.Size);
            long rowCountRaw = Marshal.ReadInt64(dfPtr, IntPtr.Size * 3);
            int rowCount = (int)rowCountRaw;

            // 2. SMART UNBOXING
            // We need to determine if dataArraysField is an ArrayObject header OR a direct pointer array.
            IntPtr actualDataBuffer;

            // Read potential header values
            long maybeLen = Marshal.ReadInt64(dataArraysField, 0);
            IntPtr maybePtr = Marshal.ReadIntPtr(dataArraysField, 16);

            // Heuristic: If the 3rd field is a valid pointer and the 1st field matches our colCount
            if (maybeLen == type.ColumnNames.Count && maybePtr != IntPtr.Zero && (long)maybePtr > 0x1000)
            {
                actualDataBuffer = maybePtr; // It's an ArrayObject header
            }
            else
            {
                actualDataBuffer = dataArraysField; // It's a direct pointer buffer (common in read_csv)
            }

            // 3. Row Selection
            var rowIndices = new List<int>();
            if (rowCount <= 20)
            {
                for (int i = 0; i < rowCount; i++) rowIndices.Add(i);
            }
            else
            {
                for (int i = 0; i < 5; i++) rowIndices.Add(i);
                for (int i = rowCount - 5; i < rowCount; i++) rowIndices.Add(i);
            }

            // 4. Extraction Logic
            var sparseRows = new Dictionary<int, List<object>>();
            foreach (int r in rowIndices) sparseRows[r] = new List<object>();

            for (int c = 0; c < type.ColumnNames.Count; c++)
            {
                IntPtr colPtr = Marshal.ReadIntPtr(actualDataBuffer, c * IntPtr.Size);
                if (colPtr == IntPtr.Zero) continue;

                // Columns can ALSO be wrapped in ArrayObjects
                IntPtr colDataBuf;
                long cLen = Marshal.ReadInt64(colPtr, 0);
                IntPtr cPtr = Marshal.ReadIntPtr(colPtr, 16);

                if (cLen == rowCount && cPtr != IntPtr.Zero && (long)cPtr > 0x1000)
                    colDataBuf = cPtr;
                else
                    colDataBuf = colPtr;

                var colValues = ExtractFromBuffer(colDataBuf, type.DataTypes[c], rowCount, rowIndices);
                foreach (int r in rowIndices)
                {
                    sparseRows[r].Add(colValues.ContainsKey(r) ? colValues[r] : "null");
                }
            }

            return FormatTable(type.ColumnNames.ToList(), sparseRows, rowCount);
        }

        private Dictionary<int, object> ExtractFromBuffer(IntPtr dataBuf, Type elemType, int len, List<int> indices)
        {
            var result = new Dictionary<int, object>();
            if (dataBuf == IntPtr.Zero) return result;
            // Defensive: if caller provided an ArrayObject header instead of the raw data pointer,
            // unwrap it to the actual data buffer. This prevents reading the header region
            // as element data which can cause AccessViolation when indices are large.
            try
            {
                long possibleLen = Marshal.ReadInt64(dataBuf, 0);
                IntPtr possibleDataPtr = Marshal.ReadIntPtr(dataBuf, IntPtr.Size * 2);
                if (possibleLen == len && possibleDataPtr != IntPtr.Zero && (long)possibleDataPtr > 0x1000)
                {
                    dataBuf = possibleDataPtr;
                }
            }
            catch
            {
                // If any read fails, fall back to using provided pointer as-is
            }

            foreach (var i in indices)
            {
                if (i < 0 || i >= len) continue;
                IntPtr ptr = IntPtr.Add(dataBuf, i * 8);

                if (elemType is IntType) result[i] = Marshal.ReadInt64(ptr);
                else if (elemType is FloatType)
                {
                    byte[] b = new byte[8];
                    Marshal.Copy(ptr, b, 0, 8);
                    result[i] = BitConverter.ToDouble(b, 0);
                }
                else if (elemType is BoolType)
                {
                    // Bool storage may be either 1-byte or stored as i64 (0/1). Try reading a byte first,
                    // but fall back to reading an Int64 if that appears invalid.
                    try
                    {
                        result[i] = Marshal.ReadByte(ptr) != 0;
                    }
                    catch
                    {
                        // Fall back: read 8-byte int
                        result[i] = Marshal.ReadInt64(ptr) != 0;
                    }
                }
                else if (elemType is StringType)
                {
                    IntPtr s = Marshal.ReadIntPtr(ptr);
                    result[i] = (s == IntPtr.Zero) ? "null" : Marshal.PtrToStringAnsi(s);
                }
            }
            return result;
        }

        private Dictionary<int, object> ExtractRawColumnData(IntPtr data, Type elementType, int length, List<int> indices)
        {
            var result = new Dictionary<int, object>();
            if (data == IntPtr.Zero) return result;

            foreach (var i in indices)
            {
                if (i < 0 || i >= length) continue;

                // Every element in your ReadCsv logic is 8-byte aligned (i64, double, or ptr)
                IntPtr elemPtr = IntPtr.Add(data, i * 8);

                if (elementType is IntType) result[i] = Marshal.ReadInt64(elemPtr);
                else if (elementType is FloatType)
                {
                    byte[] bytes = new byte[8];
                    Marshal.Copy(elemPtr, bytes, 0, 8);
                    result[i] = BitConverter.ToDouble(bytes, 0);
                }
                else if (elementType is BoolType) result[i] = Marshal.ReadInt64(elemPtr) != 0;
                else if (elementType is StringType)
                {
                    IntPtr sPtr = Marshal.ReadIntPtr(elemPtr);
                    // CRITICAL: Check for null pointer before converting to string
                    result[i] = (sPtr == IntPtr.Zero) ? "null" : Marshal.PtrToStringAnsi(sPtr);
                }
            }
            return result;
        }

        private string FormatTable(List<string> columnNames, Dictionary<int, List<object>> rows, int rowCount)
        {
            var allHeaders = new List<string> { "index" };
            allHeaders.AddRange(columnNames);
            int totalCols = allHeaders.Count;

            // --- Horizontal Truncation (Columns) ---
            var colIndices = new List<int>();
            if (totalCols <= 10)
            {
                for (int i = 0; i < totalCols; i++) colIndices.Add(i);
            }
            else
            {
                for (int i = 0; i < 4; i++) colIndices.Add(i);
                colIndices.Add(-1); // Horizontal separator
                for (int i = totalCols - 3; i < totalCols; i++) colIndices.Add(i);
            }

            // --- Calculate Widths ---
            var colWidths = new Dictionary<int, int>();
            foreach (var c in colIndices)
            {
                if (c == -1) { colWidths[c] = 3; continue; }
                int maxWidth = allHeaders[c].Length;
                if (c == 0) maxWidth = Math.Max(maxWidth, rowCount.ToString().Length);

                foreach (var kvp in rows)
                {
                    string s = (c == 0) ? kvp.Key.ToString() : (kvp.Value[c - 1]?.ToString() ?? "null");
                    if (s.Length > maxWidth) maxWidth = s.Length;
                    if (maxWidth >= 30) { maxWidth = 30; break; }
                }
                colWidths[c] = maxWidth;
            }

            // --- Vertical Truncation (Rows) ---
            var rowDisplayIndices = new List<int>();
            if (rowCount <= 20)
            {
                for (int i = 0; i < rowCount; i++) rowDisplayIndices.Add(i);
            }
            else
            {
                for (int i = 0; i < 5; i++) rowDisplayIndices.Add(i);
                rowDisplayIndices.Add(-1);
                for (int i = rowCount - 5; i < rowCount; i++) rowDisplayIndices.Add(i);
            }

            // --- Build Lines ---
            string BuildLine(Func<int, string> provider)
            {
                var parts = colIndices.Select(c => (c == -1 ? "..." : provider(c)).PadRight(colWidths[c]));
                return string.Join(" | ", parts);
            }

            var lines = new List<string>();
            lines.Add(BuildLine(c => allHeaders[c]));
            lines.Add(string.Join("-+-", colIndices.Select(c => new string('-', colWidths[c]))));

            foreach (var r in rowDisplayIndices)
            {
                if (r == -1)
                {
                    lines.Add(BuildLine(c => "..."));
                    continue;
                }
                lines.Add(BuildLine(c => (c == 0) ? r.ToString() : rows[r][c - 1]?.ToString() ?? "null"));
            }

            return $"\nDataframe ({rowCount} rows, {columnNames.Count} columns):\n" +
                   string.Join("\n", lines.Select(l => "   " + l));
        }

        private int GetTypeByTag(Type type)
        {
            if (type == null)
            {
                if (_debug) Console.WriteLine("CRITICAL: GetTypeByTag received a NULL type!");
                return (int)ValueTag.None;
            }

            if (_debug) Console.WriteLine($"Resolving Tag for: {type.GetType().Name}");

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

        // Helper to handle the raw column buffers created by ReadCsvInternal


        private bool IsReferenceType(Type t)
        {
            return t is StringType || t is ArrayType || t is RecordType || t is DataframeType;
        }
        private bool IsValueType(Type t)
        {
            return t is IntType || t is FloatType || t is BoolType;
        }

        // private uint GetTypeSize(Type type)
        // {
        //     if (type is IntType || type is FloatType) return 8;
        //     if (type is BoolType) return 1;
        //     if (type is StringType || type is ArrayType || type is RecordType || type is DataframeType) return 8; // pointer size
        //     throw new Exception("Unknown type for size calculation: " + type);
        // }

        private uint GetStructSize(Type type)
        {
            if (type is RecordType rt)
                return (uint)rt.RecordFields.Count * 8; // Each field is a pointer
            if (type is DataframeType)
                return 24; // 3 pointers: columns, rows, types
            if (type is ArrayType)
                return 24; // len, cap, data*
            throw new Exception("Unknown type for struct size calculation: " + type);
        }

        private LLVMValueRef BoxValue(LLVMValueRef value, Type type)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            var mallocFunc = GetOrDeclareMalloc();

            // --- THE SHIELD ---
            var boxTypePtr = LLVMTypeRef.CreatePointer(_runtimeValueType, 0);
            if (value.TypeOf == boxTypePtr)
                return value;

            int tag = GetTypeByTag(type);
            LLVMValueRef dataPtr;

            if (IsValueType(type))
            {
                int size = type switch
                {
                    IntType => 8,
                    FloatType => 8,
                    BoolType => 8,
                    _ => 8
                };

                var mem = _builder.BuildCall2(_mallocType, mallocFunc,
                    new[] { LLVMValueRef.CreateConstInt(i64, (ulong)size) }, "value_mem");

                var castType = type switch
                {
                    IntType => LLVMTypeRef.CreatePointer(i64, 0),
                    FloatType => LLVMTypeRef.CreatePointer(ctx.DoubleType, 0),
                    BoolType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0),
                    _ => LLVMTypeRef.CreatePointer(i64, 0)
                };

                var cast = _builder.BuildBitCast(mem, castType, "value_cast");
                _builder.BuildStore(value, cast).SetAlignment(8);
                dataPtr = mem;
            }
            else if (IsReferenceType(type))
            {
                // Treat as pointer
                //if (_debug) Console.WriteLine($"Boxing complex type: {type} (Tag: {tag})");
                dataPtr = _builder.BuildBitCast(value, i8Ptr, "boxed_ptr_cast");
            }
            else if (type is VoidType)
                dataPtr = LLVMValueRef.CreateConstPointerNull(i8Ptr);
            else
                throw new Exception($"Unsupported type in BoxValue: {type}");

            // Allocate RuntimeValue (struct { i16 tag, i8* data })
            var objRaw = _builder.BuildCall2(_mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, 16) }, "runtime_obj");

            var obj = _builder.BuildBitCast(objRaw, LLVMTypeRef.CreatePointer(_runtimeValueType, 0), "runtime_cast");

            // Store tag
            var tagPtr = _builder.BuildStructGEP2(_runtimeValueType, obj, 0, "tag_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)tag), tagPtr).SetAlignment(8);

            // Store data
            var dataFieldPtr = _builder.BuildStructGEP2(_runtimeValueType, obj, 1, "data_ptr");
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
            var ctx = _module.Context;
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

            // If the condition is a Double (typical for comparison results), 
            // compare it against 0.0 using FCmp
            if (condition.TypeOf == ctx.DoubleType)
            {
                condition = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE,
                    condition, LLVMValueRef.CreateConstReal(ctx.DoubleType, 0.0), "fortest_dbl");
            }
            // If it's an i64 or i32, use ICmp
            else if (condition.TypeOf == ctx.Int8Type)
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

            // --- CLEAN VECTORIZATION HINT START ---
            // 1. Create the inner loop properties node: !{!"llvm.loop.vectorize.enable", i1 true}
            var metadataName = ctx.GetMDString("llvm.loop.vectorize.enable", 26);
            var valTrue = LLVMValueRef.CreateConstInt(ctx.Int1Type, 1);
            var vectorizeAttrValue = LLVMValueRef.CreateMDNode(new[] { metadataName, valTrue });

            // Extract the metadata representation for replacement assignment entries
            var vectorizeAttrMetadata = LLVM.ValueAsMetadata(vectorizeAttrValue);

            // 2. Create the parent tracking node as a standard Metadata Value Node
            var loopMetadataValue = LLVMValueRef.CreateMDNode(new[] { vectorizeAttrValue, vectorizeAttrValue });

            // 3. Perform mutations safely using the exact mapped API
            var loopMetadataMD = LLVM.ValueAsMetadata(loopMetadataValue);
            LLVM.ReplaceMDNodeOperandWith(loopMetadataValue, 0, loopMetadataMD);
            LLVM.ReplaceMDNodeOperandWith(loopMetadataValue, 1, vectorizeAttrMetadata);

            // 4. Emit back-edge jump and attach the loop metadata tracking block
            var backBr = _builder.BuildBr(condBlock);
            uint loopKindId = ctx.GetMDKindID("llvm.loop");
            backBr.SetMetadata(loopKindId, loopMetadataValue);
            // --- CLEAN VECTORIZATION HINT END ---

            // 6. End Block
            _builder.PositionAtEnd(endBlock);

            return default;
        }

        public LLVMValueRef VisitForEachLoop(ForEachLoopNode expr)
        {
            var ctx = _module.Context;
            var func = _builder.InsertBlock.Parent;

            // Array pointer and length
            var arrayPtr = Visit(expr.SourceExpression);
            var zero = LLVMValueRef.CreateConstInt(ctx.Int32Type, 0);
            var lenPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { zero }, "len_ptr");
            var arrayLen = _builder.BuildLoad2(ctx.Int64Type, lenPtr, "array_len");

            // Stack-local iterator
            var elementType = ((ArrayType)expr.SourceExpression.Type).ElementType;
            System.Console.WriteLine("in for loop");
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
            var idNode = (IdNode)expr.Id;
            string varName = idNode.Name;

            // 1. GET THE POINTER (Address), NOT THE VALUE
            // We look for the global variable directly.
            LLVMValueRef variablePtr = _module.GetNamedGlobal(varName);

            // If it's not in the current module, declare it as extern (same as your VisitAssign logic)
            if (variablePtr.Handle == IntPtr.Zero)
            {
                if (_definedGlobals.Contains(varName))
                {
                    System.Console.WriteLine("in increment");
                    variablePtr = _module.AddGlobal(GetLLVMType(expr.Id.Type), varName);
                    variablePtr.Linkage = LLVMLinkage.LLVMExternalLinkage;
                    variablePtr.SetAlignment(8);
                }
                else
                {
                    throw new Exception($"Variable {varName} not defined.");
                }
            }

            // 2. LOAD the current value manually using the pointer
            var llvmType = GetLLVMType(expr.Id.Type);
            var currentValue = _builder.BuildLoad2(llvmType, variablePtr, "x_load");
            currentValue.SetAlignment(8);

            // 3. MATH
            LLVMValueRef newValue;
            if (llvmType.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
            {
                newValue = _builder.BuildFAdd(currentValue, LLVMValueRef.CreateConstReal(llvmType, 1.0), "inc_add");
            }
            else
            {
                newValue = _builder.BuildAdd(currentValue, LLVMValueRef.CreateConstInt(llvmType, 1, false), "inc_add");
            }

            // 4. STORE back into the POINTER
            _builder.BuildStore(newValue, variablePtr).SetAlignment(8);

            return newValue;
        }

        public LLVMValueRef VisitDecrement(DecrementNode expr)
        {
            var idNode = (IdNode)expr.Id;
            string varName = idNode.Name;

            // 1. GET THE POINTER (Address), NOT THE VALUE
            // We look for the global variable directly.
            LLVMValueRef variablePtr = _module.GetNamedGlobal(varName);

            // If it's not in the current module, declare it as extern (same as your VisitAssign logic)
            if (variablePtr.Handle == IntPtr.Zero)
            {
                if (_definedGlobals.Contains(varName))
                {
                    variablePtr = _module.AddGlobal(GetLLVMType(expr.Id.Type), varName);
                    variablePtr.Linkage = LLVMLinkage.LLVMExternalLinkage;
                    variablePtr.SetAlignment(8);
                }
                else
                {
                    throw new Exception($"Variable {varName} not defined.");
                }
            }

            // 2. LOAD the current value manually using the pointer
            var llvmType = GetLLVMType(expr.Id.Type);
            var currentValue = _builder.BuildLoad2(llvmType, variablePtr, "x_load");
            currentValue.SetAlignment(8);

            LLVMValueRef newValue;
            if (llvmType.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
            {
                newValue = _builder.BuildFSub(currentValue, LLVMValueRef.CreateConstReal(llvmType, 1.0), "dec_sub");
            }
            else
            {
                newValue = _builder.BuildSub(currentValue, LLVMValueRef.CreateConstInt(llvmType, 1, false), "dec_sub");
            }

            _builder.BuildStore(newValue, variablePtr).SetAlignment(8);
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
            Visit(node.ThenPart);
            _builder.BuildBr(mergeBlock);

            // ELSE
            _builder.PositionAtEnd(elseBlock);

            if (node.ElsePart != null)
                Visit(node.ElsePart);

            _builder.BuildBr(mergeBlock);

            // MERGE
            _builder.PositionAtEnd(mergeBlock);

            return default;
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
            return LLVMValueRef.CreateConstInt(_module.Context.Int8Type, expr.Value ? 1UL : 0UL);
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
        private long GetTagForType(Type type)
        {
            if (type is IntType) return (long)ValueTag.Int;
            if (type is FloatType) return (long)ValueTag.Float;
            if (type is StringType) return (long)ValueTag.String;
            if (type is ArrayType) return (long)ValueTag.Array;
            if (type is RecordType) return (long)ValueTag.Record;
            return (long)ValueTag.None;
        }

        public LLVMValueRef VisitRandom(RandomNode expr)
        {
            if (expr.Arguments.Count < 2 || expr.Arguments.Count > 3)
                throw new Exception("random() expects 2 or 3 arguments");

            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var doubleType = ctx.DoubleType;

            // Ensure rand()
            var randFunc = _module.GetNamedFunction("rand");
            if (randFunc.Handle == IntPtr.Zero)
            {
                var randType = LLVMTypeRef.CreateFunction(i64, Array.Empty<LLVMTypeRef>(), false);
                randFunc = _module.AddFunction("rand", randType);
            }

            var randCall = _builder.BuildCall2(
                LLVMTypeRef.CreateFunction(i64, Array.Empty<LLVMTypeRef>()),
                randFunc,
                Array.Empty<LLVMValueRef>(),
                "rand"
            );

            // CASE 1: INTEGER RANDOM
            if (expr.Type is IntType)
            {
                var min = Visit(expr.MinValue);
                var max = Visit(expr.MaxValue);

                if (min.TypeOf == doubleType)
                    min = _builder.BuildFPToSI(min, i64, "min_i");

                if (max.TypeOf == doubleType)
                    max = _builder.BuildFPToSI(max, i64, "max_i");

                var func = _builder.InsertBlock.Parent;

                var thenBB = func.AppendBasicBlock("rand.int.ok");
                var elseBB = func.AppendBasicBlock("rand.int.swap");
                var mergeBB = func.AppendBasicBlock("rand.int.merge");

                var cond = _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntSLE,
                    min,
                    max,
                    "order_ok"
                );

                _builder.BuildCondBr(cond, thenBB, elseBB);

                // THEN
                _builder.PositionAtEnd(thenBB);

                var range1 = _builder.BuildAdd(
                    _builder.BuildSub(max, min),
                    LLVMValueRef.CreateConstInt(i64, 1),
                    "range1"
                );

                var mod1 = _builder.BuildSRem(randCall, range1, "mod1");
                var res1 = _builder.BuildAdd(mod1, min, "res1");

                _builder.BuildBr(mergeBB);
                var thenEnd = _builder.InsertBlock;

                // ELSE
                _builder.PositionAtEnd(elseBB);

                var range2 = _builder.BuildAdd(
                    _builder.BuildSub(min, max),
                    LLVMValueRef.CreateConstInt(i64, 1),
                    "range2"
                );

                var mod2 = _builder.BuildSRem(randCall, range2, "mod2");
                var res2 = _builder.BuildAdd(mod2, max, "res2");

                _builder.BuildBr(mergeBB);
                var elseEnd = _builder.InsertBlock;

                // MERGE
                _builder.PositionAtEnd(mergeBB);

                var phi = _builder.BuildPhi(i64, "rand_int");

                phi.AddIncoming(
                    new[] { res1, res2 },
                    new[] { thenEnd, elseEnd },
                    2
                );

                return phi;
            }

            // CASE 2: FLOAT RANDOM
            else if (expr.Type is FloatType)
            {
                var min = Visit(expr.MinValue);
                var max = Visit(expr.MaxValue);

                if (min.TypeOf == i64)
                    min = _builder.BuildSIToFP(min, doubleType, "min_f");

                if (max.TypeOf == i64)
                    max = _builder.BuildSIToFP(max, doubleType, "max_f");

                // rand -> double
                var randFp = _builder.BuildSIToFP(randCall, doubleType, "rand_fp");
                double randMaxValue = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 32767.0 : 2147483647.0; // HACK: to make it work for windows and linux
                var randMax = LLVMValueRef.CreateConstReal(doubleType, randMaxValue);

                var normalized = _builder.BuildFDiv(randFp, randMax, "norm");

                var range = _builder.BuildFSub(max, min, "range");
                var scaled = _builder.BuildFMul(normalized, range, "scaled");
                var result = _builder.BuildFAdd(scaled, min, "rand_float");

                // APPLY DECIMALS IF PRESENT
                if (expr.Decimals != null)
                {
                    var decimals = EnsureFloat(Visit(expr.Decimals), expr.Decimals.Type);

                    var powType = LLVMTypeRef.CreateFunction(doubleType, new[] { doubleType, doubleType });
                    var roundType = LLVMTypeRef.CreateFunction(doubleType, new[] { doubleType });

                    var powFunc = _module.GetNamedFunction("pow");
                    if (powFunc.Handle == IntPtr.Zero)
                        powFunc = _module.AddFunction("pow", powType);

                    var roundFunc = _module.GetNamedFunction("round");
                    if (roundFunc.Handle == IntPtr.Zero)
                        roundFunc = _module.AddFunction("round", roundType);

                    // optional: force decimals to integer
                    decimals = _builder.BuildFPToSI(decimals, i64, "dec_int");
                    decimals = _builder.BuildSIToFP(decimals, doubleType, "dec_back");

                    var ten = LLVMValueRef.CreateConstReal(doubleType, 10.0);

                    var multiplier = _builder.BuildCall2(
                        powType,
                        powFunc,
                        new[] { ten, decimals },
                        "multiplier"
                    );

                    var temp = _builder.BuildFMul(result, multiplier, "temp");

                    var roundedTemp = _builder.BuildCall2(
                        roundType,
                        roundFunc,
                        new[] { temp },
                        "roundedTemp"
                    );

                    result = _builder.BuildFDiv(roundedTemp, multiplier, "rounded_random");
                }

                return result;
            }

            throw new Exception("Random only supports int or float ranges");
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

                    case RecordType recType:
                        {
                            var ctx = _module.Context;
                            var i64 = ctx.Int64Type;
                            //var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

                            // Print "{ "
                            var open = _builder.BuildGlobalStringPtr("{ ", "rec_open");
                            _builder.BuildCall2(_printfType, _printf, new[] { open }, "");

                            for (int i = 0; i < recType.RecordFields.Count; i++)
                            {
                                var field = recType.RecordFields[i];

                                // --- print field name ---
                                var nameStr = _builder.BuildGlobalStringPtr(field.Label + ": ", $"field_{field.Label}");
                                _builder.BuildCall2(_printfType, _printf, new[] { nameStr }, "");

                                // --- load field value ---
                                var index = LLVMValueRef.CreateConstInt(i64, (ulong)i);

                                var slotPtr = _builder.BuildGEP2(
                                    i8Ptr,
                                    valueToPrint,
                                    new[] { index },
                                    "field_ptr"
                                );

                                var storedPtr = _builder.BuildLoad2(i8Ptr, slotPtr, "field_val_ptr");

                                LLVMValueRef fieldVal;

                                if (field.Type is IntType)
                                {
                                    fieldVal = _builder.BuildLoad2(i64, storedPtr, "int_val");

                                    var fmt = _builder.BuildGlobalStringPtr("%ld", "fmt_int");
                                    _builder.BuildCall2(_printfType, _printf, new[] { fmt, fieldVal }, "");
                                }
                                else if (field.Type is FloatType)
                                {
                                    fieldVal = _builder.BuildLoad2(ctx.DoubleType, storedPtr, "float_val");

                                    var fmt = _builder.BuildGlobalStringPtr("%f", "fmt_float");
                                    _builder.BuildCall2(_printfType, _printf, new[] { fmt, fieldVal }, "");
                                }
                                else if (field.Type is BoolType)
                                {
                                    fieldVal = _builder.BuildLoad2(ctx.Int1Type, storedPtr, "bool_val");

                                    DeclareBoolStrings();
                                    var str = _builder.BuildSelect(fieldVal, _trueStr, _falseStr, "boolstr");

                                    _builder.BuildCall2(_printfType, _printf, new[] { str }, "");
                                }
                                else if (field.Type is StringType)
                                {
                                    fieldVal = storedPtr;

                                    var fmt = _builder.BuildGlobalStringPtr("%s", "fmt_str");
                                    _builder.BuildCall2(_printfType, _printf, new[] { fmt, fieldVal }, "");
                                }
                                else
                                {
                                    // nested record / array → recurse
                                    AddImplicitPrint(storedPtr, field.Type);
                                }

                                // --- comma between fields ---
                                if (i < recType.RecordFields.Count - 1)
                                {
                                    var comma = _builder.BuildGlobalStringPtr(", ", "comma");
                                    _builder.BuildCall2(_printfType, _printf, new[] { comma }, "");
                                }
                            }

                            // Print " }\n"
                            var close = _builder.BuildGlobalStringPtr(" }\n", "rec_close");
                            return _builder.BuildCall2(_printfType, _printf, new[] { close }, "");
                        }

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

        // syntax 
        // r + {x: 5, y: 3}                          // should return a record with added fields x and y
        // x.map(d=> d.name + "Smith")               // should return an array
        // x.map(d=> d + {x: 5, y: 3})               // should return a dataframe with added columns x and y
        // x.map(d=> d + {name: d.name + "smith"})   // should return a dataframe with added columns x and y

        // {x:5}+{y:4} = {x:5, y:4}  
        // {x:5}+{x:4} = {x:4}
        // {x:5}+{x:4, y:2} = {x:4, y:2}
        // {x:5}+{x:4, y:2} + {y:3, z:1} + {x:1} = {x:1, y:3, z:1}
        // {x:5}+{y:2}+{z:1} = {x:5, y:2, z:1}
        // {x:5}+{y:2}+{z:1}+{a:"bob", b: true, f:10.1} = { x: 5, y: 2, z: 1, a: bob, b: True, f: 10.1 }

        // for(i=0; i<50; i++) print(i)
        // foreach(item in x) {print(item)} // but this works fine
        // for(i=0; i<50; i++) {print(i)} // BUG, this line can't run, it thinks it is a record node

        public LLVMValueRef VisitPrint(PrintNode expr)
        {
            var valueToPrint = Visit(expr.Expression);
            return AddImplicitPrint(valueToPrint, expr.Expression.Type);
        }
        // x=dataframe(["name", "age"], type=[string, int])                       // works
        // x=dataframe(["name", "age"], [string, int])                            // works
        // x=dataframe(["name", "age"], [["dan", 30], ["alice", 25]])            // works

        // x=dataframe(["name", "age"], [["dan", 30]])  doesn't work
        // x=dataframe(["name", "age", "name2"], [["dan", 30, 1.2], ["alice", 25, 1.1]])            // doesn't work

        // x=dataframe(["name", "age"], [["dan", "alice], [30, 25]])             // doesn't work
        // x=dataframe(columns=["name", "age"], [["dan", 30], ["alice", 25]])   // doesn't work

        // x=record({name: "Hary potter", age: 30, rating: 10.5585})  

        // x.add({name: "Hary potter2", age: 201})
        // x.addRange([{name: "voldemort", age: 80}, {name: "dumbledore", age: 70}, {name: "MERLIN", age: 101}])

        // for(i=0; i<5; i++) x.add({name: "Hary potter", age: 10}) 
        // for(i=0; i<50; i++) x.add({name: "Hary potter", age: 10 + random(1,100)}) 
        // for(i=0; i<5200000; i++) x.add({name: "Hary potter", age: 10 + random(1,100)}) 
        // for(i=0; i<5000000; i++) x.add({name: "Hary potter", age: 10 + random(1,100)}) // 5 million
        // this below can't do random inside addRange "Cannot perform + on int and"
        // for(i=0; i<520000; i++) x.addRange([{name: "voldemort", age: 80}, {name: "dumbledore", age: 70}, {name: "MERLIN", age: 101}]) 

        // x.map(d => d.age + 10) // this should return the dataframe not the column
        // x.where(d => d.age == 30)
        // x.where(d=> d > 9).where(z=> z < 93)
        // x.where(d=> d.age > 91).where(z=> z.age < 93 & z.name=="Hary potter")
        // x.where(d=> d.savings > 693444.47).where(z=> z.savings < 6903444.47 & z.name=="John")

        // x=read_csv("CSV/test.csv")

        public LLVMValueRef VisitWhere(WhereNode expr)
        {
            var sourceType = expr.SourceExpression.Type;

            if (sourceType is DataframeType dfType)
            {
                if (IsVectorizableColumnPredicate(expr.Condition, expr.IteratorId.Name, out string targetColumnName, out string op, out ExpressionNode literalExpr))
                {
                    return EmitVectorizedDataframeWhere(expr, dfType, targetColumnName, op, literalExpr);
                }
                return FallbackToMacroWhere(expr, dfType);
            }
            else if (sourceType is ArrayType arrType)
            {
                var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 4);
                var iVar = $"__i_{uniqueId}";
                var resultVar = $"__result_{uniqueId}";
                var program = BuildWhereArray(expr, arrType, iVar, resultVar);
                PerformSemanticAnalysis(program);
                return VisitSequence(program);
            }
            else
            {
                throw new Exception("Where only supports arrays and dataframes");
            }
        }

        private LLVMValueRef FallbackToMacroWhere(WhereNode expr, DataframeType dfType)
        {
            System.Console.WriteLine("Condition is not vectorizable, falling back to macro expansion...");
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 4);
            var iVar = $"__i_{uniqueId}";
            var resultVar = $"__result_{uniqueId}";

            SequenceNode program = BuildWhereDataframe(expr, dfType, iVar, resultVar);
            PerformSemanticAnalysis(program);
            return VisitSequence(program);
        }

        private bool IsVectorizableColumnPredicate(ExpressionNode condition, string iteratorName, out string columnName, out string op, out ExpressionNode literalNode)
        {
            System.Console.WriteLine("Checking if condition is vectorizable...");
            columnName = null; op = null; literalNode = null;
            ExpressionNode left = null;
            ExpressionNode right = null;

            if (condition is ComparisonNode cmp)
            {
                op = cmp.Operator; left = cmp.Left; right = cmp.Right;
            }
            else if (condition is BinaryOpNode bin && (bin.Operator == ">" || bin.Operator == "<" || bin.Operator == "=="))
            {
                op = bin.Operator; left = bin.Left; right = bin.Right;
            }

            if (left != null && right != null)
            {
                var leftType = left.GetType();
                var recordProp = leftType.GetProperty("Expression") ?? leftType.GetProperty("Record");
                var fieldProp = leftType.GetProperty("FieldName") ?? leftType.GetProperty("Name");

                if (recordProp != null && fieldProp != null)
                {
                    var recordValue = recordProp.GetValue(left) as IdNode;
                    var fieldValue = fieldProp.GetValue(left) as string;

                    if (recordValue != null && recordValue.Name == iteratorName && fieldValue != null)
                    {
                        columnName = fieldValue;
                        literalNode = right;
                        return true;
                    }
                }
            }
            return false;
        }

        private LLVMValueRef EmitVectorizedDataframeWhere(WhereNode expr, DataframeType dfType, string targetColumnName, string op, ExpressionNode literalNode)
        {
            System.Console.WriteLine("Emitting vectorized where for column: " + targetColumnName + " with operator: " + op);
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            LLVMValueRef dfPtr = Visit(expr.SourceExpression);

            var arrayHeaderType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);
            var dataframeType = LLVMTypeRef.CreateStruct(new[] { i8Ptr, LLVMTypeRef.CreatePointer(arrayHeaderType, 0), i8Ptr, i64 }, false);

            var rowCountPtr = _builder.BuildStructGEP2(dataframeType, dfPtr, 3, "rowCount_ptr");
            LLVMValueRef totalRows = _builder.BuildLoad2(i64, rowCountPtr, "rowCount");

            int filterColIndex = -1;
            for (int idx = 0; idx < dfType.ColumnNames.Count; idx++)
            {
                if (dfType.ColumnNames[idx] == targetColumnName)
                {
                    filterColIndex = idx;
                    break;
                }
            }

            var colType = dfType.DataTypes[filterColIndex];
            if (colType is not IntType && colType is not BoolType)
            {
                return FallbackToMacroWhere(expr, dfType);
            }

            var dataPtrsPtr = _builder.BuildStructGEP2(dataframeType, dfPtr, 1, "data_ptrs_ptr");
            var columnHeaderArrayPtr = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(arrayHeaderType, 0), dataPtrsPtr, "header_ptr");

            var dataPtrsDataPtr = _builder.BuildStructGEP2(arrayHeaderType, columnHeaderArrayPtr, 2, "data_ptrs_data_ptr");
            var columnBuffersArrayRaw = _builder.BuildLoad2(i8Ptr, dataPtrsDataPtr, "data_ptrs_raw");
            var columnBuffersArray = _builder.BuildBitCast(columnBuffersArrayRaw, LLVMTypeRef.CreatePointer(LLVMTypeRef.CreatePointer(arrayHeaderType, 0), 0));

            var colIndexVal = LLVMValueRef.CreateConstInt(i64, (ulong)filterColIndex);
            var targetColHeaderPtrPtr = _builder.BuildGEP2(LLVMTypeRef.CreatePointer(arrayHeaderType, 0), columnBuffersArray, new[] { colIndexVal }, "col_ptr_ptr");
            var targetColHeaderPtr = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(arrayHeaderType, 0), targetColHeaderPtrPtr, "col_array_header");

            var targetColDataPtrPtr = _builder.BuildStructGEP2(arrayHeaderType, targetColHeaderPtr, 2, "col_data_ptr_ptr");
            var targetColDataRaw = _builder.BuildLoad2(i8Ptr, targetColDataPtrPtr, "col_data_raw");

            // Determine sizing bit width maps for targeted predicate processing
            var targetLoadType = colType is BoolType ? i8 : i64;
            var typedFilterColumnData = _builder.BuildBitCast(targetColDataRaw, LLVMTypeRef.CreatePointer(targetLoadType, 0), "typed_column_data");

            LLVMValueRef compConstant = Visit(literalNode);
            if (colType is BoolType && compConstant.TypeOf == ctx.Int1Type)
            {
                compConstant = _builder.BuildZExt(compConstant, i8, "bool_to_i8");
            }

            // Allocate sequential allocation space for the bitmask layer
            var mallocFunc = GetOrDeclareMalloc();
            var selectionMaskAlloca = _builder.BuildCall2(_mallocType, mallocFunc, new[] { totalRows }, "selection_mask");

            var matchCountAlloca = _builder.BuildAlloca(i64, "match_count");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, 0), matchCountAlloca);

            // SIMD Vector Pipeline Settings (Processes 4 elements per vector sweep pass)
            uint vectorWidth = 4;
            var vectorType = LLVMTypeRef.CreateVector(targetLoadType, vectorWidth);
            var vectorPtrType = LLVMTypeRef.CreatePointer(vectorType, 0);

            var indexAlloca = _builder.BuildAlloca(i64, "simd_idx_alloca");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, 0), indexAlloca);

            var vectorWidthVal = LLVMValueRef.CreateConstInt(i64, vectorWidth);
            var remainder = _builder.BuildURem(totalRows, vectorWidthVal, "rem");
            var endVectorIdx = _builder.BuildSub(totalRows, remainder, "end_vec_idx");

            var func = _builder.InsertBlock.Parent;
            var condBB = ctx.AppendBasicBlock(func, "simd_cond");
            var bodyBB = ctx.AppendBasicBlock(func, "simd_body");
            var tailBB = ctx.AppendBasicBlock(func, "scalar_tail");
            var compactBB = ctx.AppendBasicBlock(func, "column_compact_phase");

            _builder.BuildBr(condBB);

            // SIMD Vector Conditions Checking Header
            _builder.PositionAtEnd(condBB);
            var currentIdx = _builder.BuildLoad2(i64, indexAlloca, "curr_idx");
            var isVecLoopGo = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, currentIdx, endVectorIdx, "cmp_limit");
            _builder.BuildCondBr(isVecLoopGo, bodyBB, tailBB);

            // -------------------------------------------------------------------------
            // PARALLEL SIMD LOOP SWEEP PHASE
            // -------------------------------------------------------------------------
            _builder.PositionAtEnd(bodyBB);
            var scalarElementPtr = _builder.BuildGEP2(targetLoadType, typedFilterColumnData, new[] { currentIdx }, "scalar_gep");
            var vectorizedElementPtr = _builder.BuildBitCast(scalarElementPtr, vectorPtrType, "vec_bitcast");
            var loadedVectorChunk = _builder.BuildLoad2(vectorType, vectorizedElementPtr, "loaded_chunk");

            var criteriaVector = LLVMValueRef.CreateConstVector(Enumerable.Repeat(compConstant, (int)vectorWidth).ToArray());

            LLVMIntPredicate pred = op switch
            {
                ">" => LLVMIntPredicate.LLVMIntSGT,
                "<" => LLVMIntPredicate.LLVMIntSLT,
                "==" => LLVMIntPredicate.LLVMIntEQ,
                _ => LLVMIntPredicate.LLVMIntSGT
            };
            var vectorComparisonMask = _builder.BuildICmp(pred, loadedVectorChunk, criteriaVector, "simd_icmp");

            for (uint i = 0; i < vectorWidth; i++)
            {
                var laneBit = _builder.BuildExtractElement(vectorComparisonMask, LLVMValueRef.CreateConstInt(ctx.Int32Type, i), $"lane_{i}");
                var laneByte = _builder.BuildZExt(laneBit, i8, "lane_byte");

                var targetOffset = _builder.BuildAdd(currentIdx, LLVMValueRef.CreateConstInt(i64, i));
                var maskStorePtr = _builder.BuildGEP2(i8, selectionMaskAlloca, new[] { targetOffset }, "mask_store_gep");
                _builder.BuildStore(laneByte, maskStorePtr);

                var currentMatchCount = _builder.BuildLoad2(i64, matchCountAlloca, "curr_match");
                var addedMatch = _builder.BuildZExt(laneBit, i64, "added_match");
                _builder.BuildStore(_builder.BuildAdd(currentMatchCount, addedMatch), matchCountAlloca);
            }

            _builder.BuildStore(_builder.BuildAdd(currentIdx, vectorWidthVal), indexAlloca);
            _builder.BuildBr(condBB);

            // -------------------------------------------------------------------------
            // SCALAR TAIL FALLBACK LOOP (For non-multiples of 4 trailing rows)
            // -------------------------------------------------------------------------
            _builder.PositionAtEnd(tailBB);
            var tailCondBB = ctx.AppendBasicBlock(func, "tail_cond");
            var tailBodyBB = ctx.AppendBasicBlock(func, "tail_body");
            _builder.BuildBr(tailCondBB);

            _builder.PositionAtEnd(tailCondBB);
            var tIdx = _builder.BuildLoad2(i64, indexAlloca, "tail_idx");
            var isTailGo = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, tIdx, totalRows, "tail_limit");
            _builder.BuildCondBr(isTailGo, tailBodyBB, compactBB);

            _builder.PositionAtEnd(tailBodyBB);
            var tailElemPtr = _builder.BuildGEP2(targetLoadType, typedFilterColumnData, new[] { tIdx }, "tail_gep");
            var tailVal = _builder.BuildLoad2(targetLoadType, tailElemPtr, "tail_val");

            var scalarBit = op switch
            {
                ">" => _builder.BuildICmp(pred, tailVal, compConstant, "sc_cmp"),
                "<" => _builder.BuildICmp(pred, tailVal, compConstant, "sc_cmp"),
                "==" => _builder.BuildICmp(pred, tailVal, compConstant, "sc_cmp"),
                _ => _builder.BuildICmp(pred, tailVal, compConstant, "sc_cmp")
            };
            var tailByte = _builder.BuildZExt(scalarBit, i8, "tail_byte");
            var tailMaskPtr = _builder.BuildGEP2(i8, selectionMaskAlloca, new[] { tIdx }, "tail_mask_gep");
            _builder.BuildStore(tailByte, tailMaskPtr);

            var tcMatch = _builder.BuildLoad2(i64, matchCountAlloca, "t_match");
            _builder.BuildStore(_builder.BuildAdd(tcMatch, _builder.BuildZExt(scalarBit, i64)), matchCountAlloca);
            _builder.BuildStore(_builder.BuildAdd(tIdx, LLVMValueRef.CreateConstInt(i64, 1)), indexAlloca);
            _builder.BuildBr(tailCondBB);

            // -------------------------------------------------------------------------
            // COMPACTION STRATEGY SUBROUTINE JUMP
            // -------------------------------------------------------------------------
            _builder.PositionAtEnd(compactBB);
            LLVMValueRef finalizedMatchCount = _builder.BuildLoad2(i64, matchCountAlloca, "final_matches");

            var result = CompactColumnsUsingMask(dfPtr, dataframeType, arrayHeaderType, selectionMaskAlloca, finalizedMatchCount, totalRows, dfType);

            // Clean up mask buffer memory allocations
            var freeFunc = GetOrDeclareFree();
            _builder.BuildCall2(ctx.VoidType, freeFunc, new[] { selectionMaskAlloca });

            return result;
        }

        // --- FIXED SUBROUTINE REMOVING ALL INTERNAL CONDITIONAL REALLOC ROADBLOCKS ---
        private LLVMValueRef CompactColumnsUsingMask(LLVMValueRef srcDfPtr, LLVMTypeRef dataframeType, LLVMTypeRef arrayHeaderType, LLVMValueRef selectionMaskPtr, LLVMValueRef matchCount, LLVMValueRef originalRowCount, DataframeType dfType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            var mallocFunc = GetOrDeclareMalloc();
            var reallocFunc = GetOrDeclareRealloc();

            // 1. Setup Result Dataframe Object Instance Shell
            var destDfPtr = _builder.BuildMalloc(dataframeType, "vector_dest_df");

            var srcColsPtr = _builder.BuildStructGEP2(dataframeType, srcDfPtr, 0, "src_cols");
            var destColsPtr = _builder.BuildStructGEP2(dataframeType, destDfPtr, 0, "dest_cols");
            _builder.BuildStore(_builder.BuildLoad2(i8Ptr, srcColsPtr), destColsPtr);

            var srcTypesPtr = _builder.BuildStructGEP2(dataframeType, srcDfPtr, 2, "src_types");
            var destTypesPtr = _builder.BuildStructGEP2(dataframeType, destDfPtr, 2, "dest_types");
            _builder.BuildStore(_builder.BuildLoad2(i8Ptr, srcTypesPtr), destTypesPtr);

            var destRowCountPtr = _builder.BuildStructGEP2(dataframeType, destDfPtr, 3, "dest_row_count");
            _builder.BuildStore(matchCount, destRowCountPtr);

            int totalColumns = dfType.ColumnNames.Count;
            var columnBuffersHeaderAlloca = _builder.BuildMalloc(LLVMTypeRef.CreateArray(arrayHeaderType, (uint)totalColumns), "dest_cols_headers");
            var columnBuffersHeaderPtr = _builder.BuildBitCast(columnBuffersHeaderAlloca, LLVMTypeRef.CreatePointer(arrayHeaderType, 0));

            var destDataHeaderPtr = _builder.BuildMalloc(arrayHeaderType, "dest_data_header");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)totalColumns), _builder.BuildStructGEP2(arrayHeaderType, destDataHeaderPtr, 0));
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)totalColumns), _builder.BuildStructGEP2(arrayHeaderType, destDataHeaderPtr, 1));
            _builder.BuildStore(_builder.BuildBitCast(columnBuffersHeaderPtr, i8Ptr), _builder.BuildStructGEP2(arrayHeaderType, destDataHeaderPtr, 2));

            var destDataFieldPtr = _builder.BuildStructGEP2(dataframeType, destDfPtr, 1, "dest_data_field");
            _builder.BuildStore(destDataHeaderPtr, destDataFieldPtr);

            var srcDataPtrsPtr = _builder.BuildStructGEP2(dataframeType, srcDfPtr, 1, "src_data_ptrs");
            var srcColumnHeaderArrayPtr = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(arrayHeaderType, 0), srcDataPtrsPtr, "src_header_ptr");
            var srcDataPtrsDataPtr = _builder.BuildStructGEP2(arrayHeaderType, srcColumnHeaderArrayPtr, 2, "src_data_ptrs_data");
            var srcColumnBuffersArrayRaw = _builder.BuildLoad2(i8Ptr, srcDataPtrsDataPtr, "src_ptrs_raw");
            var srcColumnBuffersArray = _builder.BuildBitCast(srcColumnBuffersArrayRaw, LLVMTypeRef.CreatePointer(LLVMTypeRef.CreatePointer(arrayHeaderType, 0), 0));

            // 2. Loop Through and Compact Every Dataframe Vector Column Separately
            for (int colIdx = 0; colIdx < totalColumns; colIdx++)
            {
                var colIndexVal = LLVMValueRef.CreateConstInt(i64, (ulong)colIdx);
                var srcColHeaderPtrPtr = _builder.BuildGEP2(LLVMTypeRef.CreatePointer(arrayHeaderType, 0), srcColumnBuffersArray, new[] { colIndexVal }, "src_col_ptr_ptr");
                var srcColHeaderPtr = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(arrayHeaderType, 0), srcColHeaderPtrPtr, "src_col_header");

                var srcColDataPtrPtr = _builder.BuildStructGEP2(arrayHeaderType, srcColHeaderPtr, 2, "src_col_data_ptr_ptr");
                var srcColDataRaw = _builder.BuildLoad2(i8Ptr, srcColDataPtrPtr, "src_col_data_raw");

                // Determine target stride data sizes matching types
                var currentType = dfType.DataTypes[colIdx];
                var elementSize = (ulong)(currentType is IntType || currentType is FloatType ? 8 : 1);
                var elementSizeVal = LLVMValueRef.CreateConstInt(i64, elementSize);
                var itemLLVMType = currentType is IntType || currentType is FloatType ? i64 : i8;

                // --- STRATEGY 2: INITIAL MAX-CAPACITY PRE-ALLOCATION PHASE ---
                // Pre-allocate matching bounds to worst-case original row size lengths up front
                var preAllocByteSize = _builder.BuildMul(originalRowCount, elementSizeVal, "pre_alloc_size");
                var destColDataRaw = _builder.BuildCall2(_mallocType, mallocFunc, new[] { preAllocByteSize }, "dest_col_raw_init");

                // Track independent target layout write positions using scalar counters
                var writeIdxAlloca = _builder.BuildAlloca(i64, $"write_idx_col_{colIdx}");
                _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, 0), writeIdxAlloca);

                var compLoopIdxAlloca = _builder.BuildAlloca(i64, $"comp_loop_idx_{colIdx}");
                _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, 0), compLoopIdxAlloca);

                var func = _builder.InsertBlock.Parent;
                var compCondBB = ctx.AppendBasicBlock(func, $"comp_cond_{colIdx}");
                var compBodyBB = ctx.AppendBasicBlock(func, $"comp_body_{colIdx}");
                var compStoreBB = ctx.AppendBasicBlock(func, $"comp_store_{colIdx}");
                var compStepBB = ctx.AppendBasicBlock(func, $"comp_step_{colIdx}");
                var compEndBB = ctx.AppendBasicBlock(func, $"comp_end_{colIdx}");

                _builder.BuildBr(compCondBB);

                // Check iteration condition ranges
                _builder.PositionAtEnd(compCondBB);
                var cIdx = _builder.BuildLoad2(i64, compLoopIdxAlloca, "c_idx");
                var isCompGo = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, cIdx, originalRowCount, "is_comp_go");
                _builder.BuildCondBr(isCompGo, compBodyBB, compEndBB);

                // Verify item mask parameters match criteria requirements
                _builder.PositionAtEnd(compBodyBB);
                var maskElemPtr = _builder.BuildGEP2(i8, selectionMaskPtr, new[] { cIdx }, "mask_elem_gep");
                var maskVal = _builder.BuildLoad2(i8, maskElemPtr, "mask_val");
                var isRowMatched = _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, maskVal, LLVMValueRef.CreateConstInt(i8, 0), "is_matched");
                _builder.BuildCondBr(isRowMatched, compStoreBB, compStepBB);

                // --- STREAMLINED TARGET ELEMENT WRITE ---
                // Fully clean and linearly structured vector storage
                _builder.PositionAtEnd(compStoreBB);
                var wIdx = _builder.BuildLoad2(i64, writeIdxAlloca, "w_idx");

                var srcElementGEP = _builder.BuildGEP2(itemLLVMType, srcColDataRaw, new[] { cIdx }, "src_el_gep");
                var loadedElementValue = _builder.BuildLoad2(itemLLVMType, srcElementGEP, "el_val");

                var destElementGEP = _builder.BuildGEP2(itemLLVMType, destColDataRaw, new[] { wIdx }, "dest_el_gep");
                _builder.BuildStore(loadedElementValue, destElementGEP);

                var nextWIdx = _builder.BuildAdd(wIdx, LLVMValueRef.CreateConstInt(i64, 1), "next_w_idx");
                _builder.BuildStore(nextWIdx, writeIdxAlloca);
                _builder.BuildBr(compStepBB);

                // Increment tracker indices
                _builder.PositionAtEnd(compStepBB);
                var nextCIdx = _builder.BuildAdd(cIdx, LLVMValueRef.CreateConstInt(i64, 1), "next_c_idx");
                _builder.BuildStore(nextCIdx, compLoopIdxAlloca);
                _builder.BuildBr(compCondBB);

                // --- STRATEGY 2: FINAL POST-LOOP REALLOC TRUNCATION PHASE ---
                _builder.PositionAtEnd(compEndBB);
                var finalColumnBytesCount = _builder.BuildMul(matchCount, elementSizeVal, "final_bytes");

                // Truncate memory allocations down to the actual tracked length of matching rows found
                var truncatedColDataRaw = _builder.BuildCall2(_reallocType, reallocFunc, new[] { destColDataRaw, finalColumnBytesCount }, "truncated_col_raw");

                // Assign details to the destination column headers
                var destHeaderGEP = _builder.BuildGEP2(arrayHeaderType, columnBuffersHeaderPtr, new[] { colIndexVal }, "dest_head_gep");
                _builder.BuildStore(matchCount, _builder.BuildStructGEP2(arrayHeaderType, destHeaderGEP, 0, "len"));
                _builder.BuildStore(matchCount, _builder.BuildStructGEP2(arrayHeaderType, destHeaderGEP, 1, "cap"));
                _builder.BuildStore(truncatedColDataRaw, _builder.BuildStructGEP2(arrayHeaderType, destHeaderGEP, 2, "data"));
            }

            return destDfPtr;
        }

        private LLVMValueRef GetOrDeclareFree()
        {
            var ctx = _module.Context;
            var freeFunc = _module.GetNamedFunction("free");
            if (freeFunc.Handle == IntPtr.Zero)
            {
                var freeType = LLVMTypeRef.CreateFunction(ctx.VoidType, new[] { LLVMTypeRef.CreatePointer(ctx.Int8Type, 0) }, false);
                freeFunc = _module.AddFunction("free", freeType);
            }
            return freeFunc;
        }




        private SequenceNode BuildWhereDataframe(WhereNode expr, DataframeType dfType, string iVar, string resultVar)
        {
            var program = new SequenceNode();

            // 1. Create column names array
            var columnsArray = new ArrayNode(dfType.ColumnNames.Select(c => (ExpressionNode)new StringNode(c)).ToList());

            // 2. Create type literals array
            var typesArray = new ArrayNode(dfType.DataTypes.Select(t => t switch
            {
                IntType => new TypeLiteralNode(new TypeNode("int")) as ExpressionNode,
                FloatType => new TypeLiteralNode(new TypeNode("float")),
                BoolType => new TypeLiteralNode(new TypeNode("bool")),
                StringType => new TypeLiteralNode(new TypeNode("string")),
                _ => throw new Exception("Unsupported type " + t)
            }).ToList());

            // --- FIX STARTS HERE ---
            // 3. Create an array of EMPTY ARRAYS (one for each column)
            // This ensures that VisitDataframeNode initializes valid pointers for each column buffer
            var columnDataList = new List<ExpressionNode>();
            foreach (var colType in dfType.DataTypes)
            {
                var emptyCol = new ArrayNode(new List<ExpressionNode>());
                emptyCol.SetType(new ArrayType(colType)); // Very important for VisitArray to know the element size
                columnDataList.Add(emptyCol);
            }
            var dataArray = new ArrayNode(columnDataList);
            // --- FIX ENDS HERE ---

            // 3. Initialize Empty Result Dataframe
            var resultDf = new DataframeNode(new List<NamedArgumentNode> {
                new NamedArgumentNode("columns", columnsArray),
                new NamedArgumentNode("type", typesArray),
                new NamedArgumentNode("data", new ArrayNode(new List<ExpressionNode>()))
                //new NamedArgumentNode("data", dataArray)  // HELLO
            });

            program.Statements.Add(new AssignNode(resultVar, resultDf));

            // 4. Setup Loop
            // i = 0
            program.Statements.Add(new AssignNode(iVar, new NumberNode(0)));

            var cond = new ComparisonNode(new IdNode(iVar), "<", new LengthNode(expr.SourceExpression));
            var step = new IncrementNode(new IdNode(iVar));

            // row = src[i]
            var currentRow = new IndexNode(expr.SourceExpression, new IdNode(iVar));

            // Replace "x" in "x.name == 'Harry'" with the IndexNode "src[i]"
            var predicate = ReplaceIterator(expr.Condition, expr.IteratorId.Name, currentRow);

            var ifBody = new SequenceNode();
            // if match: result.add(src[i])
            ifBody.Statements.Add(new AddNode(new IdNode(resultVar), currentRow));

            var loopBody = new SequenceNode();
            loopBody.Statements.Add(new IfNode(predicate, ifBody));

            var loop = new ForLoopNode(new AssignNode(iVar, new NumberNode(0)), cond, step, loopBody);
            program.Statements.Add(loop);

            // The result of the whole WhereNode is the resultVar dataframe
            program.Statements.Add(new IdNode(resultVar));
            return program;
        }

        private Type InferTypeFromString(string value)
        {
            return value switch
            {
                "int" => new IntType(),
                "float" => new FloatType(),
                "double" => new FloatType(),
                "string" => new StringType(),
                "bool" => new BoolType(),
                _ => throw new Exception("Unsupported type for inference: " + value)
            };
        }

        private SequenceNode BuildWhereArray(WhereNode expr, ArrayType arrType, string iVar, string resultVar)
        {
            var program = new SequenceNode();

            // result = []
            var emptyArray = new ArrayNode(new List<ExpressionNode>());
            emptyArray.SetType(arrType); // Ensure the new array matches the source array type

            program.Statements.Add(new AssignNode(resultVar, emptyArray));
            program.Statements.Add(new AssignNode(iVar, new NumberNode(0)));

            var cond = new ComparisonNode(new IdNode(iVar), "<", new LengthNode(expr.SourceExpression));
            var step = new IncrementNode(new IdNode(iVar));
            var element = new IndexNode(expr.SourceExpression, new IdNode(iVar));

            var predicate = ReplaceIterator(expr.Condition, expr.IteratorId.Name, element);

            var ifBody = new SequenceNode();
            ifBody.Statements.Add(new AddNode(new IdNode(resultVar), element));

            var loopBody = new SequenceNode();
            loopBody.Statements.Add(new IfNode(predicate, ifBody));

            var loop = new ForLoopNode(new AssignNode(iVar, new NumberNode(0)), cond, step, loopBody);
            program.Statements.Add(loop);
            program.Statements.Add(new IdNode(resultVar));

            return program;
        }



        public LLVMValueRef VisitMap(MapNode expr)
        {
            var srcVarName = "__map_src";
            var resultVarName = "__map_result";
            var indexVarName = "__map_i";

            // 1 Clone source array
            var srcAssign = new AssignNode(srcVarName, new CopyArrayNode(expr.SourceExpression));

            // 2 Allocate new array for result (length = src.length)
            var resultAssign = new AssignNode(resultVarName, new CopyArrayNode(new IdNode(srcVarName)));

            // 3 Loop index
            var indexAssign = new AssignNode(indexVarName, new NumberNode(0));

            // 4 Loop condition: i < src.length
            var loopCond = new ComparisonNode(
                new IdNode(indexVarName),
                "<",
                new LengthNode(new IdNode(srcVarName))
            );

            // 5 Loop step: i++
            var loopStep = new IncrementNode(new IdNode(indexVarName));

            // 6 Current element: src[i]
            var currentElement = new IndexNode(new IdNode(srcVarName), new IdNode(indexVarName));

            // 7 Map expression: replace iterator with current element
            var map = ReplaceIterator(expr.Assignment, expr.IteratorId.Name, currentElement);

            // 8 Assign mapped value into result array
            var indexAssignNode = new IndexAssignNode(new IdNode(resultVarName), new IdNode(indexVarName), map);

            // 9 Loop body
            var loopBody = new SequenceNode();
            loopBody.Statements.Add(indexAssignNode);

            // 10 For-loop
            var forLoop = new ForLoopNode(indexAssign, loopCond, loopStep, loopBody);

            // 11 Sequence of statements
            var program = new SequenceNode();
            program.Statements.Add(srcAssign);
            program.Statements.Add(resultAssign);
            program.Statements.Add(forLoop);
            program.Statements.Add(new IdNode(resultVarName)); // return result

            PerformSemanticAnalysis(program);
            return VisitSequence(program);
        }

        public LLVMValueRef VisitCopy(CopyNode expr)
        {
            var value = Visit(expr.SourceExpression); // LLVM value
            var type = expr.SourceExpression.Type;    // language type

            var elementType = ((ArrayType)expr.SourceExpression.Type).ElementType;

            if (type is ArrayType)
                return CopyArray(value, elementType);

            if (type is RecordType t)
                return CopyRecord(value, t);

            throw new Exception($"Cannot call copy on type {type}");
        }

        public LLVMValueRef VisitCopyArray(CopyArrayNode expr)
        {
            return VisitCopy(expr);
        }

        public LLVMValueRef VisitCopyRecord(CopyNode expr)
        {
            return VisitCopy(expr);
        }

        private LLVMValueRef CopyArray(LLVMValueRef srcHeaderPtr, Type elementType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var mallocFunc = GetOrDeclareMalloc();

            // 1. Load Length and Data Pointer from Source Header
            var srcLenPtr = _builder.BuildStructGEP2(_arrayStruct, srcHeaderPtr, 0, "src_len_ptr");
            var srcDataPtrField = _builder.BuildStructGEP2(_arrayStruct, srcHeaderPtr, 2, "src_data_ptr_field");

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
            var dstLenPtr = _builder.BuildStructGEP2(_arrayStruct, newHeaderPtr, 0);
            var dstCapPtr = _builder.BuildStructGEP2(_arrayStruct, newHeaderPtr, 1);
            var dstDataPtrField = _builder.BuildStructGEP2(_arrayStruct, newHeaderPtr, 2);

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

        public LLVMValueRef VisitMapExprMutating(MapNode expr) // not in use, maybe use with like an argument, eg: x.map(d => d+2, true) 
        {
            // Temp variable names
            var srcVarName = "__map_src";
            var indexVarName = "__map_i";

            // 1. Store source array
            var srcAssign = new AssignNode(srcVarName, expr.SourceExpression);

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
            var loopStep = new IncrementNode(new IdNode(indexVarName));

            // 5. src[i]
            var currentElement = new IndexNode(
                new IdNode(srcVarName),
                new IdNode(indexVarName)
            );

            // 6. Replace iterator (d => ...) with actual element
            var mapped = ReplaceIterator(
                expr.Assignment,
                expr.IteratorId.Name,
                currentElement
            );

            // 7. src[i] = mapped
            var indexAssignNode = new IndexAssignNode(
                new IdNode(srcVarName),
                new IdNode(indexVarName),
                mapped
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

            // 1. Base Case: The ID itself (e.g., x)
            if (node is IdNode id && id.Name == iteratorName)
            {
                return replacement;
            }

            // 2. FieldNode (Handles x.columnName)
            if (node is FieldNode rf)
            {
                var newSrc = ReplaceIterator(rf.SourceExpression, iteratorName, replacement);
                var newRf = new FieldNode(newSrc, rf.IdField);
                newRf.SetType(rf.Type);
                return newRf;
            }

            // 3. IndexAssignNode (e.g., x[0] = 10) - Since this is an ExpressionNode in your AST
            if (node is IndexAssignNode ian)
            {
                var newArr = ReplaceIterator(ian.ArrayExpression, iteratorName, replacement);
                var newIdx = ReplaceIterator(ian.IndexExpression, iteratorName, replacement);
                var newAss = ReplaceIterator(ian.AssignExpression, iteratorName, replacement);
                var newNode = new IndexAssignNode(newArr, newIdx, newAss);
                newNode.SetType(ian.Type);
                return newNode;
            }

            // 4. NamedArgumentNode
            if (node is NamedArgumentNode arg)
            {
                var newValue = ReplaceIterator(arg.Value, iteratorName, replacement);
                var newArg = new NamedArgumentNode(arg.Name, newValue);
                newArg.SetType(arg.Type);
                return newArg;
            }

            // 5. RecordNode
            if (node is RecordNode rec)
            {
                var newFields = rec.Fields.Select(f =>
                {
                    var val = ReplaceIterator(f.Value, iteratorName, replacement);
                    var argNode = new NamedArgumentNode(f.Label, val);
                    argNode.SetType(f.Type);
                    return argNode;
                }).ToList();
                var newRec = new RecordNode(newFields);
                newRec.SetType(rec.Type);
                return newRec;
            }

            // 6. Comparison
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

            // 7. Binary Operations (Handles x.longitude - 100.0)
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

            // 8. Indexing (Read access: x[0])
            if (node is IndexNode idx)
            {
                var nSrc = ReplaceIterator(idx.SourceExpression, iteratorName, replacement);
                var nIdx = ReplaceIterator(idx.IndexExpression, iteratorName, replacement);
                var newNode = new IndexNode(nSrc, nIdx);
                newNode.SetType(idx.Type);
                return newNode;
            }

            // 9. Unary
            if (node is UnaryOpNode un)
            {
                var newUn = new UnaryOpNode(un.Operator, ReplaceIterator(un.Operand, iteratorName, replacement));
                newUn.SetType(un.Type);
                return newUn;
            }

            // 10. Logical Operations
            if (node is LogicalOpNode log)
            {
                var newLog = new LogicalOpNode(
                    ReplaceIterator(log.Left, iteratorName, replacement),
                    log.Operator,
                    ReplaceIterator(log.Right, iteratorName, replacement)
                );
                newLog.SetType(log.Type);
                return newLog;
            }

            // 11. Function Calls (random(x.a, x.b))
            if (node is RandomNode ran)
            {
                var newArgs = ran.Arguments.Select(a => ReplaceIterator(a, iteratorName, replacement)).ToList();
                return new RandomNode(newArgs);
            }

            return node;
        }
        ulong GetSize(LLVMTypeRef type)
        {
            var ctx = _module.Context;

            if (type == ctx.Int8Type) return 1;
            if (type == ctx.Int64Type) return 8;
            if (type == ctx.DoubleType) return 8;

            // everything else is pointer in your runtime
            return 8;
        }

        public LLVMValueRef VisitArray(ArrayNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;

            uint count = (uint)expr.Elements.Count;

            var elemType = expr.ElementType;
            Console.WriteLine("in array and the array size is: " + count);
            var llvmElemType = GetLLVMType(elemType);

            var mallocFunc = GetOrDeclareMalloc();

            // 1. Header (24 bytes)
            var headerRaw = _builder.BuildCall2(
                _mallocType,
                mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, 24) },
                "arr_header"
            );

            // 2. Compute element size
            var elementSize = GetSize(llvmElemType);

            var capacity = count > 0 ? count : 100;

            var dataSize = LLVMValueRef.CreateConstInt(
                i64,
                capacity * elementSize
            );

            var dataRaw = _builder.BuildCall2(
                _mallocType,
                mallocFunc,
                new[] { dataSize },
                "arr_data"
            );

            // 3. Store metadata
            var lenPtr = _builder.BuildStructGEP2(_arrayStruct, headerRaw, 0);
            var capPtr = _builder.BuildStructGEP2(_arrayStruct, headerRaw, 1);
            var dataPtrPtr = _builder.BuildStructGEP2(_arrayStruct, headerRaw, 2);

            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, count), lenPtr).SetAlignment(8);
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, capacity), capPtr).SetAlignment(8);

            // IMPORTANT: store raw pointer (NOT i8*)
            _builder.BuildStore(dataRaw, dataPtrPtr).SetAlignment(8);

            // 4. Populate
            for (int i = 0; i < expr.Elements.Count; i++)
            {
                var val = Visit(expr.Elements[i]);
                var idx = LLVMValueRef.CreateConstInt(i64, (ulong)i);
                var elementPtr = _builder.BuildGEP2(llvmElemType, dataRaw, new[] { idx }, "elem_ptr");
                _builder.BuildStore(val, elementPtr).SetAlignment((uint)elementSize);
            }

            return headerRaw;
        }

        public LLVMValueRef VisitIndex(IndexNode expr)
        {
            Console.WriteLine("VISIT: " + expr.GetType().Name + " " + expr);
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var func = _builder.InsertBlock.Parent;

            var sourceType = expr.SourceExpression.Type;
            var indexVal = Visit(expr.IndexExpression);

            // normalize index
            if (indexVal.TypeOf != i64)
                indexVal = _builder.BuildIntCast(indexVal, i64, "idx_cast");

            var headerPtr = Visit(expr.SourceExpression);

            // =========================
            // ARRAY INDEXING
            // =========================
            if (sourceType is ArrayType arrType)
            {
                var elemType = arrType.ElementType;

                var lenPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 0);
                var dataPtrPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 2);

                var len = _builder.BuildLoad2(ctx.Int64Type, lenPtr);
                var dataPtr = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), dataPtrPtr);

                if (indexVal.TypeOf != ctx.Int64Type)
                    indexVal = _builder.BuildIntCast(indexVal, ctx.Int64Type);

                // bounds check
                var inBounds = _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntULT,
                    indexVal,
                    len
                );

                var okBlock = ctx.AppendBasicBlock(func, "arr_ok");
                var errBlock = ctx.AppendBasicBlock(func, "arr_err");
                var mergeBlock = ctx.AppendBasicBlock(func, "arr_merge");

                _builder.BuildCondBr(inBounds, okBlock, errBlock);

                // ERROR
                _builder.PositionAtEnd(errBlock);
                _builder.BuildCall2(_printfType, _printf,
                    new[] { _builder.BuildGlobalStringPtr("OOB\n") });
                _builder.BuildBr(mergeBlock);

                var errBB = _builder.InsertBlock;

                // OK
                _builder.PositionAtEnd(okBlock);

                LLVMTypeRef elemLLVMType = elemType switch
                {
                    IntType => ctx.Int64Type,
                    FloatType => ctx.DoubleType,
                    BoolType => ctx.Int8Type,
                    _ => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0)
                };

                var typedPtr = _builder.BuildBitCast(
                    dataPtr,
                    LLVMTypeRef.CreatePointer(elemLLVMType, 0)
                );

                var elemPtr = _builder.BuildGEP2(elemLLVMType, typedPtr, new[] { indexVal });

                LLVMValueRef value;

                if (elemType is IntType)
                    value = _builder.BuildLoad2(ctx.Int64Type, elemPtr);

                else if (elemType is FloatType)
                    value = _builder.BuildLoad2(ctx.DoubleType, elemPtr);

                else if (elemType is BoolType)
                    value = _builder.BuildLoad2(ctx.Int8Type, elemPtr);

                else
                    value = _builder.BuildLoad2(i8Ptr, elemPtr);

                _builder.BuildBr(mergeBlock);

                var okBB = _builder.InsertBlock;

                // MERGE
                _builder.PositionAtEnd(mergeBlock);

                var phi = _builder.BuildPhi(value.TypeOf, "arr_val");

                phi.AddIncoming(
                    new[] { LLVMValueRef.CreateConstNull(value.TypeOf), value },
                    new[] { errBB, okBB },
                    2
                );

                return phi;
            }

            if (sourceType is DataframeType dfType)
            {
                var result = DataframeIndex(headerPtr, indexVal, dfType.RowType);
                return result;
            }

            return LLVMValueRef.CreateConstNull(i8Ptr);
        }

        // public SequenceNode ColumnAccessForDataframe(FieldNode indexNode)
        // {
        //     var dfType = indexNode.SourceExpression.Type as DataframeType
        //         ?? throw new Exception("Expected dataframe type");

        //     var columnName = indexNode.IdField;

        //     var fieldType = default(Type);
        //     foreach (var item in dfType.RowType.RecordFields)
        //     {
        //         if (item.Label == columnName)
        //         {
        //             fieldType = item.Type;
        //             break;
        //         }
        //     }

        //     // Variables
        //     var srcVar = "__col_src";
        //     var resultVar = "__col_result";
        //     var iVar = "__col_i";
        //     var lenVar = "__col_len";
        //     var rowVar = "__col_row";

        //     // 1. result = []
        //     var resultConstructor = new ArrayNode(new List<ExpressionNode>())
        //     {
        //         ElementType = fieldType
        //     };
        //     resultConstructor.SetType(new ArrayType(fieldType));

        //     // 2. Assignments
        //     var srcAssign = new AssignNode(srcVar, new IdNode(((IdNode)indexNode.SourceExpression).Name));
        //     var resultAssign = new AssignNode(resultVar, resultConstructor);
        //     var indexInit = new AssignNode(iVar, new NumberNode(0));
        //     var lenAssign = new AssignNode(lenVar, new LengthNode(new IdNode(srcVar)));

        //     // 3. Loop condition & step
        //     var cond = new ComparisonNode(new IdNode(iVar), "<", new IdNode(lenVar));
        //     var step = new IncrementNode(new IdNode(iVar));

        //     var loopBody = new SequenceNode();

        //     // row = src[i]
        //     var rowAccess = new IndexNode(new IdNode(srcVar), new IdNode(iVar));
        //     rowAccess.SetType(dfType.RowType);

        //     var rowAssign = new AssignNode(rowVar, rowAccess);
        //     loopBody.Statements.Add(rowAssign);

        //     // value = row[columnIndex]
        //     var valueExpr = new FieldNode(new IdNode(rowVar), columnName);

        //     valueExpr.SetType(fieldType);

        //     // result.add(value)
        //     var addNode = new AddNode(new IdNode(resultVar), valueExpr);
        //     loopBody.Statements.Add(addNode);

        //     var loop = new ForLoopNode(indexInit, cond, step, loopBody);

        //     // 4. Program
        //     var program = new SequenceNode();
        //     program.Statements.Add(srcAssign);
        //     program.Statements.Add(resultAssign);
        //     program.Statements.Add(lenAssign);
        //     program.Statements.Add(loop);
        //     program.Statements.Add(new IdNode(resultVar));

        //     return program;
        // }

        private LLVMTypeRef _dfType;
        private LLVMTypeRef GetOrCreateDataframeType()
        {
            if (_dfType.Handle != IntPtr.Zero)
                return _dfType;

            var ctx = _module.Context;

            var arrayType = GetOrCreateArrayType();
            var arrayPtr = LLVMTypeRef.CreatePointer(arrayType, 0);

            _dfType = ctx.CreateNamedStruct("dataframe");

            _dfType.StructSetBody(new[]
            {
                arrayPtr,
                arrayPtr,
                arrayPtr,
                ctx.Int64Type,
            }, false);

            return _dfType;
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

        private LLVMValueRef DataframeIndex(LLVMValueRef dataframePtr, LLVMValueRef indexValue, RecordType rowType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            var i8PtrPtr = LLVMTypeRef.CreatePointer(i8Ptr, 0); // Pointer to pointers

            var dfType = GetOrCreateDataframeType();
            var arrayType = _arrayStruct; // Use the global struct definition
            var structType = GetOrCreateStructType(rowType);

            // 1. Normalize index to i64
            if (indexValue.TypeOf != i64)
                indexValue = _builder.BuildIntCast(indexValue, i64, "idx_cast");

            // 2. Load the data_ptrs array (the array containing pointers to column data)
            // In DataframeObject: index 1 is dataPointersData
            var dataPtrsPtr = _builder.BuildStructGEP2(dfType, dataframePtr, 1, "data_ptrs_ptr");
            var dataPtrsArrayHeader = _builder.BuildLoad2(i8Ptr, dataPtrsPtr, "header_ptr");

            // 3. Access the raw buffer inside the ArrayObject (index 2 is data)
            var dataPtrsDataPtr = _builder.BuildStructGEP2(arrayType, dataPtrsArrayHeader, 2, "data_ptrs_data_ptr");
            var dataPtrsRaw = _builder.BuildLoad2(i8Ptr, dataPtrsDataPtr, "data_ptrs_raw");

            // CRITICAL FIX: Cast i8* to i8** so GEP moves by 8 bytes, not 1 byte
            var dataPtrs = _builder.BuildBitCast(dataPtrsRaw, i8PtrPtr, "data_ptrs_typed");

            // 4. Allocate result row
            var rowPtr = _builder.BuildMalloc(structType, "row");

            for (int i = 0; i < rowType.RecordFields.Count; i++)
            {
                var fieldType = rowType.RecordFields[i].Type;
                var colIndex = LLVMValueRef.CreateConstInt(i64, (ulong)i);

                // Get the specific column's ArrayObject pointer
                var colPtrPtr = _builder.BuildGEP2(i8Ptr, dataPtrs, new[] { colIndex }, "col_ptr_ptr");
                var colArrayHeader = _builder.BuildLoad2(i8Ptr, colPtrPtr, "col_array_header");

                // Load the actual data buffer from the column's ArrayObject
                var colDataPtrPtr = _builder.BuildStructGEP2(arrayType, colArrayHeader, 2, "col_data_ptr_ptr");
                var colDataRaw = _builder.BuildLoad2(i8Ptr, colDataPtrPtr, "col_data_raw");

                LLVMValueRef value;

                // 5. Type Dispatch and Loading
                if (fieldType is IntType)
                {
                    var typed = _builder.BuildBitCast(colDataRaw, LLVMTypeRef.CreatePointer(i64, 0), "int_col");
                    var elemPtr = _builder.BuildGEP2(i64, typed, new[] { indexValue }, "elem_ptr");
                    value = _builder.BuildLoad2(i64, elemPtr, "val");
                }
                else if (fieldType is FloatType)
                {
                    var typed = _builder.BuildBitCast(colDataRaw, LLVMTypeRef.CreatePointer(ctx.DoubleType, 0), "dbl_col");
                    var elemPtr = _builder.BuildGEP2(ctx.DoubleType, typed, new[] { indexValue }, "elem_ptr");
                    value = _builder.BuildLoad2(ctx.DoubleType, elemPtr, "val");
                }
                else if (fieldType is BoolType)
                {
                    var typed = _builder.BuildBitCast(colDataRaw, LLVMTypeRef.CreatePointer(i8, 0), "bool_col");
                    var elemPtr = _builder.BuildGEP2(i8, typed, new[] { indexValue }, "elem_ptr");
                    var raw = _builder.BuildLoad2(i8, elemPtr, "raw");
                    value = _builder.BuildTrunc(raw, ctx.Int8Type, "bool");
                }
                else if (fieldType is StringType)
                {
                    // Strings are i8**, so we cast buffer to i8***
                    var typed = _builder.BuildBitCast(colDataRaw, i8PtrPtr, "str_col");
                    var elemPtr = _builder.BuildGEP2(i8Ptr, typed, new[] { indexValue }, "elem_ptr");
                    value = _builder.BuildLoad2(i8Ptr, elemPtr, "val");
                }
                else
                {
                    throw new Exception($"Unsupported type: {fieldType}");
                }

                // 6. Store into row struct
                var fieldPtr = _builder.BuildStructGEP2(structType, rowPtr, (uint)i, "field_ptr");
                _builder.BuildStore(value, fieldPtr);
            }

            return rowPtr;
        }

        private LLVMTypeRef GetOrCreateStructType(RecordType recordType)
        {
            string structName = "struct_" + string.Join("_", recordType.RecordFields.Select(f => f.Label));

            var structType = _module.GetTypeByName(structName);

            System.Console.WriteLine("get or create struct");

            if (structType.Handle == IntPtr.Zero)
            {
                var fieldTypes = recordType.RecordFields
                    .Select(f => GetLLVMType(f.Type))
                    .ToArray();

                structType = _module.Context.CreateNamedStruct(structName);
                structType.StructSetBody(fieldTypes, false);
            }
            return structType;
        }

        private LLVMTypeRef GetLLVMType(Type type) // zzz
        {
            var ctx = _module.Context;
            return type switch
            {
                FloatType => ctx.DoubleType,
                IntType => ctx.Int64Type,
                BoolType => ctx.Int8Type,
                NullType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), // Represent Null as i8*
                StringType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), // Strings are pointers
                ArrayType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0),  // Arrays  are pointers
                DataframeType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0),  // Dataframes are pointers
                RecordType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0),
                _ => throw new Exception($"Unsupported type getllvm: {type}")
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

        public LLVMValueRef VisitAdd(AddNode expr)
        {
            // Evaluate RHS once
            var valueToAdd = Visit(expr.AddExpression);

            var isBool = expr.AddExpression.Type is BoolType;

            if (expr.SourceExpression.Type is DataframeType)
            {
                return VisitDataframeAdd(expr, valueToAdd);
            }

            return VisitArrayAdd(expr.SourceExpression, valueToAdd, isBool);
        }

        private LLVMValueRef VisitArrayAdd(
            ExpressionNode sourceExpr,
            LLVMValueRef valueToAdd,
            bool isBool)
        {
            var ctx = _module.Context;

            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var arrayPtrType = LLVMTypeRef.CreatePointer(_arrayStruct, 0);

            // =====================================================
            // LOAD ARRAY
            // =====================================================

            var arrayRaw = Visit(sourceExpr);

            var array = _builder.BuildBitCast(
                arrayRaw,
                arrayPtrType,
                "array_cast"
            );

            var lenPtr =
                _builder.BuildStructGEP2(_arrayStruct, array, 0, "len_ptr");

            var capPtr =
                _builder.BuildStructGEP2(_arrayStruct, array, 1, "cap_ptr");

            var dataPtrPtr =
                _builder.BuildStructGEP2(_arrayStruct, array, 2, "data_ptr_ptr");

            var length =
                _builder.BuildLoad2(i64, lenPtr, "len");

            var capacity =
                _builder.BuildLoad2(i64, capPtr, "cap");

            var dataPtr =
                _builder.BuildLoad2(i8Ptr, dataPtrPtr, "data_ptr");

            // =====================================================
            // GROW CHECK
            // =====================================================

            var isFull = _builder.BuildICmp(
                LLVMIntPredicate.LLVMIntEQ,
                length,
                capacity,
                "is_full"
            );

            var function = _builder.InsertBlock.Parent;

            var growBlock =
                ctx.AppendBasicBlock(function, "array_grow");

            var contBlock =
                ctx.AppendBasicBlock(function, "array_cont");

            var startBlock = _builder.InsertBlock;

            _builder.BuildCondBr(isFull, growBlock, contBlock);

            // =====================================================
            // GROW BLOCK
            // =====================================================

            _builder.PositionAtEnd(growBlock);

            var zero = LLVMValueRef.CreateConstInt(i64, 0);
            var four = LLVMValueRef.CreateConstInt(i64, 4);
            var two = LLVMValueRef.CreateConstInt(i64, 2);

            var newCap = _builder.BuildSelect(
                _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntEQ,
                    capacity,
                    zero
                ),
                four,
                _builder.BuildMul(capacity, two),
                "new_cap"
            );



            var stride =
                LLVMValueRef.CreateConstInt(i64, GetTypeSize(sourceExpr.Type));

            var newSize =
                _builder.BuildMul(newCap, stride, "new_size");

            var realloc = GetOrDeclareRealloc();

            var newDataPtr = _builder.BuildCall2(
                _reallocType,
                realloc,
                new[] { dataPtr, newSize },
                "realloc"
            );

            _builder.BuildStore(newCap, capPtr);
            _builder.BuildStore(newDataPtr, dataPtrPtr);

            _builder.BuildBr(contBlock);

            // =====================================================
            // CONT BLOCK
            // =====================================================

            _builder.PositionAtEnd(contBlock);

            var finalDataPtr =
                _builder.BuildPhi(i8Ptr, "final_data");

            finalDataPtr.AddIncoming(
                new[] { dataPtr, newDataPtr },
                new[] { startBlock, growBlock },
                2
            );

            // =====================================================
            // WRITE ELEMENT
            // =====================================================

            LLVMTypeRef elementType;
            LLVMValueRef typedDataPtr;
            LLVMValueRef storedValue;

            if (isBool)
            {
                elementType = i8;

                typedDataPtr = finalDataPtr;

                storedValue =
                    _builder.BuildZExt(valueToAdd, i8, "bool_to_i8");
            }
            else
            {
                elementType = i8Ptr;

                typedDataPtr = _builder.BuildBitCast(
                    finalDataPtr,
                    LLVMTypeRef.CreatePointer(i8Ptr, 0),
                    "typed_array"
                );

                storedValue =
                    _builder.BuildBitCast(valueToAdd, i8Ptr, "val_to_ptr");
            }

            var targetPtr = _builder.BuildGEP2(
                elementType,
                typedDataPtr,
                new[] { length },
                "target_ptr"
            );

            _builder.BuildStore(storedValue, targetPtr);

            // =====================================================
            // UPDATE LENGTH
            // =====================================================

            var one = LLVMValueRef.CreateConstInt(i64, 1);

            var newLen =
                _builder.BuildAdd(length, one, "new_len");

            _builder.BuildStore(newLen, lenPtr);

            return arrayRaw;
        }

        public LLVMValueRef VisitDataframeAdd(
            AddNode expr,
            LLVMValueRef rowValue)
        {
            var ctx = _module.Context;

            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var dfType = GetOrCreateDataframeType();

            var dfPtrType =
                LLVMTypeRef.CreatePointer(dfType, 0);

            var arrayPtrType =
                LLVMTypeRef.CreatePointer(_arrayStruct, 0);

            // =====================================================
            // LOAD DATAFRAME
            // =====================================================

            var dfRaw = Visit(expr.SourceExpression);

            var df = _builder.BuildBitCast(
                dfRaw,
                dfPtrType,
                "df_cast"
            );

            // dataframe.data
            var dataArrayPtrPtr =
                _builder.BuildStructGEP2(dfType, df, 1);

            var dataArrayRaw =
                _builder.BuildLoad2(i8Ptr, dataArrayPtrPtr);

            var dataArray =
                _builder.BuildBitCast(
                    dataArrayRaw,
                    arrayPtrType,
                    "data_array"
                );

            // array.data
            var columnPtrsPtr =
                _builder.BuildStructGEP2(_arrayStruct, dataArray, 2);

            var columnPtrsRaw =
                _builder.BuildLoad2(i8Ptr, columnPtrsPtr);

            // =====================================================
            // APPEND ROW VALUES TO EACH COLUMN
            // =====================================================

            var rowType = (RecordType)expr.AddExpression.Type;

            var llvmRowStruct =
                GetOrCreateStructType(rowType);

            for (int i = 0; i < rowType.RecordFields.Count; i++)
            {
                var field = rowType.RecordFields[i];

                var fieldPtr = _builder.BuildStructGEP2(
                    llvmRowStruct,
                    rowValue,
                    (uint)i
                );

                var llvmFieldType =
                    GetLLVMType(field.Type);

                var fieldValue =
                    _builder.BuildLoad2(
                        llvmFieldType,
                        fieldPtr
                    );

                var colIdx =
                    LLVMValueRef.CreateConstInt(i64, (ulong)i);

                // column pointer array is i8**
                var colPtrPtr = _builder.BuildGEP2(
                    i8Ptr,
                    columnPtrsRaw,
                    new[] { colIdx },
                    "col_ptr_ptr"
                );

                var colRaw =
                    _builder.BuildLoad2(i8Ptr, colPtrPtr);

                AppendToArrayWithGrow(colRaw, fieldValue, llvmFieldType, field.Type);
            }

            // =====================================================
            // UPDATE DATAFRAME ROW COUNT
            // =====================================================

            var rowCountPtr =
                _builder.BuildStructGEP2(dfType, df, 3);

            var rowCount =
                _builder.BuildLoad2(i64, rowCountPtr);

            var one =
                LLVMValueRef.CreateConstInt(i64, 1);

            var newCount =
                _builder.BuildAdd(rowCount, one);

            _builder.BuildStore(newCount, rowCountPtr).SetAlignment(8);

            return df;
        }


        public LLVMValueRef VisitDataframeAdd1(AddNode expr)
        {
            var ctx = _module.Context;

            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var dfType = GetOrCreateDataframeType();
            var dfPtrType = LLVMTypeRef.CreatePointer(dfType, 0);
            var arrayPtrType = LLVMTypeRef.CreatePointer(_arrayStruct, 0);

            // -----------------------
            // LOAD DATAFRAME
            // -----------------------
            var dfRaw = Visit(expr.SourceExpression);
            var df = _builder.BuildBitCast(dfRaw, dfPtrType, "df");

            // dataframe.data (column pointer array)
            var dataArrayPtrPtr = _builder.BuildStructGEP2(dfType, df, 1);
            var dataArrayRaw = _builder.BuildLoad2(i8Ptr, dataArrayPtrPtr, "data_raw");
            var dataArray = _builder.BuildBitCast(dataArrayRaw, arrayPtrType, "data_array");

            // row count (shared across all columns)
            var rowCountPtr = _builder.BuildStructGEP2(_arrayStruct, dataArray, 0);
            var rowIndex = _builder.BuildLoad2(i64, rowCountPtr, "row_idx");

            // column pointer array
            var dataPtrPtr = _builder.BuildStructGEP2(_arrayStruct, dataArray, 2);
            var columnPtrsRaw = _builder.BuildLoad2(i8Ptr, dataPtrPtr, "column_ptrs_raw");

            // -----------------------
            // EVALUATE ROW (ONLY ONCE)
            // -----------------------
            var row = Visit(expr.AddExpression);

            var rowType = expr.AddExpression.Type as RecordType;
            if (rowType == null)
                throw new Exception("Dataframe add requires record");

            var llvmRowStruct = GetOrCreateStructType(rowType);

            // -----------------------
            // WRITE EACH FIELD INTO COLUMN
            // -----------------------
            for (int i = 0; i < rowType.RecordFields.Count; i++)
            {
                var field = rowType.RecordFields[i];

                var fieldPtr = _builder.BuildStructGEP2(
                    llvmRowStruct,
                    row,
                    (uint)i
                );

                LLVMTypeRef fieldLLVMType;

                switch (field.Type)
                {
                    case IntType:
                        fieldLLVMType = i64;
                        break;

                    case FloatType:
                        fieldLLVMType = ctx.DoubleType;
                        break;

                    case BoolType:
                        fieldLLVMType = ctx.Int1Type;
                        break;

                    case StringType:
                    default:
                        fieldLLVMType = i8Ptr;
                        break;
                }

                _builder.BuildLoad2(fieldLLVMType, fieldPtr);

                // get column i pointer
                var colIdx = LLVMValueRef.CreateConstInt(i64, (ulong)i);

                var colPtrPtr = _builder.BuildGEP2(
                    i8Ptr,
                    columnPtrsRaw,
                    new[] { colIdx },
                    "col_ptr_ptr"
                );

                var colRaw = _builder.BuildLoad2(i8Ptr, colPtrPtr);

                //  SINGLE SOURCE OF TRUTH
                //AppendToArrayWithGrow(colRaw, fieldValue, fieldLLVMType);
            }

            // -----------------------
            // INCREMENT ROW COUNT ONCE
            // -----------------------
            var one = LLVMValueRef.CreateConstInt(i64, 1);
            var newRowCount = _builder.BuildAdd(rowIndex, one, "row_inc");

            _builder.BuildStore(newRowCount, rowCountPtr);

            return df;
        }

        private ulong GetTypeSize(Type type)
        {
            switch (type)
            {
                case BoolType:
                    return 1;

                case IntType:
                    return 8;

                case FloatType:
                    return 8;

                case StringType:
                    return (ulong)IntPtr.Size;

                case ArrayType:
                    return (ulong)IntPtr.Size;

                case DataframeType:
                    return (ulong)IntPtr.Size;

                case RecordType:
                    return (ulong)IntPtr.Size;

                default:
                    return (ulong)IntPtr.Size;
            }
        }

        private void AppendToArrayWithGrow(
         LLVMValueRef arrayPtr,
         LLVMValueRef value,
         LLVMTypeRef elementType,
         Type semanticType)
        {
            var ctx = _module.Context;

            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            // =====================================================
            // HEADER FIELDS
            // =====================================================

            var lenPtr =
                _builder.BuildStructGEP2(
                    _arrayStruct,
                    arrayPtr,
                    0,
                    "len_ptr"
                );

            var capPtr =
                _builder.BuildStructGEP2(
                    _arrayStruct,
                    arrayPtr,
                    1,
                    "cap_ptr"
                );

            var dataPtrPtr =
                _builder.BuildStructGEP2(
                    _arrayStruct,
                    arrayPtr,
                    2,
                    "data_ptr_ptr"
                );

            // =====================================================
            // LOAD VALUES
            // =====================================================

            var len =
                _builder.BuildLoad2(
                    i64,
                    lenPtr,
                    "curr_len"
                );

            var cap =
                _builder.BuildLoad2(
                    i64,
                    capPtr,
                    "curr_cap"
                );

            var dataPtr =
                _builder.BuildLoad2(
                    i8Ptr,
                    dataPtrPtr,
                    "curr_data"
                );

            // =====================================================
            // GROW CHECK
            // =====================================================

            var needsGrow =
                _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntSGE,
                    len,
                    cap,
                    "needs_grow"
                );

            var currentBB = _builder.InsertBlock;
            var parentFunc = currentBB.Parent;

            var growBB =
                parentFunc.AppendBasicBlock("grow");

            var storeBB =
                parentFunc.AppendBasicBlock("store_element");

            _builder.BuildCondBr(
                needsGrow,
                growBB,
                storeBB
            );

            // =====================================================
            // GROW BLOCK
            // =====================================================

            _builder.PositionAtEnd(growBB);

            var zero =
                LLVMValueRef.CreateConstInt(i64, 0);

            var four =
                LLVMValueRef.CreateConstInt(i64, 4);

            var two =
                LLVMValueRef.CreateConstInt(i64, 2);

            var isZeroCap =
                _builder.BuildICmp(
                    LLVMIntPredicate.LLVMIntEQ,
                    cap,
                    zero
                );

            var doubledCap =
                _builder.BuildMul(cap, two);

            var newCap =
                _builder.BuildSelect(
                    isZeroCap,
                    four,
                    doubledCap,
                    "new_cap"
                );

            // =====================================================
            // REAL ELEMENT SIZE
            // =====================================================

            ulong elementSize = GetTypeSize(semanticType);



            var stride =
                LLVMValueRef.CreateConstInt(
                    i64,
                    elementSize
                );

            var newByteCount =
                _builder.BuildMul(
                    newCap,
                    stride,
                    "new_byte_count"
                );

            var reallocFunc =
                GetOrDeclareRealloc();

            var reallocType =
                LLVMTypeRef.CreateFunction(
                    i8Ptr,
                    new[] { i8Ptr, i64 }
                );

            var newDataPtr =
                _builder.BuildCall2(
                    reallocType,
                    reallocFunc,
                    new[] { dataPtr, newByteCount },
                    "reallocated_data"
                );

            _builder.BuildStore(newCap, capPtr).SetAlignment(8);
            _builder.BuildStore(newDataPtr, dataPtrPtr).SetAlignment(8);

            _builder.BuildBr(storeBB);

            // =====================================================
            // STORE BLOCK
            // =====================================================

            _builder.PositionAtEnd(storeBB);

            var finalDataPtr =
                _builder.BuildLoad2(
                    i8Ptr,
                    dataPtrPtr,
                    "final_data"
                );

            // byte offset
            var offset =
                _builder.BuildMul(
                    len,
                    stride,
                    "offset"
                );

            // raw byte ptr
            var rawElementPtr =
                _builder.BuildGEP2(
                    i8,
                    finalDataPtr,
                    new[] { offset },
                    "raw_elem_ptr"
                );

            // cast to correct type*
            var typedElementPtr =
                _builder.BuildBitCast(
                    rawElementPtr,
                    LLVMTypeRef.CreatePointer(elementType, 0),
                    "typed_elem_ptr"
                );

            // store actual value
            _builder.BuildStore(
                value,
                typedElementPtr
            ).SetAlignment(8);

            // =====================================================
            // INCREMENT LENGTH
            // =====================================================

            var one =
                LLVMValueRef.CreateConstInt(i64, 1);

            var newLen =
                _builder.BuildAdd(
                    len,
                    one,
                    "new_len"
                );

            _builder.BuildStore(
                newLen,
                lenPtr
            ).SetAlignment(8);
        }

        private void AppendToArrayWithGrow00(
            LLVMValueRef arrayPtr,
            LLVMValueRef value,
            LLVMTypeRef elementType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 1. Get pointers to struct fields
            var lenPtr = _builder.BuildStructGEP2(_arrayStruct, arrayPtr, 0);
            var capPtr = _builder.BuildStructGEP2(_arrayStruct, arrayPtr, 1);
            var dataPtrPtr = _builder.BuildStructGEP2(_arrayStruct, arrayPtr, 2);

            // 2. Load current values
            var len = _builder.BuildLoad2(i64, lenPtr, "curr_len");
            var cap = _builder.BuildLoad2(i64, capPtr, "curr_cap");
            var dataPtr = _builder.BuildLoad2(i8Ptr, dataPtrPtr, "curr_data");

            // 3. Check if we need to grow (len >= cap)
            var needsGrow = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, len, cap, "needs_grow");

            var currentBB = _builder.InsertBlock;
            var parentFunc = currentBB.Parent;
            var growBB = parentFunc.AppendBasicBlock("grow");
            var storeBB = parentFunc.AppendBasicBlock("store_element");

            _builder.BuildCondBr(needsGrow, growBB, storeBB);

            // --- Grow Block ---
            _builder.PositionAtEnd(growBB);

            // New Capacity = Capacity * 2 (handle case where cap is 0)
            var zero = LLVMValueRef.CreateConstInt(i64, 0);
            var isZeroCap = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, cap, zero);
            var defaultCap = LLVMValueRef.CreateConstInt(i64, 4);
            var doubleCap = _builder.BuildMul(cap, LLVMValueRef.CreateConstInt(i64, 2));
            var newCap = _builder.BuildSelect(isZeroCap, defaultCap, doubleCap, "new_cap");

            // Bytes to allocate = newCap * sizeof(elementType)
            // Note: You can use LLVM's SizeOf if you don't want to hardcode 8
            var typeSize = LLVMValueRef.CreateConstInt(i64, 8);
            var newByteCount = _builder.BuildMul(newCap, typeSize);

            // Call realloc(ptr, newSize)
            var reallocFunc = GetOrDeclareRealloc();
            var newDataPtr = _builder.BuildCall2(
                LLVMTypeRef.CreateFunction(i8Ptr, new[] { i8Ptr, i64 }),
                reallocFunc,
                new[] { dataPtr, newByteCount },
                "reallocated_data"
            );

            // Update struct: capacity = newCap, data = newDataPtr
            _builder.BuildStore(newCap, capPtr);
            _builder.BuildStore(newDataPtr, dataPtrPtr);
            _builder.BuildBr(storeBB);

            // --- Store Block ---
            _builder.PositionAtEnd(storeBB);

            // Re-load data pointer (it might have changed in growBB)
            // We use a PHI node or just re-load from the struct
            var finalDataPtr = _builder.BuildLoad2(i8Ptr, dataPtrPtr, "final_data");
            var typedDataPtr = _builder.BuildBitCast(finalDataPtr, LLVMTypeRef.CreatePointer(elementType, 0));

            var elemPtr = _builder.BuildGEP2(elementType, typedDataPtr, new[] { len }, "elem_dest");
            _builder.BuildStore(value, elemPtr);

            // Increment length
            var newLen = _builder.BuildAdd(len, LLVMValueRef.CreateConstInt(i64, 1));
            _builder.BuildStore(newLen, lenPtr);
        }

        private void AppendToArrayWithGrow0(
     LLVMValueRef arrayPtr,
     LLVMValueRef value,
     bool isBool)
        {
            var ctx = _module.Context;

            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            var lenPtr = _builder.BuildStructGEP2(_arrayStruct, arrayPtr, 0);
            var capPtr = _builder.BuildStructGEP2(_arrayStruct, arrayPtr, 1);
            var dataPtr = _builder.BuildStructGEP2(_arrayStruct, arrayPtr, 2);

            var len = _builder.BuildLoad2(i64, lenPtr);
            var data = _builder.BuildLoad2(i8Ptr, dataPtr);

            // write at len (assumes caller already ensured capacity!)
            var elemPtr = _builder.BuildGEP2(
                i8Ptr,
                data,
                new[] { len },
                "elem_ptr"
            );

            var stored = isBool
                ? _builder.BuildZExt(value, i8Ptr)
                : value;

            _builder.BuildStore(stored, elemPtr);

            var one = LLVMValueRef.CreateConstInt(i64, 1);
            var newLen = _builder.BuildAdd(len, one);

            _builder.BuildStore(newLen, lenPtr);
        }

        private void AppendToArrayWithGrow1(LLVMValueRef arrayHeader, LLVMValueRef value, bool isBool)
        {
            var ctx = _module.Context;

            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var elementType = isBool ? i8 : i8Ptr;

            // -----------------------
            // LOAD HEADER
            // -----------------------
            var lenPtr = _builder.BuildStructGEP2(_arrayStruct, arrayHeader, 0);
            var capPtr = _builder.BuildStructGEP2(_arrayStruct, arrayHeader, 1);
            var dataPtrPtr = _builder.BuildStructGEP2(_arrayStruct, arrayHeader, 2);

            var length = _builder.BuildLoad2(i64, lenPtr, "len");
            var capacity = _builder.BuildLoad2(i64, capPtr, "cap");
            var dataPtr = _builder.BuildLoad2(i8Ptr, dataPtrPtr, "data");

            // -----------------------
            // GROW CHECK
            // -----------------------
            var isFull = _builder.BuildICmp(
                LLVMIntPredicate.LLVMIntEQ,
                length,
                capacity
            );

            var function = _builder.InsertBlock.Parent;
            var growBlock = ctx.AppendBasicBlock(function, "grow");
            var contBlock = ctx.AppendBasicBlock(function, "cont");
            var startBlock = _builder.InsertBlock;

            _builder.BuildCondBr(isFull, growBlock, contBlock);

            // -----------------------
            // GROW BLOCK
            // -----------------------
            _builder.PositionAtEnd(growBlock);

            var zero = LLVMValueRef.CreateConstInt(i64, 0);
            var four = LLVMValueRef.CreateConstInt(i64, 4);
            var two = LLVMValueRef.CreateConstInt(i64, 2);

            var newCap = _builder.BuildSelect(
                _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, capacity, zero),
                four,
                _builder.BuildMul(capacity, two)
            );

            var stride = LLVMValueRef.CreateConstInt(i64, (ulong)(isBool ? 1 : 8));
            var newSize = _builder.BuildMul(newCap, stride);

            var realloc = GetOrDeclareRealloc();

            var newData = _builder.BuildCall2(
                _reallocType,
                realloc,
                new[] { dataPtr, newSize },
                "realloc"
            );

            _builder.BuildStore(newCap, capPtr);
            _builder.BuildStore(newData, dataPtrPtr);

            _builder.BuildBr(contBlock);

            // -----------------------
            // CONT BLOCK
            // -----------------------
            _builder.PositionAtEnd(contBlock);

            var finalData = _builder.BuildPhi(i8Ptr, "final_data");

            finalData.AddIncoming(
                new LLVMValueRef[] { dataPtr, newData },
                new LLVMBasicBlockRef[] { startBlock, growBlock },
                2
            );

            // -----------------------
            // STORE ELEMENT
            // -----------------------
            LLVMValueRef stored = isBool
                ? _builder.BuildZExt(value, i8)
                : _builder.BuildBitCast(value, i8Ptr);

            var targetPtr = _builder.BuildGEP2(
                elementType,
                finalData,
                new[] { length },
                "target"
            );

            _builder.BuildStore(stored, targetPtr);

            // -----------------------
            // UPDATE LENGTH
            // -----------------------
            var one = LLVMValueRef.CreateConstInt(i64, 1);

            var newLen = _builder.BuildAdd(length, one);
            _builder.BuildStore(newLen, lenPtr);
        }

        private void AppendToArrayWithGrow2(LLVMValueRef arrayRawPtr, LLVMValueRef value, LLVMValueRef index, bool isBool)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var arrayPtrType = LLVMTypeRef.CreatePointer(_arrayStruct, 0);
            var array = _builder.BuildBitCast(arrayRawPtr, arrayPtrType, "array_cast");

            // --- Header Access ---
            var lenPtr = _builder.BuildStructGEP2(_arrayStruct, array, 0, "len_ptr");
            var capPtr = _builder.BuildStructGEP2(_arrayStruct, array, 1, "cap_ptr");
            var dataPtrPtr = _builder.BuildStructGEP2(_arrayStruct, array, 2, "data_ptr_ptr");

            var capacity = _builder.BuildLoad2(i64, capPtr, "current_cap");
            var dataPtr = _builder.BuildLoad2(i8Ptr, dataPtrPtr, "current_data");

            // 1. Capture the "Entry" block before we branch
            var entryBlock = _builder.InsertBlock;

            var needGrow = _builder.BuildICmp(LLVMIntPredicate.LLVMIntUGE, index, capacity, "grow_check");

            var function = entryBlock.Parent;
            var growBlock = ctx.AppendBasicBlock(function, "df_col_grow");
            var contBlock = ctx.AppendBasicBlock(function, "df_col_cont");

            _builder.BuildCondBr(needGrow, growBlock, contBlock);

            // --- Grow Block ---
            _builder.PositionAtEnd(growBlock);

            var doubled = _builder.BuildMul(capacity, LLVMValueRef.CreateConstInt(i64, 2), "double_cap");
            var isZero = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, capacity, LLVMValueRef.CreateConstInt(i64, 0));

            var newCapBase = _builder.BuildSelect(isZero, LLVMValueRef.CreateConstInt(i64, 4), doubled, "new_cap_base");

            // Ensure newCap > index
            var finalCap = _builder.BuildSelect(
                _builder.BuildICmp(LLVMIntPredicate.LLVMIntUGT, newCapBase, index),
                newCapBase,
                _builder.BuildAdd(index, LLVMValueRef.CreateConstInt(i64, 1)),
                "final_cap"
            );

            // If bool: 1 byte, else (string/int): 8 bytes
            var stride = LLVMValueRef.CreateConstInt(i64, (ulong)(isBool ? 1 : 8));
            var newSize = _builder.BuildMul(finalCap, stride, "realloc_size");

            var reallocFunc = GetOrDeclareRealloc();
            var newData = _builder.BuildCall2(_reallocType, reallocFunc, new[] { dataPtr, newSize }, "realloc_call");

            _builder.BuildStore(finalCap, capPtr);
            _builder.BuildStore(newData, dataPtrPtr);
            _builder.BuildBr(contBlock);

            // 2. Capture the "Actual" Grow block (it might have changed if you added more logic)
            var actualGrowBlock = _builder.InsertBlock;

            // --- Continue Block ---
            _builder.PositionAtEnd(contBlock);

            // FIX: The PHI node must point to the correct predecessor blocks
            var finalData = _builder.BuildPhi(i8Ptr, "final_data_ptr");
            finalData.AddIncoming(
                new[] { dataPtr, newData },
                new[] { entryBlock, actualGrowBlock },
                2
            );

            // FIX: Element Type for GEP
            // If we are storing a pointer (string) or i64, the GEP element type MUST be i64 or i8Ptr
            // so that LLVM knows to multiply the index by 8.
            var gepElementType = isBool ? i8 : i8Ptr;

            var targetPtr = _builder.BuildGEP2(
                gepElementType,
                finalData,
                new[] { index },
                "target_ptr"
            );

            // Fix: Ensure we handle the value casting correctly for the store
            if (isBool)
            {
                var boolPtr = _builder.BuildBitCast(
                    targetPtr,
                    LLVMTypeRef.CreatePointer(i8, 0),
                    "bool_ptr"
                );

                var boolVal = _builder.BuildZExt(value, i8, "bool_to_i8");

                _builder.BuildStore(boolVal, boolPtr);
            }
            else if (value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                var intPtr = _builder.BuildBitCast(
                    targetPtr,
                    LLVMTypeRef.CreatePointer(i64, 0),
                    "int_ptr"
                );

                _builder.BuildStore(value, intPtr);
            }
            else
            {
                var ptrPtr = _builder.BuildBitCast(
                    targetPtr,
                    LLVMTypeRef.CreatePointer(i8Ptr, 0),
                    "ptr_ptr"
                );

                var casted = _builder.BuildBitCast(value, i8Ptr, "val_cast");

                _builder.BuildStore(casted, ptrPtr);
            }

            // Update length
            var newLen = _builder.BuildAdd(index, LLVMValueRef.CreateConstInt(i64, 1), "new_len");
            _builder.BuildStore(newLen, lenPtr);
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

            var headerPtr = Visit(expr.SourceExpression);
            var indexVal = Visit(expr.RemoveExpression);

            // 1. Determine if this is a packed (boolean) array
            bool isBool = ((ArrayType)expr.SourceExpression.Type).ElementType is BoolType;

            // This is the CRITICAL part:
            // If bool, we tell GEP the elements are 1-byte (i8).
            // If not, we tell GEP the elements are 8-bytes (i8Ptr).
            var gepStrideType = isBool ? i8 : i8Ptr;
            var strideMultiplier = isBool ? 1 : 8;
            var lenFieldPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 0, "len_ptr");
            var dataFieldPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 2, "data_ptr_ptr");
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
                return GetArrayLength(sourcePtr);
            }
            else if (expr.ArrayExpression.Type is DataframeType)
            {
                // Directly access index 3: the i64 rowCount field
                var rowCountPtr = _builder.BuildStructGEP2(_dataframeStruct, sourcePtr, 3, "rowCount_ptr");

                // Load it as an i64
                var length = _builder.BuildLoad2(ctx.Int64Type, rowCountPtr, "rowCount");
                length.SetAlignment(8);

                return length;
            }
            else
                throw new Exception("Length operator is only supported on arrays and dataframes");
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
            var loopStep = new IncrementNode(new IdNode(indexVar));

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

            var loopStep = new IncrementNode(new IdNode(indexVar));
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

            var loopStep = new IncrementNode(new IdNode(indexVar));
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
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;

            // --- OPTIMIZED PATH: EXPLICIT SIMD VECTORIZATION FOR FLOAT ARRAYS ---
            if (expr.ArrayExpression.Type is ArrayType arrType && arrType.ElementType is FloatType)
            {
                // 1. Visit the array expression to get the headerRaw pointer structure address
                LLVMValueRef headerRaw = Visit(expr.ArrayExpression);

                // 2. Extract length and the raw pointer out of your unmanaged ArrayObject fields
                // Field index 0 = length, Field index 2 = data pointer
                var lenPtr = _builder.BuildStructGEP2(_arrayStruct, headerRaw, 0, "len_ptr");
                var dataPtrPtr = _builder.BuildStructGEP2(_arrayStruct, headerRaw, 2, "data_ptr_ptr");

                LLVMValueRef arrayLength = _builder.BuildLoad2(i64, lenPtr, "array_len");
                LLVMValueRef dataRaw = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), dataPtrPtr, "data_raw");

                // 3. Cast data pointer from i8* to a typed double* pointer
                var typedDataPtr = _builder.BuildBitCast(
                    dataRaw,
                    LLVMTypeRef.CreatePointer(ctx.DoubleType, 0),
                    "typed_double_data_ptr"
                );

                // 4. Generate the optimized SIMD basic block logic loops
                return EmitVectorizedArraySum(typedDataPtr, arrayLength);
            }

            // --- FALLBACK PATH: YOUR ORIGINAL SCALAR MACRO REWRITE ---
            // (Kept cleanly intact for Int arrays or any other non-float types)
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

            var loopStep = new IncrementNode(new IdNode(indexVar));
            var currentElement = new IndexNode(new IdNode(arrayVar), new IdNode(indexVar));

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

        public LLVMValueRef EmitVectorizedArraySum(LLVMValueRef typedDataPtr, LLVMValueRef length)
        {
            var ctx = _module.Context;
            var doubleType = ctx.DoubleType;
            var i64 = ctx.Int64Type;
            var i32 = ctx.Int32Type;

            // Vector size configured to 4 elements (Matches a 256-bit wide AVX hardware register)
            uint vectorWidth = 4;
            var vectorType = LLVMTypeRef.CreateVector(doubleType, vectorWidth);
            var vectorPtrType = LLVMTypeRef.CreatePointer(vectorType, 0);

            // 1. Alloca and initialize vector accumulator to zero vectors: [0.0, 0.0, 0.0, 0.0]
            var vecAccumulatorAlloca = _builder.BuildAlloca(vectorType, "vec_accum_alloca");
            var zeroScalar = LLVMValueRef.CreateConstReal(doubleType, 0.0);
            var zeroVector = LLVMValueRef.CreateConstVector(Enumerable.Repeat(zeroScalar, (int)vectorWidth).ToArray());
            _builder.BuildStore(zeroVector, vecAccumulatorAlloca);

            // 2. Track index offset state on stack
            var indexAlloca = _builder.BuildAlloca(i64, "loop_index_alloca");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, 0), indexAlloca);

            // 3. Compute vector limits boundary: endVectorIdx = length - (length % 4)
            var vectorWidthVal = LLVMValueRef.CreateConstInt(i64, vectorWidth);
            var remainder = _builder.BuildURem(length, vectorWidthVal, "vector_loop_rem");
            var endVectorIdx = _builder.BuildSub(length, remainder, "end_vector_idx");

            // 4. Declare our branch blocks
            var func = _builder.InsertBlock.Parent;
            var vecLoopCondBB = ctx.AppendBasicBlock(func, "vec_loop_cond");
            var vecLoopBodyBB = ctx.AppendBasicBlock(func, "vec_loop_body");
            var scalarTailCondBB = ctx.AppendBasicBlock(func, "scalar_tail_cond");
            var scalarTailBodyBB = ctx.AppendBasicBlock(func, "scalar_tail_body");
            var mergeBB = ctx.AppendBasicBlock(func, "reduction_merge");

            _builder.BuildBr(vecLoopCondBB);

            // --- A. VECTOR CONDITION ---
            _builder.PositionAtEnd(vecLoopCondBB);
            var currentIdx = _builder.BuildLoad2(i64, indexAlloca, "current_idx");
            var isVecLoopGo = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, currentIdx, endVectorIdx, "cmp_vec_limit");
            // FIXED: Changed from BuildBrCond to BuildCondBr
            _builder.BuildCondBr(isVecLoopGo, vecLoopBodyBB, scalarTailCondBB);

            // --- B. VECTOR LOOP BODY ---
            _builder.PositionAtEnd(vecLoopBodyBB);

            // GEP calculation gets us a scalar address offset pointer
            var elementScalarPtr = _builder.BuildGEP2(doubleType, typedDataPtr, new[] { currentIdx }, "scalar_base_ptr");

            // Bitcast pointer type from double* to vector-pointer chunk alignment (doubleX4*)
            var vectorizedElementPtr = _builder.BuildBitCast(elementScalarPtr, vectorPtrType, "vector_bitcast_ptr");

            // SIMD Load 4 items into processor registers in a single step
            var loadedVectorChunk = _builder.BuildLoad2(vectorType, vectorizedElementPtr, "loaded_simd_chunk");

            // Vector arithmetic parallel addition instruction
            var pastVecAccum = _builder.BuildLoad2(vectorType, vecAccumulatorAlloca, "past_vec_accum");
            var updatedVecAccum = _builder.BuildFAdd(pastVecAccum, loadedVectorChunk, "simd_fadd_result");
            _builder.BuildStore(updatedVecAccum, vecAccumulatorAlloca);

            // Increment index tracking offset forward by 4
            var nextIdxVal = _builder.BuildAdd(currentIdx, vectorWidthVal, "advanced_idx");
            _builder.BuildStore(nextIdxVal, indexAlloca);
            _builder.BuildBr(vecLoopCondBB);

            // --- C. HORIZONTAL REDUCTION & TAIL SETUP ---
            _builder.PositionAtEnd(scalarTailCondBB);

            var finalScalarSumAlloca = _builder.BuildAlloca(doubleType, "final_scalar_sum_alloca");
            var compiledVectorResult = _builder.BuildLoad2(vectorType, vecAccumulatorAlloca, "compiled_vector_result");

            // Flatten vector lanes out horizontally back into a scalar accumulator register sum
            var horizontalSum = _builder.BuildExtractElement(compiledVectorResult, LLVMValueRef.CreateConstInt(i32, 0), "v_extract_0");
            for (uint i = 1; i < vectorWidth; i++)
            {
                var extractedElement = _builder.BuildExtractElement(compiledVectorResult, LLVMValueRef.CreateConstInt(i32, i), $"v_extract_{i}");
                horizontalSum = _builder.BuildFAdd(horizontalSum, extractedElement, $"h_sum_step_{i}");
            }
            _builder.BuildStore(horizontalSum, finalScalarSumAlloca);

            var postVecIdx = _builder.BuildLoad2(i64, indexAlloca, "post_vec_idx");
            var isTailLoopGo = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, postVecIdx, length, "cmp_tail_limit");
            // FIXED: Changed from BuildBrCond to BuildCondBr
            _builder.BuildCondBr(isTailLoopGo, scalarTailBodyBB, mergeBB);

            // --- D. SCALAR TAIL LOOP (Processes any remainder array items step-by-step) ---
            _builder.PositionAtEnd(scalarTailBodyBB);

            var tailIdx = _builder.BuildLoad2(i64, indexAlloca, "tail_idx");
            var tailScalarPtr = _builder.BuildGEP2(doubleType, typedDataPtr, new[] { tailIdx }, "tail_scalar_ptr");
            var singleScalarElement = _builder.BuildLoad2(doubleType, tailScalarPtr, "single_scalar_element");

            var activeScalarSum = _builder.BuildLoad2(doubleType, finalScalarSumAlloca, "active_scalar_sum");
            var updatedScalarSum = _builder.BuildFAdd(activeScalarSum, singleScalarElement, "scalar_tail_fadd");
            _builder.BuildStore(updatedScalarSum, finalScalarSumAlloca);

            var nextTailIdx = _builder.BuildAdd(tailIdx, LLVMValueRef.CreateConstInt(i64, 1), "next_tail_idx");
            _builder.BuildStore(nextTailIdx, indexAlloca);

            var reCheckTailIdx = _builder.BuildLoad2(i64, indexAlloca, "recheck_tail_idx");
            var isTailLoopStillGo = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, reCheckTailIdx, length, "cmp_tail_recheck");
            // FIXED: Changed from BuildBrCond to BuildCondBr
            _builder.BuildCondBr(isTailLoopStillGo, scalarTailBodyBB, mergeBB);

            // --- E. MERGE OUTPUT BLOCK ---
            _builder.PositionAtEnd(mergeBB);
            return _builder.BuildLoad2(doubleType, finalScalarSumAlloca, "final_vectorized_sum");
        }


        public LLVMValueRef VisitId(IdNode expr)
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
                Console.WriteLine("visiting: " + name.Substring(0, name.Length - 4));
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
            if (expr.ArrayExpression.Type is ArrayType)
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

                // 2. Load Metadata for Bounds Check and Access
                var lenFieldPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 0, "len_ptr");
                var dataFieldPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 2, "data_ptr_ptr");

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
            else if (expr.ArrayExpression.Type is DataframeType dfType)
            {
                return DataframeIndexAssign(expr, dfType);
            }
            throw new Exception("Source is neither array or dataframe");
        }

        private LLVMValueRef DataframeIndexAssign(IndexAssignNode expr, DataframeType dfType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var dfPtr = Visit(expr.ArrayExpression);
            var indexVal = Visit(expr.IndexExpression);
            var recordVal = Visit(expr.AssignExpression);

            if (indexVal.TypeOf != i64)
                indexVal = _builder.BuildIntCast(indexVal, i64, "idx_cast");

            var dfLLVMType = GetOrCreateDataframeType();
            var arrayType = GetOrCreateArrayType();
            var structType = GetOrCreateStructType(dfType.RowType);

            // =================================================
            // LOAD column pointer array
            // =================================================
            var dataPtrsPtr = _builder.BuildStructGEP2(dfLLVMType, dfPtr, 1, "data_ptrs_ptr");
            var dataPtrsArray = _builder.BuildLoad2(
                LLVMTypeRef.CreatePointer(arrayType, 0),
                dataPtrsPtr,
                "data_ptrs_array"
            );

            var dataPtrsDataPtr = _builder.BuildStructGEP2(arrayType, dataPtrsArray, 2, "data_ptrs_data");
            var dataPtrs = _builder.BuildLoad2(i8Ptr, dataPtrsDataPtr, "data_ptrs");

            // =================================================
            // LOOP OVER COLUMNS
            // =================================================
            for (int i = 0; i < dfType.RowType.RecordFields.Count; i++)
            {
                var field = dfType.RowType.RecordFields[i];
                var colIndex = LLVMValueRef.CreateConstInt(i64, (ulong)i);

                // -------------------------
                // Get column array
                // -------------------------
                var colPtrPtr = _builder.BuildGEP2(i8Ptr, dataPtrs, new[] { colIndex }, "col_ptr_ptr");
                var colArray = _builder.BuildLoad2(i8Ptr, colPtrPtr, "col_array");

                var colDataPtrPtr = _builder.BuildStructGEP2(arrayType, colArray, 2, "col_data_ptr");
                var colDataRaw = _builder.BuildLoad2(i8Ptr, colDataPtrPtr, "col_data");

                // -------------------------
                // Get field value from record
                // -------------------------
                var fieldPtr = _builder.BuildStructGEP2(structType, recordVal, (uint)i, "field_ptr");

                LLVMValueRef fieldValue;

                if (field.Type is IntType)
                {
                    fieldValue = _builder.BuildLoad2(i64, fieldPtr, "val");
                    var typed = _builder.BuildBitCast(colDataRaw, LLVMTypeRef.CreatePointer(i64, 0), "int_col");
                    var elemPtr = _builder.BuildGEP2(i64, typed, new[] { indexVal }, "elem_ptr");
                    _builder.BuildStore(fieldValue, elemPtr);
                }
                else if (field.Type is FloatType)
                {
                    fieldValue = _builder.BuildLoad2(ctx.DoubleType, fieldPtr, "val");
                    var typed = _builder.BuildBitCast(colDataRaw, LLVMTypeRef.CreatePointer(ctx.DoubleType, 0), "dbl_col");
                    var elemPtr = _builder.BuildGEP2(ctx.DoubleType, typed, new[] { indexVal }, "elem_ptr");
                    _builder.BuildStore(fieldValue, elemPtr);
                }
                else if (field.Type is BoolType)
                {
                    fieldValue = _builder.BuildLoad2(ctx.Int8Type, fieldPtr, "val");
                    var cast = _builder.BuildZExt(fieldValue, i8, "bool_cast");

                    var typed = _builder.BuildBitCast(colDataRaw, LLVMTypeRef.CreatePointer(i8, 0), "bool_col");
                    var elemPtr = _builder.BuildGEP2(i8, typed, new[] { indexVal }, "elem_ptr");
                    _builder.BuildStore(cast, elemPtr);
                }
                else if (field.Type is StringType)
                {
                    fieldValue = _builder.BuildLoad2(i8Ptr, fieldPtr, "val");

                    var typed = _builder.BuildBitCast(colDataRaw, LLVMTypeRef.CreatePointer(i8Ptr, 0), "str_col");
                    var elemPtr = _builder.BuildGEP2(i8Ptr, typed, new[] { indexVal }, "elem_ptr");
                    _builder.BuildStore(fieldValue, elemPtr);
                }
                else
                {
                    throw new Exception($"Unsupported type df index assign: {field.Type}");
                }
            }

            return recordVal;
        }

        public LLVMValueRef VisitRecord(RecordNode expr)
        {
            var fieldTypes = new List<LLVMTypeRef>();
            foreach (var field in expr.Fields)
            {
                // FORCE double for floats if that's what your FloatType uses
                fieldTypes.Add(GetLLVMType(field.Value.Type));
            }

            // Use a unique name for the struct based on types, not just labels
            // This prevents "struct_savings" (int) and "struct_savings" (float) from colliding
            string structName = "struct_" + string.Join("_", expr.Fields.Select(f => f.Label + f.Value.Type.GetType().Name));

            LLVMTypeRef structType = _module.GetTypeByName(structName);
            if (structType.Handle == IntPtr.Zero)
            {
                structType = _module.Context.CreateNamedStruct(structName);
                structType.StructSetBody(fieldTypes.ToArray(), Packed: false);
            }

            // Allocate memory
            var instancePtr = _builder.BuildMalloc(structType, "record_mem");

            for (int i = 0; i < expr.Fields.Count; i++)
            {
                var fieldValue = Visit(expr.Fields[i].Value);
                var elementPtr = _builder.BuildStructGEP2(structType, instancePtr, (uint)i, $"ptr_{expr.Fields[i].Label}");

                // IMPORTANT: Ensure the value being stored matches the expected type
                _builder.BuildStore(fieldValue, elementPtr);
            }

            return instancePtr;
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

        private (LLVMValueRef fieldPtr, LLVMTypeRef fieldType) GetFieldPointer(ExpressionNode record, string fieldName)
        {
            // 1. Get record pointer
            var recordPtr = Visit(record);
            return GetFieldPointer(record, recordPtr, fieldName);
        }

        private (LLVMValueRef fieldPtr, LLVMTypeRef fieldType) GetFieldPointer(ExpressionNode record, LLVMValueRef recordPtr, string fieldName)
        {
            // 2. Get record type
            var recordType = record.Type as RecordType;
            if (recordType == null)
                throw new Exception("Trying to access field on non-record type");

            // 3. Resolve field index
            int fieldIndex = GetFieldIndex(fieldName, recordType.RecordFields.ToList());

            // 4. Get struct type
            var structType = GetOrCreateStructType(recordType);

            if (recordPtr.TypeOf.Kind != LLVMTypeKind.LLVMPointerTypeKind)
                throw new Exception("Record must be a pointer to struct");

            var structPtr = _builder.BuildBitCast(
                recordPtr,
                LLVMTypeRef.CreatePointer(structType, 0),
                "record_ptr_cast"
            );

            // 5. Get pointer to field
            var fieldPtr = _builder.BuildStructGEP2(
                structType,
                structPtr,
                (uint)fieldIndex,
                $"ptr_{fieldName}"
            );

            // 6. Get field type
            var fieldType = structType.StructGetTypeAtIndex((uint)fieldIndex);

            return (fieldPtr, fieldType);
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

        public LLVMValueRef VisitRecordField(FieldNode expr)
        {
            var (fieldPtr, fieldType) = GetFieldPointer(expr.SourceExpression, expr.IdField);

            var value = _builder.BuildLoad2(
                fieldType,
                fieldPtr,
                $"load_{expr.IdField}"
            );

            return value;
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
            var dfValue = Visit(expr.SourceExpression);
            var pathValue = Visit(expr.FileNameExpression);

            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);

            var fn = GetOrDeclareToCsv();

            var fnType = LLVMTypeRef.CreateFunction(
                _module.Context.VoidType,
                new[] { i8Ptr, i8Ptr },
                false
            );

            var dfCast = _builder.BuildBitCast(dfValue, i8Ptr, "df_cast");

            _builder.BuildCall2(fnType, fn, new[] { dfCast, pathValue }, "");

            return default;
        }

        public LLVMValueRef VisitReadCsv(ReadCsvNode expr)
        {
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            var pathValue = Visit(expr.FileNameExpression);

            var recordSchema = (RecordNode)expr.SchemaExpression;

            string schemaString = GetSchemaString(recordSchema);
            var schemaValue = _builder.BuildGlobalStringPtr(schemaString, "csv_schema");

            var fn = GetOrDeclareReadCsv();

            // CRITICAL FIX: bitcast both args
            var pathCast = _builder.BuildBitCast(pathValue, i8Ptr, "path_cast");
            var schemaCast = _builder.BuildBitCast(schemaValue, i8Ptr, "schema_cast");

            var boxed = _builder.BuildCall2(
                LLVMTypeRef.CreateFunction(i8Ptr, new[] { i8Ptr, i8Ptr }, false),
                fn,
                new[] { pathCast, schemaCast },
                "csv_boxed"
            );

            // unbox
            var unboxPtr = _builder.BuildStructGEP2(_runtimeValueType, boxed, 1, "");
            var dfPtr = _builder.BuildLoad2(i8Ptr, unboxPtr, "df_ptr");

            return dfPtr;
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

                switch (type)
                {
                    case IntType:
                        sb.Append('I');
                        break;

                    case FloatType:
                        sb.Append('F');
                        break;

                    case BoolType:
                        sb.Append('B');
                        break;

                    case StringType:
                        sb.Append('S');
                        break;

                    default:
                        throw new Exception("Invalid type in schema: " + type);
                }
            }
            return sb.ToString();
        }









        private LLVMValueRef GetOrDeclareReadCsv()
        {
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);

            var fnType = LLVMTypeRef.CreateFunction(
                i8Ptr,              // returns ptr (boxed runtime value)
                new[] { i8Ptr, i8Ptr },
                false
            );

            var fn = _module.GetNamedFunction("ReadCsvInternal");
            if (fn.Handle != IntPtr.Zero)
                return fn;

            return _module.AddFunction("ReadCsvInternal", fnType);
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
            var tagPtr = _builder.BuildStructGEP2(_runtimeValueType, runtimeObj, 0, "tag_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i16, (ulong)tag), tagPtr);

            // 5. Store the Data Pointer
            var dataFieldPtr = _builder.BuildStructGEP2(_runtimeValueType, runtimeObj, 1, "data_ptr");
            _builder.BuildStore(dataPtr, dataFieldPtr);

            return runtimeObj;
        }

        public LLVMValueRef VisitDataframe4(DataframeNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var mallocFunc = GetOrDeclareMalloc();

            var columns = (ArrayNode)expr.Columns;
            var dataNode = (ArrayNode)expr.Data;
            var types = (ArrayNode)expr.DataTypes;

            int colCount = columns.Elements.Count;

            // =====================================================
            // 1. TRANSPOSE ROWS -> COLUMNS
            // =====================================================
            var columnData = new List<List<ExpressionNode>>();
            for (int c = 0; c < colCount; c++)
                columnData.Add(new List<ExpressionNode>());

            foreach (var rowNode in dataNode.Elements)
            {
                if (rowNode is ArrayNode row)
                {
                    for (int c = 0; c < colCount; c++)
                    {
                        ExpressionNode value = (c < row.Elements.Count)
                            ? row.Elements[c]
                            : new NullNode();
                        columnData[c].Add(value);
                    }
                }
                else
                {
                    for (int c = 0; c < colCount; c++)
                        columnData[c].Add(new NullNode());
                }
            }

            // =====================================================
            // 2. INFER TYPES
            // =====================================================
            Type[] inferredTypes = new Type[colCount];
            for (int c = 0; c < colCount; c++)
                inferredTypes[c] = InferColumnType(columnData[c]);

            // =====================================================
            // 3. BUILD COLUMN ARRAYS
            // =====================================================
            var columnArrayHeaders = new List<LLVMValueRef>();
            for (int c = 0; c < colCount; c++)
            {
                var colArray = new ArrayNode(columnData[c])
                {
                    ElementType = inferredTypes[c]
                };

                columnArrayHeaders.Add(VisitArray(colArray));
            }

            // =====================================================
            // 4. BUILD DATA POINTERS ARRAY
            // =====================================================
            var dataHeader = _builder.BuildCall2(
                _mallocType,
                mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, 24) }, // size of array struct
                "data_ptrs_header"
            );

            var dataBuffer = _builder.BuildCall2(
                _mallocType,
                mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, (ulong)(colCount * IntPtr.Size)) },
                "data_ptrs_buffer"
            );

            // store metadata into array struct
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)colCount),
                _builder.BuildStructGEP2(_arrayStruct, dataHeader, 0)); // length
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)colCount),
                _builder.BuildStructGEP2(_arrayStruct, dataHeader, 1)); // capacity
            _builder.BuildStore(dataBuffer,
                _builder.BuildStructGEP2(_arrayStruct, dataHeader, 2)); // data pointer

            // store each column pointer into dataBuffer
            for (int i = 0; i < colCount; i++)
            {
                var gep = _builder.BuildGEP2(i8Ptr, dataBuffer,
                    new[] { LLVMValueRef.CreateConstInt(i64, (ulong)i) });
                _builder.BuildStore(
                    _builder.BuildBitCast(columnArrayHeaders[i], i8Ptr, "col_cast"),
                    gep
                );
            }

            // =====================================================
            // 5. BUILD COLUMN NAMES AND TYPES ARRAYS (primitive arrays)
            // =====================================================
            var columnsPtr = VisitArray(columns);
            var typesPtr = VisitArray(types);

            // =====================================================
            // 6. BUILD DATAFRAME STRUCT
            // =====================================================
            var dfType = GetOrCreateDataframeType();
            var dfPtr = _builder.BuildMalloc(dfType, "df_instance");

            _builder.BuildStore(columnsPtr, _builder.BuildStructGEP2(dfType, dfPtr, 0));
            _builder.BuildStore(dataHeader, _builder.BuildStructGEP2(dfType, dfPtr, 1));
            _builder.BuildStore(typesPtr, _builder.BuildStructGEP2(dfType, dfPtr, 2));

            return dfPtr;
        }


        public LLVMValueRef VisitDataframe(DataframeNode expr)
        {
            var ctx = _module.Context;

            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var mallocFunc = GetOrDeclareMalloc();

            var columns = (ArrayNode)expr.Columns;
            var dataNode = (ArrayNode)expr.Data;
            var types = (ArrayNode)expr.DataTypes;

            int colCount = columns.Elements.Count;

            var columnArrayHeaders = new List<LLVMValueRef>();

            // Transpose Rows to Columns
            var columnData = new List<List<ExpressionNode>>();
            for (int c = 0; c < colCount; c++)
            {
                columnData.Add(new List<ExpressionNode>());
            }

            foreach (var rowNode in dataNode.Elements)
            {
                if (rowNode is not ArrayNode row)
                    continue;

                for (int c = 0; c < colCount; c++)
                {
                    ExpressionNode value = c < row.Elements.Count ? row.Elements[c] : new NullNode();
                    columnData[c].Add(value);
                }
            }

            // Type Inference
            Type[] inferredTypes = new Type[colCount];
            for (int c = 0; c < colCount; c++)
            {
                inferredTypes[c] = InferColumnType(columnData[c]);
            }

            // Build Column Arrays
            for (int c = 0; c < colCount; c++)
            {
                var colArray = new ArrayNode(columnData[c])
                {
                    ElementType = inferredTypes[c]
                };

                columnArrayHeaders.Add(VisitArray(colArray));
            }

            // Build Column Pointer Array
            ulong arrayHeaderSize = 24;
            var dataHeader = _builder.BuildCall2(_mallocType, mallocFunc, new[] { LLVMValueRef.CreateConstInt(i64, arrayHeaderSize) }, "data_header");

            ulong ptrSize = (ulong)IntPtr.Size;
            ulong dataBufferSize = (ulong)colCount * ptrSize;
            var dataBuffer = _builder.BuildCall2(_mallocType, mallocFunc, new[] { LLVMValueRef.CreateConstInt(i64, dataBufferSize) }, "data_buffer");

            // header.length
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)colCount), _builder.BuildStructGEP2(_arrayStruct, dataHeader, 0));
            // header.capacity
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)colCount), _builder.BuildStructGEP2(_arrayStruct, dataHeader, 1));
            // header.data
            _builder.BuildStore(dataBuffer, _builder.BuildStructGEP2(_arrayStruct, dataHeader, 2));

            // Store Column Pointers
            var ptrArrayType = LLVMTypeRef.CreatePointer(i8Ptr, 0);
            var typedBuffer = _builder.BuildBitCast(dataBuffer, ptrArrayType, "typed_buffer");

            for (int i = 0; i < colCount; i++)
            {
                var gep = _builder.BuildGEP2(i8Ptr, typedBuffer, new[] { LLVMValueRef.CreateConstInt(i64, (ulong)i) }, "data_gep");
                _builder.BuildStore(_builder.BuildBitCast(columnArrayHeaders[i], i8Ptr, "col_cast"), gep);
            }

            // Final Dataframe Instance
            var columnsPtr = VisitArray(columns);
            var typesPtr = VisitArray(types);
            var dfType = GetOrCreateDataframeType();
            var dfPtr = _builder.BuildMalloc(dfType, "df_instance");

            _builder.BuildStore(columnsPtr, _builder.BuildStructGEP2(dfType, dfPtr, 0));
            _builder.BuildStore(dataHeader, _builder.BuildStructGEP2(dfType, dfPtr, 1));
            _builder.BuildStore(typesPtr, _builder.BuildStructGEP2(dfType, dfPtr, 2));

            ulong rowCount = (ulong)dataNode.Elements.Count;
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, rowCount), _builder.BuildStructGEP2(dfType, dfPtr, 3));

            return dfPtr;
        }

        private Type InferColumnType(List<ExpressionNode> values)
        {
            foreach (var v in values)
            {
                if (v is NumberNode) return new IntType();
                if (v is FloatNode) return new FloatType();
                if (v is BooleanNode) return new BoolType();
                if (v is StringNode) return new StringType();
                if (v is TypeLiteralNode tp) return InferTypeFromString(tp.TypeNode.Name);
            }
            return new StringType();
        }


        public LLVMValueRef VisitDataframe_works_but_cant_do_many_columns(DataframeNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            var mallocFunc = GetOrDeclareMalloc();

            var columns = (ArrayNode)expr.Columns;
            var rows = (ArrayNode)expr.Data;
            var types = (ArrayNode)expr.DataTypes;

            int colCount = columns.Elements.Count;

            // =========================================================
            // 1. Transpose rows -> columns
            // =========================================================
            var columnData = new List<List<ExpressionNode>>();
            for (int c = 0; c < colCount; c++)
                columnData.Add(new List<ExpressionNode>());

            foreach (ArrayNode row in rows.Elements)
            {
                if (row.Elements.Count != 0)
                {
                    for (int c = 0; c < colCount; c++)
                        columnData[c].Add(row.Elements[c]);// right here we fail for where on dataframe

                }
            }

            // =========================================================
            // 2. Build each column as ArrayObject (REAL arrays)
            // =========================================================
            var columnArrayHeaders = new List<LLVMValueRef>();

            for (int c = 0; c < colCount; c++)
            {
                ArrayNode colArray;
                if (types.Elements[0] is TypeLiteralNode)
                {
                    colArray = new ArrayNode(columnData[c])
                    {

                        ElementType = InferTypeFromString((types.Elements[c] as TypeLiteralNode).TypeNode.Name)
                    };
                }
                else
                {
                    colArray = new ArrayNode(columnData[c])
                    {
                        ElementType = types.Elements[c].Type
                    };
                }

                columnArrayHeaders.Add(VisitArray(colArray)); // returns ArrayObject*
            }

            // =========================================================
            // 3. Build dataPointersData = ArrayObject of ptrs
            // =========================================================

            var dataHeader = _builder.BuildCall2(_mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, 24) },
                "data_ptrs_header");

            var dataBuffer = _builder.BuildCall2(_mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, (ulong)(colCount * 8)) },
                "data_ptrs_buffer");

            // store metadata
            _builder.BuildStore(
                LLVMValueRef.CreateConstInt(i64, (ulong)colCount),
                _builder.BuildStructGEP2(_arrayStruct, dataHeader, 0));

            _builder.BuildStore(
                LLVMValueRef.CreateConstInt(i64, (ulong)colCount),
                _builder.BuildStructGEP2(_arrayStruct, dataHeader, 1));

            _builder.BuildStore(
                dataBuffer,
                _builder.BuildStructGEP2(_arrayStruct, dataHeader, 2));

            // fill pointer array
            for (int i = 0; i < colCount; i++)
            {
                var idx = LLVMValueRef.CreateConstInt(i64, (ulong)i);

                var cast = _builder.BuildBitCast(columnArrayHeaders[i], i8Ptr, "col_ptr_cast");
                var gep = _builder.BuildGEP2(i8Ptr, dataBuffer, new[] { idx }, "data_gep");

                _builder.BuildStore(cast, gep);
            }

            // =========================================================
            // 4. Build column names array
            // =========================================================
            var columnsPtr = VisitArray(columns);

            // =========================================================
            // 5. Build types array
            // =========================================================
            var typesPtr = VisitArray(types);

            // =========================================================
            // 6. Build dataframe struct
            // =========================================================
            var dfType = GetOrCreateDataframeType();
            var dfPtr = _builder.BuildMalloc(dfType, "df");

            _builder.BuildStore(columnsPtr,
                _builder.BuildStructGEP2(dfType, dfPtr, 0));

            _builder.BuildStore(dataHeader,
                _builder.BuildStructGEP2(dfType, dfPtr, 1));

            _builder.BuildStore(typesPtr,
                _builder.BuildStructGEP2(dfType, dfPtr, 2));

            long rowCount =
                columnData.Count > 0
                    ? columnData[0].Count
                    : 0;

            _builder.BuildStore(
                LLVMValueRef.CreateConstInt(i64, (ulong)rowCount),
                _builder.BuildStructGEP2(dfType, dfPtr, 3)
                );

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

            foreach (var col in expr.Columns)
            {
                var nameVal = Visit(col); // LLVMValueRef to string
            }

            throw new NotImplementedException();
        }

        LLVMValueRef GetArrayData(LLVMValueRef arrayPtr)
        {
            var ctx = _module.Context;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            return _builder.BuildLoad2(
                i8Ptr,
                _builder.BuildStructGEP2(_arrayStruct, arrayPtr, 2),
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

        public LLVMValueRef VisitTypeLiteral(TypeLiteralNode expr)
        {
            if (_debug) Console.WriteLine("visiting: TypeLiteral (Skipping CodeGen)");
            // Return a dummy null pointer so LLVM doesn't get a completely empty handle
            return LLVMValueRef.CreateConstNull(LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0));
        }

        public LLVMValueRef VisitSqrt(SqrtNode expr)
        {
            // 1. Visit the argument
            var val = Visit(expr.Value);

            // 2. Promotion: If it's an int, convert to double
            if (expr.Value.Type is IntType)
            {
                val = _builder.BuildSIToFP(val, _module.Context.DoubleType, "int2double");
            }

            // 3. Define the Intrinsic. 
            // LLVM intrinsics are named "llvm.<name>.<type>"
            // For a 64-bit float (double), it is "llvm.sqrt.f64"
            string intrinsicName = "llvm.sqrt.f64";
            var doubleType = _module.Context.DoubleType;
            var sqrtType = LLVMTypeRef.CreateFunction(doubleType, new[] { doubleType });

            // 4. Register the intrinsic in the module if not already present
            var sqrtFunc = _module.GetNamedFunction(intrinsicName);
            if (sqrtFunc.Handle == IntPtr.Zero)
            {
                sqrtFunc = _module.AddFunction(intrinsicName, sqrtType);
            }

            // 5. Call it
            return _builder.BuildCall2(sqrtType, sqrtFunc, new[] { val }, "sqrttmp");
        }

        public LLVMValueRef VisitLog(LogNode expr)
        {
            // 1. Evaluate argument
            var val = Visit(expr.Value);

            // 2. Promote int → double if needed
            if (expr.Value.Type is IntType)
            {
                val = _builder.BuildSIToFP(val, _module.Context.DoubleType, "int2double");
            }

            var doubleType = _module.Context.DoubleType;

            // 3. Intrinsic function type: double -> double
            var logType = LLVMTypeRef.CreateFunction(doubleType, new[] { doubleType });

            const string intrinsicName = "llvm.log.f64";

            var logFunc = _module.GetNamedFunction(intrinsicName);
            if (logFunc.Handle == IntPtr.Zero)
            {
                logFunc = _module.AddFunction(intrinsicName, logType);
            }

            // 4. Call intrinsic
            return _builder.BuildCall2(
                logType,
                logFunc,
                new[] { val },
                "logtmp"
            );
        }

        public LLVMValueRef VisitPow(PowNode expr)
        {
            var lhs = Visit(expr.Value);
            var rhs = Visit(expr.Power);

            var ctx = _module.Context;
            var doubleType = ctx.DoubleType;

            // Promote ints → doubles if needed
            if (expr.Value.Type is IntType)
                lhs = _builder.BuildSIToFP(lhs, doubleType, "int2double_lhs");

            if (expr.Power.Type is IntType)
                rhs = _builder.BuildSIToFP(rhs, doubleType, "int2double_rhs");

            // pow signature: double(double, double)
            var powType = LLVMTypeRef.CreateFunction(
                doubleType,
                new[] { doubleType, doubleType }
            );

            const string intrinsicName = "llvm.pow.f64";

            var powFunc = _module.GetNamedFunction(intrinsicName);
            if (powFunc.Handle == IntPtr.Zero)
            {
                powFunc = _module.AddFunction(intrinsicName, powType);
            }

            return _builder.BuildCall2(
                powType,
                powFunc,
                new[] { lhs, rhs },
                "powtmp"
            );
        }

        public LLVMValueRef VisitExponentialMathFunc(ExponentialMathFuncNode expr)
        {
            var val = Visit(expr.Value);

            // Promote int → double (same as sqrt)
            if (expr.Value.Type is IntType)
            {
                val = _builder.BuildSIToFP(val, _module.Context.DoubleType, "int2double");
            }

            var doubleType = _module.Context.DoubleType;

            // IMPORTANT: correct LLVM intrinsic
            string intrinsicName = "llvm.exp.f64";

            var expType = LLVMTypeRef.CreateFunction(doubleType, new[] { doubleType });

            var expFunc = _module.GetNamedFunction(intrinsicName);

            if (expFunc.Handle == IntPtr.Zero)
            {
                expFunc = _module.AddFunction(intrinsicName, expType);
            }

            return _builder.BuildCall2(expType, expFunc, new[] { val }, "exptmp");
        }

        public LLVMValueRef VisitField(FieldNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            // ============================
            // IMPORTANT FIX
            // ============================
            var sourceVal = Visit(expr.SourceExpression);

            // CASE 1: RECORD
            if (expr.SourceExpression.Type is RecordType)
            {
                var (fieldPtr, _) = GetFieldPointer(expr.SourceExpression, sourceVal, expr.IdField);

                return _builder.BuildLoad2(
                    GetLLVMType(expr.Type),
                    fieldPtr,
                    $"val_{expr.IdField}"
                );
            }

            // CASE 2: DATAFRAME FIELD
            if (expr.SourceExpression.Type is DataframeType dfType)
            {
                int colIndex = dfType.ColumnNames.ToList().IndexOf(expr.IdField);
                if (colIndex < 0)
                    throw new Exception($"Column {expr.IdField} not found");

                var colIndexVal = LLVMValueRef.CreateConstInt(i64, (ulong)colIndex);

                // dataframe pointer is already computed VALUE
                var dfPtr = sourceVal;

                // load column pointers array
                // var dataFieldPtr = _builder.BuildStructGEP2(dfType, dfPtr, 1, "df_data_ptr");
                var dataFieldPtr = _builder.BuildStructGEP2(GetOrCreateDataframeType(), dfPtr, 1, "df_data_ptr");
                var dataHeader = _builder.BuildLoad2(i8Ptr, dataFieldPtr, "data_header");

                var rawDataPtrPtr = _builder.BuildStructGEP2(_arrayStruct, dataHeader, 2);
                var rawDataPtr = _builder.BuildLoad2(i8Ptr, rawDataPtrPtr);

                var colPtrPtr = _builder.BuildGEP2(i8Ptr, rawDataPtr, new[] { colIndexVal });
                var colPtr = _builder.BuildLoad2(i8Ptr, colPtrPtr);

                // RETURN COLUMN ARRAY POINTER (IMPORTANT)
                return _builder.BuildBitCast(colPtr, i8Ptr);
            }

            throw new Exception("Field access only supported on records or dataframes");
        }

        public LLVMValueRef VisitColumns(ColumnsNode expr)
        {
            var ctx = _module.Context;
            var sourcePtr = Visit(expr.DataframeExpression); // pointer to dataframe
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            if (expr.DataframeExpression.Type is DataframeType)
            {
                // Step 1: get pointer to the 'columns' field
                var columnsFieldPtr = _builder.BuildStructGEP2(_dataframeStruct, sourcePtr, 0, "columns_ptr_field");

                // Step 2: load the pointer to the columns array
                var columnsPtr = _builder.BuildLoad2(i8Ptr, columnsFieldPtr, "columns_ptr");

                return columnsPtr; // Return the pointer to the columns array (i8*)
            }
            else
                throw new Exception("Columns operator is only supported on dataframes");
        }

        public LLVMValueRef VisitCast(CastNode node)
        {
            var value = Visit(node.Expression);

            if (node.Expression.Type is IntType && node.ToType is FloatType)
            {
                if (_debug) Console.WriteLine($"Converting {node.Expression.Type} to {node.ToType} in cast node");
                return _builder.BuildSIToFP(value, _module.Context.DoubleType, "int2double");
            }

            throw new NotImplementedException($"Unsupported cast {node.FromType} -> {node.ToType}");
        }

        public LLVMValueRef VisitCorrelation(CorrelationNode expr)
        {
            var xVar = "__corr_x";
            var yVar = "__corr_y";
            var iVar = "__corr_i";

            var sumX = "__corr_sum_x";
            var sumY = "__corr_sum_y";

            var meanX = "__corr_mean_x";
            var meanY = "__corr_mean_y";

            var num = "__corr_num";
            var denomX = "__corr_dx";
            var denomY = "__corr_dy";

            // assign arrays
            var assignX = new AssignNode(xVar, expr.SourceExpression);
            var assignY = new AssignNode(yVar, expr.TargetExpression);

            var initSumX = new AssignNode(sumX, new NumberNode(0));
            var initSumY = new AssignNode(sumY, new NumberNode(0));
            var initI = new AssignNode(iVar, new NumberNode(0));

            var lenX = new LengthNode(new IdNode(xVar));

            // FIRST LOOP: compute means
            var cond1 = new ComparisonNode(new IdNode(iVar), "<", lenX);
            var step1 = new IncrementNode(new IdNode(iVar));

            var x_i = new IndexNode(new IdNode(xVar), new IdNode(iVar));
            var y_i = new IndexNode(new IdNode(yVar), new IdNode(iVar));

            var addSumX = new AssignNode(sumX,
                new BinaryOpNode(new IdNode(sumX), "+", x_i)
            );

            var addSumY = new AssignNode(sumY,
                new BinaryOpNode(new IdNode(sumY), "+", y_i)
            );

            var loop1Body = new SequenceNode();
            loop1Body.Statements.Add(addSumX);
            loop1Body.Statements.Add(addSumY);

            var loop1 = new ForLoopNode(initI, cond1, step1, loop1Body);

            var meanAssignX = new AssignNode(meanX,
                new BinaryOpNode(new IdNode(sumX), "/", lenX)
            );

            var meanAssignY = new AssignNode(meanY,
                new BinaryOpNode(new IdNode(sumY), "/", lenX)
            );

            // reset index for second loop
            var resetI = new AssignNode(iVar, new NumberNode(0));

            // SECOND LOOP: covariance
            var cond2 = new ComparisonNode(new IdNode(iVar), "<", lenX);
            var step2 = new IncrementNode(new IdNode(iVar));

            var dx = new BinaryOpNode(x_i, "-", new IdNode(meanX));
            var dy = new BinaryOpNode(y_i, "-", new IdNode(meanY));

            var addNum = new AssignNode(num,
                new BinaryOpNode(new IdNode(num), "+",
                    new BinaryOpNode(dx, "*", dy)
                )
            );

            var addDenomX = new AssignNode(denomX,
                new BinaryOpNode(new IdNode(denomX), "+",
                    new BinaryOpNode(dx, "*", dx)
                )
            );

            var addDenomY = new AssignNode(denomY,
                new BinaryOpNode(new IdNode(denomY), "+",
                    new BinaryOpNode(dy, "*", dy)
                )
            );

            var loop2Body = new SequenceNode();
            loop2Body.Statements.Add(addNum);
            loop2Body.Statements.Add(addDenomX);
            loop2Body.Statements.Add(addDenomY);

            var loop2 = new ForLoopNode(resetI, cond2, step2, loop2Body);

            // FINAL RESULT
            var corr = new AssignNode("__corr_result",
                new BinaryOpNode(
                    new IdNode(num),
                    "/",
                    new SqrtNode(
                        new BinaryOpNode(
                            new IdNode(denomX),
                            "*",
                            new IdNode(denomY)
                        )
                    )
                )
            );

            var program = new SequenceNode();
            program.Statements.Add(assignX);
            program.Statements.Add(assignY);

            program.Statements.Add(initSumX);
            program.Statements.Add(initSumY);
            program.Statements.Add(initI);
            program.Statements.Add(loop1);

            program.Statements.Add(meanAssignX);
            program.Statements.Add(meanAssignY);

            program.Statements.Add(new AssignNode(iVar, new NumberNode(0)));

            program.Statements.Add(new AssignNode(num, new NumberNode(0)));
            program.Statements.Add(new AssignNode(denomX, new NumberNode(0)));
            program.Statements.Add(new AssignNode(denomY, new NumberNode(0)));

            program.Statements.Add(loop2);

            program.Statements.Add(corr);
            program.Statements.Add(new IdNode("__corr_result"));

            PerformSemanticAnalysis(program);

            return VisitSequence(program);
        }
    }
    /*
    df = read_csv("CSV/Fire_Prediction_2023_Bolivia_encoded_10K.csv")
    df = read_csv("CSV/Fire_Prediction_2023_Bolivia_encoded_FULL.csv")
    df.where(x => x.latitude > -18.0)

    df2 = dataframe(["name", "age", "isCool","savings"],[["Harry", 12, true,100.08],["Bob", 23, false, 1.0],["Harry", 34, false, 0.0]])
    df2 = dataframe(["name", "age", "isCool","savings"],[["Harry", 12, true,100.08],["Bob", 23, false, 1.0],["Harry", 34, false, 0.0],["Harry", 74, false, 0.0],["Harry", 24, true, 0.0],["Harry", 31, false, 0.0]])

    df.where(x=> x.name == "Harry")

    df2.add({name: "Barry", age: 45, isCool: true, savings: 100.00})
    df2.add({name: "Harry", age: 39, isCool: true})

    */
}

