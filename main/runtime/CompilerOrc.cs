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
using System.Diagnostics.CodeAnalysis;

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
                Console.WriteLine("no file in internal");
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

                    // Calculate the exact offset in the record (8 bytes per slot)
                    int offset = col * 8;

                    if (typeCode == 'I')
                    {
                        long.TryParse(rawValue, out long val);
                        Marshal.WriteInt64(recordBuffer, offset, val);
                    }
                    else if (typeCode == 'F')
                    {
                        double.TryParse(rawValue, System.Globalization.CultureInfo.InvariantCulture, out double val);
                        long bits = BitConverter.DoubleToInt64Bits(val);
                        Marshal.WriteInt64(recordBuffer, offset, bits);
                    }
                    else if (typeCode == 'B')
                    {
                        bool val = rawValue.ToLower() == "true" || rawValue == "1";
                        Marshal.WriteByte(recordBuffer, offset, val ? (byte)1 : (byte)0);
                    }
                    else if (typeCode == 'S')
                    {
                        IntPtr strPtr = Marshal.StringToHGlobalAnsi(rawValue);
                        Marshal.WriteIntPtr(recordBuffer, offset, strPtr);
                    }
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
            if (dfPtr == IntPtr.Zero || pathPtr == IntPtr.Zero)
            {
                Console.WriteLine("CRITICAL: Received Null Pointer in ToCsvInternal!");
                return;
            }

            try
            {
                // Get the export path
                string path = Marshal.PtrToStringAnsi(pathPtr);
                Console.WriteLine($"Exporting to: {path}");

                // 1. Unpack Dataframe { ptr columns_array, ptr rows_array, ptr types_array }
                // Each of these is a pointer (8 bytes on x64)
                IntPtr columnsArrayPtr = Marshal.ReadIntPtr(dfPtr, 0);
                IntPtr rowsArrayPtr = Marshal.ReadIntPtr(dfPtr, 8);
                IntPtr typesArrayPtr = Marshal.ReadIntPtr(dfPtr, 16);

                // 2. Get Metadata (Lengths) from our custom Array Headers
                // Assuming your Array Layout is: [ i64 length | i64 capacity | i64 data_ptr ]
                long colCount = Marshal.ReadInt64(columnsArrayPtr, 0);
                long rowCount = Marshal.ReadInt64(rowsArrayPtr, 0);

                // Get raw data buffers (offset 16 is the 3rd field in the array struct)
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
                        colNames.Add(Marshal.PtrToStringAnsi(namePtr) ?? "Unknown");
                    }
                    writer.WriteLine(string.Join(",", colNames));

                    // 4. Write Data Rows
                    for (int i = 0; i < rowCount; i++)
                    {
                        // recordPtr is the address of the specific Malloc'd struct for this row
                        IntPtr recordPtr = Marshal.ReadIntPtr(rowDataBuf, i * 8);
                        if (recordPtr == IntPtr.Zero) continue;

                        List<string> rowValues = new List<string>();

                        for (int j = 0; j < colCount; j++)
                        {
                            int offset = j * 8; // Because we forced LLVM to use 8-byte alignment

                            // CRITICAL: Ensure typesArray was also built with i64 in LLVM
                            int typeCode = (int)Marshal.ReadInt64(typeDataBuf, offset);
                            string cellValue = "";

                            switch (typeCode)
                            {
                                case 1: // Int
                                    cellValue = Marshal.ReadInt64(recordPtr, offset).ToString();
                                    break;

                                case 2: // Float
                                    long floatBits = Marshal.ReadInt64(recordPtr, offset);
                                    cellValue = BitConverter.Int64BitsToDouble(floatBits)
                                                .ToString(System.Globalization.CultureInfo.InvariantCulture);
                                    break;

                                case 3: // Bool
                                    cellValue = Marshal.ReadInt64(recordPtr, offset) == 1 ? "true" : "false";
                                    break;

                                case 4: // String
                                    IntPtr strPtr = Marshal.ReadIntPtr(recordPtr, offset);

                                    // ADD THIS DEBUG LINE HERE:
                                    Console.WriteLine($"DEBUG: Row {i}, Col {j} is STRING. TypeCode={typeCode}, PointerValue={strPtr.ToInt64():X}");

                                    // Safety check: PtrToStringAnsi crashes if strPtr is garbage
                                    if (strPtr != IntPtr.Zero)
                                    {
                                        try
                                        {
                                            cellValue = Marshal.PtrToStringAnsi(strPtr);
                                        }
                                        catch
                                        {
                                            cellValue = "INVALID_STR_PTR";
                                            Console.WriteLine($"CRASH PREVENTED: Pointer {strPtr} was not a valid string!");
                                        }
                                    }
                                    else
                                    {
                                        cellValue = "";
                                    }
                                    break;

                                default:
                                    cellValue = "NULL";
                                    break;
                            }
                            rowValues.Add(cellValue);
                        }
                        writer.WriteLine(string.Join(",", rowValues));
                    }
                }
                Console.WriteLine($"--- Success: {rowCount} rows written ---");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CRITICAL ERROR during CSV Export: {ex.Message}");
                throw;
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

        private uint GetEnumAttributeKind(string name)
        {
            // In LLVMSharp, we need to convert the string to a marshalled SByte pointer
            using (var marshaledName = new MarshaledString(name))
            {
                return LLVM.GetEnumAttributeKindForName(marshaledName, (UIntPtr)name.Length);
            }
        }

        private LLVMValueRef GetOrDeclareMalloc()
        {
            var ctx = _module.Context; // Use the context of the CURRENT module
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // Re-generate the function type every time to ensure Context compatibility
            // This is cheap and prevents AccessViolation if _mallocType was from an old Context
            _mallocType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i64 }, false);

            var mallocFn = _module.GetNamedFunction("malloc");
            if (mallocFn.Handle == IntPtr.Zero)
            {
                mallocFn = _module.AddFunction("malloc", _mallocType);

                // Optional: Add the 'noalias' attribute to return value for better optimization
                // and tell LLVM it's a system allocation
                mallocFn.AddAttributeAtIndex(LLVMAttributeIndex.LLVMAttributeReturnIndex,
                    _module.Context.CreateEnumAttribute(GetEnumAttributeKind("noalias"), 0));
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

        // private void DeclarePrintf()
        // {
        //     var llvmCtx = _module.Context;
        //     _printfType = LLVMTypeRef.CreateFunction(
        //           llvmCtx.Int32Type,
        //         new[] { LLVMTypeRef.CreatePointer(llvmCtx.DoubleType, 0) }, // should this be a double?
        //         true); // varargs

        //     _printf = _module.AddFunction("printf", _printfType);
        // }
        private void DeclarePrintf()
        {
            var llvmCtx = _module.Context;

            // FIX: Format string must be a pointer to an 8-bit integer (i8/char), NOT a double
            var i8Ptr = LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0);

            _printfType = LLVMTypeRef.CreateFunction(
                llvmCtx.Int32Type,
                new[] { i8Ptr },
                true // varargs for extra arguments like index values and lengths
            );

            // Guard to ensure we don't accidentally redefine it if called multiple times
            _printf = _module.GetNamedFunction("printf");
            if (_printf.Handle == IntPtr.Zero)
            {
                _printf = _module.AddFunction("printf", _printfType);
            }
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
        // add range can only take an array, not an id or an expr, only an array
        // z.addRange(x.select(age)) doesn't work
        // z.addRange(arr)           doesn't work

        // TODO: fix the problems

        // BROKEN FUNCTIONALITY
        // can't use existing variables inside where and map
        // can't use random inside addRange
        // we do not handle wrong index
        // can't have array of arrays with different types
        // can't do x.columns[0]

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
            if (!_stopwatch)
            {
                compilerTestList = new List<double>();
                IRTestList = new List<double>();
                RuntimeTestList = new List<double>();

            }
            if (_stopwatch) StartStopWatch();
            LLVMValueRef resultValue = Visit(expr);
            if (_stopwatch) compilerTestList.Add(StopStopWatch("Ran codegen"));

            if (_stopwatch) StartStopWatch();

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
            if (_stopwatch) IRTestList.Add(StopStopWatch("Ran IR codegen"));

            if (_stopwatch) StartStopWatch();
            var tempResult = delegateResult();
            if (_stopwatch) RuntimeTestList.Add(StopStopWatch("Ran program"));

            if (_stopwatch)
            {
                Console.WriteLine($"\ncompiler list: \n{string.Join(", ", compilerTestList)}");
                Console.WriteLine($"\nIR list: \n{string.Join(", ", IRTestList)}");
                Console.WriteLine($"\nRuntime list: \n{string.Join(", ", RuntimeTestList)}");
            }

            if (tempResult == IntPtr.Zero) throw new Exception("JIT execution returned null pointer");

            RuntimeValue result = Marshal.PtrToStructure<RuntimeValue>(tempResult);

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

                // If it's a string, double, long, or bool, ToString() just works!
                // No more Marshal.Read inside the formatter.
                return v.ToString();
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
                var fieldType = field.Value?.Type ?? field.Type;
                int offset = i * 8; // Every field is exactly 8 bytes wide

                if (fieldType is IntType)
                {
                    result.Add(Marshal.ReadInt64(recordPtr, offset));
                }
                else if (fieldType is FloatType)
                {
                    long bits = Marshal.ReadInt64(recordPtr, offset);
                    result.Add(BitConverter.Int64BitsToDouble(bits));
                }
                else if (fieldType is BoolType)
                {
                    // We store bools as 64-bit 0 or 1 in ReadCsvInternal
                    result.Add(Marshal.ReadByte(recordPtr, offset) != 0);
                }
                else if (fieldType is StringType)
                {
                    // For strings, the 8 bytes IS the pointer to the char array
                    IntPtr sPtr = Marshal.ReadIntPtr(recordPtr, offset);
                    result.Add(sPtr == IntPtr.Zero ? "" : Marshal.PtrToStringAnsi(sPtr));
                }
                else
                {
                    // For nested records/arrays, we still store the pointer
                    result.Add(Marshal.ReadIntPtr(recordPtr, offset));
                }
            }
            return result;
        }

        private LLVMValueRef BoxValue(LLVMValueRef value, Type type)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8 = ctx.Int8Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(i8, 0);

            // 1. ALWAYS get the fresh function signature for THIS context
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
                    BoolType => 1,
                    _ => 8
                };

                // 2. FIX: Use 'mallocFuncType' instead of '_mallocType'
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
                _builder.BuildStore(value, cast);
                dataPtr = mem;
            }
            else if (IsReferenceType(type))
            {
                dataPtr = _builder.BuildBitCast(value, i8Ptr, "boxed_ptr_cast");
            }
            else if (type is VoidType)
            {
                // For void, we point to null data
                dataPtr = LLVMValueRef.CreateConstPointerNull(i8Ptr);
            }
            else
            {
                throw new Exception($"Unsupported type in BoxValue: {type}");
            }

            // 3. FIX: Use 'mallocFuncType' instead of '_mallocType' here too
            var objRaw = _builder.BuildCall2(_mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, 16) }, "runtime_obj");

            // Store tag
            var tagPtr = _builder.BuildStructGEP2(_runtimeValueType, objRaw, 0, "tag_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)tag), tagPtr).SetAlignment(8);

            // Store data
            var dataFieldPtr = _builder.BuildStructGEP2(_runtimeValueType, objRaw, 1, "data_ptr");
            _builder.BuildStore(dataPtr, dataFieldPtr).SetAlignment(8);

            return objRaw;
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

            if (condition.TypeOf == ctx.DoubleType)
            {
                condition = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE,
                    condition, LLVMValueRef.CreateConstReal(ctx.DoubleType, 0.0), "fortest_dbl");
            }
            else if (condition.TypeOf != ctx.Int8Type) // If it's not already i8, treat it as an integer condition NOTE
            {
                condition = _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE,
                    condition, LLVMValueRef.CreateConstInt(condition.TypeOf, 0), "fortest_int");
            }

            _builder.BuildCondBr(condition, bodyBlock, endBlock);

            // 4. Body Block
            _builder.PositionAtEnd(bodyBlock);
            Visit(expr.Body);
            _builder.BuildBr(stepBlock);

            // 5. Step Block (Increment)
            _builder.PositionAtEnd(stepBlock);
            if (expr.Step != null) Visit(expr.Step);

            // --- VECTORIZATION HINT START ---
            // 1. Create the attribute: !{!"llvm.loop.vectorize.enable", i1 1}
            var metadataName = ctx.GetMDString("llvm.loop.vectorize.enable", 26);
            var valOne = LLVMValueRef.CreateConstInt(ctx.Int8Type, 1);
            var vectorizeAttr = LLVMValueRef.CreateMDNode(new[] { metadataName, valOne });

            // 2. Create a non-circular Loop Node.
            // We use a dummy string as the first element instead of a self-reference.
            // This is often enough for the optimizer to find the hint.
            var dummyId = ctx.GetMDString("loop.id", 7);
            var loopMetadata = LLVMValueRef.CreateMDNode(new[] { dummyId, vectorizeAttr });

            // 3. Attach to the back-edge branch
            var backBr = _builder.BuildBr(condBlock);
            backBr.SetMetadata(ctx.GetMDKindID("llvm.loop"), loopMetadata);
            // --- VECTORIZATION HINT END ---

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

            // decide if we need write-back
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
            var i64Type = _module.Context.Int64Type;

            // 1. Existing: Int -> Float (Actual value conversion)
            if (node.Expression.Type is IntType && node.ToType is FloatType)
            {
                if (_debug) Console.WriteLine($"Converting {node.Expression.Type} to {node.ToType} (SIToFP)");
                return _builder.BuildSIToFP(value, _module.Context.DoubleType, "int2double");
            }

            // 2. NEW: Float -> Int (Bitwise preservation for CSV storage)
            if (node.Expression.Type is FloatType && node.ToType is IntType)
            {
                if (_debug) Console.WriteLine($"Bitcasting {node.Expression.Type} to {node.ToType} for storage");
                // This keeps the 64 bits of the double exactly as they are
                return _builder.BuildBitCast(value, i64Type, "double_to_i64_bits");
            }

            // 3. NEW: Bool -> Int (Zero-extension for storage)
            if (node.Expression.Type is BoolType && node.ToType is IntType)
            {
                if (_debug) Console.WriteLine($"Extending {node.Expression.Type} to {node.ToType}");
                // Turns i1 (0/1) into i64 (0/1)
                return _builder.BuildZExt(value, i64Type, "bool_to_i64");
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

                        finalArg = valueToPrint;
                        var selectedStr = _builder.BuildSelect(finalArg, _trueStr, _falseStr, "boolstr");

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
                                    fieldVal = _builder.BuildLoad2(ctx.Int8Type, storedPtr, "bool_val");

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
        // x.map(d=> d + {x= 5, y= 3, z= "hi", cool3= false})               // should return a dataframe with added columns x and y
        // x.map(d=> d + {name= d.name + "smith"})   // should return a dataframe with added columns x and y

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
        // x=dataframe({name: string, age: int})
        // x=dataframe({name: string, age: int}, [{name= "dan", age= 30}, {name= "alice", age= 25}])
        // x=dataframe({name: string, age: int}, rows=[{name: "dan", age: 30}, {name: "alice", age: 25}])
        // x=dataframe({name: string, age: int, cool: bool}, rows=[{name= "dan", age= 30, cool= true}, {name= "alice", age= 25, cool= false}])

        // x=record({name: "Hary potter", age: 30, rating: 10.5585})

        // x.add({name: "Hary potter2", age: 201})
        // x.add({name="bob",age=5,hasJob=true,savings=1.1})
        // x.add({name="bob",age=5,hasJob=false,savings=1.1})
        // x.add({name="bob",age=5,cool=false})
        // x.add({name="bob",age=5,cool=true})
        // x.addRange([{name: "voldemort", age: 80}, {name: "dumbledore", age: 70}, {name: "MERLIN", age: 101}])

        // for(i=0; i<50; i++) x.add({name: "Hary potter", age: 10 + random(1,100)})
        // for(i=0; i<5200000; i++) {x.add({name="Hary potter", age= 10 + random(1,100)}) }
        // for(i=0; i<5000000; i++) x.add({name="Hary potter", age= 10 + random(1,100)}) // 5 million
        // this below can't do random inside addRange "Cannot perform + on int and"
        // for(i=0; i<520000; i++) x.addRange([{name="voldemort", age=80}, {name="dumbledore", age=70}, {name="MERLIN", age=101}])

        // x.map(d => d.age + 100)
        // x.map(d => d+ {power= d.age + 100}) // it creates a new column with the name item1
        // x.where(d=> d.age > 50)
        // x.where(d=> d > 9).where(z=> z < 93)
        // x.where(d=> d.age > 91).where(z=> z.age < 93 & z.name=="Hary potter")
        // x.where(d=> d.savings > 693444.47).where(z=> z.savings < 6903444.47 & z.name=="John")
        // x.where(d=> d.age == 38 or d.age == 71)

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
            var resultVar = "__where_result";
            var iVar = "__where_i";

            if (sourceType is not DataframeType dfType)
                throw new Exception("Where only supports dataframe");

            var srcAssign = new AssignNode(srcVar, expr.SourceExpr);
            var indexInit = new AssignNode(iVar, new NumberNode(0));

            // --- CLEAN REWRITE: Pass the schema structure directly ---
            // If dfType stores the original RecordNode, pass it. 
            // If it only stores the RecordType, we reconstruct the matching literal fields.
            var fields = new List<NamedArgumentNode>();
            for (int i = 0; i < dfType.ColumnNames.Count; i++)
            {
                var name = dfType.ColumnNames[i];
                var item = dfType.DataTypes[i];

                ExpressionNode typeLit = item switch
                {
                    IntType => new TypeLiteralNode(new TypeNode("int")),
                    FloatType => new TypeLiteralNode(new TypeNode("float")),
                    BoolType => new TypeLiteralNode(new TypeNode("bool")),
                    StringType => new TypeLiteralNode(new TypeNode("string")),
                    _ => throw new Exception("Unknown type mapping")
                };

                fields.Add(new NamedArgumentNode(name, typeLit));
            }
            var schemaRecord = new RecordNode(fields);

            var resultDf = new DataframeNode(new List<NamedArgumentNode> {
                new NamedArgumentNode("schema", schemaRecord)
            });

            var resultAssign = new AssignNode(resultVar, resultDf);

            var cond = new ComparisonNode(new IdNode(iVar), "<", new LengthNode(new IdNode(srcVar)));
            var step = new IncrementNode(new IdNode(iVar));

            var current = new IndexNode(new IdNode(srcVar), new IdNode(iVar))
            {
                SkipBoundsCheck = true
            };

            ExpressionNode ifCond = ReplaceIterator(expr.Condition, expr.IteratorId.Name, current);

            var ifBody = new SequenceNode();
            ifBody.Statements.Add(new AddNode(new IdNode(resultVar), current));

            var loop = new ForLoopNode(indexInit, cond, step, new SequenceNode
            {
                Statements = { new IfNode(ifCond, ifBody) }
            });

            return new SequenceNode { Statements = { srcAssign, resultAssign, loop, new IdNode(resultVar) } };
        }

        public SequenceNode WhereForArray(Type sourceType, WhereNode expr)
        {
            var arrType = (ArrayType)sourceType;
            var elementType = arrType.ElementType;

            var srcVarName = "__where_src";
            var resultVarName = "__where_result";
            var indexVarName = "__where_i";

            var srcAssign = new AssignNode(srcVarName, expr.SourceExpr);

            var resultArray = new ArrayNode(new List<ExpressionNode>())
            {
                ElementType = elementType
            };

            var resultAssign = new AssignNode(resultVarName, resultArray);
            var indexInit = new AssignNode(indexVarName, new NumberNode(0));

            var loopCond = new ComparisonNode(new IdNode(indexVarName), "<", new LengthNode(new IdNode(srcVarName)));
            var loopStep = new IncrementNode(new IdNode(indexVarName));
            var currentElement = new IndexNode(new IdNode(srcVarName), new IdNode(indexVarName));

            ExpressionNode ifCond = ReplaceIterator(expr.Condition, expr.IteratorId.Name, currentElement);
            var ifBody = new SequenceNode { Statements = { new AddNode(new IdNode(resultVarName), currentElement) } };

            var forLoop = new ForLoopNode(indexInit, loopCond, loopStep, new SequenceNode { Statements = { new IfNode(ifCond, ifBody) } });

            return new SequenceNode { Statements = { srcAssign, resultAssign, forLoop, new IdNode(resultVarName) } };
        }

        public LLVMValueRef VisitMap(MapNode expr)
        {
            SequenceNode program;

            // Check what the TYPE CHECKER said the result would be
            if (expr.Type is DataframeType)
                program = MapForDataframe(expr);
            else if (expr.Type is ArrayType)
                program = MapForArray(expr);
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
            var iVar = "__map_i";

            // 1. Get the correct element type from the MapNode's result type
            Type resultElementType = null;
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

            // --- OPTIMIZATION: Index with SkipBoundsCheck ---
            var rowAccess = new IndexNode(new IdNode(srcVar), new IdNode(iVar));
            rowAccess.SkipBoundsCheck = true;

            // 3. Transform the lambda body
            var lastExpr =
            (ExpressionNode)ReplaceIteratorInNode(
                expr.TransformExpr,
                expr.IteratorId.Name,
                rowAccess
            );

            // 4. Add to the result list
            loopBody.Statements.Add(new AddNode(new IdNode(resVar), lastExpr));

            // 5. Loop Logic
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

            // 1. Assign Source
            program.Statements.Add(new AssignNode(srcVar, expr.SourceExpr));

            // 2. Capture length
            var srcLength = new LengthNode(new IdNode(srcVar));
            srcLength.SetType(new IntType());

            // 3. Initialize Result Dataframe with Capacity
            var columns = dfType.ColumnNames.Select(c => (ExpressionNode)new StringNode(c)).ToList();
            var dummyTypes = dfType.DataTypes.Select(t => (ExpressionNode)new StringNode("")).ToList();

            // Passing srcLength to ArrayNode is the key to stopping reallocs
            var rowsArray = new ArrayNode(new List<ExpressionNode>());
            rowsArray.SetType(new ArrayType(dfType.RowType));

            var dfConstructor = new DataframeNode(new List<NamedArgumentNode> {
                new NamedArgumentNode("columns", new ArrayNode(columns)),
                new NamedArgumentNode("rows", rowsArray),
                new NamedArgumentNode("type", new ArrayNode(dummyTypes))
            });
            dfConstructor.SetType(dfType);

            program.Statements.Add(new AssignNode(resVar, dfConstructor));
            program.Statements.Add(new AssignNode(iVar, new NumberNode(0)));

            var loopBody = new SequenceNode();
            var replacementNode = new IdNode(currentRowVar);

            // 4. Fetch row: currentRow = src[i]
            var rowAccess = new IndexNode(new IdNode(srcVar), new IdNode(iVar)) { SkipBoundsCheck = true };
            loopBody.Statements.Add(new AssignNode(currentRowVar, rowAccess));

            // 5. Transformation Logic
            ExpressionNode finalRowExpr = (ExpressionNode)ReplaceIteratorInNode(
                expr.TransformExpr,
                expr.IteratorId.Name,
                replacementNode
            );

            // Because we initialized 'rowsArray' with 'srcLength',
            // the 'grow' block in the IR will exist but will NEVER be executed.
            loopBody.Statements.Add(new AddNode(new IdNode(resVar), finalRowExpr));

            // 7. Loop Setup
            var cond = new ComparisonNode(new IdNode(iVar), "<", srcLength);
            var step = new IncrementNode(new IdNode(iVar));
            program.Statements.Add(new ForLoopNode(null, cond, step, loopBody));

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

            throw new Exception($"Deep copy not supported for type: {type}");
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

            // 1. Get the actual LLVM Struct Type for this record
            var recordStructType = GetLLVMType(recordType);
            var mallocFunc = GetOrDeclareMalloc();

            // 2. Calculate size using LLVM's offsetof logic to ensure alignment/padding is correct
            var sizeOfRecord = LLVMValueRef.CreateConstInt(i64, GetStructSize(recordType));

            // 3. Allocate the new record
            var newRecordBuffer = _builder.BuildCall2(
                _mallocType,
                mallocFunc,
                new[] { sizeOfRecord },
                "record_copy_mem"
            );

            // No need for generic i8** casts; we work with the struct pointers directly
            for (int i = 0; i < recordType.RecordFields.Count; i++)
            {
                var fieldInfo = recordType.RecordFields[i];
                var fieldLLVMType = GetLLVMType(fieldInfo.Type);

                // 4. Use BuildStructGEP2 to get the exact field address
                var srcFieldPtr = _builder.BuildStructGEP2(recordStructType, recordPtr, (uint)i, $"src_f{i}_ptr");
                var dstFieldPtr = _builder.BuildStructGEP2(recordStructType, newRecordBuffer, (uint)i, $"dst_f{i}_ptr");

                // 5. Load the actual value (could be a double, i64, or ptr)
                var val = _builder.BuildLoad2(fieldLLVMType, srcFieldPtr, $"f{i}_val");
                val.SetAlignment(8);

                // 6. Deep copy if it's a reference type; otherwise store the primitive
                LLVMValueRef copiedValue;
                if (fieldInfo.Type is ArrayType || fieldInfo.Type is RecordType || fieldInfo.Type is DataframeType)
                {
                    copiedValue = EmitDeepCopy(val, fieldInfo.Type);
                }
                else
                {
                    // For primitives (Double, Int), we just store the loaded value
                    copiedValue = val;
                }

                var storeInstr = _builder.BuildStore(copiedValue, dstFieldPtr);
                storeInstr.SetAlignment(8);
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
        private bool IsValueType(Type t)
        {
            return t is IntType || t is FloatType || t is BoolType;
        }

        private uint GetTypeSize(Type type)
        {
            if (type is IntType || type is FloatType) return 8;
            if (type is BoolType) return 1; // We use 1 byte for bools to simplify memory management and alignment zzzzz
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
            uint count = (uint)expr.Elements.Count;

            bool isPrimitive = IsValueType(expr.ElementType);

            // --- NEW: Handle Dynamic vs Static Capacity ---
            LLVMValueRef countVal = LLVMValueRef.CreateConstInt(i64, count);
            uint capacity = count > 0 ? Math.Min(count, 100) : 100;
            if (expr.Capacity != null)
                capacity = Math.Min(count, 100);
            LLVMValueRef capacityVal = LLVMValueRef.CreateConstInt(i64, capacity);

            var elementType = GetLLVMType(expr.ElementType);
            uint elementSize = isPrimitive ? GetTypeSize(expr.ElementType) : 8;

            // --- NEW: Dynamic Size Calculation ---
            // Instead of C# math: (elementSize * capacity)
            // We do LLVM IR math: BuildMul(elementSize, capacityVal)
            var llvmElementSize = LLVMValueRef.CreateConstInt(i64, (ulong)elementSize);
            var totalCapacity = _builder.BuildMul(llvmElementSize, capacityVal, "total_size");

            // Align to 32 bytes (totalSize + 31) & ~31
            var thirtyOne = LLVMValueRef.CreateConstInt(i64, 31);
            var mask = LLVMValueRef.CreateConstInt(i64, unchecked((ulong)~31));
            var padded = _builder.BuildAdd(totalCapacity, thirtyOne, "padded_size");
            var alignedSizeVal = _builder.BuildAnd(padded, mask, "aligned_size");

            // Ensure at least 8 bytes to avoid malloc(0)
            var eight = LLVMValueRef.CreateConstInt(i64, 8);
            var isZero = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, alignedSizeVal, LLVMValueRef.CreateConstInt(i64, 0), "is_zero");
            alignedSizeVal = _builder.BuildSelect(isZero, eight, alignedSizeVal, "final_malloc_size");

            // 3. Allocate
            var mallocFn = GetOrDeclareMalloc();
            var headerRaw = _builder.BuildCall2(_mallocType, mallocFn,
                new[] { LLVMValueRef.CreateConstInt(i64, GetStructSize(expr.Type)) }, "arr_header");

            // Pass the DYNAMIC alignedSizeVal to malloc
            var rawDataPtr = _builder.BuildCall2(_mallocType, mallocFn,
                new[] { alignedSizeVal }, "arr_data_raw");

            // 4. Metadata Storage
            var lenPtr = _builder.BuildStructGEP2(_arrayStruct, headerRaw, 0, "len_ptr");
            var capPtr = _builder.BuildStructGEP2(_arrayStruct, headerRaw, 1, "cap_ptr");
            var dataFieldPtr = _builder.BuildStructGEP2(_arrayStruct, headerRaw, 2, "data_field_ptr");

            // Store the dynamic values
            _builder.BuildStore(countVal, lenPtr).SetAlignment(8);
            _builder.BuildStore(capacityVal, capPtr).SetAlignment(8);
            _builder.BuildStore(rawDataPtr, dataFieldPtr).SetAlignment(8);

            // 5. Populate Elements (Static elements only)
            var typedDataPtr = _builder.BuildBitCast(rawDataPtr, LLVMTypeRef.CreatePointer(elementType, 0), "typed_ptr");
            for (int i = 0; i < expr.Elements.Count; i++)
            {
                var val = Visit(expr.Elements[i]);
                var idx = LLVMValueRef.CreateConstInt(i64, (ulong)i);
                var elementPtr = _builder.BuildGEP2(elementType, typedDataPtr, new[] { idx }, "elem_ptr");
                _builder.BuildStore(val, elementPtr).SetAlignment(elementSize);
            }

            return headerRaw;
        }

        private LLVMTypeRef GetRuntimeObjType()
        {
            var ctx = _module.Context;

            // Check module for existing named type
            var existing = _module.GetTypeByName("RuntimeObj");
            if (existing.Handle != IntPtr.Zero) return existing;

            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // Static overload: CreateStruct(LLVMTypeRef[] elementTypes, bool packed)
            // No context argument here!
            return LLVMTypeRef.CreateStruct(new[] { i64, i8Ptr }, false);
        }
        public LLVMValueRef VisitIndex(IndexNode expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var func = _builder.InsertBlock.Parent;

            var headerPtr = Visit(expr.SourceExpression);
            var indexVal = Visit(expr.IndexExpression);
            var sourceType = expr.SourceExpression.Type;

            if (sourceType is ArrayType arrayType)
            {
                // Ensure index is an integer
                if (indexVal.TypeOf == ctx.DoubleType)
                    indexVal = _builder.BuildFPToSI(indexVal, i64, "idx_int");

                // Load Metadata fields
                var lenFieldPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 0, "len_field_ptr");
                var dataFieldPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 2, "data_field_ptr");

                var rawDataPtr = _builder.BuildLoad2(i8Ptr, dataFieldPtr, "data_ptr");
                rawDataPtr.SetAlignment(8);

                var arrayLen = _builder.BuildLoad2(i64, lenFieldPtr, "array_len");
                arrayLen.SetAlignment(8);

                // --- PYTHON-STYLE NEGATIVE INDEX RESOLUTION ---
                // if indexVal < 0 then indexVal + arrayLen else indexVal
                var indexIsNeg = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, indexVal, LLVMValueRef.CreateConstInt(i64, 0), "index_is_neg");
                var indexRegulated = _builder.BuildAdd(indexVal, arrayLen, "index_rel");
                var resolvedIndex = _builder.BuildSelect(indexIsNeg, indexRegulated, indexVal, "resolved_index");

                // --- OPTIMIZATION: Conditional Bounds Check ---
                if (!expr.SkipBoundsCheck)
                {
                    // Bounds Check Logic using the resolved index value
                    var isNegative = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, resolvedIndex, LLVMValueRef.CreateConstInt(i64, 0), "is_neg");
                    var isTooBig = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, resolvedIndex, arrayLen, "is_too_big");
                    var isInvalid = _builder.BuildOr(isNegative, isTooBig, "is_invalid");

                    var failBlock = ctx.AppendBasicBlock(func, "bounds.fail");
                    var safeBlock = ctx.AppendBasicBlock(func, "bounds.ok");
                    _builder.BuildCondBr(isInvalid, failBlock, safeBlock);

                    // --- FAIL BLOCK ---
                    _builder.PositionAtEnd(failBlock);
                    var errorMsg = _builder.BuildGlobalStringPtr("Runtime Error: Index Out of Bounds!\n", "err_msg");
                    _builder.BuildCall2(_printfType, _printf, new[] { errorMsg }, "print_err");

                    var runtimeObjPtrType = LLVMTypeRef.CreatePointer(GetRuntimeObjType(), 0);
                    _builder.BuildRet(LLVMValueRef.CreateConstNull(runtimeObjPtrType));

                    // --- CONTINUE IN SAFE BLOCK ---
                    _builder.PositionAtEnd(safeBlock);
                }

                // 2. DATA ACCESS (Using resolvedIndex instead of raw indexVal)
                var llvmElementType = GetLLVMType(arrayType.ElementType);
                var typedDataPtr = _builder.BuildBitCast(rawDataPtr, LLVMTypeRef.CreatePointer(llvmElementType, 0), "typed_data_ptr");

                var elementPtr = _builder.BuildGEP2(llvmElementType, typedDataPtr, new[] { resolvedIndex }, "elem_ptr");
                var loadedValue = _builder.BuildLoad2(llvmElementType, elementPtr, "loaded_val");

                // Enforce rigid alignment patterns based on primitives
                if (llvmElementType == i64 || llvmElementType == ctx.DoubleType || llvmElementType.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                {
                    loadedValue.SetAlignment(8);
                }
                else if (llvmElementType == ctx.Int32Type || llvmElementType == ctx.FloatType)
                {
                    loadedValue.SetAlignment(4);
                }

                return arrayType.ElementType switch
                {
                    BoolType => _builder.BuildZExt(loadedValue, ctx.Int8Type, "bool_val"),
                    _ => loadedValue
                };
            }
            else if (sourceType is DataframeType)
            {
                if (expr.IndexExpression.Type is IntType)
                {
                    var result = DataframeIndex(headerPtr, indexVal, expr.SkipBoundsCheck);
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
            var fieldType = dfType.RowType.RecordFields.FirstOrDefault(f => f.Label == columnName)?.Type
                ?? throw new Exception($"Field {columnName} not found");

            var srcVar = "__col_src";
            var resultVar = "__col_result";
            var iVar = "__col_i";
            var lenVar = "__col_len";
            var rowVar = "__col_row";

            var sourceId = ((IdNode)indexNode.SourceExpression).Name;
            var srcAssign = new AssignNode(srcVar, new IdNode(sourceId));

            // HOIST: Calculate length first so we can pre-size the result array
            var lenAssign = new AssignNode(lenVar, new LengthNode(new IdNode(srcVar)));

            // 1. Result = Array(capacity: dataframe.length)
            // We pass lenVar as the explicit capacity to avoid reallocations
            var resultConstructor = new ArrayNode(new List<ExpressionNode>())
            {
                ElementType = fieldType
            };
            resultConstructor.SetType(new ArrayType(fieldType));

            var resultAssign = new AssignNode(resultVar, resultConstructor);
            var indexInit = new AssignNode(iVar, new NumberNode(0));

            // 3. Loop Construction
            var cond = new ComparisonNode(new IdNode(iVar), "<", new IdNode(lenVar));
            var step = new IncrementNode(new IdNode(iVar));
            var loopBody = new SequenceNode();

            // row = src[i]
            var rowAccess = new IndexNode(new IdNode(srcVar), new IdNode(iVar));
            rowAccess.SetType(dfType.RowType);
            loopBody.Statements.Add(new AssignNode(rowVar, rowAccess));

            // value = row.column (This returns the actual double/int value, not a pointer)
            var valueExpr = new FieldNode(new IdNode(rowVar), columnName);
            valueExpr.SetType(fieldType);

            // result.add(value) -> This will now store the raw value contiguously
            var addNode = new AddNode(new IdNode(resultVar), valueExpr);
            loopBody.Statements.Add(addNode);

            var loop = new ForLoopNode(indexInit, cond, step, loopBody);

            var program = new SequenceNode();
            program.Statements.Add(srcAssign);
            program.Statements.Add(lenAssign); // Length first
            program.Statements.Add(resultAssign);
            program.Statements.Add(loop);
            program.Statements.Add(new IdNode(resultVar));

            return program;
        }

        private LLVMValueRef DataframeIndex(LLVMValueRef dataframePtr, LLVMValueRef indexValue, bool skipBoundsCheck = false)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            var dfType = GetOrCreateDataframeType();
            var arrayType = GetOrCreateArrayType();
            var func = _builder.InsertBlock.Parent;

            // 1. Load the 'rows' Array pointer from the Dataframe struct
            var rowsPtrPtr = _builder.BuildStructGEP2(dfType, dataframePtr, 1, "rows_ptr_ptr");
            var rowsPtr = _builder.BuildLoad2(i8Ptr, rowsPtrPtr, "rows");
            rowsPtr.SetAlignment(8);

            // 2. Load Data Buffer Pointer
            var dataPtrPtr = _builder.BuildStructGEP2(arrayType, rowsPtr, 2, "data_ptr_ptr");
            var dataPtr = _builder.BuildLoad2(i8Ptr, dataPtrPtr, "data");
            dataPtr.SetAlignment(8);

            // Ensure index is i64
            if (indexValue.TypeOf != i64)
                indexValue = _builder.BuildIntCast(indexValue, i64, "idx_cast");

            // --- OPTIMIZATION: Branchless path ---
            if (skipBoundsCheck)
            {
                // Direct GEP and Load without any safety checks or merge blocks
                var elemPtr = _builder.BuildGEP2(i8Ptr, dataPtr, new[] { indexValue }, "elem_ptr");
                var record = _builder.BuildLoad2(i8Ptr, elemPtr, "record");
                record.SetAlignment(8);
                return record;
            }

            // --- STANDARD SAFE PATH (With Bounds Checking) ---
            var lenPtr = _builder.BuildStructGEP2(arrayType, rowsPtr, 0, "len_ptr");
            var len = _builder.BuildLoad2(i64, lenPtr, "len");
            len.SetAlignment(8);

            var inBounds = _builder.BuildICmp(LLVMIntPredicate.LLVMIntULT, indexValue, len, "in_bounds");

            var okBlock = ctx.AppendBasicBlock(func, "df_idx_ok");
            var errBlock = ctx.AppendBasicBlock(func, "df_idx_err");
            var mergeBlock = ctx.AppendBasicBlock(func, "df_idx_merge");

            _builder.BuildCondBr(inBounds, okBlock, errBlock);

            // --- ERROR BLOCK ---
            _builder.PositionAtEnd(errBlock);
            var msg = _builder.BuildGlobalStringPtr(
                "Runtime Error: Dataframe row index out of bounds\n",
                "df_idx_err_msg"
            );
            _builder.BuildCall2(_printfType, _printf, new[] { msg }, "print_err");
            var nullVal = LLVMValueRef.CreateConstNull(i8Ptr);
            _builder.BuildBr(mergeBlock);
            var errEndBlock = _builder.InsertBlock;

            // --- OK BLOCK ---
            _builder.PositionAtEnd(okBlock);
            var okElemPtr = _builder.BuildGEP2(i8Ptr, dataPtr, new[] { indexValue }, "elem_ptr");

            // FIX: Declare 'okVal' here by loading from the element pointer
            var okVal = _builder.BuildLoad2(i8Ptr, okElemPtr, "record");
            okVal.SetAlignment(8);

            _builder.BuildBr(mergeBlock);
            var okEndBlock = _builder.InsertBlock;

            // --- MERGE BLOCK ---
            _builder.PositionAtEnd(mergeBlock);
            var phi = _builder.BuildPhi(i8Ptr, "df_idx_result");
            phi.AddIncoming(new[] { nullVal, okVal }, new[] { errEndBlock, okEndBlock }, 2);

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
            Console.WriteLine("yo we getting the LLVM type for type: " + type);
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
            var value = Visit(expr.AddExpression);

            ExecuteArrayAddition(headerPtr, value, expr.AddExpression.Type);
            return headerPtr;
        }

        public LLVMValueRef AddToDataframe(AddNode expr, DataframeType dfType)
        {
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            var dfPtr = Visit(expr.SourceExpression);
            var valueToAdd = Visit(expr.AddExpression);

            if (expr.AddExpression.Type is not RecordType)
                throw new Exception("Only records can be added to dataframe");

            // df.rows (array header)
            var rowsFieldPtr = _builder.BuildStructGEP2(_dataframeStruct, dfPtr, 1, "rows_field");
            var rowsArrayPtr = _builder.BuildLoad2(i8Ptr, rowsFieldPtr, "rows_array");

            // SAFE: single append (no loops here → OK to reuse function)
            ExecuteArrayAddition(rowsArrayPtr, valueToAdd, dfType.RowType);

            return dfPtr;
        }

        private void ExecuteArrayAddition(LLVMValueRef headerPtr, LLVMValueRef valueToAdd, Type elementType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            var llvmElementType = GetLLVMType(elementType);
            var elementPtrType = LLVMTypeRef.CreatePointer(llvmElementType, 0);

            bool isPrimitive = elementType is IntType or FloatType or BoolType;

            // ===== LOAD HEADER =====
            var lenPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 0, "len_ptr");
            var capPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 1, "cap_ptr");
            var dataPtrPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 2, "data_ptr_ptr");

            var length = _builder.BuildLoad2(i64, lenPtr, "len");
            var capacity = _builder.BuildLoad2(i64, capPtr, "cap");
            var rawData = _builder.BuildLoad2(i8Ptr, dataPtrPtr, "data");

            // ===== BLOCKS =====
            var function = _builder.InsertBlock.Parent;
            var entryBlock = _builder.InsertBlock;

            var growBlock = ctx.AppendBasicBlock(function, "grow");
            var contBlock = ctx.AppendBasicBlock(function, "cont");

            var isFull = _builder.BuildICmp(
                LLVMIntPredicate.LLVMIntUGE,
                length,
                capacity,
                "is_full");

            _builder.BuildCondBr(isFull, growBlock, contBlock);

            // ===== GROW =====
            _builder.PositionAtEnd(growBlock);

            var i64Zero = LLVMValueRef.CreateConstInt(i64, 0);
            var i64Two = LLVMValueRef.CreateConstInt(i64, 2);
            var i64Four = LLVMValueRef.CreateConstInt(i64, 4);

            var newCap = _builder.BuildSelect(
                _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, capacity, i64Zero),
                i64Four,
                _builder.BuildMul(capacity, i64Two),
                "new_cap");

            uint elemSize = isPrimitive ? GetTypeSize(elementType) : 8;

            var byteSize = _builder.BuildMul(
                newCap,
                LLVMValueRef.CreateConstInt(i64, elemSize),
                "bytes");

            var reallocFunc = GetOrDeclareRealloc();

            var newData = _builder.BuildCall2(
                _reallocType,
                reallocFunc,
                new[] { rawData, byteSize },
                "realloc");

            _builder.BuildStore(newCap, capPtr);
            _builder.BuildStore(newData, dataPtrPtr);

            _builder.BuildBr(contBlock);

            // ===== CONT =====
            _builder.PositionAtEnd(contBlock);

            var phiData = _builder.BuildPhi(i8Ptr, "data_phi");

            phiData.AddIncoming(
                new LLVMValueRef[] { rawData, newData },
                new LLVMBasicBlockRef[] { entryBlock, growBlock },
                2
            );

            var typedPtr = _builder.BuildBitCast(phiData, elementPtrType, "typed_ptr");

            var indexPtr = _builder.BuildGEP2(
                llvmElementType,
                typedPtr,
                new[] { length },
                "slot");

            var storeVal = valueToAdd;

            if (!isPrimitive)
                storeVal = _builder.BuildBitCast(valueToAdd, llvmElementType, "cast");

            _builder.BuildStore(storeVal, indexPtr);

            var newLen = _builder.BuildAdd(
                length,
                LLVMValueRef.CreateConstInt(i64, 1),
                "new_len");

            _builder.BuildStore(newLen, lenPtr);
        }

        public LLVMValueRef VisitAddRange(AddRangeNode expr)
        {
            var sourceType = expr.SourceExpression.Type;

            // Route based on whether the source object is an Array or a Dataframe
            if (sourceType is ArrayType arrayType)
            {
                var targetArrayPtr = Visit(expr.SourceExpression);
                var sourceArrayPtr = Visit(expr.AddRangeExpression);

                return AddRangeToArray(targetArrayPtr, sourceArrayPtr, arrayType.ElementType);
            }
            else if (sourceType is DataframeType dfType)
            {
                var df = Visit(expr.SourceExpression);
                var arr = Visit(expr.AddRangeExpression);

                return AddRangeToDataframe(df, arr, dfType.RowType);
            }

            throw new Exception("addRange operation is only supported on arrays and dataframes");
        }

        private LLVMValueRef AddRangeToArray(LLVMValueRef targetArrayPtr, LLVMValueRef sourceArrayPtr, Type elementType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // Extract length and data pointer from the source array (the one being read from)
            var srcLenPtr = _builder.BuildStructGEP2(_arrayStruct, sourceArrayPtr, 0, "src_len_ptr");
            var srcDataPtrPtr = _builder.BuildStructGEP2(_arrayStruct, sourceArrayPtr, 2, "src_data_ptr_ptr");

            var srcLen = _builder.BuildLoad2(i64, srcLenPtr, "src_len");
            var srcData = _builder.BuildLoad2(i8Ptr, srcDataPtrPtr, "src_data");

            // Loop components
            var function = _builder.InsertBlock.Parent;
            var loop = ctx.AppendBasicBlock(function, "arr_range_loop");
            var body = ctx.AppendBasicBlock(function, "arr_range_body");
            var after = ctx.AppendBasicBlock(function, "arr_range_after");

            // Local iterator tracking loop index 'i'
            var i = _builder.BuildAlloca(i64, "i_arr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, 0), i);

            _builder.BuildBr(loop);

            // =====================
            // LOOP CONDITIONAL CHECK
            // =====================
            _builder.PositionAtEnd(loop);
            var iVal = _builder.BuildLoad2(i64, i);
            var cond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, iVal, srcLen, "loop_cond");
            _builder.BuildCondBr(cond, body, after);

            // =====================
            // LOOP BODY (Extract and Append Element)
            // =====================
            _builder.PositionAtEnd(body);

            var llvmElemType = GetLLVMType(elementType);
            var typedSrcPtr = _builder.BuildBitCast(srcData, LLVMTypeRef.CreatePointer(llvmElemType, 0), "typed_src_ptr");

            var elemPtr = _builder.BuildGEP2(
                llvmElemType,
                typedSrcPtr,
                new[] { iVal },
                "src_elem_ptr");

            var elem = _builder.BuildLoad2(llvmElemType, elemPtr, "extracted_elem");

            // SAFE: Reuses your existing scalar array appending state engine
            ExecuteArrayAddition(targetArrayPtr, elem, elementType);

            // Increment index counter
            var next = _builder.BuildAdd(iVal, LLVMValueRef.CreateConstInt(i64, 1), "next_i");
            _builder.BuildStore(next, i);
            _builder.BuildBr(loop);

            // =====================
            // EXIT
            // =====================
            _builder.PositionAtEnd(after);

            return targetArrayPtr;
        }



        private LLVMValueRef AddRangeToDataframe(LLVMValueRef dfPtr, LLVMValueRef arrayPtr, Type rowType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // array header
            var lenPtr = _builder.BuildStructGEP2(_arrayStruct, arrayPtr, 0, "len_ptr");
            var dataPtrPtr = _builder.BuildStructGEP2(_arrayStruct, arrayPtr, 2, "data_ptr_ptr");

            var len = _builder.BuildLoad2(i64, lenPtr, "len");
            var data = _builder.BuildLoad2(i8Ptr, dataPtrPtr, "data");

            // dataframe rows
            var rowsPtr = _builder.BuildStructGEP2(_dataframeStruct, dfPtr, 1, "rows_ptr");
            var rowsArr = _builder.BuildLoad2(i8Ptr, rowsPtr, "rows_arr");

            var function = _builder.InsertBlock.Parent;
            var entry = _builder.InsertBlock;

            var loop = ctx.AppendBasicBlock(function, "loop");
            var body = ctx.AppendBasicBlock(function, "body");
            var after = ctx.AppendBasicBlock(function, "after");

            var i = _builder.BuildAlloca(i64, "i");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, 0), i);

            _builder.BuildBr(loop);

            // =====================
            // LOOP
            // =====================
            _builder.PositionAtEnd(loop);

            var iVal = _builder.BuildLoad2(i64, i);
            var cond = _builder.BuildICmp(
                LLVMIntPredicate.LLVMIntSLT,
                iVal,
                len);

            _builder.BuildCondBr(cond, body, after);

            // =====================
            // BODY
            // =====================
            _builder.PositionAtEnd(body);

            var elemPtr = _builder.BuildGEP2(
                LLVMTypeRef.CreatePointer(GetLLVMType(rowType), 0),
                _builder.BuildBitCast(
                    data,
                    LLVMTypeRef.CreatePointer(GetLLVMType(rowType), 0)),
                new[] { iVal },
                "elem_ptr");

            var elem = _builder.BuildLoad2(GetLLVMType(rowType), elemPtr);

            // SAFE: reuse ONLY scalar append logic
            ExecuteArrayAddition(rowsArr, elem, rowType);

            var next = _builder.BuildAdd(
                iVal,
                LLVMValueRef.CreateConstInt(i64, 1));

            _builder.BuildStore(next, i);
            _builder.BuildBr(loop);

            // =====================
            // EXIT
            // =====================
            _builder.PositionAtEnd(after);

            return dfPtr;
        }

        public LLVMValueRef VisitRemove(RemoveNode expr)
        {
            var sourceType = expr.SourceExpression.Type;

            if (_debug)
                Console.WriteLine("semantic type of array being indexed: " + sourceType);

            var sourcePtr = Visit(expr.SourceExpression);
            var indexVal = Visit(expr.RemoveExpression);

            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var arrayHeaderType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);

            // --- 1. EXTRACT LENGTH POINTER BASED ON TYPE ---
            LLVMValueRef lenPtr;
            if (sourceType is ArrayType)
            {
                lenPtr = _builder.BuildStructGEP2(arrayHeaderType, sourcePtr, 0, "len_ptr");
            }
            else if (sourceType is DataframeType)
            {
                var dfStructType = LLVMTypeRef.CreateStruct(new[] { i8Ptr, i8Ptr, i8Ptr }, false);
                var rowsFieldPtr = _builder.BuildStructGEP2(dfStructType, sourcePtr, 1, "df_rows_field");
                var rowsHeaderPtr = _builder.BuildLoad2(i8Ptr, rowsFieldPtr, "rows_header_ptr");
                lenPtr = _builder.BuildStructGEP2(arrayHeaderType, rowsHeaderPtr, 0, "rows_len_ptr");
            }
            else
            {
                throw new Exception("Remove operation is only supported on arrays and dataframes");
            }

            // --- 2. UNIFIED BOUNDS CHECKING ---
            var len = _builder.BuildLoad2(i64, lenPtr, "len");
            len.Alignment = 8;

            var function = _builder.InsertBlock.Parent;
            var safeBlock = ctx.AppendBasicBlock(function, "remove_safe_inline");
            var skipBlock = ctx.AppendBasicBlock(function, "remove_skip_inline");
            var continueBlock = ctx.AppendBasicBlock(function, "remove_continue");

            var isOOB = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, indexVal, len, "is_oob");
            var isNegative = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, indexVal, LLVMValueRef.CreateConstInt(i64, 0), "is_neg");
            var isInvalid = _builder.BuildOr(isOOB, isNegative, "is_invalid_idx");

            _builder.BuildCondBr(isInvalid, skipBlock, safeBlock);

            // --- SKIP PATH: Print error safely, don't crash the REPL ---
            _builder.PositionAtEnd(skipBlock);
            var errorStr = sourceType is DataframeType
                ? "Runtime Error: Row index %lld is out of bounds for DataFrame with %lld rows\n"
                : "Runtime Error: Index %lld is out of bounds for array with length %lld\n";
            var errorStringConstant = _builder.BuildGlobalStringPtr(errorStr, "remove_err_msg");

            _builder.BuildCall2(
                _printfType,
                _printf,
                new[] { errorStringConstant, indexVal, len },
                "print_err"
            );
            _builder.BuildBr(continueBlock);

            // --- SAFE PATH: Execute raw memory shifts ---
            _builder.PositionAtEnd(safeBlock);
            if (sourceType is ArrayType arrayType)
            {
                RemoveFromArrayRaw(sourcePtr, indexVal, arrayType);
            }
            else if (sourceType is DataframeType)
            {
                RemoveFromDataframeRaw(sourcePtr, indexVal);
            }
            _builder.BuildBr(continueBlock);

            _builder.PositionAtEnd(continueBlock);
            return sourcePtr;
        }



        public LLVMValueRef RemoveFromArrayRaw(LLVMValueRef arrayPtr, LLVMValueRef removeIdx, ArrayType arrayType)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // Structure layout matches exactly Pack = 8 standard (i64, i64, ptr)
            var arrayHeaderType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);

            var lenPtr = _builder.BuildStructGEP2(arrayHeaderType, arrayPtr, 0, "len_ptr");
            var dataPtrPtr = _builder.BuildStructGEP2(arrayHeaderType, arrayPtr, 2, "data_ptr_ptr");

            var lenLoad = _builder.BuildLoad2(i64, lenPtr, "len");
            lenLoad.Alignment = 8;

            var dataPtrLoad = _builder.BuildLoad2(i8Ptr, dataPtrPtr, "data");
            dataPtrLoad.Alignment = 8;

            // NOTICE: All safety validations and hard exits are completely stripped!
            // This executes raw memory mutations safely based on pre-verified assumptions.
            var llvmElementType = GetLLVMType(arrayType.ElementType);
            var dstGEP = _builder.BuildGEP2(llvmElementType, dataPtrLoad, new[] { removeIdx }, "dst_ptr");

            var nextIdx = _builder.BuildAdd(removeIdx, LLVMValueRef.CreateConstInt(i64, 1), "next_idx");
            var srcGEP = _builder.BuildGEP2(llvmElementType, dataPtrLoad, new[] { nextIdx }, "src_ptr");

            var remElements = _builder.BuildSub(lenLoad, removeIdx);
            var elementsToMove = _builder.BuildSub(remElements, LLVMValueRef.CreateConstInt(i64, 1), "elements_to_move");

            uint elementSize = (arrayType.ElementType is IntType or FloatType) ? 8u : (arrayType.ElementType is BoolType ? 1u : 8u);
            var byteCount = _builder.BuildMul(elementsToMove, LLVMValueRef.CreateConstInt(i64, elementSize), "move_bytes");

            var exactMemmoveType = LLVMTypeRef.CreateFunction(ctx.VoidType, new[] { i8Ptr, i8Ptr, i64 }, false);
            var memmoveFunc = _module.GetNamedFunction("memmove");
            if (memmoveFunc.Handle == IntPtr.Zero)
            {
                memmoveFunc = _module.AddFunction("memmove", exactMemmoveType);
            }

            var bitcastDst = _builder.BuildBitCast(dstGEP, i8Ptr, "dst_void_ptr");
            var bitcastSrc = _builder.BuildBitCast(srcGEP, i8Ptr, "src_void_ptr");

            _builder.BuildCall2(exactMemmoveType, memmoveFunc, new[] { bitcastDst, bitcastSrc, byteCount }, "");

            var updatedLen = _builder.BuildSub(lenLoad, LLVMValueRef.CreateConstInt(i64, 1), "new_array_len");

            var storeInstruction = _builder.BuildStore(updatedLen, lenPtr);
            storeInstruction.Alignment = 8;

            return arrayPtr;
        }

        public LLVMValueRef RemoveFromDataframeRaw(LLVMValueRef dfPtr, LLVMValueRef indexVal)
        {
            var ctx = _module.Context;

            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            var dfStructType =
                LLVMTypeRef.CreateStruct(
                    new[] { i8Ptr, i8Ptr, i8Ptr },
                    false);

            var arrayHeaderType =
                LLVMTypeRef.CreateStruct(
                    new[] { i64, i64, i8Ptr },
                    false);

            // df->rows
            var rowsFieldPtr =
                _builder.BuildStructGEP2(
                    dfStructType,
                    dfPtr,
                    1,
                    "df_rows_field");

            var rowsHeaderPtr =
                _builder.BuildLoad2(
                    i8Ptr,
                    rowsFieldPtr,
                    "rows_header_ptr");

            // rows array header
            var lenPtr =
                _builder.BuildStructGEP2(
                    arrayHeaderType,
                    rowsHeaderPtr,
                    0,
                    "rows_len_ptr");

            var dataFieldPtr =
                _builder.BuildStructGEP2(
                    arrayHeaderType,
                    rowsHeaderPtr,
                    2,
                    "rows_data_ptr_ptr");

            var currentLen = _builder.BuildLoad2(i64, lenPtr, "current_len");
            currentLen.Alignment = 8;

            var dataPtr = _builder.BuildLoad2(i8Ptr, dataFieldPtr, "data_ptr");
            dataPtr.Alignment = 8;

            // pointers are 8 bytes
            var gepStrideType = i8Ptr;

            var dstPtr =
                _builder.BuildGEP2(
                    gepStrideType,
                    dataPtr,
                    new[] { indexVal },
                    "dst_ptr");

            var nextIdx =
                _builder.BuildAdd(
                    indexVal,
                    LLVMValueRef.CreateConstInt(i64, 1),
                    "next_idx");

            var srcPtr =
                _builder.BuildGEP2(
                    gepStrideType,
                    dataPtr,
                    new[] { nextIdx },
                    "src_ptr");

            var numElementsToMove =
                _builder.BuildSub(
                    _builder.BuildSub(currentLen, indexVal),
                    LLVMValueRef.CreateConstInt(i64, 1));

            var ptrSize = LLVMValueRef.CreateConstInt(i64, (ulong)IntPtr.Size);

            var bytesToMove = _builder.BuildMul(numElementsToMove, ptrSize, "bytes_to_move");

            var memmoveFunc = GetOrDeclareMemmove();

            _builder.BuildCall2(
                _memmoveType,
                memmoveFunc,
                new[] { dstPtr, srcPtr, bytesToMove },
                "");

            var newLen =
                _builder.BuildSub(
                    currentLen,
                    LLVMValueRef.CreateConstInt(i64, 1));

            var storeInstruction = _builder.BuildStore(newLen, lenPtr);
            storeInstruction.Alignment = 8; // Ensure 8-byte alignment


            return dfPtr;
        }
        public LLVMValueRef VisitRemoveRange(RemoveRangeNode expr)
        {
            var sourceVal = Visit(expr.SourceExpression);
            var removeArrayVal = Visit(expr.RemoveRangeExpression);

            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            var arrayHeaderType = LLVMTypeRef.CreateStruct(new[] { i64, i64, i8Ptr }, false);
            var dfStructType = LLVMTypeRef.CreateStruct(new[] { i8Ptr, i8Ptr, i8Ptr }, false);

            var removeLenPtr = _builder.BuildStructGEP2(arrayHeaderType, removeArrayVal, 0, "remove_len_ptr");
            var removeDataPtrPtr = _builder.BuildStructGEP2(arrayHeaderType, removeArrayVal, 2, "remove_data_ptr_ptr");

            var removeLen = _builder.BuildLoad2(i64, removeLenPtr, "remove_len");
            var removeDataPtr = _builder.BuildLoad2(i8Ptr, removeDataPtrPtr, "remove_data_ptr");

            // Fetch the target rows array to mutate (Works for both naked arrays and DataFrames)
            LLVMValueRef srcLenPtr;
            LLVMValueRef srcDataPtr;
            LLVMTypeRef elemType;

            if (expr.SourceExpression.Type is ArrayType arrayType)
            {
                srcLenPtr = _builder.BuildStructGEP2(arrayHeaderType, sourceVal, 0, "src_len_ptr");
                var srcDataPtrPtr = _builder.BuildStructGEP2(arrayHeaderType, sourceVal, 2, "src_data_ptr_ptr");
                srcDataPtr = _builder.BuildLoad2(i8Ptr, srcDataPtrPtr, "src_data_ptr");
                elemType = GetLLVMType(arrayType.ElementType);
            }
            else if (expr.SourceExpression.Type is DataframeType)
            {
                // df->rows is at struct index 1
                var rowsFieldPtr = _builder.BuildStructGEP2(dfStructType, sourceVal, 1, "df_rows_field");
                var rowsHeaderPtr = _builder.BuildLoad2(i8Ptr, rowsFieldPtr, "rows_header_ptr");

                srcLenPtr = _builder.BuildStructGEP2(arrayHeaderType, rowsHeaderPtr, 0, "src_len_ptr");
                var srcDataPtrPtr = _builder.BuildStructGEP2(arrayHeaderType, rowsHeaderPtr, 2, "src_data_ptr_ptr");
                srcDataPtr = _builder.BuildLoad2(i8Ptr, srcDataPtrPtr, "src_data_ptr");

                // DataFrame rows contain pointers to Records (struct instances)
                elemType = i8Ptr;
            }
            else
            {
                throw new Exception("Unsupported removeRange target type.");
            }

            // --- GENERATE UNIFIED FILTER LOOP IR ---
            var entryBlock = _builder.InsertBlock;
            var function = entryBlock.Parent;

            var srcLen = _builder.BuildLoad2(i64, srcLenPtr, "src_len");

            var filterStartBlock = ctx.AppendBasicBlock(function, "filter_start");
            var filterLoopBlock = ctx.AppendBasicBlock(function, "filter_loop");
            var filterBodyBlock = ctx.AppendBasicBlock(function, "filter_body");
            var checkMatchLoop = ctx.AppendBasicBlock(function, "check_match_loop");
            var checkMatchBody = ctx.AppendBasicBlock(function, "check_match_body");
            var performCopyBlock = ctx.AppendBasicBlock(function, "perform_copy");
            var filterNextBlock = ctx.AppendBasicBlock(function, "filter_next");
            var afterBlock = ctx.AppendBasicBlock(function, "removerange_after");

            _builder.BuildBr(filterStartBlock);

            // Initial allocas
            _builder.PositionAtEnd(filterStartBlock);
            var readIdxAlloc = _builder.BuildAlloca(i64, "read_idx_ptr");
            var writeIdxAlloc = _builder.BuildAlloca(i64, "write_idx_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, 0), readIdxAlloc);
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, 0), writeIdxAlloc);
            _builder.BuildBr(filterLoopBlock);

            // Outer Loop Condition
            _builder.PositionAtEnd(filterLoopBlock);
            var currentReadIdx = _builder.BuildLoad2(i64, readIdxAlloc, "current_read_idx");
            var filterCond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, currentReadIdx, srcLen, "filter_cond");
            _builder.BuildCondBr(filterCond, filterBodyBlock, afterBlock);

            _builder.PositionAtEnd(filterBodyBlock);
            _builder.BuildBr(checkMatchLoop);

            // Inner Match Loop Checking Deletions
            _builder.PositionAtEnd(checkMatchLoop);
            var matchPhi = _builder.BuildPhi(i64, "match_i");
            matchPhi.AddIncoming(new[] { LLVMValueRef.CreateConstInt(i64, 0) }, new[] { filterBodyBlock }, 1);

            var matchCond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, matchPhi, removeLen, "match_cond");
            _builder.BuildCondBr(matchCond, checkMatchBody, performCopyBlock);

            _builder.PositionAtEnd(checkMatchBody);
            var remIdxPtr = _builder.BuildGEP2(i64, removeDataPtr, new[] { matchPhi }, "rem_idx_ptr");
            var remIdxTarget = _builder.BuildLoad2(i64, remIdxPtr, "rem_idx_target");
            var isMatch = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, currentReadIdx, remIdxTarget, "is_match");

            var nextMatchBlock = ctx.AppendBasicBlock(function, "next_match");
            _builder.BuildCondBr(isMatch, filterNextBlock, nextMatchBlock);

            _builder.PositionAtEnd(nextMatchBlock);
            var nextMatchI = _builder.BuildAdd(matchPhi, LLVMValueRef.CreateConstInt(i64, 1), "next_match_i");
            _builder.BuildBr(checkMatchLoop);
            matchPhi.AddIncoming(new[] { nextMatchI }, new[] { nextMatchBlock }, 1);

            // Perform the Shift Down
            _builder.PositionAtEnd(performCopyBlock);
            var currentWriteIdx = _builder.BuildLoad2(i64, writeIdxAlloc, "current_write_idx");

            var srcElemGEP = _builder.BuildGEP2(elemType, srcDataPtr, new[] { currentReadIdx }, "src_elem_gep");
            var loadedElem = _builder.BuildLoad2(elemType, srcElemGEP, "loaded_elem");

            var dstElemGEP = _builder.BuildGEP2(elemType, srcDataPtr, new[] { currentWriteIdx }, "dst_elem_gep");
            _builder.BuildStore(loadedElem, dstElemGEP);

            var updatedWriteIdx = _builder.BuildAdd(currentWriteIdx, LLVMValueRef.CreateConstInt(i64, 1), "updated_write_idx");
            _builder.BuildStore(updatedWriteIdx, writeIdxAlloc);
            _builder.BuildBr(filterNextBlock);

            // Advance Read Pointer
            _builder.PositionAtEnd(filterNextBlock);
            var nextReadIdx = _builder.BuildAdd(currentReadIdx, LLVMValueRef.CreateConstInt(i64, 1), "next_read_idx");
            _builder.BuildStore(nextReadIdx, readIdxAlloc);
            _builder.BuildBr(filterLoopBlock);

            // Finalize lengths
            _builder.PositionAtEnd(afterBlock);
            var totalWrittenElements = _builder.BuildLoad2(i64, writeIdxAlloc, "total_written_elements");
            _builder.BuildStore(totalWrittenElements, srcLenPtr);

            return sourceVal;
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
            // Use BuildStructGEP2 with your Array struct type (len is index 0)
            var lengthGEP = _builder.BuildStructGEP2(GetOrCreateArrayType(), arrayPtr, 0, "len_ptr");
            var length = _builder.BuildLoad2(i64, lengthGEP, "len");
            length.SetAlignment(8); // Optimization
            return length;
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
            var global = _module.GetNamedGlobal(expr.Name);
            if (global.Handle == IntPtr.Zero)
            {
                global = _module.AddGlobal(llvmType, expr.Name);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage;
            }
            ptrToLoad = global;

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
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var ptrType = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0); // Use opaque ptr if LLVM 15+
            var func = _builder.InsertBlock.Parent;

            var headerPtr = Visit(expr.ArrayExpression);
            var indexVal = Visit(expr.IndexExpression);
            if (indexVal.TypeOf == ctx.DoubleType)
                indexVal = _builder.BuildFPToSI(indexVal, i64, "idx_int");

            // Load Data Pointer
            var dataFieldPtr = _builder.BuildStructGEP2(_arrayStruct, headerPtr, 2, "data_ptr_ptr");
            var dataPtr = _builder.BuildLoad2(ptrType, dataFieldPtr, "data_ptr");
            dataPtr.SetAlignment(8);

            // Value to store
            var valueToAssign = Visit(expr.AssignExpression);

            // Bounds Check logic (as before)...

            // --- SAFE BLOCK ---
            var arrayType = (ArrayType)expr.ArrayExpression.Type;
            var llvmElemType = GetLLVMType(arrayType.ElementType);

            // Use the actual element type for the GEP to ensure correct stride
            var elementPtr = _builder.BuildGEP2(llvmElemType, dataPtr, new[] { indexVal }, "elem_ptr");

            var store = _builder.BuildStore(valueToAssign, elementPtr);
            store.SetAlignment(8);

            return valueToAssign;
        }

        public LLVMValueRef VisitRecord(RecordNode expr)
        {
            var ctx = _module.Context;
            var i32 = ctx.Int32Type;
            var recordType = (RecordType)expr.Type;

            // 1. Get the struct definition for THIS module
            var structType = GetOrCreateRecordStructType(recordType);

            // 2. Get malloc and its signature type for THIS context
            var mallocFunc = GetOrDeclareMalloc();

            // 3. Size calculation (Ensure it is i64 for malloc)
            var sizeValue = structType.SizeOf;

            // 4. THE CRITICAL CALL: Use the fresh mallocFuncType
            var instancePtr = _builder.BuildCall2(_mallocType, mallocFunc, new[] { sizeValue }, "record_ptr");
            var typedPtr = _builder.BuildBitCast(instancePtr, LLVMTypeRef.CreatePointer(structType, 0), "typed_record");

            for (int i = 0; i < expr.Fields.Count; i++)
            {
                var fieldValue = Visit(expr.Fields[i].Value);

                var fieldPtr = _builder.BuildGEP2(
                    structType,
                    typedPtr,
                    new[] { LLVMValueRef.CreateConstInt(i32, 0), LLVMValueRef.CreateConstInt(i32, (ulong)i) },
                    $"field_{i}"
                );

                var store = _builder.BuildStore(fieldValue, fieldPtr);
                store.SetAlignment(GetAlignment(fieldValue.TypeOf));
            }

            return instancePtr;
        }

        private uint GetAlignment(LLVMTypeRef type)
        {
            var ctx = _module.Context;

            if (type == ctx.Int64Type)
                return 8;

            if (type == ctx.DoubleType)
                return 8;

            if (type == ctx.Int8Type) // for bools, we store as i64 for simplicity, so alignment is 8
                return 1;

            if (type.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                return 8;

            return 8;
        }

        public LLVMValueRef VisitRecordFieldAssign(RecordFieldAssignNode expr)
        {
            var fieldPtr = GetFieldPointer(expr.IdRecord, expr.IdField);
            var newValue = Visit(expr.AssignExpression);

            var recType = (RecordType)expr.IdRecord.Type;
            var fieldDef = recType.RecordFields.First(f => f.Label == expr.IdField);

            // Primitive stored inline
            _builder.BuildStore(newValue, fieldPtr);

            return newValue;
        }

        private LLVMValueRef GetFieldPointer(ExpressionNode recordExpr, string fieldName)
        {
            var i32 = _module.Context.Int32Type;
            var recordPtr = Visit(recordExpr);

            if (recordExpr.Type is not RecordType recordType)
                throw new Exception("Expected record type");

            int fieldIndex = GetFieldIndex(fieldName, recordType.RecordFields);
            var structType = GetOrCreateRecordStructType(recordType);

            return _builder.BuildGEP2(structType, recordPtr, new LLVMValueRef[]
                { LLVMValueRef.CreateConstInt(i32, 0),
                LLVMValueRef.CreateConstInt(i32, (ulong)fieldIndex)
                },
                $"ptr_{fieldName}"
            );
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
                // Get the pointer to the specific slot (e.g., recordBase + 8)
                var fieldSlotPtr = GetFieldPointer(expr.SourceExpression, expr.IdField);
                var ctx = _module.Context;

                // Perform a SINGLE load based on the type
                if (expr.Type is IntType)
                    return _builder.BuildLoad2(ctx.Int64Type, fieldSlotPtr, $"val_{expr.IdField}");

                if (expr.Type is FloatType)
                    return _builder.BuildLoad2(ctx.DoubleType, fieldSlotPtr, $"val_{expr.IdField}");

                if (expr.Type is BoolType)
                {
                    // If you store bools as i64 in C#, load i64 and truncate to i1
                    var i8Val = _builder.BuildLoad2(ctx.Int8Type, fieldSlotPtr, $"val_bool_raw");
                    return _builder.BuildIntCast(i8Val, ctx.Int8Type, $"val_{expr.IdField}");
                }

                if (expr.Type is StringType)
                {
                    // For strings, the value IN the slot IS the pointer to the characters
                    var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
                    return _builder.BuildLoad2(i8Ptr, fieldSlotPtr, $"val_{expr.IdField}");
                }

                return _builder.BuildLoad2(LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), fieldSlotPtr, "val_ptr");
            }
            else if (expr.SourceExpression.Type is DataframeType dfType)
            {

                var d = ColumnAccessForDataframe(expr);
                PerformSemanticAnalysis(d);
                return Visit(d);

            }
            else
                throw new Exception("Field access is only supported on records");
        }




        public LLVMValueRef VisitToCsv(ToCsvNode expr)
        {
            // 1. Visit the Dataframe expression (the struct containing {cols, rows, types})
            var dfValue = Visit(expr.Expression);

            // 2. Visit the Path expression (the string/filename)
            var pathValue = Visit(expr.FileNameExpr);

            // 3. Setup Function Type: void ToCsvInternal(ptr, ptr)
            // Note: Modern LLVM uses opaque pointers (ptr). i8Ptr is the standard way to represent this.
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
            // dfValue is likely already a pointer to the DF struct.
            // We cast to i8Ptr to satisfy the C# signature.
            var dfCast = _builder.BuildBitCast(dfValue, i8Ptr, "df_cast");

            // pathValue is already a pointer to the string characters from Visit(StringNode)
            _builder.BuildCall2(toCsvFnType, toCsvFn, new[] { dfCast, pathValue }, "");

            return default;
        }

        public LLVMValueRef VisitReadCsv(ReadCsvNode expr)
        {
            var pathValue = Visit(expr.FileNameExpr);

            // This will now always be populated by the Type Checker
            RecordNode recordSchema = expr.SchemaExpr.Value as RecordNode;

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
            int count = schema.Fields.Count;

            var header = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(), new[] { LLVMValueRef.CreateConstInt(i64, 24) }, "types_header");
            var data = _builder.BuildCall2(_mallocType, GetOrDeclareMalloc(), new[] { LLVMValueRef.CreateConstInt(i64, (ulong)(count * 8)) }, "types_data");

            for (int i = 0; i < count; i++)
            {
                var fieldType = schema.Fields[i].Type;
                int typeId = GetTypeByTag(fieldType);

                // DEBUG: This will tell you what the compiler THINKS the type is
                //Console.WriteLine($"Compiling Metadata: Column {i} ({schema.Fields[i].Label}) is Type {fieldType} -> ID {typeId}");

                var target = _builder.BuildGEP2(i64, data, new[] { LLVMValueRef.CreateConstInt(i64, (ulong)i) }, "ptr");
                _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, (ulong)typeId), target);
            }

            // Set len, cap, data
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

        public LLVMValueRef VisitDataframe(DataframeNode expr)
        {
            var dfType = (DataframeType)expr.Type;

            // 1. Column Names Array
            var columnNodes = dfType.ColumnNames.Select(name =>
            {
                var node = new StringNode(name);
                node.SetType(new StringType());
                return (ExpressionNode)node;
            }).ToList();

            var columnArray = new ArrayNode(columnNodes);
            var colType = new StringType();
            columnArray.ElementType = colType;
            // CRITICAL: Ensure the node.Type is a fully formed ArrayType
            columnArray.SetType(new ArrayType(colType));

            var colsPtr = Visit(columnArray);

            // 2. Rows (Assuming expr.Rows was already typed by the TypeChecker)
            var rowsPtr = Visit(expr.Rows);

            // 3. Type Tags Array
            var datatypeNodes = dfType.DataTypes.Select(t =>
            {
                var node = new NumberNode(GetTypeByTag(t));
                node.SetType(new IntType());
                return (ExpressionNode)node;
            }).ToList();

            var datatypeArray = new ArrayNode(datatypeNodes);
            var tagType = new IntType();
            datatypeArray.ElementType = tagType;
            // CRITICAL: Ensure the node.Type is a fully formed ArrayType
            datatypeArray.SetType(new ArrayType(tagType));

            var dataTypesPtr = Visit(datatypeArray);

            // 4. Struct Allocation
            var dfStructType = GetOrCreateDataframeType();
            var dfPtr = _builder.BuildMalloc(dfStructType, "df");

            var f0 = _builder.BuildStructGEP2(dfStructType, dfPtr, 0, "cols_gep");
            var f1 = _builder.BuildStructGEP2(dfStructType, dfPtr, 1, "rows_gep");
            var f2 = _builder.BuildStructGEP2(dfStructType, dfPtr, 2, "types_gep");

            _builder.BuildStore(colsPtr, f0);
            _builder.BuildStore(rowsPtr, f1);
            _builder.BuildStore(dataTypesPtr, f2);

            return dfPtr;
        }

        public LLVMValueRef VisitNamedArgument(NamedArgumentNode expr) // this is never called?
        {
            // This node is just a wrapper for a regular expression with a name.
            // The name is used for semantic analysis and doesn't affect codegen directly.
            return Visit(expr.Value);
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
                            _ => ctx.Int8Type // Bool
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

        public LLVMValueRef VisitSlice(SliceNode node)
        {
            var sourceVal = node.Source.Accept(this); // The pointer to ArrayObject or DataframeObject
            var sourceType = node.Source.Type;
            var _mallocFunc = GetOrDeclareMalloc();

            // Fix: Fall back to a raw AST node instead of an LLVM value if it's null
            var startNode = node.Start ?? new NumberNode(0);
            var endNode = node.End ?? new NumberNode(0);

            // 1. If it's a regular array: [1, 2, 3][0:2]
            if (sourceType is ArrayType arrayType)
            {
                var llvmElemType = GetLLVMType(arrayType.ElementType);
                // Pass the AST nodes directly since SliceArrayInternal handles their compilation
                return SliceArrayInternal(sourceVal, llvmElemType, startNode, endNode);
            }

            // 2. If it's a dataframe: df[5:10]
            if (sourceType is DataframeType dfType)
            {
                var colsPtrPtr = _builder.BuildStructGEP2(_dataframeStruct, sourceVal, 0, "cols_ptr_ptr");
                var rowsPtrPtr = _builder.BuildStructGEP2(_dataframeStruct, sourceVal, 1, "rows_ptr_ptr");
                var typesPtrPtr = _builder.BuildStructGEP2(_dataframeStruct, sourceVal, 2, "types_ptr_ptr");

                var originalCols = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(_arrayStruct, 0), colsPtrPtr, "orig_cols");
                var originalRows = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(_arrayStruct, 0), rowsPtrPtr, "orig_rows");
                var originalTypes = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(_arrayStruct, 0), typesPtrPtr, "orig_types");

                originalCols.SetAlignment(8);
                originalRows.SetAlignment(8);
                originalTypes.SetAlignment(8);

                // Pass the AST nodes here too
                var llvmPtrType = LLVMTypeRef.CreatePointer(_runtimeValueType, 0);
                var slicedRows = SliceArrayInternal(originalRows, llvmPtrType, startNode, endNode);

                var newDfPtr = _builder.BuildCall2(_mallocType, _mallocFunc,
                    new[] { LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 24) }, "new_df_header");

                var newColsPtr = _builder.BuildStructGEP2(_dataframeStruct, newDfPtr, 0, "new_cols_ptr");
                var newRowsPtr = _builder.BuildStructGEP2(_dataframeStruct, newDfPtr, 1, "new_rows_ptr");
                var newTypesPtr = _builder.BuildStructGEP2(_dataframeStruct, newDfPtr, 2, "new_types_ptr");

                var store0 = _builder.BuildStore(originalCols, newColsPtr);
                var store1 = _builder.BuildStore(slicedRows, newRowsPtr);
                var store2 = _builder.BuildStore(originalTypes, newTypesPtr);

                store0.SetAlignment(8);
                store1.SetAlignment(8);
                store2.SetAlignment(8);

                return newDfPtr;
            }

            throw new Exception("Codegen Error: Slicing not supported for type " + sourceType);
        }

        /// <summary>
        /// Core logic to create a new array containing a subset of elements from a source array.
        /// </summary>
        private LLVMValueRef SliceArrayInternal(LLVMValueRef sourceArrayPtr, LLVMTypeRef llvmElemType, ExpressionNode startNode, ExpressionNode endNode)
        {
            var i64 = LLVMTypeRef.Int64;
            var zero = LLVMValueRef.CreateConstInt(i64, 0);

            // A. Get current length from the source array header
            var lenPtr = _builder.BuildStructGEP2(_arrayStruct, sourceArrayPtr, 0, "len_ptr");
            var sourceLen = _builder.BuildLoad2(i64, lenPtr, "source_len");
            sourceLen.SetAlignment(8);

            // B. Resolve Start/End with defaults (0 and sourceLen)
            var startIdx = startNode != null ? startNode.Accept(this) : zero;
            var endIdx = endNode != null ? endNode.Accept(this) : sourceLen;

            // C. Boundary Protection (Clamping) & Negative Index Resolution

            // 1. Process Start Index: if start < 0 then start + sourceLen else start
            var startIsNeg = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, startIdx, zero, "start_is_neg");
            var startRegulated = _builder.BuildAdd(startIdx, sourceLen, "start_rel");
            var startResolved = _builder.BuildSelect(startIsNeg, startRegulated, startIdx, "start_resolved");

            // Clamp start: max(0, min(startResolved, sourceLen))
            var startTooBig = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, startResolved, sourceLen, "start_too_big");
            var startTmp = _builder.BuildSelect(startTooBig, sourceLen, startResolved);
            var startTooSmall = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, startTmp, zero, "start_too_small");
            var startFinal = _builder.BuildSelect(startTooSmall, zero, startTmp, "start_final");

            // 2. Process End Index: if end < 0 then end + sourceLen else end
            var endIsNeg = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, endIdx, zero, "end_is_neg");
            var endRegulated = _builder.BuildAdd(endIdx, sourceLen, "end_rel");
            var endResolved = _builder.BuildSelect(endIsNeg, endRegulated, endIdx, "end_resolved");

            // Clamp end: max(startFinal, min(endResolved, sourceLen))
            var endTooBig = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, endResolved, sourceLen, "end_too_big");
            var endTmp = _builder.BuildSelect(endTooBig, sourceLen, endResolved);
            var endTooSmall = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, endTmp, startFinal, "end_too_small");
            var endFinal = _builder.BuildSelect(endTooSmall, startFinal, endTmp, "end_final");

            // D. Calculate new length
            var newLen = _builder.BuildSub(endFinal, startFinal, "new_len");

            // E. Allocate and setup the new array
            var newArrayPtr = AllocateArrayHeader(newLen);
            var newDataRaw = GetArrayData(newArrayPtr);

            // F. Get Typed Pointers for GEP
            var sourceDataRaw = GetArrayData(sourceArrayPtr);
            var srcTyped = _builder.BuildBitCast(sourceDataRaw, LLVMTypeRef.CreatePointer(llvmElemType, 0), "src_typed");
            var destTyped = _builder.BuildBitCast(newDataRaw, LLVMTypeRef.CreatePointer(llvmElemType, 0), "dest_typed");

            // G. Calculate the source offset
            var offsetSrcPtr = _builder.BuildGEP2(llvmElemType, srcTyped, new[] { startFinal }, "offset_src_ptr");

            // H. Copy elements using a loop
            BuildLoop(newLen, (index) =>
            {
                // Load from source
                var elPtr = _builder.BuildGEP2(llvmElemType, offsetSrcPtr, new[] { index }, "el_ptr");
                var element = _builder.BuildLoad2(llvmElemType, elPtr, "el");

                // Match structure element widths for primitive alignments
                if (llvmElemType == i64 || llvmElemType == LLVMTypeRef.Double || llvmElemType.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                {
                    element.SetAlignment(8);
                }

                // Store to destination
                var destPtr = _builder.BuildGEP2(llvmElemType, destTyped, new[] { index }, "dest_ptr");
                _builder.BuildStore(element, destPtr);
            });

            return newArrayPtr;
        }



        // CURRENTLY NOT USED FUNCTIONS!
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

        private LLVMTypeRef GetArrayPtrType()
        {
            return LLVMTypeRef.CreatePointer(GetOrCreateArrayType(), 0);
        }

        private LLVMTypeRef GetDataframePtrType()
        {
            return LLVMTypeRef.CreatePointer(GetOrCreateDataframeType(), 0);
        }

        private LLVMValueRef GetArrayCapacity(LLVMValueRef arrayPtr)
        {
            var i64 = _module.Context.Int64Type;
            // Capacity is index 1
            var capacityGEP = _builder.BuildStructGEP2(GetOrCreateArrayType(), arrayPtr, 1, "cap_ptr");
            var cap = _builder.BuildLoad2(i64, capacityGEP, "cap");
            cap.SetAlignment(8);
            return cap;
        }

        private LLVMValueRef GetArrayDataPtr(LLVMValueRef arrayPtr)
        {
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            // Data pointer is index 2
            var dataPtrGEP = _builder.BuildStructGEP2(GetOrCreateArrayType(), arrayPtr, 2, "data_ptr_ptr");
            var data = _builder.BuildLoad2(i8Ptr, dataPtrGEP, "data_ptr");
            data.SetAlignment(8);
            return data;
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

        /*  Command example of how to construct a dataframe in C# that matches the expected memory layout for your LLVM codegen:

        # READ CSV:
        df = read_csv([index: int, name: string, age: int, hasJob: bool, savings: float], "CSV/mytest.csv")
        df = read_csv("CSV/Fire_Prediction_2023_Bolivia_encoded_small.csv")

        # CREATE DATAFRAME:
        df2 = dataframe(schema={name: string, age: int}, rows=[{"Alice", 25},{"Charlie", 22}])
        df2 = dataframe(schema={name: string, age: int, hasJob: bool}, rows=[{"Alice", 25, true},{"Bob", 30, false},{"Charlie", 22, true}])
        
        # TO CSV
        to_csv(df, "CSV/mytest.csv")

        # RECORDS:
        r = record({name: "Harry Potter", age: 9786, isCool: true, rating: 10.5585})        
        r = {name: "Harry Potter", age: 9786, isCool: true, rating: 10.5585}
        r1 = {name = "Harry", age = 12} + {hasJob= true}
        r2 = {name = "Barry", age = 14, hasJob = true}
        r3 = {name = "Lars", age = 44, hasJob = false}
        arr = [{name = "Barry", age = 14, hasJob = false},{name = "Lars", age = 44, hasJob = true}]
        arr = [r2,r3]

        # SELELT COLUMNS:
        df.select(latitude, longitude)

        # FOR EACH LOOP:
        foreach(item in df) { item.age = item.age + 10 }

        # FOR LOOP:
        for(i=0; i < 500000; i++) { x.add({name: "Hary potter", age: 10 + random(1,100)}) }
        for(i=0; i < 500; i++) df.add({ date: "2023-01-01", latitude: -18.0, longitude: -69.38, wind-speed-min: 1.59, wind-speed-max: 6.47, wind-speed-mean: 3.73, wind-direction-min: 20.86, wind-direction-max: 299.09, wind-direction-mean: 135.45, surface-air-temperature-min: 275.79, surface-air-temperature-max: 284.51, surface-air-temperature-mean: 279.01, total-rainfall-sum: 0.01, surface-humidity-min: 0.01, surface-humidity-max: 0.01, surface-humidity-mean: 0.01, ndvi: 0.15, elevation: 4578.83, slope: 90, aspect: 10.15, fire_label: 1, land_cover_class_1: false, land_cover_class_2: false, land_cover_class_4: false, land_cover_class_5: false, land_cover_class_6: false, land_cover_class_7: false, land_cover_class_8: false, land_cover_class_9: false, land_cover_class_10: false, land_cover_class_11: false, land_cover_class_12: false, land_cover_class_13: false, land_cover_class_14: false, land_cover_class_15: false, land_cover_class_16: true, land_cover_class_17: false })
        for(i=0; i < 500; i++) df.add({ date: "2023-01-01", latitude: -18.0, longitude:  fire_label: 1, land_cover_class_1: false, land_cover_class_2: false, land_cover_class_4: false, land_cover_class_5: false, land_cover_class_6: false, land_cover_class_7: false, land_cover_class_8: false, land_cover_class_9: false, land_cover_class_10: false, land_cover_class_11: false, land_cover_class_12: false, land_cover_class_13: false, land_cover_class_14: false, land_cover_class_15: false, land_cover_class_16: true, land_cover_class_17: false })

        # MIX:
        df = dataframe(schema={date: string, latitude: float, longitude: float, wind-speed-min: float, wind-speed-max: float, wind-speed-mean: float, wind-direction-min: float, wind-direction-max: float, wind-direction-mean: float, surface-air-temperature-min: float, surface-air-temperature-max: float, surface-air-temperature-mean: float, total-rainfall-sum: float, surface-humidity-min: float, surface-humidity-max: float, surface-humidity-mean: float, ndvi: float, elevation: float, slope: float, aspect: float, fire_label: int, land_cover_class_1: bool, land_cover_class_2: bool, land_cover_class_4: bool, land_cover_class_5: bool, land_cover_class_6: bool, land_cover_class_7: bool, land_cover_class_8: bool, land_cover_class_9: bool, land_cover_class_10: bool, land_cover_class_11: bool, land_cover_class_12: bool, land_cover_class_13: bool, land_cover_class_14: bool, land_cover_class_15: bool, land_cover_class_16: bool, land_cover_class_17: bool})
        for(i=0; i < 100; i++) { df.add({ date: "2023-01-01", latitude: -18.0, longitude: -69.38, wind-speed-min: 1.59, wind-speed-max: 6.47, wind-speed-mean: 3.73, wind-direction-min: 20.86, wind-direction-max: 299.09, wind-direction-mean: 135.45, surface-air-temperature-min: 275.79, surface-air-temperature-max: 284.51, surface-air-temperature-mean: 279.01, total-rainfall-sum: 0.01, surface-humidity-min: 0.01, surface-humidity-max: 0.01, surface-humidity-mean: 0.01, ndvi: 0.15, elevation: 4578.83, slope: 90, aspect: 10.15, fire_label: 1, land_cover_class_1: false, land_cover_class_2: false, land_cover_class_4: false, land_cover_class_5: false, land_cover_class_6: false, land_cover_class_7: false, land_cover_class_8: false, land_cover_class_9: false, land_cover_class_10: false, land_cover_class_11: false, land_cover_class_12: false, land_cover_class_13: false, land_cover_class_14: false, land_cover_class_15: false, land_cover_class_16: true, land_cover_class_17: false })}
        x.where(d=> d.age > 50)
        arrname = ["Harry", "Barry", "Mary", "Larry", "Carrie", "Terry", "Sherry", "Perry", "Garry", "Berry", "Narry", "Kerry", "Jerry", "Merry", "Larry", "Carry", "Tarry", "Sherry", "Perry", "Garry",]
        for(i=0; i<49; i++) { df2.add({name: arrname[random(0, arrname.length)], age: random(10,99)}) }

        # TEST: WHERE
        df.where(x => x.latitude > -18.0)
        df.where(x => x.latitude > -18.0).where(x => x.longitude < -69.0)
        df.where(x => x.latitude > -18.0 & x.longitude < -69.0)

        # TEST: MAP
        df.map(x => x.latitude - 100.0)
        df.map(x => x + {latitude= x.latitude - 100.0})
        df.map(x => x + {latitude= x.latitude - 100.0}).map(x => x+{ longitude= 100.0})
        df.map(x => x + {latitude= x.latitude - 100.0, longitude= 100.0})


        # TEST: MIN, MAX, MEAN & SUM
        df_lat = df.map(x => x.latitude);
        df_min = df_lat.min; df_max = df_lat.max; df_mean = df_lat.mean; df_sum = df_lat.sum;


        # TEST: WHERE on array (VECTOR TEST)
        df_lat = df.map(x => x.latitude)
        df_lat.where(x => x > -18.0)
        df_lat.where(x => x > -18.0).where(x => x <  -16.0)


        # TEST: MAP on array (VECTOR TEST)
        df_lat = df.map(x => x.latitude)
        df_lat.map(x => x + 100.0)
        df_lat.map(x => x + 100.0).map(x => x - 0.05)


        # TEST: corr
        df.latitude.corr(df.wind-speed-max)


        compiler list:
        16.6333, 0.9592, 0.5678, 0.5073, 4.9772, 6.3993, 0.5997, 1.033, 0.5602

        IR list:
        12.6831, 11.2088, 6.7141, 7.0108, 10.1785, 14.9467, 19.4002, 13.9254, 9.777

        Runtime list:
        107.503, 108.0411, 60.8264, 59.0818, 64.1876, 1033.8113, 880.304, 1015.3324, 916.654

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