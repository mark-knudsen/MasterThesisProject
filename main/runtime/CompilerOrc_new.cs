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
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RuntimeValue
    {
        public short tag;      // Index 0 (i16)
        public short pad1;     // Index 1 (i16)
        public int pad2;       // Index 2 (i32)
        public IntPtr data;    // Index 3 (ptr)

        public RuntimeValue(short tag, IntPtr data)
        {
            this.tag = tag;
            this.pad1 = 0;
            this.pad2 = 0;
            this.data = data;
        }
    }



    public enum ValueTag
    {
        Int = 1,
        Float = 2,
        Bool = 3,
        String = 4,
        Array = 5,
        None = 0
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
        //private ArrayHelpers _arrayHelper;
        LLVMTypeRef _memmoveType;
        LLVMTypeRef _reallocType;
        private Type _lastType; // Store the type of the last expression for auto-printing
        private NodeExpr _lastNode; // Store the last expression for auto-printing
        private LLVMTypeRef _runtimeValueType;
        HashSet<string> _definedGlobals = new(); // wouldn't we use our context for this job?
        private Context _context;


        public Context GetContext() => _context;
        public void ClearContext() => _context = Context.Empty;

        private LLVMTypeRef _mallocType;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MainDelegate();
        private LLVMTypeRef _runtimeObjType;
        private LLVMValueRef _fmtFloat;

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
            InitTypes();

            // Declare malloc and other required functions
            DeclareValueStruct();
        }


        private void InitTypes()
        {
            var ctx = _module.Context;

            _runtimeObjType = LLVMTypeRef.CreateStruct(new[]
            {
                ctx.Int16Type,   // tag
                ctx.Int16Type,   // padding (alignment)
                ctx.Int32Type,   // padding (alignment)
                LLVMTypeRef.CreatePointer(ctx.Int8Type, 0) // data (generic pointer)
            }, false);
        }



        private LLVMValueRef GetOrDeclareMalloc()
        {
            var mallocFunc = _module.GetNamedFunction("malloc");
            if (mallocFunc.Handle != IntPtr.Zero)
                return mallocFunc;

            var ctx = _module.Context;

            _mallocType = LLVMTypeRef.CreateFunction(
                LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), // i8*
                new[] { ctx.Int64Type },
                false
            );

            return _module.AddFunction("malloc", _mallocType);
        }


        private LLVMValueRef BoxToI64(LLVMValueRef value)
        {
            if (value.TypeOf == _module.Context.Int64Type) return value;

            if (value.TypeOf == _module.Context.DoubleType)
                return _builder.BuildBitCast(value, _module.Context.Int64Type, "num_to_i64");

            if (value.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                return _builder.BuildPtrToInt(value, _module.Context.Int64Type, "ptr_to_i64");

            return _builder.BuildZExt(value, _module.Context.Int64Type, "zext");
        }
        private void DeclareValueStruct()
        {
            var ctx = _module.Context;
            var tagType = ctx.Int32Type; // Tag type
            var ptrType = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0); // Pointer to data

            // Define the value struct: tag + pointer to the data (value)
            _runtimeValueType = _runtimeObjType;
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
            RewriteArrayAssignments(expr);

            //_lastNode = GetLastExpression(expr);

            var programedResult = GetProgramResult(expr);

            if (programedResult == null) return new VoidType();
            if (programedResult is ExpressionNodeExpr exp) return exp.Type;

            return new VoidType();
        }

        void RewriteArrayAssignments(NodeExpr node)
        {
            switch (node)
            {
                case AssignNodeExpr assign:
                    if (assign.Expression.Type is ArrayType)
                    {
                        // if (_debug) Console.WriteLine("we cloning array"); // here and only here do we change the node
                        assign.Expression = new CloneArrayNodeExpr(assign.Expression);
                    }
                    break;

                case BinaryOpNodeExpr binOp:
                    RewriteArrayAssignments(binOp.Left);
                    RewriteArrayAssignments(binOp.Right);
                    break;

                case MapNodeExpr map:
                    RewriteArrayAssignments(map.ArrayExpr);
                    RewriteArrayAssignments(map.Assignment);
                    break;

                case SequenceNodeExpr seq:
                    foreach (var expr in seq.Statements)
                        RewriteArrayAssignments(expr);
                    break;
            }
        }

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
        // AddImplicitPrint and visitprint are very similar, can be refactored
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

        private bool IsAlreadyBoxed(object type, LLVMValueRef value)
        {
            // If LLVM type is pointer → assume boxed
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                return true;

            return false;
        }

        private void DeclarePrintfFormats()
        {
            _fmtFloat = _builder.BuildGlobalStringPtr("%f\n", "fmt_float");
        }

        public object Run(NodeExpr expr, bool debug = false)
        {
            _debug = debug;

            // 1️ Semantic analysis
            Type prediction = PerformSemanticAnalysis(expr);

            // 2️ Create a fresh main function
            CreateMain();

            // 3️ Declare printf and formats
            DeclarePrintf();
            DeclarePrintfFormats();

            // 4. Generate LLVM IR for expression
            LLVMValueRef resultValue = Visit(expr);

            if (_debug) Console.WriteLine("LLVM TYPE: " + resultValue.TypeOf);
            if (_debug) Console.WriteLine("LANG TYPE: " + prediction);


            // Robust check: Is this value already a pointer to a box?
            var structPtrType = LLVMTypeRef.CreatePointer(_runtimeObjType, 0);

            bool alreadyBoxed = (resultValue.TypeOf == structPtrType) ||
                                (prediction is ArrayType) ||
                                (prediction is StringType) ||
                                (expr is IndexNodeExpr); // Index results are always boxed

            if (alreadyBoxed)
            {
                // Return the existing RuntimeValue*
                _builder.BuildRet(resultValue);
            }
            else
            {
                // Box raw i64/double values
                var finalResult = BoxValue(resultValue, prediction);
                _builder.BuildRet(finalResult);
            }


            if (_debug) DumpIR(_module);

            // 5️ Wrap module in ThreadSafeModule
            var tsc = OrcBindings.LLVMOrcCreateNewThreadSafeContext();
            var tsm = OrcBindings.LLVMOrcCreateNewThreadSafeModule(_module.Handle, tsc);

            var dylib = OrcBindings.LLVMOrcLLJITGetMainJITDylib(_jit);
            ThrowIfError(OrcBindings.LLVMOrcLLJITAddLLVMIRModule(_jit, dylib, tsm));

            // 6️ Lookup function pointer
            ulong addr;
            ThrowIfError(OrcBindings.LLVMOrcLLJITLookup(_jit, out addr, _funcName));

            var fnPtr = (IntPtr)addr;
            var delegateResult = Marshal.GetDelegateForFunctionPointer<MainDelegate>(fnPtr);

            // 7️ Call the JIT function
            var tempResult = delegateResult();

            // 8️ Marshal the returned RuntimeValue*
            if (tempResult == IntPtr.Zero)
                return null;

            RuntimeValue result = Marshal.PtrToStructure<RuntimeValue>(tempResult);

            switch ((ValueTag)result.tag)
            {
                case ValueTag.Int:
                    return Marshal.ReadInt64(result.data);

                case ValueTag.Float:
                    // Read the double safely
                    return ReadDouble(result.data);

                case ValueTag.Bool:
                    return Marshal.ReadByte(result.data) != 0;

                case ValueTag.String:
                    return Marshal.PtrToStringAnsi(result.data);

                case ValueTag.Array:
                    return HandleArray2(result.data);

                case ValueTag.None:
                    return null;

                default:
                    throw new Exception("Unknown RuntimeValue tag");
            }
        }

        private object HandleArray2(IntPtr dataPtr)
        {
            if (dataPtr == IntPtr.Zero) return "[]";

            // Read length (8 bytes)
            long length = Marshal.ReadInt64(dataPtr);
            var elements = new List<string>((int)length);

            for (long i = 0; i < length; i++)
            {
                // 1. Calculate offset: skip Length (8) + Capacity (8) = 16 bytes.
                // 2. Read the element address as a 64-bit integer (matching your IR 'store i64')
                long elementRawAddr = Marshal.ReadInt64(dataPtr, 16 + (int)(i * 8));
                IntPtr objPtr = (IntPtr)elementRawAddr;

                if (objPtr == IntPtr.Zero)
                {
                    elements.Add("none");
                    continue;
                }

                // 3. This was the crashing line. With the correct objPtr, it will work now.
                Console.WriteLine("before PtrToStruct...");
                RuntimeValue element = Marshal.PtrToStructure<RuntimeValue>(objPtr);

                Console.WriteLine("After PtrToStruct...");
                string elStr = "none";
                switch ((ValueTag)element.tag)
                {
                    case ValueTag.Int:
                        elStr = Marshal.ReadInt64(element.data).ToString();
                        break;
                    case ValueTag.Float:
                        elStr = ReadDouble(element.data).ToString(CultureInfo.InvariantCulture);
                        break;
                    case ValueTag.Array:
                        // Support nested arrays by passing the data pointer recursively
                        elStr = HandleArray2(element.data).ToString();
                        break;
                    case ValueTag.String:
                        elStr = Marshal.PtrToStringAnsi(element.data) ?? "";
                        break;
                    case ValueTag.Bool:
                        elStr = (Marshal.ReadByte(element.data) != 0).ToString().ToLower();
                        break;
                }
                elements.Add(elStr);
            }

            return "[" + string.Join(", ", elements) + "]";
        }

        private double ReadDouble(IntPtr dataPtr)
        {
            if (dataPtr == IntPtr.Zero)
                return 0.0;

            // The double is stored in unmanaged memory
            unsafe
            {
                return *(double*)dataPtr.ToPointer();
            }
        }

        private LLVMValueRef BoxValue(LLVMValueRef value, Type type)
        {
            // 1. Get the current context and define types locally for THIS run
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i32 = ctx.Int32Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 2. Local type definitions tied to THIS context (replaces stale class fields)
            var mallocType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i64 }, false);
            // Option A: Literal/Anonymous struct (most common for temporary boxing)


            int tag = type switch
            {
                IntType => (Int16)ValueTag.Int,
                FloatType => (Int16)ValueTag.Float,
                BoolType => (Int16)ValueTag.Bool,
                StringType => (Int16)ValueTag.String,
                ArrayType => (Int16)ValueTag.Array,
                _ => (Int16)ValueTag.None
            };

            LLVMValueRef dataPtr;

            // 3. Handle data allocation and storage
            if (type is StringType)
            {
                dataPtr = value;
            }
            else if (type is VoidType)
            {
                // For 'None' (void), we don't need to malloc data, just use a null pointer
                dataPtr = LLVMValueRef.CreateConstPointerNull(i8Ptr);
            }

            else if (type is IntType)
            {
                var malloc = GetOrDeclareMalloc();
                var size = LLVMValueRef.CreateConstInt(i64, 8);
                // Use local mallocType instead of _mallocType
                var rawMem = _builder.BuildCall2(mallocType, malloc, new[] { size }, "int_mem");
                var cast = _builder.BuildBitCast(rawMem, LLVMTypeRef.CreatePointer(i64, 0), "int_cast");
                _builder.BuildStore(value, cast).SetAlignment(8);
                dataPtr = rawMem;
            }
            else if (type is FloatType)
            {
                var malloc = GetOrDeclareMalloc();
                var size = LLVMValueRef.CreateConstInt(i64, 8);
                // Use local mallocType instead of _mallocType
                var rawMem = _builder.BuildCall2(mallocType, malloc, new[] { size }, "float_mem");
                var cast = _builder.BuildBitCast(rawMem, LLVMTypeRef.CreatePointer(ctx.DoubleType, 0), "float_cast");
                _builder.BuildStore(value, cast).SetAlignment(8);
                dataPtr = rawMem;
            }
            else if (type is BoolType)
            {
                var malloc = GetOrDeclareMalloc();
                var size = LLVMValueRef.CreateConstInt(i64, 1);
                // Use local mallocType instead of _mallocType
                var rawMem = _builder.BuildCall2(mallocType, malloc, new[] { size }, "bool_mem");
                var cast = _builder.BuildBitCast(rawMem, LLVMTypeRef.CreatePointer(ctx.Int1Type, 0), "bool_cast");
                _builder.BuildStore(value, cast).SetAlignment(8);
                dataPtr = rawMem;
            }
            else if (type is ArrayType)
            {
                // Arrays are represented as an i8* pointer (with a length header at index 0).
                // We can box it the same way as strings.
                dataPtr = value;
            }
            else
                throw new Exception($"Unsupported LLVM type in BoxValue: {value.TypeOf}");


            // 4. Allocate the 'RuntimeValue' struct (using local mallocType)
            var mallocReturn = GetOrDeclareMalloc();
            var sizeReturn = LLVMValueRef.CreateConstInt(i64, 16);
            var obj = _builder.BuildCall2(mallocType, mallocReturn, new[] { sizeReturn }, "runtime_obj");

            // 5. Store tag and data using the local runtimeValueType
            var tagPtr = _builder.BuildStructGEP2(_runtimeObjType, obj, 0, "tag_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int16Type, (ulong)tag), tagPtr);

            // Data pointer is at index 3 in RuntimeObject struct (after padding fields)
            var dataFieldPtr = _builder.BuildStructGEP2(_runtimeObjType, obj, 3, "data_ptr");
            _builder.BuildStore(dataPtr, dataFieldPtr);

            return obj;
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
            var ctx = _module.Context;

            // 1️ Initialization (e.g., i = 0)
            if (expr.Initialization != null)
                Visit(expr.Initialization);

            // 2️ Create basic blocks
            var condBlock = func.AppendBasicBlock("for.cond");
            var bodyBlock = func.AppendBasicBlock("for.body");
            var stepBlock = func.AppendBasicBlock("for.step");
            var endBlock = func.AppendBasicBlock("for.end");

            // Jump to condition
            _builder.BuildBr(condBlock);

            // 3️ Condition block
            _builder.PositionAtEnd(condBlock);
            var condition = Visit(expr.Condition);

            // Convert condition to i1 if needed
            if (condition.TypeOf == ctx.DoubleType)
                condition = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE,
                    condition, LLVMValueRef.CreateConstReal(ctx.DoubleType, 0.0), "fortest_dbl");
            else if (condition.TypeOf != ctx.Int1Type)
                condition = _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE,
                    condition, LLVMValueRef.CreateConstInt(condition.TypeOf, 0), "fortest_int");

            _builder.BuildCondBr(condition, bodyBlock, endBlock);

            // 4️ Body block
            _builder.PositionAtEnd(bodyBlock);
            Visit(expr.Body);
            _builder.BuildBr(stepBlock);

            // 5️ Step block
            _builder.PositionAtEnd(stepBlock);
            if (expr.Step != null)
                Visit(expr.Step);
            _builder.BuildBr(condBlock);

            // 6️ End block
            _builder.PositionAtEnd(endBlock);

            return default; // void
        }

        public LLVMValueRef VisitForEachLoopExpr(ForEachLoopNodeExpr expr)
        {
            var ctx = _module.Context;
            var func = _builder.InsertBlock.Parent;

            // --- 1. Evaluate array object ---
            var arrayObj = Visit(expr.Array); // This is RuntimeValue*
            var arrayPtr = _builder.BuildStructGEP2(_runtimeObjType, arrayObj, 3, "array_data_ptr");
            var rawArrayPtr = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(ctx.Int64Type, 0), arrayPtr, "raw_array_ptr");

            // Load array length
            var lenPtr = _builder.BuildGEP2(ctx.Int64Type, rawArrayPtr, new[] { LLVMValueRef.CreateConstInt(ctx.Int64Type, 0) }, "len_ptr");
            var arrayLen = _builder.BuildLoad2(ctx.Int64Type, lenPtr, "array_len");

            // --- 2. Allocate iterator variable ---
            var elementType = ((ArrayType)expr.Array.Type).ElementType;
            var iteratorAlloc = _builder.BuildAlloca(GetLLVMType(elementType), expr.Iterator.Name);
            _context = _context.Add(expr.Iterator.Name, iteratorAlloc, null, elementType);

            // --- 3. Setup foreach blocks ---
            var condBlock = func.AppendBasicBlock("foreach.cond");
            var bodyBlock = func.AppendBasicBlock("foreach.body");
            var endBlock = func.AppendBasicBlock("foreach.end");

            // Counter
            var counterAlloc = _builder.BuildAlloca(ctx.Int64Type, "foreach_counter");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int64Type, 0), counterAlloc);
            _builder.BuildBr(condBlock);

            // --- 4. Condition check ---
            _builder.PositionAtEnd(condBlock);
            var curIdx = _builder.BuildLoad2(ctx.Int64Type, counterAlloc, "cur_idx");
            var cond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, curIdx, arrayLen, "foreach_test");
            _builder.BuildCondBr(cond, bodyBlock, endBlock);

            // --- 5. Loop body ---
            _builder.PositionAtEnd(bodyBlock);

            // Compute offset: data starts at index 2
            var two64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 2);
            var memIdx = _builder.BuildAdd(curIdx, two64, "mem_idx");
            var elemPtr = _builder.BuildGEP2(ctx.Int64Type, rawArrayPtr, new[] { memIdx }, "elem_ptr");
            var boxedElem = _builder.BuildLoad2(ctx.Int64Type, elemPtr, "boxed_elem");

            // --- 6. Unbox element into iterator ---
            var elemLLVMVal = UnboxValue(boxedElem, elementType); // returns i64/double/ptr depending on type
            _builder.BuildStore(elemLLVMVal, iteratorAlloc);

            // --- 7. Visit loop body ---
            Visit(expr.Body);

            // --- 8. Increment counter ---
            var one64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 1);
            var nextIdx = _builder.BuildAdd(curIdx, one64, "next_idx");
            _builder.BuildStore(nextIdx, counterAlloc);
            _builder.BuildBr(condBlock);

            // --- 9. End block ---
            _builder.PositionAtEnd(endBlock);

            return default;
        }

        private LLVMValueRef UnboxValue(LLVMValueRef boxedPtr, Type type)
        {
            var ctx = _module.Context;

            // Cast to RuntimeValue*
            var runtimeObjPtr = _builder.BuildBitCast(boxedPtr, LLVMTypeRef.CreatePointer(_runtimeObjType, 0), "unbox_obj_ptr");
            var dataPtr = _builder.BuildStructGEP2(_runtimeObjType, runtimeObjPtr, 3, "data_ptr");
            var rawDataPtr = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), dataPtr, "raw_data");

            if (type is IntType)
            {
                var intPtr = _builder.BuildBitCast(rawDataPtr, LLVMTypeRef.CreatePointer(ctx.Int64Type, 0), "int_ptr");
                return _builder.BuildLoad2(ctx.Int64Type, intPtr, "int_val");
            }
            else if (type is FloatType)
            {
                var floatPtr = _builder.BuildBitCast(rawDataPtr, LLVMTypeRef.CreatePointer(ctx.DoubleType, 0), "float_ptr");
                return _builder.BuildLoad2(ctx.DoubleType, floatPtr, "float_val");
            }
            else if (type is BoolType)
            {
                var boolPtr = _builder.BuildBitCast(rawDataPtr, LLVMTypeRef.CreatePointer(ctx.Int1Type, 0), "bool_ptr");
                return _builder.BuildLoad2(ctx.Int1Type, boolPtr, "bool_val");
            }
            else if (type is StringType)
            {
                return rawDataPtr; // i8* pointer
            }
            else
            {
                throw new Exception("Unsupported type in foreach unboxing");
            }
        }



        public LLVMValueRef VisitIncrementExpr(IncrementNodeExpr expr)
        {
            // 1. Locate the global variable
            var global = _module.GetNamedGlobal(expr.Id);

            // Fallback: if not found in current module but defined in environment
            if (global.Handle == IntPtr.Zero && _definedGlobals.Contains(expr.Id))
            {
                LLVMTypeRef llvmType = (expr.Type is FloatType) ? _module.Context.DoubleType : _module.Context.Int64Type;
                global = _module.AddGlobal(llvmType, expr.Id);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage;
            }

            if (global.Handle == IntPtr.Zero)
                throw new Exception($"Variable {expr.Id} not found.");

            // 2. Determine the correct LLVM type (Int64 or Double)
            LLVMTypeRef type = (expr.Type is FloatType) ? _module.Context.DoubleType : _module.Context.Int64Type;

            // 3. Load the current value
            var oldValue = _builder.BuildLoad2(type, global, "inc_load");

            // 4. Perform the addition based on type
            LLVMValueRef newValue;
            if (expr.Type is IntType)
            {
                newValue = _builder.BuildAdd(oldValue, LLVMValueRef.CreateConstInt(type, 1), "inc_add");
            }
            else if (expr.Type is FloatType)
            {
                newValue = _builder.BuildFAdd(oldValue, LLVMValueRef.CreateConstReal(type, 1.0), "inc_add");
            }
            else
            {
                throw new Exception("Unsupported type for increment");
            }

            // 5. Store the result back to the global
            _builder.BuildStore(newValue, global).SetAlignment(8);

            // 6. Return the newValue (LLVMValueRef)
            // We don't need to create a RuntimeValue struct here because the 
            // Visitor pattern expects the generated LLVM IR value, not a C# struct.
            return newValue;
        }


        public LLVMValueRef VisitDecrementExpr(DecrementNodeExpr expr)
        {
            LLVMValueRef global = _module.GetNamedGlobal(expr.Id);

            if (global.Handle == IntPtr.Zero && _definedGlobals.Contains(expr.Id))
            {
                LLVMTypeRef llvmType = (expr.Type is FloatType) ? _module.Context.DoubleType : _module.Context.Int64Type;
                global = _module.AddGlobal(llvmType, expr.Id);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage;
            }

            if (global.Handle == IntPtr.Zero)
                throw new Exception($"Variable {expr.Id} not found.");

            LLVMTypeRef type = (expr.Type is FloatType) ? _module.Context.DoubleType : _module.Context.Int64Type;

            var oldValue = _builder.BuildLoad2(type, global, "dec_load");

            LLVMValueRef newValue;
            if (expr.Type is IntType)
            {
                newValue = _builder.BuildSub(oldValue, LLVMValueRef.CreateConstInt(type, 1), "dec_sub");
            }
            else if (expr.Type is FloatType)
            {
                newValue = _builder.BuildFSub(oldValue, LLVMValueRef.CreateConstReal(type, 1.0), "dec_sub");
            }
            else
            {
                throw new Exception("Unsupported type for decrement");
            }

            _builder.BuildStore(newValue, global).SetAlignment(8);

            return newValue;

        }

        public LLVMValueRef VisitComparisonExpr(ComparisonNodeExpr expr)
        {
            var left = Visit(expr.Left);
            var right = Visit(expr.Right);

            // Determine the types of the operands
            bool lhsIsInt = left.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && left.TypeOf.IntWidth > 1;
            bool rhsIsInt = right.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && right.TypeOf.IntWidth > 1;
            bool lhsIsBool = left.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && left.TypeOf.IntWidth == 1;
            bool rhsIsBool = right.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && right.TypeOf.IntWidth == 1;

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
                    var and = _builder.BuildAnd(left, right, "and_tmp");
                    return and;
                }

                // Handle logical OR (||)
                if (expr.Operator == "||")
                {
                    var or = _builder.BuildOr(left, right, "or_tmp");
                    return or;
                }
            }

            // Case 2: both integers → ICmp
            if (lhsIsInt && rhsIsInt)
            {
                return expr.Operator switch
                {
                    "<" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, left, right, "icmp_tmp"),
                    ">" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, left, right, "icmp_tmp"),
                    "<=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, left, right, "icmp_tmp"),
                    ">=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, left, right, "icmp_tmp"),
                    "==" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, left, right, "icmp_tmp"),
                    "!=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, left, right, "icmp_tmp"),
                    _ => throw new Exception("Unknown operator")
                };
            }

            // Case 3: at least one is float/double → promote integers (not booleans) to double
            if (lhsIsInt)
                left = _builder.BuildSIToFP(left, _module.Context.DoubleType, "int2double");
            if (rhsIsInt)
                right = _builder.BuildSIToFP(right, _module.Context.DoubleType, "int2double");

            // Case 4: boolean comparison? Only allow equality/inequality
            if (lhsIsBool && rhsIsBool)
            {
                return expr.Operator switch
                {
                    "==" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, left, right, "bool_eq"),
                    "!=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, left, right, "bool_ne"),
                    _ => throw new Exception("Cannot compare booleans with < > <= >=")
                };
            }

            // Case 5: both are doubles → FCmp
            return expr.Operator switch
            {
                "<" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, left, right, "fcmp_tmp"),
                ">" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, left, right, "fcmp_tmp"),
                "<=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLE, left, right, "fcmp_tmp"),
                ">=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGE, left, right, "fcmp_tmp"),
                "==" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, left, right, "fcmp_tmp"),
                "!=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, left, right, "fcmp_tmp"),
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

            // 1. Define Function Signatures
            var powSig = LLVMTypeRef.CreateFunction(doubleType, new[] { doubleType, doubleType });
            var roundSig = LLVMTypeRef.CreateFunction(doubleType, new[] { doubleType });

            // 2. Declare/Get Functions
            var powFunc = _module.GetNamedFunction("pow");
            if (powFunc.Handle == IntPtr.Zero)
            {
                powFunc = _module.AddFunction("pow", powSig);
            }

            var roundFunc = _module.GetNamedFunction("round");
            if (roundFunc.Handle == IntPtr.Zero)
            {
                roundFunc = _module.AddFunction("round", roundSig);
            }

            // 3. Build the Logic
            var ten = LLVMValueRef.CreateConstReal(doubleType, 10.0);

            // Note: First arg to BuildCall2 must be the Signature (powSig/roundSig)
            var multiplier = _builder.BuildCall2(powSig, powFunc, new[] { ten, decimals }, "multiplier");

            var temp = _builder.BuildFMul(val, multiplier, "temp");
            var roundedTemp = _builder.BuildCall2(roundSig, roundFunc, new[] { temp }, "roundedTemp");
            var finalVal = _builder.BuildFDiv(roundedTemp, multiplier, "rounded_final");

            // 4. RETURN the LLVMValueRef (the instruction result)
            return finalVal;
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


        public LLVMValueRef VisitRandomExpr(RandomNodeExpr expr)
        {
            var llvmCtx = _module.Context;
            var i64 = llvmCtx.Int64Type;

            // 1. Declare rand() if missing
            var randFunc = _module.GetNamedFunction("rand");
            if (randFunc.Handle == IntPtr.Zero)
            {
                // Note: C's rand() actually returns i32, but i64 is fine if your env handles it
                var randType = LLVMTypeRef.CreateFunction(i64, Array.Empty<LLVMTypeRef>(), false);
                randFunc = _module.AddFunction("rand", randType);
            }

            // 2. Call rand()
            var randCallType = LLVMTypeRef.CreateFunction(i64, Array.Empty<LLVMTypeRef>(), false);
            var randValue = _builder.BuildCall2(randCallType, randFunc, Array.Empty<LLVMValueRef>(), "randcall");

            // 3. Optional min/max logic
            if (expr.MinValue != null && expr.MaxValue != null)
            {
                var minVal = Visit(expr.MinValue);
                var maxVal = Visit(expr.MaxValue);

                // Ensure types match for arithmetic
                if (minVal.TypeOf == llvmCtx.DoubleType) minVal = _builder.BuildFPToSI(minVal, i64, "min_i");
                if (maxVal.TypeOf == llvmCtx.DoubleType) maxVal = _builder.BuildFPToSI(maxVal, i64, "max_i");

                // Simple clamp: res = min + (rand % (max - min + 1))
                var diff = _builder.BuildSub(maxVal, minVal, "diff");
                var range = _builder.BuildAdd(diff, LLVMValueRef.CreateConstInt(i64, 1), "range");
                var mod = _builder.BuildSRem(randValue, range, "mod");
                randValue = _builder.BuildAdd(mod, minVal, "rand_result");
            }

            // 4. RETURN THE LLVMValueRef
            // Do not wrap it in RuntimeValue here.
            return randValue;
        }

        private LLVMValueRef AddImplicitPrint(LLVMValueRef valueToPrint, Type type)
        {
            var llvmCtx = _module.Context;

            LLVMValueRef finalArg;
            LLVMValueRef formatStr;

            // 1️ Direct primitive types
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
            // 2️ Pointer types (RuntimeValue* or array)
            else if (valueToPrint.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind &&
                     (valueToPrint.TypeOf.ElementType.Equals(_runtimeObjType) || type is ArrayType))
            {
                // Cast to RuntimeValue*
                var runtimeObjPtr = _builder.BuildBitCast(
                    valueToPrint,
                    LLVMTypeRef.CreatePointer(_runtimeObjType, 0),
                    "unbox_obj_ptr"
                );

                var dataPtr = _builder.BuildStructGEP2(
                    _runtimeObjType,
                    runtimeObjPtr,
                    3,
                    "unbox_data_ptr"
                );

                var rawDataPtr = _builder.BuildLoad2(
                    LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0),
                    dataPtr,
                    "unbox_data"
                );

                // --- Unbox based on type ---
                if (type is IntType)
                {
                    var intPtr = _builder.BuildBitCast(
                        rawDataPtr,
                        LLVMTypeRef.CreatePointer(llvmCtx.Int64Type, 0),
                        "unboxed_int_ptr"
                    );
                    finalArg = _builder.BuildLoad2(llvmCtx.Int64Type, intPtr, "unboxed_int");
                    formatStr = _builder.BuildGlobalStringPtr("%ld\n", "fmt_int");
                }
                else if (type is FloatType)
                {
                    var floatPtr = _builder.BuildBitCast(
                        rawDataPtr,
                        LLVMTypeRef.CreatePointer(llvmCtx.DoubleType, 0),
                        "unboxed_float_ptr"
                    );
                    finalArg = _builder.BuildLoad2(llvmCtx.DoubleType, floatPtr, "unboxed_float");
                    formatStr = _builder.BuildGlobalStringPtr("%f\n", "fmt_float");
                }
                else if (type is BoolType)
                {
                    var boolPtr = _builder.BuildBitCast(
                        rawDataPtr,
                        LLVMTypeRef.CreatePointer(llvmCtx.Int1Type, 0),
                        "unboxed_bool_ptr"
                    );
                    var boolVal = _builder.BuildLoad2(llvmCtx.Int1Type, boolPtr, "unboxed_bool");

                    DeclareBoolStrings();
                    var selectedStr = _builder.BuildSelect(boolVal, _trueStr, _falseStr, "boolstr");
                    return _builder.BuildCall2(_printfType, _printf, new[] { selectedStr }, "print_bool");
                }
                else if (type is StringType)
                {
                    finalArg = rawDataPtr;
                    formatStr = _builder.BuildGlobalStringPtr("%s\n", "fmt_str");
                }
                else if (type is ArrayType)
                {
                    // rawDataPtr is i8* to array; cast to i64* to read length
                    var arrPtr = _builder.BuildBitCast(
                        rawDataPtr,
                        LLVMTypeRef.CreatePointer(llvmCtx.Int64Type, 0),
                        "arr_len_ptr"
                    );
                    var arrLen = _builder.BuildLoad2(llvmCtx.Int64Type, arrPtr, "arr_len");
                    finalArg = arrLen;
                    formatStr = _builder.BuildGlobalStringPtr("Array(len=%ld)\n", "fmt_array");
                }
                else
                {
                    finalArg = rawDataPtr;
                    formatStr = _builder.BuildGlobalStringPtr("unknown type\n", "fmt_unknown");
                }
            }
            else
            {

                // Fallback for primitives when LLVM type is mismatched
                switch (type)
                {
                    case IntType:
                        finalArg = valueToPrint; // This is the i64 (10)
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
                            boolCond = valueToPrint;
                        else if (valueToPrint.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                            boolCond = _builder.BuildICmp(
                                LLVMIntPredicate.LLVMIntNE,
                                valueToPrint,
                                LLVMValueRef.CreateConstInt(valueToPrint.TypeOf, 0),
                                "boolcond"
                            );
                        else if (valueToPrint.TypeOf == llvmCtx.DoubleType)
                            boolCond = _builder.BuildFCmp(
                                LLVMRealPredicate.LLVMRealONE,
                                valueToPrint,
                                LLVMValueRef.CreateConstReal(llvmCtx.DoubleType, 0.0),
                                "boolcond"
                            );
                        else
                            throw new Exception("Unsupported bool representation");

                        var selectedStr = _builder.BuildSelect(boolCond, _trueStr, _falseStr, "boolstr");
                        return _builder.BuildCall2(_printfType, _printf, new[] { selectedStr }, "print_bool");

                    case StringType:
                        finalArg = valueToPrint;
                        formatStr = _builder.BuildGlobalStringPtr("%s\n", "fmt_str");
                        break;

                    case ArrayType:
                        var arrPtr = _builder.BuildBitCast(valueToPrint, LLVMTypeRef.CreatePointer(llvmCtx.Int64Type, 0), "arr_len_ptr");
                        var arrLen = _builder.BuildLoad2(llvmCtx.Int64Type, arrPtr, "arr_len");
                        arrLen.SetAlignment(8);
                        finalArg = arrLen;
                        formatStr = _builder.BuildGlobalStringPtr("Array(len=%ld)\n", "fmt_array");
                        break;

                    default:
                        throw new Exception($"Unsupported type for printing: {type}");
                }
            }

            // Final printf call
            return _builder.BuildCall2(_printfType, _printf, new[] { formatStr, finalArg }, "printf_call");
        }

        public LLVMValueRef VisitPrintExpr(PrintNodeExpr expr)
        {
            var val = Visit(expr.Expression);

            // This does the actual printing to the console
            return AddImplicitPrint(val, expr.Expression.Type);

            // Returning 'val' here means the REPL will display the value
            // that was just printed as the "Result".
            //return val;
        }

        public LLVMValueRef VisitWhereExpr(WhereNodeExpr expr)
        {
            // 1 Allocate local variables for source and result
            var srcVarName = "__where_src";
            var resultVarName = "__where_result";
            var indexVarName = "__where_i";

            // Save source array into a temp variable
            var srcAssign = new AssignNodeExpr(srcVarName, new IdNodeExpr(expr.ArrayExpr is IdNodeExpr id ? id.Name : "__tmp_array"));

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
            var srcAssign = new AssignNodeExpr(srcVarName, new CloneArrayNodeExpr(expr.ArrayExpr));

            // 2 Allocate new array for result (length = src.length)
            var resultAssign = new AssignNodeExpr(resultVarName, new CloneArrayNodeExpr(new IdNodeExpr(srcVarName)));

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

        public LLVMValueRef VisitCloneArrayExpr(CloneArrayNodeExpr expr)
        {
            var srcArray = Visit(expr.SourceArray); // Evaluate the array
            var lengthPtr = _builder.BuildGEP2(_module.Context.Int64Type, srcArray, new[] { LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 0) }, "len_ptr");
            var length = _builder.BuildLoad2(_module.Context.Int64Type, lengthPtr, "length");

            // Allocate new array with length + 2 slots
            var elementSize = LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 8);
            var totalSlots = _builder.BuildAdd(length, LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 2));
            var totalBytes = _builder.BuildMul(totalSlots, elementSize);
            var mallocFunc = GetOrDeclareMalloc();
            var newArray = _builder.BuildCall2(_mallocType, mallocFunc, new[] { totalBytes }, "arr_ptr");

            // Store length and capacity
            var lenPtrNew = _builder.BuildGEP2(_module.Context.Int64Type, newArray, new[] { LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 0) }, "len_ptr");
            var capPtrNew = _builder.BuildGEP2(_module.Context.Int64Type, newArray, new[] { LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 1) }, "cap_ptr");
            _builder.BuildStore(length, lenPtrNew).SetAlignment(8);
            _builder.BuildStore(length, capPtrNew).SetAlignment(8);

            // Loop to copy elements
            var index = _builder.BuildAlloca(_module.Context.Int64Type, "i");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 0), index);

            var func = _builder.InsertBlock.Parent;
            var loopCond = func.AppendBasicBlock("loop.cond");
            var loopBody = func.AppendBasicBlock("loop.body");
            var loopEnd = func.AppendBasicBlock("loop.end");

            _builder.BuildBr(loopCond);
            _builder.PositionAtEnd(loopCond);

            var iVal = _builder.BuildLoad2(_module.Context.Int64Type, index, "i_val");
            var cmp = _builder.BuildICmp(LLVMSharp.Interop.LLVMIntPredicate.LLVMIntSLT, iVal, length, "cmp");
            _builder.BuildCondBr(cmp, loopBody, loopEnd);

            _builder.PositionAtEnd(loopBody);

            // Copy element (assume ptr/boxed value)
            var offset = _builder.BuildAdd(iVal, LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 2));
            var srcElemPtr = _builder.BuildGEP2(_module.Context.Int64Type, srcArray, new[] { offset }, "src_elem");
            var val = _builder.BuildLoad2(_module.Context.Int64Type, srcElemPtr, "val");

            var dstElemPtr = _builder.BuildGEP2(_module.Context.Int64Type, newArray, new[] { offset }, "dst_elem");
            _builder.BuildStore(val, dstElemPtr);

            var iNext = _builder.BuildAdd(iVal, LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 1));
            _builder.BuildStore(iNext, index);
            _builder.BuildBr(loopCond);

            _builder.PositionAtEnd(loopEnd);
            return newArray;
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

        // Helper: recursively replace iterator variable with current element
        private ExpressionNodeExpr ReplaceIterator(ExpressionNodeExpr expr, string iteratorName, ExpressionNodeExpr replacement)
        {
            switch (expr)
            {
                case IdNodeExpr id when id.Name == iteratorName:
                    return replacement;

                case ComparisonNodeExpr cmp:
                    return new ComparisonNodeExpr(
                        ReplaceIterator(cmp.Left, iteratorName, replacement),
                        cmp.Operator,
                        ReplaceIterator(cmp.Right, iteratorName, replacement)
                    );

                case BinaryOpNodeExpr bin:
                    return new BinaryOpNodeExpr(
                        ReplaceIterator(bin.Left, iteratorName, replacement),
                        bin.Operator,
                        ReplaceIterator(bin.Right, iteratorName, replacement)
                    );

                case UnaryOpNodeExpr un:
                    return new UnaryOpNodeExpr(
                        un.Operator,
                        ReplaceIterator(un.Operand, iteratorName, replacement)
                    );

                case IndexNodeExpr idx:
                    return new IndexNodeExpr(
                        ReplaceIterator(idx.ArrayExpression, iteratorName, replacement),
                        ReplaceIterator(idx.IndexExpression, iteratorName, replacement)
                    );


                default:
                    return expr;
            }
        }

        public LLVMValueRef VisitArrayExpr(ArrayNodeExpr expr)
        {
            var ctx = _module.Context;
            uint count = (uint)expr.Elements.Count;

            var mallocFunc = GetOrDeclareMalloc();

            // Allocate array slots: length + capacity + elements
            var slots = count + 2;
            var totalBytes = LLVMValueRef.CreateConstInt(ctx.Int64Type, (ulong)slots * 8);
            var arrayPtr = _builder.BuildCall2(_mallocType, mallocFunc, new[] { totalBytes }, "arr_ptr");

            // Store length
            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int64Type, count),
                _builder.BuildGEP2(ctx.Int64Type, arrayPtr,
                    new[] { LLVMValueRef.CreateConstInt(ctx.Int64Type, 0) }, "len_ptr"))
                .SetAlignment(8);

            // Store capacity
            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int64Type, count),
                _builder.BuildGEP2(ctx.Int64Type, arrayPtr,
                    new[] { LLVMValueRef.CreateConstInt(ctx.Int64Type, 1) }, "cap_ptr"))
                .SetAlignment(8);

            var elementType = expr.ElementType ?? new IntType();

            for (int i = 0; i < expr.Elements.Count; i++)
            {
                var val = Visit(expr.Elements[i]);
                LLVMValueRef boxed;

                // Box values **always via malloc + RuntimeObject**
                boxed = elementType switch
                {
                    IntType => BoxInt(val),
                    FloatType => BoxFloat(val),
                    BoolType => BoxBool(val),
                    StringType => BoxString(val),
                    _ => throw new Exception($"Unsupported array element type: {elementType}")
                };

                // Store boxed element in array (starting at index 2)
                var boxedIntValue = _builder.BuildPtrToInt(boxed, ctx.Int64Type, "boxed_to_i64");
                _builder.BuildStore(boxedIntValue,
                    _builder.BuildGEP2(ctx.Int64Type, arrayPtr,
                        new[] { LLVMValueRef.CreateConstInt(ctx.Int32Type, (ulong)(i + 2)) },
                        $"idx_{i}"))
                    .SetAlignment(8);
            }
      
            expr.SetType(new ArrayType(expr.ElementType));
            if (_debug) Console.WriteLine("array pointer: " + arrayPtr);

            return BoxArray(arrayPtr);
        }

        // --------------------- Helpers ---------------------

        private LLVMValueRef BoxArray(LLVMValueRef arrPtr)
        {
            var ctx = _module.Context;
            var mallocFunc = GetOrDeclareMalloc();

            var objRaw = _builder.BuildCall2(_mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(ctx.Int64Type, 16) }, "malloc_obj");

            var obj = _builder.BuildBitCast(objRaw,
                LLVMTypeRef.CreatePointer(_runtimeObjType, 0), "obj");

            // tag = Array
            _builder.BuildStore(
                LLVMValueRef.CreateConstInt(ctx.Int16Type, (ulong)ValueTag.Array),
                _builder.BuildGEP2(_runtimeObjType, obj,
                    new[] { LLVMValueRef.CreateConstInt(ctx.Int32Type, 0),
                    LLVMValueRef.CreateConstInt(ctx.Int32Type, 0) },
                    "tag_ptr"));

            // data = pointer to array
            _builder.BuildStore(arrPtr,
                _builder.BuildGEP2(_runtimeObjType, obj,
                    new[] { LLVMValueRef.CreateConstInt(ctx.Int32Type, 0),
                    LLVMValueRef.CreateConstInt(ctx.Int32Type, 3) },
                    "data_ptr"));

            return obj;
        }

        // Box primitives: always malloc for data, store pointer in RuntimeObject
        private LLVMValueRef BoxFloat(LLVMValueRef value)
        {
            var ctx = _module.Context;
            var mallocFunc = GetOrDeclareMalloc();

            LLVMValueRef floatValue = value;
            if (value.TypeOf == ctx.Int64Type || value.TypeOf == ctx.Int32Type)
            {
                floatValue = _builder.BuildSIToFP(value, ctx.DoubleType, "int_to_double");
            }
            else if (value.TypeOf == ctx.Int1Type)
            {
                var intVal = _builder.BuildZExt(value, ctx.Int64Type, "bool_to_i64");
                floatValue = _builder.BuildSIToFP(intVal, ctx.DoubleType, "bool_to_double");
            }
            else if (value.TypeOf != ctx.DoubleType)
            {
                throw new Exception($"Cannot box value as float; unsupported type {value.TypeOf}");
            }

            // Allocate 8 bytes for double
            var doubleMem = _builder.BuildCall2(_mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(ctx.Int64Type, 8) }, "double_mem");
            _builder.BuildStore(floatValue, _builder.BuildBitCast(doubleMem,
                LLVMTypeRef.CreatePointer(ctx.DoubleType, 0), "cast_ptr"));

            // Allocate RuntimeObject
            var objRaw = _builder.BuildCall2(_mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(ctx.Int64Type, 16) }, "float_obj");
            var obj = _builder.BuildBitCast(objRaw, LLVMTypeRef.CreatePointer(_runtimeObjType, 0), "typed_obj");

            // Tag
            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int16Type, (ulong)ValueTag.Float),
                _builder.BuildGEP2(_runtimeObjType, obj,
                    new[] { LLVMValueRef.CreateConstInt(ctx.Int32Type, 0),
                    LLVMValueRef.CreateConstInt(ctx.Int32Type, 0) },
                    "tag_ptr"));

            // Data = pointer to double
            _builder.BuildStore(doubleMem,
                _builder.BuildGEP2(_runtimeObjType, obj,
                    new[] { LLVMValueRef.CreateConstInt(ctx.Int32Type, 0),
                    LLVMValueRef.CreateConstInt(ctx.Int32Type, 3) },
                    "data_ptr"));

            return obj;
        }

        private LLVMValueRef BoxBool(LLVMValueRef value)
        {
            var ctx = _module.Context;
            var mallocFunc = GetOrDeclareMalloc();

            var boolMem = _builder.BuildCall2(_mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(ctx.Int64Type, 1) }, "bool_mem");
            _builder.BuildStore(value, _builder.BuildBitCast(boolMem,
                LLVMTypeRef.CreatePointer(ctx.Int1Type, 0), "cast_ptr"));

            var objRaw = _builder.BuildCall2(_mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(ctx.Int64Type, 16) }, "bool_obj");
            var obj = _builder.BuildBitCast(objRaw, LLVMTypeRef.CreatePointer(_runtimeObjType, 0), "typed_obj");

            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int16Type, (ulong)ValueTag.Bool),
                _builder.BuildGEP2(_runtimeObjType, obj,
                    new[] { LLVMValueRef.CreateConstInt(ctx.Int32Type, 0),
                    LLVMValueRef.CreateConstInt(ctx.Int32Type, 0) },
                    "tag_ptr"));

            _builder.BuildStore(boolMem,
                _builder.BuildGEP2(_runtimeObjType, obj,
                    new[] { LLVMValueRef.CreateConstInt(ctx.Int32Type, 0),
                    LLVMValueRef.CreateConstInt(ctx.Int32Type, 3) },
                    "data_ptr"));

            return obj;
        }

        private LLVMValueRef BoxString(LLVMValueRef value)
        {
            var ctx = _module.Context;
            var mallocFunc = GetOrDeclareMalloc();

            // Allocate RuntimeObject
            var objRaw = _builder.BuildCall2(_mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(ctx.Int64Type, 16) }, "str_obj");
            var obj = _builder.BuildBitCast(objRaw, LLVMTypeRef.CreatePointer(_runtimeObjType, 0), "typed_obj");

            // Tag = String
            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int16Type, (ulong)ValueTag.String),
                _builder.BuildGEP2(_runtimeObjType, obj,
                    new[] { LLVMValueRef.CreateConstInt(ctx.Int32Type, 0),
                    LLVMValueRef.CreateConstInt(ctx.Int32Type, 0) },
                    "tag_ptr"));

            // Data = pointer to i8*
            _builder.BuildStore(value,
                _builder.BuildGEP2(_runtimeObjType, obj,
                    new[] { LLVMValueRef.CreateConstInt(ctx.Int32Type, 0),
                    LLVMValueRef.CreateConstInt(ctx.Int32Type, 3) },
                    "data_ptr"));

            return obj;
        }

        // New helper for Ints
        private LLVMValueRef BoxInt(LLVMValueRef value)
        {
            var ctx = _module.Context;
            var mallocFunc = GetOrDeclareMalloc();

            var intMem = _builder.BuildCall2(_mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(ctx.Int64Type, 8) }, "int_mem");
            _builder.BuildStore(value, _builder.BuildBitCast(intMem,
                LLVMTypeRef.CreatePointer(ctx.Int64Type, 0), "cast_ptr"));

            var objRaw = _builder.BuildCall2(_mallocType, mallocFunc,
                new[] { LLVMValueRef.CreateConstInt(ctx.Int64Type, 16) }, "int_obj");
            var obj = _builder.BuildBitCast(objRaw, LLVMTypeRef.CreatePointer(_runtimeObjType, 0), "typed_obj");

            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int16Type, (ulong)ValueTag.Int),
                _builder.BuildGEP2(_runtimeObjType, obj,
                    new[] { LLVMValueRef.CreateConstInt(ctx.Int32Type, 0),
                    LLVMValueRef.CreateConstInt(ctx.Int32Type, 0) },
                    "tag_ptr"));

            _builder.BuildStore(intMem,
                _builder.BuildGEP2(_runtimeObjType, obj,
                    new[] { LLVMValueRef.CreateConstInt(ctx.Int32Type, 0),
                    LLVMValueRef.CreateConstInt(ctx.Int32Type, 3) },
                    "data_ptr"));

            return obj;
        }


        public LLVMValueRef VisitIndexExpr(IndexNodeExpr expr)
        {
            var ctx = _module.Context;
            var func = _builder.InsertBlock.Parent;

            // 1. Visit Array and Index expressions
            var arrayValue = Visit(expr.ArrayExpression);
            var indexVal = Visit(expr.IndexExpression);

            if (indexVal.TypeOf == ctx.DoubleType)
                indexVal = _builder.BuildFPToSI(indexVal, ctx.Int64Type, "idx_int");

            // 1.a Unbox array pointer if array is boxed
            LLVMValueRef arrayPtr;
            bool isBoxedArray = arrayValue.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind &&
                                (arrayValue.TypeOf.ElementType.Equals(_runtimeObjType) || expr.ArrayExpression.Type is ArrayType);

            if (isBoxedArray)
            {
                var arrayObjPtr = _builder.BuildBitCast(arrayValue, LLVMTypeRef.CreatePointer(_runtimeObjType, 0), "array_obj_ptr");
                var dataPtr = _builder.BuildStructGEP2(_runtimeObjType, arrayObjPtr, 3, "array_data_ptr");
                var rawArrayPtr = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), dataPtr, "raw_array_ptr");
                arrayPtr = _builder.BuildBitCast(rawArrayPtr, LLVMTypeRef.CreatePointer(ctx.Int64Type, 0), "array_ptr");
            }
            else
            {
                arrayPtr = _builder.BuildBitCast(arrayValue, LLVMTypeRef.CreatePointer(ctx.Int64Type, 0), "array_ptr");
            }

            // 2. Bounds check
            var zero = LLVMValueRef.CreateConstInt(ctx.Int64Type, 0);
            var lenPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { zero }, "len_ptr");
            var arrayLen = _builder.BuildLoad2(ctx.Int64Type, lenPtr, "array_len");

            var isNeg = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, indexVal, zero, "is_neg");
            var isTooBig = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, indexVal, arrayLen, "is_too_big");
            var isInvalid = _builder.BuildOr(isNeg, isTooBig, "is_invalid");

            var failBlock = func.AppendBasicBlock("bounds.fail");
            var safeBlock = func.AppendBasicBlock("bounds.ok");
            _builder.BuildCondBr(isInvalid, failBlock, safeBlock);

            // --- FAIL BLOCK ---
            _builder.PositionAtEnd(failBlock);
            var errorMsg = _builder.BuildGlobalStringPtr("Runtime Error: Index Out of Bounds!\n", "err_msg");
            _builder.BuildCall2(_printfType, _printf, new[] { errorMsg });
            _builder.BuildRet(LLVMValueRef.CreateConstNull(LLVMTypeRef.CreatePointer(_runtimeObjType, 0)));

            // --- SAFE BLOCK ---
            _builder.PositionAtEnd(safeBlock);

            // 3. Compute memory index (skip 2 header slots: Length and Capacity)
            var offsetIdx = _builder.BuildAdd(indexVal, LLVMValueRef.CreateConstInt(ctx.Int64Type, 2), "offset_idx");

            // 4. Load the address from the array (which is an i64 representation of a pointer)
            var elementPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { offsetIdx }, "elem_ptr");
            var elementInt = _builder.BuildLoad2(ctx.Int64Type, elementPtr, "elem_i64");

            // 5. Convert that i64 back to a RuntimeValue* pointer and return it
            // DO NOT malloc a new box here!
            var runtimeObjPtr = _builder.BuildIntToPtr(elementInt, LLVMTypeRef.CreatePointer(_runtimeObjType, 0), "runtime_obj_ptr");

            return runtimeObjPtr;

        }


        private LLVMTypeRef GetLLVMType(Type type)
        {
            var ctx = _module.Context;
            return type switch
            {
                FloatType => ctx.DoubleType,
                IntType => ctx.Int64Type,
                StringType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0),
                ArrayType => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), // Arrays are pointers
                BoolType => ctx.Int1Type,
                _ => throw new Exception($"Unsupported type: {type}")
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

            // 1. Evaluate array and the value to add
            var arrayValue = Visit(expr.ArrayExpression);
            var arrayName = ((IdNodeExpr)expr.ArrayExpression).Name;
            var value = Visit(expr.AddExpression);


            // Always box the element (integers, floats, etc.)
            LLVMValueRef boxed = BoxValue(value, expr.AddExpression.Type);

            // 2. Extract raw array pointer if the array is boxed RuntimeObject
            LLVMValueRef arrayObjPtr = default;
            LLVMValueRef arrayPtr;

            bool isBoxedArray = arrayValue.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind &&
                                (arrayValue.TypeOf.ElementType.Equals(_runtimeObjType) || expr.ArrayExpression.Type is ArrayType);

            if (isBoxedArray)
            {
                // Unbox RuntimeObject to get the raw array pointer
                arrayObjPtr = _builder.BuildBitCast(arrayValue,
                    LLVMTypeRef.CreatePointer(_runtimeObjType, 0), "array_obj_ptr");

                var dataPtr = _builder.BuildStructGEP2(_runtimeObjType, arrayObjPtr, 3, "array_data_ptr");
                var rawArrayPtr = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), dataPtr, "raw_array_ptr");
                arrayPtr = _builder.BuildBitCast(rawArrayPtr, LLVMTypeRef.CreatePointer(ctx.Int64Type, 0), "array_ptr");
            }
            else
            {
                arrayPtr = _builder.BuildBitCast(arrayValue, LLVMTypeRef.CreatePointer(ctx.Int64Type, 0), "array_ptr");
            }

            // 3. Prepare constants
            var zero64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 0);
     
            var one64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 1);
            var two64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 2);
            var eight64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 8);

            // Save entry block for PHI
            var entryBlock = _builder.InsertBlock;

            // 4. Load length and capacity
            var lenPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { zero64 }, "len_ptr");
            var length = _builder.BuildLoad2(ctx.Int64Type, lenPtr, "length");
            length.SetAlignment(8);

            var capPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { one64 }, "cap_ptr");
            var capacity = _builder.BuildLoad2(ctx.Int64Type, capPtr, "capacity");
            capacity.SetAlignment(8);

            // 5. Check if we need to grow
            var isFull = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, length, capacity, "is_full");
            var func = _builder.InsertBlock.Parent;
            var growBlock = ctx.AppendBasicBlock(func, "grow");
            var contBlock = ctx.AppendBasicBlock(func, "cont");
            _builder.BuildCondBr(isFull, growBlock, contBlock);

            // ---- Grow block ----
            _builder.PositionAtEnd(growBlock);
            var newCap = _builder.BuildMul(capacity, two64, "new_capacity");
            var minCap = LLVMValueRef.CreateConstInt(ctx.Int64Type, 4);
            newCap = _builder.BuildSelect(
                _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, capacity, zero64),
                minCap,
                newCap,
                "new_capacity"
            );
            var totalSlots = _builder.BuildAdd(newCap, two64, "slots"); // header
            var totalBytes = _builder.BuildMul(totalSlots, eight64, "total_bytes");

            var reallocFunc = GetOrDeclareRealloc();
            var newArrayPtr = _builder.BuildCall2(_reallocType, reallocFunc, new[] { arrayPtr, totalBytes }, "realloc_array");

            // Store new capacity
            var capPtr2 = _builder.BuildGEP2(ctx.Int64Type, newArrayPtr, new[] { one64 }, "cap_ptr2");
            _builder.BuildStore(newCap, capPtr2).SetAlignment(8);

            _builder.BuildBr(contBlock);

            // ---- Continue block ----
            _builder.PositionAtEnd(contBlock);

            // PHI node for array pointer
            var arrayPtrPhi = _builder.BuildPhi(LLVMTypeRef.CreatePointer(ctx.Int64Type, 0), "array_ptr_phi");
            arrayPtrPhi.AddIncoming(
                new[] { arrayPtr, newArrayPtr },
                new[] { entryBlock, growBlock },
                2
            );

            // Reload length after possible growth
            var lenPtr2 = _builder.BuildGEP2(ctx.Int64Type, arrayPtrPhi, new[] { zero64 }, "len_ptr2");
            var length2 = _builder.BuildLoad2(ctx.Int64Type, lenPtr2, "length2");
            length2.SetAlignment(8);

            // 6. Store the boxed element (always cast pointer to i64)
            var elemIndex = _builder.BuildAdd(length2, two64, "elem_index"); // skip 2 header slots
            var elemPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtrPhi, new[] { elemIndex }, "elem_ptr");
            var boxedIntValue = _builder.BuildPtrToInt(boxed, ctx.Int64Type, "boxed_to_i64");
            _builder.BuildStore(boxedIntValue, elemPtr).SetAlignment(8);

             // 7. Increment length
            var newLength = _builder.BuildAdd(length2, one64, "new_length");
   
            _builder.BuildStore(newLength, lenPtr2).SetAlignment(8);

            // 8. Update boxed array RuntimeObject if needed
            if (arrayObjPtr.Handle != IntPtr.Zero)
            {
                var dataPtr = _builder.BuildStructGEP2(_runtimeObjType, arrayObjPtr, 3, "array_data_ptr");
                var castArrayPtr = _builder.BuildBitCast(arrayPtrPhi, LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), "cast_array_ptr");
                _builder.BuildStore(castArrayPtr, dataPtr);
            }

            // 9. Update global variable if this is a named array
            if (expr.ArrayExpression is IdNodeExpr id)
            {
                var global = _module.GetNamedGlobal(id.Name);
                if (global.Handle != IntPtr.Zero)
                {
                    if (arrayObjPtr.Handle != IntPtr.Zero)
                    {
                        _builder.BuildStore(arrayObjPtr, global);
                    }
                    else
                    {
                        var boxedArray = BoxArray(arrayPtrPhi);
                        _builder.BuildStore(boxedArray, global);
                        arrayObjPtr = boxedArray;
                    }
                }
            }

            // 10. Return boxed array object
            if (arrayObjPtr.Handle == IntPtr.Zero)
                arrayObjPtr = BoxArray(arrayPtrPhi);

            return arrayObjPtr;
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

            // --- 1. Evaluate Array Pointer ---
            var arrayValue = Visit(expr.ArrayExpression);

            LLVMValueRef arrayPtr;
            LLVMValueRef arrayObjPtr = default;

            bool isBoxedArray = arrayValue.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind &&
                                (arrayValue.TypeOf.ElementType.Equals(_runtimeObjType) || expr.ArrayExpression.Type is ArrayType);

            if (isBoxedArray)
            {
                arrayObjPtr = _builder.BuildBitCast(arrayValue, LLVMTypeRef.CreatePointer(_runtimeObjType, 0), "array_obj_ptr");
                var dataPtr = _builder.BuildStructGEP2(_runtimeObjType, arrayObjPtr, 3, "array_data_ptr");
                var rawArrayPtr = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), dataPtr, "raw_array_ptr");
                arrayPtr = _builder.BuildBitCast(rawArrayPtr, LLVMTypeRef.CreatePointer(ctx.Int64Type, 0), "array_ptr");
            }
            else
            {
                arrayPtr = _builder.BuildBitCast(arrayValue, LLVMTypeRef.CreatePointer(ctx.Int64Type, 0), "array_ptr");
            }

            var indexVal = Visit(expr.RemoveExpression);
            if (indexVal.TypeOf == ctx.DoubleType)
                indexVal = _builder.BuildFPToSI(indexVal, i64, "idx_int");

            // Constants
            var zero64 = LLVMValueRef.CreateConstInt(i64, 0);
            var one64 = LLVMValueRef.CreateConstInt(i64, 1);
            var two64 = LLVMValueRef.CreateConstInt(i64, 2);
            var eight64 = LLVMValueRef.CreateConstInt(i64, 8);
            var falseBit = LLVMValueRef.CreateConstInt(ctx.Int1Type, 0);

            // --- 2. Load Length (at offset 0) ---
            var lenPtr = _builder.BuildGEP2(i64, arrayPtr, new[] { zero64 }, "len_ptr");
            var length = _builder.BuildLoad2(i64, lenPtr, "length");

            // --- 3. Bounds Check ---
            var inBounds = _builder.BuildICmp(LLVMIntPredicate.LLVMIntULT, indexVal, length, "in_bounds");

            var function = _builder.InsertBlock.Parent;
            var removeBlock = function.AppendBasicBlock("do_remove");
            var skipBlock = function.AppendBasicBlock("skip_remove");

            _builder.BuildCondBr(inBounds, removeBlock, skipBlock);

            // --- 4. Remove Block ---
            _builder.PositionAtEnd(removeBlock);

            // moveCount = length - index - 1
            var moveCount = _builder.BuildSub(_builder.BuildSub(length, indexVal, "tmp1"), one64, "move_count");

            var needMove = _builder.BuildICmp(LLVMIntPredicate.LLVMIntUGT, moveCount, zero64, "need_move");
            var memmoveBlock = function.AppendBasicBlock("memmove_block");
            var afterMoveBlock = function.AppendBasicBlock("after_memmove");
            _builder.BuildCondBr(needMove, memmoveBlock, afterMoveBlock);

            // --- 5. Memmove Block ---
            _builder.PositionAtEnd(memmoveBlock);

            // Header is [Len, Cap]. index 0 is at GEP offset 2.
            var dstIdx = _builder.BuildAdd(two64, indexVal, "dst_idx");
            var srcIdx = _builder.BuildAdd(dstIdx, one64, "src_idx");

            var dstPtr = _builder.BuildGEP2(i64, arrayPtr, new[] { dstIdx }, "dst_ptr");
            var srcPtr = _builder.BuildGEP2(i64, arrayPtr, new[] { srcIdx }, "src_ptr");
            var bytes = _builder.BuildMul(moveCount, eight64, "move_bytes");

            // memmove in C is (dest, src, len)
            var dstI8 = _builder.BuildBitCast(dstPtr, i8Ptr, "dst_i8");
            var srcI8 = _builder.BuildBitCast(srcPtr, i8Ptr, "src_i8");

            var memmoveFunc = GetOrDeclareMemmove();
            _builder.BuildCall2(
                _memmoveType,
                memmoveFunc,
                new[] { dstI8, srcI8, bytes },
                "memmove_call"
            );

            _builder.BuildBr(afterMoveBlock);

            // --- 6. After Memmove ---
            _builder.PositionAtEnd(afterMoveBlock);
            var newLength = _builder.BuildSub(length, one64, "new_length");
            _builder.BuildStore(newLength, lenPtr);
            _builder.BuildBr(skipBlock);

            // --- 7. Skip Block ---
            _builder.PositionAtEnd(skipBlock);

            // Keep global reference consistent for named variable access
            if (expr.ArrayExpression is IdNodeExpr id)
            {
                var name = id.Name;
                var global = _module.GetNamedGlobal(name);
                if (global.Handle != IntPtr.Zero)
                {
                    if (arrayObjPtr.Handle != IntPtr.Zero)
                    {
                        _builder.BuildStore(arrayObjPtr, global);
                    }
                    else
                    {
                        var boxedArray = BoxArray(arrayPtr);
                        _builder.BuildStore(boxedArray, global);
                        arrayObjPtr = boxedArray;
                    }
                }
            }

            if (arrayObjPtr.Handle != IntPtr.Zero)
            {
                return arrayObjPtr;
            }

            return BoxArray(arrayPtr);
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
            // 1️ Visit the array expression to get its RuntimeValue* pointer
            LLVMValueRef arrVal = Visit(expr.ArrayExpression);

            // 2️ Access the data pointer inside the RuntimeValue struct
            // RuntimeValue layout: { i16 tag, i16 pad1, i32 pad2, ptr data }
            var dataPtr = _builder.BuildStructGEP2(_runtimeObjType, arrVal, 3, "data_ptr");
            var rawArrayPtr = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(_module.Context.Int64Type, 0), dataPtr, "raw_array_ptr");

            // 3️ Read the length from the first i64 in array memory
            var lenPtr = _builder.BuildGEP2(_module.Context.Int64Type, rawArrayPtr,
                new[] { LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 0) }, "len_ptr");
            var lengthValue = _builder.BuildLoad2(_module.Context.Int64Type, lenPtr, "length");

            // 4️ Box the length into a RuntimeValue* so the runtime can handle it safely
            //LLVMValueRef boxedLength = BoxValue(lengthValue, new IntType());

            // 5️ Return the boxed RuntimeValue*
            return lengthValue;
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
                    global.Linkage = LLVMLinkage.LLVMInternalLinkage;

                    // Optional: initialize
                    //_builder.BuildStore(LLVMValueRef.CreateConstInt(llvmType, 0), global);
                }

                ptrToLoad = global;
            }

            if (_debug) Console.WriteLine($"visiting: variable: {expr.Name} (Type: {entry.Type}, Ptr: {ptrToLoad})");

            // Load the value from the pointer
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
                //Console.WriteLine("visiting: " + expr);
                var name = expr.GetType().Name; // it fails here for if visits, but not the others. Why would it not be able to get the if nodes type and name?
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
            {
                return _builder.BuildFNeg(value, "fneg");
            }
            else
            {
                return _builder.BuildNeg(value, "neg");
            }
        }

        public LLVMValueRef VisitIndexAssignExpr(IndexAssignNodeExpr expr)
        {
            var ctx = _module.Context;
            var func = _builder.InsertBlock.Parent;

            // 1. Get array and index
            var arrayPtr = Visit(expr.ArrayExpression);
            var indexVal = Visit(expr.IndexExpression);

            // Ensure index is i64
            if (indexVal.TypeOf == ctx.DoubleType)
                indexVal = _builder.BuildFPToSI(indexVal, ctx.Int64Type, "idx_int");

            // 2. Bounds check (same as your IndexExpr)
            var zero = LLVMValueRef.CreateConstInt(ctx.Int64Type, 0);
            var lenPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { zero }, "len_ptr");
            var arrayLen = _builder.BuildLoad2(ctx.Int64Type, lenPtr, "array_len");

            var isNegative = _builder.BuildICmp(
                LLVMIntPredicate.LLVMIntSLT,
                indexVal,
                LLVMValueRef.CreateConstInt(ctx.Int64Type, 0),
                "is_neg");

            var isTooBig = _builder.BuildICmp(
                LLVMIntPredicate.LLVMIntSGE,
                indexVal,
                arrayLen,
                "is_too_big");

            var isInvalid = _builder.BuildOr(isNegative, isTooBig, "is_invalid");

            var failBlock = func.AppendBasicBlock("bounds.fail");
            var safeBlock = func.AppendBasicBlock("bounds.ok");

            _builder.BuildCondBr(isInvalid, failBlock, safeBlock);

            // FAIL
            _builder.PositionAtEnd(failBlock);
            var errorMsg = _builder.BuildGlobalStringPtr("Runtime Error: Index Out of Bounds!\n", "err_msg");
            _builder.BuildCall2(_printfType, _printf, new[] { errorMsg }, "print_err");

            var nullReturn = LLVMValueRef.CreateConstNull(LLVMTypeRef.CreatePointer(ctx.Int8Type, 0));
            _builder.BuildRet(nullReturn);

            // SAFE
            _builder.PositionAtEnd(safeBlock);

            // 3. Compute actual index (offset by header)
            var two = LLVMValueRef.CreateConstInt(ctx.Int64Type, 2);
            var actualIdx = _builder.BuildAdd(indexVal, two, "offset_idx");

            // 4. Get element pointer
            var elementPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { actualIdx }, "elem_ptr");

            // 5. Evaluate RHS value
            var value = Visit(expr.AssignExpression);
            var boxed = BoxToI64(value);

            // 6. Store into array
            _builder.BuildStore(boxed, elementPtr).SetAlignment(8);

            // 7. Return assigned value (common convention)
            return value;
        }
    }
}