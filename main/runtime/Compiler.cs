using System;
using LLVMSharp;
using LLVMSharp.Interop;
// using Collections.Generic;
// using Linq;
using System.Runtime.InteropServices;

namespace MyCompiler
{
    public unsafe class Compiler : IExpressionVisitor
    {
        private LLVMValueRef _printf;
        private LLVMTypeRef _printfType;
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

            // _module = LLVMModuleRef.CreateWithName("base");
            // _builder = _module.Context.CreateBuilder();
            // _engine = _module.CreateMCJITCompiler();
            _context = Context.Empty; // stores variables/functions
        }

        private void InitializeModule()
        {
            _module = LLVMModuleRef.CreateWithName("KaleidoscopeModule");
            _builder = _module.Context.CreateBuilder();
            _passBuilderOptions = LLVM.CreatePassBuilderOptions();
            _engine = _module.CreateMCJITCompiler();
        }

        void PrintAST(NodeExpr node, string indent = "")
        {
            switch (node)
            {
                case SequenceNodeExpr s:
                    Console.WriteLine(indent + $"Sequence: ");
                    PrintAST(s.Statements[0], indent + "  ");
                    break;
                case StringNodeExpr str:
                    Console.WriteLine(indent + $"String: {str.Value}");
                    break;
                case NumberNodeExpr n:
                    Console.WriteLine(indent + $"Number: {n.Value}");
                    break;
                case BooleanNodeExpr b:
                    Console.WriteLine(indent + $"Boolean: {b.Value}");
                    break;
                case BinaryOpNodeExpr bin:
                    Console.WriteLine(indent + $"BinaryOp: {bin.Operator}");
                    PrintAST(bin.Left, indent + "  ");
                    PrintAST(bin.Right, indent + "  ");
                    break;
                case ComparisonNodeExpr cmp:
                    Console.WriteLine(indent + $"Comparison: {cmp.Operator}");
                    PrintAST(cmp.Left, indent + "  ");
                    PrintAST(cmp.Right, indent + "  ");
                    break;
            }
        }

        private void DeclarePrintf()
        {
            // Fix: Use _module.Context to get the LLVM types
            var llvmCtx = _module.Context;
            _printfType = LLVMTypeRef.CreateFunction(
                  llvmCtx.Int32Type,
                new[] { LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0) },
                true); // varargs

            _printf = _module.AddFunction("printf", _printfType);
        }


        private LLVMValueRef CreateFormatString(string format)
        {
            var str = _builder.BuildGlobalStringPtr(format, "fmt");
            return str;
        }

        private LLVMValueRef _trueStr;
        private LLVMValueRef _falseStr;

        private void DeclareBoolStrings()
        {
            _trueStr = _builder.BuildGlobalStringPtr("True\n", "true_str");
            _falseStr = _builder.BuildGlobalStringPtr("False\n", "false_str");
        }
        private void BuildAutoPrint(LLVMValueRef value, MyType type)
        {
            LLVMValueRef formatStr;

            switch (type)
            {
                case MyType.Int:
                    formatStr = CreateFormatString("%d\n");
                    break;

                case MyType.String:
                    formatStr = CreateFormatString("%s\n");
                    break;

           case MyType.Bool:
{
    var selectedStr = _builder.BuildSelect(
        value,        // i1 condition
        _trueStr,
        _falseStr,
        "boolstr");

    _builder.BuildCall2(
        _printfType,
        _printf,
        new[] { selectedStr },
        "");

    return;
}

                default:
                    return; // don't auto print unsupported types
            }

            _builder.BuildCall2(
                _printfType,
                _printf,
                new LLVMValueRef[] { formatStr, value },
                ""
            );
        }

        // NOTES
        // Compiler → runtime code generation
        // TypeChecker → static type inference

        public object Run(NodeExpr expr, bool generateIR = false)
        {
            InitializeModule();
            ExpressionNodeExpr lastExpr = GetLastExpression(expr);

            // 🔥 TYPE CHECK FIRST
            PrintAST(expr);
            var typeChecker = new TypeChecker(_context);
            MyType returnType = typeChecker.Check(expr);
            _context = typeChecker.UpdatedContext;

            LLVMTypeRef llvmReturnType = LLVMTypeRef.Int32;
            //LLVMTypeRef llvmReturnType = GetLLVMType(returnType);

            var funcType = LLVMTypeRef.CreateFunction(
                llvmReturnType,
                Array.Empty<LLVMTypeRef>(),
                false);

            var func = _module.AddFunction("main", funcType);
            //var func = _module.AddFunction($"exec_{Guid.NewGuid():N}", funcType);
            var entry = func.AppendBasicBlock("entry");
            _builder.PositionAtEnd(entry);

            var resultValue = Visit(expr);
            DeclarePrintf();
            DeclareBoolStrings();

            // if (returnType == MyType.None)
            //     _builder.BuildRetVoid();
            // else if (returnType == MyType.Array && lastExpr is ArrayNodeExpr)
            //     _builder.BuildRetVoid();
            // else
            //     _builder.BuildRet(resultValue);

            //var resultValue = Visit(expr);

            if (returnType != MyType.None && !(lastExpr is PrintNodeExpr))
            {
                BuildAutoPrint(resultValue, returnType);
            }

            _builder.BuildRet(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0));

            // should we just add print func to print the result always(given we return a value of course)?
            //A.Implicit Output for Return Values

            // If you want your compiler to automatically print results from function calls 
            // or return values without explicitly using printf, 
            // you can implement the behavior of automatically printing the return value of a function(or the value of an expression).
            // This will mimic the behavior seen in interactive notebooks

            if (generateIR)
            {
                // Generate LLVM IR and write to a file
                string llvmIR = _module.PrintToString();
                File.WriteAllText("output_actual.ll", llvmIR); // save to file for further compilation
            }

            Console.WriteLine("Generated LLVM IR:\n" + _module.PrintToString());

            Console.WriteLine("the res1:");
            var res = _engine.RunFunction(func, Array.Empty<LLVMGenericValueRef>());

            // Console.WriteLine("the res2:");
            // var d = ExtractResults(res, returnType);
            // Console.WriteLine("the res:" + d);
            return ExtractResults(res, returnType, lastExpr);
        }

        public LLVMValueRef VisitIdExpr(IdNodeExpr expr) // the old and working one
        {
            // Console.WriteLine("we calling visit id");
            var variable = _context.Get(expr.Name);

            if (variable == null)
                throw new Exception($"Undefined variable '{expr.Name}'");

            var llvmType = GetLLVMType(variable.Value.Type);

            return _builder.BuildLoad2(llvmType, variable.Value.Value, expr.Name);
        }

        public LLVMValueRef VisitAssignExpr(AssignNodeExpr expr) // the old and working one
        {
            var value = Visit(expr.Expression);
            //expr.SetType(expr.Expression.Type); 
            int? length = null;
            var global = _module.GetNamedGlobal(expr.Id);

            if (global.Handle == IntPtr.Zero)
            {
                LLVMTypeRef llvmType = GetLLVMType(expr.Expression.Type);
                global = _module.AddGlobal(llvmType, expr.Id);

                // Initialize
                if (expr.Expression.Type == MyType.Int)
                    global.Initializer = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false);
                else if (expr.Expression.Type == MyType.Array)
                {
                    // i didn't see it run the first line here or this ones if comparison
                    var ptrType = LLVMTypeRef.CreatePointer(_module.Context.Int32Type, 0);
                    global.Initializer = LLVMValueRef.CreateConstPointerNull(ptrType);
                }
                else if (expr.Expression.Type == MyType.Array)
                {
                    // Initialize
                    global.Initializer = LLVMValueRef.CreateConstPointerNull(llvmType);

                    if (expr.Expression is ArrayNodeExpr arr)
                        length = arr.Elements.Count;

                }
                else
                    // bro it calls this else as well, it makes no sense
                    global.Initializer = LLVMValueRef.CreateConstNull(llvmType);

                // Update context with the real Global Value
            }
            _context = _context.Add(expr.Id, global, expr.Expression.Type, length);

            _builder.BuildStore(value, global);
            return default;
        }

        public LLVMValueRef VisitBinaryExpr(BinaryOpNodeExpr expr)
        {
            var left = Visit(expr.Left);
            var right = Visit(expr.Right);

            if (expr.Operator == "+")
            {
                if (expr.Left.Type == MyType.Int)
                    return _builder.BuildAdd(left, right, "a");

                if (expr.Left.Type == MyType.String)
                    return BuildStringConcat(left, right);
            }

            switch (expr.Operator)
            {
                case "-": return _builder.BuildSub(left, right, "s");
                case "*": return _builder.BuildMul(left, right, "m");
                case "/": return _builder.BuildSDiv(left, right, "d");
            }

            throw new InvalidOperationException("Unknown operator");
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
            Console.WriteLine("we here to increment");
            var entry = _context.Get(expr.Id)
                ?? throw new InvalidOperationException($"Variable {expr.Id} not defined");

            var valueRef = entry.Value;
            var current = _builder.BuildLoad2(LLVMTypeRef.Int32, valueRef, expr.Id);
            var incremented = _builder.BuildAdd(current, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 1, false), "inc");
            _builder.BuildStore(incremented, valueRef);

            return incremented;
        }

        public LLVMValueRef VisitComparisonExpr(ComparisonNodeExpr expr)
        {
            var leftVal = Visit(expr.Left);
            var rightVal = Visit(expr.Right);

            // STRING COMPARISON
            if (expr.Left.Type == MyType.String)
            {
                return BuildStringComparison(leftVal, rightVal, expr.Operator);
            }

            // INTEGER COMPARISON
            return expr.Operator switch
            {
                "<" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, leftVal, rightVal, "cmptmp"),
                ">" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, leftVal, rightVal, "cmptmp"),
                "<=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, leftVal, rightVal, "cmptmp"),
                ">=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, leftVal, rightVal, "cmptmp"),
                "==" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, leftVal, rightVal, "cmptmp"),
                "!=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, leftVal, rightVal, "cmptmp"),
                _ => throw new Exception("Unknown comparison operator")
            };
        }

        public LLVMValueRef VisitNumberExpr(NumberNodeExpr expr)
        {
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)expr.Value, false);
        }
        public LLVMValueRef VisitStringExpr(StringNodeExpr expr)
        {
            return _builder.BuildGlobalStringPtr(expr.Value, "str");
        }
        public LLVMValueRef VisitBooleanExpr(BooleanNodeExpr expr)
        {
            return LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, expr.Value ? 1UL : 0UL, false);
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

            if (expr.Expression.Type == MyType.Int)
                formatStr = _builder.BuildGlobalStringPtr("%d\n", "print_int_fmt");
            else if (expr.Expression.Type == MyType.String)
                formatStr = _builder.BuildGlobalStringPtr("%s\n", "print_str_fmt");
            else if (expr.Expression.Type == MyType.Bool)
                formatStr = _builder.BuildGlobalStringPtr("%b\n", "print_bool_fmt");
            else
                throw new Exception("Cannot print this type");

            // Call printf but IGNORE return value, by default we returned this func, but that doesn' return the value
            _builder.BuildCall2(
                LLVMTypeRef.CreateFunction(
                    llvmCtx.Int32Type,
                    new[] { LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0) },
                    true),
                printf,
                new[] { formatStr, valueToPrint },
                "printcall"
            );

            // 🔥 Return the printed value, not printf result
            return valueToPrint;
        }
        public LLVMValueRef VisitForLoopExpr(ForLoopNodeExpr expr)
        {
            var func = _builder.InsertBlock.Parent;

            // Blocks
            var condBlock = func.AppendBasicBlock("for.cond");
            var bodyBlock = func.AppendBasicBlock("for.body");
            var stepBlock = func.AppendBasicBlock("for.step");
            var endBlock = func.AppendBasicBlock("for.end");

            // 1️⃣ Initialization
            if (expr.Initialization != null)
                Visit(expr.Initialization);

            _builder.BuildBr(condBlock);

            // 2️⃣ Condition
            _builder.PositionAtEnd(condBlock);
            LLVMValueRef condValue = expr.Condition != null
                ? Visit(expr.Condition)
                : LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 1, false);
            _builder.BuildCondBr(condValue, bodyBlock, endBlock);

            // 3️⃣ Body
            _builder.PositionAtEnd(bodyBlock);
            if (expr.Body != null)
                Visit(expr.Body);

            // Jump to step (or directly to cond if step is null)
            _builder.BuildBr(expr.Step != null ? stepBlock : condBlock);

            // 4️⃣ Step
            _builder.PositionAtEnd(stepBlock);
            if (expr.Step != null)
                Visit(expr.Step); // This will increment 'i'
            _builder.BuildBr(condBlock);

            // 5️⃣ End
            _builder.PositionAtEnd(endBlock);

            return default;
        }

        public LLVMValueRef VisitArrayExpr(ArrayNodeExpr expr)
        {
            var llvmCtx = _module.Context;
            int length = expr.Elements.Count;

            var elementType = llvmCtx.Int32Type;

            // --- calculate total size in bytes ---
            ulong elementSize = 4; // i32 = 4 bytes
            ulong totalSize = (ulong)length * elementSize;

            var sizeValue = LLVMValueRef.CreateConstInt(
                llvmCtx.Int64Type,
                totalSize,
                false
            );

            // --- call malloc ---
            var malloc = GetMalloc();

            var rawPtr = _builder.BuildCall2(
                LLVMTypeRef.CreateFunction(
                    LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0),
                    new[] { llvmCtx.Int64Type },
                    false
                ),
                malloc,
                new[] { sizeValue },
                "malloccall"
            );

            // --- cast i8* → i32* ---
            var arrayPtr = _builder.BuildBitCast(
                rawPtr,
                LLVMTypeRef.CreatePointer(elementType, 0),
                "arrayptr"
            );

            // --- store elements ---
            for (int i = 0; i < length; i++)
            {
                var elemValue = Visit(expr.Elements[i]);

                var elemPtr = _builder.BuildGEP2(
                    elementType,
                    arrayPtr,
                    new LLVMValueRef[]
                    {
                        LLVMValueRef.CreateConstInt(llvmCtx.Int32Type, (ulong)i, false)
                    },
                    "elemptr"
                );

                _builder.BuildStore(elemValue, elemPtr);
            }

            return arrayPtr; // SAFE to return (heap memory)
        }

        public LLVMValueRef VisitIfExpr(IfNodeExpr expr)
        {
            var condValue = Visit(expr.Condition);
            var func = _builder.InsertBlock.Parent;

            var thenBlock = func.AppendBasicBlock("then");
            var elseBlock = func.AppendBasicBlock("else");
            var mergeBlock = func.AppendBasicBlock("ifcont");

            _builder.BuildCondBr(condValue, thenBlock, elseBlock);

            // THEN
            _builder.PositionAtEnd(thenBlock);
            var thenVal = Visit(expr.ThenPart);
            _builder.BuildBr(mergeBlock);
            thenBlock = _builder.InsertBlock;

            // ELSE
            LLVMValueRef elseVal = default;
            _builder.PositionAtEnd(elseBlock);
            if (expr.ElsePart != null)
            {
                elseVal = Visit(expr.ElsePart);
            }
            _builder.BuildBr(mergeBlock);
            elseBlock = _builder.InsertBlock;

            _builder.PositionAtEnd(mergeBlock);

            if (expr.Type == MyType.None)
                return default;

            var llvmType = GetLLVMType(expr.Type);

            // 🔥 If one side returned default, replace with zero value
            if (thenVal.Handle == IntPtr.Zero)
                thenVal = LLVMValueRef.CreateConstNull(llvmType);

            if (elseVal.Handle == IntPtr.Zero)
                elseVal = LLVMValueRef.CreateConstNull(llvmType);

            var phi = _builder.BuildPhi(llvmType, "iftmp");

            phi.AddIncoming(
                new[] { thenVal, elseVal },
                new[] { thenBlock, elseBlock },
                2);

            return phi;
        }

        public LLVMValueRef VisitSequenceExpr(SequenceNodeExpr expr)
        {
            LLVMValueRef last = default;

            foreach (var stmt in expr.Statements)
            {
                // Console.WriteLine("The stmt: " + stmt);
                last = Visit(stmt);
            }
            return last;
        }

        private LLVMValueRef GetPrintf() // why are we even calling this when Im never calling the print func?
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

            // here is says the arrayNodeExpr has id=x and type=int / should the array node have the type array?
            // and it says that the idNodeExpr has name=x and type=array
            // also why does arrayNodeExpr have an id value? ah it is because it is assignodes expression
        }

        private LLVMValueRef BuildStringConcat(LLVMValueRef left, LLVMValueRef right)
        {
            var ctx = _module.Context;

            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var i32 = ctx.Int32Type;

            // ---- strlen ----
            var strlenType = LLVMTypeRef.CreateFunction(i32, new[] { i8Ptr }, false);
            var strlen = _module.GetNamedFunction("strlen");
            if (strlen.Handle == IntPtr.Zero)
                strlen = _module.AddFunction("strlen", strlenType);

            // ---- malloc ----
            var mallocType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i32 }, false);
            var malloc = _module.GetNamedFunction("malloc");
            if (malloc.Handle == IntPtr.Zero)
                malloc = _module.AddFunction("malloc", mallocType);

            // ---- strcpy ----
            var strcpyType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i8Ptr, i8Ptr }, false);
            var strcpy = _module.GetNamedFunction("strcpy");
            if (strcpy.Handle == IntPtr.Zero)
                strcpy = _module.AddFunction("strcpy", strcpyType);

            // ---- strcat ----
            var strcatType = LLVMTypeRef.CreateFunction(i8Ptr, new[] { i8Ptr, i8Ptr }, false);
            var strcat = _module.GetNamedFunction("strcat");
            if (strcat.Handle == IntPtr.Zero)
                strcat = _module.AddFunction("strcat", strcatType);

            // length(left)
            var leftLen = _builder.BuildCall2(strlenType, strlen, new[] { left }, "leftLen");

            // length(right)
            var rightLen = _builder.BuildCall2(strlenType, strlen, new[] { right }, "rightLen");

            // totalLen = leftLen + rightLen + 1
            var sumLen = _builder.BuildAdd(leftLen, rightLen, "sumLen");
            var one = LLVMValueRef.CreateConstInt(i32, 1, false);
            var totalLen = _builder.BuildAdd(sumLen, one, "totalLen");

            // allocate memory
            var resultPtr = _builder.BuildCall2(mallocType, malloc, new[] { totalLen }, "mallocCall");

            // strcpy(resultPtr, left)
            _builder.BuildCall2(strcpyType, strcpy, new[] { resultPtr, left }, "");

            // strcat(resultPtr, right)
            _builder.BuildCall2(strcatType, strcat, new[] { resultPtr, right }, "");

            return resultPtr;
        }
        private LLVMValueRef BuildStringComparison(LLVMValueRef left, LLVMValueRef right, string op)
        {
            var ctx = _module.Context;

            var i8Ptr = LLVMTypeRef.CreatePointer(ctx.Int8Type, 0);
            var i32 = ctx.Int32Type;

            // declare strcmp
            var strcmpType = LLVMTypeRef.CreateFunction(i32, new[] { i8Ptr, i8Ptr }, false);
            var strcmp = _module.GetNamedFunction("strcmp");
            if (strcmp.Handle == IntPtr.Zero)
                strcmp = _module.AddFunction("strcmp", strcmpType);

            var cmpResult = _builder.BuildCall2(strcmpType, strcmp, new[] { left, right }, "strcmpCall");

            var zero = LLVMValueRef.CreateConstInt(i32, 0, false);

            return op switch
            {
                "==" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, cmpResult, zero, "streq"),
                "!=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, cmpResult, zero, "strne"),
                _ => throw new Exception("Invalid string comparison operator")
            };
        }

        private object ExtractResults(LLVMGenericValueRef res, MyType type, NodeExpr lastExpr = null)
        {
            int? length = 0;
            if (type == MyType.Int)
                return (int)LLVM.GenericValueToInt(res, 0);
            if (type == MyType.None)
                return null;
            if (type == MyType.String)
            {
                nint ptr = (nint)LLVM.GenericValueToPointer(res);
                return ptr == 0 ? null : Marshal.PtrToStringAnsi((IntPtr)ptr);
            }
            if (type == MyType.Bool)
                return LLVM.GenericValueToInt(res, 0) == 1;
            if (type == MyType.Array) // alright it knows that type is array
            {
                if (type == MyType.Array)
                {
                    nint ptr = (nint)LLVM.GenericValueToPointer(res);

                    var lastNode = GetLastNode(lastExpr);

                    if (lastExpr is IdNodeExpr id)
                    {
                        var entry = _context.Get(id.Name);
                        length = entry.Value.length ?? 0;
                    }
                    else if (lastExpr is ArrayNodeExpr arr)
                    {
                        length = arr.Elements.Count;
                    }
                    else
                    {
                        throw new Exception("Cannot determine array length.");
                    }
                    int l = (int)length;
                    int[] result = new int[l];

                    for (int i = 0; i < l; i++)
                        result[i] = Marshal.ReadInt32((IntPtr)(ptr + i * 4));

                    return result;
                }
            }

            System.Console.WriteLine("we return nothing");
            return null;
        }

        private LLVMValueRef GetMalloc()
        {
            var malloc = _module.GetNamedFunction("malloc");
            if (malloc.Handle != IntPtr.Zero)
                return malloc;

            var llvmCtx = _module.Context;

            var mallocType = LLVMTypeRef.CreateFunction(
                LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0), // returns i8*
                new[] { llvmCtx.Int64Type },                    // takes size (i64)
                false
            );

            return _module.AddFunction("malloc", mallocType);
        }

        private LLVMTypeRef GetLLVMType(MyType type)
        {
            return type switch
            {
                MyType.Int => LLVMTypeRef.Int32,
                MyType.String => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0),
                MyType.Array => LLVMTypeRef.CreatePointer(LLVMTypeRef.Int32, 0),
                MyType.Bool => LLVMTypeRef.Int1,
                _ => LLVMTypeRef.Void
            };
        }

        private ExpressionNodeExpr GetLastExpression(NodeExpr expr)
        {
            if (expr is SequenceNodeExpr seq)
                return seq.Statements.LastOrDefault() as ExpressionNodeExpr;
            return expr as ExpressionNodeExpr;
        }
        private NodeExpr GetLastNode(NodeExpr expr)
        {
            if (expr is SequenceNodeExpr seq)
                return seq.Statements.LastOrDefault() as NodeExpr;
            return expr as NodeExpr;
        }

        private LLVMValueRef Visit(NodeExpr expr)
        {
            return expr.Accept(this);
        }
    }
}