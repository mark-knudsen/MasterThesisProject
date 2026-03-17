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
        LLVMTypeRef _memmoveType;
        LLVMTypeRef _reallocType;
        private MyType _lastType; // Store the type of the last expression for auto-printing
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

        private MyType PerformSemanticAnalysis(NodeExpr expr)
        {
            var checker = new TypeChecker(_context, _debug);
            _lastType = checker.Check(expr);
            _context = checker.UpdatedContext;

            //_lastNode = GetLastExpression(expr);

            var programedResult = GetProgramResult(expr);

            if (programedResult == null) return MyType.None;
            if (programedResult is ExpressionNodeExpr exp) return exp.Type; // it says the type is int right here for round

            return MyType.None;
        }

        // Problems

        // TODO: fix the problems

        // BROKEN FUNCTIONALITY   
        // remove does not return the array, but works regardless                        
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
                var elementPtr = IntPtr.Add(dataPtr, (int)((i + 2) * 8));
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

        private LLVMValueRef BoxValue(LLVMValueRef value, MyType type)
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
                MyType.Int => (Int16)ValueTag.Int,
                MyType.Float => (Int16)ValueTag.Float,
                MyType.Bool => (Int16)ValueTag.Bool,
                MyType.String => (Int16)ValueTag.String,
                MyType.Array => (Int16)ValueTag.Array,
                _ => (Int16)ValueTag.None
            };

            LLVMValueRef dataPtr;

            // 3. Handle data allocation and storage
            if (type == MyType.String)
            {
                dataPtr = value;
            }
            else if (type == MyType.None)
            {
                // For 'None' (void), we don't need to malloc data, just use a null pointer
                dataPtr = LLVMValueRef.CreateConstPointerNull(i8Ptr);
            }

            else if (type == MyType.Int)
            {
                var malloc = GetOrDeclareMalloc();
                var size = LLVMValueRef.CreateConstInt(i64, 8);
                // Use local mallocType instead of _mallocType
                var rawMem = _builder.BuildCall2(mallocType, malloc, new[] { size }, "int_mem");
                var cast = _builder.BuildBitCast(rawMem, LLVMTypeRef.CreatePointer(i64, 0), "int_cast");
                _builder.BuildStore(value, cast).SetAlignment(8);
                dataPtr = rawMem;
            }
            else if (type == MyType.Float)
            {
                var malloc = GetOrDeclareMalloc();
                var size = LLVMValueRef.CreateConstInt(i64, 8);
                // Use local mallocType instead of _mallocType
                var rawMem = _builder.BuildCall2(mallocType, malloc, new[] { size }, "float_mem");
                var cast = _builder.BuildBitCast(rawMem, LLVMTypeRef.CreatePointer(ctx.DoubleType, 0), "float_cast");
                _builder.BuildStore(value, cast).SetAlignment(8);
                dataPtr = rawMem;
            }
            else if (type == MyType.Bool)
            {
                var malloc = GetOrDeclareMalloc();
                var size = LLVMValueRef.CreateConstInt(i64, 1);
                // Use local mallocType instead of _mallocType
                var rawMem = _builder.BuildCall2(mallocType, malloc, new[] { size }, "bool_mem");
                var cast = _builder.BuildBitCast(rawMem, LLVMTypeRef.CreatePointer(ctx.Int1Type, 0), "bool_cast");
                _builder.BuildStore(value, cast).SetAlignment(8);
                dataPtr = rawMem;
            }
            else if (type == MyType.Array)
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

        public LLVMValueRef VisitIncrementExpr(IncrementNodeExpr expr)
        {
            LLVMValueRef global = _module.GetNamedGlobal(expr.Id);

            // If not in this module, but we know it exists globally
            if (global.Handle == IntPtr.Zero && _definedGlobals.Contains(expr.Id))
            {
                LLVMTypeRef llvmType = (expr.Type == MyType.Float) ? _module.Context.DoubleType : _module.Context.Int64Type;
                global = _module.AddGlobal(llvmType, expr.Id);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage; // Extern
            }

            if (global.Handle == IntPtr.Zero)
                throw new Exception($"Variable {expr.Id} not found.");

            // Explicit type for LLVM 20
            LLVMTypeRef type = (expr.Type == MyType.Float) ? _module.Context.DoubleType : _module.Context.Int64Type;

            var oldValue = _builder.BuildLoad2(type, global, "inc_load");

            LLVMValueRef newValue;
            if (expr.Type == MyType.Int)
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
                LLVMTypeRef llvmType = (expr.Type == MyType.Float) ? _module.Context.DoubleType : _module.Context.Int64Type;
                global = _module.AddGlobal(llvmType, expr.Id);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage; // Extern
            }

            if (global.Handle == IntPtr.Zero)
                throw new Exception($"Variable {expr.Id} not found.");

            // Explicit type for LLVM 20
            LLVMTypeRef type = (expr.Type == MyType.Float) ? _module.Context.DoubleType : _module.Context.Int64Type;

            var oldValue = _builder.BuildLoad2(type, global, "dec_load");

            LLVMValueRef newValue;
            if (expr.Type == MyType.Int)
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

        private LLVMValueRef EnsureFloat(LLVMValueRef value, MyType currentType)
        {
            if (currentType == MyType.Float) return value;
            if (currentType == MyType.Int)
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
            if (expr.Operator == "+" && (leftType == MyType.String || rightType == MyType.String))
            {
                // BuildStringConcat handles Int/Float -> String conversion internally
                return BuildStringConcat(lhs, leftType, rhs, rightType);
            }

            // 3. Check for logical operations (AND/OR)
            if (expr.Operator == "&&" || expr.Operator == "||")
            {
                // Both operands must be booleans (LLVM integer type with width 1, i1)
                bool lhsIsBool = leftType == MyType.Bool;
                bool rhsIsBool = rightType == MyType.Bool;

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
            bool lhsIsInt = leftType == MyType.Int;
            bool rhsIsInt = rightType == MyType.Int;
            bool lhsIsFloat = leftType == MyType.Float;
            bool rhsIsFloat = rightType == MyType.Float;

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

            var memmoveType = LLVMTypeRef.CreateFunction(
                ctx.VoidType,
                new[] { i8Ptr, i8Ptr, ctx.Int64Type },
                false
            );

            _memmoveType = memmoveType;

            var fn = _module.GetNamedFunction("memmove");
            if (fn.Handle != IntPtr.Zero)
                return fn;

            return _module.AddFunction("memmove", memmoveType);
        }

        private LLVMTypeRef GetLLVMType(MyType type)
        {
            var ctx = _module.Context;
            return type switch
            {
                MyType.Float => ctx.DoubleType,
                MyType.Int => ctx.Int64Type,
                MyType.String => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0),
                MyType.Array => LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), // Arrays are pointers
                MyType.Bool => ctx.Int1Type,
                _ => throw new Exception($"Unsupported type: {type}")
            };
        }

        // private MyType MapLLVMTypeToMyType(LLVMTypeRef llvmType)
        // {
        //     if (llvmType.Equals(_module.Context.Int64Type))
        //         return MyType.Int;
        //     if (llvmType.Equals(_module.Context.Int32Type))
        //         return MyType.Int;

        //     if (llvmType.Equals(_module.Context.DoubleType))
        //         return MyType.Float;

        //     if (llvmType.Equals(_module.Context.Int1Type)) // boolean type
        //         return MyType.Bool;

        //     // LLVM uses i8* for both C-strings and our array representation.
        //     // Use this mapping mainly for string literal cases; array cases should be handled
        //     // based on semantic types instead of relying on LLVM type introspection.
        //     if (llvmType.Kind == LLVMTypeKind.LLVMPointerTypeKind &&
        //         llvmType.ElementType.Equals(_module.Context.Int8Type))
        //     {
        //         return MyType.String;
        //     }

        //     // Add more cases as needed...

        //     // Default to None to avoid throwing for pointer/complex types we don't handle.
        //     return MyType.None;
        // }

        private LLVMValueRef GetSprintf()
        {
            var fn = _module.GetNamedFunction("sprintf");
            if (fn.Handle != IntPtr.Zero) return fn;
            return _module.AddFunction("sprintf", LLVMTypeRef.CreateFunction(
                _module.Context.Int32Type,
                new[] { LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0), LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0) },
                true));
        }

        private LLVMValueRef BuildStringConcat(LLVMValueRef lhs, MyType lhsType, LLVMValueRef rhs, MyType rhsType)
        {
            var ctx = _module.Context;
            var malloc = GetMalloc();
            var sprintf = GetSprintf();
            var ptrType = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // 1. Helper to ensure we have an i8* for sprintf
            LLVMValueRef PrepareArg(LLVMValueRef val, MyType type)
            {
                if (type == MyType.String) return val;

                // Allocate buffer for number conversion
                var buf = _builder.BuildCall2(malloc.Type, malloc.Func, new[] { LLVMValueRef.CreateConstInt(_module.Context.Int64Type, 32) }, "num_buf");
                var fmtStr = type == MyType.Int ? "%ld" : "%g";
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

        private LLVMValueRef AddImplicitPrint(LLVMValueRef valueToPrint, MyType type)
        {
            if (_debug) Console.WriteLine("prints type: " + type);
            if (_debug) Console.WriteLine("value to print type: " + valueToPrint.TypeOf);

            LLVMValueRef finalArg;
            LLVMValueRef formatStr;
            var llvmCtx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0);

            switch (type)
            {
                case MyType.Int:
                    finalArg = valueToPrint; // keep as i64
                    formatStr = _builder.BuildGlobalStringPtr("%ld\n", "fmt_int"); // long format
                    break;

                case MyType.Float:
                    finalArg = valueToPrint; // already double
                    formatStr = _builder.BuildGlobalStringPtr("%f\n", "fmt_float");
                    break;

                case MyType.Bool:
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


                case MyType.String:
                    // valueToPrint should already be a pointer to i8
                    finalArg = valueToPrint;
                    formatStr = _builder.BuildGlobalStringPtr("%s\n", "fmt_str");
                    break;

                case MyType.Array:
                    // Arrays are stored as i8* with a length header (i64) at index 0.
                    // Print a simple representation (length) to avoid treating the raw bytes as a C-string.
                    var arrPtr = _builder.BuildBitCast(valueToPrint, LLVMTypeRef.CreatePointer(llvmCtx.Int64Type, 0), "arr_len_ptr");
                    var arrLen = _builder.BuildLoad2(llvmCtx.Int64Type, arrPtr, "arr_len");
                    arrLen.SetAlignment(8);
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
            // 1️ Allocate local variables for source and result
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

                default:
                    return expr;
            }
        }

        // public LLVMValueRef VisitWhereExpr2(WhereNodeExpr expr) // works, but probably unsafe and can't assign to itself x = x.where...
        // {
        //     // 1️ Create the target filtered array
        //     var zVarName = expr.IteratorId.Name;
        //     var zAssign = new AssignNodeExpr(zVarName, new ArrayNodeExpr(new List<ExpressionNodeExpr>()));

        //     // 2️ Initialize loop index i
        //     var indexVarName = "filtered_where_index";
        //     var initIndex = new AssignNodeExpr(indexVarName, new NumberNodeExpr(0));

        //     // 3️ For loop condition: i < array.length
        //     var loopCond = new ComparisonNodeExpr(
        //         new IdNodeExpr(indexVarName),
        //         "<",
        //         new LengthNodeExpr(expr.ArrayExpr)
        //     );

        //     // 4️ Loop step: i++
        //     var loopStep = new IncrementNodeExpr(indexVarName);

        //     // 5️ Get current element
        //     var currentElement = new IndexNodeExpr(expr.ArrayExpr, new IdNodeExpr(indexVarName));

        //     // 6️ Build if condition
        //     ExpressionNodeExpr ifCond;
        //     if (expr.Condition is ComparisonNodeExpr cmp)
        //         ifCond = new ComparisonNodeExpr(currentElement, cmp.Operator, cmp.Right);
        //     else
        //         ifCond = expr.Condition; // fallback

        //     // 7️ Add element to filtered array inside if
        //     var addNode = new AddNodeExpr(new IdNodeExpr(zVarName), currentElement);

        //     var ifBody = new SequenceNodeExpr();
        //     ifBody.Statements.Add(addNode);
        //     var ifNode = new IfNodeExpr(ifCond, ifBody);

        //     // 8️ For-loop body
        //     var loopBody = new SequenceNodeExpr();
        //     loopBody.Statements.Add(ifNode);

        //     var forLoop = new ForLoopNodeExpr(initIndex, loopCond, loopStep, loopBody);

        //     // 9️ Sequence program
        //     var program = new SequenceNodeExpr();
        //     program.Statements.Add(zAssign);
        //     program.Statements.Add(forLoop);

        //     // 10️ Return filtered array (as final expression)
        //     program.Statements.Add(new IdNodeExpr(zVarName));

        //     PerformSemanticAnalysis(program);

        //     return VisitSequenceExpr(program);
        // }

        public LLVMValueRef VisitArrayExpr(ArrayNodeExpr expr)
        {
            var ctx = _module.Context;
            uint count = (uint)expr.Elements.Count;

            // Allocate length + capacity + elements
            var slots = count + 2; // [length][capacity][elements...]
            var totalBytes = LLVMValueRef.CreateConstInt(ctx.Int64Type, (ulong)slots * 8);
            var mallocFunc = GetOrDeclareMalloc();
            var arrayPtr = _builder.BuildCall2(_mallocType, mallocFunc, new[] { totalBytes }, "arr_ptr");

            var zero64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 0);

            // Store length
            var lenPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { zero64 }, "len_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int64Type, count), lenPtr).SetAlignment(8);

            // Store capacity = count
            var capPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { LLVMValueRef.CreateConstInt(ctx.Int64Type, 1) }, "cap_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int64Type, count), capPtr).SetAlignment(8);

            // Store elements starting at index 2
            for (int i = 0; i < expr.Elements.Count; i++)
            {
                var val = Visit(expr.Elements[i]);
                var boxed = BoxToI64(val);

                var idx = LLVMValueRef.CreateConstInt(ctx.Int64Type, (ulong)(i + 2));
                var elemPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { idx }, $"idx_{i}");
                _builder.BuildStore(boxed, elemPtr).SetAlignment(8);
            }

            return arrayPtr;
        }

        public LLVMValueRef VisitIndexExpr(IndexNodeExpr expr)
        {
            var ctx = _module.Context;
            var arrayPtr = Visit(expr.ArrayExpression);
            var indexVal = Visit(expr.IndexExpression);

            // 1. Ensure Index is i64
            if (indexVal.TypeOf == ctx.DoubleType)
                indexVal = _builder.BuildFPToSI(indexVal, ctx.Int64Type, "idx_int");

            // 2. Adjust for Header: Actual index in memory is UserIndex + 1
            var two = LLVMValueRef.CreateConstInt(ctx.Int64Type, 2);
            var actualIdx = _builder.BuildAdd(indexVal, two, "offset_idx");

            // 3. GEP2 and Load
            var elementPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { actualIdx }, "elem_ptr");
            var rawValue = _builder.BuildLoad2(ctx.Int64Type, elementPtr, "raw_val");
            rawValue.SetAlignment(8);

            // 4. Transform based on Type Inference
            return expr.Type switch
            {
                MyType.String => _builder.BuildIntToPtr(rawValue, LLVMTypeRef.CreatePointer(ctx.Int8Type, 0), "to_str"),
                MyType.Float => _builder.BuildBitCast(rawValue, ctx.DoubleType, "to_float"),
                MyType.Int => _builder.BuildBitCast(rawValue, ctx.Int64Type, "to_int"), // should we even do this for int if it is the default?
                _ => rawValue
            };
        }

        public LLVMValueRef VisitAddExpr(AddNodeExpr expr)
        {
            var ctx = _module.Context;

            // Visit array and value
            var arrayPtr = Visit(expr.ArrayExpression); // i64* array
            var arrayName = ((IdNodeExpr)expr.ArrayExpression).Name;
            var value = Visit(expr.AddExpression);
            var boxed = BoxToI64(value);

            var zero64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 0);
            var one64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 1);
            var two64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 2);
            var eight64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 8);

            // Save entry block
            var entryBlock = _builder.InsertBlock;

            // Load length and capacity
            var lenPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { zero64 }, "len_ptr");
            var length = _builder.BuildLoad2(ctx.Int64Type, lenPtr, "length");
            length.SetAlignment(8);

            var capPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { one64 }, "cap_ptr");
            var capacity = _builder.BuildLoad2(ctx.Int64Type, capPtr, "capacity");
            capacity.SetAlignment(8);

            // Check if grow needed
            var isFull = _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, length, capacity, "is_full");

            var function = _builder.InsertBlock.Parent;
            var growBlock = ctx.AppendBasicBlock(function, "grow");
            var contBlock = ctx.AppendBasicBlock(function, "cont");
            _builder.BuildCondBr(isFull, growBlock, contBlock);

            // ---- grow ----
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

            // store new capacity
            var capPtr2 = _builder.BuildGEP2(ctx.Int64Type, newArrayPtr, new[] { one64 }, "cap_ptr2");
            _builder.BuildStore(newCap, capPtr2).SetAlignment(8);

            _builder.BuildBr(contBlock);

            // ---- continue ----
            _builder.PositionAtEnd(contBlock);

            // PHI node for array pointer
            var arrayPtrType = LLVMTypeRef.CreatePointer(ctx.Int64Type, 0);
            var arrayPtrPhi = _builder.BuildPhi(arrayPtrType, "array_ptr_phi");
            arrayPtrPhi.AddIncoming(
                new[] { arrayPtr, newArrayPtr },   // values
                new[] { entryBlock, growBlock },   // blocks
                2
            );

            // store new element
            var elemIndex = _builder.BuildAdd(length, two64, "elem_index"); // offset 2
            var elemPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtrPhi, new[] { elemIndex }, "elem_ptr");
            _builder.BuildStore(boxed, elemPtr).SetAlignment(8);

            // increment length
            var newLength = _builder.BuildAdd(length, one64, "new_length");
            //_builder.BuildStore(newLength, lenPtr).SetAlignment(8);

            var lenPtr2 = _builder.BuildGEP2(ctx.Int64Type, arrayPtrPhi, new[] { zero64 }, "len_ptr2");
            _builder.BuildStore(newLength, lenPtr2).SetAlignment(8);

            var global = _module.GetNamedGlobal(arrayName);

            if (global.Handle == IntPtr.Zero)
                throw new Exception($"Undefined variable {arrayName}");

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

        public LLVMValueRef VisitRemoveExpr(RemoveNodeExpr expr) // mine
        {
            var ctx = _module.Context;

            // --- Evaluate array and index ---
            var arrayPtr = Visit(expr.ArrayExpression);
            var arrayName = ((IdNodeExpr)expr.ArrayExpression).Name;
            var indexVal = Visit(expr.RemoveExpression);

            if (indexVal.TypeOf == ctx.DoubleType)
                indexVal = _builder.BuildFPToSI(indexVal, ctx.Int64Type, "idx_int");

            var zero32 = LLVMValueRef.CreateConstInt(ctx.Int32Type, 0);
            var two64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 2);
            var three64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 3);
            var one64 = LLVMValueRef.CreateConstInt(ctx.Int64Type, 1);
            var eight = LLVMValueRef.CreateConstInt(ctx.Int64Type, 8);

            // --- Load length ---
            var lenPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { zero32 }, "len_ptr");
            var length = _builder.BuildLoad2(ctx.Int64Type, lenPtr, "length");

            // --- Bounds check: index < length ---
            var inBounds = _builder.BuildICmp(LLVMIntPredicate.LLVMIntULT, indexVal, length, "in_bounds");

            var function = _builder.InsertBlock.Parent;
            var removeBlock = ctx.AppendBasicBlock(function, "do_remove");
            var skipBlock = ctx.AppendBasicBlock(function, "skip_remove");

            _builder.BuildCondBr(inBounds, removeBlock, skipBlock);

            // --- Remove block ---
            _builder.PositionAtEnd(removeBlock);

            // compute moveCount = length - index - 1
            var tmp = _builder.BuildSub(length, indexVal, "len_minus_idx");
            var moveCount = _builder.BuildSub(tmp, one64, "move_count");

            // dst = arr + (index + 2)
            var dstIndex = _builder.BuildAdd(indexVal, two64, "dst_index");
            var dstPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { dstIndex }, "dst_ptr");

            // src = arr + (index + 3)
            var srcIndex = _builder.BuildAdd(indexVal, three64, "src_index");
            var srcPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { srcIndex }, "src_ptr");

            // bytes = moveCount * 8
            var bytes = _builder.BuildMul(moveCount, eight, "move_bytes");

            // cast to i8*
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var dstCast = _builder.BuildBitCast(dstPtr, i8Ptr, "dst_cast");
            var srcCast = _builder.BuildBitCast(srcPtr, i8Ptr, "src_cast");

            // call memmove
            var memmove = GetOrDeclareMemmove();
            _builder.BuildCall2(_memmoveType, memmove, new[] { dstCast, srcCast, bytes }, "");

            // decrease length
            var newLength = _builder.BuildSub(length, one64, "new_length");
            _builder.BuildStore(newLength, lenPtr);

            // jump to skip block
            _builder.BuildBr(skipBlock);

            // --- Skip block ---
            _builder.PositionAtEnd(skipBlock);

            return arrayPtr;
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
            var zero32 = LLVMValueRef.CreateConstInt(ctx.Int32Type, 0);
            var lenPtr = _builder.BuildGEP2(ctx.Int64Type, arrayPtr, new[] { zero32 }, "len_ptr");
            var length = _builder.BuildLoad2(ctx.Int64Type, lenPtr, "length");
            length.SetAlignment(8); // make sure alignment matches allocation

            return length;
        }

        public LLVMValueRef VisitIdExpr(IdNodeExpr expr)
        {
            var entry = _context.Get(expr.Name);
            if (entry == null) throw new Exception($"Variable {expr.Name} not found in context.");

            var varType = entry.Type;
            if (_debug) Console.WriteLine($"visiting: variable: {expr.Name} (Type: {varType})");

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
            if (lastExpr != null && lastExpr.Type != MyType.None)
            {
                AddImplicitPrint(last, lastExpr.Type);
            }

            return last;
        }

        private LLVMValueRef Visit(NodeExpr expr)
        {
            if (_debug)
            {
                //Console.WriteLine("visiting: " + expr);
                var name = expr.GetType().Name;
                Console.WriteLine("visiting code-gen: " + name.Substring(0, name.Length - 8));
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
    }
}