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

        public Type Check(NodeExpr node)
        {
            if (_debug) Console.WriteLine("we checking");
            return Visit(node);
        }

        private Type Visit(NodeExpr node)
        {
            var name = node.GetType().Name; // it fails here for if visits, but not the others. Why would it not be able to get the if nodes type and name?
            if (_debug) Console.WriteLine("visiting: " + name.Substring(0, name.Length - 8));
            return node switch // it says the last numbers node is null
            {
                NumberNodeExpr n => VisitNumber(n),
                StringNodeExpr str => VisitString(str),
                BooleanNodeExpr b => VisitBoolean(b),
                IdNodeExpr id => VisitId(id),
                UnaryOpNodeExpr un => VisitUnaryOp(un),
                BinaryOpNodeExpr bin => VisitBinary(bin),
                LogicalOpNodeExpr log => VisitLogical(log),
                ComparisonNodeExpr cmp => VisitComparison(cmp),
                IncrementNodeExpr inc => VisitIncrement(inc),
                DecrementNodeExpr dec => VisitDecrement(dec),
                AssignNodeExpr assign => VisitAssign(assign),
                SequenceNodeExpr seq => VisitSequence(seq),
                RandomNodeExpr ran => VisitRandom(ran),
                IfNodeExpr _if => VisitIf(_if),
                PrintNodeExpr pr => VisitPrint(pr),
                ForLoopNodeExpr _for => VisitForLoop(_for),
                ForEachLoopNodeExpr _foreach => VisitForEachLoop(_foreach),
                ArrayNodeExpr arr => VisitArray(arr),
                CopyArrayNodeExpr clo => VisitCopyArray(clo),
                IndexNodeExpr idx => VisitIndex(idx),
                IndexAssignNodeExpr idxa => VisitIndexAssign(idxa),
                WhereNodeExpr whe => VisitWhere(whe),
                MapNodeExpr map => VisitMap(map),
                ReadCsvNodeExpr read => VisitReadCsv(read),
                AddNodeExpr add => VisitAdd(add),
                AddRangeNodeExpr addr => VisitAddRange(addr),
                RemoveNodeExpr remo => VisitRemove(remo),
                RemoveRangeNodeExpr remor => VisitRemoveRange(remor),
                LengthNodeExpr len => VisitLength(len),
                MinNodeExpr min => VisitMin(min),
                MaxNodeExpr max => VisitMax(max),
                MeanNodeExpr mean => VisitMean(mean),
                SumNodeExpr sum => VisitSum(sum),

                FunctionDefNode fdef => VisitFunctionDef(fdef),
                FunctionCallNode fcall => VisitFunctionCall(fcall),
                RoundNodeExpr rnd => VisitRound(rnd),
                FloatNodeExpr flt => VisitFloat(flt),
                _ => throw new NotSupportedException($"Type check not implemented for {node.GetType().Name}")
            };
        }

        public Type VisitForEachLoop(ForEachLoopNodeExpr expr)
        {
            // 1. Check the array expression to ensure it's actually an array
            Type arrayType = Visit(expr.Array);
            if (arrayType is not ArrayType)
            {
                throw new Exception("Foreach target must be an array.");
            }

            // 2. Determine the element type (e.g., MyType.Float for [12, 200])
            Type elementType = null; // Default
            if (expr.Array is ArrayNodeExpr arrayNode)
            {
                elementType = arrayNode.ElementType ?? new FloatType();
            }
            else if (expr.Array is IdNodeExpr idNode)
            {
                var entry = _context.Get(idNode.Name);
                elementType = entry?.ElementType ?? new FloatType();
            }

            // arr = ["a", "b", "c"];
            // arr = [1, 2, 3];
            // arr = [true, false, false];


            // 3. Register the iterator variable (e.g., 'item') in the context
            // This allows the Body to know 'item' is a Float/String
            _context = _context.Add(expr.Iterator.Name, default, elementType);

            // 4. Check the body
            Visit(expr.Body);

            return new VoidType(); // Loops don't return a value
        }

        public Type VisitNumber(NumberNodeExpr expr)
        {
            expr.SetType(new IntType());
            return new IntType();
        }
        public Type VisitString(StringNodeExpr expr)
        {
            expr.SetType(new StringType());
            return new StringType();
        }

        public Type VisitBoolean(BooleanNodeExpr expr)
        {
            expr.SetType(new BoolType());
            return new BoolType();
        }

        public Type VisitId(IdNodeExpr expr)
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

        public Type VisitBinary(BinaryOpNodeExpr expr)
        {
            var leftType = Visit(expr.Left);
            var rightType = Visit(expr.Right);

            // Helper to check if a type is numeric (Int or Float)
            bool isLeftNum = leftType is IntType || leftType is FloatType;
            bool isRightNum = rightType is IntType || rightType is FloatType;

            if (expr.Operator == "+")
            {
                // 1. Handle Numeric Addition (allow mixing Int and Float)
                if (isLeftNum && isRightNum)
                {
                    // If both are Int, result is Int. If any is Float, promote to Float.
                    Type resultType = (leftType is FloatType || rightType is FloatType)
                                        ? new FloatType() : new IntType();
                    expr.SetType(resultType);
                    return resultType;
                }
                // 2. Handle String Concatenation (Updated to allow Mixed Types)
                if (expr.Operator == "+")
                {
                    // If either side is a string, the result is a string
                    if (leftType is StringType || rightType is StringType)
                    {
                        expr.SetType(new StringType());
                        return new StringType();
                    }
                }

                throw new Exception($"Invalid operands {leftType} and {rightType} for +");
            }

            if (expr.Operator is "-" or "*" or "/")
            {
                // Allow math on any numeric types
                if (isLeftNum && isRightNum)
                {
                    Type resultType = (leftType is FloatType || rightType is FloatType)
                                        ? new FloatType() : new IntType();
                    expr.SetType(resultType);
                    return resultType;
                }

                throw new Exception($"Arithmetic operator {expr.Operator} requires numeric types, got {leftType} and {rightType}");
            }

            // Handle Comparisons (==, !=, <, >)
            if (expr.Operator is "==" or "!=" or "<" or ">")
            {
                // For comparisons, we just need the types to be compatible 
                // (e.g., comparing a Float and an Int is fine)
                if (isLeftNum && isRightNum)
                {
                    expr.SetType(new BoolType());
                    return new BoolType();
                }

                if (leftType == rightType)
                {
                    expr.SetType(new BoolType());
                    return new BoolType();
                }
            }

            throw new Exception($"Unknown operator {expr.Operator} or type mismatch: {leftType} and {rightType}");
        }

        public Type VisitLogical(LogicalOpNodeExpr expr)
        {
            var leftType = Visit(expr.Left);
            var rightType = Visit(expr.Right);

            // Helper to check if a type is numeric (Int or Float)
            if (leftType is not BoolType || rightType is not BoolType)
                throw new Exception($"Invalid operands type {leftType} and {rightType}");


            if (expr.Operator is "&&" or "||")
            {
                expr.SetType(new BoolType());
                return new BoolType();
            }

            throw new Exception($"Unknown operator {expr.Operator} or type mismatch: {leftType} and {rightType}");
        }

        public Type VisitComparison(ComparisonNodeExpr expr)
        {
            var leftType = Visit(expr.Left);
            var rightType = Visit(expr.Right);

            bool isLeftNum = leftType is IntType || leftType is FloatType;
            bool isRightNum = rightType is IntType || rightType is FloatType;

            // === LOGICAL OPERATORS ===
            if (expr.Operator is "&&" or "||")
            {
                if (leftType is not BoolType || rightType is not BoolType)
                    throw new Exception(
                        $"Logical operator {expr.Operator} requires bool operands, got {leftType} and {rightType}");

                expr.SetType(new BoolType());
                return new BoolType();
            }

            // === ORDERING (< > <= >=) ===
            if (expr.Operator is "<" or ">" or "<=" or ">=")
            {
                if (!(isLeftNum && isRightNum))
                    throw new Exception(
                        $"Operator {expr.Operator} requires numeric operands, got {leftType} and {rightType}");

                expr.SetType(new BoolType());
                return new BoolType();
            }

            // === EQUALITY (== !=) ===
            if (expr.Operator is "==" or "!=")
            {
                // allow numeric mixing (int == float)
                if (isLeftNum && isRightNum)
                {
                    expr.SetType(new BoolType());
                    return new BoolType();
                }

                if (leftType.GetType() != rightType.GetType())
                    throw new Exception(
                        $"Type mismatch in equality comparison: {leftType} {expr.Operator} {rightType}");

                expr.SetType(new BoolType());
                return new BoolType();
            }

            throw new Exception($"Unknown operator {expr.Operator}");
        }

        public Type VisitComparison2(ComparisonNodeExpr expr)
        {
            var leftType = Visit(expr.Left);
            var rightType = Visit(expr.Right);

            // expr.SetType(MyType.Bool);
            // return MyType.Bool;

            // Numeric comparisons
            if (expr.Operator is ">" or "<" or ">=" or "<=")
            {
                if (leftType is not IntType && leftType is not IntType)
                    throw new Exception("Ordering operators require number");

                if (rightType is not FloatType && rightType is not FloatType)
                    throw new Exception("Ordering operators require number");

                expr.SetType(new BoolType());
                return new BoolType();
            }

            if (expr.Operator is "&&" or "||")
            {
                if (leftType is not BoolType)
                    throw new Exception("Left operand must be bool");

                if (rightType is not BoolType)
                    throw new Exception("Right operand must be bool");

                expr.SetType(new BoolType());
                return new BoolType();
            }

            // Equality comparisons
            if (expr.Operator is "==" or "!=")
            {
                if (leftType.GetType() != rightType.GetType())
                    throw new Exception($"Type mismatch in equality comparison: {leftType} {expr.Operator} {rightType}");

                expr.SetType(new BoolType());
                return new BoolType();
            }

            throw new Exception($"Unknown operator {expr.Operator}");
        }

        public Type VisitIncrement(IncrementNodeExpr expr)
        {
            var entry = _context.Get(expr.Id);
            if (entry == null) throw new Exception($"Variable {expr.Id} not defined");

            // FIX: Change entry.Value.Type to entry.Type
            var type = entry.Type;

            expr.SetType(type);
            return type;
        }

        public Type VisitDecrement(DecrementNodeExpr expr)
        {
            var entry = _context.Get(expr.Id);
            if (entry == null) throw new Exception($"Variable {expr.Id} not defined");

            // FIX: Change entry.Value.Type to entry.Type
            var type = entry.Type;

            expr.SetType(type);
            return type;
        }

        public Type VisitAssign(AssignNodeExpr expr)
        {
            Type valType = Visit(expr.Expression);

            // Obtain element type from either literal array or array type expression
            Type elemType = null;
            if (expr.Expression is ArrayNodeExpr arrLiteral)
                elemType = arrLiteral.ElementType;
            else if (valType is ArrayType arrayType)
                elemType = arrayType.ElementType;

            // Use 'default' for LLVMValueRef to avoid CS0246
            _context = _context.Add(expr.Id, default, null, valType, elemType);
            return valType;
        }

        public Type VisitRandom(RandomNodeExpr expr)
        {
            // var valueTypeMin = Visit(expr.MinValue); // I don't think visiting these does anything
            // var valueTypeMax = Visit(expr.MaxValue);

            expr.SetType(new IntType());
            return new IntType();
        }

        public Type VisitIf(IfNodeExpr expr)
        {
            var condType = Visit(expr.Condition);

            // 1. Condition Check
            if (condType is not BoolType)
                throw new Exception("If condition must be Bool: " + condType.ToString());

            // Use .Then and .Else to match your Node definition
            Type thenType = Visit(expr.ThenPart);
            Console.WriteLine("Type: ", thenType.GetType());

            System.Console.WriteLine("then typee:" + thenType);

            Type elseType = new VoidType();
            if (expr.ElsePart != null)
                elseType = Visit(expr.ElsePart);

            Type finalType;

            // 2. Promotion Logic (Bool <-> Number)
            if (thenType.GetType() != elseType.GetType())
            {
                // Allow mixing any Numeric type (Int/Float) with Booleans
                bool isThenNumeric = (thenType is FloatType || thenType is IntType || thenType is BoolType);
                bool isElseNumeric = (elseType is FloatType || elseType is IntType || elseType is BoolType);

                // If we have mixed Numbers and Bools
                if (isThenNumeric && isElseNumeric)
                {
                    // If either side is an Int, let's treat the result as an Int/Float 
                    // so we see numbers instead of "True/False"
                    finalType = new FloatType();
                }
                // Handle cases where one side is None (Void)
                else if (thenType is VoidType)
                    finalType = elseType;
                else if (elseType is VoidType)
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

        public Type VisitPrint(PrintNodeExpr expr)
        {
            Visit(expr.Expression);
            return new VoidType();
        }

        public Type VisitForLoop(ForLoopNodeExpr expr)
        {
            if (expr.Initialization != null) Visit(expr.Initialization);

            var condType = Visit(expr.Condition);
            // Optimization: Allow Float as condition (0.0 is false)
            if (condType is not BoolType && condType is not FloatType)
                throw new Exception("For loop condition must be Bool or Number");

            if (expr.Step != null) Visit(expr.Step);

            Visit(expr.Body); // Now visits the sequence/block
            return new VoidType();
        }

        public Type VisitSequence(SequenceNodeExpr expr)
        {
            Type lastType = new VoidType();

            foreach (var stmt in expr.Statements)
                lastType = Visit(stmt);

            return lastType;
        }

        // Inside public class TypeChecker : ITypeVisitor
        // Handle [1, 2, 3]
        public Type VisitArray(ArrayNodeExpr expr)
        {
            Type elementType = new IntType();

            if (expr.Elements.Count > 0)
            {
                elementType = Visit(expr.Elements[0]);
                expr.ElementType = elementType;
            }

            var arrayType = new ArrayType(elementType);
            expr.SetType(arrayType);
            return arrayType;
        }

        public Type VisitCopyArray(CopyArrayNodeExpr expr)
        {
            Type elementType = new IntType();
            Visit(expr.SourceArray);

            var arrayType = new ArrayType(elementType);
            expr.SetType(arrayType);
            return arrayType;
        }

        // Handle arr[0]
        // Inside TypeChecker.cs
        public Type VisitIndex(IndexNodeExpr expr)
        {
            Visit(expr.ArrayExpression); // Ensure target is valid
            Visit(expr.IndexExpression);

            Type inferred = new IntType();
            if (expr.ArrayExpression is IdNodeExpr idNode)
            {
                var entry = _context.Get(idNode.Name);
                if (entry?.Type is ArrayType arrType)
                    inferred = entry.ElementType ?? arrType.ElementType ?? new FloatType();
            }
            else if (expr.ArrayExpression is ArrayNodeExpr arrayLiteral)
            {
                inferred = arrayLiteral.ElementType ?? new FloatType();
            }
            else if (expr.ArrayExpression.Type is ArrayType arrayExprType)
            {
                inferred = arrayExprType.ElementType;
            }

            expr.SetType(inferred);
            return inferred;
        }

        public Type VisitIndexAssign(IndexAssignNodeExpr expr)
        {
            Visit(expr.ArrayExpression); // Ensure target is valid
            Visit(expr.IndexExpression);
            Visit(expr.AssignExpression);

            Type inferred = new IntType();
            if (expr.ArrayExpression is IdNodeExpr idNode)
            {
                var entry = _context.Get(idNode.Name);
                if (entry?.Type is ArrayType arrType)
                    inferred = entry.ElementType ?? arrType.ElementType ?? new FloatType();
            }
            else if (expr.ArrayExpression is ArrayNodeExpr arrayLiteral)
            {
                inferred = arrayLiteral.ElementType ?? new FloatType();
            }
            else if (expr.ArrayExpression.Type is ArrayType arrayExprType)
            {
                inferred = arrayExprType.ElementType;
            }

            expr.SetType(inferred);
            return inferred;
        }

        public Type VisitFunctionDef(FunctionDefNode node)
        {
            // 1. Convert the string return type (e.g., "int") to your MyType enum
            Type returnType = node.ReturnTypeName;

            // 2. Save the global context
            var globalContext = _context;

            // 3. Create Local Scope: Add parameters so the body can see them
            // We assume parameters are Floats/Numbers in your current setup

            foreach (var paramName in node.Parameters)
            {
                // If the function returns a string, assume parameters are strings.
                // Otherwise, assume they are numbers.
                Type pType = (returnType is StringType) ? new StringType() : new FloatType();

                _context = _context.Add(paramName, default!, null, pType, null);
            }

            // 4. Visit the body to ensure variables like 'x' and 'y' are defined
            Visit(node.Body);

            // 5. Restore Global Context (parameters shouldn't exist outside)
            _context = globalContext;

            // 6. Register the FUNCTION itself so we can call it
            // Using 'rType' (the enum) instead of 'node.ReturnTypeName' (the string)
            _context = _context.Add(node.Name, default!, null, returnType, null);

            return new VoidType();
        }

        public Type VisitFunctionCall(FunctionCallNode expr)
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

        public Type VisitRound(RoundNodeExpr expr)
        {
            Visit(expr.Value);
            expr.SetType(new FloatType());
            return new FloatType();
        }

        public Type VisitFloat(FloatNodeExpr expr)
        {
            expr.SetType(new FloatType());
            return new FloatType();
        }

        public Type VisitWhere(WhereNodeExpr expr)
        {
            if(_debug) Console.WriteLine("yo the array node in where: " + expr.ArrayExpr);
            Visit(new AssignNodeExpr(expr.IteratorId.Name, new NumberNodeExpr(0)));
            
            Visit(expr.IteratorId);
            var condType = Visit(expr.Condition);
            var arrayType = Visit(expr.ArrayExpr);

            if (arrayType is not ArrayType)
                throw new Exception("where can only be used on arrays");

            // 5. Check condition
            if (condType is not BoolType)
                throw new Exception("where condition must return bool");

            // 2. Determine element type (adjust depending on your language)
            expr.SetType(expr.ArrayExpr.Type);
            return expr.ArrayExpr.Type;
        }

        public Type VisitMap(MapNodeExpr expr)
        {
            // if(_debug) Console.WriteLine("yo the array node in where: " + expr.ArrayExpr);
            Visit(new AssignNodeExpr(expr.IteratorId.Name, new NumberNodeExpr(0)));

            Visit(expr.IteratorId);
            Visit(expr.Assignment);
            var arrayType = Visit(expr.ArrayExpr);

            if (arrayType is not ArrayType)
                throw new Exception("map can only be used on arrays");

            // 2. Determine element type (adjust depending on your language)
            expr.SetType(expr.ArrayExpr.Type);
            return expr.ArrayExpr.Type;
        }

        public Type VisitReadCsv(ReadCsvNodeExpr expr)
        {
            Visit(expr.Expression);
            Visit(expr.FileNameExpr);

            expr.SetType(new StringType());
            return new StringType();
        }

        public Type VisitAdd(AddNodeExpr expr)
        {
            Visit(expr.ArrayExpression);
            Visit(expr.AddExpression);
            expr.SetType(expr.ArrayExpression.Type);
            return expr.ArrayExpression.Type;
        }

        public Type VisitAddRange(AddRangeNodeExpr expr)
        {
            Visit(expr.ArrayExpression);
            Visit(expr.AddRangeExpression);
            expr.SetType(expr.ArrayExpression.Type);
            return expr.ArrayExpression.Type;
        }

        public Type VisitRemove(RemoveNodeExpr expr)
        {
            Visit(expr.ArrayExpression);
            Visit(expr.RemoveExpression);
            expr.SetType(expr.ArrayExpression.Type);
            return expr.ArrayExpression.Type;
        }

        public Type VisitRemoveRange(RemoveRangeNodeExpr expr)
        {
            Visit(expr.ArrayExpression);
            Visit(expr.RemoveRangeExpression);
            expr.SetType(expr.ArrayExpression.Type);
            return expr.ArrayExpression.Type;
        }

        public Type VisitLength(LengthNodeExpr expr)
        {
            expr.SetType(new IntType());
            return new IntType();
        }
        public Type VisitMin(MinNodeExpr expr)
        {
            expr.SetType(new FloatType());
            return new FloatType();
        }

        public Type VisitMax(MaxNodeExpr expr)
        {
            expr.SetType(new FloatType());
            return new FloatType();
        }

        public Type VisitMean(MeanNodeExpr expr)
        {
            expr.SetType(new FloatType());
            return new FloatType();
        }

        public Type VisitSum(SumNodeExpr expr)
        {
            expr.SetType(new FloatType());
            return new FloatType();
        }

        public Type VisitUnaryOp(UnaryOpNodeExpr expr)
        {
            // Visit the operand first
            Type operandType = Visit(expr.Operand);

            // Check the type of the operand and apply the unary minus operation
            if (operandType is IntType)
            {
                // Unary minus on an integer, result is also an integer
                expr.SetType(new IntType());
                return new IntType();
            }
            else if (operandType is FloatType)
            {
                // Unary minus on a float, result is also a float
                expr.SetType(new FloatType());
                return new FloatType();
            }
            else
            {
                // If it's neither an integer nor a float, raise a type error
                throw new Exception("Unary minus operator can only be applied to integers or floats.");
            }
        }
    }
}