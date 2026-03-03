using System;
using LLVMSharp;
using System.Linq;
using System.Reflection; // for using the stopwatch!
using LLVMSharp.Interop;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualBasic;

internal static class OrcBindings
{
    private const string LibLLVM = "libLLVM-20.so"; // adjust if needed

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcCreateLLJIT(
        out IntPtr Result,
        IntPtr Options);

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcLLJITAddLLVMIRModule(
        IntPtr J,
        IntPtr JD,
        IntPtr TSM);

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcLLJITLookup(
        IntPtr J,
        out ulong Result,
        [MarshalAs(UnmanagedType.LPStr)] string Name);

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcLLJITGetMainJITDylib(
        IntPtr J);

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcCreateNewThreadSafeContext();

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcCreateNewThreadSafeModule(IntPtr Module, IntPtr Context);

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcCreateDynamicLibrarySearchGeneratorForProcess(
    out IntPtr Result,
    IntPtr GlobalPrefix);

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcJITDylibAddGenerator(
        IntPtr JD,
        IntPtr Generator);

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMGetErrorMessage(IntPtr Err);

    [DllImport(LibLLVM)]
    public static extern void LLVMDisposeErrorMessage(IntPtr ErrMsg);

    [DllImport(LibLLVM)]
    public static extern void LLVMConsumeError(IntPtr Err);
}

namespace MyCompiler
{
    public class CompilerOrc : IExpressionVisitor, ICompiler
    {
        private LLVMModuleRef _module;
        private LLVMBuilderRef _builder;
        private IntPtr _jit; // JIT execution pointer
        private LLVMValueRef _trueStr;
        private LLVMValueRef _falseStr;
        private bool _jitInitialized = false;
        private int _replCounter = 0;

        HashSet<string> _definedGlobals = new(); // wouldn't we use our context for this job?
        private Context _context;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate double MainDelegate();

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
            DeclareMalloc();
            DeclareValueStruct();
        }

        private void DeclareMalloc()
        {
            var ctx = _module.Context;
            var i64 = ctx.Int64Type;
            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var mallocType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i64 }, false);
            _module.AddFunction("malloc", mallocType);
        }
        private void DeclareValueStruct()
        {
            var ctx = _module.Context;
            var tagType = ctx.Int32Type; // Tag type
            var ptrType = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0); // Pointer to data

            // Define the value struct: tag + pointer to the data (value)
            var valueType = LLVMTypeRef.CreateStruct(new[] { tagType, ptrType }, false);
        }
        
        private void DeclareBoolStrings()
        {
            _trueStr = _builder.BuildGlobalStringPtr("true\n", "true_str");
            _falseStr = _builder.BuildGlobalStringPtr("false\n", "false_str");
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

        public object Run(NodeExpr expr, bool generateIR = false)
        {
            // Unique function name per REPL execution
            string funcName = $"__anon_expr_{_replCounter++}";

            // 1 Create a fresh context + module for this command
            var context = LLVMContextRef.Create();
            _module = context.CreateModuleWithName("repl_module");

            var builder = context.CreateBuilder();

            // 2 Create:  define double @__anon_expr_X()
            var funcType = LLVMTypeRef.CreateFunction(
                LLVMTypeRef.Double,
                Array.Empty<LLVMTypeRef>(),
                false);

            var function = _module.AddFunction(funcName, funcType);
            var entry = function.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            // 3 Generate IR for the expression
            // IMPORTANT: Visit must use THIS builder/module
            //_currentModule = _module;
            _builder = builder;
            //DeclareBoolStrings();

            LLVMValueRef resultValue = Visit(expr);

            builder.BuildRet(resultValue);

            DumpIR(_module);

            // 4 Verify IR before giving to ORC
            // if (module.Verify(LLVMVerifierFailureAction.LLVMPrintMessageAction) != 0) // this code doesn't exist
            //     throw new Exception("Invalid IR generated.");

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
                OrcBindings.LLVMOrcLLJITLookup(_jit, out addr, funcName)
            );
            //DumpIR(module);

            // 7 Call it
            var fnPtr = (IntPtr)addr;
            var del = Marshal.GetDelegateForFunctionPointer<MainDelegate>(fnPtr);

            double result = del();

            return result;
        }

        public LLVMValueRef VisitForLoopExpr(ForLoopNodeExpr expr)
        {
            System.Console.WriteLine("visiting for loop");
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

            if (global.Handle == IntPtr.Zero)
            {
                // Not defined in this module → declare external
                global = module.AddGlobal(LLVMTypeRef.Double, expr.Id);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage;
            }

            var value = _builder.BuildLoad2(LLVMTypeRef.Double, global, "inc_load");
            var newValue = _builder.BuildFSub(value, LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, 1.0), "inc_add");

            _builder.BuildStore(newValue, global);
            return newValue;
        }

        public LLVMValueRef VisitIncrementExpr(IncrementNodeExpr expr)
        {
            var module = _module;
            LLVMValueRef global = module.GetNamedGlobal(expr.Id);

            if (global.Handle == IntPtr.Zero)
            {
                // Not defined in this module → declare external
                global = module.AddGlobal(LLVMTypeRef.Double, expr.Id);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage;
            }

            var value = _builder.BuildLoad2(LLVMTypeRef.Double, global, "inc_load");
            var newValue = _builder.BuildFAdd(value, LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, 1.0), "inc_add");

            _builder.BuildStore(newValue, global);
            return newValue;
        }
        public LLVMValueRef VisitComparisonExpr(ComparisonNodeExpr expr)
        {
            var left = Visit(expr.Left);
            var right = Visit(expr.Right);

            // Ensure both are doubles for comparison
            if (left.TypeOf != LLVMTypeRef.Double) left = _builder.BuildSIToFP(left, LLVMTypeRef.Double, "l_to_d");
            if (right.TypeOf != LLVMTypeRef.Double) right = _builder.BuildSIToFP(right, LLVMTypeRef.Double, "r_to_d");

            return expr.Operator switch
            {
                "<" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, left, right, "cmptmp"),
                ">" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, left, right, "cmptmp"),
                "<=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLE, left, right, "cmptmp"),
                ">=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGE, left, right, "cmptmp"),
                "==" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, left, right, "cmptmp"),
                "!=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, left, right, "cmptmp"),
                _ => throw new Exception("Unknown operator")
            };
        }

        public LLVMValueRef VisitBinaryExpr(BinaryOpNodeExpr expr)
        {
            Console.WriteLine("visiting binary operation");

            var lhs = Visit(expr.Left); // Visit left operand
            var rhs = Visit(expr.Right); // Visit right operand

            Console.WriteLine($"lhs: {lhs} rhs: {rhs}");

            return expr.Operator switch
            {
                "+" => _builder.BuildFAdd(lhs, rhs, "faddtmp"),
                "-" => _builder.BuildFSub(lhs, rhs, "fsubtmp"),
                "*" => _builder.BuildFMul(lhs, rhs, "fmultmp"),
                "/" => _builder.BuildFDiv(lhs, rhs, "fdivtmp"),
                _ => throw new InvalidOperationException($"Math operator {expr.Operator} not supported for numeric types")
            };
        }

        public LLVMValueRef VisitNumberExpr(NumberNodeExpr expr)
        {
            Console.WriteLine("visiting number");
            return LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, expr.Value);
        }

        public LLVMValueRef VisitBooleanExpr(BooleanNodeExpr expr)
        {
            Console.WriteLine("visiting boolean");
            // Use 1.0 for True to match your Int-to-Bool promotion
            return LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, expr.Value ? 1.0 : 0.0);
        }
        // public LLVMValueRef VisitStringExpr(StringNodeExpr expr)
        // {
        //     return _builder.BuildGlobalStringPtr(expr.Value, "str");
        // }
        // public LLVMValueRef VisitFloatExpr(FloatNodeExpr expr)
        // {
        //     return LLVMValueRef.CreateConstReal(_module.Context.DoubleType, expr.Value);
        // }

        public LLVMValueRef VisitAssignExpr(AssignNodeExpr expr)
        {
            Console.WriteLine($"visiting assignment: {expr.Id}");

            var value = Visit(expr.Expression);

            // Ensure numeric values are double
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                value = _builder.BuildSIToFP(value, LLVMTypeRef.Double, "assign_cvt");

            var storageType = value.TypeOf;
            var module = _module;

            LLVMValueRef global;

            if (module.GetNamedGlobal(expr.Id) == null) // local
            {
                if (!_definedGlobals.Contains(expr.Id)) // global
                {
                    System.Console.WriteLine("we dont have the id");
                    // First definition: define global in this module
                    global = module.AddGlobal(storageType, expr.Id);
                    global.Initializer = LLVMValueRef.CreateConstReal(storageType, 0.0);
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

            _builder.BuildStore(value, global);

            // Assignment returns value
            return default;
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
            Console.WriteLine("visiting sequence");
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