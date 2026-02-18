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

        public Compiler()
        {
            LLVM.InitializeNativeTarget();
            LLVM.InitializeNativeAsmPrinter();
            LLVM.InitializeNativeAsmParser();

            _module = LLVMModuleRef.CreateWithName("base");
            _builder = _module.Context.CreateBuilder();
            _context = Context.Empty; // stores variables/functions
        }

        private void InitializeModule()
        {
            _module = LLVMModuleRef.CreateWithName("KaleidoscopeModule");
            _builder = _module.Context.CreateBuilder();
            _passBuilderOptions = LLVM.CreatePassBuilderOptions();
        }

        public object Run(NodeExpr expr)
        {
            // 1. Setup
            _module = LLVMModuleRef.CreateWithName("repl_module");
            _builder = _module.Context.CreateBuilder();
            LLVMExecutionEngineRef _engine = _module.CreateMCJITCompiler();

            // 2. PREDICT the type (The "Peek")
            // 2. PREDICT the type (The "Peek")
            var lastNode = GetLastExpression(expr);

            // Default to None (Void) if the node is null or not a value-returning expression
            MyType prediction = MyType.None;

            if (lastNode is ExpressionNodeExpr exp)
            {
                prediction = exp.Type;

                // Check for float promotion
                if (exp is BinaryOpNodeExpr bin && (bin.Left.Type == MyType.Float || bin.Right.Type == MyType.Float))
                {
                    prediction = MyType.Float;
                }
            }

            LLVMTypeRef llvmRetType = GetLLVMType(prediction);

            // 3. Create Function Signature
            var funcType = LLVMTypeRef.CreateFunction(llvmRetType, Array.Empty<LLVMTypeRef>());
            var func = _module.AddFunction($"exec_{Guid.NewGuid():N}", funcType);
            _builder.PositionAtEnd(func.AppendBasicBlock("entry"));

            // 4. GENERATE CODE (The only Visit call)
            var resultValue = Visit(expr);
            if (prediction == MyType.None) return "Function Defined";


            // 5. Finalize the Return
            // If our prediction was Int but Visit actually produced a Float (or vice versa),
            // we must make sure the 'ret' instruction matches the 'define' signature.
            if (llvmRetType != LLVMTypeRef.Void)
            {
                // If the function signature says double, make sure we return a double
                if (llvmRetType == LLVMTypeRef.Double && resultValue.TypeOf != LLVMTypeRef.Double)
                {
                    resultValue = _builder.BuildSIToFP(resultValue, LLVMTypeRef.Double, "ret_conv");
                }
                _builder.BuildRet(resultValue);
            }
            else
            {
                _builder.BuildRetVoid();
            }
            Console.WriteLine(_module.PrintToString());

            // 6. Execute
            var res = _engine.RunFunction(func, Array.Empty<LLVMGenericValueRef>());
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
            if (type == MyType.Int) return (int)LLVM.GenericValueToInt(res, 0);
            if (type == MyType.Float)
                return LLVM.GenericValueToFloat(_module.Context.DoubleType, res);
            if (type == MyType.String)
            {
                nint ptr = (nint)LLVM.GenericValueToPointer(res);
                return ptr == 0 ? null : Marshal.PtrToStringAnsi((IntPtr)ptr);
            }
            if (type == MyType.Bool)
            {
                // Check if the 1-bit value is 1
                System.Console.WriteLine("res: " + LLVM.GenericValueToInt(res, 0));
                return LLVM.GenericValueToInt(res, 0) == 1;
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


            // If either side is a Float, we do floating point math
            bool isFloat = (expr.Left.Type == MyType.Float || expr.Right.Type == MyType.Float);

            if (isFloat)
            {

                // For simplicity, you might need to "Promote" an Int to a Float here
                lhs = EnsureFloat(lhs, expr.Left.Type);
                rhs = EnsureFloat(rhs, expr.Right.Type);

                return expr.Operator switch
                {
                    "+" => _builder.BuildFAdd(lhs, rhs, "faddtmp"),
                    "-" => _builder.BuildFSub(lhs, rhs, "fsubtmp"),
                    "*" => _builder.BuildFMul(lhs, rhs, "fmultmp"),
                    "/" => _builder.BuildFDiv(lhs, rhs, "fdivtmp"),
                    _ => throw new InvalidOperationException()
                };
            }
            // Check if we are doing string concatenation
            if (expr.Operator == "+" && (expr.Left.Type == MyType.String || expr.Right.Type == MyType.String))
            {
                return BuildStringConcat(lhs, expr.Left.Type, rhs, expr.Right.Type);
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
            var global = _module.GetNamedGlobal(expr.Id);

            if (global.Handle == IntPtr.Zero)
            {
                LLVMTypeRef llvmType = GetLLVMType(expr.Expression.Type);
                global = _module.AddGlobal(llvmType, expr.Id);

                // Initialize
                if (expr.Expression.Type == MyType.Int)
                    global.Initializer = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
                else
                    global.Initializer = LLVMValueRef.CreateConstNull(llvmType);

                // Update context with the real Global Value
                _context = _context.Add(expr.Id, global, expr.Expression.Type);
            }

            _builder.BuildStore(value, global);
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
            var actualType = entry.Value.Type;
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

            // 1. Initialization: Run the starting statement (e.g., i = 0)
            if (expr.Initialization != null) Visit(expr.Initialization);


            // 2. Define the Basic Blocks
            // We need these labels to handle the "jump" logic
            var condBlock = func.AppendBasicBlock("for.cond");
            var bodyBlock = func.AppendBasicBlock("for.body");
            var stepBlock = func.AppendBasicBlock("for.step");
            var endBlock = func.AppendBasicBlock("for.end");

            // Start by jumping into the condition check
            _builder.BuildBr(condBlock);

            // 3. Condition Block: Check if we should run the loop
            _builder.PositionAtEnd(condBlock);
            var condition = Visit(expr.Condition);

            // Safety check: Ensure the condition is actually a boolean (i1)
            // If your condition returns i32, you might need an ICmp here.
            _builder.BuildCondBr(condition, bodyBlock, endBlock);

            // 4. Body Block: Run the single expression/statement
            _builder.PositionAtEnd(bodyBlock);
            Visit(expr.Body);

            // After the body finishes, jump to the 'step'
            _builder.BuildBr(stepBlock);

            // 5. Step Block: Run the increment (e.g., i++)
            _builder.PositionAtEnd(stepBlock);
            if (expr.Step != null) Visit(expr.Step);
            _builder.BuildBr(condBlock);

            // Jump back to the condition to see if we go again
            _builder.BuildBr(condBlock);

            // 6. End Block: Move the builder here so subsequent code compiles AFTER the loop
            _builder.PositionAtEnd(endBlock);

            // Return a dummy value (Loops are statements and don't usually return data)
            return LLVMValueRef.CreateConstInt(llvmCtx.Int32Type, 0);
        }




        public LLVMValueRef VisitFunctionDef(FunctionDefNode node)
        {
            // 1. Define the function signature (all doubles for simplicity)
            var doubleType = LLVMTypeRef.Double;
            var argTypes = Enumerable.Repeat(doubleType, node.Parameters.Count).ToArray();
            var funcType = LLVMTypeRef.CreateFunction(doubleType, argTypes);

            // 2. Add function to module
            var func = _module.AddFunction(node.Name, funcType);

            // 3. Save the current builder position (the REPL entry)
            var previousBlock = _builder.InsertBlock;

            // 4. Start building the function body
            var entry = func.AppendBasicBlock("entry");
            _builder.PositionAtEnd(entry);

            // 5. Create a NEW local scope for this function
            _localScope = new Dictionary<string, LLVMValueRef>();
            for (int i = 0; i < node.Parameters.Count; i++)
            {
                var paramName = node.Parameters[i];
                var paramValue = func.GetParam((uint)i);
                _localScope[paramName] = paramValue;
            }

            // 6. Compile the body and return the result
            var bodyResult = Visit(node.Body);
            // Ensure the result is a Double to match the signature
            var finalResult = EnsureFloat(bodyResult, MyType.Float);
            _builder.BuildRet(finalResult);

            // 7. CLEAN UP: Remove local scope and go back to where we were
            _localScope = null;
            if (previousBlock.Handle != IntPtr.Zero)
                _builder.PositionAtEnd(previousBlock);

            return func;
        }

        public LLVMValueRef VisitFunctionCall(FunctionCallNode node)
        {
            var func = _module.GetNamedFunction(node.Name);
            if (func.Handle == IntPtr.Zero)
                throw new Exception($"Function {node.Name} not found");

            // 1. Visit all arguments
            var args = new LLVMValueRef[node.Arguments.Count];
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                var argVal = Visit(node.Arguments[i]);
                args[i] = EnsureFloat(argVal, node.Arguments[i].Type);
            }

            // 2. REPLACEMENT FOR GlobalValueType:
            // In LLVM, a function value is actually a pointer to a function. 
            // We need the "FunctionType" itself for BuildCall2.
            LLVMTypeRef funcType = func.TypeOf.ElementType;

            // 3. Robustness Check: If funcType is somehow null (opaque pointers), 
            // we can reconstruct it since we know our functions use Double.
            if (funcType.Handle == IntPtr.Zero)
            {
                var doubleType = LLVMTypeRef.Double;
                var argTypes = Enumerable.Repeat(doubleType, node.Arguments.Count).ToArray();
                funcType = LLVMTypeRef.CreateFunction(doubleType, argTypes);
            }

            // 4. Build the call
            return _builder.BuildCall2(funcType, func, args, "calltmp");
        }


        //------Helper-functions------//

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
                MyType.Float => LLVMTypeRef.Double, // I try this!
                MyType.String => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
                MyType.Bool => LLVMTypeRef.Int1,
                _ => LLVMTypeRef.Void
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