using System;
using LLVMSharp;
using LLVMSharp.Interop;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ComponentModel.DataAnnotations;

namespace MyCompiler
{
    public unsafe class Compiler : IExpressionVisitor
    {
        private LLVMModuleRef _module;
        private LLVMBuilderRef _builder;

        private LLVMOpaquePassBuilderOptions* _passBuilderOptions;
        private Context _context;

        // Global variables (persists across REPL lines)
        private Dictionary<string, LLVMValueRef> _globalScope = new Dictionary<string, LLVMValueRef>();

        // Local variables (only exists while compiling a specific function)
        private Dictionary<string, LLVMValueRef> _localScope = null;
        // Maps function name to its Signature (LLVMTypeRef)
        private Dictionary<string, LLVMTypeRef> _functionPrototypes = new Dictionary<string, LLVMTypeRef>();

        public Compiler()
        {
            LLVM.InitializeNativeTarget();
            LLVM.InitializeNativeAsmPrinter();
            LLVM.InitializeNativeAsmParser();

            _module = LLVMModuleRef.CreateWithName("base");
            _builder = _module.Context.CreateBuilder();
            _context = Context.Empty; // stores variables/functions


            // Register built-ins so VisitFunctionCall can find them
            _functionPrototypes["print"] = LLVMTypeRef.CreateFunction(LLVMTypeRef.Int32, new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) }, true);

            var doubleType = LLVMTypeRef.Double;
            _functionPrototypes["round"] = LLVMTypeRef.CreateFunction(doubleType, new[] { doubleType, doubleType });
        }

        private void InitializeModule()
        {
            _module = LLVMModuleRef.CreateWithName("KaleidoscopeModule");
            _builder = _module.Context.CreateBuilder();
            _passBuilderOptions = LLVM.CreatePassBuilderOptions();
        }

        public object Run(NodeExpr expr)
        {
            _module = LLVMModuleRef.CreateWithName("repl_module");
            _builder = _module.Context.CreateBuilder();

            // 1. Predict
            var lastNode = GetLastExpression(expr);
            MyType prediction = MyType.None;
            if (lastNode is ExpressionNodeExpr exp) prediction = exp.Type;

            // --- FIX: CHECK FUNCTION SIGNATURE BEFORE DEFINING WRAPPER ---
            if (lastNode is FunctionCallNode call)
            {
                if (_functionPrototypes.TryGetValue(call.Name, out var signature))
                {
                    prediction = MapLLVMTypeToMyType(signature.ReturnType);
                }
            }

            // 2. UNIVERSAL WRAPPER SIGNATURE
            LLVMTypeRef llvmRetType = prediction switch
            {
                MyType.Float => LLVMTypeRef.Double,
                MyType.Int => LLVMTypeRef.Int64,  // Use i64 for the REPL wrapper for safety
                MyType.Bool => LLVMTypeRef.Int1,
                MyType.String => LLVMTypeRef.Int64,  // Pointers are 64-bit
                MyType.None => LLVMTypeRef.Void,
                _ => LLVMTypeRef.Int64   // Absolute fallback
            };

            var funcType = LLVMTypeRef.CreateFunction(llvmRetType, Array.Empty<LLVMTypeRef>());
            var func = _module.AddFunction($"exec_{Guid.NewGuid():N}", funcType);
            _builder.PositionAtEnd(func.AppendBasicBlock("entry"));

            // 3. GENERATE CODE
            var resultValue = Visit(expr);

            // 4. FINALIZE RETURN
            if (llvmRetType == LLVMTypeRef.Void)
            {
                _builder.BuildRetVoid();

                // Debug & Execute for Void
                Console.WriteLine("--- GENERATED IR ---");
                Console.WriteLine(_module.PrintToString());

                using var engineVoid = _module.CreateMCJITCompiler();
                engineVoid.RunFunction(func, Array.Empty<LLVMGenericValueRef>());

                if (lastNode is ForLoopNodeExpr) return "Loop Executed";
                if (lastNode is FunctionDefNode) return "Function Defined";
                return null;
            }
            else
            {
                LLVMValueRef finalRet = resultValue;

                // A. Safety Check: If Visit returned Void but wrapper expects a result
                if (resultValue.TypeOf == LLVMTypeRef.Void)
                {
                    // Use a dummy zero to prevent the cast crash
                    finalRet = (llvmRetType == LLVMTypeRef.Double)
                        ? LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, 0)
                        : LLVMValueRef.CreateConstInt(LLVMTypeRef.Int64, 0);
                }
                // B. Handle Pointers
                else if (resultValue.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                {
                    prediction = MyType.String;
                    finalRet = _builder.BuildPtrToInt(resultValue, LLVMTypeRef.Int64, "ptr_to_i64");

                    // CRITICAL: If our wrapper is Double, we MUST use BitCast for pointers.
                    // SIToFP will destroy the memory address bits.
                    if (llvmRetType == LLVMTypeRef.Double)
                    {
                        finalRet = _builder.BuildBitCast(finalRet, LLVMTypeRef.Double, "ptr_bits_to_double");
                    }
                }

                // C. Match result to the wrapper's return signature (Double or Int64)
                if (llvmRetType == LLVMTypeRef.Double && finalRet.TypeOf != LLVMTypeRef.Double)
                {
                    // Use SIToFP for integers so the value (e.g. 42) is preserved as 42.0
                    if (finalRet.TypeOf == LLVMTypeRef.Int32 || finalRet.TypeOf == LLVMTypeRef.Int64)
                        finalRet = _builder.BuildSIToFP(finalRet, LLVMTypeRef.Double, "i_to_double");
                    else
                        finalRet = _builder.BuildBitCast(finalRet, LLVMTypeRef.Double, "force_bits");
                }
                else if (llvmRetType == LLVMTypeRef.Int64 && finalRet.TypeOf != LLVMTypeRef.Int64)
                {
                    // If we have a double but need i64 (for a string pointer), bitcast
                    if (finalRet.TypeOf == LLVMTypeRef.Double)
                        finalRet = _builder.BuildBitCast(finalRet, LLVMTypeRef.Int64, "double_to_i64");
                    else
                        finalRet = _builder.BuildZExt(finalRet, LLVMTypeRef.Int64, "zext64");
                }

                _builder.BuildRet(finalRet);
            }

            // 5. DEBUG & EXECUTE
            Console.WriteLine("--- GENERATED IR ---");
            Console.WriteLine(_module.PrintToString());

            using var engine = _module.CreateMCJITCompiler();
            var res = engine.RunFunction(func, Array.Empty<LLVMGenericValueRef>());

            Console.WriteLine($"Final Prediction: {prediction}"); // Debug line
            return ExtractResult(res, prediction);
        }

        // --- Helpers to keep Run() clean ---

        private NodeExpr GetLastExpression(NodeExpr expr) // Return NodeExpr, not ExpressionNodeExpr
        {
            if (expr is SequenceNodeExpr seq)
                return seq.Statements.LastOrDefault();
            return expr;
        }

        private MyType PeekTypeInCurrentSequence(NodeExpr root, string varName)
        {
            if (root is SequenceNodeExpr seq)
            {
                // Look for the last assignment to this variable name in the current block
                var lastAssign = seq.Statements.OfType<AssignNodeExpr>().LastOrDefault(a => a.Id == varName);
                if (lastAssign != null) return lastAssign.Expression.Type;
            }
            return MyType.None;
        }


        private object ExtractResult(LLVMGenericValueRef res, MyType type)
        {
            if (type == MyType.Int) return (int)LLVM.GenericValueToInt(res, 1);
            if (type == MyType.Bool) return LLVM.GenericValueToInt(res, 0) != 0;
            if (type == MyType.Float) return LLVM.GenericValueToFloat(LLVMTypeRef.Double, res);

            if (type == MyType.String)
            {
                // 1. Try to get the address as a raw 64-bit integer
                ulong addr = LLVM.GenericValueToInt(res, 0);

                // 2. If that looks like 0, try the floating-point bitcast fallback 
                // (Just in case the exec wrapper used bitcast instead of ptrtoint)
                if (addr == 0)
                {
                    double rawDouble = LLVM.GenericValueToFloat(LLVMTypeRef.Double, res);
                    addr = (ulong)BitConverter.DoubleToInt64Bits(rawDouble);
                }

                if (addr == 0) return "(null)";

                // 3. Convert the address to a C# string
                IntPtr ptr = new IntPtr((long)addr);
                return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(ptr);
            }
            return null;
        }

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

        public LLVMValueRef VisitBooleanExpr(BooleanNodeExpr expr)
        {
            ulong val = expr.Value ? 1UL : 0UL;
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, val, false);
        }
        public LLVMValueRef VisitStringExpr(StringNodeExpr expr)
        {
            return _builder.BuildGlobalStringPtr(expr.Value, "str");
        }
        public LLVMValueRef VisitFloatExpr(FloatNodeExpr expr)
        {
            return LLVMValueRef.CreateConstReal(_module.Context.DoubleType, expr.Value);
        }

        // OLD VisitBinaryExpr - NO CONNCAT!
        // public LLVMValueRef VisitBinaryExpr(BinaryOpNodeExpr expr)
        // {
        //     var lhs = Visit(expr.Left);
        //     var rhs = Visit(expr.Right);

        //     switch (expr.Operator)
        //     {
        //         case "+": return _builder.BuildAdd(lhs, rhs, "addtmp");
        //         case "-": return _builder.BuildSub(lhs, rhs, "subtmp");
        //         case "*": return _builder.BuildMul(lhs, rhs, "multmp");
        //         case "/": return _builder.BuildSDiv(lhs, rhs, "divtmp");
        //         default: throw new InvalidOperationException("Unknown operator");
        //     }
        // }

        public LLVMValueRef VisitBinaryExpr(BinaryOpNodeExpr expr)
        {
            var lhs = Visit(expr.Left);
            var rhs = Visit(expr.Right);
            // 1. THE ULTIMATE GUARD: If either side is an LLVM Pointer, we MUST use String Concat
            // This ignores whatever 'MyType' says and looks at the actual generated LLVM IR type
            bool lhsIsPtr = lhs.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind;
            bool rhsIsPtr = rhs.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind;

            if (expr.Operator == "+" && (lhsIsPtr || rhsIsPtr))
            {
                // Use your BuildStringConcat helper
                // We pass the types so the helper knows whether to use %s or %d
                return BuildStringConcat(lhs, lhsIsPtr ? MyType.String : MyType.Float,
                                         rhs, rhsIsPtr ? MyType.String : MyType.Float);
            }

            // 2. Math logic (only if neither is a pointer)
            bool isFloat = (expr.Left.Type == MyType.Float || expr.Right.Type == MyType.Float ||
                            expr.Left.Type == MyType.Int || expr.Right.Type == MyType.Int);

            if (isFloat)
            {
                lhs = EnsureFloat(lhs, expr.Left.Type);
                rhs = EnsureFloat(rhs, expr.Right.Type);
                return expr.Operator switch
                {
                    "+" => _builder.BuildFAdd(lhs, rhs, "faddtmp"),
                    "-" => _builder.BuildFSub(lhs, rhs, "fsubtmp"),
                    _ => throw new InvalidOperationException()
                };
            }

            // Standard Integer Math
            return expr.Operator switch
            {
                "+" => _builder.BuildAdd(lhs, rhs, "addtmp"),
                "-" => _builder.BuildSub(lhs, rhs, "subtmp"),
                "*" => _builder.BuildMul(lhs, rhs, "multmp"),
                "/" => _builder.BuildSDiv(lhs, rhs, "divtmp"),
                _ => throw new InvalidOperationException("Unknown operator")
            };
        }


        public LLVMValueRef VisitAssignExpr(AssignNodeExpr expr)
        {
            var value = Visit(expr.Expression);

            // FIX: Ensure numbers are always stored as Doubles
            if (value.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind)
            {
                value = _builder.BuildSIToFP(value, LLVMTypeRef.Double, "assign_cvt");
            }
            // Always create/get the global in the CURRENT module
            LLVMTypeRef storageType = (value.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                ? LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0)
                : LLVMTypeRef.Double;

            // This finds the variable if it exists in the CURRENT module, or creates it if not.
            var varPtr = _module.GetNamedGlobal(expr.Id);
            if (varPtr.Handle == IntPtr.Zero)
            {
                varPtr = _module.AddGlobal(storageType, expr.Id);
                varPtr.Initializer = LLVMValueRef.CreateConstNull(storageType);
            }

            // Update context so the Symbol Table knows the latest Type/Reference
            MyType myType = (value.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind) ? MyType.String : MyType.Float;
            _context = _context.Add(expr.Id, varPtr, myType);

            _builder.BuildStore(value, varPtr);
            return value;
        }


        public LLVMValueRef VisitIdExpr(IdNodeExpr expr)
        {
            // 1. Check Local Function Parameters FIRST
            // These are direct values, so we return them immediately (no Load needed)
            if (_localScope != null && _localScope.TryGetValue(expr.Name, out var paramValue))
            {
                // For your thesis: Parameters in this setup are treated as Floats 
                // because our FunctionDef defines them as doubles.
                expr.SetType(MyType.Float);
                return paramValue;
            }

            // 2. Check the Symbol Table (Global Variables)
            var entry = _context.Get(expr.Name);
            if (entry is null) throw new Exception($"Variable {expr.Name} not defined");

            // Pull the type from the symbol table
            var actualType = entry.Value.Type; // Is this MyType.String?
            expr.SetType(actualType);

            // Variables are pointers in memory, so we MUST use BuildLoad2
            LLVMTypeRef llvmType = GetLLVMType(actualType);
            return _builder.BuildLoad2(llvmType, entry.Value.Value, expr.Name);
        }

        public LLVMValueRef VisitDecrementExpr(DecrementNodeExpr expr)
        {
            var entry = _context.Get(expr.Id)
                ?? throw new InvalidOperationException($"Variable {expr.Id} not defined");

            var valueRef = entry.Value;

            var current = _builder.BuildLoad2(LLVMTypeRef.Int32, valueRef, expr.Id);

            var decremented = _builder.BuildSub(
                current,
                LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 1, false),
                "dec");

            _builder.BuildStore(decremented, valueRef);

            return decremented;
        }

        public LLVMValueRef VisitIncrementExpr(IncrementNodeExpr expr)
        {
            var entry = _context.Get(expr.Id)
                ?? throw new InvalidOperationException($"Variable {expr.Id} not defined");

            var valueRef = entry.Value;

            var current = _builder.BuildLoad2(LLVMTypeRef.Int32, valueRef, expr.Id);

            var incremented = _builder.BuildAdd(
                current,
                LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 1, false),
                "inc");

            _builder.BuildStore(incremented, valueRef);

            return incremented;
        }

        // Update your IExpressionVisitor interface to include this
        public LLVMValueRef VisitComparisonExpr(ComparisonNodeExpr expr)
        {
            var lhs = Visit(expr.Left);
            var rhs = Visit(expr.Right);

            // Check if we are comparing floats
            bool isFloat = (expr.Left.Type == MyType.Float || expr.Right.Type == MyType.Float);

            if (isFloat)
            {
                lhs = EnsureFloat(lhs, expr.Left.Type);
                rhs = EnsureFloat(rhs, expr.Right.Type);

                return expr.Operator switch
                {
                    "==" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, lhs, rhs, "fcmptmp"),
                    "!=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealONE, lhs, rhs, "fcmptmp"),
                    "<" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, lhs, rhs, "fcmptmp"),
                    ">" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, lhs, rhs, "fcmptmp"),
                    "<=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLE, lhs, rhs, "fcmptmp"),
                    ">=" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGE, lhs, rhs, "fcmptmp"),
                    _ => throw new Exception($"Unknown float operator: {expr.Operator}")
                };
            }

            // Integer Comparison
            return expr.Operator switch
            {
                "==" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, lhs, rhs, "cmptmp"),
                "!=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, lhs, rhs, "cmptmp"),
                "<" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, lhs, rhs, "cmptmp"),
                ">" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, lhs, rhs, "cmptmp"),
                "<=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, lhs, rhs, "cmptmp"),
                ">=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, lhs, rhs, "cmptmp"),
                _ => throw new Exception($"Unknown integer operator: {expr.Operator}")
            };
        }

        public LLVMValueRef VisitIfExpr(IfNodeExpr expr)
        {
            if (expr.Condition == null) throw new Exception("IF condition is null");

            var cond = Visit(expr.Condition);

            // Safety: LLVM CondBr requires an i1. If your condition is i32, convert it.
            if (cond.TypeOf == LLVMTypeRef.Int32)
                cond = _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, cond, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0), "ifcond");

            var func = _builder.InsertBlock.Parent;
            var thenBB = func.AppendBasicBlock("then");
            var elseBB = func.AppendBasicBlock("else");
            var mergeBB = func.AppendBasicBlock("ifcont");

            _builder.BuildCondBr(cond, thenBB, elseBB);

            // Then
            _builder.PositionAtEnd(thenBB);
            Visit(expr.ThenPart); // Ensure ThenPart is not null
            _builder.BuildBr(mergeBB);

            // Else
            _builder.PositionAtEnd(elseBB);
            if (expr.ElsePart != null)
                Visit(expr.ElsePart);
            _builder.BuildBr(mergeBB);

            _builder.PositionAtEnd(mergeBB);
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0); // Return dummy 0 for void-like if
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


        public LLVMValueRef VisitRandomExpr(RandomNodeExpr expr)
        {
            var llvmCtx = _module.Context;

            // Get or declare rand()
            var randFunc = _module.GetNamedFunction("rand");
            if (randFunc.Handle == IntPtr.Zero)
            {
                var randType = LLVMTypeRef.CreateFunction(llvmCtx.Int32Type, Array.Empty<LLVMTypeRef>(), false);
                randFunc = _module.AddFunction("rand", randType);
            }

            var randValue = _builder.BuildCall2(LLVMTypeRef.CreateFunction(llvmCtx.Int32Type, Array.Empty<LLVMTypeRef>()),
            randFunc, Array.Empty<LLVMValueRef>(), "randcall");

            if (expr.MinValue != null && expr.MaxValue != null)
            {
                var minValue = Visit(expr.MinValue);
                var maxValue = Visit(expr.MaxValue);

                var diff = _builder.BuildSub(maxValue, minValue, "diff");

                if (_builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, maxValue, minValue, "checkMax").ToString().Contains("false"))
                {
                    diff = _builder.BuildSub(minValue, maxValue, "diff");
                    minValue = maxValue;
                }

                var one = LLVMValueRef.CreateConstInt(llvmCtx.Int32Type, 1, false);
                var rangeSize = _builder.BuildAdd(diff, one, "rangesize");

                var modResult = _builder.BuildSRem(randValue, rangeSize, "modtmp");
                return _builder.BuildAdd(modResult, minValue, "randomInRange");
            }

            return randValue;
        }

        public LLVMValueRef VisitPrintExpr(PrintNodeExpr expr)
        {
            var valueToPrint = Visit(expr.Expression);
            var printf = GetPrintf();
            var llvmCtx = _module.Context;

            LLVMValueRef formatStr;

            // 1. Get the actual LLVM Kind
            var typeKind = valueToPrint.TypeOf.Kind;

            // 2. Select format string based on the REAL IR type
            if (typeKind == LLVMTypeKind.LLVMDoubleTypeKind)
            {
                formatStr = _builder.BuildGlobalStringPtr("%f\n", "print_float_fmt");
            }
            else if (typeKind == LLVMTypeKind.LLVMPointerTypeKind)
            {
                formatStr = _builder.BuildGlobalStringPtr("%s\n", "print_str_fmt");
            }
            else // Assume i32/i1
            {
                formatStr = _builder.BuildGlobalStringPtr("%d\n", "print_int_fmt");
            }

            // 3. Call printf
            var printfType = LLVMTypeRef.CreateFunction(llvmCtx.Int32Type,
                new[] { LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0) }, true);

            return _builder.BuildCall2(printfType, printf,
                new[] { formatStr, valueToPrint }, "printcall");
        }

        // For loop implementation to LLVM, more advanced
        public LLVMValueRef VisitForLoopExpr(ForLoopNodeExpr expr)
        {
            var func = _builder.InsertBlock.Parent;
            var llvmCtx = _module.Context;

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

            // Ensure condition is i1 (Boolean)
            // If your comparison returns Double, you'd need a BuildFCmp here.
            // If it returns i32, use BuildICmp.
            if (condition.TypeOf != LLVMTypeRef.Int1)
            {
                // Example: convert i32 to i1 by checking if != 0
                condition = _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE,
                    condition, LLVMValueRef.CreateConstInt(condition.TypeOf, 0), "fortest");
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

            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0);
        }


        public LLVMValueRef VisitFunctionDef(FunctionDefNode node)
        {
            // 1. Map the Return Type (Fixes error CS0103 for llvmRetType)
            MyType retType = MapStringToMyType(node.ReturnTypeName);
            LLVMTypeRef llvmRetType = GetLLVMType(retType);

            // 2. Define Parameter Types (Assuming Double for now)
            var argTypes = Enumerable.Repeat(LLVMTypeRef.Double, node.Parameters.Count).ToArray();

            // 3. Create the Function Signature (Fixes error CS0103 for funcType)
            LLVMTypeRef funcType = LLVMTypeRef.CreateFunction(llvmRetType, argTypes);

            // Register for the Registry so VisitFunctionCall can find it later
            _functionPrototypes[node.Name] = funcType;

            // 4. Create the function in the module
            var func = _module.AddFunction(node.Name, funcType);

            // SAVE the current builder position (where the REPL 'exec' wrapper is)
            var previousBlock = _builder.InsertBlock;

            // 5. Build the Function Body
            var entry = func.AppendBasicBlock("entry");
            _builder.PositionAtEnd(entry);

            // Setup local scope for parameters
            _localScope = new Dictionary<string, LLVMValueRef>();
            for (int i = 0; i < node.Parameters.Count; i++)
            {
                _localScope[node.Parameters[i]] = func.GetParam((uint)i);
            }

            // Generate body code
            var bodyResult = Visit(node.Body);

            // 6. Finalize the Function Return
            if (retType == MyType.None) // This is the "void" case
            {
                _builder.BuildRetVoid();
            }
            else
            {
                // Use MatchType to ensure the body matches the declared func:Type
                var finalValue = MatchType(bodyResult, llvmRetType);
                _builder.BuildRet(finalValue);
            }

            // 7. CLEANUP: Jump back to where we were in the REPL wrapper
            _localScope = null;
            if (previousBlock.Handle != IntPtr.Zero)
            {
                _builder.PositionAtEnd(previousBlock);
            }

            return func;
        }


        private MyType MapStringToMyType(string name)
        {
            if (string.IsNullOrEmpty(name)) return MyType.Float; // Default fallback

            return name.ToLower() switch
            {
                "int" => MyType.Int,
                "string" => MyType.String,
                "float" => MyType.Float,
                "double" => MyType.Float,
                "bool" => MyType.Bool,
                "void" => MyType.None,
                _ => MyType.Float  // Default for your thesis language
            };
        }
        // Helper to bridge your Parser and Compiler
        private MyType MapLLVMTypeToMyType(LLVMTypeRef type)
        {
            if (type == LLVMTypeRef.Double) return MyType.Float;
            if (type == LLVMTypeRef.Int32) return MyType.Int;
            if (type == LLVMTypeRef.Int1) return MyType.Bool;
            if (type.Kind == LLVMTypeKind.LLVMPointerTypeKind) return MyType.String;
            if (type == LLVMTypeRef.Int64) return MyType.Int; // Or String, depending on your cast logic

            return MyType.None;
        }

        public LLVMValueRef VisitFunctionCall(FunctionCallNode node)
        {
            if (!_functionPrototypes.TryGetValue(node.Name, out var signature))
                throw new Exception($"Function {node.Name} not defined.");

            var args = new List<LLVMValueRef>();
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                var val = Visit(node.Arguments[i]);

                // --- THE FIX ---
                // Since our functions use 'double' for all parameters, 
                // we must cast every argument to double before calling.
                if (val.TypeOf != LLVMTypeRef.Double)
                {
                    val = _builder.BuildSIToFP(val, LLVMTypeRef.Double, "arg_to_double");
                }
                args.Add(val);
            }

            return _builder.BuildCall2(signature, _module.GetNamedFunction(node.Name), args.ToArray(), "calltmp");
        }

        //------Helper-functions------//

        private LLVMValueRef MatchType(LLVMValueRef value, LLVMTypeRef targetType)
        {
            if (value.TypeOf == targetType) return value;

            // 1. Float (Double) <-> Int32
            if (targetType == LLVMTypeRef.Double && value.TypeOf == LLVMTypeRef.Int32)
                return _builder.BuildSIToFP(value, LLVMTypeRef.Double, "cast_to_float");

            if (targetType == LLVMTypeRef.Int32 && value.TypeOf == LLVMTypeRef.Double)
                return _builder.BuildFPToSI(value, LLVMTypeRef.Int32, "cast_to_int");

            // 2. Handle Pointers (For String functions)
            // If the body returned a pointer but the function expects i64
            if (targetType == LLVMTypeRef.Int64 && value.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                return _builder.BuildPtrToInt(value, LLVMTypeRef.Int64, "ptr_to_int");

            // 3. Handle Boolean (i1)
            if (targetType == LLVMTypeRef.Int1 && value.TypeOf != LLVMTypeRef.Int1)
                return _builder.BuildFCmp(LLVMRealPredicate.LLVMRealUNE, value, LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, 0), "to_bool");

            return value;
        }
        private LLVMValueRef EnsureFloat(LLVMValueRef value, MyType currentType)
        {
            if (value.TypeOf == LLVMTypeRef.Double) return value;
            if (value.TypeOf == LLVMTypeRef.Int32)
                return _builder.BuildSIToFP(value, LLVMTypeRef.Double, "cast_tmp");

            return value; // Hope for the best, or throw an error
        }
        private LLVMValueRef BuildStringConcat(LLVMValueRef lhs, MyType lhsType, LLVMValueRef rhs, MyType rhsType)
        {
            var llvmCtx = _module.Context;
            var malloc = GetMalloc();
            var sprintf = GetSprintf();


            // Inside BuildStringConcat
            if (lhsType != MyType.String)
                lhs = _builder.BuildFPToSI(lhs, LLVMTypeRef.Int32, "tmp_int");
            if (rhsType != MyType.String)
                rhs = _builder.BuildFPToSI(rhs, LLVMTypeRef.Int32, "tmp_int");

            // 1. Allocate 256 bytes for the new string
            var buffer = _builder.BuildCall2(
                LLVMTypeRef.CreateFunction(LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0), new[] { llvmCtx.Int64Type }),
                malloc,
                new[] { LLVMValueRef.CreateConstInt(llvmCtx.Int64Type, 256) },
                "concat_buf"
            );

            // 2. Determine the format string (e.g., "%s%s", "%s%d", or "%d%s")
            string fmtStr = "";
            fmtStr += (lhsType == MyType.String) ? "%s" : "%d";
            fmtStr += (rhsType == MyType.String) ? "%s" : "%d";

            var fmtPtr = _builder.BuildGlobalStringPtr(fmtStr, "concat_fmt");

            // 3. Call sprintf(buffer, fmt, lhs, rhs)
            var sprintfType = LLVMTypeRef.CreateFunction(
                llvmCtx.Int32Type,
                new[] { LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0), LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0) },
                true
            );

            _builder.BuildCall2(sprintfType, sprintf, new[] { buffer, fmtPtr, lhs, rhs }, "sprintf_call");

            return buffer;
        }
        private LLVMValueRef GetMalloc()
        {
            var fn = _module.GetNamedFunction("malloc");
            if (fn.Handle != IntPtr.Zero) return fn;
            return _module.AddFunction("malloc", LLVMTypeRef.CreateFunction(
                LLVMTypeRef.CreatePointer(_module.Context.Int8Type, 0),
                new[] { _module.Context.Int64Type }, false));
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

        private LLVMTypeRef GetLLVMType(MyType type)
        {
            return type switch
            {
                MyType.Int => LLVMTypeRef.Int32,
                MyType.Float => LLVMTypeRef.Double,
                MyType.Bool => LLVMTypeRef.Int1,
                MyType.String => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0), // Must be a pointer!
                MyType.None => LLVMTypeRef.Void,
                _ => LLVMTypeRef.Double
            };
        }
        private LLVMValueRef GetPrintf()
        {
            var printf = _module.GetNamedFunction("printf");
            if (printf.Handle != IntPtr.Zero) return printf;

            // Fix: Use _module.Context to get the LLVM types
            var llvmCtx = _module.Context;

            var printfType = LLVMTypeRef.CreateFunction(
                llvmCtx.Int32Type,
                new[] { LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0) },
                true
            );
            return _module.AddFunction("printf", printfType);
        }

        private LLVMValueRef Visit(NodeExpr expr)
        {
            return expr.Accept(this);
        }
    }
}