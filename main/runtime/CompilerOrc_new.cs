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

namespace MyCompiler
{
    public struct RuntimeValue
    {
        public int tag; // should this be changed to Int64?
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
        private MyType _lastType; // Store the type of the last expression for auto-printing
        private NodeExpr _lastNode; // Store the last expression for auto-printing
        private LLVMTypeRef _runtimeValueType;
        HashSet<string> _definedGlobals = new(); // wouldn't we use our context for this job?
        private Context _context;
        private LLVMTypeRef _mallocType;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate RuntimeValue MainDelegate();

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
                LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
                new[] { LLVMTypeRef.Int64 }
            );

            return _module.AddFunction("malloc", _mallocType);
        }

        private LLVMValueRef BoxToI64(LLVMValueRef value)
        {
            if (value.TypeOf == LLVMTypeRef.Int64) return value;

            if (value.TypeOf == LLVMTypeRef.Double)
                return _builder.BuildBitCast(value, LLVMTypeRef.Int64, "num_to_i64");

            if (value.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                return _builder.BuildPtrToInt(value, LLVMTypeRef.Int64, "ptr_to_i64");

            return _builder.BuildZExt(value, LLVMTypeRef.Int64, "zext");
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
                new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Double, 0) },
                true); // varargs

            _printf = _module.AddFunction("printf", _printfType);
        }

        private LLVMValueRef CreateFormatString(string format)
        {
            var str = _builder.BuildGlobalStringPtr(format, "fmt");
            return str;
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
            var checker = new TypeChecker(_context);
            _lastType = checker.Check(expr);
            _context = checker.UpdatedContext;

            _lastNode = GetLastExpression(expr);

            return _lastNode is ExpressionNodeExpr exp ? exp.Type : MyType.None;
        }



        // Problems

        // TODO: fix the problems

        // can't increment              
        // can't get output from x++               x=2; x++
        // can't used assigned string              x="harry"; x
        // can't print booleans                    print(5<4) | print(true)
        // can't use numbers if assigned as ints   x=5; x+2


        // create a orc unit test


        void CreateMain()
        {
            _funcName = $"main_{_replCounter++}";

            // 1 Create a fresh context + module for this command
            var context = LLVMContextRef.Create();
            _module = context.CreateModuleWithName("repl_module");

            var builder = context.CreateBuilder();

            // 2 Create:  define double @__anon_expr_X()
            var funcType = LLVMTypeRef.CreateFunction( // the integration test does not like that the return type is a struct, it cant run
                _runtimeValueType,
                Array.Empty<LLVMTypeRef>(),
                false);

            var function = _module.AddFunction(_funcName, funcType);
            var entry = function.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            // 3 Generate IR for the expression
            // IMPORTANT: Visit must use THIS builder/module
            _builder = builder;
        }


        public object Run(NodeExpr expr, bool generateIR = false)
        {
            // 1. Semantic analysis
            var prediction = PerformSemanticAnalysis(expr);

            CreateMain();
            DeclarePrintf();

            LLVMValueRef resultValue = Visit(expr);

            Console.WriteLine("LLVM TYPE: " + resultValue.TypeOf);
            Console.WriteLine("LANG TYPE: " + prediction);

            if (_lastNode is ExpressionNodeExpr)
            {
                var boxed = BoxValue(resultValue, prediction);
                _builder.BuildRet(boxed);
            }
            else
                _builder.BuildRet(_runtimeValueType.Undef);

            DumpIR(_module);

            // 5 Wrap in ThreadSafeModule
            var tsc = OrcBindings.LLVMOrcCreateNewThreadSafeContext();
            var tsm = OrcBindings.LLVMOrcCreateNewThreadSafeModule(_module.Handle, tsc);

            var dylib = OrcBindings.LLVMOrcLLJITGetMainJITDylib(_jit);

            ThrowIfError(
                OrcBindings.LLVMOrcLLJITAddLLVMIRModule(_jit, dylib, tsm)
            );

            // 6 Lookup function pointer
            ulong addr;
            ThrowIfError(
                OrcBindings.LLVMOrcLLJITLookup(_jit, out addr, _funcName)
            );
            //DumpIR(module);

            // 7 Call it
            var fnPtr = (IntPtr)addr; // the integration test fails here for some reason
            var delegateResult = Marshal.GetDelegateForFunctionPointer<MainDelegate>(fnPtr);

            var result = delegateResult();

            switch ((ValueTag)result.tag)
            {
                case ValueTag.Int:
                    return Marshal.ReadInt64(result.data);

                case ValueTag.Float:
                    return Marshal.PtrToStructure<double>(result.data);

                case ValueTag.String:
                    return Marshal.PtrToStringAnsi(result.data);

                case ValueTag.Bool:
                    long b = Marshal.ReadByte(result.data);
                    return b != 0;

                case ValueTag.Array:
                    return HandleArray(result.data);

                case ValueTag.None:
                    return default;

            }
            return result;
        }

        private object HandleArray(IntPtr dataPtr)
        {
            // Assuming you know the number of elements, or retrieve it somehow
            int elementCount = 3;  // Replace with actual logic for determining array size
            List<double> elements = new List<double>();

            for (int i = 0; i < elementCount; i++)
            {
                IntPtr elementPtr = new IntPtr(dataPtr.ToInt64() + i * 8);  // 8 bytes for double
                double element = Marshal.PtrToStructure<double>(elementPtr);
                elements.Add(element);
            }

            return elements;
        }

        private LLVMValueRef BoxValue(LLVMValueRef value, MyType type)
        {
            int tag = type switch
            {
                MyType.Int => (int)ValueTag.Int,
                MyType.Float => (int)ValueTag.Float,
                MyType.Bool => (int)ValueTag.Bool,
                MyType.String => (int)ValueTag.String,
                MyType.Array => (int)ValueTag.Array,
                _ => (int)ValueTag.None
            };

            var ctx = _module.Context;
            LLVMValueRef dataPtr;

            var kind = value.TypeOf.Kind;

            if (kind == LLVMTypeKind.LLVMDoubleTypeKind)
            {
                var malloc = GetOrDeclareMalloc();

                var size = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 8);

                var rawMem = _builder.BuildCall2(
                    _mallocType,
                    malloc,
                    new[] { size },
                    "num_mem");

                var cast = _builder.BuildBitCast(
                    rawMem,
                    LLVMTypeRef.CreatePointer(LLVMTypeRef.Double, 0),
                    "num_cast");

                var store = _builder.BuildStore(value, cast);
                store.SetAlignment(8);

                dataPtr = rawMem;
            }
            else if (kind == LLVMTypeKind.LLVMPointerTypeKind)
            {
                dataPtr = _builder.BuildBitCast(
                    value,
                    LLVMTypeRef.CreatePointer(ctx.Int8Type, 0),
                    "ptrcast");
            }
            else if (kind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                var malloc = GetOrDeclareMalloc();
                var size = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 8);
                var rawMem = _builder.BuildCall2(_mallocType, malloc, new[] { size }, "int_mem");
                var cast = _builder.BuildBitCast(rawMem, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int64, 0), "int_cast");
                var store = _builder.BuildStore(value, cast);
                store.SetAlignment(8);
                dataPtr = rawMem;
            }
            else if (type == MyType.Bool) // this might have to check with kind, but works 
            {
                var malloc = GetOrDeclareMalloc();
                var size = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 1); // just 1 byte
                var rawMem = _builder.BuildCall2(_mallocType, malloc, new[] { size }, "bool_mem");
                var cast = _builder.BuildBitCast(rawMem, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int1, 0), "bool_cast");
                var store = _builder.BuildStore(value, cast);
                store.SetAlignment(8);

                dataPtr = rawMem;
            }
            else
            {
                throw new Exception($"Unsupported LLVM type in BoxValue: {value.TypeOf}");
            }

            var boxed = _runtimeValueType.Undef;

            boxed = _builder.BuildInsertValue(
                boxed,
                LLVMValueRef.CreateConstInt(ctx.Int32Type, (ulong)tag),
                0,
                "with_tag");

            boxed = _builder.BuildInsertValue(
                boxed,
                dataPtr,
                1,
                "with_data");

            return boxed;
        }

        private NodeExpr GetLastExpression(NodeExpr expr)
        {
            if (expr is SequenceNodeExpr seq)
            {
                var last = seq.Statements.LastOrDefault();
                // If the sequence is empty, return the sequence itself; 
                // otherwise, recurse into the last statement.
                return last != null ? GetLastExpression(last) : expr;
            }

            return expr;
        }

        public LLVMValueRef VisitForLoopExpr(ForLoopNodeExpr expr)
        {
            var func = _builder.InsertBlock.Parent;
            //var llvmCtx = _module.Context;

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
            if (condition.TypeOf == LLVMTypeRef.Double)
            {
                condition = _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE,
                    condition, LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, 0.0), "fortest_dbl");
            }
            // If it's an i64 or i32, use ICmp
            else if (condition.TypeOf != LLVMTypeRef.Int1)
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

        public LLVMValueRef VisitDecrementExpr(DecrementNodeExpr expr)
        {
            var module = _module;
            LLVMValueRef global = module.GetNamedGlobal(expr.Id);

            var value = _builder.BuildLoad2(global.TypeOf.ElementType, global, "dec_load");

            LLVMValueRef newValue;
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                newValue = _builder.BuildSub(value, LLVMValueRef.CreateConstInt(value.TypeOf, 1), "dec_sub");
            else
                newValue = _builder.BuildFSub(value, LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, 1.0), "dec_sub");


            var store = _builder.BuildStore(newValue, global);
            store.SetAlignment(8);
            return newValue;
        }

        public LLVMValueRef VisitIncrementExpr(IncrementNodeExpr expr)
        {
            var module = _module;
            LLVMValueRef global = module.GetNamedGlobal(expr.Id);
            var value = _builder.BuildLoad2(global.TypeOf.ElementType, global, "inc_load");

            LLVMValueRef newValue;
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                newValue = _builder.BuildAdd(value, LLVMValueRef.CreateConstInt(value.TypeOf, 1), "inc_add");
            else
                newValue = _builder.BuildFAdd(value, LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, 1.0), "inc_add");

            var store = _builder.BuildStore(newValue, global);
            store.SetAlignment(8);
            return newValue;
        }

        public LLVMValueRef VisitComparisonExpr(ComparisonNodeExpr expr)
        {
            var left = Visit(expr.Left);
            var right = Visit(expr.Right);

            // Determine numeric integers only (ignore i1)
            bool lhsIsInt = left.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && left.TypeOf.IntWidth > 1;
            bool rhsIsInt = right.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && right.TypeOf.IntWidth > 1;

            // Case 1: both integers → ICmp
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

            // Case 2: at least one is float/double → promote integers (not booleans) to double
            if (lhsIsInt)
                left = _builder.BuildSIToFP(left, LLVMTypeRef.Double, "int2double");
            if (rhsIsInt)
                right = _builder.BuildSIToFP(right, LLVMTypeRef.Double, "int2double");

            // Case 3: boolean comparison? Only allow equality/inequality
            if (left.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && left.TypeOf.IntWidth == 1 &&
                right.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && right.TypeOf.IntWidth == 1)
            {
                return expr.Operator switch
                {
                    "==" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, left, right, "bool_eq"),
                    "!=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, left, right, "bool_ne"),
                    _ => throw new Exception("Cannot order boolean values with < > <= >=")
                };
            }

            // Case 4: both are doubles → FCmp
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

        public LLVMValueRef VisitComparisonExpr2(ComparisonNodeExpr expr)
        {
            var left = Visit(expr.Left);
            var right = Visit(expr.Right);

            bool lhsIsInt = left.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && left.TypeOf.IntWidth > 1;
            bool rhsIsInt = right.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && right.TypeOf.IntWidth > 1;

            // Case 1: both integers → ICmp
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

            // Case 2: at least one number is float/double → promote ints only
            if (lhsIsInt)
                left = _builder.BuildSIToFP(left, LLVMTypeRef.Double, "int2double");
            if (rhsIsInt)
                right = _builder.BuildSIToFP(right, LLVMTypeRef.Double, "int2double");

            // Now both are double → FCmp
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

        //         public LLVMValueRef VisitComparisonExpr(ComparisonNodeExpr expr)
        // {
        //     var left = Visit(expr.Left);
        //     var right = Visit(expr.Right);

        //     if (left.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind &&
        //   right.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind &&
        //   left.TypeOf.IntWidth == 1 && right.TypeOf.IntWidth == 1)
        //     {
        //         return expr.Operator switch
        //         {
        //             "==" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, left, right, "cmp"),
        //             "!=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, left, right, "cmp"),
        //             _ => throw new Exception("Unsupported boolean comparison")
        //         };
        //     }
        //     else
        //     {
        //         // Promote ints to double if needed
        //         // if (left.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
        //         //     left = _builder.BuildSIToFP(left, LLVMTypeRef.Double, "l_to_d");
        //         // if (right.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
        //         //     right = _builder.BuildSIToFP(right, LLVMTypeRef.Double, "r_to_d");

        //         return expr.Operator switch
        //         {
        //             "<" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, left, right, "cmp"),
        //             ">" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, left, right, "cmp"),
        //             "<=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLE, left, right, "cmp"),
        //             ">=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGE, left, right, "cmp"),
        //             "==" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, left, right, "cmp"),
        //             "!=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, left, right, "cmp"),
        //             _ => throw new Exception("Unknown operator")
        //         };
        //     }
        // }

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
                elseValue = LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, 0);

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
                return _builder.BuildSIToFP(value, LLVMTypeRef.Double, "cast_tmp");

            return value; // Hope for the best, or throw an error
        }

        public LLVMValueRef VisitRoundExpr(RoundNodeExpr expr)
        {
            var val = EnsureFloat(Visit(expr.Value), expr.Value.Type);
            var decimals = EnsureFloat(Visit(expr.Decimals), expr.Decimals.Type);

            var doubleType = LLVMTypeRef.Double;
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
            var lhs = Visit(expr.Left);
            var rhs = Visit(expr.Right);

            bool lhsIsInt = lhs.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind;
            bool rhsIsInt = rhs.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind;

            if (lhsIsInt && rhsIsInt)
            {
                // Both integers → integer arithmetic
                return expr.Operator switch
                {
                    "+" => _builder.BuildAdd(lhs, rhs, "addtmp"),
                    "-" => _builder.BuildSub(lhs, rhs, "subtmp"),
                    "*" => _builder.BuildMul(lhs, rhs, "multmp"),
                    "/" => _builder.BuildSDiv(lhs, rhs, "divtmp"), // signed div
                    _ => throw new InvalidOperationException($"Unsupported operator {expr.Operator}")
                };
            }
            else
            {
                // Promote any int operand to double
                if (lhsIsInt)
                    lhs = _builder.BuildSIToFP(lhs, LLVMTypeRef.Double, "int2double");
                if (rhsIsInt)
                    rhs = _builder.BuildSIToFP(rhs, LLVMTypeRef.Double, "int2double");

                // Floating point arithmetic
                return expr.Operator switch
                {
                    "+" => _builder.BuildFAdd(lhs, rhs, "faddtmp"),
                    "-" => _builder.BuildFSub(lhs, rhs, "fsubtmp"),
                    "*" => _builder.BuildFMul(lhs, rhs, "fmultmp"),
                    "/" => _builder.BuildFDiv(lhs, rhs, "fdivtmp"),
                    _ => throw new InvalidOperationException($"Unsupported operator {expr.Operator}")
                };
            }
        }

        public LLVMValueRef VisitStringExpr(StringNodeExpr expr)
        {
            return _builder.BuildGlobalStringPtr(expr.Value, "str");
        }
        public LLVMValueRef VisitNumberExpr(NumberNodeExpr expr)
        {
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, (ulong)expr.Value);
        }
        public LLVMValueRef VisitBooleanExpr(BooleanNodeExpr expr)
        {
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, expr.Value ? 1UL : 0UL);
        }
        public LLVMValueRef VisitFloatExpr(FloatNodeExpr expr)
        {
            return LLVMValueRef.CreateConstReal(_module.Context.DoubleType, expr.Value);
        }

        public LLVMValueRef VisitAssignExpr(AssignNodeExpr expr)
        {
            Console.WriteLine($"visiting assignment: {expr.Id}");

            var value = Visit(expr.Expression);

            // Ensure numeric values are double // why are we doing this?
            // if (value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
            //     value = _builder.BuildSIToFP(value, LLVMTypeRef.Double, "assign_cvt");

            var storageType = value.TypeOf;
            var module = _module;

            LLVMValueRef global;

            if (module.GetNamedGlobal(expr.Id) == null) // local
            {
                if (!_definedGlobals.Contains(expr.Id)) // global
                {
                    //Console.WriteLine("we dont have the id");
                    // First definition: define global in this module
                    global = module.AddGlobal(storageType, expr.Id);
                    if (value.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
                        global.Initializer = LLVMValueRef.CreateConstReal(value.TypeOf, 0.0);
                    else if (value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                        global.Initializer = LLVMValueRef.CreateConstInt(value.TypeOf, 0);
                    else
                        global.Initializer = LLVMValueRef.CreateConstNull(value.TypeOf);
                    global.Linkage = LLVMLinkage.LLVMExternalLinkage;

                    _definedGlobals.Add(expr.Id);
                }
                else
                {
                    global = module.AddGlobal(storageType, expr.Id);
                    global.Linkage = LLVMLinkage.LLVMExternalLinkage;
                }
            }
            else
            {
                global = module.GetNamedGlobal(expr.Id);
            }

            var store = _builder.BuildStore(value, global);
            store.SetAlignment(8);

            // Assignment returns value
            return value;
        }

        public LLVMValueRef VisitRandomExpr(RandomNodeExpr expr)
        {
            var llvmCtx = _module.Context;
            var i32 = llvmCtx.Int32Type;

            // 1. Get or declare rand()
            var randFunc = _module.GetNamedFunction("rand");
            if (randFunc.Handle == IntPtr.Zero)
            {
                var randType = LLVMTypeRef.CreateFunction(i32, Array.Empty<LLVMTypeRef>(), false);
                randFunc = _module.AddFunction("rand", randType);
            }

            var randValue = _builder.BuildCall2(
                LLVMTypeRef.CreateFunction(i32, Array.Empty<LLVMTypeRef>()),
                randFunc, Array.Empty<LLVMValueRef>(), "randcall");

            if (expr.MinValue != null && expr.MaxValue != null)
            {
                var minVal = Visit(expr.MinValue);
                var maxVal = Visit(expr.MaxValue);

                // Ensure we are working with Integers for rand math
                if (minVal.TypeOf == LLVMTypeRef.Double) minVal = _builder.BuildFPToSI(minVal, i32, "min_i");
                if (maxVal.TypeOf == LLVMTypeRef.Double) maxVal = _builder.BuildFPToSI(maxVal, i32, "max_i");

                // --- THE "VISIT IF" STYLE ---
                var func = _builder.InsertBlock.Parent;
                var thenBB = func.AppendBasicBlock("rand.correct"); // min <= max
                var elseBB = func.AppendBasicBlock("rand.swap");    // min > max
                var mergeBB = func.AppendBasicBlock("rand.cont");

                // Create a local variable to store the result (since blocks can't "return")
                var resultPtr = _builder.BuildAlloca(i32, "rand_result_ptr");

                var cond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, minVal, maxVal, "order_check");
                _builder.BuildCondBr(cond, thenBB, elseBB);

                // "Then" Part (Correct Order)
                _builder.PositionAtEnd(thenBB);
                var diff1 = _builder.BuildSub(maxVal, minVal, "diff1");
                var range1 = _builder.BuildAdd(diff1, LLVMValueRef.CreateConstInt(i32, 1), "range1");
                var res1 = _builder.BuildAdd(_builder.BuildSRem(randValue, range1, "mod1"), minVal, "res1");

                var store = _builder.BuildStore(res1, resultPtr);
                store.SetAlignment(8);

                _builder.BuildBr(mergeBB);

                // "Else" Part (Wrong Order - Swap logic)
                _builder.PositionAtEnd(elseBB);
                var diff2 = _builder.BuildSub(minVal, maxVal, "diff2");
                var range2 = _builder.BuildAdd(diff2, LLVMValueRef.CreateConstInt(i32, 1), "range2");
                var res2 = _builder.BuildAdd(_builder.BuildSRem(randValue, range2, "mod2"), maxVal, "res2");
                var store2 = _builder.BuildStore(res1, resultPtr);
                store2.SetAlignment(8);

                _builder.BuildBr(mergeBB);

                // Merge
                _builder.PositionAtEnd(mergeBB);
                var finalInt = _builder.BuildLoad2(i32, resultPtr, "final_rand_int");
                return _builder.BuildSIToFP(finalInt, LLVMTypeRef.Double, "final_rand_dbl");
            }

            return _builder.BuildSIToFP(randValue, LLVMTypeRef.Double, "rand_simple");
        }

        public LLVMValueRef VisitPrintExpr(PrintNodeExpr expr)
        {
            var valueToPrint = Visit(expr.Expression);
            //var printf = GetPrintf(); // Use your helper // declare i32 thing
            var llvmCtx = _module.Context;
            var i8Ptr = LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0);

            LLVMValueRef formatStr;
            LLVMValueRef finalArg = valueToPrint;

            // 1. STRINGS
            if (expr.Expression.Type == MyType.String)
            {
                // If it's stored as a Double (smuggled), we must extract the pointer
                if (valueToPrint.TypeOf == LLVMTypeRef.Double)
                {
                    var asInt64 = _builder.BuildBitCast(valueToPrint, llvmCtx.Int64Type, "print_smuggle_cast");
                    finalArg = _builder.BuildIntToPtr(asInt64, i8Ptr, "print_ptr_cast");
                }
                // If it's already a pointer (global string literal)
                else if (valueToPrint.TypeOf.Kind != LLVMTypeKind.LLVMPointerTypeKind)
                {
                    finalArg = _builder.BuildIntToPtr(valueToPrint, i8Ptr, "print_ptr_cast");
                }

                formatStr = _builder.BuildGlobalStringPtr("%s\n", "fmt_str");
            }
            else if (expr.Expression.Type == MyType.Bool)
            {
                LLVMValueRef boolCond;

                // If your bool is stored as double (0.0 or 1.0)
                if (valueToPrint.TypeOf == LLVMTypeRef.Double)
                {
                    var zero = LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, 0.0);
                    boolCond = _builder.BuildFCmp(
                        LLVMRealPredicate.LLVMRealONE,
                        valueToPrint,
                        zero,
                        "boolcond");
                }
                // If stored as integer
                else if (valueToPrint.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind &&
                         valueToPrint.TypeOf.IntWidth > 1)
                {
                    var zero = LLVMValueRef.CreateConstInt(valueToPrint.TypeOf, 0);
                    boolCond = _builder.BuildICmp(
                        LLVMIntPredicate.LLVMIntNE,
                        valueToPrint,
                        zero,
                        "boolcond");
                }
                else
                {
                    // already i1
                    boolCond = valueToPrint;
                }

                var selectedStr = _builder.BuildSelect(
                    boolCond,
                    _trueStr,
                    _falseStr,
                    "boolstr");

                // Use the exact signature of printf: (ptr, ...) -> i32
                var printfType2 = LLVMTypeRef.CreateFunction(
                    llvmCtx.Int32Type,
                    new[] { i8Ptr },
                    true);

                return _builder.BuildCall2(
                    printfType2,
                    _printf,
                    new[] { selectedStr },
                    "print_bool");
            }
            // 2. NUMBERS
            else
            {
                // Printf %f expects a Double. If it's an i32 or i64, convert it.
                if (valueToPrint.TypeOf != LLVMTypeRef.Double)
                {
                    finalArg = _builder.BuildSIToFP(valueToPrint, LLVMTypeRef.Double, "print_to_dbl");
                }
                formatStr = _builder.BuildGlobalStringPtr("%f\n", "fmt_float");
            }

            // Use the exact signature of printf: (ptr, ...) -> i32
            var printfType = LLVMTypeRef.CreateFunction(
                llvmCtx.Int32Type,
                new[] { i8Ptr },
                true);

            return _builder.BuildCall2(printfType, _printf, new[] { formatStr, finalArg }, "printcall");
        }

        public LLVMValueRef VisitArrayExpr(ArrayNodeExpr expr)
        {
            uint count = (uint)expr.Elements.Count;
            var size = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, count * 8);

            var mallocFunc = GetOrDeclareMalloc();

            // CRITICAL FIX: Pass _mallocType directly, NOT mallocFunc.TypeOf
            var arrayPtr = _builder.BuildCall2(_mallocType, mallocFunc, new[] { size }, "arr_ptr");

            for (int i = 0; i < expr.Elements.Count; i++)
            {
                var val = Visit(expr.Elements[i]);
                var boxed = BoxToI64(val);

                var idx = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)i);
                // Ensure we index into an i64 array
                var elementPtr = _builder.BuildGEP2(LLVMTypeRef.Int64, arrayPtr, new[] { idx }, $"idx_{i}");

                var store = _builder.BuildStore(boxed, elementPtr);
                store.SetAlignment(8);

            }
            return arrayPtr;
        }
        public LLVMValueRef VisitIdExpr(IdNodeExpr expr)
        {
            Console.WriteLine($"visiting variable: {expr.Name}");

            // 1 Lookup symbol by name
            var module = _module;
            LLVMValueRef global = module.GetNamedGlobal(expr.Name);

            if (global.Handle == IntPtr.Zero)
            {
                // Not defined in this module → declare external
                global = module.AddGlobal(LLVMTypeRef.Double, expr.Name);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage;
            }

            // 2 Load value
            return _builder.BuildLoad2(LLVMTypeRef.Double, global, expr.Name);
        }

        public LLVMValueRef VisitSequenceExpr(SequenceNodeExpr expr)
        {
            LLVMValueRef last = default;

            foreach (var stmt in expr.Statements)
            {
                last = Visit(stmt);
            }

            return last;
        }

        private LLVMValueRef Visit(NodeExpr expr)
        {
            return expr.Accept(this);
        }
    }
}