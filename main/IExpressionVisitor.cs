using LLVMSharp.Interop;
using LLVMSharp;

namespace MyCompiler
{
    public interface IExpressionVisitor
    {
        LLVMValueRef VisitBinaryExpr(BinaryOpNodeExpr expr);
        LLVMValueRef VisitNumberExpr(NumberNodeExpr expr);
        LLVMValueRef VisitSequenceExpr(SequenceNodeExpr expr);
        LLVMValueRef VisitBooleanExpr(BooleanNodeExpr expr);
        LLVMValueRef VisitForLoopExpr(ForLoopNodeExpr expr);
        LLVMValueRef VisitPrintExpr(PrintNodeExpr expr);
        LLVMValueRef VisitRandomExpr(RandomNodeExpr expr);
        LLVMValueRef VisitIfExpr(IfNodeExpr expr);
        LLVMValueRef VisitComparisonExpr(ComparisonNodeExpr expr);
        LLVMValueRef VisitStringExpr(StringNodeExpr expr);
        LLVMValueRef VisitIncrementExpr(IncrementNodeExpr expr);
        LLVMValueRef VisitDecrementExpr(DecrementNodeExpr expr);
        LLVMValueRef VisitIdExpr(IdNodeExpr expr);
        LLVMValueRef VisitAssignExpr(AssignNodeExpr expr);
        LLVMValueRef VisitArrayExpr(ArrayNodeExpr expr);
    }

    public interface ITypeVisitor
    {
        MyType VisitNumber(NumberNodeExpr expr);
        MyType VisitString(StringNodeExpr expr);
        MyType VisitBoolean(BooleanNodeExpr expr);
        MyType VisitComparison(ComparisonNodeExpr expr);
        MyType VisitIncrement(IncrementNodeExpr expr);
        MyType VisitDecrement(DecrementNodeExpr expr);
        MyType VisitId(IdNodeExpr expr);
        MyType VisitBinary(BinaryOpNodeExpr expr);
        MyType VisitAssign(AssignNodeExpr expr);
        MyType VisitSequence(SequenceNodeExpr expr);
        MyType VisitRandom(RandomNodeExpr expr);
        MyType VisitIf(IfNodeExpr expr);
        MyType VisitPrint(PrintNodeExpr expr);
        MyType VisitForLoop(ForLoopNodeExpr expr);
        MyType VisitArray(ArrayNodeExpr expr);
    }

    public class TypeChecker : ITypeVisitor
    {
        private Context _context;

        public TypeChecker(Context context)
        {
            _context = context;
        }

        public MyType Check(NodeExpr node)
        {
            System.Console.WriteLine("we checking");
            return Visit(node);
        }

        private MyType Visit(NodeExpr node)
        {
            Console.WriteLine($"Visiting {node.GetType().Name}");
            return node switch
            {
                NumberNodeExpr n => VisitNumber(n),
                StringNodeExpr str => VisitString(str),
                BooleanNodeExpr b => VisitBoolean(b),
                IdNodeExpr id => VisitId(id),
                BinaryOpNodeExpr bin => VisitBinary(bin),
                ComparisonNodeExpr cmp => VisitComparison(cmp),
                IncrementNodeExpr inc => VisitIncrement(inc),
                DecrementNodeExpr dec => VisitDecrement(dec),
                AssignNodeExpr assign => VisitAssign(assign),
                SequenceNodeExpr seq => VisitSequence(seq),
                RandomNodeExpr ran => VisitRandom(ran),
                IfNodeExpr _if => VisitIf(_if),
                PrintNodeExpr pr => VisitPrint(pr),
                ForLoopNodeExpr _for => VisitForLoop(_for),
                ArrayNodeExpr ar => VisitArray(ar),
                _ => throw new NotSupportedException($"Type check not implemented for {node.GetType().Name}")
            };
        }

        public MyType VisitNumber(NumberNodeExpr expr)
        {
            expr.SetType(MyType.Int);
            return MyType.Int;
        }
        public MyType VisitString(StringNodeExpr expr)
        {
            expr.SetType(MyType.String);
            return MyType.String;
        }

        public MyType VisitBoolean(BooleanNodeExpr expr)
        {
            expr.SetType(MyType.Bool);
            return MyType.Bool;
        }

        public MyType VisitId(IdNodeExpr expr)
        {
            var entry = _context.Get(expr.Name);
            if (entry == null)
                throw new Exception($"Undefined variable '{expr.Name}'");

            expr.SetType(entry.Value.Type);
            return entry.Value.Type;
        }

        public MyType VisitBinary(BinaryOpNodeExpr expr)
        {
            var leftType = Visit(expr.Left);
            var rightType = Visit(expr.Right);

            if (expr.Operator == "+")
            {
                if (leftType == MyType.Int && rightType == MyType.Int)
                {
                    expr.SetType(MyType.Int);
                    return MyType.Int;
                }

                if (leftType == MyType.String && rightType == MyType.String)
                {
                    expr.SetType(MyType.String);
                    return MyType.String;
                }

                throw new Exception("Invalid operands for +");
            }

            if (leftType != rightType)
                throw new Exception($"Type mismatch: {leftType} {expr.Operator} {rightType}");

            if (expr.Operator is "-" or "*" or "/")
            {
                if (leftType != MyType.Int)
                    throw new Exception("Arithmetic operators require Int");

                expr.SetType(MyType.Int);
                return MyType.Int;
            }

            throw new Exception($"Unknown operator {expr.Operator}");
        }

        public MyType VisitComparison(ComparisonNodeExpr expr)
        {
            var leftType = Visit(expr.Left);
            var rightType = Visit(expr.Right);

            if (leftType != rightType)
                throw new Exception($"Type mismatch in comparison: {leftType} {expr.Operator} {rightType}");

            if (expr.Operator is "==" or "!=")
            {
                expr.SetType(MyType.Bool);
                return MyType.Bool;
            }

            // only ints support < > <= >=
            if (expr.Operator is ">" or "<" or ">=" or "<=")
            {
                if (leftType != MyType.Int)
                    throw new Exception("Ordering operators require Int");

                expr.SetType(MyType.Bool);
                return MyType.Bool;
            }

            throw new Exception($"Unknown operator {expr.Operator}");
        }

        public MyType VisitIncrement(IncrementNodeExpr expr)
        {
            var entry = _context.Get(expr.Id);

            if (entry == null)
                throw new Exception($"Undefined variable '{expr.Id}'");

            if (entry.Value.Type != MyType.Int)
                throw new Exception("Increment operator requires Int");

            //expr.SetType(MyType.Int); // or MyType.None depending on design
            return MyType.Int;        // or MyType.None
        }

        public MyType VisitDecrement(DecrementNodeExpr expr)
        {
            var entry = _context.Get(expr.Id);

            if (entry == null)
                throw new Exception($"Undefined variable '{expr.Id}'");

            if (entry.Value.Type != MyType.Int)
                throw new Exception("Decrement operator requires Int");

            //expr.SetType(MyType.Int);
            return MyType.Int;
        }

        public MyType VisitAssign(AssignNodeExpr expr)
        {
            var valueType = Visit(expr.Expression);

            _context = _context.Add(expr.Id, null, valueType, 0);

            //expr.SetType(MyType.None);
            return MyType.None;
        }

        public MyType VisitRandom(RandomNodeExpr expr)
        {
            var valueTypeMin = Visit(expr.MinValue);
            var valueTypeMax = Visit(expr.MaxValue);

            expr.SetType(MyType.Int);
            return MyType.Int;
        }

        public MyType VisitIf(IfNodeExpr expr)
        {
            var condType = Visit(expr.Condition);

            if (condType != MyType.Bool)
                throw new Exception("If condition must be Bool");

            var thenType = Visit(expr.ThenPart);
            var elseType = expr.ElsePart != null
                ? Visit(expr.ElsePart)
                : MyType.None;

            if (thenType == MyType.None && elseType == MyType.None)
            {
                expr.SetType(MyType.None);
                return MyType.None;
            }

            if (thenType == MyType.None)
            {
                expr.SetType(elseType);
                return elseType;
            }

            if (elseType == MyType.None)
            {
                expr.SetType(thenType);
                return thenType;
            }

            if (thenType != elseType)
                throw new Exception("Then and Else branches must have same type");

            expr.SetType(thenType);
            return thenType;
        }

        public MyType VisitPrint(PrintNodeExpr expr)
        {
            var innerType = Visit(expr.Expression);

            expr.SetType(innerType);   // print returns same type
            return innerType;
        }
        public MyType VisitForLoop(ForLoopNodeExpr expr)
        {
            // 1. Initialization
            if (expr.Initialization != null)
                Visit(expr.Initialization);

            // 2. Condition must be Bool
            var condType = Visit(expr.Condition);

            if (condType != MyType.Bool)
                throw new Exception("For loop condition must be Bool");

            // 3. Step
            if (expr.Step != null)
                Visit(expr.Step);

            // 4. Body
            Visit(expr.Body);

            // 5. For loops are statements
            return MyType.None;
        }

        public MyType VisitArray(ArrayNodeExpr expr)
        {
            expr.SetType(MyType.Array);
            return MyType.Array;
        }

        public MyType VisitSequence(SequenceNodeExpr expr)
        {
            MyType lastType = MyType.None;

            foreach (var stmt in expr.Statements)
                lastType = Visit(stmt);

            return lastType;
        }


        public Context UpdatedContext => _context;
    }
}