using System;
using LLVMSharp;
using LLVMSharp.Interop;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MyCompiler 
{
    // [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    // public delegate void Print(int d); // default double
    // public unsafe class Compiler : IExpressionVisitor
    // {
    //     private LLVMModuleRef _module;
    //     private LLVMBuilderRef _builder;
    //     private LLVMExecutionEngineRef _engine;
    //     private LLVMOpaquePassBuilderOptions* _passBuilderOptions;
    //     private readonly Dictionary<string, NodeExpr> _functions;
    //     private Context _context;

    //     List<LLVMValueRef> toRun = new List<LLVMValueRef>();
    
    //     public Compiler()
    //     {
    //         LLVM.InitializeNativeTarget();
    //         LLVM.InitializeNativeAsmPrinter();
    //         LLVM.InitializeNativeAsmParser();
    //         _functions = new Dictionary<string, NodeExpr>();
    //         _context = Context.Empty;
    //     }
    //     private void PutChard(int x) // default double
    //     {
    //         try
    //         {
    //             Console.Write((char)x);
    //         }
    //         catch
    //         {
    //         }
    //     }

    //     private void InitializeModule()
    //     {
    //         _module = LLVMModuleRef.CreateWithName("Kaleidoscope Module");
    //         _builder = _module.Context.CreateBuilder();
    //         _passBuilderOptions = LLVM.CreatePassBuilderOptions();

    //         _engine = _module.CreateMCJITCompiler();

    //         var ft = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, Array.Empty<LLVMTypeRef>());
    //         //var ft = LLVMTypeRef.CreateFunction(LLVMTypeRef.Double, [LLVMTypeRef.Double]); // default

    //         var write = _module.AddFunction("putchard", ft);
    //         write.Linkage = LLVMLinkage.LLVMExternalLinkage;
    //         Delegate d = new Print(PutChard);
    //         var p = Marshal.GetFunctionPointerForDelegate(d);
    //         _engine.AddGlobalMapping(write, p);
    //     }

    //     public object Run(NodeExpr expr) // default List<NodeExpr> exprs
    //     {
    //         // If we modify the module after we already executed some function with
    //         // _engine.RunFunction it will break, so for each run we instantiate the module again
    //         // any previous defined function will be emitted again in the current module

    //         InitializeModule();
    //         //var toRun = new List<LLVMValueRef>();


    //         _context = Context.Empty;
    //         var my_v = Visit(expr);

    //         // foreach (var item in exprs)
    //         // {
    //         //     _context = Context.Empty;
    //         //     var v = Visit(item);

    //         //     // Since we could have several expressions to be evaluated, we need to complete the emission of all
    //         //     // the code before running any of them, we keep track of what we need to run and then execute later in order
    //         //     // if (item is FunctionExpression { Proto.Name: "anon_expr" })
    //         //     // {
    //         //     //     toRun.Add(v);
    //         //     // }
    //         // }

    //         var passes = new MarshaledString("mem2reg,instcombine,reassociate,gvn,simplifycfg");
    //         var passesError = LLVM.RunPasses(_module, passes, _engine.TargetMachine, _passBuilderOptions);

    //         if (passesError != null)
    //         {
    //             sbyte* errorMessage = LLVM.GetErrorMessage(passesError);
    //             var span = MemoryMarshal.CreateReadOnlySpanFromNullTerminated((byte*)errorMessage);
    //             Console.WriteLine(span.AsString());
    //             return "Error";
    //         }

    //         foreach (var func in toRun)
    //         {
    //             var res = _engine.RunFunction(func, Array.Empty<LLVMGenericValueRef>());
    //             int fres = (int)LLVM.GenericValueToInt(res, 0);
    //             Console.WriteLine("> {0}", fres);
    //         }

    //         // foreach (var v in toRun) // ours
    //         // {
    //         //     var disasm = _module.PrintToString();
    //         //     Console.WriteLine(disasm);


    //         //     System.Console.WriteLine("yo we are creating a torun file 1");
    //         //     var res = _engine.RunFunction(v, Array.Empty<LLVMGenericValueRef>());
    //         //     System.Console.WriteLine("yo we are creating a torun file 2");
    //         //     //var fres = LLVMTypeRef.Double.GenericValueToFloat(res); // default
    //         //     int fres = (int)LLVMTypeRef.Int32.GenericValueToInt(res, true);
    //         //     System.Console.WriteLine("yo we are creating a torun file 3");
    //         //     Console.WriteLine("> {0}", fres);
    //         // }

    //         LLVM.DisposePassBuilderOptions(_passBuilderOptions);
    //         _builder.Dispose();
    //         _module.Dispose();

    //         return my_v;
    //     }

    //     public LLVMValueRef VisitSequenceExpr(SequenceNodeExpr expr)
    //     {
    //         var result = new LLVMValueRef();
    //         foreach (var stmt in expr.Statements)
    //         {
    //             System.Console.WriteLine("running for each statement");
    //             result = stmt.Accept(this);
    //             toRun.Add(result);
    //         }
    //         return result;
    //     }

    //     // public LLVMValueRef VisitBinaryExpr(BinaryOpNodeExpr expr) // ours
    //     // {
    //     //     var lhsVal = Visit(expr.Left);
    //     //     var rhsVal = Visit(expr.Right);

    //     //     System.Console.WriteLine("left   " + lhsVal);
    //     //     System.Console.WriteLine("right  " + rhsVal);

    //     //     var someval = BinaryVal(lhsVal, rhsVal, expr.Operator);
    //     //     toRun.Add(someval);
    //     //     return BinaryVal(lhsVal, rhsVal, expr.Operator);
    //     // }

    //     private LLVMValueRef WrapInFunction(LLVMValueRef exprValue, LLVMTypeRef returnType)
    //     {
    //         var funcType = LLVMTypeRef.CreateFunction(returnType, Array.Empty<LLVMTypeRef>());
    //         var func = _module.AddFunction("anon_expr", funcType);
    //         var entry = func.AppendBasicBlock("entry");
    //         _builder.PositionAtEnd(entry);
    //         _builder.BuildRet(exprValue);
    //         return func;
    //     }


    //     public LLVMValueRef VisitBinaryExpr(BinaryOpNodeExpr expr)
    //     {
    //         var lhs = Visit(expr.Left);
    //         var rhs = Visit(expr.Right);

    //         switch (expr.Operator)
    //         {
    //             case "+": return _builder.BuildAdd(lhs, rhs, "addtmp");
    //             case "-": return _builder.BuildSub(lhs, rhs, "subtmp");
    //             case "*": return _builder.BuildMul(lhs, rhs, "multmp");
    //             case "/": return _builder.BuildSDiv(lhs, rhs, "divtmp");
    //             default: throw new InvalidOperationException();
    //         }
    //     }

    //     private LLVMValueRef BinaryVal(LLVMValueRef lhsVal, LLVMValueRef rhsVal, string nodeType)
    //     {
    //         System.Console.WriteLine("operator " + nodeType);
    //         switch (nodeType)
    //             {
    //                 case "+":
    //                     return _builder.BuildAdd(lhsVal, rhsVal, "addtmp");
    //                 case "-":
    //                     return _builder.BuildSub(lhsVal, rhsVal, "addtmp");
    //                 case "*":
    //                     return _builder.BuildMul(lhsVal, rhsVal, "addtmp");
    //                 case "/":
    //                     return _builder.BuildSDiv(lhsVal, rhsVal, "addtmp");

    //                 default:
    //                     throw new InvalidOperationException();
    //             }
    //     }
        
    //     public LLVMValueRef VisitNumberExpr(NumberNodeExpr expr)
    //     {
    //         return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)expr.Value, false);
    //     }
    //     private LLVMValueRef Visit(NodeExpr body) => body.Accept(this);
    // }



    // ChatGPT code
    public unsafe class Compiler : IExpressionVisitor
    {
        private LLVMModuleRef _module;
        private LLVMBuilderRef _builder;
        private LLVMExecutionEngineRef _engine;
        private LLVMOpaquePassBuilderOptions* _passBuilderOptions;

        public Compiler()
        {
            LLVM.InitializeNativeTarget();
            LLVM.InitializeNativeAsmPrinter();
            LLVM.InitializeNativeAsmParser();
        }

        private void InitializeModule()
        {
            _module = LLVMModuleRef.CreateWithName("KaleidoscopeModule");
            _builder = _module.Context.CreateBuilder();
            _passBuilderOptions = LLVM.CreatePassBuilderOptions();
            _engine = _module.CreateMCJITCompiler();
        }

        public int Run(NodeExpr expr)
        {
            InitializeModule();

            var funcType = LLVMTypeRef.CreateFunction(
                LLVMTypeRef.Int32,
                Array.Empty<LLVMTypeRef>()
            );

            var function = _module.AddFunction("anon_expr", funcType);
            var entry = function.AppendBasicBlock("entry");
            _builder.PositionAtEnd(entry);

            var resultValue = Visit(expr);

            _builder.BuildRet(resultValue);

            // Optional optimization passes
            var passes = new MarshaledString("mem2reg,instcombine,reassociate,gvn,simplifycfg");
            var passError = LLVM.RunPasses(_module, passes, _engine.TargetMachine, _passBuilderOptions);

            if (passError != null)
            {
                Console.WriteLine("Pass error");
                return -1;
            }

            // Print generated IR (debug)
            Console.WriteLine(_module.PrintToString());

            // Execute function
            var res = _engine.RunFunction(function, Array.Empty<LLVMGenericValueRef>());
            int finalResult = (int)LLVM.GenericValueToInt(res, 0);

            Console.WriteLine("> {0}", finalResult);

            // Cleanup
            LLVM.DisposePassBuilderOptions(_passBuilderOptions);
            _builder.Dispose();
            _module.Dispose();

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
                case "+":
                    return _builder.BuildAdd(lhs, rhs, "addtmp");
                case "-":
                    return _builder.BuildSub(lhs, rhs, "subtmp");
                case "*":
                    return _builder.BuildMul(lhs, rhs, "multmp");
                case "/":
                    return _builder.BuildSDiv(lhs, rhs, "divtmp");
                default:
                    throw new InvalidOperationException("Unknown operator");
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

        private LLVMValueRef Visit(NodeExpr expr)
        {
            return expr.Accept(this);
        }
    }
}