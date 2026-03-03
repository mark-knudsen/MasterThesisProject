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

            // _globalsModule = LLVMModuleRef.CreateWithName("globals");
            // var globalsCtx = _globalsModule.Context;

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

        private void DumpIR()
        {
            string llvmIR = _module.PrintToString();
            Console.WriteLine("Generated LLVM IR:\n");
            Console.WriteLine(llvmIR);
            File.WriteAllText("output_actual_orc.ll", llvmIR);
        }

        private Dictionary<string, LLVMValueRef> _globalVars = new Dictionary<string, LLVMValueRef>(); // Track global variables
        string funcName = "main";

        public object Run(NodeExpr expr, bool generateIR = false)
        {
            // Create the function name and add it to the module
            //funcName = $"main_{_replCounter++}";
            CreateMainFunction(funcName);

            DumpIR(); // I believe after here the IR is even wrong
            Console.WriteLine(expr);

            // 1. Generate the IR for the expression
            System.Console.WriteLine("hi0");
            var resultValue = Visit(expr);
            System.Console.WriteLine("hi1");

            if (!createdMain)
            {
                _builder.BuildRet(resultValue);
                createdMain = true;
            }
            DumpIR();
            System.Console.WriteLine("hi2");

            // 2. Finalize the module and JIT execution
            var tsc = OrcBindings.LLVMOrcCreateNewThreadSafeContext();
            System.Console.WriteLine("hi3");
            var tsm = OrcBindings.LLVMOrcCreateNewThreadSafeModule(_module.Handle, tsc);
            DumpIR();
            System.Console.WriteLine("hi4");
            OrcBindings.LLVMOrcLLJITAddLLVMIRModule(_jit, OrcBindings.LLVMOrcLLJITGetMainJITDylib(_jit), tsm); // we fail right here, but the IR looks wrong way before
            System.Console.WriteLine("hi5");

            // 3. Lookup the function to be executed
            ulong addr;
            System.Console.WriteLine("hi6");
            DumpIR();
            ThrowIfError(OrcBindings.LLVMOrcLLJITLookup(_jit, out addr, funcName));

            // 4. Convert the addr (ulong) to IntPtr for calling the function
            IntPtr addrAsIntPtr = (IntPtr)addr;

            // 5. Calling the function through the delegate
            var mainFunc = Marshal.GetDelegateForFunctionPointer<MainDelegate>(addrAsIntPtr);
            var result = mainFunc();
            return result;
        }

        bool createdMain;

        private void CreateMainFunction(string funcName)
        {
            if (createdMain) return;
            // Define the function signature
            var i8Ptr = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);

            // Define the function signature
            var funcType = LLVMTypeRef.CreateFunction(i8Ptr, Array.Empty<LLVMTypeRef>(), false);
            var mainFunc = _module.AddFunction(funcName, funcType);

            // Create a basic block for the function body
            var entry = mainFunc.AppendBasicBlock("entry");
            _builder.PositionAtEnd(entry);
            //createdMain = true;
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

        public LLVMValueRef VisitAssignExpr(AssignNodeExpr expr) // it does do the assign
        {
            Console.WriteLine($"visiting assignment: {expr.Id} = {expr.Expression}");

            // var value = Visit(expr.Expression); // Visit the right-hand side expression

            // // Store the value in the variables dictionary
            // //_context.Get[expr.Id] = value;

            // _context.Add(expr.Id, value, expr.Expression.Type);

            // return value;

            var value = Visit(expr.Expression);

            // 1. Unified Storage Type logic
            // We treat Strings and Arrays as pointers (ptr), and numbers as Double.
            LLVMTypeRef storageType;
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
            {
                storageType = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0); // Opaque ptr
            }
            else if (value.TypeOf == LLVMTypeRef.Double || value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                // Ensure all numbers are stored as Doubles for consistency
                if (value.TypeOf != LLVMTypeRef.Double)
                {
                    value = _builder.BuildSIToFP(value, LLVMTypeRef.Double, "assign_cvt");
                }
                storageType = LLVMTypeRef.Double;
            }
            else
            {
                storageType = value.TypeOf;
            }

            // 2. Persistent Global Management
            var varPtr = _module.GetNamedGlobal(expr.Id);
            if (varPtr.Handle == IntPtr.Zero)
            {
                varPtr = _module.AddGlobal(storageType, expr.Id);
                varPtr.Initializer = LLVMValueRef.CreateConstNull(storageType);
                varPtr.Alignment = 8; // Best practice for 64-bit values
            }

            // 3. Update Context using AST Type
            // Instead of guessing if it's a string, we trust the Parser/AST's prediction
            _context = _context.Add(expr.Id, varPtr, expr.Expression.Type);

            // 4. Perform the Store
            _builder.BuildStore(value, varPtr);

            return default;
        }

        // public LLVMValueRef VisitAssignExpr(AssignNodeExpr expr) // also works and uses globalvars, no ide if that makes a difference
        // {
        //     Console.WriteLine($"visiting assignment: {expr.Id} = {expr.Expression}");

        //     var value = Visit(expr.Expression);

        //     // Ensure we're using the correct type for global variables
        //     LLVMTypeRef storageType;
        //     if (value.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
        //     {
        //         storageType = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0); // Opaque ptr
        //     }
        //     else if (value.TypeOf == LLVMTypeRef.Double || value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
        //     {
        //         if (value.TypeOf != LLVMTypeRef.Double)
        //         {
        //             value = _builder.BuildSIToFP(value, LLVMTypeRef.Double, "assign_cvt");
        //         }
        //         storageType = LLVMTypeRef.Double;
        //     }
        //     else
        //     {
        //         storageType = value.TypeOf;
        //     }

        //     // Check if the variable already exists in the global scope
        //     if (!_globalVars.ContainsKey(expr.Id))
        //     {
        //         // If not, create it
        //         var varPtr = _module.AddGlobal(storageType, expr.Id);
        //         varPtr.Initializer = LLVMValueRef.CreateConstNull(storageType);
        //         varPtr.Alignment = 8; // Best practice for 64-bit values
        //         _globalVars[expr.Id] = varPtr;  // Track the global variable
        //     }

        //     // Store the updated value in the global variable
        //     var varRef = _globalVars[expr.Id];
        //     _builder.BuildStore(value, varRef);

        //     // Update context with the new value for later retrieval
        //     _context = _context.Add(expr.Id, varRef, expr.Expression.Type);

        //     return default;
        // }

        private Dictionary<string, LLVMValueRef> _localScope = null;

        public LLVMValueRef VisitIdExpr(IdNodeExpr expr)
        {

            Console.WriteLine($"visiting variable: {expr.Name}");

            // Check if the variable exists in the dictionary and return its value
            // if (_context.Get(expr.Name, out var variableValue))
            // {
            //     return variableValue;
            // }

            // var d = _context.Get(expr.Name);
            // return d.Value;

            // throw new InvalidOperationException($"Variable '{expr.Name}' not found.");

            // 1. Check Local Function Parameters FIRST
            if (_localScope != null && _localScope.TryGetValue(expr.Name, out var paramValue))
            {
                // Get the type the TypeChecker assigned (Critical for add vs helloname)
                var entryType = _context.Get(expr.Name)?.Type ?? MyType.Float;
                expr.SetType(entryType);

                // Since your VisitFunctionDef defines parameters as LLVM Double, 
                // we "Unbox" them based on what they actually represent.
                return UnboxFromDouble(paramValue, entryType);
            }

            // 2. Check the Symbol Table (Global Variables)
            var entry = _context.Get(expr.Name);
            if (entry is null) throw new Exception($"Variable {expr.Name} not defined");

            var actualType = entry.Type;
            expr.SetType(actualType);

            // 3. Load from memory
            LLVMTypeRef llvmType = (actualType == MyType.String || actualType == MyType.Array)
                ? LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0)
                : LLVMTypeRef.Double;

            return _builder.BuildLoad2(llvmType, entry.Value, expr.Name);
        }

        private LLVMValueRef UnboxFromDouble(LLVMValueRef val, MyType target)
        {
            // If we want a number, and it's already a double, just return it!
            if (target == MyType.Float || target == MyType.Int)
            {
                return val;
            }

            // If we want a string/pointer, we must go: Double bits -> Int64 -> Pointer
            if (target == MyType.String || target == MyType.Array)
            {
                var asInt64 = _builder.BuildBitCast(val, LLVMTypeRef.Int64, "smuggled_to_i64");
                return _builder.BuildIntToPtr(asInt64, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0), "i64_to_ptr");
            }

            return val;
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