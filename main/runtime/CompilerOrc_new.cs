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

        LLVMModuleRef _currentModule;

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
            _currentModule = _module;
            _builder = builder;

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

        public LLVMValueRef VisitAssignExpr(AssignNodeExpr expr)
        {
            Console.WriteLine($"visiting assignment: {expr.Id}");

            var value = Visit(expr.Expression);

            // Ensure numeric values are double
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                value = _builder.BuildSIToFP(value, LLVMTypeRef.Double, "assign_cvt");

            var storageType = value.TypeOf;
            var module = _currentModule;

            LLVMValueRef global;

            if (!_definedGlobals.Contains(expr.Id))
            {
                // First definition: define global in this module
                global = module.AddGlobal(storageType, expr.Id);
                global.Initializer = LLVMValueRef.CreateConstReal(storageType, 0.0);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage;

                _definedGlobals.Add(expr.Id);
            }
            else
            {
                // Subsequent modules: declare as external
                global = module.AddGlobal(storageType, expr.Id);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage;
            }

            _builder.BuildStore(value, global);

            // Assignment returns value
            return value;
        }
        public LLVMValueRef VisitIdExpr(IdNodeExpr expr)
        {
            Console.WriteLine($"visiting variable: {expr.Name}");

            // 1 Lookup symbol by name
            var module = _currentModule;
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