namespace MyCompiler
{
    public class TypeChecker : ITypeVisitor
    {
        private Context _context;
        public Context UpdatedContext => _context;
        private bool _debug;

        public TypeChecker(Context context, bool debug = true)
        {
            _debug = debug;
            _context = context;
        }

        public MyType Check(NodeExpr node)
        {
            if (_debug) Console.WriteLine("we checking");
            return Visit(node);
        }

        private MyType Visit(NodeExpr node)
        {
            // var name = node.GetType().Name; // it fails here for if visits, but not the others. Why would it not be able to get the if nodes type and name?
            // Console.WriteLine("visiting " + name.Substring(0, name.Length - 8));
            return node switch // it says the last numbers node is null
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
                ArrayNodeExpr arr => VisitArray(arr),
                IndexNodeExpr idx => VisitIndex(idx),
                WhereNodeExpr whe => VisitWhere(whe),
                AddNodeExpr add => VisitAdd(add),
                RemoveNodeExpr remo => VisitRemove(remo),

                FunctionDefNode fdef => VisitFunctionDef(fdef),
                FunctionCallNode fcall => VisitFunctionCall(fcall),
                RoundNodeExpr rnd => VisitRound(rnd),
                FloatNodeExpr flt => VisitFloat(flt),
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
            {
                // Remove .Line if it's causing an error
                throw new Exception($"Undefined variable '{expr.Name}'");
            }

            expr.SetType(entry.Type);
            return entry.Type;
        }

        public MyType VisitBinary(BinaryOpNodeExpr expr)
        {
            var leftType = Visit(expr.Left);
            var rightType = Visit(expr.Right);

            // Helper to check if a type is numeric (Int or Float)
            bool isLeftNum = leftType == MyType.Int || leftType == MyType.Float;
            bool isRightNum = rightType == MyType.Int || rightType == MyType.Float;

            if (expr.Operator == "+")
            {
                // 1. Handle Numeric Addition (allow mixing Int and Float)
                if (isLeftNum && isRightNum)
                {
                    // If both are Int, result is Int. If any is Float, promote to Float.
                    MyType resultType = (leftType == MyType.Float || rightType == MyType.Float)
                                        ? MyType.Float : MyType.Int;
                    expr.SetType(resultType);
                    return resultType;
                }
                // 2. Handle String Concatenation (Updated to allow Mixed Types)
                if (expr.Operator == "+")
                {
                    // If either side is a string, the result is a string
                    if (leftType == MyType.String || rightType == MyType.String)
                    {
                        expr.SetType(MyType.String);
                        return MyType.String;
                    }
                }

                throw new Exception($"Invalid operands {leftType} and {rightType} for +");
            }

            if (expr.Operator is "-" or "*" or "/")
            {
                // Allow math on any numeric types
                if (isLeftNum && isRightNum)
                {
                    MyType resultType = (leftType == MyType.Float || rightType == MyType.Float)
                                        ? MyType.Float : MyType.Int;
                    expr.SetType(resultType);
                    return resultType;
                }

                throw new Exception($"Arithmetic operator {expr.Operator} requires numeric types, got {leftType} and {rightType}");
            }


            // it fails on comparison with '&' and '|'

            // > (5<7 and true)  // fails
            // AST Structure:
            // we checking
            // visiting Sequence
            // visiting Comparison
            // visiting Number
            // visiting BinaryOp
            // visiting Number
            // visiting Boolean
            // Compiler Error: Unknown operator && or type mismatch: Int and Bool

            // > 5<7             // succeeds
            // AST Structure:
            // we checking
            // visiting Sequence
            // visiting Comparison
            // visiting Number
            // visiting Number
            // visiting: MyCompiler.SequenceNodeExpr
            // visiting: MyCompiler.ComparisonNodeExpr
            // visiting: MyCompiler.NumberNodeExpr
            // visiting: MyCompiler.NumberNodeExpr
            // prints type: Bool
            // value to print type: i1
            // LLVM TYPE: i1
            // LANG TYPE: Bool
            // Generated LLVM IR:


            // Handle Comparisons (==, !=, <, >)
            if (expr.Operator is "==" or "!=" or "<" or ">" or "&&" or "||")
            {
                // For comparisons, we just need the types to be compatible 
                // (e.g., comparing a Float and an Int is fine)
                if (isLeftNum && isRightNum)
                {
                    expr.SetType(MyType.Bool);
                    return MyType.Bool;
                }

                if (leftType == rightType)
                {
                    expr.SetType(MyType.Bool);
                    return MyType.Bool;
                }
            }

            throw new Exception($"Unknown operator {expr.Operator} or type mismatch: {leftType} and {rightType}");
        }

        public MyType VisitComparison(ComparisonNodeExpr expr)
        {
            var leftType = Visit(expr.Left);
            var rightType = Visit(expr.Right);

            // expr.SetType(MyType.Bool);
            // return MyType.Bool;

            // Numeric comparisons
            if (expr.Operator is ">" or "<" or ">=" or "<=" or "&&" or "||")
            {
                if (leftType != MyType.Int && leftType != MyType.Float)
                    throw new Exception("Ordering operators require number");

                if (rightType != MyType.Int && rightType != MyType.Float)
                    throw new Exception("Ordering operators require number");

                expr.SetType(MyType.Bool);
                return MyType.Bool;
            }

            // Equality comparisons
            if (expr.Operator is "==" or "!=")
            {
                if (leftType != rightType)
                    throw new Exception($"Type mismatch in equality comparison: {leftType} {expr.Operator} {rightType}");

                expr.SetType(MyType.Bool);
                return MyType.Bool;
            }

            throw new Exception($"Unknown operator {expr.Operator}");
        }

        public MyType VisitIncrement(IncrementNodeExpr expr)
        {
            var entry = _context.Get(expr.Id);
            if (entry == null) throw new Exception($"Variable {expr.Id} not defined");

            // FIX: Change entry.Value.Type to entry.Type
            var type = entry.Type;

            expr.SetType(type);
            return type;
        }

        public MyType VisitDecrement(DecrementNodeExpr expr)
        {
            var entry = _context.Get(expr.Id);
            if (entry == null) throw new Exception($"Variable {expr.Id} not defined");

            // FIX: Change entry.Value.Type to entry.Type
            var type = entry.Type;

            expr.SetType(type);
            return type;
        }

        public MyType VisitAssign(AssignNodeExpr expr)
        {
            MyType valType = Check(expr.Expression);
            MyType? elemType = (expr.Expression is ArrayNodeExpr arr) ? arr.ElementType : null;

            // Use 'default' for LLVMValueRef to avoid CS0246
            _context = _context.Add(expr.Id, default, null, valType, elemType);
            return valType;
        }

        public MyType VisitRandom(RandomNodeExpr expr)
        {
            expr.SetType(MyType.Int);
            return MyType.Int;
        }

        public MyType VisitIf(IfNodeExpr expr)
        {
            var condType = Visit(expr.Condition);

            // 1. Condition Check
            if (condType != MyType.Bool)
                throw new Exception("If condition must be Bool");

            // Use .Then and .Else to match your Node definition
            MyType thenType = Visit(expr.ThenPart);

            MyType elseType = MyType.None;
            if (expr.ElsePart != null)
                elseType = Visit(expr.ElsePart);

            MyType finalType;

            // 2. Promotion Logic (Bool <-> Number)
            if (thenType != elseType)
            {
                // Allow mixing any Numeric type (Int/Float) with Booleans
                bool isThenNumeric = (thenType == MyType.Float || thenType == MyType.Int || thenType == MyType.Bool);
                bool isElseNumeric = (elseType == MyType.Float || elseType == MyType.Int || elseType == MyType.Bool);

                // If we have mixed Numbers and Bools
                if (isThenNumeric && isElseNumeric)
                {
                    // If either side is an Int, let's treat the result as an Int/Float 
                    // so we see numbers instead of "True/False"
                    finalType = MyType.Float;
                }
                // Handle cases where one side is None (Void)
                else if (thenType == MyType.None)
                    finalType = elseType;
                else if (elseType == MyType.None)
                    finalType = thenType;
                else
                    throw new Exception($"Type Mismatch: Then branch is {thenType}, Else is {elseType}");
            }
            else
                finalType = thenType;

            // 3. Set the type and return
            expr.SetType(finalType);
            return finalType;
        }

        public MyType VisitPrint(PrintNodeExpr expr)
        {
            var innerType = Visit(expr.Expression);

            expr.SetType(innerType);   // print returns same type
            return innerType;
        }

        public MyType VisitForLoop(ForLoopNodeExpr expr)
        {
            if (expr.Initialization != null) Visit(expr.Initialization);

            var condType = Visit(expr.Condition);
            // Optimization: Allow Float as condition (0.0 is false)
            if (condType != MyType.Bool && condType != MyType.Float)
                throw new Exception("For loop condition must be Bool or Number");

            if (expr.Step != null) Visit(expr.Step);

            Visit(expr.Body); // Now visits the sequence/block
            return MyType.None;
        }

        public MyType VisitSequence(SequenceNodeExpr expr)
        {
            MyType lastType = MyType.None;

            foreach (var stmt in expr.Statements)
                lastType = Visit(stmt);

            return lastType;
        }

        // Inside public class TypeChecker : ITypeVisitor
        // Handle [1, 2, 3]
        public MyType VisitArray(ArrayNodeExpr expr)
        {
            if (expr.Elements.Count > 0)
                expr.ElementType = Check(expr.Elements[0]);

            expr.SetType(MyType.Array);
            return MyType.Array;
        }

        // Handle arr[0]
        // Inside TypeChecker.cs
        public MyType VisitIndex(IndexNodeExpr expr)
        {
            Check(expr.ArrayExpression); // Ensure target is valid

            if (expr.ArrayExpression is IdNodeExpr idNode)
            {
                var entry = _context.Get(idNode.Name);
                if (entry != null && entry.Type == MyType.Array)
                {
                    var inferred = entry.ElementType ?? MyType.Float;
                    expr.SetType(inferred);
                    return inferred;
                }
            }
            expr.SetType(MyType.Float);
            return MyType.Float;
        }

        public MyType VisitFunctionDef(FunctionDefNode node)
        {
            // 1. Convert the string return type (e.g., "int") to your MyType enum
            MyType returnType = node.ReturnTypeName;

            // 2. Save the global context
            var globalContext = _context;

            // 3. Create Local Scope: Add parameters so the body can see them
            // We assume parameters are Floats/Numbers in your current setup


            foreach (var paramName in node.Parameters)
            {
                // If the function returns a string, assume parameters are strings.
                // Otherwise, assume they are numbers.
                MyType pType = (returnType == MyType.String) ? MyType.String : MyType.Float;

                _context = _context.Add(paramName, default!, null, pType, null);
            }

            // 4. Visit the body to ensure variables like 'x' and 'y' are defined
            Visit(node.Body);

            // 5. Restore Global Context (parameters shouldn't exist outside)
            _context = globalContext;

            // 6. Register the FUNCTION itself so we can call it
            // Using 'rType' (the enum) instead of 'node.ReturnTypeName' (the string)
            _context = _context.Add(node.Name, default!, null, returnType, null);

            return MyType.None;
        }

        public MyType VisitFunctionCall(FunctionCallNode expr)
        {
            var entry = _context.Get(expr.Name);
            if (entry == null) throw new Exception($"Function {expr.Name} is not defined");

            // Check if the number of arguments matches (Very important for LLVM!)
            // Note: You'll need to store the parameter count in your ContextEntry to do this properly.

            foreach (var arg in expr.Arguments) Visit(arg);

            // Use the actual return type of the function!
            expr.SetType(entry.Type);
            return entry.Type;
        }

        public MyType VisitRound(RoundNodeExpr expr)
        {
            Visit(expr.Value);
            expr.SetType(MyType.Float);
            return MyType.Float;
        }

        public MyType VisitFloat(FloatNodeExpr expr)
        {
            expr.SetType(MyType.Float);
            return MyType.Float;
        }

        public MyType VisitAdd(AddNodeExpr expr)
        {
            Visit(expr.ArrayExpression);
            Visit(expr.AddExpression);
            expr.SetType(expr.AddExpression.Type);
            return expr.AddExpression.Type;
        }

        public MyType VisitWhere(WhereNodeExpr expr)
        {
            System.Console.WriteLine("yo the array node in where: " + expr.ArrayNodeExpr);
            VisitAssign(new AssignNodeExpr(expr.IteratorId.Name, new NumberNodeExpr(0)));



            Visit(expr.IteratorId);
            Visit(expr.ArrayNodeExpr);
            Visit(expr.Condition);
            var arrayType = Visit(expr.ArrayNodeExpr);

            if (arrayType != MyType.Array)
                throw new Exception("where can only be used on arrays");

            // 2. Determine element type (adjust depending on your language)

            // 5. Check condition
            var condType = Visit(expr.Condition);

            if (condType != MyType.Bool)
                throw new Exception("where condition must return bool");



            expr.SetType(MyType.Array);
            return MyType.Array;
        }

        public MyType VisitRemove(RemoveNodeExpr expr)
        {
            Visit(expr.ArrayExpression);
            Visit(expr.RemoveExpression);
            expr.SetType(expr.RemoveExpression.Type);
            return expr.RemoveExpression.Type;
        }
    }
}