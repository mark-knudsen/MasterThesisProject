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
    public struct RuntimeValue
    {
        public Int16 tag;
        public IntPtr data;
    }

    public enum ValueTag
    {
        Int = 1,
        Float = 2,
        Bool = 3,
        String = 4,
        Array = 5,
        Record = 6,
        None = 0
    }
    public static class LanguageRuntime
    {
        // Regular static method works with GetFunctionPointerForDelegate
        public static IntPtr ReadCsvInternal(IntPtr pathPtr)
        {
            if (pathPtr == IntPtr.Zero) return IntPtr.Zero;
            string path = Marshal.PtrToStringAnsi(pathPtr);
            if (!File.Exists(path)) return IntPtr.Zero;

            try
            {
                var lines = File.ReadAllLines(path).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
                int count = lines.Length;
                IntPtr buffer = Marshal.AllocHGlobal((count + 2) * 8);

                Marshal.WriteInt64(buffer, 0, count);
                Marshal.WriteInt64(buffer, 8, count);

                for (int i = 0; i < count; i++)
                {
                    string val = lines[i].Trim();
                    long bitPattern = 0;

                    // 1. Try Integer
                    if (long.TryParse(val, out long iVal))
                    {
                        bitPattern = iVal;
                    }
                    // 2. Try Boolean
                    else if (bool.TryParse(val, out bool bVal))
                    {
                        bitPattern = bVal ? 1 : 0;
                    }
                    // 3. Try Double/Float
                    else if (double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double dVal))
                    {
                        bitPattern = BitConverter.DoubleToInt64Bits(dVal);
                    }
                    // 4. Fallback: It's a string!
                    else
                    {
                        // We must allocate memory for this string so LLVM can find it later
                        bitPattern = (long)Marshal.StringToHGlobalAnsi(val);
                    }

                    Marshal.WriteInt64(buffer, (i + 2) * 8, bitPattern);
                }
                return buffer;
            }
            catch { return IntPtr.Zero; }
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
        private LLVMTypeRef _i8Ptr;
        private LLVMTypeRef _fopenType;
        private LLVMValueRef _fopenFunc;
        private LLVMTypeRef _fcloseType;
        private LLVMValueRef _fcloseFunc;
        private LLVMTypeRef _fprintfType;
        private LLVMValueRef _fprintfFunc;


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



        // 1. Define the delegate (put this at class level)
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr ReadCsvDelegate(IntPtr path);

        // 2. Add this inside your CompilerOrc class to keep the delegate alive
        private static ReadCsvDelegate _readCsvCachedDelegate;


        public CompilerOrc()
        {
            LLVM.InitializeNativeTarget();
            LLVM.InitializeNativeAsmPrinter();
            LLVM.InitializeNativeAsmParser();
            LLVM.LoadLibraryPermanently(null);


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

        private LLVMValueRef BoxToI64(LLVMValueRef value)
        {
            var i64 = _module.Context.Int64Type;

            // 1. Already i64? No work needed.
            if (value.TypeOf == i64) return value;

            // 2. Double -> i64 (Preserve bit pattern, don't convert value!)
            if (value.TypeOf == _module.Context.DoubleType)
                return _builder.BuildBitCast(value, i64, "double_bits");

            // 3. Pointer -> i64 (Store the address)
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                return _builder.BuildPtrToInt(value, i64, "ptr_bits");

            // 4. Everything else (Int1/Bool, Int32, etc.) -> i64
            // We use Zero-Extend to ensure upper bits are 0.
            return _builder.BuildZExt(value, i64, "zext_to_i64");
        }


        private LLVMValueRef UnboxFromI64(LLVMValueRef val, Type targetType)
        {
            var ctx = _module.Context;

            if (targetType is FloatType)
            {
                // If your array stores RAW bits of a double, use BitCast.
                // If your array stores INTS and you want FLOATS, use SIToFP.
                // Given your previous result (3.95E-322), you likely need BitCast 
                // because your boxing logic is likely BitCasting to i64 on the way in.
                return _builder.BuildBitCast(val, ctx.DoubleType, "unbox_float");
            }

            if (targetType is BoolType)
            {
                // Truncate i64 to i1 (1 bit)
                return _builder.BuildTrunc(val, ctx.Int1Type, "unbox_bool");
            }

            if (targetType is IntType)
            {
                return val; // i64 is our native internal storage
            }

            // For Arrays/Strings (pointers), we cast the i64 back to a pointer
            if (targetType is ArrayType || targetType is StringType)
            {
                var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
                return _builder.BuildIntToPtr(val, i8Ptr, "unbox_ptr");
            }

            return val;
        }
        private unsafe void SetupCsvFunctions()
        {
            var ctx = _module.Context;
            var i32 = ctx.Int32Type;
            _i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 1. Function Declarations (Stay the same)
            _fopenType = LLVMTypeRef.CreateFunction(_i8Ptr, new[] { _i8Ptr, _i8Ptr });
            _fopenFunc = _module.AddFunction("fopen", _fopenType);

            _fcloseType = LLVMTypeRef.CreateFunction(i32, new[] { _i8Ptr });
            _fcloseFunc = _module.AddFunction("fclose", _fcloseType);

            _fprintfType = LLVMTypeRef.CreateFunction(i32, new[] { _i8Ptr, _i8Ptr }, true);
            _fprintfFunc = _module.AddFunction("fprintf", _fprintfType);

            // 2. Setup the JIT Linker
            var jitHandle = (LLVMOrcOpaqueLLJIT*)_jit;
            var dylib = LLVM.OrcLLJITGetMainJITDylib(jitHandle);

            _readCsvCachedDelegate = new ReadCsvDelegate(LanguageRuntime.ReadCsvInternal);
            IntPtr fnAddr = Marshal.GetFunctionPointerForDelegate(_readCsvCachedDelegate);

            // LLVM 20 requires sbyte* for MangleAndIntern
            fixed (byte* pName = System.Text.Encoding.UTF8.GetBytes("ReadCsvInternal\0"))
            {
                var internedName = LLVM.OrcLLJITMangleAndIntern(jitHandle, (sbyte*)pName);

                // FIX 1: Use LLVMJITSymbolGenericFlags directly (no 'Orc' in middle)
                // If the enum itself is missing, use the raw byte: (byte)1
                var flags = new LLVMJITSymbolFlags
                {
                    GenericFlags = (byte)LLVMJITSymbolGenericFlags.LLVMJITSymbolGenericFlagsExported,
                    TargetFlags = 0
                };

                var symbol = new LLVMJITEvaluatedSymbol
                {
                    Address = (ulong)fnAddr.ToInt64(),
                    Flags = flags
                };

                // FIX 2: LLVMOrcCSymbolMapPair field name
                // In this version of the interop, the field is called 'Definition'
                var pair = new LLVMOrcCSymbolMapPair
                {
                    Name = internedName,
                    Sym = symbol
                };

                // FIX 3: Use the direct Materializer function
                var materializer = LLVM.OrcAbsoluteSymbols(&pair, 1);

                var err = LLVM.OrcJITDylibDefine(dylib, materializer);
                if (err != null) ThrowIfError((IntPtr)err);
            }
        }


        private void DeclareValueStruct()
        {
            var ctx = _module.Context;
            var tagType = ctx.Int32Type; // Tag type
                                         // In LLVM 15+, this creates the standard Opaque Pointer (ptr)
            var ptrType = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0); // Pointer to data

            // Define the value struct: tag + pointer to the data (value)
            _runtimeValueType = LLVMTypeRef.CreateStruct(new[] { tagType, ptrType }, false);
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

        private unsafe void EnsureJit()
        {
            if (_jitInitialized) return;

            // 1. Create the Builder
            LLVMOrcOpaqueLLJITBuilder* builder = LLVM.OrcCreateLLJITBuilder();

            // 2. Create the JIT instance
            LLVMOrcOpaqueLLJIT* jitInstance;
            LLVMOpaqueError* error = LLVM.OrcCreateLLJIT(&jitInstance, builder);

            if (error != null)
            {
                // Cast the pointer to IntPtr so your existing ThrowIfError can take it
                ThrowIfError((IntPtr)error);
            }

            // Store the raw pointer as an IntPtr in your class field
            _jit = (IntPtr)jitInstance;
            _jitInitialized = true;
        }




        private void ThrowIfError(IntPtr errPtr)
        {
            if (errPtr == IntPtr.Zero) return;

            unsafe
            {
                // Convert IntPtr back to the opaque error pointer
                var err = (LLVMOpaqueError*)errPtr;

                // In LLVM 20, we use Orc.GetErrorMessage or similar
                // If your interop doesn't have Orc.GetErrorMessage, use the standard:
                sbyte* msgPtr = LLVM.GetErrorMessage(err);
                string message = Marshal.PtrToStringAnsi((IntPtr)msgPtr);

                Console.WriteLine("JIT Error: " + message);

                // Clean up the error
                LLVM.DisposeErrorMessage(msgPtr);
                // Do not call ConsumeError if you called GetErrorMessage (it often consumes it for you)

                throw new Exception(message);
            }
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

        // Problems

        // TODO: fix the problems

        // BROKEN FUNCTIONALITY        
        // assigning a vairable to another variable that is an array is broke  x=[7,8,9]; z=x; x.add(0); z;    z is now some broken data, we have seen this before               
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

            // --- FIX: Deep Type Detection ---
            // We look for the "True" type for display purposes.
            Type displayType = prediction;

            // Drill down into sequences to find the last assignment
            NodeExpr lastStmt = expr;
            if (expr is SequenceNodeExpr seq && seq.Statements.Count > 0)
            {
                lastStmt = seq.Statements.Last();
            }

            if (lastStmt is AssignNodeExpr assign)
            {
                // Get the actual type of the variable from the symbol table
                var symbol = _context.Get(assign.Id);
                if (symbol != null) displayType = symbol.Type;
            }
            // --------------------------------

            CreateMain();
            DeclarePrintf();

            LLVMValueRef resultValue = Visit(expr);

            if (_debug) Console.WriteLine("LLVM TYPE: " + resultValue.TypeOf);
            if (_debug) Console.WriteLine("LANG TYPE: " + prediction);

            // If the user typed "x = 5", VisitAssignExpr already called BoxValue.
            // If the user typed "5 + 5", it's a raw i64 and NEEDS boxing for the REPL to print it.
            LLVMValueRef finalToReturn;

            if (lastStmt is AssignNodeExpr || lastStmt is ToCsvNodeExpr || lastStmt is RecordNodeExpr)
            {
                finalToReturn = resultValue; // Already boxed/ready
            }
            else
            {
                finalToReturn = BoxValue(resultValue, prediction);
            }

            _builder.BuildRet(finalToReturn);

            if (_debug) DumpIR(_module);

            // 5. JIT Compilation
            var tsc = OrcBindings.LLVMOrcCreateNewThreadSafeContext();
            var tsm = OrcBindings.LLVMOrcCreateNewThreadSafeModule(_module.Handle, tsc);
            var dylib = OrcBindings.LLVMOrcLLJITGetMainJITDylib(_jit);
            ThrowIfError(OrcBindings.LLVMOrcLLJITAddLLVMIRModule(_jit, dylib, tsm));

            // 6. Execution
            ulong addr;
            ThrowIfError(OrcBindings.LLVMOrcLLJITLookup(_jit, out addr, _funcName));
            var fnPtr = (IntPtr)addr;
            var delegateResult = Marshal.GetDelegateForFunctionPointer<MainDelegate>(fnPtr);

            IntPtr tempResult = delegateResult();
            if (tempResult == IntPtr.Zero) return null;

            RuntimeValue result = Marshal.PtrToStructure<RuntimeValue>(tempResult);

            // 7. Result Mapping
            switch ((ValueTag)result.tag)
            {
                case ValueTag.Int:
                    return Marshal.ReadInt64(result.data);

                case ValueTag.Float:
                    return BitConverter.Int64BitsToDouble(Marshal.ReadInt64(result.data));

                case ValueTag.String:
                    return Marshal.PtrToStringAnsi(result.data);

                case ValueTag.Bool:
                    return Marshal.ReadInt64(result.data) != 0;

                case ValueTag.Array:
                    if (_debug) Console.WriteLine("return array");

                    // --- USE displayType HERE ---
                    Type innerType = (displayType as ArrayType)?.ElementType;

                    long header = Marshal.ReadInt64(result.data);

                    // Smart dereference logic:
                    // If header > 10,000,000 it's almost certainly a memory address (Box).
                    // Otherwise, it's the array length (Raw Buffer).
                    IntPtr finalBuffer = (header > 10000000) ? Marshal.ReadIntPtr(result.data) : result.data;

                    return HandleArray2(finalBuffer, innerType);
                case ValueTag.Record:
                    if (_debug) Console.WriteLine("return Record");

                    // USE displayType HERE because it was drilled down 
                    // to the actual RecordType during semantic analysis
                    if (displayType is RecordType recType)
                    {
                        return HandleRecord(result.data, recType);
                    }
                    return $"Record Failure: displayType is {displayType?.GetType().Name}";

                case ValueTag.None:
                    return null;

                default:
                    return result;
            }
        }

        // record(["name", "age", "is cool", "rating"], ["Hary potter", 9786, true, 10.5585]) 
        // record([ "full_name", "age_in_moons", "is_active_wizard", "power_level_index",  "assigned_house", "patronus_form","wand_core_material", "academic_gpa", "has_invisibility_cloak", "quidditch_position", "total_gold_galleons", "last_sighting_coordinates"],["Harry James Potter",12456,true,98.7742,"Gryffindor","Stag","Phoenix Feather",3.85,true,"Seeker",45200.50,"51.5074° N, 0.1278° W"])



        private object HandleArray2(IntPtr dataPtr, Type elementType)
        {
            if (dataPtr == IntPtr.Zero) return "[]";

            try
            {
                // 1. Determine if we are looking at the Buffer or a Box
                long firstVal = Marshal.ReadInt64(dataPtr);

                // If firstVal is a pointer (usually > 0x1000000), dereference it to get the buffer.
                // Array lengths are usually small (0 to 1 million).
                IntPtr actualBufferPtr = (firstVal > 1000000) ? Marshal.ReadIntPtr(dataPtr) : dataPtr;

                if (actualBufferPtr == IntPtr.Zero) return "[]";

                long length = Marshal.ReadInt64(actualBufferPtr);
                if (length < 0 || length > 1000000) return "[]";

                var resultElements = new List<string>();

                for (long i = 0; i < length; i++)
                {
                    // Elements start at index 2 (skip Len/Cap)
                    IntPtr elementPtr = IntPtr.Add(actualBufferPtr, (int)((i + 2) * 8));
                    long rawValue = Marshal.ReadInt64(elementPtr);

                    if (elementType is FloatType)
                        resultElements.Add(BitConverter.Int64BitsToDouble(rawValue).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    else if (elementType is BoolType)
                        resultElements.Add((rawValue & 1) == 1 ? "true" : "false");
                    else if (elementType is StringType)
                        resultElements.Add(rawValue == 0 ? "\"\"" : $"\"{Marshal.PtrToStringAnsi((IntPtr)rawValue)}\"");
                    else if (elementType is ArrayType inner)
                        resultElements.Add(HandleArray2((IntPtr)rawValue, inner.ElementType).ToString());
                    else
                        resultElements.Add(rawValue.ToString()); // Default (Int)
                }

                return "[" + string.Join(", ", resultElements) + "]";
            }
            catch (Exception ex)
            {
                return $"[Error: {ex.Message}]";
            }
        }

        private object HandleRecord(IntPtr dataPtr, RecordType record)
        {
            if (dataPtr == IntPtr.Zero) return "{ empty }";

            var result = new Dictionary<string, object>();
            int fieldSize = 8; // Assuming 64-bit alignment for all fields

            if (_debug) Console.WriteLine("Record type: " + record);
            if (_debug) Console.WriteLine("Record type count: " + record.RecordFields?.Count);

            for (int i = 0; i < record.RecordFields?.Count; i++)
            {
                var rec = record.RecordFields[i];
                string label = rec.Label;
                Type recType = rec.Value.Type;

                if (_debug) Console.WriteLine($"Record {i} - label: {label}, type: {recType}");

                // Calculate the address where this specific field is stored inside the record
                IntPtr fieldLocation = IntPtr.Add(dataPtr, i * fieldSize);

                switch (recType)
                {
                    case IntType:
                        // Read 8 bytes directly as a long/int64
                        result[label] = Marshal.ReadInt64(fieldLocation);
                        break;

                    case FloatType:
                        // Read the 8 bytes as a long, then convert the bits to a double
                        long doubleBits = Marshal.ReadInt64(fieldLocation);
                        result[label] = BitConverter.Int64BitsToDouble(doubleBits);
                        break;

                    case BoolType:
                        // LLVM i1 is usually stored as a single byte (0 or 1) 
                        // but padded to the field size in a struct.
                        byte boolVal = Marshal.ReadByte(fieldLocation);
                        result[label] = boolVal != 0;
                        break;

                    case StringType:
                        // The record contains a pointer (address) to the string data
                        IntPtr stringPtr = Marshal.ReadIntPtr(fieldLocation);

                        if (stringPtr == IntPtr.Zero)
                        {
                            result[label] = "null";
                        }
                        else
                        {
                            // Use PtrToStringAnsi for C-style strings (null-terminated)
                            result[label] = Marshal.PtrToStringAnsi(stringPtr);
                        }
                        break;

                    default:
                        result[label] = "Unknown Type";
                        break;
                }
            }

            return "{ " + string.Join(", ", result.Select(kv => $"{kv.Key}: {kv.Value}")) + " }";
        }

        private LLVMValueRef BoxValue(LLVMValueRef value, Type type)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 1. THE RECURSION SHIELD
            // If it's already a managed object, return it as-is.
            // 1. IMPROVED SHIELD
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
            {
                // If it's already the 16-byte struct, return it.
                if (value.Name.Contains("runtime_obj")) return value;

                // If it's the 8-byte box (from VisitArrayExpr), we DON'T return it.
                // We let the code below wrap it in a RuntimeValue { Tag: 5, Data: arr_box }.
            }

            int tag = type switch
            {
                IntType => (Int16)ValueTag.Int,
                FloatType => (Int16)ValueTag.Float,
                BoolType => (Int16)ValueTag.Bool,
                StringType => (Int16)ValueTag.String,
                ArrayType => (Int16)ValueTag.Array,
                RecordType => (Int16)ValueTag.Record,
                _ => (Int16)ValueTag.None
            };

            LLVMValueRef dataPtr;
            var mallocType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i64 }, false);

            // 3. Handle data allocation and storage
            if (type is StringType || type is RecordType || type is ArrayType)
            {
                if (value.IsAGlobalVariable.Handle != IntPtr.Zero)
                {
                    // String constants are the pointer; variables hold the pointer.
                    if (type is StringType && value.Name.StartsWith("str")) dataPtr = value;
                    else dataPtr = _builder.BuildLoad2(i8Ptr, value, "actual_ptr");
                }
                else dataPtr = value;
            }
            // 3. Handle Primitives (Int, Float, Bool)
            else if (type is IntType || type is BoolType || type is FloatType)
            {
                var malloc = GetOrDeclareMalloc();
                dataPtr = _builder.BuildCall2(mallocType, malloc, new[] { LLVMValueRef.CreateConstInt(i64, 8) }, "val_mem");

                LLVMValueRef valToStore = (type is BoolType) ? _builder.BuildZExt(value, i64, "b64") : value;
                _builder.BuildStore(valToStore, dataPtr).SetAlignment(8);
            }
            else dataPtr = LLVMValueRef.CreateConstPointerNull(i8Ptr);

            return CreateRuntimeObject((short)tag, dataPtr);
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
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var func = _builder.InsertBlock.Parent;

            // 1. Get the BOX
            var arrayRef = Visit(expr.Array);

            // 2. Resolve the BUFFER (The Double Load Fix)
            LLVMValueRef boxPtr;
            if (!arrayRef.IsAGlobalVariable.Handle.Equals(IntPtr.Zero))
                boxPtr = _builder.BuildLoad2(i8Ptr, arrayRef, "load_box_from_global");
            else
                boxPtr = arrayRef;

            // LOAD THE ACTUAL BUFFER FROM THE BOX
            var actualBuffer = _builder.BuildLoad2(i8Ptr, boxPtr, "actual_buffer");

            // 3. Fetch length from the Buffer (offset 0)
            var zero64 = LLVMValueRef.CreateConstInt(i64, 0);
            var lenPtr = _builder.BuildGEP2(i64, actualBuffer, new[] { zero64 }, "len_ptr");
            var arrayLen = _builder.BuildLoad2(i64, lenPtr, "array_len");

            // 4. Setup Iterator and Counter
            var elementType = ((ArrayType)expr.Array.Type).ElementType;
            var llvmElemType = GetLLVMType(elementType);
            var iteratorAlloc = _builder.BuildAlloca(llvmElemType, expr.Iterator.Name);

            var counterAlloc = _builder.BuildAlloca(i64, "foreach_counter");
            _builder.BuildStore(zero64, counterAlloc);

            // 5. Update Scope
            var previousContext = _context;
            _context = _context.Add(expr.Iterator.Name, iteratorAlloc, null, elementType);

            // 6. Blocks
            var condBlock = func.AppendBasicBlock("foreach.cond");
            var bodyBlock = func.AppendBasicBlock("foreach.body");
            var endBlock = func.AppendBasicBlock("foreach.end");

            _builder.BuildBr(condBlock);

            // 7. Condition Block
            _builder.PositionAtEnd(condBlock);
            var curIdx = _builder.BuildLoad2(i64, counterAlloc, "cur_idx");
            var isLess = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, curIdx, arrayLen, "foreach_test");
            _builder.BuildCondBr(isLess, bodyBlock, endBlock);

            // 8. Body Block
            _builder.PositionAtEnd(bodyBlock);

            // Calculate memory index (header is 2 x i64)
            var two64 = LLVMValueRef.CreateConstInt(i64, 2);
            var memIdx = _builder.BuildAdd(curIdx, two64, "mem_idx");

            // Get element from actualBuffer, not arrayPtr!
            var elementPtr = _builder.BuildGEP2(i64, actualBuffer, new[] { memIdx }, "elem_ptr");
            var rawVal = _builder.BuildLoad2(i64, elementPtr, "raw_val");

            // Unbox and Store
            var unboxedVal = UnboxFromI64(rawVal, elementType);
            _builder.BuildStore(unboxedVal, iteratorAlloc);

            // Visit the actual loop body
            Visit(expr.Body);

            // 9. Increment (Inside Body)
            var one64 = LLVMValueRef.CreateConstInt(i64, 1);
            var nextIdx = _builder.BuildAdd(curIdx, one64, "next_idx");
            _builder.BuildStore(nextIdx, counterAlloc);
            _builder.BuildBr(condBlock);

            // 10. Wrap up
            _builder.PositionAtEnd(endBlock);
            _context = previousContext;

            return LLVMValueRef.CreateConstPointerNull(i8Ptr);
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
            var ctx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 1. Ensure condition is i1
            if (condValue.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
            {
                condValue = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE,
                    condValue, LLVMValueRef.CreateConstReal(ctx.DoubleType, 0.0), "if_cond_f");
            }
            else if (condValue.TypeOf.Kind != LLVMTypeKind.LLVMIntegerTypeKind || condValue.TypeOf.IntWidth != 1)
            {
                // If it's a boxed bool or int, we need to unbox it or compare it. 
                // For simplicity, assuming condValue is already an unboxed i1 or i64 here.
                condValue = _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE,
                    condValue, LLVMValueRef.CreateConstInt(condValue.TypeOf, 0), "if_cond_i");
            }

            var function = _builder.InsertBlock.Parent;
            var thenBlock = function.AppendBasicBlock("then");
            var elseBlock = function.AppendBasicBlock("else");
            var mergeBlock = function.AppendBasicBlock("ifcont");

            _builder.BuildCondBr(condValue, thenBlock, elseBlock);

            // --- THEN ---
            _builder.PositionAtEnd(thenBlock);
            var thenValue = Visit(node.ThenPart);
            _builder.BuildBr(mergeBlock);
            thenBlock = _builder.InsertBlock; // Update in case Visit created new blocks

            // --- ELSE ---
            _builder.PositionAtEnd(elseBlock);
            LLVMValueRef elseValue;
            if (node.ElsePart != null)
            {
                elseValue = Visit(node.ElsePart);
            }
            else
            {
                // If no else, return a boxed "None" to match the pointer return type
                elseValue = CreateRuntimeObject((short)ValueTag.None, LLVMValueRef.CreateConstPointerNull(i8Ptr));
            }
            _builder.BuildBr(mergeBlock);
            elseBlock = _builder.InsertBlock;

            // --- MERGE ---
            _builder.PositionAtEnd(mergeBlock);

            // PHI node now always takes 'ptr' (the boxed RuntimeValue)
            var phi = _builder.BuildPhi(thenValue.TypeOf, "iftmp");
            phi.AddIncoming(new[] { thenValue, elseValue }, new[] { thenBlock, elseBlock }, 2);

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
        public LLVMValueRef VisitBooleanExpr(BooleanNodeExpr expr)
        {
            return LLVMValueRef.CreateConstInt(_module.Context.Int1Type, expr.Value ? 1UL : 0UL);
        }
        public LLVMValueRef VisitFloatExpr(FloatNodeExpr expr)
        {
            return LLVMValueRef.CreateConstReal(_module.Context.DoubleType, expr.Value);
        }


        public LLVMValueRef VisitAssignExpr(AssignNodeExpr expr)
        {
            // 1. Get the raw value (could be a constant, a global ptr, or a fresh box)
            var value = Visit(expr.Expression);
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);

            // 2. Box it. 
            // We pass 'value' as-is. BoxValue will check if it's a global 
            // and decide if it needs to load the pointer or wrap it.
            var entry = _context.Get(expr.Id);
            var boxedValue = BoxValue(value, entry.Type);

            // 3. --- SMART GLOBAL HANDLING ---
            LLVMValueRef global = _module.GetNamedGlobal(expr.Id);

            if (global.Handle == IntPtr.Zero)
            {
                // Add the global to the current module
                global = _module.AddGlobal(i8Ptr, expr.Id);

                // If we haven't defined it in the JIT session yet, give it an initializer
                if (!_definedGlobals.Contains(expr.Id))
                {
                    global.Initializer = LLVMValueRef.CreateConstNull(i8Ptr);
                    _definedGlobals.Add(expr.Id);
                }

                // Always use ExternalLinkage so the JIT can cross-reference 
                // across different REPL modules
                global.Linkage = LLVMLinkage.LLVMExternalLinkage;
            }

            // 4. Store the 16-byte RuntimeValue pointer into the global variable
            var store = _builder.BuildStore(boxedValue, global);
            store.SetAlignment(8);

            return boxedValue;
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
            var srcAssign = new AssignNodeExpr(srcVarName, expr.ArrayExpr); // instead of creating a new id, we simply use the expr.Array which itself is an id

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

            // Create the sequence program as you did before
            var program = new SequenceNodeExpr();
            program.Statements.Add(srcAssign);
            program.Statements.Add(resultAssign);
            program.Statements.Add(forLoop);

            // Return the result variable (which is a Box)
            program.Statements.Add(new IdNodeExpr(resultVarName));

            PerformSemanticAnalysis(program);

            // This calls VisitSequenceExpr, which calls VisitIdExpr for the last statement.
            // VisitIdExpr MUST return the Box address for array types.
            return VisitSequenceExpr(program);
        }

        private int _mapCounter = 0;

        public LLVMValueRef VisitMapExpr(MapNodeExpr expr)
        {
            int id = _mapCounter++;
            var srcVarName = $"__map_src_{id}";
            var resultVarName = $"__map_res_{id}";
            var indexVarName = $"__map_idx_{id}";

            // Use CopyArrayNodeExpr to ensure we start with a clean Box/Buffer
            var srcAssign = new AssignNodeExpr(srcVarName, new CopyArrayNodeExpr(expr.ArrayExpr));

            // result = new array of same length
            // Simplest way: copy src again, we will overwrite the values anyway
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
            program.Statements.Add(new IdNodeExpr(resultVarName)); // Return the unique box

            PerformSemanticAnalysis(program);
            return VisitSequenceExpr(program);
        }

        private LLVMValueRef CopyArray(LLVMValueRef srcRef, ArrayType type)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // Resolve actual buffer from box
            LLVMValueRef srcBuffer;
            if (!srcRef.IsAGlobalVariable.Handle.Equals(IntPtr.Zero))
            {
                var boxPtr = _builder.BuildLoad2(i8Ptr, srcRef, "load_box_from_global");
                srcBuffer = _builder.BuildLoad2(i8Ptr, boxPtr, "actual_src_buffer");
            }
            else
            {
                srcBuffer = _builder.BuildLoad2(i8Ptr, srcRef, "actual_src_buffer");
            }

            var zero = LLVMValueRef.CreateConstInt(i64, 0);
            var one = LLVMValueRef.CreateConstInt(i64, 1);
            var two = LLVMValueRef.CreateConstInt(i64, 2);
            var eight = LLVMValueRef.CreateConstInt(i64, 8);

            // Load length
            var lenPtr = _builder.BuildGEP2(i64, srcBuffer, new[] { zero }, "len_ptr");
            var length = _builder.BuildLoad2(i64, lenPtr, "length");

            // Allocate new buffer
            var totalSlots = _builder.BuildAdd(length, two, "total_slots");
            var totalBytes = _builder.BuildMul(totalSlots, eight, "total_bytes");

            var mallocFunc = GetOrDeclareMalloc();
            var newBuffer = _builder.BuildCall2(_mallocType, mallocFunc, new[] { totalBytes }, "new_buffer");

            // Initialize header
            var lenPtrNew = _builder.BuildGEP2(i64, newBuffer, new[] { zero }, "len_ptr_new");
            var capPtrNew = _builder.BuildGEP2(i64, newBuffer, new[] { one }, "cap_ptr_new");

            _builder.BuildStore(length, lenPtrNew);
            _builder.BuildStore(length, capPtrNew);

            // Loop setup
            var indexAlloc = _builder.BuildAlloca(i64, "i");
            _builder.BuildStore(zero, indexAlloc);

            var func = _builder.InsertBlock.Parent;
            var loopCond = func.AppendBasicBlock("clone.cond");
            var loopBody = func.AppendBasicBlock("clone.body");
            var loopEnd = func.AppendBasicBlock("clone.end");

            _builder.BuildBr(loopCond);

            // Condition
            _builder.PositionAtEnd(loopCond);
            var iVal = _builder.BuildLoad2(i64, indexAlloc, "i_val");
            var cmp = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, iVal, length, "cmp");
            _builder.BuildCondBr(cmp, loopBody, loopEnd);

            // Body
            _builder.PositionAtEnd(loopBody);
            var offset = _builder.BuildAdd(iVal, two, "offset");

            var srcElemPtr = _builder.BuildGEP2(i64, srcBuffer, new[] { offset }, "src_elem_ptr");
            var val = _builder.BuildLoad2(i64, srcElemPtr, "val");

            var dstElemPtr = _builder.BuildGEP2(i64, newBuffer, new[] { offset }, "dst_elem_ptr");
            _builder.BuildStore(val, dstElemPtr);

            var next = _builder.BuildAdd(iVal, one, "next");
            _builder.BuildStore(next, indexAlloc);

            _builder.BuildBr(loopCond);

            // End
            _builder.PositionAtEnd(loopEnd);

            // Wrap in box
            var boxSize = eight;
            var newBox = _builder.BuildCall2(_mallocType, mallocFunc, new[] { boxSize }, "new_box");
            _builder.BuildStore(newBuffer, newBox);

            return newBox;
        }




        public LLVMValueRef VisitMapExprMutating(MapNodeExpr expr) // not in use, maybe use with like an argument, eg: x.map(d => d+2, true) 
        {
            // Temp variable names
            var srcVarName = "__map_src";
            var indexVarName = "__map_i";

            // 1. Store source array
            var srcAssign = new AssignNodeExpr(srcVarName, expr.ArrayExpr);

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
                ReplaceIterator(idx.ArrayExpression, iteratorName, replacement);
                ReplaceIterator(idx.IndexExpression, iteratorName, replacement);
            }

            return node;
        }
        public LLVMValueRef VisitArrayExpr(ArrayNodeExpr expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var count = (uint)expr.Elements.Count;
            var mallocFunc = GetOrDeclareMalloc();

            // 1. Allocate and Setup Buffer [Length, Capacity, Data...]
            var totalBytes = LLVMValueRef.CreateConstInt(i64, (ulong)(count + 2) * 8);
            var arrayPtr = _builder.BuildCall2(_mallocType, mallocFunc, new[] { totalBytes }, "arr_buffer");

            var countVal = LLVMValueRef.CreateConstInt(i64, count);
            _builder.BuildStore(countVal, _builder.BuildGEP2(i64, arrayPtr, new[] { LLVMValueRef.CreateConstInt(i64, 0) })).SetAlignment(8);
            _builder.BuildStore(countVal, _builder.BuildGEP2(i64, arrayPtr, new[] { LLVMValueRef.CreateConstInt(i64, 1) })).SetAlignment(8);

            // 2. Initialize elements
            for (int i = 0; i < expr.Elements.Count; i++)
            {
                var elementValue = Visit(expr.Elements[i]);
                var elementPtr = _builder.BuildGEP2(i64, arrayPtr, new[] { LLVMValueRef.CreateConstInt(i64, (ulong)(i + 2)) }, $"elem_{i}");
                _builder.BuildStore(elementValue, elementPtr).SetAlignment(8);
            }

            // 3. Create the stable Box (Pointer to the Buffer)
            var boxPtr = _builder.BuildCall2(_mallocType, mallocFunc, new[] { LLVMValueRef.CreateConstInt(i64, 8) }, "arr_box");
            _builder.BuildStore(arrayPtr, boxPtr).SetAlignment(8);

            expr.SetType(new ArrayType(expr.ElementType));
            return boxPtr;
        }



        public LLVMValueRef VisitIndexExpr(IndexNodeExpr expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var func = _builder.InsertBlock.Parent;

            // 1. Get the value from the expression (e.g., the variable 'c' or 'arr')
            var arrayValue = Visit(expr.ArrayExpression);

            // 2. UNWRAP THE RUNTIME OBJECT
            // Variables like 'c' or 'arr' are globals pointing to a 16-byte RuntimeValue.
            // We need to get the 'data' pointer (the 8 bytes at offset 8).
            LLVMValueRef runtimeObjPtr;
            if (arrayValue.IsAGlobalVariable.Handle != IntPtr.Zero)
            {
                runtimeObjPtr = _builder.BuildLoad2(i8Ptr, arrayValue, "load_runtime_obj");
            }
            else
            {
                runtimeObjPtr = arrayValue;
            }

            // Now get the 'data' field from { i16 tag, i8* data }
            var dataPtrAddr = _builder.BuildGEP2(LLVMTypeRef.CreateStruct(new[] { ctx.Int16Type, i8Ptr }, false),
                                                 runtimeObjPtr, new[] { LLVMValueRef.CreateConstInt(ctx.Int32Type, 0), LLVMValueRef.CreateConstInt(ctx.Int32Type, 1) }, "data_field_ptr");
            var dataPtr = _builder.BuildLoad2(i8Ptr, dataPtrAddr, "array_data_ptr");

            // 3. SMART DEREFERENCE (LLVM Version of HandleArray2 logic)
            // We check the first 8 bytes. If it's a huge number (pointer), we load again.
            var first8Bytes = _builder.BuildLoad2(i64, dataPtr, "peek_header");
            var threshold = LLVMValueRef.CreateConstInt(i64, 1000000);
            var isBoxed = _builder.BuildICmp(LLVMIntPredicate.LLVMIntUGT, first8Bytes, threshold, "is_boxed_ptr");

            var actualBuffer = _builder.BuildSelect(isBoxed,
                _builder.BuildLoad2(i8Ptr, dataPtr, "deref_box"),
                dataPtr,
                "final_buffer_ptr");

            // 4. Evaluate Index
            var indexVal = Visit(expr.IndexExpression);
            // ... rest of your index logic (Bounds check and Load) ...

            // (Keep your existing Bounds Check code here, using 'actualBuffer')

            // 5. Load Element
            var actualIdx = _builder.BuildAdd(indexVal, LLVMValueRef.CreateConstInt(i64, 2), "offset_idx");
            var elementPtr = _builder.BuildGEP2(i64, actualBuffer, new[] { actualIdx }, "elem_ptr");
            var rawValue = _builder.BuildLoad2(i64, elementPtr, "raw_val");

            // 6. Cast based on Element Type
            var arrayType = (ArrayType)expr.ArrayExpression.Type;
            var elemType = arrayType.ElementType;

            if (elemType is FloatType)
                return _builder.BuildBitCast(rawValue, ctx.DoubleType, "to_float");
            if (elemType is BoolType)
                return _builder.BuildTrunc(rawValue, ctx.Int1Type, "to_bool");
            if (elemType is StringType || elemType is ArrayType)
                return _builder.BuildIntToPtr(rawValue, i8Ptr, "to_ptr");

            return rawValue; // Default Int
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
                StringType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), // Strings are pointers
                ArrayType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0),  // Arrays  are pointers
                                                                          //RecordType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), // Record  are pointers
                                                                          // RecordType recType => GetOrCreateStructType(recType),
                RecordType recType => LLVMTypeRef.CreatePointer(GetOrCreateStructType(recType), 0),
                BoolType => ctx.Int1Type,
                _ => throw new Exception($"Unsupported type: {type}") // it doesn't have a type? how?
            };
        }

        public LLVMValueRef VisitAddExpr(AddNodeExpr expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 1. Get the Box pointer (from global or local)
            var boxPtrRaw = Visit(expr.ArrayExpression);
            LLVMValueRef boxPtr;
            if (!boxPtrRaw.IsAGlobalVariable.Handle.Equals(IntPtr.Zero))
                boxPtr = _builder.BuildLoad2(i8Ptr, boxPtrRaw, "load_box_from_global");
            else
                boxPtr = boxPtrRaw;

            // 2. Load the actual buffer from the Box
            var arrayPtr = _builder.BuildLoad2(i8Ptr, boxPtr, "actual_buffer");

            var value = Visit(expr.AddExpression);
            var boxedVal = BoxToI64(value);

            // ... (Length/Capacity loading logic remains the same using arrayPtr) ...
            var lenPtr = _builder.BuildGEP2(i64, arrayPtr, new[] { LLVMValueRef.CreateConstInt(i64, 0) }, "len_ptr");
            var length = _builder.BuildLoad2(i64, lenPtr, "length");
            var capPtr = _builder.BuildGEP2(i64, arrayPtr, new[] { LLVMValueRef.CreateConstInt(i64, 1) }, "cap_ptr");
            var capacity = _builder.BuildLoad2(i64, capPtr, "capacity");

            var isFull = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, length, capacity, "is_full");
            var function = _builder.InsertBlock.Parent;
            var growBlock = ctx.AppendBasicBlock(function, "grow");
            var contBlock = ctx.AppendBasicBlock(function, "cont");
            var entryBlock = _builder.InsertBlock;
            _builder.BuildCondBr(isFull, growBlock, contBlock);

            // ---- grow ----
            _builder.PositionAtEnd(growBlock);
            var newCap = _builder.BuildSelect(_builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, capacity, LLVMValueRef.CreateConstInt(i64, 0)),
                         LLVMValueRef.CreateConstInt(i64, 4), _builder.BuildMul(capacity, LLVMValueRef.CreateConstInt(i64, 2)), "new_cap");

            var totalBytes = _builder.BuildMul(_builder.BuildAdd(newCap, LLVMValueRef.CreateConstInt(i64, 2)), LLVMValueRef.CreateConstInt(i64, 8));
            var reallocFunc = GetOrDeclareRealloc();
            var newArrayPtr = _builder.BuildCall2(_reallocType, reallocFunc, new[] { arrayPtr, totalBytes }, "realloc_array");

            _builder.BuildStore(newCap, _builder.BuildGEP2(i64, newArrayPtr, new[] { LLVMValueRef.CreateConstInt(i64, 1) })).SetAlignment(8);

            // CRITICAL: Update the Box with the new pointer!
            _builder.BuildStore(newArrayPtr, boxPtr).SetAlignment(8);
            _builder.BuildBr(contBlock);

            // ---- continue ----
            _builder.PositionAtEnd(contBlock);
            var arrayPtrPhi = _builder.BuildPhi(i8Ptr, "active_buffer");
            arrayPtrPhi.AddIncoming(new[] { arrayPtr, newArrayPtr }, new[] { entryBlock, growBlock }, 2);

            // ... (Store element and increment length as before) ...
            var elemPtr = _builder.BuildGEP2(i64, arrayPtrPhi, new[] { _builder.BuildAdd(length, LLVMValueRef.CreateConstInt(i64, 2)) });
            _builder.BuildStore(boxedVal, elemPtr).SetAlignment(8);
            _builder.BuildStore(_builder.BuildAdd(length, LLVMValueRef.CreateConstInt(i64, 1)), _builder.BuildGEP2(i64, arrayPtrPhi, new[] { LLVMValueRef.CreateConstInt(i64, 0) }));

            return boxPtr; // Return the stable box
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
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // --- 1. Get the BOX pointer ---
            var boxPtrRaw = Visit(expr.ArrayExpression);

            LLVMValueRef boxPtr;
            if (!boxPtrRaw.IsAGlobalVariable.Handle.Equals(IntPtr.Zero))
                boxPtr = _builder.BuildLoad2(i8Ptr, boxPtrRaw, "load_box");
            else
                boxPtr = boxPtrRaw;

            // --- 2. Load the actual BUFFER from the Box ---
            var arrayBuffer = _builder.BuildLoad2(i8Ptr, boxPtr, "actual_buffer");

            // 3. Evaluate Index to remove
            var indexVal = Visit(expr.RemoveExpression);
            if (indexVal.TypeOf == ctx.DoubleType)
                indexVal = _builder.BuildFPToSI(indexVal, i64, "idx_int");

            // Constants
            var zero64 = LLVMValueRef.CreateConstInt(i64, 0);
            var one64 = LLVMValueRef.CreateConstInt(i64, 1);
            var two64 = LLVMValueRef.CreateConstInt(i64, 2);
            var eight64 = LLVMValueRef.CreateConstInt(i64, 8);
            var falseBit = LLVMValueRef.CreateConstInt(ctx.Int1Type, 0);

            // --- 4. Load Length (using arrayBuffer) ---
            var lenPtr = _builder.BuildGEP2(i64, arrayBuffer, new[] { zero64 }, "len_ptr");
            var length = _builder.BuildLoad2(i64, lenPtr, "length");

            // --- 5. Bounds Check ---
            var inBounds = _builder.BuildICmp(LLVMIntPredicate.LLVMIntULT, indexVal, length, "in_bounds");

            var function = _builder.InsertBlock.Parent;
            var removeBlock = function.AppendBasicBlock("do_remove");
            var skipBlock = function.AppendBasicBlock("skip_remove");

            _builder.BuildCondBr(inBounds, removeBlock, skipBlock);

            // --- 6. Remove Block ---
            _builder.PositionAtEnd(removeBlock);

            // moveCount = length - index - 1
            var moveCount = _builder.BuildSub(_builder.BuildSub(length, indexVal, "tmp1"), one64, "move_count");

            var needMove = _builder.BuildICmp(LLVMIntPredicate.LLVMIntUGT, moveCount, zero64, "need_move");
            var memmoveBlock = function.AppendBasicBlock("memmove_block");
            var afterMoveBlock = function.AppendBasicBlock("after_memmove");
            _builder.BuildCondBr(needMove, memmoveBlock, afterMoveBlock);

            // --- 7. Memmove Block ---
            _builder.PositionAtEnd(memmoveBlock);

            // Index 0 is at offset 2 in buffer [Len, Cap, E0, E1...]
            var dstIdx = _builder.BuildAdd(two64, indexVal, "dst_idx");
            var srcIdx = _builder.BuildAdd(dstIdx, one64, "src_idx");

            var dstPtr = _builder.BuildGEP2(i64, arrayBuffer, new[] { dstIdx }, "dst_ptr");
            var srcPtr = _builder.BuildGEP2(i64, arrayBuffer, new[] { srcIdx }, "src_ptr");
            var bytes = _builder.BuildMul(moveCount, eight64, "move_bytes");

            var memmoveFunc = GetOrDeclareMemmove();
            _builder.BuildCall2(
                _memmoveType,
                memmoveFunc,
                new[] { dstPtr, srcPtr, bytes, falseBit },
                ""
            );

            _builder.BuildBr(afterMoveBlock);

            // --- 8. After Memmove (Update Length) ---
            _builder.PositionAtEnd(afterMoveBlock);
            var newLength = _builder.BuildSub(length, one64, "new_length");
            _builder.BuildStore(newLength, lenPtr).SetAlignment(8);
            _builder.BuildBr(skipBlock);

            // --- 9. Skip Block ---
            _builder.PositionAtEnd(skipBlock);

            // Return the Box (the stable reference)
            return boxPtr;
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
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 1. Get the BOX pointer
            var boxPtrRaw = Visit(expr.ArrayExpression);

            LLVMValueRef boxPtr;
            if (!boxPtrRaw.IsAGlobalVariable.Handle.Equals(IntPtr.Zero))
                boxPtr = _builder.BuildLoad2(i8Ptr, boxPtrRaw, "load_box_from_global");
            else
                boxPtr = boxPtrRaw;

            // 2. NEW: Load the actual BUFFER from the Box
            var actualBuffer = _builder.BuildLoad2(i8Ptr, boxPtr, "actual_buffer");

            // 3. Length is at offset 0 of the buffer
            var zero64 = LLVMValueRef.CreateConstInt(i64, 0);
            var lenPtr = _builder.BuildGEP2(i64, actualBuffer, new[] { zero64 }, "len_ptr");

            return _builder.BuildLoad2(i64, lenPtr, "length");
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
            var firstElement = new IndexNodeExpr(new IdNodeExpr(arrayVar), new FloatNodeExpr(0));
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
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);

            LLVMValueRef ptr = _module.GetNamedGlobal(expr.Name);
            if (ptr.Handle == IntPtr.Zero)
            {
                ptr = _module.AddGlobal(i8Ptr, expr.Name);
                ptr.Linkage = LLVMLinkage.LLVMExternalLinkage;
            }

            var boxPtr = _builder.BuildLoad2(i8Ptr, ptr, "runtime_obj_" + expr.Name);
            return boxPtr;
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
            // Safety check for empty branches (like an 'if' without an 'else')
            if (expr == null) return default;

            if (_debug)
            {
                var type = expr.GetType();
                var name = type.Name;

                // Remove "NodeExpr" suffix for a cleaner console view
                string cleanName = name.EndsWith("NodeExpr")
                    ? name.Substring(0, name.Length - 8)
                    : name;

                Console.WriteLine($"[DEBUG] Visiting: {cleanName}");
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
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var func = _builder.InsertBlock.Parent;

            // 1. Get the Array Value (e.g., from variable @arr or @c)
            var arrayValue = Visit(expr.ArrayExpression);

            // 2. UNWRAP RUNTIME OBJECT (16-byte struct)
            // We need to get the 'data' pointer from the RuntimeValue { i16 tag, i8* data }
            LLVMValueRef runtimeObjPtr = arrayValue.IsAGlobalVariable.Handle != IntPtr.Zero
                ? _builder.BuildLoad2(i8Ptr, arrayValue, "load_runtime_obj")
                : arrayValue;

            // Access field [1] (the data pointer) of the RuntimeValue struct
            var runtimeStructType = LLVMTypeRef.CreateStruct(new[] { ctx.Int16Type, i8Ptr }, false);
            var dataPtrAddr = _builder.BuildGEP2(runtimeStructType, runtimeObjPtr,
                new[] { LLVMValueRef.CreateConstInt(ctx.Int32Type, 0), LLVMValueRef.CreateConstInt(ctx.Int32Type, 1) }, "data_field_ptr");
            var dataPtr = _builder.BuildLoad2(i8Ptr, dataPtrAddr, "array_data_ptr");

            // 3. SMART DEREFERENCE (Handle Box vs Raw Buffer)
            // Peek at the first 8 bytes. If > 1,000,000, it's a pointer to a buffer (Box).
            var first8Bytes = _builder.BuildLoad2(i64, dataPtr, "peek_header");
            var isBoxed = _builder.BuildICmp(LLVMIntPredicate.LLVMIntUGT, first8Bytes, LLVMValueRef.CreateConstInt(i64, 1000000), "is_boxed");

            var arrayBuffer = _builder.BuildSelect(isBoxed,
                _builder.BuildLoad2(i8Ptr, dataPtr, "deref_box"),
                dataPtr,
                "actual_buffer");

            // 4. Evaluate and Cast Index
            var indexVal = Visit(expr.IndexExpression);
            if (indexVal.TypeOf == ctx.DoubleType)
                indexVal = _builder.BuildFPToSI(indexVal, i64, "idx_int");

            // 5. Bounds Check
            var zero = LLVMValueRef.CreateConstInt(i64, 0);
            var arrayLen = _builder.BuildLoad2(i64, _builder.BuildGEP2(i64, arrayBuffer, new[] { zero }, "len_ptr"), "array_len");

            var isInvalid = _builder.BuildOr(
                _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, indexVal, zero),
                _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, indexVal, arrayLen),
                "is_invalid");

            var failBlock = func.AppendBasicBlock("bounds.fail");
            var safeBlock = func.AppendBasicBlock("bounds.ok");
            _builder.BuildCondBr(isInvalid, failBlock, safeBlock);

            // --- FAIL BLOCK ---
            _builder.PositionAtEnd(failBlock);
            var errorMsg = _builder.BuildGlobalStringPtr("Runtime Error: Index Out of Bounds!\n", "err_msg");
            _builder.BuildCall2(_printfType, _printf, new[] { errorMsg }, "print_err");
            _builder.BuildRet(LLVMValueRef.CreateConstNull(i8Ptr)); // Abort current execution

            // --- SAFE BLOCK ---
            _builder.PositionAtEnd(safeBlock);
            var actualIdx = _builder.BuildAdd(indexVal, LLVMValueRef.CreateConstInt(i64, 2), "offset_idx");
            var elementPtr = _builder.BuildGEP2(i64, arrayBuffer, new[] { actualIdx }, "elem_ptr");

            // 6. Evaluate and Store RHS
            var valueToAssign = Visit(expr.AssignExpression);

            // Ensure the value is cast to i64 (bit-pattern) before storing in the buffer
            var valueAsI64 = BoxToI64(valueToAssign);

            _builder.BuildStore(valueAsI64, elementPtr).SetAlignment(8);

            return valueToAssign;
        }


        public LLVMValueRef VisitToCsvExpr(ToCsvNodeExpr expr)
        {
            if (_fopenFunc.Handle == IntPtr.Zero) SetupCsvFunctions();

            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var i16 = ctx.Int16Type;
            var i32 = ctx.Int32Type;

            // --- 1. RESOLVE FILENAME ---
            var fileNameValue = Visit(expr.FileNameExpr);
            LLVMValueRef fileNamePtr = !fileNameValue.IsAGlobalVariable.Handle.Equals(IntPtr.Zero)
                ? fileNameValue : _builder.BuildLoad2(i8Ptr, fileNameValue, "fname_ptr");

            // --- 2. UNWRAP RUNTIME OBJECT ---
            var runtimeValuePtr = Visit(expr.Expression);
            if (runtimeValuePtr.IsAGlobalVariable.Handle != IntPtr.Zero)
                runtimeValuePtr = _builder.BuildLoad2(i8Ptr, runtimeValuePtr, "load_global_rv");

            var rvType = LLVMTypeRef.CreateStruct(new[] { i16, i8Ptr }, false);
            var dataFieldAddr = _builder.BuildStructGEP2(rvType, runtimeValuePtr, 1, "data_field_addr");
            var boxOrBufferPtr = _builder.BuildLoad2(i8Ptr, dataFieldAddr, "box_or_buffer");

            // --- 3. SMART DEREFERENCE ---
            var headerPeek = _builder.BuildLoad2(i64, boxOrBufferPtr, "peek_header");
            var isBoxed = _builder.BuildICmp(LLVMIntPredicate.LLVMIntUGT, headerPeek,
                LLVMValueRef.CreateConstInt(i64, 1000000), "is_boxed");

            var arrayBuffer = _builder.BuildSelect(isBoxed,
                _builder.BuildLoad2(i8Ptr, boxOrBufferPtr, "load_inner_buffer"),
                boxOrBufferPtr,
                "actual_array_buffer");

            // --- 4. OPEN FILE ---
            var modePtr = _builder.BuildGlobalStringPtr("w", "mode_w");
            var fileHandle = _builder.BuildCall2(_fopenType, _fopenFunc, new[] { fileNamePtr, modePtr }, "fh");

            // --- 5. SETUP LOOP & LENGTH ---
            var zero64 = LLVMValueRef.CreateConstInt(i64, 0);
            var lenPtr = _builder.BuildGEP2(i64, arrayBuffer, new[] { zero64 }, "len_ptr");
            var length = _builder.BuildLoad2(i64, lenPtr, "array_len");

            var func = _builder.InsertBlock.Parent;
            var condBB = func.AppendBasicBlock("csv.cond");
            var bodyBB = func.AppendBasicBlock("csv.body");
            var endBB = func.AppendBasicBlock("csv.end");

            var idxAlloc = _builder.BuildAlloca(i64, "csv_idx");
            _builder.BuildStore(zero64, idxAlloc);
            _builder.BuildBr(condBB);

            // --- 6. LOOP CONDITION ---
            _builder.PositionAtEnd(condBB);
            var currIdx = _builder.BuildLoad2(i64, idxAlloc, "curr_idx");
            var cond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, currIdx, length, "loop_test");
            _builder.BuildCondBr(cond, bodyBB, endBB);

            // --- 7. LOOP BODY (Writing Data) ---
            _builder.PositionAtEnd(bodyBB);

            var memIdx = _builder.BuildAdd(currIdx, LLVMValueRef.CreateConstInt(i64, 2), "mem_idx");
            var elemPtr = _builder.BuildGEP2(i64, arrayBuffer, new[] { memIdx }, "elem_ptr");
            var rawVal = _builder.BuildLoad2(i64, elemPtr, "raw_val");

            var arrayType = (ArrayType)expr.Expression.Type;
            LLVMValueRef printVal = rawVal;
            string fmtStr = "%lld\n";

            // --- Handle Booleans, Floats, and Strings ---
            if (arrayType.ElementType is BoolType)
            {
                var boolBit = _builder.BuildTrunc(rawVal, ctx.Int1Type, "bool_bit");
                var trueStr = _builder.BuildGlobalStringPtr("true\n", "str_true");
                var falseStr = _builder.BuildGlobalStringPtr("false\n", "str_false");
                printVal = _builder.BuildSelect(boolBit, trueStr, falseStr, "bool_text_ptr");
                fmtStr = "%s";
            }
            else if (arrayType.ElementType is FloatType)
            {
                printVal = _builder.BuildBitCast(rawVal, ctx.DoubleType, "val_to_double");
                fmtStr = "%f\n";
            }
            else if (arrayType.ElementType is StringType)
            {
                // Convert the i64 address back to an i8* pointer for fprintf
                printVal = _builder.BuildIntToPtr(rawVal, i8Ptr, "str_ptr");
                fmtStr = "%s\n";
            }

            var fmt = _builder.BuildGlobalStringPtr(fmtStr, "csv_fmt");
            _builder.BuildCall2(_fprintfType, _fprintfFunc, new[] { fileHandle, fmt, printVal }, "written");

            // Increment index
            var nextIdx = _builder.BuildAdd(currIdx, LLVMValueRef.CreateConstInt(i64, 1), "next_idx");
            _builder.BuildStore(nextIdx, idxAlloc);
            _builder.BuildBr(condBB);

            // --- 8. CLEANUP & RETURN ---
            _builder.PositionAtEnd(endBB);
            _builder.BuildCall2(_fcloseType, _fcloseFunc, new[] { fileHandle }, "closed");

            return CreateRuntimeObject((short)0, LLVMValueRef.CreateConstPointerNull(i8Ptr));
        }


        private LLVMValueRef CreateRuntimeObject(short tag, LLVMValueRef dataPtr)
        {
            var ctx = _module.Context;
            var i16 = ctx.Int16Type;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 1. Define the struct type { i16, ptr }
            var runtimeObjType = LLVMTypeRef.CreateStruct(new[] { i16, i8Ptr }, false);

            // 2. Ensure malloc is declared correctly
            var mallocFunc = _module.GetNamedFunction("malloc");
            var mallocType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i64 });
            if (mallocFunc.Handle == IntPtr.Zero)
            {
                mallocFunc = _module.AddFunction("malloc", mallocType);
            }

            // 3. Allocate 16 bytes for the struct
            var runtimeObj = _builder.BuildCall2(mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(i64, 16) }, "runtime_obj");

            // 4. Store the Tag
            var tagPtr = _builder.BuildStructGEP2(runtimeObjType, runtimeObj, 0, "tag_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i16, (ulong)tag), tagPtr);

            // 5. Store the Data Pointer
            var dataFieldPtr = _builder.BuildStructGEP2(runtimeObjType, runtimeObj, 1, "data_ptr");
            _builder.BuildStore(dataPtr, dataFieldPtr);

            return runtimeObj;
        }



        public LLVMValueRef VisitReadCsvExpr(ReadCsvNodeExpr node)
        {
            var pathBox = Visit(node.FileNameExpr);
            LLVMValueRef actualPath;

            // Check if the path is a literal string constant or a boxed variable
            if (pathBox.IsAConstantDataSequential.Handle != IntPtr.Zero || pathBox.IsAGlobalVariable.Handle != IntPtr.Zero)
            {
                // It's a literal string (@str constant)
                actualPath = pathBox;
            }
            else
            {
                // It's a boxed variable { i16 tag, ptr data }, so unbox it
                var pathPtrGEP = _builder.BuildStructGEP2(_runtimeValueType, pathBox, 1, "path_ptr_gep");
                actualPath = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0), pathPtrGEP, "actual_path");
            }

            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            var readCsvType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i8Ptr });

            var readCsvFunc = _module.GetNamedFunction("ReadCsvInternal");
            if (readCsvFunc.Handle == IntPtr.Zero)
            {
                readCsvFunc = _module.AddFunction("ReadCsvInternal", readCsvType);
            }

            var rawArrayPtr = _builder.BuildCall2(readCsvType, readCsvFunc, new[] { actualPath }, "raw_array_ptr");

            return CreateRuntimeObject(5, rawArrayPtr);
        }

        public LLVMValueRef VisitCopyExpr(CopyNodeExpr expr)
        {
            var value = Visit(expr.Expression); // LLVM value
            var type = expr.Expression.Type;    // language type

            if (type is ArrayType tt)
                return CopyArray(value, tt);

            if (type is RecordType t)
                return CopyRecord(value, t);

            throw new Exception($"Cannot call copy on type {type}");
        }


        public LLVMValueRef VisitRecordExpr(RecordNodeExpr expr)
        {
            // 1. Collect LLVM Types from the fields to define the Struct
            var fieldTypes = new List<LLVMTypeRef>();

            foreach (var field in expr.Fields) // x=record(["name", "age"],["dan", 100])    x.name   z=record(["name", "age","iscool", "score"],["dan", 100, true, 10.435345]) 
            {
                // This returns your ContextEntry
                var entry = Visit(field.Value);
                fieldTypes.Add(GetLLVMType(field.Value.Type));
            }

            // 2. Define the STRUCT TYPE (The Blueprint)
            string structName = "struct_" + string.Join("_", expr.Fields.Select(f => f.Label));

            // Look in LLVM's internal type registry
            LLVMTypeRef structType = _module.GetTypeByName(structName);

            // If LLVM doesn't know this struct yet, define it
            if (structType.Handle == IntPtr.Zero)
            {
                // USE THE LLVM CONTEXT, NOT YOUR CLASS
                structType = _module.Context.CreateNamedStruct(structName);
                structType.StructSetBody(fieldTypes.ToArray(), Packed: false);
            }

            // 3. Allocate Memory (Heap allocation via malloc)
            // For a REPL/JIT, malloc is safer for persistence
            //var sizeOf = structType.SizeOf;
            var instancePtr = _builder.BuildMalloc(structType, "record_mem");

            // 4. Store each field value into the Struct
            for (int i = 0; i < expr.Fields.Count; i++)
            {
                var fieldValue = expr.Fields[i].Value.Accept(this);

                // BuildStructGEP2 gets the pointer to the specific field index
                var elementPtr = _builder.BuildStructGEP2(structType, instancePtr, (uint)i, $"ptr_{expr.Fields[i].Label}");


                var store = _builder.BuildStore(fieldValue, elementPtr);
                store.SetAlignment(8);
            }

            return instancePtr;
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
            // 1. Get the value (which is currently a pointer to a RuntimeValue struct)
            var boxedPtr = Visit(recordExpr);

            // 2. UNWRAP THE BOX
            // We need to load the 'data' field (at index 1) from the RuntimeValue
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            var runtimeValueType = LLVMTypeRef.CreateStruct(new[] { _module.Context.Int16Type, i8Ptr }, false);

            // Get pointer to the data field in the box
            var dataFieldAddr = _builder.BuildStructGEP2(runtimeValueType, boxedPtr, 1, "data_addr");
            // Load the actual raw struct pointer
            var rawRecordPtr = _builder.BuildLoad2(i8Ptr, dataFieldAddr, "raw_record_ptr");

            // 3. Now use the raw pointer with your struct type
            var recordType = recordExpr.Type as RecordType;
            var structType = GetOrCreateStructType(recordType);
            int fieldIndex = GetFieldIndex(fieldName, recordType.RecordFields);

            var fieldPtr = _builder.BuildStructGEP2(
                structType,
                rawRecordPtr, // Use the unwrapped pointer here!
                (uint)fieldIndex,
                $"ptr_{fieldName}"
            );

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

        public LLVMValueRef VisitRecordFieldExpr(RecordFieldNodeExpr expr)
        {
            var (fieldPtr, fieldType) = GetFieldPointer(expr.IdRecord, expr.IdField);

            var value = _builder.BuildLoad2(
                fieldType,
                fieldPtr,
                $"load_{expr.IdField}"
            );

            return value;
        }

        private LLVMValueRef CopyRecord(LLVMValueRef recordPtr, RecordType recordType)
        {
            Console.WriteLine("copying record!");
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
    }
}