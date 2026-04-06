using System.ComponentModel.Design;

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

        public Type Check(Node node)
        {
            if (_debug) Console.WriteLine("we type checking");
            return Visit(node);
        }

        private Type Visit(Node node)
        {
            var name = node.GetType().Name;

            if (_debug) Console.WriteLine("visiting: " + name.Substring(0, name.Length - 4));
            return node switch // it says the last numbers node is null
            {
                NumberNode n => VisitNumber(n),
                StringNode str => VisitString(str),
                BooleanNode b => VisitBoolean(b),
                NullNode nul => VisitNull(nul),
                IdNode id => VisitId(id),
                UnaryOpNode un => VisitUnaryOp(un),
                BinaryOpNode bin => VisitBinary(bin),
                LogicalOpNode log => VisitLogical(log),
                ComparisonNode cmp => VisitComparison(cmp),
                IncrementNode inc => VisitIncrement(inc),
                DecrementNode dec => VisitDecrement(dec),
                AssignNode assign => VisitAssign(assign),
                SequenceNode seq => VisitSequence(seq),
                RandomNode ran => VisitRandom(ran),
                IfNode _if => VisitIf(_if),
                PrintNode pr => VisitPrint(pr),
                ForLoopNode _for => VisitForLoop(_for),
                ForEachLoopNode _foreach => VisitForEachLoop(_foreach),
                ArrayNode arr => VisitArray(arr),
                IndexNode idx => VisitIndex(idx),
                IndexAssignNode idxa => VisitIndexAssign(idxa),
                WhereNode whe => VisitWhere(whe),
                MapNode map => VisitMap(map),
                ReadCsvNode read => VisitReadCsv(read),

                ToCsvNode tocsv => VisitToCsv(tocsv),
                AddNode add => VisitAdd(add),
                AddRangeNode addr => VisitAddRange(addr),
                RemoveNode remo => VisitRemove(remo),
                RemoveRangeNode remor => VisitRemoveRange(remor),
                LengthNode len => VisitLength(len),
                MinNode min => VisitMin(min),
                MaxNode max => VisitMax(max),
                MeanNode mean => VisitMean(mean),
                SumNode sum => VisitSum(sum),
                //FunctionDefNode fdef => VisitFunctionDef(fdef),
                //FunctionCallNode fcall => VisitFunctionCall(fcall),
                RoundNode rnd => VisitRound(rnd),
                FloatNode flt => VisitFloat(flt),
                RecordNode rec => VisitRecord(rec),
                RecordFieldNode recf => VisitRecordField(recf),
                RecordFieldAssignNode reca => VisitRecordFieldAssign(reca),
                CopyNode cop => VisitCopy(cop),
                AddFieldNode radd => VisitAddField(radd),
                RemoveFieldNode rrem => VisitRemoveField(rrem),
                DataframeNode df => VisitDataframe(df),
                ColumnsNode cols => VisitColumns(cols),
                ShowDataframeNode showdf => VisitShowDataframe(showdf),
                //InternalDataframeFieldNode interDFField => VisitInternalDataframeField(interDFField),
                //PropertyAccessNode propAccecc => VisitPropertyAccess(propAccecc),
                NamedArgumentNode namedArg => VisitNamedArgument(namedArg),
                TypeLiteralNode typeLit => VisitTypeLiteral(typeLit),

                _ => throw new NotSupportedException($"Type check not implemented for {node.GetType().Name}")
            };
        }

        // Inside TypeChecker.cs
        public Type VisitForEachLoop(ForEachLoopNode expr)
        {
            var collectionType = Visit(expr.Array);
            RecordType rowType = null;

            // Determine the type of 'item'
            if (collectionType is DataframeType df) rowType = df.RowType;
            else if (collectionType is ArrayType arr && arr.ElementType is RecordType rec) rowType = rec;

            if (rowType == null) throw new Exception("ForEach requires a Dataframe or Array of Records");

            //_context = _context.Add(expr.Iterator.Name, null, rowType);
            var testEntry = _context.Get(expr.Iterator.Name);
            Console.WriteLine($"[DEBUG] Internal Check: Found '{expr.Iterator.Name}'? " + (testEntry != null));


            // CRITICAL: Push the context so 'item' exists for the body
            var oldContext = _context;

            // Use named arguments to be 100% safe
            _context = _context.Add(
                name: expr.Iterator.Name,
                value: default,
                _value: null!,
                type: rowType // Make sure it hits the 4th parameter!
            );

            Visit(expr.Body);
            _context = oldContext;
            return new VoidType();
        }


        public Type VisitNumber(NumberNode expr)
        {
            expr.SetType(new IntType());
            return expr.Type;
        }

        public Type VisitString(StringNode expr)
        {
            expr.SetType(new StringType());
            return expr.Type;
        }

        public Type VisitBoolean(BooleanNode expr)
        {
            expr.SetType(new BoolType());
            return expr.Type;
        }

        public Type VisitNull(NullNode expr)
        {
            expr.SetType(new NullType());
            return expr.Type;
        }


        public Type VisitId(IdNode expr)
        {
            // if(_debug) Console.WriteLine(" we looking at id: " + expr.Name + " and its type is " + entry?.Type);
            // if(_debug) Console.WriteLine("id type: " + entry.Type); // prof we can get records type at type check time

            Console.WriteLine($"[DEBUG] VisitId looking for: '{expr.Name}'");

            var entry = _context.Get(expr.Name);

            Console.WriteLine($"[DEBUG] Looking for '{expr.Name}'. Found? {entry != null}");

            if (entry != null)
            {
                // If rowType was put in the wrong parameter, entry.Type is null here!
                expr.SetType(entry.Type);
                return entry.Type;
            }
            return expr.Type;
        }


        public Type VisitBinary(BinaryOpNode expr)
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
                    return expr.Type;
                }
                // 2. Handle String Concatenation (Updated to allow Mixed Types)
                if (expr.Operator == "+")
                {
                    // If either side is a string, the result is a string
                    if (leftType is StringType || rightType is StringType)
                    {
                        expr.SetType(new StringType());
                        return expr.Type;
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
                    return expr.Type;
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
                    return expr.Type;
                }

                if (leftType == rightType)
                {
                    expr.SetType(new BoolType());
                    return expr.Type;
                }
            }

            throw new Exception($"Unknown operator {expr.Operator} or type mismatch: {leftType} and {rightType}");
        }

        public Type VisitLogical(LogicalOpNode expr)
        {
            var leftType = Visit(expr.Left);
            var rightType = Visit(expr.Right);

            // Helper to check if a type is numeric (Int or Float)
            if (leftType is not BoolType || rightType is not BoolType)
                throw new Exception($"Invalid operands type {leftType} and {rightType}");


            if (expr.Operator is "&&" or "||")
            {
                expr.SetType(new BoolType());
                return expr.Type;
            }

            throw new Exception($"Unknown operator {expr.Operator} or type mismatch: {leftType} and {rightType}");
        }

        public Type VisitComparison(ComparisonNode expr)
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
                return expr.Type;
            }

            // === ORDERING (< > <= >=) ===
            if (expr.Operator is "<" or ">" or "<=" or ">=")
            {
                if (!(isLeftNum && isRightNum))
                    throw new Exception(
                        $"Operator {expr.Operator} requires numeric operands, got {leftType} and {rightType}");

                expr.SetType(new BoolType());
                return expr.Type;
            }

            // === EQUALITY (== !=) ===
            if (expr.Operator is "==" or "!=")
            {
                // allow numeric mixing (int == float)
                if (isLeftNum && isRightNum)
                {
                    expr.SetType(new BoolType());
                    return expr.Type;
                }

                if (leftType.GetType() != rightType.GetType())
                    throw new Exception(
                        $"Type mismatch in equality comparison: {leftType} {expr.Operator} {rightType}");

                expr.SetType(new BoolType());
                return expr.Type;
            }

            throw new Exception($"Unknown operator {expr.Operator}");
        }

        public Type VisitIncrement(IncrementNode expr)
        {
            var entry = _context.Get(expr.Id);
            if (entry == null) throw new Exception($"Variable {expr.Id} not defined");

            // FIX: Change entry.Value.Type to entry.Type
            var type = entry.Type;

            expr.SetType(type);
            return expr.Type;
        }

        public Type VisitDecrement(DecrementNode expr)
        {
            var entry = _context.Get(expr.Id);
            if (entry == null) throw new Exception($"Variable {expr.Id} not defined");

            // FIX: Change entry.Value.Type to entry.Type
            var type = entry.Type;

            expr.SetType(type);
            return expr.Type;
        }

        public Type VisitAssign(AssignNode expr)
        {
            Type valType = Visit(expr.Expression);

            // Obtain element type from either literal array or array type expression
            Type elemType = null;
            if (expr.Expression is ArrayNode arrLiteral)
                elemType = arrLiteral.ElementType;
            else if (valType is ArrayType arrayType)
                elemType = arrayType.ElementType;

            // Use 'default' for LLVMValueRef to avoid CS0246
            _context = _context.Add(expr.Id, default, null, valType, elemType);
            expr.SetType(valType); // <--- ADD THIS LINE
            return valType;
        }

        public Type VisitRandom(RandomNode expr)
        {
            // var valueTypeMin = Visit(expr.MinValue); // I don't think visiting these does anything
            // var valueTypeMax = Visit(expr.MaxValue);

            expr.SetType(new IntType());
            return expr.Type;
        }

        public Type VisitIf(IfNode expr)
        {
            // 1. Visit the condition and ensure it's a Boolean
            var condType = Visit(expr.Condition);
            if (condType is not BoolType)
            {
                throw new Exception($"Type Error: 'if' condition must be a bool, but got {condType}");
            }

            // 2. Visit branches
            Type thenType = Visit(expr.ThenPart);
            Type elseType = (expr.ElsePart != null) ? Visit(expr.ElsePart) : new VoidType();

            // Debugging (Fixed your Console.WriteLine syntax)
            Console.WriteLine($"[TypeCheck] If branches: Then={thenType}, Else={elseType}");

            Type finalType;

            // 3. Unification / Promotion Logic
            if (thenType.GetType() == elseType.GetType())
            {
                // Exact match (e.g., both are Void, both are Int, or both are String)
                finalType = thenType;
            }
            else
            {
                // One branch is Void (Statement style: if (c) { x=1 })
                if (thenType is VoidType)
                    finalType = elseType;
                else if (elseType is VoidType)
                    finalType = thenType;

                // Mixed Numeric types (Expression style: x = if (c) 1.5 else 2)
                else if (IsNumeric(thenType) && IsNumeric(elseType))
                {
                    // Promote to Float if either side is Float or Bool (treated as 0/1)
                    finalType = (thenType is FloatType || elseType is FloatType)
                                ? new FloatType()
                                : new IntType();
                }
                else
                {
                    // HARD ERROR: Trying to mix incompatible types (e.g., String and Int)
                    throw new Exception($"Type Mismatch: 'if' branches are incompatible ({thenType} vs {elseType})");
                }
            }

            expr.SetType(finalType);
            return finalType;
        }

        // Helper method to keep the code clean
        private bool IsNumeric(Type t) => t is IntType || t is FloatType || t is BoolType;


        public Type VisitPrint(PrintNode expr)
        {
            Visit(expr.Expression);
            return new VoidType();
        }

        public Type VisitForLoop(ForLoopNode expr)
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

        public Type VisitSequence(SequenceNode expr)
        {
            Type lastType = new VoidType();

            foreach (var stmt in expr.Statements)
                lastType = Visit(stmt);

            return lastType;
        }

        // Inside public class TypeChecker : ITypeVisitor
        // Handle [1, 2, 3]
        public Type VisitArray(ArrayNode expr)
        {
            Type elementType = expr.ElementType;

            if (expr.Elements.Count > 0)
            {
                elementType = Visit(expr.Elements[0]);
                expr.ElementType = elementType;
            }

            var arrayType = new ArrayType(elementType);
            expr.SetType(arrayType);
            return expr.Type;
        }

        public Type VisitCopy(CopyNode expr)
        {
            Visit(expr.Source);
            expr.SetType(expr.Source.Type);
            return expr.Type;
        }

        // Handle arr[0]
        // Inside TypeChecker.cs
        public Type VisitIndex(IndexNode expr)
        {
            Visit(expr.SourceExpression);
            Visit(expr.IndexExpression);

            Type inferred = new IntType();
            if (expr.SourceExpression is IdNode idNode) // In the parser whe instantiate the index with an IdNode, the rest of the code should never be true
            {
                var entry = _context.Get(idNode.Name);
                if (entry?.Type is ArrayType arrType)
                    inferred = entry.ElementType ?? arrType.ElementType ?? new IntType();
                else if (entry?.Type is DataframeType dfType)
                    inferred = dfType.RowType;
            }
            // else if (expr.SourceExpression is ArrayNode arrayLiteral)
            // {
            //     inferred = arrayLiteral.ElementType ?? new FloatType();
            // }
            // else if (expr.SourceExpression.Type is ArrayType arrayExprType)
            // {
            //     inferred = arrayExprType.ElementType;
            // }

            expr.SetType(inferred);
            return expr.Type;
        }

        public Type VisitIndexAssign(IndexAssignNode expr)
        {
            Visit(expr.ArrayExpression);
            Visit(expr.IndexExpression);
            Visit(expr.AssignExpression);

            if (expr.ArrayExpression is IdNode idNode)
            {
                var entry = _context.Get(idNode.Name);
                var assignType = expr.AssignExpression.Type.GetType();
                var arrayType = entry?.ElementType.GetType();
                if (arrayType != assignType)
                    throw new Exception($"Can't assign {arrayType.Name} to {assignType.Name} array");
            }

            expr.SetType(new VoidType());
            return expr.Type;
        }


        public Type VisitRound(RoundNode expr)
        {
            Visit(expr.Value);
            expr.SetType(new FloatType());
            return expr.Type;
        }

        public Type VisitFloat(FloatNode expr)
        {
            expr.SetType(new FloatType());
            return expr.Type;
        }

        public Type VisitWhere(WhereNode expr)
        {
            var arrayType = Visit(expr.SourceExpr);

            Console.WriteLine("we got source: " + expr.SourceExpr.Type);

            // we need to assign the lambda value and give it the specific type

            if (arrayType is ArrayType arrType)
            {
                if (arrType.ElementType is StringType)
                    Visit(new AssignNode(expr.IteratorId.Name, new StringNode("")));
                else if (arrType.ElementType is BoolType)
                    Visit(new AssignNode(expr.IteratorId.Name, new BooleanNode(false)));
                else
                    Visit(new AssignNode(expr.IteratorId.Name, new NumberNode(0)));
            }
            else if (arrayType is DataframeType dfType)
            {
                var recordArguments = new List<NamedArgumentNode>();
                for (int i = 0; i < dfType.ColumnNames.Count; i++)
                {
                    if (dfType.DataTypes[i] is StringType)
                        recordArguments.Add(new NamedArgumentNode(dfType.ColumnNames[i], new StringNode("")));
                    else if (dfType.DataTypes[i] is BoolType)
                        recordArguments.Add(new NamedArgumentNode(dfType.ColumnNames[i], new BooleanNode(false)));
                    else
                        recordArguments.Add(new NamedArgumentNode(dfType.ColumnNames[i], new NumberNode(0)));
                }

                var record = new RecordNode(recordArguments);

                Visit(new AssignNode(expr.IteratorId.Name, record));
                Console.WriteLine(" we got data frame, now to get the records and assign to iterator");
            }
            else
                throw new Exception("where can only be used on arrays and dataframes");

            Visit(expr.IteratorId);

            Console.WriteLine("array type: " + arrayType);

            var condType = Visit(expr.Condition);
            Console.WriteLine("we got comparison: " + condType);

            if (arrayType is not ArrayType && arrayType is not DataframeType)
                throw new Exception("where can only be used on arrays and dataframes");

            // 5. Check condition
            if (condType is not BoolType)
                throw new Exception("where condition must return bool");

            // 2. Determine element type (adjust depending on your language)
            expr.SetType(expr.SourceExpr.Type);
            return expr.Type;
        }

        public Type VisitMap(MapNode expr)
        {
            var sourceType = Visit(expr.SourceExpr);
            Type elementType;

            // 1. Determine what the iterator 'x' represents
            if (sourceType is ArrayType arrayType)
            {
                elementType = arrayType.ElementType;
            }
            else if (sourceType is DataframeType dfType)
            {
                // For dataframes, we iterate over the RowType (the Record)
                elementType = dfType.RowType;
            }
            else
            {
                throw new Exception("Map source must be an array or a dataframe.");
            }

            var previousContext = _context;

            // 2. Inject 'x' into the context with the correct type (Primitive or Record)
            _context = _context.Add(expr.IteratorId.Name, default, null!, elementType);

            try
            {
                // Visit the iterator ID to ensure it's registered
                Visit(expr.IteratorId);

                // 3. Visit the transformation (the lambda body)
                // If x is a Record, x.name will now resolve correctly because x has RowType
                var bodyType = Visit(expr.Assignment);

                // 4. The result of a Map is always an Array of whatever the body returns
                var resultType = new ArrayType(bodyType);
                expr.SetType(resultType);
                return resultType;
            }
            finally
            {
                _context = previousContext;
            }
        }

        public Type VisitReadCsv(ReadCsvNode expr)
        {
            // Debug to console
            Console.WriteLine($"TypeChecking ReadCsv: Path is {expr.FileNameExpr?.GetType().Name}, Schema is {expr.SchemaExpr?.GetType().Name}");

            Visit(expr.FileNameExpr);

            if (expr.SchemaExpr == null)
                throw new Exception("read_csv requires a schema.");

            Type schemaType = Visit(expr.SchemaExpr);

            if (schemaType is RecordType recType)
            {
                var names = recType.RecordFields.Select(f => f.Label).ToList();
                var types = recType.RecordFields.Select(f => f.Value?.Type ?? f.Type).ToList();

                for (int i = 0; i < recType.RecordFields.Count; i++)
                {
                    recType.RecordFields[i].Type = types[i];
                }

                var dfType = new DataframeType(names, types, recType);
                expr.SetType(dfType);
                return dfType;
            }

            // If we reach here, the schema wasn't a record
            throw new Exception($"read_csv requires a record template, but got {schemaType?.GetType().Name}");
        }

        public Type VisitToCsv(ToCsvNode expr)
        {
            // 1. Get the types of the arguments
            Type exprType = Visit(expr.Expression);
            Type pathType = Visit(expr.FileNameExpr);

            // 2. Semantic Check: Is the first argument actually a Dataframe?
            if (!(exprType is DataframeType))
            {
                throw new Exception($"to_csv() error: First argument must be a Dataframe, but got {exprType?.GetType().Name}");
            }

            // 3. Semantic Check: Is the second argument a String?
            if (!(pathType is StringType))
            {
                throw new Exception($"to_csv() error: Second argument must be a String (file path), but got {pathType?.GetType().Name}");
            }

            // 4. Set the return type to Void/None 
            // (Ensure this matches whatever type your REPL uses for 'null' results)
            var voidType = new VoidType();
            expr.SetType(voidType);
            return voidType;
        }

        public Type VisitAdd(AddNode expr)
        {
            var arrayType = Visit(expr.SourceExpression);
            var addType = Visit(expr.AddExpression);

            // check if the array and the node are of the same type

            if (arrayType is DataframeType dfType)
            {
                if (addType is not RecordType recType)
                    throw new Exception("add can only add records to dataframes");

                // 1. Check Field Names (You already have this)
                var dfColumns = dfType.ColumnNames;
                var recFields = recType.RecordFields.Select(f => f.Label).ToList();
                if (!dfColumns.All(col => recFields.Contains(col)))
                    throw new Exception("Record fields do not match dataframe columns");

                // 2. NEW: Check Field Types
                foreach (var dfField in dfType.RowType.RecordFields)
                {
                    var providedField = recType.RecordFields.First(f => f.Label == dfField.Label);

                    // Check if types match exactly
                    if (dfField.Type.GetType() != providedField.Type.GetType())
                    {
                        // Optional: Allow Int -> Float promotion but throw error for others
                        if (!(dfField.Type is FloatType && providedField.Type is IntType))
                        {
                            throw new Exception($"Type mismatch for field '{dfField.Label}': " +
                                $"expected {dfField.Type}, but got {providedField.Type}");
                        }
                    }
                }
            }
            else if (arrayType is ArrayType arrType)
            {
                var arrayElementType = ((ArrayType)arrayType).ElementType;

                if (arrayElementType.GetType() != addType.GetType())
                    throw new Exception($"Can't add {addType} value to a {arrayElementType} array");
            }
            else
                throw new Exception("add can only be used on arrays and dataframes");

            expr.SetType(expr.SourceExpression.Type);
            return expr.Type;
        }

        public Type VisitAddRange(AddRangeNode expr)
        {
            Visit(expr.SourceExpression);
            Visit(expr.AddRangeExpression);
            expr.SetType(expr.SourceExpression.Type);
            return expr.Type;
        }

        public Type VisitRemove(RemoveNode expr)
        {
            Visit(expr.SourceExpression);
            Visit(expr.RemoveExpression);
            expr.SetType(expr.SourceExpression.Type);
            return expr.Type;
        }

        public Type VisitRemoveRange(RemoveRangeNode expr)
        {
            Visit(expr.SourceExpression);
            Visit(expr.RemoveRangeExpression);
            expr.SetType(expr.SourceExpression.Type);
            return expr.Type;
        }

        public Type VisitLength(LengthNode expr)
        {
            Visit(expr.ArrayExpression);
            expr.SetType(new IntType());
            return expr.Type;
        }

        static void ValidType(Type arrayElementType)
        {
            if (arrayElementType is StringType or BoolType or ArrayType)
                throw new Exception($"Can't find minimum value for {arrayElementType.GetType().Name}");
        }

        public Type VisitMin(MinNode expr)
        {
            var arrayElementType = (Visit(expr.ArrayExpression) as ArrayType).ElementType;
            ValidType(arrayElementType);

            expr.SetType(arrayElementType);
            return expr.Type;
        }

        public Type VisitMax(MaxNode expr)
        {
            var arrayElementType = (Visit(expr.ArrayExpression) as ArrayType).ElementType;
            ValidType(arrayElementType);

            expr.SetType(arrayElementType);
            return expr.Type;
        }

        public Type VisitMean(MeanNode expr)
        {
            var arrayElementType = (Visit(expr.ArrayExpression) as ArrayType).ElementType;
            ValidType(arrayElementType);

            expr.SetType(new FloatType());
            return expr.Type;
        }

        public Type VisitSum(SumNode expr)
        {
            var arrayElementType = (Visit(expr.ArrayExpression) as ArrayType).ElementType;
            ValidType(arrayElementType);

            expr.SetType(arrayElementType);
            return expr.Type;
        }

        public Type VisitUnaryOp(UnaryOpNode expr)
        {
            // Visit the operand first
            Type operandType = Visit(expr.Operand);

            // Check the type of the operand and apply the unary minus operation
            if (operandType is IntType)
                expr.SetType(new IntType()); // Unary minus on an integer, result is also an integer
            else if (operandType is FloatType)
                expr.SetType(new FloatType());  // Unary minus on a float, result is also a float
            else
                throw new Exception("Unary minus operator can only be applied to integers or floats.");

            return expr.Type;
        }

        public Type VisitRecord(RecordNode expr)
        {
            foreach (var field in expr.Fields)
            {
                // 1. Visit the node (NamedArgument or Value)
                Type fieldType = Visit(field.Value);

                // 2. Explicitly store the type in the node instance
                // This ensures field.Value.Type and field.Type are populated
                field.Value.SetType(fieldType);
                field.Type = fieldType;

                if (_debug) Console.WriteLine($"Field {field.Label} resolved to {fieldType}");
            }

            // 3. Create the RecordType using the now-populated fields
            var recordType = new RecordType(expr.Fields);

            expr.SetType(recordType);

            return recordType;
        }

        public Type ResolveType(TypeNode node)
        {
            return node.Name switch
            {
                "int" => new IntType(),
                "float" => new FloatType(),
                "string" => new StringType(),
                "bool" => new BoolType(),
                _ => throw new Exception($"Unknown type '{node.Name}'")
            };
        }

        public Type VisitRecordField(RecordFieldNode expr)
        {
            Console.WriteLine($"[DEBUG] Visiting RecordField. Accessing '{expr.IdField}' on node '{expr.IdRecord}'");

            // 1. THIS IS THE TRIGGER
            var lhsType = Visit(expr.IdRecord);

            Console.WriteLine($"[DEBUG] Result of visiting '{expr.IdRecord}': " + (lhsType?.ToString() ?? "STILL NULL"));
            if (lhsType is RecordType rt)
            {
                var field = rt.RecordFields.FirstOrDefault(f => f.Label == expr.IdField);
                if (field != null)
                {
                    // Stamp the node type so CodeGen knows it's a string/int/etc.
                    expr.SetType(field.Type);
                    return field.Type;
                }
                throw new Exception($"Field {expr.IdField} not found in record.");
            }

            // This is where you are hitting the error because lhsType is still NULL
            throw new Exception($"Cannot access field '{expr.IdField}' on non-record type: {lhsType}");
        }

        public Type VisitRecordFieldAssign(RecordFieldAssignNode expr)
        {
            Visit(expr.AssignExpression);
            Visit(expr.IdRecord);

            expr.SetType(new VoidType());
            return expr.Type;
        }


        public Type VisitAddField(AddFieldNode expr)
        {
            Visit(expr.Record);
            Visit(expr.Value);
            expr.SetType(expr.Value.Type);
            return expr.Type;
        }

        public Type VisitRemoveField(RemoveFieldNode expr)
        {
            Visit(expr.Record);
            expr.SetType(new VoidType());
            return expr.Type;
        }
        public Type VisitDataframe(DataframeNode expr)
        {
            // 1. Extract and Normalize Columns
            var columnNames = expr.Columns.Elements
                .OfType<StringNode>()
                .Select(c => c.Value)
                .ToList();

            // Ensure index is present in the AST and name list
            if (!expr.HasIndex)
            {
                // Insert into the actual AST so Code Gen sees it
                expr.Columns.Elements.Insert(0, new StringNode("index"));
                columnNames.Insert(0, "index");

                if (expr.Rows != null)
                {
                    for (int i = 0; i < expr.Rows.Elements.Count; i++)
                    {
                        if (expr.Rows.Elements[i] is RecordNode record)
                        {
                            var indexNode = new NumberNode(i);
                            // CRITICAL: Type check the injected node immediately
                            Visit(indexNode);

                            record.Fields.Insert(0, new RecordField
                            {
                                Label = "index",
                                Value = indexNode
                            });
                        }
                    }
                }
                expr.HasIndex = true;
            }

            // 2. Determine Types
            List<Type> columnTypes;
            RecordType rowType;

            if (expr.Rows != null && expr.Rows.Elements.Count > 0)
            {
                // Infer from first row
                var firstRow = (RecordNode)expr.Rows.Elements[0];
                // This Visit call will now see the 'index' field we inserted
                var inferredRowType = Visit(firstRow) as RecordType;

                columnTypes = new List<Type>();
                var finalFields = new List<RecordField>();

                foreach (var col in columnNames)
                {
                    var field = inferredRowType.RecordFields.FirstOrDefault(f => f.Label == col);
                    if (field == null) throw new Exception($"Column '{col}' missing in row.");

                    columnTypes.Add(field.Value.Type);
                    finalFields.Add(field);
                }
                rowType = new RecordType(finalFields);
            }
            else
            {
                // Explicit types for empty dataframe
                if (expr.DataTypes == null) throw new Exception("Empty dataframe requires 'types'.");

                columnTypes = expr.DataTypes.Elements.Select(e => ResolveTypeNode(e)).ToList();
                // Prepend index type to match the columnNames insertion
                columnTypes.Insert(0, new IntType());

                rowType = new RecordType(columnNames.Select((name, i) => new RecordField
                {
                    Label = name,
                    Type = columnTypes[i]
                }).ToList());
            }

            var dfType = new DataframeType(columnNames, columnTypes, rowType);
            expr.SetType(dfType);
            return dfType;
        }

        private Type ResolveTypeNode(ExpressionNode expr)
        {
            if (expr is TypeLiteralNode t)
            {
                return t.TypeNode.Name switch
                {
                    "int" => new IntType(),
                    "float" => new FloatType(),
                    "bool" => new BoolType(),
                    "string" => new StringType(),
                    _ => throw new Exception($"Unknown type '{t.TypeNode.Name}'")
                };
            }

            throw new Exception("Expected type node.");
        }

        public Type VisitNamedArgument(NamedArgumentNode expr)
        {
            // If expr.Value is a StringNode ("Bob"), Visit returns StringType
            Type valType = Visit(expr.Value);

            // Save the type on the NamedArgument node itself
            expr.SetType(valType);

            return valType;
        }

        public Type VisitShowDataframe(ShowDataframeNode expr)
        {
            // 1. Resolve the source Dataframe
            var sourceType = Visit(expr.Source) as DataframeType;
            if (sourceType == null)
                throw new Exception("Show requires a Dataframe source.");

            var columnNames = new List<string>();
            var columnTypes = new List<Type>();
            var semanticFields = new List<RecordField>();

            // 2. Validate requested columns against the source schema
            foreach (var colExpr in expr.Columns)
            {
                // Visit the column name expression (usually a StringNode)
                var colExprType = Visit(colExpr);
                if (!(colExprType is StringType))
                    throw new Exception($"Show column names must be strings, got {colExprType}");

                if (colExpr is StringNode strNode)
                {
                    string name = strNode.Value;

                    // Find the index of this column in the source dataframe
                    int idx = sourceType.ColumnNames.IndexOf(name);
                    if (idx < 0)
                        throw new Exception($"Column '{name}' not found in dataframe. Available: {string.Join(", ", sourceType.ColumnNames)}");

                    // Pull the correct Type (which we just fixed in VisitDataframe)
                    var colType = sourceType.DataTypes[idx];
                    if (colType == null)
                        throw new Exception($"Internal Error: Column '{name}' has a null type in source dataframe.");

                    columnNames.Add(name);
                    columnTypes.Add(colType);

                    // 3. Build the RecordField for the new RowType
                    semanticFields.Add(new RecordField
                    {
                        Label = name,
                        Type = colType
                        // Value is null because this is a Type definition
                    });
                }
                else
                {
                    throw new Exception("Show columns must be string literals (e.g., \"name\")");
                }
            }

            // 4. Create the new RowType (a subset of the original record)
            var rowType = new RecordType(semanticFields);

            // 5. Construct the resulting DataframeType
            // Note: Show creates a "View" or a new DF structure with only selected columns
            var resultType = new DataframeType(columnNames, columnTypes, rowType);

            expr.SetType(resultType);
            return resultType;
        }

        public Type VisitColumns(ColumnsNode expr)
        {
            Visit(expr.DataframeExpression);
            expr.SetType(new ArrayType(new StringType()));
            return expr.Type;
        }
        public Type VisitTypeLiteral(TypeLiteralNode expr)
        {
            return ResolveType(expr.TypeNode); // Just return the wrapped type

        }

        // public Type VisitFunctionDef(FunctionDefNode node)
        // {
        //     // 1. Convert the string return type (e.g., "int") to your MyType enum
        //     Type returnType = node.ReturnTypeName;

        //     // 2. Save the global context
        //     var globalContext = _context;

        //     // 3. Create Local Scope: Add parameters so the body can see them
        //     // We assume parameters are Floats/Numbers in your current setup

        //     foreach (var paramName in node.Parameters)
        //     {
        //         // If the function returns a string, assume parameters are strings.
        //         // Otherwise, assume they are numbers.
        //         Type pType = (returnType is StringType) ? new StringType() : new FloatType();

        //         _context = _context.Add(paramName, default!, null, pType, null);
        //     }

        //     // 4. Visit the body to ensure variables like 'x' and 'y' are defined
        //     Visit(node.Body);

        //     // 5. Restore Global Context (parameters shouldn't exist outside)
        //     _context = globalContext;

        //     // 6. Register the FUNCTION itself so we can call it
        //     // Using 'rType' (the enum) instead of 'node.ReturnTypeName' (the string)
        //     _context = _context.Add(node.Name, default!, null, returnType, null);

        //     return new VoidType();
        // }

        // public Type VisitFunctionCall(FunctionCallNode expr)
        // {
        //     var entry = _context.Get(expr.Name);
        //     if (entry == null) throw new Exception($"Function {expr.Name} is not defined");

        //     // Check if the number of arguments matches (Very important for LLVM!)
        //     // Note: You'll need to store the parameter count in your ContextEntry to do this properly.

        //     foreach (var arg in expr.Arguments) Visit(arg);

        //     // Use the actual return type of the function!
        //     expr.SetType(entry.Type);
        //     return expr.Type;
        // }
    }
}