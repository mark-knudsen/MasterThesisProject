using System;
using LLVMSharp;
using LLVMSharp.Interop;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MyCompiler 
{
    public unsafe class Compiler : IExpressionVisitor
    {
        private LLVMModuleRef _module;
        private LLVMBuilderRef _builder;
        private LLVMExecutionEngineRef _engine;
        private LLVMOpaquePassBuilderOptions* _passBuilderOptions;
        private Context _context;

        public Compiler()
        {
            LLVM.InitializeNativeTarget();
            LLVM.InitializeNativeAsmPrinter();
            LLVM.InitializeNativeAsmParser();

            _module = LLVMModuleRef.CreateWithName("base");
            _builder = _module.Context.CreateBuilder();
            _engine = _module.CreateMCJITCompiler();
            _context = Context.Empty; // stores variables/functions
        }

        private void InitializeModule()
        {
            _module = LLVMModuleRef.CreateWithName("KaleidoscopeModule");
            _builder = _module.Context.CreateBuilder();
            _passBuilderOptions = LLVM.CreatePassBuilderOptions();
        }
        public object Run(NodeExpr expr) // Work in progress, it can print a simple string, can't assign string, also breaks x++
        {
            _module = LLVMModuleRef.CreateWithName("repl_module");
            _builder = _module.Context.CreateBuilder();
            _engine = _module.CreateMCJITCompiler();

            LLVMTypeRef returnType;

            ExpressionNodeExpr lastExpr = null;

            if (expr is SequenceNodeExpr seq && seq.Statements.Count > 0)
            {
                // find the last expression in the sequence
                var last = seq.Statements.Last();
                if (last is ExpressionNodeExpr e)
                    lastExpr = e;
            }
            else if (expr is ExpressionNodeExpr e)
            {
                lastExpr = e;
            }

            returnType = lastExpr != null ? GetLLVMType(lastExpr.Type) : LLVMTypeRef.Void;

            var funcType = LLVMTypeRef.CreateFunction(returnType, Array.Empty<LLVMTypeRef>());
            var function = _module.AddFunction($"anon_expr_{Guid.NewGuid():N}", funcType);

            var entry = function.AppendBasicBlock("entry");
            _builder.PositionAtEnd(entry);

            var resultValue = Visit(expr);

            _builder.BuildRet(resultValue);

            Console.WriteLine(_module.PrintToString());

            var res = _engine.RunFunction(function, Array.Empty<LLVMGenericValueRef>());

            if (lastExpr != null)
            {
                if (lastExpr.Type == MyType.Int)
                    return (int)LLVM.GenericValueToInt(res, 0);
                else if (lastExpr.Type == MyType.String)
                {
                    var ptr = LLVM.GenericValueToPointer(res);
                    return Marshal.PtrToStringAnsi((nint)ptr);
                }
            }

            return null; // nothing to return
        }

        // public object Run(NodeExpr expr) // everything works but just not with strings
        // {
        //     // Create a fresh module
        //     _module = LLVMModuleRef.CreateWithName("repl_module");
        //     _builder = _module.Context.CreateBuilder();

        //     // Create a new JIT engine for this module
        //     _engine = _module.CreateMCJITCompiler();

        //     // Create a single anonymous function for this input
        //     LLVMTypeRef returnType;
        //     MyType exprType = MyType.None;

        //     if (expr is ExpressionNodeExpr eExpr)
        //     {
        //         returnType = GetLLVMType(eExpr.Type);
        //         exprType = eExpr.Type;
        //     }
        //     else
        //     {
        //         returnType = LLVMTypeRef.Void; // statements / functions return void
        //     }

        //     //var returnType = GetLLVMType(expr.Type);

        //     System.Console.WriteLine($"Compiling expression of type {exprType} to LLVM IR...");
        //     System.Console.WriteLine($"Return type{returnType} to LLVM IR...");

        //     var funcType = LLVMTypeRef.CreateFunction(
        //         returnType,
        //         Array.Empty<LLVMTypeRef>()
        //     );

        //     var functionName = $"anon_expr_{Guid.NewGuid():N}";
        //     var function = _module.AddFunction(functionName, funcType);

        //     var entry = function.AppendBasicBlock("entry");
        //     _builder.PositionAtEnd(entry);

        //     // Generate instructions for the AST
        //     var resultValue = Visit(expr);

        //     _builder.BuildRet(resultValue);

        //     // Print the IR for debugging
        //     Console.WriteLine(_module.PrintToString());

        //     // Execute the function
        //     var res = _engine.RunFunction(function, Array.Empty<LLVMGenericValueRef>());
        //     int finalResult = (int)LLVM.GenericValueToInt(res, 0);

        //     if (exprType == MyType.Int)
        //     {
        //         return (int)LLVM.GenericValueToInt(res, 0); // 0 = unsigned
        //     }
        //     else if (exprType == MyType.String)
        //     {
        //         var ptr = LLVM.GenericValueToPointer(res);
        //         return Marshal.PtrToStringAnsi((nint)ptr);
        //     }

        //     System.Console.WriteLine(" we using fallback for unknown type, returning int result as string");

        //     return finalResult; // fall back
        // }

        public LLVMValueRef VisitNumberExpr(NumberNodeExpr expr)
        {
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)expr.Value, false);
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

        public LLVMValueRef VisitStringExpr(StringNodeExpr expr)
        {
            return _builder.BuildGlobalStringPtr(expr.Value, "str");
        }

        public LLVMValueRef VisitBinaryExpr(BinaryOpNodeExpr expr)
        {
            var lhs = Visit(expr.Left);
            var rhs = Visit(expr.Right);

            switch (expr.Operator)
            {
                case "+": return _builder.BuildAdd(lhs, rhs, "addtmp");
                case "-": return _builder.BuildSub(lhs, rhs, "subtmp");
                case "*": return _builder.BuildMul(lhs, rhs, "multmp");
                case "/": return _builder.BuildSDiv(lhs, rhs, "divtmp");
                default: throw new InvalidOperationException("Unknown operator");
            }
        }

        public LLVMValueRef VisitAssignExpr(AssignNodeExpr expr)
        {
            var value = Visit(expr.Expression);

            var global = _module.GetNamedGlobal(expr.Id);
            if (global.Handle == IntPtr.Zero)
            {
                LLVMTypeRef llvmType = GetLLVMType(expr.Expression.Type);
                global = _module.AddGlobal(llvmType, expr.Id);

                // Initialize correctly for string vs int
                if (expr.Expression.Type == MyType.Int)
                    global.Initializer = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
                else if (expr.Expression.Type == MyType.String)
                    global.Initializer = LLVMValueRef.CreateConstNull(llvmType);
            }

            _builder.BuildStore(value, global);
            _context = _context.Add(expr.Id, global);

            return value;
        }


        public LLVMValueRef VisitIdExpr(IdNodeExpr expr)
        {
            var value = _context.Get(expr.Name);
            if (value is null)
                throw new InvalidOperationException($"Variable {expr.Name} not defined");

            return _builder.BuildLoad2(LLVMTypeRef.Int32, value.Value, expr.Name);
        }

        public LLVMValueRef VisitDecrementExpr(DecrementNodeExpr expr)
        {
            var global = _context.Get(expr.Id) ?? throw new InvalidOperationException($"Variable {expr.Id} not defined");
            var current = _builder.BuildLoad2(LLVMTypeRef.Int32, global, expr.Id);
            var decremented = _builder.BuildSub(current, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 1, false), "dec");
            _builder.BuildStore(decremented, global);
            return decremented;
        }

        public LLVMValueRef VisitIncrementExpr(IncrementNodeExpr expr)
        {
            var global = _context.Get(expr.Id) ?? throw new InvalidOperationException($"Variable {expr.Id} not defined");
            var current = _builder.BuildLoad2(LLVMTypeRef.Int32, global, expr.Id);
            var incremented = _builder.BuildAdd(current, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 1, false), "inc");
            _builder.BuildStore(incremented, global);
            return incremented;
        }

        private LLVMTypeRef GetLLVMType(MyType type)
        {
            return type switch
            {
                MyType.Int => LLVMTypeRef.Int32,
                MyType.String => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
                _ => throw new Exception("Unknown type")
            };
        }

        private LLVMValueRef Visit(NodeExpr expr)
        {
            return expr.Accept(this);
        }
    }
}