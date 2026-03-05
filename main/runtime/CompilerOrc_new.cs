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
        public int tag;      // 4 bytes
                             // The [Pack = 8] or an explicit field tells C# to skip 4 bytes here
        public IntPtr data;  // 8 bytes
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
        private int _replCounter = 0;
        private MyType _lastType; // Store the type of the last expression for auto-printing
        private NodeExpr _lastNode; // Store the last expression for auto-printing
        private LLVMTypeRef _runtimeValueType;
        HashSet<string> _definedGlobals = new(); // wouldn't we use our context for this job?
        private Context _context;
        private LLVMTypeRef _mallocType;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MainDelegate();
        private LLVMContextRef _llvmContext; // Persistent context
        private readonly List<IntPtr> _persistentStrings = new();



        public CompilerOrc()
        {
            LLVM.InitializeNativeTarget();
            LLVM.InitializeNativeAsmPrinter();
            LLVM.InitializeNativeAsmParser();

            // 1. Create a single persistent context for the whole session
            _llvmContext = LLVMContextRef.Create();
            _builder = _llvmContext.CreateBuilder();

            // 2. Initialize JIT
            EnsureJit();

            // 3. Define types once in the persistent context
            DeclareValueStruct();

            _context = Context.Empty;
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
            // Use the persistent _llvmContext
            var tagType = _llvmContext.Int32Type;
            var ptrType = LLVMTypeRef.CreatePointer(_llvmContext.Int8Type, 0);

            // Define the value struct: [i32 (tag), i8* (dataPtr)]
            _runtimeValueType = _llvmContext.CreateNamedStruct("RuntimeValue");
            _runtimeValueType.StructSetBody(new[] { tagType, ptrType }, false);
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

        public object Run(NodeExpr expr, bool generateIR = false)
        {
            var prediction = PerformSemanticAnalysis(expr);
            string funcName = $"main_{_replCounter++}";

            // Create a new module for this specific command, using the SHARED context
            _module = _llvmContext.CreateModuleWithName($"module_{_replCounter}");

            // Define the function return type: RuntimeValue*
            var funcType = LLVMTypeRef.CreateFunction(
                LLVMTypeRef.CreatePointer(_runtimeValueType, 0),
                Array.Empty<LLVMTypeRef>(),
                false);

            var function = _module.AddFunction(funcName, funcType);
            var entry = function.AppendBasicBlock("entry");
            _builder.PositionAtEnd(entry);

            // Visiting logic
            LLVMValueRef resultValue = Visit(expr);

            if (_lastNode is ExpressionNodeExpr exprNode)
            {
                var boxedPtr = BoxValue(resultValue, exprNode.Type);
                _builder.BuildRet(boxedPtr);
            }
            else if (_lastNode is AssignNodeExpr assignNode)
            {
                // Important: Use the type of the assignment!
                var boxedPtr = BoxValue(resultValue, assignNode.Type);
                _builder.BuildRet(boxedPtr);
            }

            if (generateIR) DumpIR(_module);

            // --- JIT Hand-off ---
            // Note: LLVMOrcCreateNewThreadSafeModule takes ownership of the module
            var tsc = OrcBindings.LLVMOrcCreateNewThreadSafeContext();
            var tsm = OrcBindings.LLVMOrcCreateNewThreadSafeModule(_module.Handle, tsc);

            var dylib = OrcBindings.LLVMOrcLLJITGetMainJITDylib(_jit);
            ThrowIfError(OrcBindings.LLVMOrcLLJITAddLLVMIRModule(_jit, dylib, tsm));

            ulong addr;
            ThrowIfError(OrcBindings.LLVMOrcLLJITLookup(_jit, out addr, funcName));

            var fnPtr = (IntPtr)addr;
            var delegateResult = Marshal.GetDelegateForFunctionPointer<MainDelegate>(fnPtr);
            IntPtr resultPtr = delegateResult();

            if (resultPtr == IntPtr.Zero) return null;

            RuntimeValue result = Marshal.PtrToStructure<RuntimeValue>(resultPtr);

            return (ValueTag)result.tag switch
            {
                ValueTag.Int => Marshal.ReadInt64(result.data),
                ValueTag.Float => Marshal.PtrToStructure<double>(result.data),
                ValueTag.String => Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(result.data)),
                ValueTag.Bool => Marshal.ReadInt64(result.data) != 0,
                ValueTag.Array => HandleArray(result.data),
                _ => null
            };
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
            var ctx = _module.Context;
            var (mType, mFunc) = GetMalloc();
            int tag = (int)GetTag(type);

            // 1. Allocate data payload (8 bytes)
            var dataMem = _builder.BuildCall2(mType, mFunc, new[] { LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 8) }, "data_mem");

            // 2. Storage Logic based on the Tag
            if (tag == (int)ValueTag.Int)
            {
                var intVal = value.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind
                    ? _builder.BuildFPToSI(value, LLVMTypeRef.Int64, "to_i64")
                    : _builder.BuildZExtOrBitCast(value, LLVMTypeRef.Int64, "to_i64");

                var cast = _builder.BuildBitCast(dataMem, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int64, 0), "int_ptr");
                _builder.BuildStore(intVal, cast);
            }
            else if (tag == (int)ValueTag.Float)
            {
                var dblVal = value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind
                    ? _builder.BuildSIToFP(value, LLVMTypeRef.Double, "to_dbl")
                    : value;

                var cast = _builder.BuildBitCast(dataMem, LLVMTypeRef.CreatePointer(LLVMTypeRef.Double, 0), "dbl_ptr");
                _builder.BuildStore(dblVal, cast);
            }
            else if (tag == (int)ValueTag.Bool)
            {
                // Zero-extend i1 to i64 so Marshal.ReadInt64(data) != 0 works correctly
                var boolVal = _builder.BuildZExt(value, LLVMTypeRef.Int64, "bool_to_i64");
                var cast = _builder.BuildBitCast(dataMem, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int64, 0), "bool_ptr");
                _builder.BuildStore(boolVal, cast);
            }
            else // String, Array
            {
                // IMPORTANT: value is already a pointer (i8*). 
                // We want to store the pointer itself into the 8 bytes of dataMem.
                // So we cast dataMem to i8** (pointer to a pointer).
                var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
                var i8PtrPtr = LLVMTypeRef.CreatePointer(i8Ptr, 0);

                var cast = _builder.BuildBitCast(dataMem, i8PtrPtr, "ptr_box_cast");
                _builder.BuildStore(value, cast);
            }

            // 3. Allocate the RuntimeValue struct (16 bytes)
            var structMem = _builder.BuildCall2(mType, mFunc, new[] { LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 16) }, "struct_mem");

            // Store Tag (at offset 0)
            var tagPtr = _builder.BuildStructGEP2(_runtimeValueType, structMem, 0, "tag_ptr");
            _builder.BuildStore(LLVMValueRef.CreateConstInt(ctx.Int32Type, (ulong)tag), tagPtr);

            // Store Data Pointer (at offset 8)
            var dataPtrField = _builder.BuildStructGEP2(_runtimeValueType, structMem, 1, "data_ptr");
            _builder.BuildStore(dataMem, dataPtrField);

            return structMem;
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

        private ValueTag GetTag(MyType type)
        {
            return type switch
            {
                MyType.Int => ValueTag.Int,
                MyType.Float => ValueTag.Float,
                MyType.Bool => ValueTag.Bool,
                MyType.String => ValueTag.String,
                MyType.Array => ValueTag.Array,
                _ => ValueTag.None
            };
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

            if (global.Handle == IntPtr.Zero)
            {
                // Not defined in this module → declare external
                global = module.AddGlobal(LLVMTypeRef.Double, expr.Id);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage;
            }

            var value = _builder.BuildLoad2(LLVMTypeRef.Double, global, "dec_load");
            var newValue = _builder.BuildFSub(value, LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, 1.0), "dec_sub");

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
            if (value.TypeOf == LLVMTypeRef.Double) return value;
            if (value.TypeOf == LLVMTypeRef.Int32)
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
            // 1. Visit sides
            var lhs = Visit(expr.Left);
            var rhs = Visit(expr.Right);

            // 2. Handle Strings (Concatenation)
            if (expr.Operator == "+" && (expr.Left.Type == MyType.String || expr.Right.Type == MyType.String))
            {
                return BuildStringConcat(lhs, expr.Left.Type, rhs, expr.Right.Type);
            }

            // 3. Handle Numbers
            // Ensure both are Doubles so BuildFAdd doesn't crash
            lhs = EnsureDouble(lhs);
            rhs = EnsureDouble(rhs);

            return expr.Operator switch
            {
                "+" => _builder.BuildFAdd(lhs, rhs, "add"),
                "-" => _builder.BuildFSub(lhs, rhs, "sub"),
                "*" => _builder.BuildFMul(lhs, rhs, "mul"),
                "/" => _builder.BuildFDiv(lhs, rhs, "div"),
                "==" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, lhs, rhs, "cmp"),
                _ => throw new Exception($"Unknown operator {expr.Operator}")
            };
        }


        private (LLVMTypeRef Type, LLVMValueRef Func) GetMalloc()
        {
            // Look for it in the CURRENT module
            var existing = _module.GetNamedFunction("malloc");

            // Define the type: i8* malloc(i64)
            var i8Ptr = LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0);
            var mallocType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { LLVMTypeRef.Int64 }, false);

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
        private LLVMValueRef BuildStringConcat(LLVMValueRef lhs, MyType lhsType, LLVMValueRef rhs, MyType rhsType)
        {
            var ctx = _module.Context;
            var malloc = GetMalloc(); // Returns (LLVMTypeRef Type, LLVMValueRef Func)
            var sprintf = GetSprintf();
            var ptrType = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);

            // Prepare Arguments for sprintf
            LLVMValueRef Prepare(LLVMValueRef val, MyType type)
            {
                if (type == MyType.String) return _builder.BuildBitCast(val, ptrType, "str_ptr");
                return EnsureDouble(val);
            }

            var arg1 = Prepare(lhs, lhsType);
            var arg2 = Prepare(rhs, rhsType);

            // 1. Malloc buffer (Use malloc.Type instead of _mallocType)
            var buffer = _builder.BuildCall2(malloc.Type, malloc.Func,
                new[] { LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 256) }, "concat_buf");

            // 2. Format String (using %g for cleaner number formatting)
            string fmtStr = (lhsType == MyType.String ? "%s" : "%g") +
                            (rhsType == MyType.String ? "%s" : "%g");
            var fmtPtr = _builder.BuildGlobalStringPtr(fmtStr, "fmt");
            var fmtArg = _builder.BuildBitCast(fmtPtr, ptrType, "fmt_ptr");

            // 3. Call sprintf
            // We define the type for this specific call to match the arguments exactly
            var sprintfType = LLVMTypeRef.CreateFunction(ctx.Int32Type,
                new[] { ptrType, ptrType, arg1.TypeOf, arg2.TypeOf }, false);

            _builder.BuildCall2(sprintfType, sprintf, new[] { buffer, fmtArg, arg1, arg2 }, "sprintf_call");

            return buffer;
        }

        // Helper to ensure numbers are always doubles in the IR
        private LLVMValueRef EnsureDouble(LLVMValueRef val)
        {
            if (val.TypeOf == LLVMTypeRef.Double) return val;
            // Converts i32 or i64 to double
            return _builder.BuildSIToFP(val, LLVMTypeRef.Double, "conv");
        }


        public LLVMValueRef VisitNumberExpr(NumberNodeExpr expr)
        {
            // Force everything to Double at the source
            return LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, expr.Value);
        }
        public LLVMValueRef VisitBooleanExpr(BooleanNodeExpr expr)
        {
            return LLVMValueRef.CreateConstReal(LLVMTypeRef.Int1, expr.Value ? 1ul : 0ul);   // Use 1.0 for True to match your Int-to-Bool promotion
        }
        public LLVMValueRef VisitFloatExpr(FloatNodeExpr expr)
        {
            return LLVMValueRef.CreateConstReal(_module.Context.DoubleType, expr.Value);
        }
        public LLVMValueRef VisitStringExpr(StringNodeExpr expr)
        {
        
            return _builder.BuildGlobalStringPtr(expr.Value, "tmp_str");

        }
        public LLVMValueRef VisitAssignExpr(AssignNodeExpr expr)
        {
            Console.WriteLine($"visiting assignment: {expr.Id}");

            var value = Visit(expr.Expression);

            // 1. Ensure numeric values are double for consistency
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
                value = _builder.BuildSIToFP(value, LLVMTypeRef.Double, "assign_cvt");

            var storageType = value.TypeOf;
            var module = _module;

            // 2. Check if it already exists in THIS module
            LLVMValueRef global = module.GetNamedGlobal(expr.Id);

            if (global.Handle == IntPtr.Zero)
            {
                // Not in this module. Is it totally new or defined in a previous REPL run?
                global = module.AddGlobal(storageType, expr.Id);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage;

                if (!_definedGlobals.Contains(expr.Id))
                {
                    // BRAND NEW: Needs an initializer to be "defined"
                    _definedGlobals.Add(expr.Id);

                    if (storageType.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                        global.Initializer = LLVMValueRef.CreateConstPointerNull(storageType);
                    else
                        global.Initializer = LLVMValueRef.CreateConstReal(storageType, 0.0);
                }
                // If it WAS in _definedGlobals, we leave it as External Linkage without 
                // an initializer so the JIT links it to the previous module's memory.
            }

            // 3. Perform the store
            _builder.BuildStore(value, global);

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
                _builder.BuildStore(res1, resultPtr); // Save result
                _builder.BuildBr(mergeBB);

                // "Else" Part (Wrong Order - Swap logic)
                _builder.PositionAtEnd(elseBB);
                var diff2 = _builder.BuildSub(minVal, maxVal, "diff2");
                var range2 = _builder.BuildAdd(diff2, LLVMValueRef.CreateConstInt(i32, 1), "range2");
                var res2 = _builder.BuildAdd(_builder.BuildSRem(randValue, range2, "mod2"), maxVal, "res2");
                _builder.BuildStore(res2, resultPtr); // Save result
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
                _builder.BuildStore(boxed, elementPtr);
            }
            return arrayPtr;
        }
        public LLVMValueRef VisitIdExpr(IdNodeExpr expr)
        {
            var module = _module;
            LLVMValueRef global = module.GetNamedGlobal(expr.Name);

            // Check if it's an array or string based on your MyType
            var llvmType = (expr.Type == MyType.Array || expr.Type == MyType.String)
                ? LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0)
                : LLVMTypeRef.Double;

            if (global.Handle == IntPtr.Zero)
            {
                global = module.AddGlobal(llvmType, expr.Name);
                global.Linkage = LLVMLinkage.LLVMExternalLinkage;
            }

            return _builder.BuildLoad2(llvmType, global, expr.Name);
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