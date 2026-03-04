using System;
using LLVMSharp;
using LLVMSharp.Interop;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace MyCompiler
{
    public unsafe class CompilerOLD : IExpressionVisitor
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

        // 1. Store the type globally so it never gets garbage collected or lost
        private LLVMTypeRef _mallocType;

        public CompilerOLD()
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

        public object Run(NodeExpr expr, bool debug = false)
        {
            // 1. SEMANTIC ANALYSIS (Type Checking)
            // We pass the current _context so the checker knows about existing variables.
            var checker = new TypeChecker(_context);
            checker.Check(expr);

            // Update the compiler's context with any new variables found during type checking
            _context = checker.UpdatedContext;

            // 2. INITIALIZE LLVM
            _module = LLVMModuleRef.CreateWithName("repl_module");
            _builder = _module.Context.CreateBuilder();

            // 3. PREDICT WRAPPER RETURN TYPE
            // Now that the TypeChecker has run, expr.Type is GUARANTEED to be populated.
            var lastNode = GetLastExpression(expr);
            MyType prediction = MyType.None;

            if (lastNode is ExpressionNodeExpr exp)
            {
                prediction = exp.Type;
            }

            // 4. UNIVERSAL WRAPPER SIGNATURE
            LLVMTypeRef llvmRetType = prediction switch
            {
                MyType.Float => LLVMTypeRef.Double,
                MyType.Int => LLVMTypeRef.Double, // Consistency: Numbers are doubles
                MyType.Bool => LLVMTypeRef.Double,
                MyType.String => LLVMTypeRef.Int64,  // Pointers as i64 bits
                MyType.Array => LLVMTypeRef.Int64,  // Pointers as i64 bits
                MyType.None => LLVMTypeRef.Void,
                _ => LLVMTypeRef.Int64
            };

            var funcType = LLVMTypeRef.CreateFunction(llvmRetType, Array.Empty<LLVMTypeRef>());
            var func = _module.AddFunction($"exec_{Guid.NewGuid():N}", funcType);
            _builder.PositionAtEnd(func.AppendBasicBlock("entry"));

            // 5. GENERATE CODE
            var resultValue = Visit(expr);

            // 6. FINALIZE RETURN (The "Boxing" Logic)
            if (llvmRetType == LLVMTypeRef.Void)
            {
                _builder.BuildRetVoid();
            }
            else
            {
                LLVMValueRef finalRet = resultValue;

                // Pointer Handling (Strings/Arrays)
                if (resultValue.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                {
                    finalRet = _builder.BuildPtrToInt(resultValue, LLVMTypeRef.Int64, "ptr_to_i64");
                    if (llvmRetType == LLVMTypeRef.Double)
                        finalRet = _builder.BuildBitCast(finalRet, LLVMTypeRef.Double, "ptr_bits_to_double");
                }
                // Number Handling
                else if (llvmRetType == LLVMTypeRef.Double && finalRet.TypeOf != LLVMTypeRef.Double)
                {
                    finalRet = _builder.BuildSIToFP(finalRet, LLVMTypeRef.Double, "i_to_double");
                }

                _builder.BuildRet(finalRet);
            }

            // 7. EXECUTE
            Console.WriteLine("--- GENERATED IR ---");
            Console.WriteLine(_module.PrintToString());

            using var engine = _module.CreateMCJITCompiler();
            var res = engine.RunFunction(func, Array.Empty<LLVMGenericValueRef>());

            // 8. EXTRACT
            return ExtractResult(res, prediction, expr);
        }

        // --- Helpers to keep Run() clean ---

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

        private object ExtractResult(LLVMGenericValueRef res, MyType type, NodeExpr originalExpr)
        {
            // 0. SILENCE CHECK
            var lastNode = GetLastExpression(originalExpr);
            if (lastNode is AssignNodeExpr || lastNode is PrintNodeExpr || lastNode is IfNodeExpr ||
                lastNode is ForLoopNodeExpr || lastNode is IncrementNodeExpr || lastNode is DecrementNodeExpr)
            {
                return null;
            }

            ulong rawBits = 0;

            // 1. EXTRACTION logic
            if (type == MyType.Float || type == MyType.Int)
            {
                double d = LLVM.GenericValueToFloat(LLVMTypeRef.Double, res);
                rawBits = (ulong)BitConverter.DoubleToInt64Bits(d);
            }
            else if (type == MyType.String || type == MyType.Array)
            {
                // Fix: Use the static LLVM helper instead of .ToInt()
                rawBits = LLVM.GenericValueToInt(res, 0);

                // Fallback: If your Run() method bitcasted the pointer to a double for the wrapper
                if (rawBits == 0)
                {
                    double d = LLVM.GenericValueToFloat(LLVMTypeRef.Double, res);
                    rawBits = (ulong)BitConverter.DoubleToInt64Bits(d);
                }
            }
            else if (type == MyType.Bool)
            {
                // Fix: Use the static LLVM helper
                rawBits = LLVM.GenericValueToInt(res, 0);
            }

            // 2. INTERPRETATION
            if (rawBits == 0 && type != MyType.Float && type != MyType.Bool) return "null";

            switch (type)
            {
                case MyType.Float:
                case MyType.Int:
                    return BitConverter.UInt64BitsToDouble(rawBits);

                case MyType.String:
                    try
                    {
                        return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(new IntPtr((long)rawBits)) ?? "null";
                    }
                    catch { return "null"; }

                case MyType.Array:
                    try
                    {
                        IntPtr address = new IntPtr((long)rawBits);
                        int count = 0;
                        if (lastNode is ArrayNodeExpr arr) count = arr.Elements.Count;
                        else count = 0; // Default or lookup logic

                        List<string> elements = new List<string>();
                        for (int i = 0; i < count; i++)
                        {
                            long boxedVal = System.Runtime.InteropServices.Marshal.ReadInt64(address, i * 8);
                            double dblVal = BitConverter.Int64BitsToDouble(boxedVal);
                            // CultureInfo.InvariantCulture ensures dots for decimals
                            elements.Add(dblVal.ToString(CultureInfo.InvariantCulture));
                        }
                        return "[" + string.Join(", ", elements) + "]";
                    }
                    catch { return $"[Array @ 0x{rawBits:X}]"; }

                case MyType.Bool:
                    // Assuming bools come back as bits inside a double from your wrapper
                    double bVal = LLVM.GenericValueToFloat(LLVMTypeRef.Double, res);
                    return bVal != 0.0;

                default:
                    return rawBits;
            }
        }

        public LLVMValueRef VisitNumberExpr(NumberNodeExpr expr)
        {
            return LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, expr.Value);
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
            // Use 1.0 for True to match your Int-to-Bool promotion
            return LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, expr.Value ? 1.0 : 0.0);
        }

        public LLVMValueRef VisitStringExpr(StringNodeExpr expr)
        {
            return _builder.BuildGlobalStringPtr(expr.Value, "str");
        }
        public LLVMValueRef VisitFloatExpr(FloatNodeExpr expr)
        {
            return LLVMValueRef.CreateConstReal(_module.Context.DoubleType, expr.Value);
        }


        public LLVMValueRef VisitBinaryExpr(BinaryOpNodeExpr expr)
        {
            // 1. Get the types assigned by the TypeChecker
            var leftType = expr.Left.Type;
            var rightType = expr.Right.Type;

            // 2. PRIORITY: Numeric Math
            // If both sides are numeric (Int/Float), use standard LLVM math instructions.
            bool isNumeric = (leftType == MyType.Int || leftType == MyType.Float) &&
                             (rightType == MyType.Int || rightType == MyType.Float);

            if (isNumeric)
            {
                var lhs = EnsureFloat(Visit(expr.Left), leftType);
                var rhs = EnsureFloat(Visit(expr.Right), rightType);

                return expr.Operator switch
                {
                    "+" => _builder.BuildFAdd(lhs, rhs, "faddtmp"),
                    "-" => _builder.BuildFSub(lhs, rhs, "fsubtmp"),
                    "*" => _builder.BuildFMul(lhs, rhs, "fmultmp"),
                    "/" => _builder.BuildFDiv(lhs, rhs, "fdivtmp"),
                    _ => throw new InvalidOperationException($"Math operator {expr.Operator} not supported for numeric types")
                };
            }

            // 3. SECONDARY: String Concatenation
            // Only if it's a '+' and at least one side is a String (or a pointer address)
            var lhsVal = Visit(expr.Left);
            var rhsVal = Visit(expr.Right);

            bool lhsIsPtr = lhsVal.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind;
            bool rhsIsPtr = rhsVal.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind;

            if (expr.Operator == "+" && (leftType == MyType.String || rightType == MyType.String || lhsIsPtr || rhsIsPtr))
            {
                // Use the metadata types to guide BuildStringConcat
                return BuildStringConcat(lhsVal, leftType, rhsVal, rightType);
            }

            // 4. FALLBACK: Integer/Other Math
            return expr.Operator switch
            {
                "+" => _builder.BuildAdd(lhsVal, rhsVal, "addtmp"),
                "-" => _builder.BuildSub(lhsVal, rhsVal, "subtmp"),
                "==" => _builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, lhsVal, rhsVal, "cmptmp"),
                _ => throw new InvalidOperationException($"Unknown operator {expr.Operator} for types {leftType} and {rightType}")
            };
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
        public LLVMValueRef VisitIndexExpr(IndexNodeExpr expr)
        {
            var arrayPtr = Visit(expr.ArrayExpression);
            var index = Visit(expr.IndexExpression);

            // 1. Index must be i32 for GEP
            if (index.TypeOf == LLVMTypeRef.Double)
                index = _builder.BuildFPToSI(index, LLVMTypeRef.Int32, "idx_int");

            // 2. Load the value from the i64 array
            var elementPtr = _builder.BuildGEP2(LLVMTypeRef.Int64, arrayPtr, new[] { index }, "ptr");
            var boxed = _builder.BuildLoad2(LLVMTypeRef.Int64, elementPtr, "val");

            // 3. THE FIX: Bitcast for Numbers, IntToPtr for Strings
            if (expr.Type == MyType.Float || expr.Type == MyType.Int)
            {
                // "Treat these 64 bits as a Double"
                return _builder.BuildBitCast(boxed, LLVMTypeRef.Double, "i2d");
            }

            if (expr.Type == MyType.String)
            {
                // "Treat these 64 bits as a memory address"
                var opaquePtr = LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0);
                return _builder.BuildIntToPtr(boxed, opaquePtr, "i2ptr");
            }

            return boxed;
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

        private LLVMValueRef UnboxFromI64(LLVMValueRef boxed, MyType target)
        {
            if (target == MyType.Float || target == MyType.Int)
                return _builder.BuildBitCast(boxed, LLVMTypeRef.Double, "i2d");

            if (target == MyType.String || target == MyType.Array)
                return _builder.BuildIntToPtr(boxed, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0), "i2p");

            return boxed;
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

        public LLVMValueRef VisitAssignExpr(AssignNodeExpr expr)
        {
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


        public LLVMValueRef VisitIdExpr(IdNodeExpr expr)
        {
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

        public LLVMValueRef VisitIncrementExpr(IncrementNodeExpr expr)
        {
            var entry = _context.Get(expr.Id)
                ?? throw new Exception($"Variable {expr.Id} not defined");

            var varPtr = entry.Value;
            var value = _builder.BuildLoad2(LLVMTypeRef.Double, varPtr, "inc_load");
            var newValue = _builder.BuildFAdd(value, LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, 1.0), "inc_add");

            _builder.BuildStore(newValue, varPtr);
            return newValue;
        }

        public LLVMValueRef VisitDecrementExpr(DecrementNodeExpr expr)
        {
            var entry = _context.Get(expr.Id)
                ?? throw new Exception($"Variable {expr.Id} not defined");

            var varPtr = entry.Value;
            var value = _builder.BuildLoad2(LLVMTypeRef.Double, varPtr, "dec_load");
            var newValue = _builder.BuildFSub(value, LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, 1.0), "dec_sub");

            _builder.BuildStore(newValue, varPtr);
            return newValue;
        }


        // Update your IExpressionVisitor interface to include this
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
            return default;
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

            // 1. STRINGS
            if (expr.Expression.Type == MyType.String)
            {
                if (valueToPrint.TypeOf.Kind != LLVMTypeKind.LLVMPointerTypeKind)
                {
                    // If it's bits in a double, bitcast to i64 first
                    if (valueToPrint.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
                        valueToPrint = _builder.BuildBitCast(valueToPrint, LLVMTypeRef.Int64, "bits");

                    // Convert i64 address to actual Pointer
                    valueToPrint = _builder.BuildIntToPtr(valueToPrint, LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0), "ptr");
                }
                formatStr = _builder.BuildGlobalStringPtr("%s\n", "print_str_fmt");
            }
            // 2. NUMBERS (Including those loaded from Arrays)
            else
            {
                // If the value is an i64 (which happens when loading from our generic i64 array slots),
                // we MUST bitcast it back to Double so printf %f can read it correctly.
                if (valueToPrint.TypeOf.Kind == LLVMTypeKind.LLVMIntegerTypeKind && valueToPrint.TypeOf.IntWidth == 64)
                {
                    valueToPrint = _builder.BuildBitCast(valueToPrint, LLVMTypeRef.Double, "array_val_to_dbl");
                    formatStr = _builder.BuildGlobalStringPtr("%f\n", "print_float_fmt");
                }
                else if (valueToPrint.TypeOf.Kind == LLVMTypeKind.LLVMDoubleTypeKind)
                {
                    formatStr = _builder.BuildGlobalStringPtr("%f\n", "print_float_fmt");
                }
                else
                {
                    formatStr = _builder.BuildGlobalStringPtr("%d\n", "print_int_fmt");
                }
            }

            var printfType = LLVMTypeRef.CreateFunction(llvmCtx.Int32Type,
                new[] { LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0) }, true);

            return _builder.BuildCall2(printfType, printf, new[] { formatStr, valueToPrint }, "printcall");
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


        public LLVMValueRef VisitFunctionDef(FunctionDefNode node)
        {
            // 1. Map the Return Type (Fixes error CS0103 for llvmRetType)
            MyType retType = node.ReturnTypeName;
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
                "array" => MyType.Array,
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

                // If it's a string pointer, bitcast it to a double
                if (node.Arguments[i].Type == MyType.String)
                {
                    // Pointer -> i64 -> double
                    var asInt = _builder.BuildPtrToInt(val, LLVMTypeRef.Int64, "ptr_to_i64");
                    val = _builder.BuildBitCast(asInt, LLVMTypeRef.Double, "ptr_as_double");
                }
                else if (val.TypeOf != LLVMTypeRef.Double)
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
            var ptrType = LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0);

            // --- INTERNAL HELPER: Prepare individual arguments for sprintf ---
            LLVMValueRef PrepareArg(LLVMValueRef val, MyType type)
            {
                if (type == MyType.String)
                {
                    // Case A: It's already an LLVM Pointer (e.g., a global constant "Hello ")
                    if (val.TypeOf.Kind == LLVMTypeKind.LLVMPointerTypeKind)
                    {
                        return val;
                    }

                    // Case B: It's a "Smuggled String" inside a Double (e.g., parameter 'x')
                    // We BitCast bits to i64, then turn that i64 into a Pointer
                    var asInt64 = _builder.BuildBitCast(val, llvmCtx.Int64Type, "smuggled_to_i64");
                    return _builder.BuildIntToPtr(asInt64, ptrType, "i64_to_ptr");
                }

                // Case C: It's a legitimate number (Int/Float/Bool)
                // Convert to i32 so sprintf can use %d
                return _builder.BuildFPToSI(val, llvmCtx.Int32Type, "num_to_i32");
            }

            // Prepare both sides
            var lhsFinal = PrepareArg(lhs, lhsType);
            var rhsFinal = PrepareArg(rhs, rhsType);

            // 1. Allocate 256 bytes for the result string
            var buffer = _builder.BuildCall2(
                LLVMTypeRef.CreateFunction(ptrType, new[] { llvmCtx.Int64Type }),
                malloc,
                new[] { LLVMValueRef.CreateConstInt(llvmCtx.Int64Type, 256) },
                "concat_buf"
            );

            // 2. Generate the dynamic format string based on the types
            // This ensures we get "%s%s" for two strings
            string fmtStr = (lhsType == MyType.String ? "%s" : "%d") +
                            (rhsType == MyType.String ? "%s" : "%d");

            var fmtPtr = _builder.BuildGlobalStringPtr(fmtStr, "concat_fmt");

            // 3. Call sprintf(buffer, fmt, lhs, rhs)
            // We use a variadic function type: (ptr, ptr, ...) -> i32
            var sprintfType = LLVMTypeRef.CreateFunction(
                llvmCtx.Int32Type,
                new[] { ptrType, ptrType },
                true // IsVariadic
            );

            _builder.BuildCall2(sprintfType, sprintf, new[] { buffer, fmtPtr, lhsFinal, rhsFinal }, "sprintf_call");

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
                MyType.Int => LLVMTypeRef.Double,
                MyType.Float => LLVMTypeRef.Double,
                MyType.Bool => LLVMTypeRef.Int1,
                MyType.String => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0), // Must be a pointer!
                MyType.None => LLVMTypeRef.Void,
                MyType.Array => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
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