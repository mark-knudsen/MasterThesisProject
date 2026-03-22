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
            if (value.TypeOf == _module.Context.Int64Type) return value;

            if (value.TypeOf == _module.Context.DoubleType)
                return _builder.BuildBitCast(value, _module.Context.Int64Type, "num_to_i64");

            if (value.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                return _builder.BuildPtrToInt(value, _module.Context.Int64Type, "ptr_to_i64");

            return _builder.BuildZExt(value, _module.Context.Int64Type, "zext");
        }

        private LLVMValueRef UnboxFromI64(LLVMValueRef val, Type targetType)
        {
            if (targetType is FloatType)
                return _builder.BuildBitCast(val, _module.Context.DoubleType);
            if (targetType is BoolType)
                return _builder.BuildTrunc(val, _module.Context.Int1Type);
            if (targetType is IntType)
                return val; // It's already i64
            return val; // Pointers/Strings are already i64/ptr equivalent
        }

        private void DeclareValueStruct()
        {
            var ctx = _module.Context;
            var tagType = ctx.Int32Type; // Tag type
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

        // Problems

        // TODO: fix the problems

        // BROKEN FUNCTIONALITY  
        // We cannot compare strings                      
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

        public object Run(NodeExpr expr, bool debug = false)
        {
            _debug = debug;
            // 1. Semantic analysis
            var prediction = PerformSemanticAnalysis(expr);

            CreateMain();
            DeclarePrintf();

            LLVMValueRef resultValue = Visit(expr);

            //PrintArray(resultValue);

            if (_debug) Console.WriteLine("LLVM TYPE: " + resultValue.TypeOf);
            if (_debug) Console.WriteLine("LANG TYPE: " + prediction);

            var boxedPtr = BoxValue(resultValue, prediction); // don't we just need one of them?
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

                    // 1. Get the element type from the prediction
                    // Assuming prediction is of type 'ArrayType', we get its 'ElementType' property
                    Type innerType = (prediction as ArrayType)?.ElementType;

                    // 2. Pass both the pointer and the type to the handler
                    return HandleArray2(result.data, innerType);


                case ValueTag.None:
                    if (_debug) Console.WriteLine("return none");

                    return default;
            }

            return result;
        }

        private object HandleArray(IntPtr dataPtr)
        {
            if (dataPtr == IntPtr.Zero)
                return "[]";

            // First 8 bytes = length
            long length = Marshal.ReadInt64(dataPtr);
            var elements = new List<string>((int)length);

            if (_debug) Console.WriteLine("we about to iterate and print array");

            for (long i = 0; i < length; i++)
            {
                // Each element pointer offset
                var elementPtr = IntPtr.Add(dataPtr, (int)((i + 1) * 8));

                if (_debug) Console.WriteLine("we about to iterate and print array1"); // we fail right here
                // Read as a RuntimeValue struct
                RuntimeValue element = Marshal.PtrToStructure<RuntimeValue>(Marshal.ReadIntPtr(elementPtr));
                if (_debug) Console.WriteLine("we about to iterate and print array2");

                string elStr = element.tag switch
                {
                    (Int16)ValueTag.Int => Marshal.ReadInt64(element.data).ToString(),
                    (Int16)ValueTag.Float => Marshal.PtrToStructure<double>(element.data).ToString(),
                    (Int16)ValueTag.Bool => (Marshal.ReadByte(element.data) != 0).ToString(),
                    (Int16)ValueTag.String => Marshal.PtrToStringAnsi(element.data),
                    (Int16)ValueTag.Array => HandleArray(element.data).ToString(),
                    _ => "none"
                };

                if (_debug) Console.WriteLine("we about to iterate and print array3");
                elements.Add(elStr);
            }

            if (_debug) Console.WriteLine("we about to iterate and print array4");
            return "[" + string.Join(", ", elements) + "]";
        }

        private object HandleArray2(IntPtr dataPtr, Type elementType)
        {
            if (dataPtr == IntPtr.Zero) return "[]";

            // 1. Read the length from the header (offset 0)
            long length = Marshal.ReadInt64(dataPtr);
            var resultElements = new List<string>();

            // 2. Iterate through elements
            for (long i = 0; i < length; i++)
            {
                // Data starts at index 2 (skipping len and cap)
                IntPtr elementPtr = IntPtr.Add(dataPtr, (int)((i + 2) * 8));
                long rawValue = Marshal.ReadInt64(elementPtr);

                if (elementType is FloatType)
                {
                    double dblValue = BitConverter.Int64BitsToDouble(rawValue);
                    resultElements.Add(dblValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                else if (elementType is BoolType)
                {
                    resultElements.Add((rawValue & 1) == 1 ? "true" : "false");
                }
                else if (elementType is IntType)
                {
                    resultElements.Add(rawValue.ToString());
                }
                else if (elementType is StringType)
                {
                    string str = Marshal.PtrToStringAnsi((IntPtr)rawValue);
                    resultElements.Add($"\"{str}\"");
                }
                // --- ADD THIS BLOCK FOR NESTED ARRAYS ---
                else if (elementType is ArrayType innerArrayType)
                {
                    // rawValue is the pointer to the inner array's memory
                    IntPtr innerPtr = (IntPtr)rawValue;
                    // Recursively call HandleArray2 for the inner list
                    string innerResult = HandleArray2(innerPtr, innerArrayType.ElementType).ToString();
                    resultElements.Add(innerResult);
                }
            }

            return "[" + string.Join(", ", resultElements) + "]";
        }


        private LLVMValueRef BoxValue(LLVMValueRef value, Type type)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i16 = ctx.Int16Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            var mallocType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i64 }, false);
            // Option A: Literal/Anonymous struct (most common for temporary boxing)
            var runtimeValueType = LLVMTypeRef.CreateStruct(new[] { i16, i8Ptr }, false); // it ABSOLUTELY have to match the struct

            int tag = type switch
            {
                IntType => (int)ValueTag.Int,
                FloatType => (int)ValueTag.Float,
                BoolType => (int)ValueTag.Bool,
                StringType => (int)ValueTag.String,
                ArrayType => (int)ValueTag.Array,
                _ => (int)ValueTag.None
            };

            LLVMValueRef dataPtr;

            // --- REVISED LOGIC FOR STRINGS AND ARRAYS ---
            if (type is StringType || type is ArrayType)
            {
                // Check if 'value' is actually a Global Variable (like @arr or @x)
                // If it is, 'value' is the ADDRESS of the pointer. We need to LOAD it.
                if (!value.IsAGlobalVariable.Handle.Equals(IntPtr.Zero))
                {
                    dataPtr = _builder.BuildLoad2(i8Ptr, value, "actual_ptr");
                }
                else
                {
                    dataPtr = value;
                }
            }
            else if (type is VoidType)
            {
                dataPtr = LLVMValueRef.CreateConstPointerNull(i8Ptr);
            }
            else if (type is IntType)
            {
                var malloc = GetOrDeclareMalloc();
                var rawMem = _builder.BuildCall2(mallocType, malloc, new[] { LLVMValueRef.CreateConstInt(i64, 8) }, "int_mem");
                var cast = _builder.BuildBitCast(rawMem, LLVMTypeRef.CreatePointer(i64, 0), "int_cast");
                _builder.BuildStore(value, cast).SetAlignment(8);
                dataPtr = rawMem;
            }
            else if (type is FloatType)
            {
                var malloc = GetOrDeclareMalloc();
                var rawMem = _builder.BuildCall2(mallocType, malloc, new[] { LLVMValueRef.CreateConstInt(i64, 8) }, "float_mem");
                var cast = _builder.BuildBitCast(rawMem, LLVMTypeRef.CreatePointer(ctx.DoubleType, 0), "float_cast");
                _builder.BuildStore(value, cast).SetAlignment(8);
                dataPtr = rawMem;
            }
            else if (type is BoolType)
            {
                var malloc = GetOrDeclareMalloc();
                // Use 8 bytes even for bools so Marshal.ReadInt64 doesn't over-read
                var rawMem = _builder.BuildCall2(mallocType, malloc, new[] { LLVMValueRef.CreateConstInt(i64, 8) }, "bool_mem");
                var cast = _builder.BuildBitCast(rawMem, LLVMTypeRef.CreatePointer(i64, 0), "bool_cast");

                // Zero-extend i1 to i64 for consistent reading in C#
                var extendedBool = _builder.BuildZExt(value, i64, "bool_to_i64");
                _builder.BuildStore(extendedBool, cast).SetAlignment(8);
                dataPtr = rawMem;
            }
            else
            {
                throw new Exception($"Unsupported LLVM type in BoxValue: {value.TypeOf}");
            }

            // 4. Allocate the 'RuntimeValue' struct
            var mallocReturn = GetOrDeclareMalloc();
            var obj = _builder.BuildCall2(mallocType, mallocReturn, new[] { LLVMValueRef.CreateConstInt(i64, 16) }, "runtime_obj");

            // 5. Store tag (i32) and data (ptr)
            var tagPtr = _builder.BuildStructGEP2(runtimeValueType, obj, 0, "tag_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i16, (ulong)tag), tagPtr);

            var dataFieldPtr = _builder.BuildStructGEP2(runtimeValueType, obj, 1, "data_ptr");
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

            // 1. FIX: Load actual pointer if it's a global variable
            var arrayRef = Visit(expr.Array);
            LLVMValueRef arrayPtr;
            if (!arrayRef.IsAGlobalVariable.Handle.Equals(IntPtr.Zero))
                arrayPtr = _builder.BuildLoad2(i8Ptr, arrayRef, "actual_array_ptr");
            else
                arrayPtr = arrayRef;

            // 2. Fetch length correctly (offset 0)
            var zero64 = LLVMValueRef.CreateConstInt(i64, 0);
            var lenPtr = _builder.BuildGEP2(i64, arrayPtr, new[] { zero64 }, "len_ptr");
            var arrayLen = _builder.BuildLoad2(i64, lenPtr, "array_len");

            // 3. Setup Iterator Allocation
            var elementType = ((ArrayType)expr.Array.Type).ElementType;
            var llvmElemType = GetLLVMType(elementType);
            // Alloca must be in the entry block or before the loop to be stable
            var iteratorAlloc = _builder.BuildAlloca(llvmElemType, expr.Iterator.Name);

            // 4. SCOPE: Save old context, add iterator, restore later
            var previousContext = _context;
            _context = _context.Add(expr.Iterator.Name, iteratorAlloc, null, elementType);

            // 5. Loop blocks
            var condBlock = func.AppendBasicBlock("foreach.cond");
            var bodyBlock = func.AppendBasicBlock("foreach.body");
            var endBlock = func.AppendBasicBlock("foreach.end");

            // 6. Counter Initialization
            var counterAlloc = _builder.BuildAlloca(i64, "foreach_counter");
            _builder.BuildStore(zero64, counterAlloc);
            _builder.BuildBr(condBlock);

            // 7. Condition Block
            _builder.PositionAtEnd(condBlock);
            var curIdx = _builder.BuildLoad2(i64, counterAlloc, "cur_idx");
            var cond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, curIdx, arrayLen, "foreach_test");
            _builder.BuildCondBr(cond, bodyBlock, endBlock);

            // 8. Body Block
            _builder.PositionAtEnd(bodyBlock);

            // Calculate memory index (header is 2 x i64)
            var two64 = LLVMValueRef.CreateConstInt(i64, 2);
            var memIdx = _builder.BuildAdd(curIdx, two64, "mem_idx");
            var elementPtr = _builder.BuildGEP2(i64, arrayPtr, new[] { memIdx }, "elem_ptr");
            var rawVal = _builder.BuildLoad2(i64, elementPtr, "raw_val");

            // Unbox if the iterator is a specific type (like float or bool)
            var unboxedVal = UnboxFromI64(rawVal, elementType);
            _builder.BuildStore(unboxedVal, iteratorAlloc);

            // Visit the actual loop body
            Visit(expr.Body);

            // 9. Increment and Jump Back
            var one64 = LLVMValueRef.CreateConstInt(i64, 1);
            var nextIdx = _builder.BuildAdd(curIdx, one64, "next_idx");
            _builder.BuildStore(nextIdx, counterAlloc);
            _builder.BuildBr(condBlock);

            // 10. Wrap up
            _builder.PositionAtEnd(endBlock);
            _context = previousContext; // Restore context to remove the iterator variable

            return LLVMValueRef.CreateConstPointerNull(i8Ptr); // Foreach returns void
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

            // Ensure condition is i1 (Boolean)
            if (condValue.TypeOf == _module.Context.DoubleType)
            {
                condValue = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE,
                    condValue, LLVMValueRef.CreateConstReal(_module.Context.DoubleType, 0.0), "if_cond_f");
            }
            else if (condValue.TypeOf != _module.Context.Int1Type)
            {
                condValue = _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE,
                    condValue, LLVMValueRef.CreateConstInt(condValue.TypeOf, 0), "if_cond_i");
            }

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

            // 2. --- THE FIX ---
            // If the RHS is a global variable, we need the VALUE inside it, 
            // not the address of the global itself.
            if (!value.IsAGlobalVariable.Handle.Equals(IntPtr.Zero))
            {
                var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
                value = _builder.BuildLoad2(i8Ptr, value, "loaded_val");
            }


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
            var srcAssign = new AssignNodeExpr(srcVarName, new CopyArrayNodeExpr(expr.ArrayExpr));

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

        public LLVMValueRef VisitCopyArrayExpr(CopyArrayNodeExpr expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 1. Evaluate the source expression (could be a global like @arr)
            var srcArrayRef = Visit(expr.SourceArray);

            // 2. FIX: If it's a global variable, load the actual pointer stored inside it
            LLVMValueRef srcArray;
            if (!srcArrayRef.IsAGlobalVariable.Handle.Equals(IntPtr.Zero))
            {
                srcArray = _builder.BuildLoad2(i8Ptr, srcArrayRef, "actual_src_ptr");
            }
            else
            {
                srcArray = srcArrayRef;
            }

            // 3. Load length from the header (index 0)
            var zero64 = LLVMValueRef.CreateConstInt(i64, 0);
            var lengthPtr = _builder.BuildGEP2(i64, srcArray, new[] { zero64 }, "len_ptr");
            var length = _builder.BuildLoad2(i64, lengthPtr, "length");

            // 4. Allocate new array with length + 2 slots (Header: [Len, Cap])
            var eight64 = LLVMValueRef.CreateConstInt(i64, 8);
            var two64 = LLVMValueRef.CreateConstInt(i64, 2);
            var totalSlots = _builder.BuildAdd(length, two64, "total_slots");
            var totalBytes = _builder.BuildMul(totalSlots, eight64, "total_bytes");

            var mallocFunc = GetOrDeclareMalloc();
            var newArray = _builder.BuildCall2(_mallocType, mallocFunc, new[] { totalBytes }, "new_arr_ptr");

            // 5. Store length and capacity in the new array header
            var capPtrNew = _builder.BuildGEP2(i64, newArray, new[] { LLVMValueRef.CreateConstInt(i64, 1) }, "cap_ptr_new");
            var lenPtrNew = _builder.BuildGEP2(i64, newArray, new[] { zero64 }, "len_ptr_new");

            _builder.BuildStore(length, lenPtrNew).SetAlignment(8);
            _builder.BuildStore(length, capPtrNew).SetAlignment(8);

            // 6. Loop setup to copy elements
            var indexAlloc = _builder.BuildAlloca(i64, "clone_i");
            _builder.BuildStore(zero64, indexAlloc);

            var func = _builder.InsertBlock.Parent;
            var loopCond = func.AppendBasicBlock("clone.cond");
            var loopBody = func.AppendBasicBlock("clone.body");
            var loopEnd = func.AppendBasicBlock("clone.end");

            _builder.BuildBr(loopCond);
            _builder.PositionAtEnd(loopCond);

            var iVal = _builder.BuildLoad2(i64, indexAlloc, "i_val");
            var cmp = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, iVal, length, "cmp");
            _builder.BuildCondBr(cmp, loopBody, loopEnd);

            // 7. Loop Body: Copy elements one by one
            _builder.PositionAtEnd(loopBody);

            // Data starts at offset 2
            var offset = _builder.BuildAdd(iVal, two64, "offset");

            // Load from source (using our resolved srcArray)
            var srcElemPtr = _builder.BuildGEP2(i64, srcArray, new[] { offset }, "src_elem_ptr");
            var val = _builder.BuildLoad2(i64, srcElemPtr, "val");

            // Store to destination
            var dstElemPtr = _builder.BuildGEP2(i64, newArray, new[] { offset }, "dst_elem_ptr");
            _builder.BuildStore(val, dstElemPtr).SetAlignment(8);

            // Increment index
            var iNext = _builder.BuildAdd(iVal, LLVMValueRef.CreateConstInt(i64, 1), "i_next");
            _builder.BuildStore(iNext, indexAlloc);
            _builder.BuildBr(loopCond);

            // 8. Exit
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
            var count = (uint)expr.Elements.Count;

            // 1. Raw Malloc
            var mallocFunc = GetOrDeclareMalloc();
            var totalBytes = LLVMValueRef.CreateConstInt(ctx.Int64Type, (ulong)(count + 2) * 8);
            var arrayPtr = _builder.BuildCall2(_mallocType, mallocFunc, new[] { totalBytes }, "arr_ptr");

            // 2. Set Length and Capacity MANUALLY (Immediate)
            // We do this manually to "prime" the array so IndexAssign's bounds check passes.
            var zero64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 0);
            var one64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 1);
            var countVal = LLVMValueRef.CreateConstInt(ctx.Int64Type, count);

            _builder.BuildStore(countVal, _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { zero64 })).SetAlignment(8);
            _builder.BuildStore(countVal, _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { one64 })).SetAlignment(8);

            // 3. Temporary Context Entry for Initialization
            var tmpName = $"__init_{Guid.NewGuid().ToString().Substring(0, 4)}";
            // We add it to context so the child nodes (IdNodeExpr) can find it
            _context = _context.Add(tmpName, arrayPtr, "tmp", new ArrayType(expr.ElementType));

            // 4. Use IndexAssign for every element
            for (int i = 0; i < expr.Elements.Count; i++)
            {
                // This will now use your VisitIndexAssign, which will:
                // - Visit IdNodeExpr (Returning the raw pointer safely)
                // - Perform Bounds Check (Which passes because we set length in Step 2)
                // - Box the value and store it
                Visit(new IndexAssignNodeExpr(
                    new IdNodeExpr(tmpName),
                    new NumberNodeExpr(i),
                    expr.Elements[i]
                ));
            }

            expr.SetType(new ArrayType(expr.ElementType));
            return arrayPtr;
        }


        public LLVMValueRef VisitIndexExpr(IndexNodeExpr expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var func = _builder.InsertBlock.Parent;

            // 1. Visit the Array Reference
            var arrayRef = Visit(expr.ArrayExpression);

            // --- FIX: Load the actual heap pointer if it's a global ---
            LLVMValueRef arrayPtr;
            if (!arrayRef.IsAGlobalVariable.Handle.Equals(IntPtr.Zero))
                arrayPtr = _builder.BuildLoad2(i8Ptr, arrayRef, "actual_array_ptr");
            else
                arrayPtr = arrayRef;

            var indexVal = Visit(expr.IndexExpression);
            if (indexVal.TypeOf == ctx.DoubleType)
                indexVal = _builder.BuildFPToSI(indexVal, i64, "idx_int");

            // 2. Bounds Check (Using actual arrayPtr)
            var zero64 = LLVMValueRef.CreateConstInt(i64, 0);
            var lenPtr = _builder.BuildGEP2(i64, arrayPtr, new[] { zero64 }, "len_ptr");
            var arrayLen = _builder.BuildLoad2(i64, lenPtr, "array_len");

            var isNegative = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, indexVal, zero64, "is_neg");
            var isTooBig = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, indexVal, arrayLen, "is_too_big");
            var isInvalid = _builder.BuildOr(isNegative, isTooBig, "is_invalid");

            var failBlock = func.AppendBasicBlock("bounds.fail");
            var safeBlock = func.AppendBasicBlock("bounds.ok");
            _builder.BuildCondBr(isInvalid, failBlock, safeBlock);

            // --- FAIL ---
            _builder.PositionAtEnd(failBlock);
            var errorMsg = _builder.BuildGlobalStringPtr("Runtime Error: Index Out of Bounds!\n", "err_msg");
            _builder.BuildCall2(_printfType, _printf, new[] { errorMsg }, "print_err");
            _builder.BuildRet(LLVMValueRef.CreateConstNull(i8Ptr));

            // --- SAFE ---
            _builder.PositionAtEnd(safeBlock);
            var two64 = LLVMValueRef.CreateConstInt(i64, 2);
            var actualIdx = _builder.BuildAdd(indexVal, two64, "offset_idx");

            var elementPtr = _builder.BuildGEP2(i64, arrayPtr, new[] { actualIdx }, "elem_ptr");
            var rawValue = _builder.BuildLoad2(i64, elementPtr, "raw_val");

            // 3. Unbox/Cast based on type
            return expr.Type switch
            {
                // If it's a string, we stored the pointer as an i64. 
                // We cast it back to a pointer (i8*) so printf/methods can use it.
                StringType => _builder.BuildIntToPtr(rawValue, LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), "to_str"),

                // Bitcast reinterprets the 64 bits of the i64 as a double.
                FloatType => _builder.BuildBitCast(rawValue, ctx.DoubleType, "to_float"),

                // For Int, no change needed as our internal array storage is i64.
                IntType => rawValue,

                // For Booleans, you have two choices:
                // 1. ICmp NE 0: Returns an i1 (true/false).
                // 2. Trunc: Just chops off the extra 63 bits.
                // ICmp is safer if you aren't 100% sure the upper bits are clean.
                BoolType => _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, rawValue, LLVMValueRef.CreateConstInt(ctx.Int64Type, 0), "to_bool"),

                // If it's a nested array (ArrayType), the rawValue is the pointer to the inner array.
                // Cast it to a pointer so it's ready for GEP.
                ArrayType => _builder.BuildIntToPtr(rawValue, LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), "to_arr_ptr"),

                _ => rawValue
            };

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
                _ => throw new Exception($"Unsupported type: {type}") // it doesn't have a type? how?
            };
        }



        public LLVMValueRef VisitAddExpr(AddNodeExpr expr)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;

            // 1. Get the reference (could be a global variable @arr)
            var arrayRef = Visit(expr.ArrayExpression);
            var arrayName = ((IdNodeExpr)expr.ArrayExpression).Name;

            // 2. IMPORTANT: Load the actual heap pointer from the global if needed
            LLVMValueRef arrayPtr;
            if (!arrayRef.IsAGlobalVariable.Handle.Equals(IntPtr.Zero))
                arrayPtr = _builder.BuildLoad2(LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), arrayRef, "actual_array_ptr");
            else
                arrayPtr = arrayRef;

            var value = Visit(expr.AddExpression);
            var boxed = BoxToI64(value);

            var zero64 = LLVMValueRef.CreateConstInt(i64, 0);
            var one64 = LLVMValueRef.CreateConstInt(i64, 1);
            var two64 = LLVMValueRef.CreateConstInt(i64, 2);
            var eight64 = LLVMValueRef.CreateConstInt(i64, 8);

            var entryBlock = _builder.InsertBlock;

            // Use i64 for GEP calculations
            var lenPtr = _builder.BuildGEP2(i64, arrayPtr, new[] { zero64 }, "len_ptr");
            var length = _builder.BuildLoad2(i64, lenPtr, "length");

            var capPtr = _builder.BuildGEP2(i64, arrayPtr, new[] { one64 }, "cap_ptr");
            var capacity = _builder.BuildLoad2(i64, capPtr, "capacity");

            var isFull = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, length, capacity, "is_full");

            var function = _builder.InsertBlock.Parent;
            var growBlock = ctx.AppendBasicBlock(function, "grow");
            var contBlock = ctx.AppendBasicBlock(function, "cont");
            _builder.BuildCondBr(isFull, growBlock, contBlock);

            // ---- grow ----
            _builder.PositionAtEnd(growBlock);
            var newCap = _builder.BuildMul(capacity, two64, "new_capacity");
            var minCap = LLVMValueRef.CreateConstInt(i64, 4);
            newCap = _builder.BuildSelect(_builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, capacity, zero64), minCap, newCap, "new_cap_sel");

            var totalSlots = _builder.BuildAdd(newCap, two64, "slots");
            var totalBytes = _builder.BuildMul(totalSlots, eight64, "total_bytes");

            var reallocFunc = GetOrDeclareRealloc();
            // PASS THE HEAP PTR, NOT THE GLOBAL REF
            var newArrayPtr = _builder.BuildCall2(_reallocType, reallocFunc, new[] { arrayPtr, totalBytes }, "realloc_array");

            var capPtrGrow = _builder.BuildGEP2(i64, newArrayPtr, new[] { one64 }, "cap_ptr_grow");
            _builder.BuildStore(newCap, capPtrGrow).SetAlignment(8);
            _builder.BuildBr(contBlock);

            // ---- continue ----
            _builder.PositionAtEnd(contBlock);

            var arrayPtrType = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var arrayPtrPhi = _builder.BuildPhi(arrayPtrType, "array_ptr_phi");
            arrayPtrPhi.AddIncoming(new[] { arrayPtr, newArrayPtr }, new[] { entryBlock, growBlock }, 2);

            var elemIndex = _builder.BuildAdd(length, two64, "elem_index");
            var elemPtr = _builder.BuildGEP2(i64, arrayPtrPhi, new[] { elemIndex }, "elem_ptr");
            _builder.BuildStore(boxed, elemPtr).SetAlignment(8);

            var newLength = _builder.BuildAdd(length, one64, "new_length");
            var lenPtrCont = _builder.BuildGEP2(i64, arrayPtrPhi, new[] { zero64 }, "len_ptr_cont");
            _builder.BuildStore(newLength, lenPtrCont).SetAlignment(8);

            // Update the Global Variable with the (potentially new) pointer
            var global = _module.GetNamedGlobal(arrayName);
            _builder.BuildStore(arrayPtrPhi, global);

            return arrayPtrPhi;
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
            var arrayPtrRaw = Visit(expr.ArrayExpression);

            LLVMValueRef arrayPtr;
            // Check if we are dealing with the Global variable itself or an already loaded pointer
            if (arrayPtrRaw.IsAGlobalVariable.Handle != IntPtr.Zero)
                // It's @arr (the global), so we load the malloc'd address stored inside it
                arrayPtr = _builder.BuildLoad2(i8Ptr, arrayPtrRaw, "arr_load");
            else
                // It's already the malloc'd address (from a nested expression)
                arrayPtr = arrayPtrRaw;

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

            // Execute Call with the 4-argument signature
            var memmoveFunc = GetOrDeclareMemmove();
            _builder.BuildCall2(
                _memmoveType,
                memmoveFunc,
                new[] { dstPtr, srcPtr, bytes, falseBit },
                ""
            );

            _builder.BuildBr(afterMoveBlock);

            // --- 6. After Memmove ---
            _builder.PositionAtEnd(afterMoveBlock);
            var newLength = _builder.BuildSub(length, one64, "new_length");
            _builder.BuildStore(newLength, lenPtr);
            _builder.BuildBr(skipBlock);

            // --- 7. Skip Block ---
            _builder.PositionAtEnd(skipBlock);

            return arrayPtrRaw;
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

            var arrayRef = Visit(expr.ArrayExpression);

            LLVMValueRef arrayPtr;
            if (!arrayRef.IsAGlobalVariable.Handle.Equals(IntPtr.Zero))
                arrayPtr = _builder.BuildLoad2(i8Ptr, arrayRef, "actual_ptr");
            else
                arrayPtr = arrayRef;

            // Length is at offset 0
            var zero64 = LLVMValueRef.CreateConstInt(i64, 0);
            var lenPtr = _builder.BuildGEP2(i64, arrayPtr, new[] { zero64 }, "len_ptr");
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
            if (entry == null) throw new Exception($"Variable {expr.Name} not found.");

            var llvmType = GetLLVMType(entry.Type);
            LLVMValueRef ptr;

            // 1. Is it a local/parameter?
            if (entry.Value.Handle != IntPtr.Zero)
            {
                ptr = entry.Value;
            }
            else
            {
                // 2. It's a Global (REPL variable). 
                // We must find it in the current module or declare it so LLVM knows it exists elsewhere.
                ptr = _module.GetNamedGlobal(expr.Name);
                if (ptr.Handle == IntPtr.Zero)
                {
                    ptr = _module.AddGlobal(llvmType, expr.Name);
                    ptr.Linkage = LLVMLinkage.LLVMExternalLinkage; // Tell JIT: "Look in previous modules"
                }
            }

            // 3. Array/String Check (As we discussed before - no load for these!)
            if (entry.Type is ArrayType || entry.Type is StringType)
            {
                return ptr;
            }

            // 4. Primitive Load
            // Note: Use 'llvmType' explicitly in BuildLoad2
            return _builder.BuildLoad2(llvmType, ptr, expr.Name + "_load");
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

        public LLVMValueRef VisitReadCsvExpr(ReadCsvNodeExpr expr)
        {
            Console.WriteLine("we visit read csv");

            // should in actuality return a dataframe, but we are far away from that
            // a start could be to just return the text from a file

            return Visit(expr.FileNameExpr);
        }
    }
}