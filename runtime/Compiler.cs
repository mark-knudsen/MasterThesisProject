using System;
using LLVMSharp;
using LLVMSharp.Interop;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MyCompiler
{
    public unsafe class Compiler : IExpressionVisitor
    {
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

            _module = LLVMModuleRef.CreateWithName("base");
            _builder = _module.Context.CreateBuilder();
            _engine = _module.CreateMCJITCompiler();
            _context = Context.Empty; // stores variables/functions
        }

        private void InitializeModule()
        {
            _module = LLVMModuleRef.CreateWithName("KaleidoscopeModule");
            _builder = _module.Context.CreateBuilder();
            _passBuilderOptions = LLVM.CreatePassBuilderOptions();
            _engine = _module.CreateMCJITCompiler();
        }

public object Run(NodeExpr expr)
{
    // 1. Determine last expression
    ExpressionNodeExpr lastExpr = GetLastExpression(expr);

    InitializeModule();

    // SAFETY CHECK
    if (lastExpr == null)
        throw new Exception("No expression to execute.");

    // DEBUG
    Console.WriteLine("LastExpr.Type BEFORE codegen = " + lastExpr.Type);

    // 2. Determine correct LLVM return type FROM AST
    LLVMTypeRef retType = GetLLVMType(lastExpr.Type);

    // 3. Create function with correct return type
    var funcType = LLVMTypeRef.CreateFunction(
        retType,
        Array.Empty<LLVMTypeRef>(),
        false
    );

    var func = _module.AddFunction(
        $"exec_{Guid.NewGuid():N}",
        funcType
    );

    var entry = func.AppendBasicBlock("entry");
    _builder.PositionAtEnd(entry);

    // 4. Generate IR
    var resultValue = Visit(expr);

    // 5. Return correctly
    if (lastExpr.Type == MyType.None)
        _builder.BuildRetVoid();
    else
        _builder.BuildRet(resultValue);

    Console.WriteLine(_module.PrintToString());

    // 6. Execute
    Console.WriteLine("ABOUT TO RUN THE PROGRAM");
    var res = _engine.RunFunction(func, Array.Empty<LLVMGenericValueRef>());
    Console.WriteLine("WE RAN THE PROGRAM");

    return ExtractResult(res, lastExpr.Type, lastExpr);
}

        // public object Run(NodeExpr expr)
        // {
        //     // 1. Initialize Module & Engine
        //     // _module = LLVMModuleRef.CreateWithName("repl_module"); // do we need this? we call it elsewhere
        //     // _builder = _module.Context.CreateBuilder();
        //     // _engine = _module.CreateMCJITCompiler();

        //     // 2. Resolve the last expression and its type (The Peeker)
        //     ExpressionNodeExpr lastExpr = GetLastExpression(expr);

        //     // Check if the last expression is a variable we already know about, 
        //     // OR if it's being assigned right now in this sequence.
        //     if (lastExpr is IdNodeExpr id)
        //     {
        //         // Look in existing context OR peek at the assignment in the current block
        //         var type = _context.Get(id.Name)?.Type ?? PeekTypeInCurrentSequence(expr, id.Name);
        //         // yet here is says the type var is indeed Array
        //         id.SetType(type);
        //     }
        //     // it does say that the lastExprs type is array

        //     // 3. Build the LLVM Function
        //     LLVMTypeRef retType = lastExpr != null ? lastExpr.Type == MyType.Array ? LLVMTypeRef.CreatePointer(LLVMTypeRef.Int32, 0) 
        //     : GetLLVMType(lastExpr.Type) : LLVMTypeRef.Void;

        //     Console.WriteLine("LastExpr.Type = " + lastExpr.Type);


        //     var func = _module.AddFunction($"exec_{Guid.NewGuid():N}", LLVMTypeRef.CreateFunction(retType, Array.Empty<LLVMTypeRef>()));
        //     _builder.PositionAtEnd(func.AppendBasicBlock("entry"));

        //     // 4. Compile body and Return
        //     var resultValue = Visit(expr);
        //     if (retType.Handle != LLVMTypeRef.Void.Handle)
        //         _builder.BuildRet(resultValue);
        //     else
        //         _builder.BuildRetVoid();

        //     Console.WriteLine(_module.PrintToString());

        //     // 5. Execute and Extract

        //     Console.WriteLine("ABOUT TO RUN THE PROGRAM");
        //     var res = _engine.RunFunction(func, Array.Empty<LLVMGenericValueRef>());
        //     Console.WriteLine("WE RAN THE PROGRAM");
        //     return ExtractResult(res, lastExpr?.Type ?? MyType.None, lastExpr);
        // }

        // --- Helpers to keep Run() clean ---

        private ExpressionNodeExpr GetLastExpression(NodeExpr expr)
        {
            if (expr is SequenceNodeExpr seq)
                return seq.Statements.LastOrDefault() as ExpressionNodeExpr;
            return expr as ExpressionNodeExpr;
        }

        private MyType PeekTypeInCurrentSequence(NodeExpr root, string varName)
        {
            if (root is SequenceNodeExpr seq)
            {
                // Look for the last assignment to this variable name in the current block
                var lastAssign = seq.Statements.OfType<AssignNodeExpr>().LastOrDefault(a => a.Id == varName);
                // the line below sees that the lastassign.expression.type is int, which is wrong
                // wait the lastAssign is not null so why does it not return here
                if (lastAssign != null) return lastAssign.Expression.Type;
            }
            // it goes down to here and returns
            return MyType.None;
        }

        private object ExtractResult(LLVMGenericValueRef res, MyType type, NodeExpr lastExpr = null)
        {
            System.Console.WriteLine("extract reults");
            if (type == MyType.Int) 
                return (int)LLVM.GenericValueToInt(res, 0);
            
            if (type == MyType.String)
            {
                nint ptr = (nint)LLVM.GenericValueToPointer(res);
                return ptr == 0 ? null : Marshal.PtrToStringAnsi((IntPtr)ptr);
            }

            if (type == MyType.Bool)
                return LLVM.GenericValueToInt(res, 0) == 1;

            if (type == MyType.Array) // alright it knows that type is array
            {
            System.Console.WriteLine("extract reults of array");
                nint ptr = (nint)LLVM.GenericValueToPointer(res);

                // var arrayNode = lastExpr as ArrayNodeExpr;
                int length = 0;
                // Console.WriteLine("lenght: " + length);

                if (lastExpr is ArrayNodeExpr arr)
                {
                    length = arr.Elements.Count;
                    System.Console.WriteLine("we have array node");
                }
                else if (lastExpr is AssignNodeExpr assign && assign.Expression is ArrayNodeExpr arr2)
                {
                    length = arr2.Elements.Count;
                    System.Console.WriteLine("we have assign node");
                }
                // FIX: when assigning array, it absolutely should return array as the last node/type
                // NOT this
                // LastExpr.Type BEFORE codegen = Int

                // This
                // LastExpr.Type BEFORE codegen = Array

                // else if (lastExpr is IdNodeExpr id) // we need to check the length somehow, it could be recommended to save the length at index 0
                // {
                //     var entry = _context.Get(expr.Name);
                //     length = entry.
                // }

                // else
                //     throw new Exception("Cannot determine array length."); // we get down to here


                // You need the length; if your ArrayNodeExpr has a length field, use that
                //int length = res; // temporary, or store in ArrayNodeExpr
                int[] result = new int[length];
                for (int i = 0; i < length; i++)
                    result[i] = Marshal.ReadInt32((IntPtr)(ptr + i * 4)); // assume 4-byte ints
                return result;
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

        public LLVMValueRef VisitBinaryExpr(BinaryOpNodeExpr expr)
        {
            var lhs = Visit(expr.Left);
            var rhs = Visit(expr.Right);

            switch (expr.Operator)
            {
                case "+": return _builder.BuildAdd(lhs, rhs, "addtmp");
                case "-": return _builder.BuildSub(lhs, rhs, "subtmp");
                case "*": return _builder.BuildMul(lhs, rhs, "multmp");
                case "/": return _builder.BuildSDiv(lhs, rhs, "divtmp");
                default: throw new InvalidOperationException("Unknown operator");
            }
            // ehm, it goes to the default case? or no, bro it must do the default case, the rest of them would return
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



    // public LLVMValueRef VisitArrayExpr(ArrayNodeExpr expr)
    // {
    //     int length = expr.Elements.Count;
    //     var elementType = LLVMTypeRef.Int32; // this should be dynamic based on the type of elements in the array, but we'll assume int for simplicity

    //     // Allocate array on the stack (int example)
    //     var arrayAlloc = _builder.BuildArrayAlloca(
    //         elementType,
    //         LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)length, false), 
    //         "arrayalloc"
    //     );

    //     Console.WriteLine("ARRAY1");
    //     for (int i = 0; i < length; i++)
    //     {
    //         var elemValue = Visit(expr.Elements[i]);

    //         // High-level wrapper: just use array of indices
    //         var elemPtr = _builder.BuildGEP2(
    //             elementType,
    //             arrayAlloc,
    //             new LLVMValueRef[] {
    //                 LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0, false),
    //                 LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)i, false)
    //             },
    //             "elemptr"
    //         );

    //         _builder.BuildStore(elemValue, elemPtr);
    //     }
    //     Console.WriteLine("ARRAY2" + arrayAlloc.ToString());
    //     return arrayAlloc;
    // }

    // use these functions for help
    // public readonly LLVMValueRef BuildGEP2(LLVMTypeRef Ty, LLVMValueRef Pointer, LLVMValueRef[] Indices, string Name = "") => BuildGEP2(Ty, Pointer, Indices.AsSpan(), Name.AsSpan());

    // public readonly LLVMValueRef BuildGEP2(LLVMTypeRef Ty, LLVMValueRef Pointer, ReadOnlySpan<LLVMValueRef> Indices, ReadOnlySpan<char> Name)
    // {
    //     fixed (LLVMValueRef* pIndices = Indices)
    //     {
    //         using var marshaledName = new MarshaledString(Name);
    //         return LLVM.BuildGEP2(this, Ty, Pointer, (LLVMOpaqueValue**)pIndices, (uint)Indices.Length, marshaledName);
    //     }
    // }

        public LLVMValueRef VisitAssignExpr(AssignNodeExpr expr)
        {
            var value = Visit(expr.Expression);
            //expr.SetType(expr.Expression.Type); 
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
                else
                // bro it calls this else as well, it makes no sense
                    global.Initializer = LLVMValueRef.CreateConstNull(llvmType);

                // Update context with the real Global Value
            }
            _context = _context.Add(expr.Id, global, expr.Expression.Type);

            _builder.BuildStore(value, global);
            return value;
        }

        public LLVMValueRef VisitIdExpr(IdNodeExpr expr)
        {
            System.Console.WriteLine("VISITING ID EXPR: " + expr.Name);
            var entry = _context.Get(expr.Name);
            if (entry is null) throw new Exception($"Variable {expr.Name} not defined");

            // CRITICAL: Pull the type from the symbol table and put it on the node
            var actualType = entry.Value.Type;
            expr.SetType(actualType);

            LLVMTypeRef llvmType = GetLLVMType(actualType);
            // I think it fails here
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
            // Fix: Pass the nodes from the expression into the Visit method
            var leftVal = Visit(expr.Left);
            var rightVal = Visit(expr.Right);

            return expr.Operator switch
            {
                "<" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, leftVal, rightVal, "cmptmp"),
                ">" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, leftVal, rightVal, "cmptmp"),
                "==" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, leftVal, rightVal, "cmptmp"),
                "!=" => _builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, leftVal, rightVal, "cmptmp"),
                _ => throw new Exception("Unknown comparison operator")
            };
        }

        public LLVMValueRef VisitIfExpr(IfNodeExpr expr)
        {
            // 1. Compile the condition
            var condValue = Visit(expr.Condition);

            // 2. Get the current function to attach blocks to
            var func = _builder.InsertBlock.Parent;

            // 3. Create the blocks
            var thenBlock = func.AppendBasicBlock("then");
            var elseBlock = func.AppendBasicBlock("else");
            var mergeBlock = func.AppendBasicBlock("ifcont");

            // 4. Create the branch logic
            _builder.BuildCondBr(condValue, thenBlock, elseBlock);

            // --- Build THEN part ---
            _builder.PositionAtEnd(thenBlock);
            var thenVal = Visit(expr.ThenPart);
            _builder.BuildBr(mergeBlock); // Jump to end
                                          // Re-get thenBlock in case Visit modified it (for nested ifs)
            thenBlock = _builder.InsertBlock;

            // --- Build ELSE part ---
            _builder.PositionAtEnd(elseBlock);
            LLVMValueRef elseVal;
            if (expr.ElsePart != null)
            {
                elseVal = Visit(expr.ElsePart);
            }
            else
            {
                // If no else, just use a dummy value or null
                elseVal = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0);
            }
            _builder.BuildBr(mergeBlock);
            elseBlock = _builder.InsertBlock;

            // --- Build MERGE part ---
            _builder.PositionAtEnd(mergeBlock);

            return null;
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

                if(_builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, maxValue, minValue, "checkMax").ToString().Contains("false"))
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

            LLVMValueRef formatStr;

            // Determine format string based on type
            if (expr.Expression.Type == MyType.Int)
                formatStr = _builder.BuildGlobalStringPtr("%d\n", "print_int_fmt");
            else if (expr.Expression.Type == MyType.String)
                formatStr = _builder.BuildGlobalStringPtr("%s\n", "print_str_fmt");
            else
                throw new Exception("Cannot print this type");

            // Call printf(formatStr, valueToPrint)
            var llvmCtx = _module.Context;

            return _builder.BuildCall2(
                LLVMTypeRef.CreateFunction(llvmCtx.Int32Type, new[] { LLVMTypeRef.CreatePointer(llvmCtx.Int8Type, 0) }, true),
                printf,
                new[] { formatStr, valueToPrint },
                "printcall"
            );
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

            // bro are you out of your mind, why the hell are you in the forloop?
            // 6. End Block: Move the builder here so subsequent code compiles AFTER the loop
            _builder.PositionAtEnd(endBlock);

            // Return a dummy value (Loops are statements and don't usually return data)
            return LLVMValueRef.CreateConstInt(llvmCtx.Int32Type, 0);
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

        private LLVMValueRef Visit(NodeExpr expr)
        {
            return expr.Accept(this);
        }
    }
}