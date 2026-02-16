using System;
using LLVMSharp;
using LLVMSharp.Interop;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MyCompiler 
{
    // ChatGPT code
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

        public int Run(NodeExpr expr)
        {
            // Create a fresh module
            _module = LLVMModuleRef.CreateWithName("repl_module");
            _builder = _module.Context.CreateBuilder();

            // Create a new JIT engine for this module
            _engine = _module.CreateMCJITCompiler();

            // Create a single anonymous function for this input
            var funcType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, Array.Empty<LLVMTypeRef>());
            var functionName = $"anon_expr_{Guid.NewGuid():N}";
            var function = _module.AddFunction(functionName, funcType);

            var entry = function.AppendBasicBlock("entry");
            _builder.PositionAtEnd(entry);

            // Generate instructions for the AST
            var resultValue = Visit(expr);

            _builder.BuildRet(resultValue);

            // Print the IR for debugging
            Console.WriteLine(_module.PrintToString());

            // Execute the function
            var res = _engine.RunFunction(function, Array.Empty<LLVMGenericValueRef>());
            int finalResult = (int)LLVM.GenericValueToInt(res, 0);

            return finalResult;
        }

        public LLVMValueRef VisitNumberExpr(NumberNodeExpr expr)
        {
            return LLVMValueRef.CreateConstInt(
                LLVMTypeRef.Int32,
                (ulong)expr.Value,
                false
            );
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

        public LLVMValueRef VisitSequenceExpr(SequenceNodeExpr expr)
        {
            LLVMValueRef last = default;

            foreach (var stmt in expr.Statements)
            {
                last = Visit(stmt);
            }

            return last;
        }

        public LLVMValueRef VisitAssignExpr(AssignNodeExpr expr)
        {
            var value = Visit(expr.Expression); 

            // Allocate global in LLVM if needed
            var global = _module.GetNamedGlobal(expr.Id);
            if (global.Handle == IntPtr.Zero)
            {
                global = _module.AddGlobal(LLVMTypeRef.Int32, expr.Id);
                global.Initializer = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
            }

            _builder.BuildStore(value, global);

            // Update context
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

        private LLVMValueRef Visit(NodeExpr expr)
        {
            return expr.Accept(this);
        }
    }
}