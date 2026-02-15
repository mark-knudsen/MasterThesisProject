using System;
using System.Collections.Generic;
using LLVMSharp.Interop;

namespace MyCompiler
{
    public class LLVMCompiler : IVisitor<LLVMValueRef>
    {
        private LLVMContextRef _context;
        private LLVMModuleRef _module;
        private LLVMBuilderRef _builder;
        private Dictionary<string, LLVMValueRef> _namedValues = new();

        public LLVMCompiler()
        {
   
            _context = LLVMContextRef.Create();
            _module = _context.CreateModuleWithName("my_jit_module");
            _builder = _context.CreateBuilder();
        }

        public void CompileAndRun(Node root)
        {
            // 1. Setup the function using the context's types
            var returnType = _context.Int32Type; // USE CONTEXT
            var funcType = LLVMTypeRef.CreateFunction(returnType, Array.Empty<LLVMTypeRef>());
            var mainFunc = _module.AddFunction("main", funcType);
            var entry = mainFunc.AppendBasicBlock("entry");
            _builder.PositionAtEnd(entry);

            // 2. Generate the IR code
            LLVMValueRef lastValue = root.Accept(this);

            // 3. Return logic using context's types
            if (lastValue == null) 
                lastValue = LLVMValueRef.CreateConstInt(_context.Int32Type, 0); // USE CONTEXT

            _builder.BuildRet(lastValue);

            // 4. Verify (The error should disappear now)
            if (!_module.TryVerify(LLVMVerifierFailureAction.LLVMPrintMessageAction, out var message)) {
                Console.WriteLine($"LLVM Verification Error: {message}");
            }
            
            _module.Dump();
            ExecuteJIT(mainFunc);
        }

        private void ExecuteJIT(LLVMValueRef function)
        {
            LLVM.InitializeNativeTarget();
            LLVM.InitializeNativeAsmPrinter();
            LLVM.InitializeNativeAsmParser();

            if (_module.TryCreateExecutionEngine(out var engine, out var error))
            {
                var result = engine.RunFunction(function, Array.Empty<LLVMGenericValueRef>());
                // result.AsInt turns the LLVM handle back into a C# ulong/int
                Console.WriteLine($"\n[JIT Execution Success]");
                // The '32' matches our Int32 type, and 'true' means it's a signed integer
                // Failsafe version
                // Console.WriteLine($"Machine Code Output: {result.ReturnToInt(true)}");
            }
            else
            {
                Console.WriteLine($"JIT Error: {error}");
            }
        }

        // --- Visitor Methods ---

        public LLVMValueRef Visit(SequenceNode node)
        {
            LLVMValueRef last = null;
            foreach (var stmt in node.Statements)
            {
                last = stmt.Accept(this);
            }
            return last;
        }

        public LLVMValueRef Visit(NumberNode node) 
            => LLVMValueRef.CreateConstInt(_context.Int32Type, (ulong)node.Value);

        public LLVMValueRef Visit(BinaryOpNode node)
        {
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);

            return node.Operator switch
            {
                "+" => _builder.BuildAdd(left, right, "addtmp"),
                "-" => _builder.BuildSub(left, right, "subtmp"),
                "*" => _builder.BuildMul(left, right, "multmp"),
                "/" => _builder.BuildSDiv(left, right, "divtmp"), // SDiv = Signed Division
                _ => throw new Exception("Op not supported in LLVM yet")
            };
        }

        public LLVMValueRef Visit(IdNode node)
        {
            if (_namedValues.TryGetValue(node.Name, out var val)) return val;
            throw new Exception($"Undefined variable: {node.Name}");
        }

        public LLVMValueRef Visit(AssignNode node)
        {
            var val = node.Expression.Accept(this);
            _namedValues[node.Id] = val;
            return val;
        }

        // --- Stubs for the rest (to be implemented) ---
        public LLVMValueRef Visit(StringNode node) => throw new NotImplementedException("Strings are complex in LLVM!");
        public LLVMValueRef Visit(BooleanNode node) => LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, node.Value ? 1u : 0u);
        public LLVMValueRef Visit(IfNode node) => throw new NotImplementedException("Need Basic Blocks for If");
        public LLVMValueRef Visit(ForLoopNode node) => throw new NotImplementedException("Need Basic Blocks for Loops");
        public LLVMValueRef Visit(ComparisonNode node) 
            => _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, node.Left.Accept(this), node.Right.Accept(this), "cmptmp");
        public LLVMValueRef Visit(PrintNode node) => throw new NotImplementedException("Requires linking to C printf");
        public LLVMValueRef Visit(IncrementNode node) => throw new NotImplementedException();
        public LLVMValueRef Visit(DecrementNode node) => throw new NotImplementedException();
        public LLVMValueRef Visit(RandomNode node) => throw new NotImplementedException();
    }
}