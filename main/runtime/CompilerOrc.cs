using System;
using LLVMSharp;
using LLVMSharp.Interop;
using System.Linq;
using System.Reflection; // for using the stopwatch!
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Data;

namespace MyCompiler
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)] // Pack 8 is standard for 64-bit pointers
    public struct RuntimeValue
    {
        public long tag;
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

    static class Native
    {
#if LINUX
        [DllImport("libc.so.6", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr malloc(IntPtr size);

        [DllImport("libc.so.6", CallingConvention = CallingConvention.Cdecl)]
        public static extern void free(IntPtr ptr);

#else
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr malloc(IntPtr size);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void free(IntPtr ptr);
#endif
    }

    public static class LanguageRuntime
    {
        public static IntPtr ReadCsvInternal(IntPtr pathPtr, IntPtr schemaPtr)
        {
            string path = Marshal.PtrToStringAnsi(pathPtr);
            if (!File.Exists(path))
            {
                Console.WriteLine("no file in internale");
                return IntPtr.Zero;
            }

            if (schemaPtr == IntPtr.Zero)
                throw new Exception("no scheme");

            string schema = Marshal.PtrToStringAnsi((nint)schemaPtr);

            var lines = File.ReadAllLines(path).Skip(1).ToArray();
            int rowCount = lines.Length;

            // --- THE NATIVE WAY ---
            // 1. Allocate Array Header { i64, i64, ptr } (24 bytes)
            IntPtr rowsArrayHeader = Native.malloc((IntPtr)24);

            // 2. Allocate Data Buffer (rowCount * 8 bytes)
            IntPtr rowsDataBuffer = Native.malloc((IntPtr)(rowCount * 8));

            for (int i = 0; i < rowCount; i++)
            {
                string[] parts = lines[i].Split(',');
                // Allocate Record Buffer
                IntPtr recordBuffer = Native.malloc((IntPtr)(parts.Length * 8));

                for (int col = 0; col < parts.Length; col++)
                {
                    char typeCode = col < schema.Length ? schema[col] : 'S';
                    string rawValue = parts[col].Trim();

                    // Allocate value box (8 bytes)
                    IntPtr valuePtr = Native.malloc((IntPtr)8);

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
                    else if (typeCode == 'S')
                    {
                        // Strings are tricky; StringToHGlobalAnsi uses Marshal's heap.
                        // For a 100% "Real" IR way, you'd malloc + strcpy here too.
                        IntPtr strPtr = Marshal.StringToHGlobalAnsi(rawValue);
                        valuePtr = strPtr;
                    }
                    else
                        throw new Exception("Invalid data type: " + typeCode);
                    Marshal.WriteIntPtr(recordBuffer, col * 8, valuePtr);
                }
                Marshal.WriteIntPtr(rowsDataBuffer, i * 8, recordBuffer);
            }

            // Initialize Header: Length, Capacity, DataPtr
            Marshal.WriteInt64(rowsArrayHeader, 0, rowCount);
            Marshal.WriteInt64(rowsArrayHeader, 8, rowCount);
            Marshal.WriteIntPtr(rowsArrayHeader, 16, rowsDataBuffer);

            // 3. Wrap in RuntimeValue { i64 tag, ptr data } (16 bytes)
            IntPtr runtimeBox = Native.malloc((IntPtr)16);
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
                            cellValue = BitConverter.Int64BitsToDouble(bits).ToString();
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
        int _headTailCount = 5;
        int _maxRowCount = 50;
        private bool _debug = true;
        private bool _stopwatch = false;
        private bool _showAllColumns = false;
        private bool _showAllRows = false;
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
        private LLVMTypeRef _readCsvInternalType;
        private ToCsvDelegate _toCsvDelegate;
        public delegate IntPtr ReadCsvDelegate(IntPtr pathPtr, IntPtr schemaPtr);
        public delegate void ToCsvDelegate(IntPtr data, IntPtr path);
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
        void StopStopWatch(string testName = null)
        {
            sw.Stop();
            if (testName is not null)
                Console.WriteLine("\n--- Execution Stats - " + testName + " ---");
            else
                Console.WriteLine("\n--- Execution Stats ---");

            Console.WriteLine($"Execution Time: {sw.Elapsed.TotalMilliseconds} ms");
            Console.WriteLine($"Ticks: {sw.ElapsedTicks}");
            Console.WriteLine("------------------------\n");
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
            var i8 = ctx.Int8Type;

            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            _dataframeStruct = LLVMTypeRef.CreateStruct(new[] { i8Ptr, i8Ptr, i8Ptr }, false);
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

            //var programedResult = GetProgramResult(expr);
            var programedResult = GetLastExpression(expr);

            if (programedResult == null) return new VoidType();
            if (programedResult is ExpressionNode exp) return exp.Type;

            return new VoidType();
        }

        // TODO: 
        // none atm

        // Problems

        // TODO: fix the problems

        // BROKEN FUNCTIONALITY   
        // record copy is not working
        // making a dataframe takes a repeat of record syntax, could be smoother              

        // UNIT TESTING
        // create a orc unit test
        // A REPL can do multipe commands in a row and they need to be tested to see if they work correctly

        // OTHER
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
            _stopwatch = useStopWatch;

            _showAllColumns = showAllColumns;
            _showAllRows = showAllRows;
            // 1. Semantic analysis
            var prediction = PerformSemanticAnalysis(expr);

            CreateMain();
            DeclarePrintf();

            if (_debug) Console.WriteLine("we code gen");

            if (_stopwatch) StartStopWatch();
            LLVMValueRef resultValue = Visit(expr);
            if (_stopwatch) StopStopWatch("Ran codegen");

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

            var tempResult = delegateResult();
            if (tempResult == IntPtr.Zero) throw new Exception("JIT execution returned null pointer");

            if (_stopwatch) StartStopWatch();
            RuntimeValue result = Marshal.PtrToStructure<RuntimeValue>(tempResult);
            if (_stopwatch) StopStopWatch("Ran compiler");

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
                        return HandleDataframe(result.data, dfType);

                    throw new Exception("Run time object failed for tag dataframe");

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

            if (_debug) Console.WriteLine("Array length: " + array.length);
            if (_debug) Console.WriteLine("Array capacity: " + array.capacity);

            long len = array.length;

            bool truncate = len > (_headTailCount + _headTailCount);

            for (long i = 0; i < len; i++)
            {
                // Insert "..." once in the middle
                if (truncate && i == _headTailCount)
                {
                    elements.Add("...");
                    i = len - _headTailCount; // jump to tail
                }

                IntPtr elemPtr = IntPtr.Add(array.data, (int)(i * stride));

                if (elementType is IntType)
                    elements.Add(Marshal.ReadInt64(elemPtr));
                else if (elementType is FloatType)
                    elements.Add(Marshal.PtrToStructure<double>(elemPtr));
                else if (elementType is BoolType)
                    elements.Add(Marshal.ReadByte(elemPtr) != 0);
                else if (elementType is StringType)
                    elements.Add("\"" + Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(elemPtr)) + "\"");
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

            for (int i = 0; i < record.RecordFields?.Count; i++)
            {
                var rec = record.RecordFields[i];
                string label = rec.Label;

                Type recType = rec.Type;
                // if(_debug) Console.WriteLine($"{label}: {recType}");

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
                    ? f.ToString()
                    : kv.Value?.ToString() ?? "null";
                return $"{kv.Key}: {valStr}";
            });

            return "{ " + string.Join(", ", entries) + " }";
        }

        private List<string> ExtractArray(IntPtr arrayObjPtr, ArrayType type)
        {
            // Instead of PtrToStructure, we read the 3 fields of %array manually:
            // %array = type { i64 (len), i64 (cap), ptr (data) }

            long length = Marshal.ReadInt64(arrayObjPtr, 0);       // Offset 0: length
            // long capacity = Marshal.ReadInt64(arrayObjPtr, 8);  // Offset 8: capacity (unused here)
            IntPtr dataPtr = Marshal.ReadIntPtr(arrayObjPtr, 16);  // Offset 16: data pointer

            var result = new List<string>();
            var elementType = type.ElementType;

            for (long i = 0; i < length; i++)
            {
                // Each element in the data array is a pointer (8 bytes)
                IntPtr elemSlot = IntPtr.Add(dataPtr, (int)(i * 8));
                IntPtr actualValPtr = Marshal.ReadIntPtr(elemSlot);

                if (actualValPtr == IntPtr.Zero)
                {
                    result.Add("null");
                    continue;
                }

                if (elementType is StringType)
                {
                    result.Add(Marshal.PtrToStringAnsi(actualValPtr) ?? "");
                }
                else if (elementType is RecordType rec)
                {
                    // Unpack the record
                    var fields = new List<string>();
                    for (int f = 0; f < rec.RecordFields.Count; f++)
                    {
                        IntPtr fieldSlot = IntPtr.Add(actualValPtr, f * 8);
                        IntPtr valPtr = Marshal.ReadIntPtr(fieldSlot);

                        var fieldType = rec.RecordFields[f].Type;
                        if (fieldType is FloatType)
                        {
                            byte[] b = new byte[8];
                            Marshal.Copy(valPtr, b, 0, 8);
                            fields.Add(BitConverter.ToDouble(b, 0).ToString());
                        }
                        else if (fieldType is IntType)
                        {
                            fields.Add(Marshal.ReadInt64(valPtr).ToString());
                        }
                        else if (fieldType is StringType)
                        {
                            fields.Add(Marshal.PtrToStringAnsi(valPtr) ?? "");
                        }
                        else fields.Add("?");
                    }
                    result.Add(string.Join(" | ", fields));
                }
                else
                {
                    result.Add("?");
                }
            }
            return result;
        }

        private string HandleDataframe(IntPtr ptr, DataframeType type)
        {
            if (ptr == IntPtr.Zero) return "dataframe(null)";
            var dfObj = Marshal.PtrToStructure<DataframeObject>(ptr);

            // 1. Get Column Names (Always needed)
            var columnNames = ExtractArray(dfObj.columns, new ArrayType(new StringType()));

            // 2. Get Tags (Optimized to read only what we need)
            var dataTypeHeader = Marshal.PtrToStructure<ArrayObject>(dfObj.dataTypes);
            List<Int16> colTags = new List<Int16>();
            for (long i = 0; i < dataTypeHeader.length; i++)
            {
                // tags are stored as i64 in your memory layout, so we add i * 8
                colTags.Add(Marshal.ReadInt16(IntPtr.Add(dataTypeHeader.data, (int)(i * 8))));
            }

            // 3. Optimized Row Extraction
            var rowsHeader = Marshal.PtrToStructure<ArrayObject>(dfObj.rows);
            long rowCount = rowsHeader.length;

            // Key Optimization: Create a Dictionary to store ONLY the rows we need
            // Key = original index, Value = the extracted record data
            var sparseRows = new Dictionary<int, List<object>>();

            // Determine which indices we need based on your display rules
            var indicesToExtract = new List<int>();
            if (_showAllRows || rowCount <= _maxRowCount)
            {
                for (int i = 0; i < rowCount; i++) indicesToExtract.Add(i);
            }
            else
            {
                for (int i = 0; i < _headTailCount; i++) indicesToExtract.Add(i); // Head
                for (int i = (int)rowCount - _headTailCount; i < rowCount; i++) indicesToExtract.Add(i); // Tail
            }

            // Extract only the necessary records from memory
            foreach (int r in indicesToExtract)
            {
                IntPtr recordPtr = Marshal.ReadIntPtr(IntPtr.Add(rowsHeader.data, r * 8));
                sparseRows[r] = ExtractRecord(recordPtr, type.RowType);
            }

            // Pass the sparse dictionary and the total count to the formatter
            return FormatTable(columnNames, sparseRows, colTags, (int)rowCount);
        }

        private string FormatTable(List<string> columnNames, Dictionary<int, List<object>> rows, List<Int16> colTypes, int rowCount)
        {
            bool showAllColumns = _showAllColumns;
            bool showAllRows = _showAllRows;

            var allColumnNames = new List<string> { "" }; // Index column at 0
            allColumnNames.AddRange(columnNames);
            int totalCols = allColumnNames.Count;

            // --- 1. Determine which column indices to show ---
            var colIndices = new List<int>();
            if (showAllColumns || totalCols <= 10)
            {
                for (int i = 0; i < totalCols; i++) colIndices.Add(i);
            }
            else
            {
                for (int i = 0; i < 4; i++) colIndices.Add(i); // Index + first 3
                colIndices.Add(-1); // Horizontal "..." separator
                for (int i = totalCols - 3; i < totalCols; i++) colIndices.Add(i);
            }

            // --- 2. Build mapping for tags (aligned with colIndices) ---
            var tags = colTypes != null
                ? new List<Int16> { -1 }.Concat(colTypes).ToList()
                : new List<Int16> { -1 };
            while (tags.Count < totalCols) tags.Add(-1);

            // Helper for value retrieval
            string GetStringValue(object v, int colIndex, int rowIndex)
            {
                if (colIndex == 0) return rowIndex.ToString();
                if (v == null) return "null";

                IntPtr ptr = (v is IntPtr p) ? p : IntPtr.Zero;
                if (ptr == IntPtr.Zero) return v.ToString();

                int tag = tags[colIndex];
                try
                {
                    switch (tag)
                    {
                        case 0: case 1: return Marshal.ReadInt64(ptr).ToString();
                        case 2:
                            byte[] bytes = new byte[8];
                            Marshal.Copy(ptr, bytes, 0, 8);
                            return BitConverter.ToDouble(bytes, 0).ToString();
                        case 3: return Marshal.ReadByte(ptr) != 0 ? "True" : "False";
                        case 4:
                            IntPtr sAddr = Marshal.ReadIntPtr(ptr);
                            return sAddr == IntPtr.Zero ? "" : Marshal.PtrToStringAnsi(sAddr);
                        default: return "???";
                    }
                }
                catch { return "ERR"; }
            }

            // --- 3. Calculate Widths using only extracted rows ---
            var colWidths = new Dictionary<int, int>();
            foreach (var c in colIndices)
            {
                if (c == -1) { colWidths[c] = 3; continue; }

                int maxWidth = allColumnNames[c].Length;

                // Ensure index column fits the largest row number AND the "..." ellipsis
                if (c == 0)
                {
                    // Change is here: added Math.Max(3, ...)
                    maxWidth = Math.Max(3, Math.Max(maxWidth, rowCount.ToString().Length));
                }

                foreach (var kvp in rows)
                {
                    string s = GetStringValue(c == 0 ? null : kvp.Value[c - 1], c, kvp.Key);
                    if (s.Length > maxWidth) maxWidth = s.Length;
                    if (maxWidth >= 30) break;
                }
                colWidths[c] = Math.Min(maxWidth, 30);
            }

            // --- 4. Determine which row indices to display ---
            var rowIndices = new List<int>();
            if (showAllRows || rowCount <= _maxRowCount)
            {
                for (int i = 0; i < rowCount; i++) rowIndices.Add(i);
            }
            else
            {
                for (int i = 0; i < 5; i++) rowIndices.Add(i);
                rowIndices.Add(-1); // Vertical "..."
                for (int i = (int)rowCount - 5; i < rowCount; i++) rowIndices.Add(i);
            }

            // --- 5. Formatting Helper ---
            string BuildLine(Func<int, string> contentProvider)
            {
                var parts = new List<string>();
                foreach (var c in colIndices)
                {
                    // Get the raw value
                    string val = (c == -1) ? "..." : contentProvider(c);

                    // PAD RIGHT to the calculated width
                    parts.Add(val.PadRight(colWidths[c]));
                }
                return string.Join(" | ", parts);
            }

            // --- 6. Assemble Table ---
            var lines = new List<string>();

            // Header
            lines.Add(BuildLine(c => allColumnNames[c]));

            // Separator - FIX: Use the same join structure as BuildLine
            // Instead of manual string.Join, we mirror the " | " spacing
            var separatorParts = colIndices.Select(c => new string('-', colWidths[c]));
            lines.Add(string.Join("-+-", separatorParts));

            // Data Rows
            foreach (var r in rowIndices)
            {
                if (r == -1)
                {
                    // This now works perfectly because BuildLine handles the padding for column 0
                    lines.Add(BuildLine(c => "..."));
                    continue;
                }

                if (rows.ContainsKey(r))
                {
                    lines.Add(BuildLine(c =>
                    {
                        if (c == 0) return r.ToString();
                        return GetStringValue(rows[r][c - 1], c, r);
                    }));
                }
            }

            return $"\nDataframe ({rowCount} rows, {totalCols - 1} columns):\n" +
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
            var iName = "__i";
            var iVar = new IdNode(iName);
            var init = new AssignNode(iName, new NumberNode(0));
            var length = new LengthNode(expr.Source);
            var cond = new ComparisonNode(iVar, "<", length);
            var step = new IncrementNode(iVar);

            // xs[i]
            var indexAccess = new IndexNode(expr.Source, iVar);

            // row = xs[i]
            var assignItem = new AssignNode(expr.Iterator.Name, indexAccess);

            var bodySeq = new SequenceNode();
            bodySeq.Statements.Add(assignItem);

            // original body
            bodySeq.Statements.Add(expr.Body);

            // 🔥 decide if we need write-back
            Type elementType = new NullType();
            if (expr.Source.Type is DataframeType df) elementType = df.RowType;
            else if (expr.Source.Type is ArrayType arr) elementType = arr.ElementType;

            bool needsWriteBack =
                elementType is IntType ||
                elementType is FloatType ||
                elementType is BoolType;

            if (needsWriteBack)
            {
                // xs[i] = row
                var writeBack = new IndexAssignNode(
                    expr.Source,
                    iVar,
                    new IdNode(expr.Iterator.Name)
                );

                bodySeq.Statements.Add(writeBack);
            }

            var lowered = new ForLoopNode(init, cond, step, bodySeq);

            PerformSemanticAnalysis(lowered);
            return Visit(lowered);
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

            if (expr.Operator == "+" && leftType is RecordType l && rightType is RecordType r)
            {
                return BuildRecordMergeInline(expr, l, r);
            }

            throw new InvalidOperationException($"Cannot perform {expr.Operator} on {leftType} and {rightType}");
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

        private LLVMValueRef LoadField(LLVMValueRef recordPtr, int index)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // get slot pointer
            var slotPtr = _builder.BuildGEP2(
                i8Ptr,
                recordPtr,
                new[] { LLVMValueRef.CreateConstInt(i64, (ulong)index) }
            );

            // load pointer stored in slot
            var loadedPtr = _builder.BuildLoad2(i8Ptr, slotPtr, "loaded_field");

            return loadedPtr;
        }

        public LLVMValueRef BuildRecordMergeInline(BinaryOpNode expr, RecordType lhsType, RecordType rhsType)
        {
            var leftVal = Visit(expr.Left);
            var rightVal = Visit(expr.Right);

            var resultType = (RecordType)expr.Type;

            var resultValues = new List<LLVMValueRef>();

            var lhsIndex = lhsType.RecordFields
                .Select((f, i) => (f.Label, i))
                .ToDictionary(x => x.Label, x => x.i);

            var rhsIndex = rhsType.RecordFields
                .Select((f, i) => (f.Label, i))
                .ToDictionary(x => x.Label, x => x.i);

            foreach (var field in resultType.RecordFields)
            {
                if (rhsIndex.TryGetValue(field.Label, out var rIdx))
                {
                    resultValues.Add(LoadField(rightVal, rIdx)); // override
                }
                else
                {
                    var lIdx = lhsIndex[field.Label];
                    resultValues.Add(LoadField(leftVal, lIdx));
                }
            }

            return BuildRecordFromValues(resultType, resultValues);
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
        // x=dataframe(["name", "age"], type=[string, int])
        // x=dataframe(["name", "age"], [{name: "dan", age: 30}, {name: "alice", age: 25}])

        // x=record({name: "Hary potter", age: 30, rating: 10.5585})  

        // x.add({name: "Hary potter2", age: 201})
        // x.addRange([{name: "voldemort", age: 80}, {name: "dumbledore", age: 70}, {name: "MERLIN", age: 101}])

        // for(i=0; i<50; i++) x.add({name: "Hary potter", age: 10 + random(1,100)}) 
        // for(i=0; i<5200000; i++) x.add({name: "Hary potter", age: 10 + random(1,100)}) 
        // for(i=0; i<5000000; i++) x.add({name: "Hary potter", age: 10 + random(1,100)}) // 5 million
        // this below can't do random inside addRange "Cannot perform + on int and"
        // for(i=0; i<520000; i++) x.addRange([{name: "voldemort", age: 80}, {name: "dumbledore", age: 70}, {name: "MERLIN", age: 101}]) 

        // x.map(d => d.age + 10) // this should return the dataframe not the column
        // x.where(d=> d.age > 90)  
        // x.where(d=> d > 9).where(z=> z < 93)
        // x.where(d=> d.age > 91).where(z=> z.age < 93 & z.name=="Hary potter")
        // x.where(d=> d.savings > 693444.47).where(z=> z.savings < 6903444.47 & z.name=="John")

        // x=read_csv("CSV/test.csv")

        public LLVMValueRef VisitWhere(WhereNode expr)
        {
            var sourceType = expr.SourceExpr.Type;
            var program = new SequenceNode();

            // Handle source array type and different element types
            if (sourceType is ArrayType)
                program = WhereForArray(sourceType, expr);
            else if (sourceType is DataframeType)
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

            var step = new IncrementNode(new IdNode(iVar));

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

            var loopStep = new IncrementNode(new IdNode(indexVarName));
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
            SequenceNode program;

            // Check what the TYPE CHECKER said the result would be
            if (expr.Type is DataframeType)
            {
                program = MapForDataframe(expr);
            }
            else if (expr.Type is ArrayType)
            {
                // MapForArray works perfectly even if the source is a Dataframe,
                // because it just loops and performs the 'add' to a new array.
                program = MapForArray(expr);
            }
            else
                throw new Exception($"Unsupported map result type: {expr.Type}");

            PerformSemanticAnalysis(program);
            return VisitSequence(program);
        }

        public SequenceNode MapForArray(MapNode expr)
        {
            var program = new SequenceNode();
            var srcVar = "__map_src";
            var resVar = "__map_result";
            var iVar = "__map_i"; // Your variable is named iVar

            // 1. Get the correct element type from the MapNode's result type
            MyCompiler.Type resultElementType = null;
            if (expr.Type is ArrayType at) resultElementType = at.ElementType;
            else if (expr.Type is DataframeType df) resultElementType = df.RowType;

            // 2. Setup
            program.Statements.Add(new AssignNode(srcVar, expr.SourceExpr));

            // Initialize empty array with the correct ElementType
            var emptyArray = new ArrayNode(new List<ExpressionNode>());
            emptyArray.ElementType = resultElementType;

            program.Statements.Add(new AssignNode(resVar, emptyArray));
            program.Statements.Add(new AssignNode(iVar, new NumberNode(0)));

            var loopBody = new SequenceNode();

            // FIX: Use iVar here instead of indexVarName
            var rowAccess = new IndexNode(new IdNode(srcVar), new IdNode(iVar));

            // 3. Transform the lambda body
            ExpressionNode lastExpr = null;
            foreach (var e in expr.Assignments)
            {
                lastExpr = ReplaceIterator(e as ExpressionNode, expr.IteratorId.Name, rowAccess);
            }

            // 4. Add to the result list
            loopBody.Statements.Add(new AddNode(new IdNode(resVar), lastExpr));

            // 5. Loop Logic - Use iVar here as well
            var cond = new ComparisonNode(new IdNode(iVar), "<", new LengthNode(new IdNode(srcVar)));
            var step = new IncrementNode(new IdNode(iVar));
            program.Statements.Add(new ForLoopNode(null, cond, step, loopBody));

            program.Statements.Add(new IdNode(resVar));
            return program;
        }

        private string InferFieldName(Node node)
        {
            if (node == null) return null;

            // 1. Direct Assignment: x.longitude = 100.0
            // This is crucial for the Type Checker and the Compiler to know the column name.
            if (node is RecordFieldAssignNode rfan)
                return rfan.IdField;

            // 2. Property Access: x.longitude
            if (node is FieldNode rf)
                return rf.IdField;

            // 3. Binary Operations: x.longitude - 100.0
            if (node is BinaryOpNode bin)
                return InferFieldName(bin.Left) ?? InferFieldName(bin.Right);

            // 4. Fallback for ID nodes if necessary
            if (node is IdNode id)
                return id.Name;

            return null;
        }

        public SequenceNode MapForDataframe(MapNode expr)
        {
            var program = new SequenceNode();
            var srcVar = "__map_src";
            var resVar = "__map_result";
            var iVar = "__map_i";
            var currentRowVar = "__current_row";
            var dfType = (DataframeType)expr.Type;

            // 1. Assign source
            program.Statements.Add(new AssignNode(srcVar, expr.SourceExpr));

            // 2. Initialize Result Dataframe (Structure only)
            var columns = dfType.ColumnNames.Select(c => (ExpressionNode)new StringNode(c)).ToList();
            var dummyValues = dfType.DataTypes.Select(t => (ExpressionNode)new StringNode("")).ToList();
            var dfConstructor = new DataframeNode(new List<NamedArgumentNode> {
                new NamedArgumentNode("columns", new ArrayNode(columns)),
                new NamedArgumentNode("type", new ArrayNode(dummyValues))
            });
            dfConstructor.SetType(dfType);

            program.Statements.Add(new AssignNode(resVar, dfConstructor));
            program.Statements.Add(new AssignNode(iVar, new NumberNode(0)));

            var loopBody = new SequenceNode();
            var replacementNode = new IdNode(currentRowVar);

            // row = src[i]
            var rowAccess = new IndexNode(new IdNode(srcVar), new IdNode(iVar));
            loopBody.Statements.Add(new AssignNode(currentRowVar, rowAccess));

            bool isStatementStyle = expr.Assignments.Any(a =>
                a is AssignNode || a is RecordFieldAssignNode || a.GetType().Name.Contains("Assign")
            );

            if (expr.Assignments.Count == 1 && !isStatementStyle)
            {
                //if (_debug) Console.WriteLine("DEBUG: Compiling MAP using Path A (Functional)");

                // Path A logic
                var bodyNode = expr.Assignments.First();

                if (bodyNode is BinaryOpNode bin && bin.Operator == "+" && bin.Right is RecordNode rec)
                {
                    // If NodeContainsId correctly returns TRUE for { lat2: x.latitude }, 
                    // this block is skipped, and no hoisting occurs.
                    if (!NodeContainsId(rec, expr.IteratorId.Name) && IsPure(rec))
                    {
                        var invVar = "__const_record";
                        program.Statements.Add(new AssignNode(invVar, rec));
                        bodyNode = new BinaryOpNode(bin.Left, "+", new IdNode(invVar));
                    }
                }

                // Perform the replacement on the bodyNode (hoisted or not)
                var replacedBody = (ExpressionNode)ReplaceIteratorInNode(bodyNode, expr.IteratorId.Name, replacementNode);
                loopBody.Statements.Add(new AddNode(new IdNode(resVar), replacedBody));
            }
            else
            {
                //if (_debug) Console.WriteLine("DEBUG: Compiling MAP using Path B (Imperative)");

                // 1. Clone row (mutate safely)
                var cloneNode = new BinaryOpNode(new IdNode(currentRowVar), "+", new RecordNode(new List<NamedArgumentNode>()));
                cloneNode.SetType(dfType.RowType);
                loopBody.Statements.Add(new AssignNode(currentRowVar, cloneNode));

                // 2. Apply all assignments
                foreach (var action in expr.Assignments)
                {
                    var replacedAction = ReplaceIteratorInNode(action, expr.IteratorId.Name, replacementNode);
                    loopBody.Statements.Add((StatementNode)replacedAction);
                }

                // 3. Collect row
                loopBody.Statements.Add(new AddNode(new IdNode(resVar), new IdNode(currentRowVar)));
            }

            // 3. The Loop Structure
            var cond = new ComparisonNode(new IdNode(iVar), "<", new LengthNode(new IdNode(srcVar)));
            var step = new IncrementNode(new IdNode(iVar));
            program.Statements.Add(new ForLoopNode(null, cond, step, loopBody));

            // Return the new dataframe
            program.Statements.Add(new IdNode(resVar));

            return program;
        }

        // Helper to check if a node tree uses a specific variable
        private bool IsPure(Node node)
        {
            if (node == null) return true;
            if (node is RandomNode) return false;

            if (node is RecordNode rec)
            {
                foreach (var f in rec.Fields)
                    if (!IsPure(f.Value)) return false;
            }

            if (node is BinaryOpNode bin)
                return IsPure(bin.Left) && IsPure(bin.Right);

            return true;
        }

        private bool NodeContainsId(Node node, string idName)
        {
            if (node == null) return false;

            if (node is IdNode id) return id.Name == idName;

            if (node is FieldNode rf)
                return NodeContainsId(rf.SourceExpression, idName);

            if (node is RecordNode rec)
            {
                return rec.Fields.Any(f =>
                {
                    // Check the NamedArgumentNode inside the RecordField
                    if (f.Value is NamedArgumentNode nan)
                        return NodeContainsId(nan.Value, idName);
                    return NodeContainsId(f.Value, idName);
                });
            }

            if (node is BinaryOpNode bin)
                return NodeContainsId(bin.Left, idName) || NodeContainsId(bin.Right, idName);

            return false;
        }

        public LLVMValueRef VisitCopy(CopyNode expr)
        {
            var value = Visit(expr.SourceExpression);
            return EmitDeepCopy(value, expr.SourceExpression.Type);
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

            // 1. Allocate the NEW Dataframe header container
            var newDfPtr = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(),
                new[] { LLVMValueRef.CreateConstInt(ctx.Int64Type, 24) }, "df_copy_header");

            // 2. Deep Copy Columns (Array of Strings)
            var colsPtr = _builder.BuildLoad2(i8Ptr, _builder.BuildStructGEP2(_dataframeStruct, dfPtr, 0), "ld_cols");
            var columnArrayType = new ArrayType(new StringType()); // Columns are always strings
            var newCols = CopyArray(colsPtr, columnArrayType);
            _builder.BuildStore(newCols, _builder.BuildStructGEP2(_dataframeStruct, newDfPtr, 0));

            // 3. Deep Copy Rows (Array of Records)
            var rowsPtr = _builder.BuildLoad2(i8Ptr, _builder.BuildStructGEP2(_dataframeStruct, dfPtr, 1), "ld_rows");
            var rowArrayType = new ArrayType(dfType.RowType); // Uses the RowType from your DataframeType
            var newRows = CopyArray(rowsPtr, rowArrayType);
            _builder.BuildStore(newRows, _builder.BuildStructGEP2(_dataframeStruct, newDfPtr, 1));

            // 4. Deep Copy DataTypes (Array of Integers)
            var typesPtr = _builder.BuildLoad2(i8Ptr, _builder.BuildStructGEP2(_dataframeStruct, dfPtr, 2), "ld_types");
            var typeArrayType = new ArrayType(new IntType()); // Metadata array is always ints
            var newTypes = CopyArray(typesPtr, typeArrayType);
            _builder.BuildStore(newTypes, _builder.BuildStructGEP2(_dataframeStruct, newDfPtr, 2));

            return newDfPtr;
        }

        private LLVMValueRef CopyRecord(LLVMValueRef recordPtr, RecordType recordType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;

            // i8*  (field type)
            var slotType = LLVMTypeRef.CreatePointer(i8, 0);

            // i8** (array of fields)
            var slotArrayType = LLVMTypeRef.CreatePointer(slotType, 0);

            var mallocFunc = GetOrDeclareMalloc();

            var numFields = (ulong)recordType.RecordFields.Count;

            // 1. Allocate raw buffer for new record (numFields * 8 bytes)
            var newRecordBuffer = _builder.BuildCall2(
                _mallocType,
                mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, numFields * (ulong)IntPtr.Size) },
                "record_copy_buffer"
            );

            // 2. Cast BOTH source and destination to i8**
            var srcBuffer = _builder.BuildBitCast(recordPtr, slotArrayType, "src_slots");
            var dstBuffer = _builder.BuildBitCast(newRecordBuffer, slotArrayType, "dst_slots");

            for (int i = 0; i < recordType.RecordFields.Count; i++)
            {
                var fieldType = recordType.RecordFields[i].Type;
                var index = LLVMValueRef.CreateConstInt(i64, (ulong)i);

                // 3. Get pointer to field slot in source
                var srcSlotPtr = _builder.BuildGEP2(
                    slotType,   // element type = i8*
                    srcBuffer,  // i8**
                    new[] { index },
                    "src_slot"
                );

                // 4. Load stored pointer (i8*)
                var storedPtr = _builder.BuildLoad2(slotType, srcSlotPtr, "stored_ptr");

                // 5. Deep copy the value
                var copiedValuePtr = EmitDeepCopy(storedPtr, fieldType);

                // 6. Get pointer to destination slot
                var dstSlotPtr = _builder.BuildGEP2(
                    slotType,
                    dstBuffer,
                    new[] { index },
                    "dst_slot"
                );

                // 7. Store copied pointer
                _builder.BuildStore(copiedValuePtr, dstSlotPtr);
            }

            // 8. Return raw buffer (external representation = i8*)
            return newRecordBuffer;
        }

        private LLVMValueRef CopyArray(LLVMValueRef srcHeaderPtr, ArrayType arrayType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);
            var elementType = arrayType.ElementType;

            var mallocFunc = GetOrDeclareMalloc();

            // 1. Load Length and Data Pointer
            var srcLenPtr = _builder.BuildStructGEP2(_arrayStruct, srcHeaderPtr, 0, "src_len_ptr");
            var srcDataPtrField = _builder.BuildStructGEP2(_arrayStruct, srcHeaderPtr, 2, "src_data_ptr_field");
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
            _builder.BuildStore(length, _builder.BuildStructGEP2(_arrayStruct, newHeaderPtr, 0));
            _builder.BuildStore(length, _builder.BuildStructGEP2(_arrayStruct, newHeaderPtr, 1));
            _builder.BuildStore(newDataPtr, _builder.BuildStructGEP2(_arrayStruct, newHeaderPtr, 2));

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

        // Not used!
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

            // 4. i++
            var loopStep = new IncrementNode(new IdNode(indexVarName));

            // 5. src[i]
            var currentElement = new IndexNode(
                new IdNode(srcVarName),
                new IdNode(indexVarName)
            );

            // 6. Replace iterator (d => ...) with actual element
            var mappedExpr = ReplaceIterator(
                expr.Assignments[0] as ExpressionNode,
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

        public Node ReplaceIteratorInNode(Node node, string iteratorName, ExpressionNode replacement)
        {
            if (node == null) return null;

            // Handle Record Field Assignment (x.latitude = 5)
            if (node is RecordFieldAssignNode rfan)
            {
                var newRec = ReplaceIterator(rfan.IdRecord, iteratorName, replacement);
                var newExpr = ReplaceIterator(rfan.AssignExpression, iteratorName, replacement);
                return new RecordFieldAssignNode(newRec, rfan.IdField, newExpr);
            }

            // Handle standard Assignment (y = x.latitude)
            if (node is AssignNode ass)
            {
                var newExpr = ReplaceIterator(ass.Expression, iteratorName, replacement);
                return new AssignNode(ass.Id, newExpr);
            }

            // Fallback to expression replacer
            if (node is ExpressionNode expr)
            {
                return ReplaceIterator(expr, iteratorName, replacement);
            }

            return node;
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

        private bool IsReferenceType(Type t)
        {
            return t is StringType || t is ArrayType || t is RecordType || t is DataframeType;
        }

        private uint GetTypeSize(Type type)
        {
            if (type is IntType || type is FloatType) return 8;
            if (type is BoolType) return 1;
            if (type is StringType || type is ArrayType || type is RecordType || type is DataframeType) return 8; // pointer size
            throw new Exception("Unknown type for size calculation: " + type);
        }

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

        public LLVMValueRef VisitArray(ArrayNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            uint count = (uint)expr.Elements.Count;

            var mallocFunc = GetOrDeclareMalloc();

            // 1. Resolve element type            
            var elementType = GetLLVMType(expr.ElementType);
            var elementPtrType = LLVMTypeRef.CreatePointer(elementType, 0);

            bool isRefType = IsReferenceType(expr.ElementType);

            // 2. Compute sizes using elementType
            uint elementSize = isRefType
            ? (uint)IntPtr.Size   // 8 bytes
            : GetTypeSize(expr.ElementType);

            uint capacity = count > 0 ? Math.Min(count * 2, 100) : 100; // we set very high capacity to avoid frequent reallocations, should be handled better
            if (expr.Capacity != null)
                capacity = Math.Min(count * 2, 100);

            uint totalSize = elementSize * capacity;

            // 3. Allocate header (len, cap, data*)            
            var headerSize = GetStructSize(expr.Type);

            var headerRaw = _builder.BuildCall2(
                _mallocType,
                mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, headerSize) },
                "arr_header"
            );

            // 4. Allocate data buffer            
            var rawDataPtr = _builder.BuildCall2(
                _mallocType,
                mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, totalSize) },
                "arr_data_raw"
            );

            // Cast i8* → elementType*
            var dataPtr = _builder.BuildBitCast(rawDataPtr, elementPtrType, "arr_data");

            // 5. Store metadata
            var lenPtr = _builder.BuildStructGEP2(_arrayStruct, headerRaw, 0, "len_ptr");
            var capPtr = _builder.BuildStructGEP2(_arrayStruct, headerRaw, 1, "cap_ptr");
            var dataFieldPtr = _builder.BuildStructGEP2(_arrayStruct, headerRaw, 2, "data_field_ptr");

            _builder.BuildStore(
                LLVMValueRef.CreateConstInt(i64, count),
                lenPtr
            ).SetAlignment(8);

            _builder.BuildStore(
                LLVMValueRef.CreateConstInt(i64, capacity),
                capPtr
            ).SetAlignment(8);

            // Store as i8* in struct
            var dataAsI8 = _builder.BuildBitCast(dataPtr, i8Ptr, "data_as_i8");
            _builder.BuildStore(dataAsI8, dataFieldPtr);

            // 6. Populate elements
            for (int i = 0; i < expr.Elements.Count; i++)
            {
                var val = Visit(expr.Elements[i]);
                var idx = LLVMValueRef.CreateConstInt(i64, (ulong)i);

                var elementPtr = _builder.BuildGEP2(
                    elementType,
                    dataPtr,
                    new[] { idx },
                    "elem_ptr"
                );

                if (isRefType)
                {
                    // Ensure value is pointer-compatible
                    var castVal = _builder.BuildBitCast(val, elementType, "ref_cast");
                    _builder.BuildStore(castVal, elementPtr);
                }
                else
                {
                    // Value types stored directly
                    _builder.BuildStore(val, elementPtr).SetAlignment(elementSize);
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

            var sourceType = expr.SourceExpression.Type;

            if (sourceType is ArrayType)
            {
                if (indexVal.TypeOf == ctx.DoubleType)
                    indexVal = _builder.BuildFPToSI(indexVal, i64, "idx_int");

                // 2. Load Metadata from Header
                var lenFieldPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 0, "len_field_ptr");
                var dataFieldPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 2, "data_field_ptr");

                var arrayLen = _builder.BuildLoad2(i64, lenFieldPtr, "array_len");
                arrayLen.SetAlignment(8);
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
                    RecordType => _builder.BuildBitCast(rawValue, i8Ptr),
                    DataframeType => _builder.BuildBitCast(rawValue, i8Ptr),
                    NullType => _builder.BuildTrunc(rawValue, i8Ptr),
                    _ => rawValue
                };
            }
            else if (sourceType is DataframeType)
            {
                LLVMValueRef result;

                if (expr.IndexExpression.Type is IntType)
                {
                    result = DataframeIndex(headerPtr, indexVal);
                    return _builder.BuildBitCast(result, i8Ptr);
                }
            }

            return default;
        }

        public SequenceNode ColumnAccessForDataframe(FieldNode indexNode) 
        {
            var dfType = indexNode.SourceExpression.Type as DataframeType
                ?? throw new Exception("Expected dataframe type");

            var columnName = indexNode.IdField;

            var fieldType = default(Type);
            foreach (var item in dfType.RowType.RecordFields)
            {
                if (item.Label == columnName)
                {
                    fieldType = item.Type;
                    break;
                }
            }

            // Variables
            var srcVar = "__col_src";
            var resultVar = "__col_result";
            var iVar = "__col_i";
            var lenVar = "__col_len";
            var rowVar = "__col_row";

            // 1. result = []
            var resultConstructor = new ArrayNode(new List<ExpressionNode>())
            {
                ElementType = fieldType
            };
            resultConstructor.SetType(new ArrayType(fieldType));

            // 2. Assignments
            var srcAssign = new AssignNode(srcVar, new IdNode(((IdNode)indexNode.SourceExpression).Name));
            var resultAssign = new AssignNode(resultVar, resultConstructor);
            var indexInit = new AssignNode(iVar, new NumberNode(0));
            var lenAssign = new AssignNode(lenVar, new LengthNode(new IdNode(srcVar)));

            // 3. Loop condition & step
            var cond = new ComparisonNode(new IdNode(iVar), "<", new IdNode(lenVar));
            var step = new IncrementNode(new IdNode(iVar));

            var loopBody = new SequenceNode();

            // row = src[i]
            var rowAccess = new IndexNode(new IdNode(srcVar), new IdNode(iVar)); 
            rowAccess.SetType(dfType.RowType);

            var rowAssign = new AssignNode(rowVar, rowAccess);
            loopBody.Statements.Add(rowAssign);

            // value = row[columnIndex]
            var valueExpr = new FieldNode(new IdNode(rowVar), columnName);

            valueExpr.SetType(fieldType);

            // result.add(value)
            var addNode = new AddNode(new IdNode(resultVar), valueExpr);
            loopBody.Statements.Add(addNode);

            var loop = new ForLoopNode(indexInit, cond, step, loopBody);

            // 4. Program
            var program = new SequenceNode();
            program.Statements.Add(srcAssign);
            program.Statements.Add(resultAssign);
            program.Statements.Add(lenAssign);
            program.Statements.Add(loop);
            program.Statements.Add(new IdNode(resultVar));

            return program;
        }

        private LLVMValueRef DataframeIndex(LLVMValueRef dataframePtr, LLVMValueRef indexValue)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var dfType = GetOrCreateDataframeType();
            var arrayType = GetOrCreateArrayType();

            var func = _builder.InsertBlock.Parent;

            // Load rows
            var rowsPtrPtr = _builder.BuildStructGEP2(dfType, dataframePtr, 1, "rows_ptr_ptr");
            var rowsPtr = _builder.BuildLoad2(i8Ptr, rowsPtrPtr, "rows");

            var lenPtr = _builder.BuildStructGEP2(arrayType, rowsPtr, 0, "len_ptr");
            var len = _builder.BuildLoad2(i64, lenPtr, "len");

            var dataPtrPtr = _builder.BuildStructGEP2(arrayType, rowsPtr, 2, "data_ptr_ptr");
            var dataPtr = _builder.BuildLoad2(i8Ptr, dataPtrPtr, "data");

            // ensure i64
            if (indexValue.TypeOf != i64)
                indexValue = _builder.BuildIntCast(indexValue, i64, "idx_cast");

            // conditions
            var isEmpty = _builder.BuildICmp(
                LLVMIntPredicate.LLVMIntEQ,
                len,
                LLVMValueRef.CreateConstInt(i64, 0),
                "is_empty"
            );

            var inBounds = _builder.BuildICmp(
                LLVMIntPredicate.LLVMIntULT,
                indexValue,
                len,
                "in_bounds"
            );

            var invalid = _builder.BuildOr(isEmpty, _builder.BuildNot(inBounds), "invalid");

            // blocks            
            var okBlock = ctx.AppendBasicBlock(func, "df_idx_ok");
            var errBlock = ctx.AppendBasicBlock(func, "df_idx_err");
            var mergeBlock = ctx.AppendBasicBlock(func, "df_idx_merge");

            _builder.BuildCondBr(invalid, errBlock, okBlock);

            // ERROR BLOCK
            _builder.PositionAtEnd(errBlock);

            var msg = _builder.BuildGlobalStringPtr(
                "Runtime Error: Cannot index dataframe (empty or out of bounds)\n",
                "df_idx_err_msg"
            );

            _builder.BuildCall2(_printfType, _printf, new[] { msg }, "print_err");

            var nullVal = LLVMValueRef.CreateConstNull(i8Ptr);

            _builder.BuildBr(mergeBlock); // jump to merge

            var errEndBlock = _builder.InsertBlock; // capture block for PHI

            // OK BLOCK            
            _builder.PositionAtEnd(okBlock);

            var elemPtr = _builder.BuildGEP2(
                i8Ptr,
                dataPtr,
                new[] { indexValue },
                "elem_ptr"
            );

            var okVal = _builder.BuildLoad2(i8Ptr, elemPtr, "record");

            _builder.BuildBr(mergeBlock); // jump to merge

            var okEndBlock = _builder.InsertBlock;

            // MERGE BLOCK
            _builder.PositionAtEnd(mergeBlock);

            var phi = _builder.BuildPhi(i8Ptr, "df_idx_result");

            phi.AddIncoming(
                new[] { nullVal, okVal },
                new[] { errEndBlock, okEndBlock },
                2
            );

            return phi;
        }

        private LLVMTypeRef GetOrCreateRecordStructType(RecordType recordType)
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
                _ => throw new Exception($"Unsupported type: {type}")
            };
        }

        public LLVMValueRef VisitAdd(AddNode expr)
        {
            var sourceType = expr.SourceExpression.Type;

            if (_debug) Console.WriteLine("semantic type of array being indexed: " + sourceType);

            if (sourceType is ArrayType)
                return AddToArray(expr);
            else if (sourceType is DataframeType dfType)
                return AddToDataframe(expr, dfType); // Pass the type along

            throw new Exception("Add operation is only supported on arrays and dataframes");
        }

        private LLVMValueRef AddToArray(AddNode expr)
        {
            var headerPtr = Visit(expr.SourceExpression);
            ExecuteArrayAddition(headerPtr, Visit(expr.AddExpression), expr.AddExpression.Type);

            return headerPtr;
        }

        public LLVMValueRef AddToDataframe(AddNode expr, DataframeType dfType)
        {
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 1. Get dataframe pointer
            var dfPtr = Visit(expr.SourceExpression);

            // 2. Evaluate the value to add (THIS IS THE IMPORTANT PART)
            // This should already be a record pointer
            var valueToAdd = Visit(expr.AddExpression);

            // Sanity check (optional but very useful while debugging)
            if (!(expr.AddExpression.Type is RecordType))
                throw new Exception("Can only add records to a dataframe");

            // 3. Get rows array pointer: df.rows
            var rowsFieldPtr = _builder.BuildStructGEP2(_dataframeStruct, dfPtr, 1, "rows_field");
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

            // 1. Resolve types
            LLVMTypeRef llvmElementType = GetLLVMType(elementType);
            bool isReferenceType = IsReferenceType(elementType);

            var elementPtrType = LLVMTypeRef.CreatePointer(llvmElementType, 0);

            // 2. Load array metadata
            var lenPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 0, "len_ptr");
            // lenPtr.SetAlignment(8);
            var capPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 1, "cap_ptr");
            //capPtr.SetAlignment(8);
            var dataPtrPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 2, "data_ptr_ptr");

            var length = _builder.BuildLoad2(i64, lenPtr, "len");
            length.SetAlignment(8);
            var capacity = _builder.BuildLoad2(i64, capPtr, "cap");
            capacity.SetAlignment(8);
            var rawDataPtr = _builder.BuildLoad2(i8Ptr, dataPtrPtr, "data");

            // 3. Check capacity
            var isFull = _builder.BuildICmp(
                LLVMIntPredicate.LLVMIntUGE,
                length,
                capacity,
                "is_full"
            );

            var function = _builder.InsertBlock.Parent;

            var growBlock = ctx.AppendBasicBlock(function, "grow");
            var contBlock = ctx.AppendBasicBlock(function, "cont");

            // IMPORTANT: capture block BEFORE branching
            var entryBlock = _builder.InsertBlock;

            _builder.BuildCondBr(isFull, growBlock, contBlock);

            // GROW BLOCK
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

            uint elementSize = isReferenceType
                ? (uint)IntPtr.Size
                : GetTypeSize(elementType);

            var newByteSize = _builder.BuildMul(
                newCap,
                LLVMValueRef.CreateConstInt(i64, elementSize),
                "bytes"
            );

            var reallocFunc = GetOrDeclareRealloc();

            var newRawDataPtr = _builder.BuildCall2(
                _reallocType,
                reallocFunc,
                new[] { rawDataPtr, newByteSize },
                "realloc"
            );

            _builder.BuildStore(newCap, capPtr).SetAlignment(8);
            _builder.BuildStore(newRawDataPtr, dataPtrPtr);

            _builder.BuildBr(contBlock);

            // CONT BLOCK
            _builder.PositionAtEnd(contBlock);

            var phi = _builder.BuildPhi(i8Ptr, "final_data_ptr");

            phi.AddIncoming(
                new LLVMValueRef[] { rawDataPtr, newRawDataPtr },
                new LLVMBasicBlockRef[] { entryBlock, growBlock },
                2
            );

            var finalTypedPtr = _builder.BuildBitCast(phi, elementPtrType);

            // 4. Compute target slot
            var targetPtr = _builder.BuildGEP2(
                llvmElementType,
                finalTypedPtr,
                new[] { length },
                "target"
            );

            // 5. Store value
            LLVMValueRef storedValue;

            if (isReferenceType) // Already a pointer → just ensure type matches 
                storedValue = _builder.BuildBitCast(valueToAdd, llvmElementType);
            else
                storedValue = valueToAdd;

            _builder.BuildStore(storedValue, targetPtr);

            // 6. Increment length
            var newLen = _builder.BuildAdd(
                length,
                LLVMValueRef.CreateConstInt(i64, 1),
                "new_len"
            );

            _builder.BuildStore(newLen, lenPtr).SetAlignment(8);
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
            var sourceType = expr.SourceExpression.Type;

            if (_debug) Console.WriteLine("semantic type of array being indexed: " + sourceType);

            if (sourceType is ArrayType arrayType)
                return RemoveFromArray(expr, arrayType);
            else if (sourceType is DataframeType)
                return RemoveFromDataframe(expr);

            throw new Exception("Remove operation is only supported on arrays and dataframes");
        }

        public LLVMValueRef RemoveFromArray(RemoveNode expr, ArrayType arrayType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var headerPtr = Visit(expr.SourceExpression);
            var indexVal = Visit(expr.RemoveExpression);

            // 1. Determine if this is a packed (boolean) array
            bool isBool = arrayType.ElementType is BoolType;

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

        public LLVMValueRef RemoveFromDataframe(RemoveNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 1. Define the Dataframe and Array Header Types
            // Dataframe: { ptr cols, ptr rows, ptr types }
            var dfStructType = LLVMTypeRef.CreateStruct(new[] { i8Ptr, i8Ptr, i8Ptr }, false);
            // Array Header: { i64 len, i64 cap, i8* data }
            var arrayHeaderType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);

            // 2. Get the Dataframe pointer and the index to remove
            var dfPtr = Visit(expr.SourceExpression);
            var indexVal = Visit(expr.RemoveExpression);

            // 3. Access df->rows (index 1 in the Dataframe struct)
            var rowsFieldPtr = _builder.BuildStructGEP2(dfStructType, dfPtr, 1, "df_rows_field");
            var rowsHeaderPtr = _builder.BuildLoad2(i8Ptr, rowsFieldPtr, "rows_header_ptr");

            // 4. Access the fields inside the rows array header
            var lenPtr = _builder.BuildStructGEP2(arrayHeaderType, rowsHeaderPtr, 0, "rows_len_ptr");
            var dataFieldPtr = _builder.BuildStructGEP2(arrayHeaderType, rowsHeaderPtr, 2, "rows_data_ptr_ptr");

            var currentLen = _builder.BuildLoad2(i64, lenPtr, "current_len");
            var dataPtr = _builder.BuildLoad2(i8Ptr, dataFieldPtr, "data_ptr");

            // 5. Calculate Move logic (same as RemoveFromArray)
            // We are moving pointers (Records), so stride is 8 bytes
            var gepStrideType = i8Ptr;
            var strideSize = LLVMValueRef.CreateConstInt(i64, 8);

            // Destination: where the removed element is
            var dstPtr = _builder.BuildGEP2(gepStrideType, dataPtr, new[] { indexVal }, "dst_ptr");

            // Source: the element immediately after
            var nextIdx = _builder.BuildAdd(indexVal, LLVMValueRef.CreateConstInt(i64, 1), "next_idx");
            var srcPtr = _builder.BuildGEP2(gepStrideType, dataPtr, new[] { nextIdx }, "src_ptr");

            // Bytes to move: (length - index - 1) * 8
            var numElementsToMove = _builder.BuildSub(
                _builder.BuildSub(currentLen, indexVal),
                LLVMValueRef.CreateConstInt(i64, 1)
            );
            var bytesToMove = _builder.BuildMul(numElementsToMove, strideSize, "bytes_to_move");

            // 6. Execute Memmove
            var memmoveFunc = GetOrDeclareMemmove();
            // Signature: void memmove(void* dst, void* src, i64 len)
            _builder.BuildCall2(_memmoveType, memmoveFunc, new[] { dstPtr, srcPtr, bytesToMove }, "");

            // 7. Update the length of the rows array
            var newLen = _builder.BuildSub(currentLen, LLVMValueRef.CreateConstInt(i64, 1));
            _builder.BuildStore(newLen, lenPtr);

            // Return the dataframe pointer (standard for chaining/assignments)
            return dfPtr;
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
                // Step 1: get pointer to the 'rows' field
                var rowsFieldPtr = _builder.BuildStructGEP2(_dataframeStruct, sourcePtr, 1, "rows_ptr_field");

                // Step 2: load the pointer to the ArrayObject
                var rowsPtr = _builder.BuildLoad2(i8Ptr, rowsFieldPtr, "rows_ptr");

                // Step 3: cast to ArrayObject*
                var rowsArrayPtr = _builder.BuildBitCast(rowsPtr, LLVMTypeRef.CreatePointer(_arrayStruct, 0), "rows_array_ptr");

                // Step 4: get the 'length' field
                var lenPtr = _builder.BuildStructGEP2(_arrayStruct, rowsArrayPtr, 0, "rows_length_ptr");
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
                // Step 1: get pointer to the 'columns' field
                var columnsFieldPtr = _builder.BuildStructGEP2(_dataframeStruct, sourcePtr, 0, "columns_ptr_field");

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
            var length = _builder.BuildLoad2(i64, lengthGEP, "length");  // Load the array length
            length.SetAlignment(8);
            return length;
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

        public LLVMValueRef VisitRecord(RecordNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            var slotType = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0); // i8*
            var slotArrayType = LLVMTypeRef.CreatePointer(slotType, 0); // i8**

            var mallocFunc = GetOrDeclareMalloc();

            var numFields = (ulong)expr.Fields.Count;

            // Allocate raw memory
            var rawBuffer = _builder.BuildCall2(
                _mallocType,
                mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, numFields * 8) },
                "record_buffer"
            );

            // CRITICAL: cast to i8**
            var buffer = _builder.BuildBitCast(rawBuffer, slotArrayType, "record_slots");

            for (int i = 0; i < expr.Fields.Count; i++)
            {
                var val = Visit(expr.Fields[i].Value);
                LLVMValueRef fieldPtr;

                bool isPrimitive =
                    val.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind ||
                    val.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind;

                if (isPrimitive)
                {
                    var mem = _builder.BuildCall2(
                        _mallocType,
                        mallocFunc,
                        new[] { LLVMValueRef.CreateConstInt(i64, 8) },
                        "field_mem"
                    );

                    var castPtr = _builder.BuildBitCast(
                        mem,
                        LLVMTypeRef.CreatePointer(val.TypeOf, 0),
                        "cast"
                    );

                    _builder.BuildStore(val, castPtr).SetAlignment(8);

                    fieldPtr = mem;
                }
                else
                {
                    fieldPtr = _builder.BuildBitCast(val, i8Ptr, "to_i8ptr");
                }

                // NOW GEP IS CORRECT (step = 8 bytes)
                var index = LLVMValueRef.CreateConstInt(i64, (ulong)i);

                var slotPtr = _builder.BuildGEP2(
                    slotType,   // NOT i8Ptr variable reused — must match exactly
                    buffer,
                    new[] { index },
                    "field_ptr"
                );

                _builder.BuildStore(fieldPtr, slotPtr);
            }

            return rawBuffer; // keep external representation as i8*
        }

        public LLVMValueRef VisitRecordFieldAssign(RecordFieldAssignNode expr)
        {
            // Use the helper to get the address of the slot in the record
            var (fieldSlotPtr, _) = GetFieldPointer(expr.IdRecord, expr.IdField);

            var newValue = Visit(expr.AssignExpression);

            // Get type info for boxing logic
            var recType = (RecordType)expr.IdRecord.Type;
            var fieldDef = recType.RecordFields.First(f => f.Label == expr.IdField);

            if (!IsReferenceType(fieldDef.Type))
            {
                // Primitives are boxed: load the pointer to the box, then store the value
                var boxPtr = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0), fieldSlotPtr, "box_ptr");
                var castBoxPtr = _builder.BuildBitCast(boxPtr, LLVMTypeRef.CreatePointer(newValue.TypeOf, 0));
                _builder.BuildStore(newValue, castBoxPtr);
            }
            else
            {
                // Strings/Records: Store the pointer directly in the slot
                _builder.BuildStore(newValue, fieldSlotPtr);
            }

            return newValue;
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

        private int GetFieldIndex(string name, IReadOnlyList<RecordField> Fields)
        {
            for (int i = 0; i < Fields.Count; i++)
            {
                if (Fields[i].Label == name)
                    return i;
            }
            throw new Exception($"Field '{name}' not found.");
        }

        public LLVMValueRef VisitField(FieldNode expr)
        {
            if (expr.SourceExpression.Type is RecordType)
            {
                var (fieldSlotPtr, _) = GetFieldPointer(expr.SourceExpression, expr.IdField);
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
            else if (expr.SourceExpression.Type is DataframeType)
            {
                var d = ColumnAccessForDataframe(expr);
                PerformSemanticAnalysis(d);
                return Visit(d);
            }
            else
            {
                throw new Exception("Field access is only supported on records");
            }
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

            // 6. Return 
            return default;
        }

        public LLVMValueRef VisitReadCsv(ReadCsvNode expr)
        {
            var pathValue = Visit(expr.FileNameExpr);

            // This will now always be populated by the Type Checker
            RecordNode recordSchema = (RecordNode)expr.SchemaExpr;

            string schemaString = GetSchemaString(recordSchema);
            var schemaValue = _builder.BuildGlobalStringPtr(schemaString, "csv_schema_code");

            var readCsvFn = GetOrDeclareReadCsvInternal();
            var boxedResult = _builder.BuildCall2(_readCsvInternalType, readCsvFn, new[] { pathValue, schemaValue }, "csv_boxed_res");

            // 4. Unbox the Rows Array Pointer { i64 tag, ptr data } from the RuntimeValue
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            var dataPtrAddr = _builder.BuildStructGEP2(_runtimeValueType, boxedResult, 1, "unbox_ptr");
            var rawRowsPtr = _builder.BuildLoad2(i8Ptr, dataPtrAddr, "raw_rows_ptr");

            // 5. Construct the DataFrame using the inferred or provided RecordNode
            return BuildDataframeInternal(recordSchema, rawRowsPtr);
        }

        // Optional: get the constant string value at compile-time
        private string GetConstantString(ExpressionNode expr)
        {
            if (expr is StringNode sn)
                return sn.Value;
            throw new InvalidOperationException("Not a stringNode: " + expr.Type);
        }

        // Move your dataframe construction logic into this helper
        private LLVMValueRef BuildDataframeInternal(RecordNode schema, LLVMValueRef rowsPtr)
        {
            // Use your existing helpers for these (ensure these methods exist in your class)
            var colNamesArray = GenerateColumnNamesArray(schema);
            var dataTypesArray = GenerateDataTypesArray(schema);

            var dfPtr = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(),
                          new[] { LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 24) }, "df_ptr");

            var c1 = _builder.BuildStructGEP2(_dataframeStruct, dfPtr, 0, "cols");
            var c2 = _builder.BuildStructGEP2(_dataframeStruct, dfPtr, 1, "rows");
            var c3 = _builder.BuildStructGEP2(_dataframeStruct, dfPtr, 2, "types");

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
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)count), _builder.BuildStructGEP2(_arrayStruct, header, 0, "len"));
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)count), _builder.BuildStructGEP2(_arrayStruct, header, 1, "cap"));
            _builder.BuildStore(data, _builder.BuildStructGEP2(_arrayStruct, header, 2, "data"));

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
                int typeId = GetTypeByTag(schema.Fields[i].Type);

                var target = _builder.BuildGEP2(i8Ptr, data, new[] { LLVMValueRef.CreateConstInt(i64, (ulong)i) }, "ptr");
                // bitcast integer to ptr for storage in the pointer array
                var valAsPtr = _builder.BuildIntToPtr(LLVMValueRef.CreateConstInt(i64, (ulong)typeId), i8Ptr);
                _builder.BuildStore(valAsPtr, target);
            }

            // Set len=count, cap=count, data=data
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)count), _builder.BuildStructGEP2(_arrayStruct, header, 0, ""));
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)count), _builder.BuildStructGEP2(_arrayStruct, header, 1, ""));
            _builder.BuildStore(data, _builder.BuildStructGEP2(_arrayStruct, header, 2, ""));

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

        // Not used!
        private LLVMTypeRef GetArrayPtrType()
        {
            return LLVMTypeRef.CreatePointer(GetOrCreateArrayType(), 0);
        }

        // Not used!
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

            // 1. Source Data
            var sourceDfPtr = Visit(expr.Source);
            var sourceRowsArrayHeader = _builder.BuildLoad2(i8Ptr, _builder.BuildStructGEP2(_dataframeStruct, sourceDfPtr, 1), "src_rows_ptr");
            var lenPtr = _builder.BuildStructGEP2(_arrayStruct, sourceRowsArrayHeader, 0, "len_ptr");
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
                var sourceDataPtr = _builder.BuildLoad2(i8Ptr, _builder.BuildStructGEP2(_arrayStruct, sourceRowsArrayHeader, 2), "src_data");
                var rowPtrPtr = _builder.BuildGEP2(i8Ptr, sourceDataPtr, new[] { indexRef });
                var oldRecordPtr = _builder.BuildLoad2(i8Ptr, rowPtrPtr, "old_rec");

                _builder.BuildStore(oldRecordPtr, itSlot);

                // Update context so RecordFieldNode knows where to look
                _context = _context.Add("$row", itSlot, null!, sourceType.RowType); // unsure again about the context.add in code gen
                var rowId = new IdNode("$row");
                rowId.SetType(sourceType.RowType);

                // --- CRITICAL: Initialize list INSIDE the loop ---
                var projectedValues = new List<LLVMValueRef>();

                foreach (var colName in resultType.ColumnNames)
                {
                    var fieldAccess = new FieldNode(rowId, colName);
                    var fType = sourceType.RowType.RecordFields.First(f => f.Label == colName).Type;
                    fieldAccess.SetType(fType);

                    // VisitRecordField now returns unboxed raw values (i64, double, or i8* for strings)
                    LLVMValueRef rawValue = VisitField(fieldAccess);
                    projectedValues.Add(rawValue);
                }

                // --- CRITICAL: Build record INSIDE the loop ---
                // This ensures the malloc(size) is called with (3 * 8) = 24, not 0
                var newRecordPtr = BuildRecordFromValues(resultType.RowType, projectedValues);

                var resultDataPtr = _builder.BuildLoad2(i8Ptr, _builder.BuildStructGEP2(_arrayStruct, resultArrayHeader, 2), "res_data");
                var resElemPtr = _builder.BuildGEP2(i8Ptr, resultDataPtr, new[] { indexRef });
                _builder.BuildStore(newRecordPtr, resElemPtr);
            });

            // 5. Final Assembly
            var dfPtr = _builder.BuildMalloc(_dataframeStruct, "df_show");
            _builder.BuildStore(newColsPtr, _builder.BuildStructGEP2(_dataframeStruct, dfPtr, 0));
            _builder.BuildStore(resultArrayHeader, _builder.BuildStructGEP2(_dataframeStruct, dfPtr, 1));
            _builder.BuildStore(newDataTypesPtr, _builder.BuildStructGEP2(_dataframeStruct, dfPtr, 2));

            return dfPtr;
        }

        private LLVMValueRef AllocateArrayHeader(LLVMValueRef count)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;

            // Allocate 24-byte header
            var header = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(), new[] { LLVMValueRef.CreateConstInt(i64, 24) }, "arr_header");

            // Allocate data buffer (8 bytes per record pointer)
            var dataSize = _builder.BuildMul(count, LLVMValueRef.CreateConstInt(i64, 8), "data_size");
            var dataPtr = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(), new[] { dataSize }, "arr_data");

            // Initialize header fields
            _builder.BuildStore(count, _builder.BuildStructGEP2(_arrayStruct, header, 0)); // length
            _builder.BuildStore(count, _builder.BuildStructGEP2(_arrayStruct, header, 1)); // capacity
            _builder.BuildStore(dataPtr, _builder.BuildStructGEP2(_arrayStruct, header, 2)); // data ptr

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

        // Internal helper to build a new record from unboxed values, what does this mean?
        private LLVMValueRef BuildRecordFromValues(RecordType type, List<LLVMValueRef> values)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;

            // In LLVM 15+, 'ptr' is opaque, but we must tell GEP the size of the element
            // we are indexing. Since we are storing pointers, the element type is 'ptr'.
            var ptrType = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 1. Allocate the record (array of 8-byte pointers)
            // Size = count * 8 bytes
            var recordSize = LLVMValueRef.CreateConstInt(i64, (ulong)(values.Count * 8));
            var recordPtr = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(), new[] { recordSize }, "rec_ptr");

            for (int i = 0; i < values.Count; i++)
            {
                var fieldType = type.RecordFields[i].Type;
                LLVMValueRef pointerToStore;

                // 2. Handle Boxing for Primitives
                // If it's a raw LLVM i64/double/i1, it needs a heap-allocated box
                if (fieldType is IntType || fieldType is FloatType || fieldType is BoolType)
                {
                    if (values[i].TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind ||
                        values[i].TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
                    {
                        // Allocate 8 bytes for the primitive value
                        var valMem = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(),
                            new[] { LLVMValueRef.CreateConstInt(i64, 8) }, "val_mem");

                        LLVMTypeRef llvmType = fieldType switch
                        {
                            IntType => ctx.Int64Type,
                            FloatType => ctx.DoubleType,
                            _ => ctx.Int1Type // Bool
                        };

                        // Store the raw value into the box
                        var cast = _builder.BuildBitCast(valMem, LLVMTypeRef.CreatePointer(llvmType, 0));
                        _builder.BuildStore(values[i], cast);

                        pointerToStore = valMem;
                    }
                    else
                    {
                        // Already a pointer (already boxed)
                        pointerToStore = values[i];
                    }
                }
                else
                {
                    // Strings, Records, etc., are already pointers
                    pointerToStore = values[i];
                }

                // 3. THE CRITICAL FIX:
                // Use ptrType (8 bytes) so the index 'i' multiplies correctly.
                // This calculates address: recordPtr + (i * 8)
                var slotPtr = _builder.BuildGEP2(
                    ptrType,
                    recordPtr,
                    new[] { LLVMValueRef.CreateConstInt(i64, (ulong)i) },
                    $"slot_{i}"
                );

                // Store the 8-byte pointer into the 8-byte slot
                _builder.BuildStore(pointerToStore, slotPtr);
            }

            return recordPtr;
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

        /*  Command example of how to construct a dataframe in C# that matches the expected memory layout for your LLVM codegen:

        df = read_csv([index: int, name: string, age: int, hasJob: bool, savings: float], "CSV/mytest.csv")
        df = read_csv("CSV/Fire_Prediction_2023_Bolivia_encoded_small.csv")

        
        to_csv(df, "CSV/mytest.csv")

        df2 = dataframe(columns=["name", "age"],type=[string, int])         
        
        df2 = dataframe(columns=["name", "age", "hasJob", "savings"],data=[{name:"Bob", age: 23, hasJob: true, savings: 230500.00},{name:"Alice", age: 23, hasJob: true, savings: 100500.55},{name:"John", age: 87, hasJob: false, savings: 1209000.02},{name:"Mary", age: 29, hasJob: false, savings: 10700.25}])         
        df2 = dataframe(["name", "age", "hasJob", "savings"],[{name:"Bob", age: 23, hasJob: true, savings: 230500.00},{name:"Alice", age: 23, hasJob: true, savings: 100500.55},{name:"John", age: 87, hasJob: false, savings: 1209000.02},{name:"Mary", age: 29, hasJob: false, savings: 10700.25}])         
        
        df3 = df2.map(x => { name: x.name, age: x.age-10, hasJob: x.hasJob, savings: x.savings })
        df2.map(x => { age: x.age - 10, name: "Harry" })
        df2.map(x => { age: 10, name: x.name + "_2" })
        df2.map(x => x.age - 10)

        record(["name", "age", "is cool", "rating"], ["Hary potter", 9786, true, 10.5585]) 

        foreach(item in df3) { item.age = item.age + 10 }

        x = dataframe(columns=["name", "age"],type=[string, int])   
        for(i=0; i < 500000; i++) x.add({name: "Hary potter", age: 10 + random(1,100)})


        for(i=0; i < 5000000; i++) df.add({ date: "2023-01-01", latitude: -18.0, longitude: -69.38, wind-speed-min: 1.59, wind-speed-max: 6.47, wind-speed-mean: 3.73, wind-direction-min: 20.86, wind-direction-max: 299.09, wind-direction-mean: 135.45, surface-air-temperature-min: 275.79, surface-air-temperature-max: 284.51, surface-air-temperature-mean: 279.01, total-rainfall-sum: 0.01, surface-humidity-min: 0.01, surface-humidity-max: 0.01, surface-humidity-mean: 0.01, ndvi: 0.15, elevation: 4578.83, slope: 90, aspect: 10.15, fire_label: random(0,1), land_cover_class_1: false, land_cover_class_2: false, land_cover_class_4: false, land_cover_class_5: false, land_cover_class_6: false, land_cover_class_7: false, land_cover_class_8: false, land_cover_class_9: false, land_cover_class_10: false, land_cover_class_11: false, land_cover_class_12: false, land_cover_class_13: false, land_cover_class_14: false, land_cover_class_15: false, land_cover_class_16: true, land_cover_class_17: false })
        
        x.where(d=> d.age > 50)

        arrname = ["Harry", "Barry", "Mary", "Larry", "Carrie", "Terry", "Sherry", "Perry", "Garry", "Berry", "Narry", "Kerry", "Jerry", "Merry", "Larry", "Carry", "Tarry", "Sherry", "Perry", "Garry",]
         for(i=0; i<49; i++) df2.add({name: arrname[random(0, arrname.length)], age: random(10,99)})

        df.subset(["latitude", "longitude"])

        df.where(x => x.latitude > -18.0)
        df.where(x => x.latitude > -18.0).where(x => x.longitude < -69.0)
        df.where(x => x.latitude > -18.0 & x.longitude < -69.0)

        df.map(x => x.latitude - 100.0)
        df.map(x => x + {latitude: x.latitude - 100.0})
        df.map(x => x + {latitude: x.latitude - 100.0}).map(x => x+{ longitude: 100.0})
        df.map(x => x + {latitude: x.latitude - 100.0, longitude: 100.0})

*/


        /*
                Performance test for our Language:
                1) load in csv: around 11 seconds
                2) For loop add 5,000,000 rows total around 7 million rows: 
                    a) CodeGen time: 2.4462 ms
                    b) Compiler time: 0.0032 ms
                    c) Full stack: 7865 ms
                3) where filtering:
                    a) One where with one condition: around 0.128 seconds - [0.128, 0.166, 0.136, 0.129, 0.126, 0.133, 0.122, 0.131, 0.114, 0.137] avg 0.128 seconds
                    b) Two where's with one condition each: around 0.185 seconds -  [0.212, 0.170, 0.199, 0.181, 0.186, 0.196, 0.145, 0.165, 0.192, 0.215] avg 0.185 seconds
                    C) One where with two conditions: around 0.107 seconds - [0.120, 0.121, 0.108, 0.106, 0.107, 0.112, 0.092, 0.089, 0.109, 0.112] avg 0.107 seconds


                Performance test for Python:
                1) load in csv: around 11 seconds
                2) For loop add 5,000,000 rows total around 7 million rows: 23.5 seconds
                3) Direct filter test: 0.033 seconds - "df_direct = df[(df["latitude"] > -18.0) & (df["longitude"] < -69.0)]"
                4) Query filter test:
                    a) One where with one condition: around 0.228 seconds - [0.240, 0.218, 0.312, 0.225, 0.201, 0.213, 0.217, 0.250, 0.234, 0.200] avg 0.228 seconds
                    b) Two where's with one condition each: around 0.254 seconds -  [0.236, 0.264, 0.223, 0.257, 0.303, 0.223, 0.242, 0.247, 0.254, 0.299] avg 0.254 seconds
                    C) One where with two conditions: around 0.116 seconds -  [0.064, 0.067, 0.064, 0.136, 0.64, 0.86, 0.066, 0.070, 0.072, 0.063]  avg 0.116 seconds


                Performance test for R:
                1) load in csv: around 21.859 seconds
                2) For loop add 100,000 rows total around 2 million rows (not much added!): 32 seconds - [36, 28, 27, 36, 30, 33, 32, 33, 34, 30] avg 32 seconds 
                3) Query filter test:
                    a) One where with one condition: around 0.228 seconds - [0.74 , 0.93, 0.70, 0.68, 0.57, 0.65, 0.67, 0.57, 0.71, 0.67 ] avg 0.74 seconds
                    b) Two where's with one condition each: around 0.254 seconds -  [0.234 , 0.170, 0.130, 0.188, 0.121, 0.202, 0.160, 0.187, 0.177, 0.301 ] avg 0.187 seconds
                    C) One where with two conditions: around 0.116 seconds -  [0.064, 0.067, 0.064, 0.136, 0.64, 0.86, 0.066, 0.070, 0.072, 0.063]  avg 0.116 seconds



mx = df["latitude"].mean
my = df["elevation"].mean


components = df.map(r => {
    dx: r.latitude - mx,
    dy: r.elevation - my,
    dx2: (r.latitude - mx) * (r.latitude - mx),
    dy2: (r.elevation - my) * (r.elevation - my),
    prod: (r.latitude - mx) * (r.elevation - my)
})

numerator = components.map(x => x.prod).sum
sum_dx2 = components.map(x => x.dx2).sum
sum_dy2 = components.map(x => x.dy2).sum


correlation = numerator / sqrt(sum_dx2 * sum_dy2)

print(correlation)



        */
    }
}