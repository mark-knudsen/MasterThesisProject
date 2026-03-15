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

namespace MyCompiler
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RuntimeArray
    {
        public long Length;
        public long Capacity;
        public IntPtr Data;
    }

    public static class ArrayRuntime
    {
        [UnmanagedCallersOnly(EntryPoint = "array_new")]
        public static IntPtr ArrayNew()
        {
            var arr = new RuntimeArray
            {
                Length = 0,
                Capacity = 4,
                Data = Marshal.AllocHGlobal(sizeof(long) * 4)
            };

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<RuntimeArray>());
            Marshal.StructureToPtr(arr, ptr, false);
            return ptr;
        }

        [UnmanagedCallersOnly(EntryPoint = "array_push")]
        public static void ArrayPush(IntPtr arrPtr, long value)
        {
            var arr = Marshal.PtrToStructure<RuntimeArray>(arrPtr);

            if (arr.Length >= arr.Capacity)
            {
                arr.Capacity *= 2;
                arr.Data = Marshal.ReAllocHGlobal(arr.Data, (IntPtr)(sizeof(long) * arr.Capacity));
            }

            Marshal.WriteInt64(arr.Data, (int)(arr.Length * sizeof(long)), value);
            arr.Length++;

            Marshal.StructureToPtr(arr, arrPtr, false);
        }

        [UnmanagedCallersOnly(EntryPoint = "array_get")]
        public static long ArrayGet(IntPtr arrPtr, long index)
        {
            var arr = Marshal.PtrToStructure<RuntimeArray>(arrPtr);
            return Marshal.ReadInt64(arr.Data, (int)(index * sizeof(long)));
        }

        [UnmanagedCallersOnly(EntryPoint = "array_len")]
        public static long ArrayLen(IntPtr arrPtr)
        {
            var arr = Marshal.PtrToStructure<RuntimeArray>(arrPtr);
            return arr.Length;
        }
    }

    public class ArrayHelpers
    {
        private LLVMModuleRef _module;
        private LLVMBuilderRef _builder;

        // LLVM function references
        private LLVMValueRef _arrayNew;
        private LLVMValueRef _arrayPush;
        private LLVMValueRef _arrayGet;
        private LLVMValueRef _arrayLen;

        public ArrayHelpers(LLVMModuleRef module, LLVMBuilderRef builder)
        {
            _module = module;
            _builder = builder;
            DeclareArrayFunctions();
        }

        private void DeclareArrayFunctions()
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // array_new: () -> ptr
            var newType = LLVMTypeRef.CreateFunction(ptr, Array.Empty<LLVMTypeRef>(), false);
            _arrayNew = _module.AddFunction("array_new", newType);

            // array_push: (ptr, i64) -> void
            var pushType = LLVMTypeRef.CreateFunction(ctx.VoidType, new[] { ptr, i64 }, false);
            _arrayPush = _module.AddFunction("array_push", pushType);

            // array_get: (ptr, i64) -> i64
            var getType = LLVMTypeRef.CreateFunction(i64, new[] { ptr, i64 }, false);
            _arrayGet = _module.AddFunction("array_get", getType);

            // array_len: (ptr) -> i64
            var lenType = LLVMTypeRef.CreateFunction(i64, new[] { ptr }, false);
            _arrayLen = _module.AddFunction("array_len", lenType);
        }

        // --- Compiler helper methods ---
        public LLVMValueRef CreateArray()
        {
            return _builder.BuildCall2(_arrayNew.TypeOf.ElementType, _arrayNew, Array.Empty<LLVMValueRef>(), "newarr");
        }

        public void ArrayPushValue(LLVMValueRef arr, LLVMValueRef value)
        {
            _builder.BuildCall2(_arrayPush.TypeOf.ElementType, _arrayPush, new[] { arr, value }, "");
        }

        public LLVMValueRef ArrayGetValue(LLVMValueRef arr, LLVMValueRef index)
        {
            return _builder.BuildCall2(_arrayGet.TypeOf.ElementType, _arrayGet, new[] { arr, index }, "elem");
        }

        public LLVMValueRef ArrayLength(LLVMValueRef arr)
        {
            return _builder.BuildCall2(_arrayLen.TypeOf.ElementType, _arrayLen, new[] { arr }, "len");
        }

    }
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
        private ArrayHelpers _arrayHelper;
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

            _arrayHelper = new ArrayHelpers(_module, _builder);

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
        private void DeclareValueStruct()
        {
            var ctx = _module.Context;
            var tagType = ctx.Int16Type; // Tag type
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
            if (programedResult is ExpressionNodeExpr exp) return exp.Type; // it says the type is int right here for round

            return new VoidType();
        }

        // Problems

        // TODO: fix the problems

        // FUNCTIONALITY
        // can't do the command: "harry"
        // can get a result back with: do c=2; c++; c. 
        // but cannot get a result back from: c=2; c++                              
        // can't use random, it receives double?                    
        // can't print out array or assign it without error
        // but can use index on an array

        // UNIT TESTING
        // create a orc unit test
        // A REPL can do multipe commands in a row and they need to be tested to see if they work correctly, this has to be setup in a integration test
        // we need to not return the struct, we should only print if there is a return value
        // what the compiler returns would if anything be a exit code, 0 for no problem or say 1 for error

        // OTHER
        // currently it also prints out the lenght when calling print, not sure where it happends
        // AddImplicitPrint and visitprint are very similar, can be refactored
        // int is sometimes set to i64, but int is standard i32 in many langauges, it should be consistent and don't think we need int64 as standard for our language

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

            if (debug) DumpIR(_module);

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

            var result1 = delegateResult();
            RuntimeValue result = Marshal.PtrToStructure<RuntimeValue>(result1);

            // var table = "";
            // foreach (var item in _context.GetAll())
            // {
            //     table += "\nkey name: " + item.Key + " data type: " + item.Value.Type;
            //     //Console.WriteLine("key name: " + item.Key + " data type: " + item.Value.Type);    
            // }
            // var d = new PrintNodeExpr(new StringNodeExpr(table));
            // VisitPrintExpr(d);

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
                    return HandleArray2(result.data);

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

        private object HandleArray2(IntPtr dataPtr)
        {
            // The array is stored as:
            // [0] = length (i64)
            // [1..length] = element values (boxed as i64)
            if (dataPtr == IntPtr.Zero)
                return Array.Empty<long>();

            long length = Marshal.ReadInt64(dataPtr);
            var elements = new List<long>((int)length);

            for (long i = 0; i < length; i++)
            {
                // Element offset = (i + 1) * 8 bytes (skip the length header)
                var elementPtr = IntPtr.Add(dataPtr, (int)((i + 1) * 8));
                long rawValue = Marshal.ReadInt64(elementPtr);   // HERE we need to do something else for the different type like float!
                elements.Add(rawValue);
            }
            // Need to fix for strings and float! Currently it gains pointer to the string...

            string arrtext = "[";
            foreach (var el in elements)
            {
                arrtext += el + ", ";
            }
            arrtext = arrtext.TrimEnd(',', ' ') + "]";
            return arrtext;
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
            var runtimeValueType = LLVMTypeRef.CreateStruct(new[] { i32, i8Ptr }, false);

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
            {
                throw new Exception($"Unsupported LLVM type in BoxValue: {value.TypeOf}");
            }

            // 4. Allocate the 'RuntimeValue' struct (using local mallocType)
            var mallocReturn = GetOrDeclareMalloc();
            var sizeReturn = LLVMValueRef.CreateConstInt(i64, 16);
            var obj = _builder.BuildCall2(mallocType, mallocReturn, new[] { sizeReturn }, "runtime_obj");

            // 5. Store tag and data using the local runtimeValueType
            var tagPtr = _builder.BuildStructGEP2(runtimeValueType, obj, 0, "tag_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int16Type, (ulong)tag), tagPtr);

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
            var func = _builder.InsertBlock.Parent;

            // 1. Get the Array Pointer and Load Length from Header (Index 0)
            var arrayPtr = Visit(expr.Array);
            var zero = LLVMValueRef.CreateConstInt(ctx.Int32Type, 0);
            var lenPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { zero }, "len_ptr");
            var arrayLen = _builder.BuildLoad2(ctx.Int64Type, lenPtr, "array_len");



            // 2. Ensure the iterator variable (e.g., 'item') exists in the LLVM Module
            VisitAssignExpr(new AssignNodeExpr(expr.Iterator.Name, new BooleanNodeExpr(true)));

            LLVMValueRef iteratorGlobal = _module.GetNamedGlobal(expr.Iterator.Name);


            // 3. Setup Loop Blocks
            var condBlock = func.AppendBasicBlock("foreach.cond");
            var bodyBlock = func.AppendBasicBlock("foreach.body");
            var endBlock = func.AppendBasicBlock("foreach.end");

            // 4. Initialize hidden counter (alloca)
            var counterAlloc = _builder.BuildAlloca(ctx.Int64Type, "foreach_counter");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int64Type, 0), counterAlloc).SetAlignment(8);
            _builder.BuildBr(condBlock);

            // 5. Condition Block: Is counter < arrayLen?
            _builder.PositionAtEnd(condBlock);
            var currentIdx = _builder.BuildLoad2(ctx.Int64Type, counterAlloc, "cur_idx");
            var cond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, currentIdx, arrayLen, "foreach_test");
            _builder.BuildCondBr(cond, bodyBlock, endBlock);

            // 6. Body Block
            _builder.PositionAtEnd(bodyBlock);

            // Calculate memory index (UserIndex + 1 to skip header)
            var one = LLVMValueRef.CreateConstInt(ctx.Int64Type, 1);
            var memIdx = _builder.BuildAdd(currentIdx, one, "mem_idx");

            // Load the element
            var elementPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { memIdx }, "elem_ptr");
            var elementValRaw = _builder.BuildLoad2(ctx.Int64Type, elementPtr, "elem_val");
            elementValRaw.SetAlignment(8);

            // Store current element into the global iterator variable
            // This makes 'item' accessible to the code inside Visit(expr.Body)
            _builder.BuildStore(elementValRaw, iteratorGlobal).SetAlignment(8);

            // Execute the loop body (e.g., the print statement)
            Visit(expr.Body);

            // Increment counter and jump back
            var nextIdx = _builder.BuildAdd(currentIdx, one, "next_idx");
            _builder.BuildStore(nextIdx, counterAlloc).SetAlignment(8);
            _builder.BuildBr(condBlock);

            // 7. Exit the loop
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
            LLVMValueRef finalArg;
            LLVMValueRef formatStr;
            var llvmCtx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0);

            switch (type)
            {
                case IntType:
                    finalArg = valueToPrint; // keep as i64
                    formatStr = _builder.BuildGlobalStringPtr("%ld\n", "fmt_int"); // long format
                    break;

                case FloatType:
                    finalArg = valueToPrint; // already double
                    formatStr = _builder.BuildGlobalStringPtr("%f\n", "fmt_float");
                    break;

                case BoolType:
                    LLVMValueRef boolCond;
                    DeclareBoolStrings();

                    if (valueToPrint.TypeOf == _module.Context.DoubleType)
                    {
                        var zero = LLVMValueRef.CreateConstReal(_module.Context.DoubleType, 0.0);
                        boolCond = _builder.BuildFCmp(
                            LLVMRealPredicate.LLVMRealONE,
                            valueToPrint,
                            zero,
                            "boolcond");
                    }
                    else if (valueToPrint.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                    {
                        var zero = LLVMValueRef.CreateConstInt(valueToPrint.TypeOf, 0);
                        boolCond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, valueToPrint, zero, "boolcond");
                    }
                    else
                    {
                        boolCond = valueToPrint; // already i1
                    }

                    var selectedStr = _builder.BuildSelect(boolCond, _trueStr, _falseStr, "boolstr");

                    var printfTypeBool = LLVMTypeRef.CreateFunction(
                        llvmCtx.Int32Type,
                        new[] { i8Ptr },
                        true);

                    // Call printf for boolean
                    return _builder.BuildCall2(printfTypeBool, _printf, new[] { selectedStr }, "print_bool");


                case StringType:
                    // valueToPrint should already be a pointer to i8
                    finalArg = valueToPrint;
                    formatStr = _builder.BuildGlobalStringPtr("%s\n", "fmt_str");
                    break;

                case ArrayType:
                    // Arrays are stored as i8* with a length header (i64) at index 0.
                    // Print a simple representation (length) to avoid treating the raw bytes as a C-string.
                    var arrPtr = _builder.BuildBitCast(valueToPrint, LLVMTypeRef.CreatePointer(llvmCtx.Int64Type, 0), "arr_len_ptr");
                    var arrLen = _builder.BuildLoad2(llvmCtx.Int64Type, arrPtr, "arr_len");
                    finalArg = arrLen;
                    formatStr = _builder.BuildGlobalStringPtr("Array(len=%ld)\n", "fmt_array");
                    break;

                default:
                    throw new Exception("Unsupported type for printing");
            }

            // Call printf with the correct format and value
            var printfType = LLVMTypeRef.CreateFunction(
                llvmCtx.Int32Type,
                new[] { i8Ptr, i8Ptr },
                true
            );

            // Print the value
            return _builder.BuildCall2(printfType, _printf, new[] { formatStr, finalArg }, "printcall");
        }

        public LLVMValueRef VisitPrintExpr(PrintNodeExpr expr)
        {
            var valueToPrint = Visit(expr.Expression);

            return AddImplicitPrint(valueToPrint, expr.Expression.Type);
        }

        public LLVMValueRef VisitWhereExpr(WhereNodeExpr expr)
        {
            // 1️ Evaluate the array expression
            var inputArray = Visit(expr.ArrayNodeExpr); // LLVMValueRef

            // 2️ Create the result array
            var resultArray = _arrayHelper.CreateArray(); // LLVMValueRef

            // 3️ Get array length
            var arrayLen = _arrayHelper.ArrayLength(inputArray);

            // 4️ Create loop index i = 0
            var i = _builder.BuildAlloca(_module.Context.Int64Type, "i");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 0), i);

            // 5️ Create basic blocks
            var loopBlock = _module.Context.AppendBasicBlock(_builder.InsertBlock.Parent, "where.loop");
            var bodyBlock = _module.Context.AppendBasicBlock(_builder.InsertBlock.Parent, "where.body");
            var afterBlock = _module.Context.AppendBasicBlock(_builder.InsertBlock.Parent, "where.after");

            // 6️ Branch to loop
            _builder.BuildBr(loopBlock);

            // 7️ Loop block
            _builder.PositionAtEnd(loopBlock);
            var currentIndex = _builder.BuildLoad2(_module.Context.Int64Type, i, "i.load");
            var cond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntULT, currentIndex, arrayLen, "loopcond");
            _builder.BuildCondBr(cond, bodyBlock, afterBlock);

            // 8️ Body block
            _builder.PositionAtEnd(bodyBlock);

            // 8a️⃣ Get element at index i
            var element = _arrayHelper.ArrayGetValue(inputArray, currentIndex);

            // 8b️⃣ Bind iterator variable (e.g., "x") to element in context
            _context.Add(expr.IteratorName, null, element);

            // 8c️⃣ Evaluate the condition
            var condVal = Visit(expr.Condition); // LLVMValueRef (i1)

            // 8d️⃣ If condition is true, push element to result array
            var pushBlock = _module.Context.AppendBasicBlock(_builder.InsertBlock.Parent, "where.push");
            var skipBlock = _module.Context.AppendBasicBlock(_builder.InsertBlock.Parent, "where.skip");

            _builder.BuildCondBr(condVal, pushBlock, skipBlock);

            // Push block
            _builder.PositionAtEnd(pushBlock);
            _arrayHelper.ArrayPushValue(resultArray, element);
            _builder.BuildBr(skipBlock);

            // Skip block
            _builder.PositionAtEnd(skipBlock);

            // 8e️⃣ Increment i
            var nextIndex = _builder.BuildAdd(currentIndex,
                LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 1),
                "i.next");
            _builder.BuildStore(nextIndex, i);

            // Branch back to loop
            _builder.BuildBr(loopBlock);

            // 9️ After loop
            _builder.PositionAtEnd(afterBlock);

            // 10️ Return result array
            return resultArray;
        }

        // public LLVMValueRef VisitWhereExpr2(WhereNodeExpr expr)
        // {
        //     var input = Visit(expr.ArrayNodeExpr);

        //     var result = _arrayHelper.CreateArray();

        //     var len = ArrayLen(input);

        //     for (int i = 0; i < len; i++)
        //     {
        //         var elem = ArrayGet(input, i);

        //         _context.Add(expr.IteratorName, elem);

        //         var cond = Visit(expr.Condition);
        //         _builder.BuildCall2(_arrayPush.TypeOf.ElementType, _arrayPush, new[] { arr, value }, "");
        //         _builder.BuildCondBr(cond, ArrayPush(result, elem));

        //         if (cond)
        //             ArrayPush(result, elem);
        //     }

        //     return result;
        //     // var ctx = _module.Context;
        //     // // Create a new array

        //     // // Push a value
        //     // _arrayHelper.ArrayPushValue(arr, LLVMValueRef.CreateConstInt(ctx.Int64Type, 42));

        //     // // Get a value
        //     // var val = _arrayHelper.ArrayGetValue(arr, LLVMValueRef.CreateConstInt(ctx.Int64Type, 0));

        //     // // Get length
        //     // var len = _arrayHelper.ArrayLength(arr);

        //     // return arr;
        //     // var arrayVal = Visit(expr.ArrayNodeExpr);

        //     // var assignedVal = new AssignNodeExpr("x", new NumberNodeExpr(0));
        //     // var elements = new List<ExpressionNodeExpr>();

        //     // var sequence = new SequenceNodeExpr();


        //     // var ifVal = new IfNodeExpr(expr.Condition, );
        //     // sequence.Statements.Add(expr.Condition);
        //     // // do the for loop and return an array

        //     // ForLoopNodeExpr forLoopNodeExpr = new ForLoopNodeExpr(
        //     //     assignedVal, 
        //     //     new ComparisonNodeExpr(new NumberNodeExpr(0), "<", new NumberNodeExpr(expr.ArrayNodeExpr.Elements.Count)),
        //     //     new IncrementNodeExpr("x"),
        //     //     sequence); // body

        //     // var array = new ArrayNodeExpr(elements);

        //     // return Visit(forLoopNodeExpr);
        // }

        [UnmanagedCallersOnly(EntryPoint = "array_new")]
        public static IntPtr ArrayNew()
        {
            var arr = new RuntimeArray
            {
                Length = 0,
                Capacity = 4,
                Data = Marshal.AllocHGlobal(sizeof(long) * 4)
            };

            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<RuntimeArray>());
            Marshal.StructureToPtr(arr, ptr, false);

            return ptr;
        }

        [UnmanagedCallersOnly(EntryPoint = "array_push")]
        public static void ArrayPush(IntPtr arrPtr, long value)
        {
            var arr = Marshal.PtrToStructure<RuntimeArray>(arrPtr);

            if (arr.Length >= arr.Capacity)
            {
                arr.Capacity *= 2;
                arr.Data = Marshal.ReAllocHGlobal(arr.Data, (IntPtr)(sizeof(long) * arr.Capacity));
            }

            Marshal.WriteInt64(arr.Data, (int)(arr.Length * sizeof(long)), value);

            arr.Length++;

            Marshal.StructureToPtr(arr, arrPtr, false);
        }

        [UnmanagedCallersOnly(EntryPoint = "array_get")]
        public static long ArrayGet(IntPtr arrPtr, long index)
        {
            var arr = Marshal.PtrToStructure<RuntimeArray>(arrPtr);

            return Marshal.ReadInt64(arr.Data, (int)(index * sizeof(long)));
        }

        [UnmanagedCallersOnly(EntryPoint = "array_len")]
        public static long ArrayLen(IntPtr arrPtr)
        {
            var arr = Marshal.PtrToStructure<RuntimeArray>(arrPtr);
            return arr.Length;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RuntimeArray
        {
            public long Length;
            public long Capacity;
            public IntPtr Data;
        }

        private void PrintArray(LLVMValueRef array)
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i32 = ctx.Int32Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // Get length
            var len = _arrayHelper.ArrayLength(array);

            // Loop index
            var idx = _builder.BuildAlloca(i64, "i");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(i64, 0), idx);

            // Blocks
            var loopBlock = _module.Context.AppendBasicBlock(_builder.InsertBlock.Parent, "print.loop");
            var bodyBlock = _module.Context.AppendBasicBlock(_builder.InsertBlock.Parent, "print.body");
            var afterBlock = _module.Context.AppendBasicBlock(_builder.InsertBlock.Parent, "print.after");

            _builder.BuildBr(loopBlock);

            // Loop condition
            _builder.PositionAtEnd(loopBlock);
            var current = _builder.BuildLoad2(i64, idx, "i.load");
            var cond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntULT, current, len, "loop.cond");
            _builder.BuildCondBr(cond, bodyBlock, afterBlock);

            // Body
            _builder.PositionAtEnd(bodyBlock);
            var elem = _arrayHelper.ArrayGetValue(array, current);

            // Print element
            var fmt = _builder.BuildGlobalStringPtr("%ld ", "fmt_int");
            var printfType = LLVMTypeRef.CreateFunction(ctx.Int32Type, new[] { i8Ptr }, true);
            _builder.BuildCall2(printfType, _printf, new[] { fmt, elem }, "print_elem");

            // Increment index
            var next = _builder.BuildAdd(current, LLVMValueRef.CreateConstInt(i64, 1), "i.next");
            _builder.BuildStore(next, idx);

            _builder.BuildBr(loopBlock);

            // After loop
            _builder.PositionAtEnd(afterBlock);
            // Print newline
            var fmtNewline = _builder.BuildGlobalStringPtr("\n", "fmt_newline");
            _builder.BuildCall2(printfType, _printf, new[] { fmtNewline }, "newline");
        }


        public LLVMValueRef VisitArrayExpr(ArrayNodeExpr expr)
        {
            var ctx = _module.Context;
            uint count = (uint)expr.Elements.Count;

            // Allocate space for elements + 1 extra slot for the length header
            var size = LLVMValueRef.CreateConstInt(ctx.Int64Type, (ulong)(count + 1) * 8);

            var mallocFunc = GetOrDeclareMalloc();
            var arrayPtr = _builder.BuildCall2(_mallocType, mallocFunc, new[] { size }, "arr_ptr");

            // --- Store Length Header at Index 0 ---
            var zero = LLVMValueRef.CreateConstInt(ctx.Int32Type, 0);
            var lengthPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { zero }, "len_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int64Type, (ulong)count), lengthPtr);

            // --- Store Elements starting at Index 1 ---
            for (int i = 0; i < expr.Elements.Count; i++)
            {
                var val = Visit(expr.Elements[i]);
                var boxed = BoxToI64(val);

                // offset by 1 because index 0 is the length
                var idx = LLVMValueRef.CreateConstInt(ctx.Int32Type, (ulong)(i + 1));
                var elementPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { idx }, $"idx_{i}");

                _builder.BuildStore(boxed, elementPtr).SetAlignment(8);
            }

            var elementType = expr.ElementType ?? new FloatType();
            expr.SetType(new ArrayType(elementType));

            if (_debug) Console.WriteLine("array pointer: " + arrayPtr);
            return arrayPtr;
        }

        public LLVMValueRef VisitIndexExpr(IndexNodeExpr expr)
        {
            var ctx = _module.Context;
            var func = _builder.InsertBlock.Parent;

            // 1. Visit the Array and the Index expressions
            var arrayPtr = Visit(expr.ArrayExpression);
            var indexVal = Visit(expr.IndexExpression);

            // 2. Ensure Index is i64 (convert from Double if necessary)
            if (indexVal.TypeOf == ctx.DoubleType)
                indexVal = _builder.BuildFPToSI(indexVal, ctx.Int64Type, "idx_int");

            // 3. --- BOUNDS CHECK LOGIC ---
            // Load the Length from the Header (Index 0)
            var zero = LLVMValueRef.CreateConstInt(ctx.Int32Type, 0);
            var lenPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { zero }, "len_ptr");
            var arrayLen = _builder.BuildLoad2(ctx.Int64Type, lenPtr, "array_len");

            // Check: (index < 0) OR (index >= arrayLen)
            var isNegative = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, indexVal, LLVMValueRef.CreateConstInt(ctx.Int64Type, 0), "is_neg");
            var isTooBig = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, indexVal, arrayLen, "is_too_big");
            var isInvalid = _builder.BuildOr(isNegative, isTooBig, "is_invalid");

            var failBlock = func.AppendBasicBlock("bounds.fail");
            var safeBlock = func.AppendBasicBlock("bounds.ok");
            _builder.BuildCondBr(isInvalid, failBlock, safeBlock);

            // --- FAIL BLOCK: Handle Error ---
            _builder.PositionAtEnd(failBlock);
            var errorMsg = _builder.BuildGlobalStringPtr("Runtime Error: Index Out of Bounds!\n", "err_msg");
            _builder.BuildCall2(_printfType, _printf, new[] { errorMsg }, "print_err");

            // Return a null runtime object to stop execution gracefully
            var nullReturn = LLVMValueRef.CreateConstNull(LLVMTypeRef.CreatePointer(ctx.Int8Type, 0));
            _builder.BuildRet(nullReturn);

            // --- SAFE BLOCK: Proceed with access ---
            _builder.PositionAtEnd(safeBlock);

            // 4. Adjust for Header: Actual index in memory is indexVal + 1
            var one = LLVMValueRef.CreateConstInt(ctx.Int64Type, 1);
            var actualIdx = _builder.BuildAdd(indexVal, one, "offset_idx");

            // 5. GEP2 and Load the data
            var elementPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { actualIdx }, "elem_ptr");
            var rawValue = _builder.BuildLoad2(ctx.Int64Type, elementPtr, "raw_val");
            rawValue.SetAlignment(8);

            // 6. Transform based on Type Inference
            return expr.Type switch
            {
                StringType => _builder.BuildIntToPtr(rawValue, LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), "to_str"),
                FloatType => _builder.BuildBitCast(rawValue, ctx.DoubleType, "to_float"),
                IntType => rawValue,
                BoolType => _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, rawValue, LLVMValueRef.CreateConstInt(ctx.Int64Type, 0), "to_bool"),
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
            {
                return new StringType();
            }

            // Add more cases as needed...

            // Default to None to avoid throwing for pointer/complex types we don't handle.
            return new VoidType();
        }

        public LLVMValueRef VisitIdExpr(IdNodeExpr expr)
        {
            var entry = _context.Get(expr.Name);
            if (entry == null) throw new Exception($"Variable {expr.Name} not found in context.");

            var varType = entry.Type;
            if (_debug) Console.WriteLine($"visiting variable: {expr.Name} (Type: {varType})");

            // Get the LLVM representation of the type (e.g., double, i64, or ptr)
            var llvm_type = GetLLVMType(varType);

            var module = _module;
            LLVMValueRef global = module.GetNamedGlobal(expr.Name);

            if (global.Handle == IntPtr.Zero)
            {
                // Declare external if not in current module
                global = module.AddGlobal(llvm_type, expr.Name);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage;
            }

            // Use the explicit llvm_type for the Load2 instruction
            return _builder.BuildLoad2(llvm_type, global, expr.Name + "_load");
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
                AddImplicitPrint(last, lastExpr.Type);
            }

            return last;
        }

        private LLVMValueRef Visit(NodeExpr expr)
        {
            if (_debug) Console.WriteLine("visiting: " + expr);
            return expr.Accept(this);
        }
    }
}