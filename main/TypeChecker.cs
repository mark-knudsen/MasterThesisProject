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
            return node switch
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
                CorrelationNode corr => VisitCorrelation(corr),
                RoundNode rnd => VisitRound(rnd),
                FloatNode flt => VisitFloat(flt),
                RecordNode rec => VisitRecord(rec),
                FieldNode recf => VisitField(recf),
                RecordFieldAssignNode reca => VisitRecordFieldAssign(reca),
                CopyNode cop => VisitCopy(cop),
                DataframeNode df => VisitDataframe(df),
                ColumnsNode cols => VisitColumns(cols),
                NamedArgumentNode namedArg => VisitNamedArgument(namedArg),
                TypeLiteralNode typeLit => VisitTypeLiteral(typeLit),
                SqrtNode sqrt => VisitSqrt(sqrt),
                LogNode log => VisitLog(log),
                PowNode pow => VisitPow(pow),
                ExponentialMathFuncNode exp => VisitExponentialMathFunc(exp),
                CastNode cast => VisitCast(cast),
                SliceNode slice => VisitSlice(slice),

                _ => throw new NotSupportedException($"Type check not implemented for {node.GetType().Name}")
            };
        }

        public Type VisitForEachLoop(ForEachLoopNode statement)
        {
            var collectionType = Visit(statement.Source);
            Type rowType = null;

            // Determine the type of 'item'
            if (collectionType is DataframeType df) rowType = df.RowType;
            else if (collectionType is ArrayType arr && arr.ElementType is not RecordType) rowType = arr.ElementType;
            else if (collectionType is ArrayType arr2 && arr2.ElementType is RecordType rec) rowType = rec;

            if (rowType == null) throw new Exception("ForEach requires a Dataframe or Array of Records");

            // it should use an assign node to assign the value instead of harcoding it here
            var previousContext = _context;
            _context = _context.Add(statement.Iterator.Name, type: rowType);
            Visit(statement.Body);
            _context = previousContext; // Safely restore outer scope
            statement.SetType(new VoidType());
            return statement.Type;
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
            var entry = _context.Get(expr.Name);

            if (entry == null)
                throw new Exception($"type check - Undefined variable '{expr.Name}'");

            // if (_debug) Console.WriteLine("found entry for " + expr.Name + " with type " + entry.Type);

            expr.SetType(entry.Type);
            return entry.Type;
        }

        public Type VisitBinary(BinaryOpNode expr)
        {
            Type leftType = Visit(expr.Left);
            Type rightType = Visit(expr.Right);

            bool isLeftArray = leftType is ArrayType;
            bool isRightArray = rightType is ArrayType;

            // Handle Vectorized Operations (Array + Scalar, Scalar + Array, Array + Array)
            if (isLeftArray || isRightArray)
            {
                Type elementL = isLeftArray ? ((ArrayType)leftType).ElementType : leftType;
                Type elementR = isRightArray ? ((ArrayType)rightType).ElementType : rightType;

                // Ensure elements are numeric for math operators
                if (expr.Operator is "+" or "-" or "*" or "/")
                {
                    bool isNumL = elementL is IntType || elementL is FloatType;
                    bool isNumR = elementR is IntType || elementR is FloatType;

                    if (!isNumL || !isNumR)
                        throw new Exception($"Vectorized math requires numeric elements, got {elementL} and {elementR}");

                    Type resultElement = (elementL is FloatType || elementR is FloatType) ? new FloatType() : new IntType();

                    // If it's a division, we always promote to Float
                    if (expr.Operator == "/") resultElement = new FloatType();

                    var resultType = new ArrayType(resultElement);
                    expr.SetType(resultType);
                    return resultType;
                }
            }

            if (expr.Operator == "/" || expr.Operator == "/=")
            {
                if (leftType is IntType || rightType is IntType)
                {

                    expr.Left = InsertCast(expr.Left, leftType, new FloatType());
                    expr.Right = InsertCast(expr.Right, rightType, new FloatType());

                    leftType = Visit(expr.Left);
                    rightType = Visit(expr.Right);
                }
            }

            // Helper to check if a type is numeric (Int or Float)
            bool isLeftNum = leftType is IntType || leftType is FloatType;
            bool isRightNum = rightType is IntType || rightType is FloatType;

            if (expr.Operator == "+")
            {
                // 1. Handle Numeric Addition (allow mixing Int and Float)
                if (isLeftNum && isRightNum)
                {
                    // If both are Int, result is Int. If any is Float, promote to Float.
                    Type resultType = (leftType is FloatType || rightType is FloatType) ? new FloatType() : new IntType();
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

                if (leftType is RecordType l && rightType is RecordType r)
                {
                    var resultFields = new List<RecordField>();

                    var rhsMap = r.RecordFields.ToDictionary(f => f.Label);

                    // 1. Preserve LHS order
                    foreach (var lf in l.RecordFields)
                    {
                        if (rhsMap.TryGetValue(lf.Label, out var rf))
                        {
                            // override type from RHS
                            resultFields.Add(rf);
                        }
                        else
                        {
                            resultFields.Add(lf);
                        }
                    }

                    // 2. Append RHS-only fields
                    var lhsLabels = l.RecordFields.Select(f => f.Label).ToHashSet();

                    foreach (var rf in r.RecordFields)
                    {
                        if (!lhsLabels.Contains(rf.Label))
                        {
                            resultFields.Add(rf);
                        }
                    }

                    var mergedType = new RecordType(resultFields);
                    expr.SetType(mergedType);
                    return mergedType;
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
            if (expr.Operator is "==" or "!=" or "<" or ">" or "<=" or ">=")
            {
                // For comparisons, we just need the types to be compatible
                // (e.g., comparing a Float and an Int is fine)
                if (isLeftNum && isRightNum || leftType == rightType)
                {
                    expr.SetType(new BoolType());
                    return expr.Type;
                }
            }

            throw new Exception($"Unknown operator {expr.Operator} or type mismatch: {leftType} and {rightType}");
        }

        public Type VisitCast(CastNode expr)
        {
            Type fromType = Visit(expr.Expression);
            Type toType = expr.ToType;

            if (fromType.GetType() == toType.GetType())
            {
                expr.SetType(toType);
                return toType;
            }

            // 1. Existing: Numeric conversion
            if (fromType is IntType && toType is FloatType)
            {
                expr.SetType(toType);
                return toType;
            }

            // 2. NEW: Storage Alignment casts
            // Allow Float -> Int (for bitcasting) and Bool -> Int (for zero-extending)
            if ((fromType is FloatType && toType is IntType) ||
                (fromType is BoolType && toType is IntType))
            {
                expr.SetType(toType);
                return toType;
            }

            throw new Exception($"Cannot cast from {fromType} to {toType}");
        }

        private ExpressionNode InsertCast(ExpressionNode node, Type from, Type to)
        {
            if (from.GetType() == to.GetType())
                return node;

            // Allow Int -> Float
            if (from is IntType && to is FloatType)
                return new CastNode(node, from, to);

            // NEW: Allow the storage alignment casts
            if (from is FloatType && to is IntType)
                return new CastNode(node, from, to);

            if (from is BoolType && to is IntType)
                return new CastNode(node, from, to);

            throw new Exception($"Cannot cast {from} to {to}");
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

        public Type VisitIncrement(IncrementNode statement)
        {
            Visit(statement.Id);
            statement.SetType(new VoidType());
            return statement.Type;
        }

        public Type VisitDecrement(DecrementNode statement)
        {
            Visit(statement.Id);
            statement.SetType(new VoidType());
            return statement.Type;
        }

        public Type VisitAssign(AssignNode statement)
        {
            Type valType = Visit(statement.Expression);

            _context = _context.Add(statement.Id, valType);
            statement.SetType(new VoidType());
            return valType;
        }

        public Type VisitRandom(RandomNode expr)
        {
            if (expr.Arguments.Count < 2 || expr.Arguments.Count > 3)
                throw new Exception("random() expects 2 or 3 arguments");

            Type minType = Visit(expr.MinValue);
            Type maxType = Visit(expr.MaxValue);

            // If decimals argument exists, validate it
            if (expr.Decimals != null)
            {
                Type decType = Visit(expr.Decimals);

                if (!(decType is IntType || decType is FloatType))
                    throw new Exception("random() decimals must be numeric");
            }

            // If decimals is present → ALWAYS float
            if (expr.Decimals != null)
            {
                expr.SetType(new FloatType());
            }
            else
            {
                // Otherwise infer like before
                if (minType is FloatType || maxType is FloatType)
                    expr.SetType(new FloatType());
                else
                    expr.SetType(new IntType());
            }

            return expr.Type;
        }

        public Type VisitIf(IfNode statement)
        {
            var condType = Visit(statement.Condition);

            // 1. Condition Check
            if (condType is not BoolType)
                throw new Exception("If condition must be Bool: " + condType);

            Visit(statement.ThenPart);

            if (statement.ElsePart != null)
                Visit(statement.ElsePart);

            statement.SetType(new VoidType());
            return statement.Type;
        }

        public Type VisitPrint(PrintNode statement)
        {
            Visit(statement.Expression);
            statement.SetType(new VoidType());
            return statement.Type;
        }

        public Type VisitForLoop(ForLoopNode statement)
        {
            Visit(statement.Initialization);

            var condType = Visit(statement.Condition);

            // Optimization: Allow Float as condition (0.0 is false)
            if (condType is not BoolType && condType is not FloatType)
                throw new Exception("For loop condition must be Bool or Number");

            if (statement.Step is not IncrementNode && statement.Step is not DecrementNode)
                throw new Exception("For loop step must be increament or decrement");

            Visit(statement.Step);
            Visit(statement.Body);
            statement.SetType(new VoidType());
            return statement.Type;
        }

        public Type VisitSequence(SequenceNode node)
        {
            Type lastType = new VoidType();

            foreach (var stmt in node.Nodes)
                lastType = Visit(stmt);

            return lastType;
        }

        // Handle [1, 2, 3]
        public Type VisitArray(ArrayNode expr)
        {
            if (expr.Elements.Count > 0)
            {
                if (expr.ElementType is not null)
                {
                    if (expr.ElementType.GetType() != Visit(expr.Elements[0]).GetType())
                        throw new Exception("Array type does not match element type!");
                }
                else
                    expr.ElementType = Visit(expr.Elements[0]);

                for (int i = 1; i < expr.Elements.Count; i++)
                {
                    if (expr.ElementType is RecordType recordType)
                    {
                        RecordType record = Visit(expr.Elements[i]) as RecordType;
                        if (recordType.ToString() != record.ToString())
                            throw new Exception("Not all records have the same field names, which is not allowed in an array");
                    }

                    Type indexType = Visit(expr.Elements[i]);

                    if (indexType is ArrayType) continue;

                    if (expr.Elements[i] is TypeLiteralNode && expr.Elements[0] is TypeLiteralNode) continue;

                    if (expr.ElementType.GetType() != indexType.GetType())
                        throw new Exception("Not all elements are of the same type, which is not allowed in an array");
                }
            }
            else
            {
                if (expr.ElementType is null)
                    throw new Exception("Empty arrays need a type!");
            }

            var arrayType = new ArrayType(expr.ElementType);
            expr.SetType(arrayType);
            return expr.Type;
        }

        public Type VisitCopy(CopyNode expr)
        {
            Type sourceType = Visit(expr.SourceExpression);

            // Confirming it accepts Dataframes, Records, and Arrays smoothly now
            if (sourceType is not ArrayType && sourceType is not RecordType && sourceType is not DataframeType)
            {
                throw new Exception($"Copying is only supported on arrays, records, or dataframes, got {sourceType}");
            }

            expr.SetType(sourceType);
            return expr.Type;
        }

        // Handle arr[0]
        // Inside TypeChecker.cs
        public Type VisitIndex(IndexNode expr)
        {
            // 1. Visit children first to resolve their types down the tree
            Type sourceType = Visit(expr.SourceExpression);
            Type indexType = Visit(expr.IndexExpression);

            Type inferred = new IntType(); // Default fallback

            // 2. Drive type inference cleanly by looking at structural TYPE definitions, not node syntax shapes
            if (sourceType is ArrayType arrType)
            {
                if (indexType is not IntType)
                    throw new Exception($"Array index must be an integer, got {indexType}");

                inferred = arrType.ElementType;
            }
            else if (sourceType is DataframeType dfType)
            {
                if (indexType is not IntType)
                {
                    throw new Exception($"Dataframe indexing must be string (column) or int (row), got {indexType}");
                }
                else
                {
                    // Row access: arr[0] -> returns a single Record matching the schema row layout
                    inferred = dfType.RowType;
                }
            }
            else
            {
                throw new Exception($"Cannot apply indexing target expressions on a source of type '{sourceType}'");
            }

            expr.SetType(inferred);
            return expr.Type;
        }
        public Type VisitIndexAssign(IndexAssignNode expr)
        {
            // 1. Visit children to ensure their .Type properties are populated
            Type arrayOrDfType = Visit(expr.ArrayExpression);
            Visit(expr.IndexExpression);
            Type valueType = Visit(expr.AssignExpression);

            // 2. Determine the expected element type
            Type expectedType = null;

            if (arrayOrDfType is ArrayType arr)
            {
                expectedType = arr.ElementType;
            }
            else if (arrayOrDfType is DataframeType df)
            {
                // When assigning to a dataframe directly by index, we expect a Row (Record)
                expectedType = df.RowType;
            }
            else
            {
                throw new Exception($"Cannot perform index assignment on non-indexable type: {arrayOrDfType}");
            }

            // 3. Validate types (using your compiler's type equality, not C# GetType)
            // Assuming you have a way to check if types match
            if (expectedType.ToString() != valueType.ToString())
            {
                throw new Exception($"Type mismatch: Cannot assign {valueType} to {expectedType} element.");
            }

            expr.SetType(new VoidType());
            return expr.Type;
        }

        public Type VisitRound(RoundNode expr)
        {
            Visit(expr.Value);
            Type decType = Visit(expr.Decimals);

            if (!(decType is IntType || decType is FloatType))
                throw new Exception("round() decimals must be numeric");

            expr.SetType(new FloatType());
            return expr.Type;
        }

        public Type VisitFloat(FloatNode expr)
        {
            expr.SetType(new FloatType());
            return expr.Type;
        }

        private ExpressionNode ResolveDataType(Type type)
        {
            return type switch
            {
                StringType => new StringNode(""),
                BoolType => new BooleanNode(false),
                IntType => new NumberNode(0),
                FloatType => new FloatNode(0.0),
                _ => throw new Exception("Unsupported array element type")
            };
        }

        public Type VisitWhere(WhereNode expr)
        {
            // Visit source expression
            var sourceType = Visit(expr.SourceExpr);

            if (sourceType is not ArrayType && sourceType is not DataframeType)
                throw new Exception("where can only be used on arrays or dataframes");

            // --- Create iterator variable ---
            if (sourceType is ArrayType arrType)
            {
                ExpressionNode defaultVal = ResolveDataType(arrType.ElementType);

                Visit(new AssignNode(expr.IteratorId.Name, defaultVal));
            }
            else if (sourceType is DataframeType dfType)
            {
                // Create a RecordNode matching the row type including 'index'
                var rowFields = new List<FieldNode>();

                for (int i = 0; i < dfType.ColumnNames.Count; i++)
                {
                    var colName = dfType.ColumnNames[i];
                    var colType = dfType.DataTypes[i];

                    ExpressionNode defaultVal = ResolveDataType(colType);
                    rowFields.Add(new FieldNode(defaultVal, colName));
                }

                var rowRecord = new RecordNode(rowFields);
                Visit(new AssignNode(expr.IteratorId.Name, rowRecord));

                if (dfType.ColumnNames.Count != dfType.ColumnNames.Distinct().Count())
                    throw new Exception("Dataframe has duplicate column names");

                // --- Create empty result dataframe with same schema (no index duplication) ---
                var resultDf = new DataframeNode(new List<NamedArgumentNode>
                {
                    new NamedArgumentNode("columns",
                        new ArrayNode(dfType.ColumnNames
                            .Select(n => new StringNode(n) as ExpressionNode)
                            .ToList())
                    ),

                    new NamedArgumentNode("rows",
                        new ArrayNode(new List<ExpressionNode>())
                    ),

                    new NamedArgumentNode("type",
                        new ArrayNode(dfType.DataTypes
                            .Select(t => new StringNode(t.ToString()) as ExpressionNode)
                            .ToList())
                    )
                });
            }

            var condType = Visit(expr.Condition);
            if (condType is not BoolType)
                throw new Exception("where condition must return bool");

            // Set the type of the Where expression to match source
            expr.SetType(expr.SourceExpr.Type);
            return expr.Type;
        }

        public Type VisitMap(MapNode expr)
        {
            var sourceType = Visit(expr.SourceExpr);
            Type iteratorType;

            if (sourceType is DataframeType df)
                iteratorType = df.RowType;
            else if (sourceType is ArrayType arr)
                iteratorType = arr.ElementType;
            else
                throw new Exception($"Cannot map over type {sourceType}.");

            var previousContext = _context;
            // Inject iterator 'x' into scope
            _context = _context.Add(expr.IteratorId.Name, iteratorType);

            try
            {
                Type transformType = Visit(expr.TransformExpr);
                expr.TransformExpr.SetType(transformType);

                // Record => Dataframe
                if (transformType is RecordType rec)
                {
                    var resultType = new DataframeType(
                        rec.RecordFields.Select(f => f.Label).ToList(),
                        rec.RecordFields.Select(f => f.Type).ToList(),
                        rec
                    );

                    expr.SetType(resultType);
                    return resultType;
                }

                // Scalar => Array
                var arrayType = new ArrayType(transformType);
                expr.SetType(arrayType);
                return arrayType;
            }
            finally { _context = previousContext; }
        }

        // Helper: Try to infer the field name being assigned in a map operation (e.g. x.age - 10 => "age")
        private string InferFieldName(Node node)
        {
            if (node == null) return null;

            // Handle x.longitude = 100.0
            if (node is RecordFieldAssignNode rfan) return rfan.IdField;

            // Handle x.longitude
            if (node is FieldNode rf) return rf.IdField;

            // Handle x.longitude + 1
            if (node is BinaryOpNode bin)
                return InferFieldName(bin.Left) ?? InferFieldName(bin.Right);

            return null;
        }

        public Type VisitSqrt(SqrtNode expr)
        {
            var argType = Visit(expr.Value); // maybe always cast to float if it's an int for sqrt, log, pow, exp?

            if (!(argType is IntType || argType is FloatType))
            {
                throw new Exception($"sqrt() expected numeric type, got {argType}");
            }

            // Result is always a float
            expr.SetType(new FloatType());
            return expr.Type;
        }

        public Type VisitLog(LogNode expr)
        {
            var argType = Visit(expr.Value);
            if (!(argType is IntType || argType is FloatType))
            {
                throw new Exception($"log() expected numeric type, got {argType}");
            }

            expr.SetType(new FloatType());
            return expr.Type;
        }

        public Type VisitPow(PowNode expr)
        {
            var baseType = Visit(expr.Value);
            var exponentType = Visit(expr.Power);

            if (!(baseType is IntType || baseType is FloatType) || !(exponentType is IntType || exponentType is FloatType))
            {
                throw new Exception($"pow() expected numeric types, got {baseType} and {exponentType}");
            }

            expr.SetType(new FloatType());
            return expr.Type;
        }

        public Type VisitExponentialMathFunc(ExponentialMathFuncNode expr)
        {
            var argType = Visit(expr.Value);

            if (!(argType is IntType || argType is FloatType))
            {
                throw new Exception($"exponential() expected numeric type, got {argType}");
            }

            // Result is always a float
            expr.SetType(new FloatType());
            return expr.Type;
        }

        // Helper: Build a RecordNode from CSV (first line + type inference)
        public static RecordNode BuildRecordNodeFromCsv(string path)
        {
            using var reader = new StreamReader(path);
            var columnNames = reader.ReadLine().Split(',').Select(s => s.Trim()).ToArray();
            var dataLines = reader.ReadLine();

            if (columnNames.Length == 0 || dataLines.Length == 0)
                throw new Exception("File does not have any data");

            string[] firstRowParts = dataLines.Length > 0 ? dataLines.Split(',') : columnNames.Select(_ => "").ToArray();

            var fields = new List<RecordField>();
            for (int i = 0; i < columnNames.Length; i++)
            {
                char typeCode;
                string rawValue = firstRowParts[i].Trim();

                if (long.TryParse(rawValue, out _))
                    typeCode = 'I';
                else if (double.TryParse(rawValue, out _))
                    typeCode = 'F';
                else if (rawValue.Equals("true", StringComparison.OrdinalIgnoreCase) || rawValue.Equals("false", StringComparison.OrdinalIgnoreCase))
                    typeCode = 'B';
                else
                    typeCode = 'S';

                fields.Add(new RecordField
                {
                    Label = columnNames[i],
                    Value = typeCode switch
                    {
                        'I' => new NumberNode(0),
                        'F' => new FloatNode(0.0),
                        'B' => new BooleanNode(true),
                        'S' => new StringNode(""),
                        _ => new StringNode("")
                    },
                    Type = typeCode switch
                    {
                        'I' => new IntType(),
                        'F' => new FloatType(),
                        'B' => new BoolType(),
                        'S' => new StringType(),
                        _ => new StringType()
                    }
                });
            }

            var recordNode = new RecordNode(new List<FieldNode>()) { Fields = fields };

            return recordNode;
        }

        public Type VisitReadCsv(ReadCsvNode expr)
        {
            Visit(expr.FileNameExpr);

            if (expr.SchemaExpr == null)
            {
                if (_debug) Console.WriteLine("Inferring schema from CSV file: " + (expr.FileNameExpr as StringNode)?.Value);
                string path = (expr.FileNameExpr as StringNode).Value;
                expr.SchemaExpr = new NamedArgumentNode("schema", BuildRecordNodeFromCsv(path));
            }
            else if (expr.SchemaExpr.Name != "schema")
                throw new Exception("read_csv requires a 'schema' named argument");

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
                return expr.Type;
            }

            throw new Exception($"read_csv requires a record template, but got {schemaType?.GetType().Name}");
        }

        public Type VisitToCsv(ToCsvNode expr)
        {
            // 1. Get the types of the arguments
            Type exprType = Visit(expr.Expression);
            Type pathType = Visit(expr.FileNameExpr);

            // 2. Semantic Check: Is the first argument actually a Dataframe?
            if (exprType is not DataframeType)
                throw new Exception($"to_csv() error: First argument must be a Dataframe, but got {exprType?.GetType().Name}");

            // 3. Semantic Check: Is the second argument a String?
            if (pathType is not StringType)
                throw new Exception($"to_csv() error: Second argument must be a String (file path), but got {pathType?.GetType().Name}");

            // 4. Set the return type to Void/None
            // (Ensure this matches whatever type your REPL uses for 'null' results)
            expr.SetType(new VoidType());
            return expr.Type;
        }

        public Type VisitAdd(AddNode expr)
        {
            var sourceType = Visit(expr.SourceExpression);
            var addType = Visit(expr.AddExpression);

            if (sourceType is DataframeType dfType)
            {
                if (addType is not RecordType recType)
                    throw new Exception("Add can only add records to dataframes");

                // Check if the record fields match the dataframe columns
                var dfColumns = dfType.ColumnNames;
                var recFields = recType.RecordFields.Select(f => f.Label).ToArray();
                var recTypes = recType.RecordFields.Select(f => f.Value?.Type ?? f.Type).ToArray();

                if (dfColumns.Count != recType.RecordFields.Count)
                    throw new Exception("Record fields count must match the dataframe columns count");

                for (int t = 0; t < dfType.DataTypes.Count; t++)
                {
                    if (dfType.DataTypes[t].GetType() != recTypes[t].GetType())
                        throw new Exception($"Dataframe column '{dfType.ColumnNames[t]}' expects type {dfType.DataTypes[t]}, but record provides {recTypes[t]}");
                }

                // if (!dfColumns.All(col => recFields.Contains(col)))
                //      throw new Exception("Record fields do not match dataframe columns");

            }
            else if (sourceType is ArrayType arrType)
            {
                var arrayElementType = ((ArrayType)sourceType).ElementType;

                if (arrayElementType.GetType() != addType.GetType())
                    throw new Exception($"Can't add {addType} value to a {arrayElementType} array");
            }
            else
                throw new Exception("Add can only be used on arrays and dataframes");

            expr.SetType(sourceType);
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
            if (arrayElementType is not (IntType or FloatType))
                throw new Exception($"Can't find valid aggregation value for {arrayElementType.GetType().Name}");
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

        public Type VisitCorrelation(CorrelationNode expr)
        {
            var arrayElementType1 = (Visit(expr.SourceExpression) as ArrayType).ElementType;
            var arrayElementType2 = (Visit(expr.TargetExpression) as ArrayType).ElementType;

            ValidType(arrayElementType1);
            ValidType(arrayElementType2);

            expr.SetType(new FloatType());
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
                // This MUST return a valid Type (e.g., IntType from 'int' or '25')
                field.Type = Visit(field.Value);

                if (field.Type == null)
                    throw new Exception($"Could not resolve type for record field '{field.Label}'");

                field.Value.SetType(field.Type);
            }

            var recordType = new RecordType(expr.Fields);
            expr.SetType(recordType);
            return expr.Type;
        }

        public Type VisitField(FieldNode expr)
        {
            // 1. Visit the record source (could be an Id, an Index df[2], a Function call, etc.)
            Type SourceType = Visit(expr.SourceExpression);

            if (_debug) Console.WriteLine("Id record: " + expr.IdField + " type: " + expr.SourceExpression.Type);

            // 2. Initialize a default (or null)
            Type resolvedFieldType = null;

            // 3. Check if the source actually resolved to a RecordType
            if (SourceType is RecordType recType)
            {
                // 4. Look up the field label in the record definition
                var field = recType.RecordFields.FirstOrDefault(f => f.Label == expr.IdField);
                if (field != null) // Use the stored type from the field
                    resolvedFieldType = field.Value?.Type ?? field.Type;
                else
                    throw new Exception($"Field '{expr.IdField}' not found in record.");
            }
            else if (SourceType is DataframeType dfType)
            {
                switch (expr.IdField)
                {
                    case "columns":
                        resolvedFieldType = new ArrayType(new StringType());
                        break;
                    case "schema":
                        resolvedFieldType = new RecordType(dfType.RowType.RecordFields
                            .Select(f => new RecordField { Label = f.Label, Type = new StringType() })
                            .ToList());
                        break;
                    case "types":
                        resolvedFieldType = new ArrayType(new IntType());
                        break;
                    case "rows":
                        resolvedFieldType = new ArrayType(dfType.RowType);
                        break;
                    default:
                        {
                            int idx = dfType.ColumnNames.ToList().IndexOf(expr.IdField);
                            if (idx >= 0)
                                resolvedFieldType = new ArrayType(dfType.DataTypes[idx]);
                            else
                                throw new Exception($"Column '{expr.IdField}' not found in dataframe.");
                            break;
                        }
                }
            }
            else
                throw new Exception($"Cannot access field '{expr.IdField}' on non-record or dataframe type: {SourceType}");

            if (_debug) Console.WriteLine($"Resolved field {expr.IdField} to type: {resolvedFieldType}");

            expr.SetType(resolvedFieldType);
            return expr.Type;
        }

        public Type VisitRecordFieldAssign(RecordFieldAssignNode statement)
        {
            Type assignType = Visit(statement.AssignExpression);
            Type idType = Visit(statement.IdRecord);
            var fieldType = (idType as RecordType).RecordFields.FirstOrDefault(f => f.Label == statement.IdField).Type.GetType();

            if (assignType.GetType() != fieldType)
                throw new Exception($"Wrong assign type {assignType}: Expected {fieldType}");

            statement.SetType(new VoidType());
            return statement.Type;
        }

        public static Type ResolveTypeNode(TypeNode typeNode)
        {
            if (typeNode == null) return null;
            return typeNode.Name switch
            {
                "int" => new IntType(),
                "float" => new FloatType(),
                "bool" => new BoolType(),
                "string" => new StringType(),
                "array" => new ArrayType(null),
                _ => throw new Exception($"Unknown type node '{typeNode.Name}'")
            };
        }


        public Type VisitDataframe(DataframeNode expr)
        {
            DataframeType dataframe = BuildDataframeType(expr);

            if (expr.Types == null)
                throw new Exception("Dataframe must contain types.");

            System.Console.WriteLine("we did build dataframe");
            Visit(expr.Columns);
            System.Console.WriteLine("we visited columns");
            Visit(expr.Rows);
            System.Console.WriteLine("we visited rows");
            Visit(expr.Types);
            System.Console.WriteLine("we visited types");
            expr.SetType(dataframe);

            return expr.Type;
        }

        private DataframeType BuildDataframeType(DataframeNode df)
        {
            System.Console.WriteLine("we building dataframe type");
            var columns = ExtractColumns(df);

            Type inferredType;

            // CASE 1: no data → schema-only dataframe
            if (df.Rows == null)
            {
                System.Console.WriteLine("and we don't have data");
                if (df.Types == null)
                    throw new Exception("Empty dataframe requires explicit types");

                var types = ExtractTypes(df);

                var dataArr2 = df.Rows as ArrayNode;

                bool isEmpty = dataArr2 == null || dataArr2.Elements.Count == 0;

                if (isEmpty)
                {
                    System.Console.WriteLine("yo we in here and setting the array element type");
                    // Build columnar empty data
                    df.Rows = new ArrayNode(
                        types.Select(t =>
                            (ExpressionNode)new ArrayNode(new List<ExpressionNode>())
                            {
                                ElementType = t
                            }
                        ).ToList()
                    );

                    return new DataframeType(
                        columns,
                        types,
                        BuildRecordType(columns, types)
                    );
                }

                inferredType = BuildRecordType(columns, types);

                return new DataframeType(columns, types, inferredType as RecordType);
            }

            var dataArr = df.Rows as ArrayNode
                ?? throw new Exception("rows must be an array");

            // CASE 2: columnar dataframe
            bool isColumnar = dataArr.Elements.All(e => e is IdNode);

            if (isColumnar)
            {
                var types = ExtractTypes(df);
                inferredType = BuildRecordType(columns, types);

                return new DataframeType(columns, types, inferredType as RecordType);
            }

            // CASE 3: row dataframe
            var data = ExtractData(df);
            var inferredTypes = InferTypes(data);

            inferredType = BuildRecordType(columns, inferredTypes);


            foreach (var item in inferredTypes)
            {
                System.Console.WriteLine("inferred type: " + item);
            }

            df.Types = new ArrayNode(
                inferredTypes.Select(TypeToNumberNode).ToList()
            );

            return new DataframeType(columns, inferredTypes, inferredType as RecordType);
        }

        private RecordType BuildRecordType(List<string> columns, List<Type> types)
        {
            var fields = new List<RecordField>();

            for (int i = 0; i < columns.Count; i++)
            {
                fields.Add(new RecordField
                {
                    Label = columns[i],
                    Type = types[i],
                    Value = default
                });
            }

            return new RecordType(fields);
        }

        private Type Infer(object value)
        {
            return value switch
            {
                int => new IntType(),
                double => new FloatType(),
                float => new FloatType(), // HACK, it should never be a float, always a double
                string => new StringType(),
                bool => new BoolType(),
                _ => throw new Exception("Unsupported type for inferenceX:" + value.GetType().Name)
            };
        }

        private ExpressionNode TypeToNumberNode(Type type)
        {
             return type switch
            {
                IntType => new NumberNode(1),
                FloatType => new NumberNode(2),
                BoolType => new NumberNode(3),
                StringType => new NumberNode(4),
                _ => throw new Exception("Unsupported type for inference for the type: " + type)
            };
        }

        private ExpressionNode TypeToNode(Type type)
        {
            return type switch
            {
                IntType => new NumberNode(0),
                FloatType => new FloatNode(0),
                BoolType => new BooleanNode(false),
                StringType => new StringNode(""),
                _ => throw new Exception("Unsupported type for inference for the type: " + type)
            };
        }

        private List<string> ExtractColumns(DataframeNode df)
        {
            var arr = df.Columns as RecordNode
                ?? throw new Exception("columns must be an array");

            return arr.Fields.Select(e =>
            {
                return e.Label;
            }).ToList();
        }

        private List<List<object>> ExtractData(DataframeNode df)
        {
            var arr = df.Rows as ArrayNode
                ?? throw new Exception("data must be an array");

            return arr.Elements.Select(row =>
            {
                if (row is not RecordNode rowArr)
                    throw new Exception("each row must be an array" + row.GetType().Name);

                return rowArr.Fields.Select(f => f.Value as object).ToList();

            }).ToList();
        }
        private object ValueOf(ExpressionNode node)
        {
            return node switch
            {
                NumberNode n => n.Value,
                FloatNode f => f.Value,
                StringNode s => s.Value,
                BooleanNode b => b.Value,
                _ => throw new Exception($"Unsupported value type: {node.GetType().Name}")
            };
        }
        private List<Type> ExtractTypes(DataframeNode df)
        {
            var arr = df.Rows as ArrayNode
                ?? throw new Exception("type must be an array");

            return arr.Elements.Select(node =>
            {
                if (node is TypeLiteralNode t)
                    return InferFromString(t.TypeNode.Name); // or t.Type

                return InferTypeFromNode(node);

                throw new Exception("types must be type literals");
            }).ToList();
        }
        private List<Type> InferTypes(List<List<object>> data)
        {
            if (data.Count == 0)
                throw new Exception("Cannot infer types from empty data");

            return data[0].Select(x => InferTypeFromNode(x as Node)).ToList();
        }

        private Type InferFromString(string value)
        {
            return value switch
            {
                "int" => new IntType(),
                "double" => new FloatType(),
                "float" => new FloatType(), // HACK, it should never be a float, always a double
                "string" => new StringType(),
                "bool" => new BoolType(),
                _ => throw new Exception("Unsupported type for inference: " + value)
            };
        }

        private Type InferTypeFromNode(Node value)
        {
            return value switch
            {
                NumberNode => new IntType(),
                FloatNode => new FloatType(),
                StringNode => new StringType(),
                BooleanNode => new BoolType(),
                _ => throw new Exception("Unsupported type for inference" + value.GetType().Name)
            };
        }

        private Type ResolveType(ExpressionNode expr) // FIX, might be redundant
        {
            if (expr.Type is Type type)
                return type;

            if (expr is TypeNode typeNod)
            {
                return typeNod.Name switch
                {
                    "int" => new IntType(),
                    "float" => new FloatType(),
                    "string" => new StringType(),
                    "bool" => new BoolType(),
                    _ => throw new Exception($"Unknown type '{typeNod.Name}'")
                };
            }

            if (expr is TypeLiteralNode typeLit)
            {
                return typeLit.TypeNode.Name switch
                {
                    "int" => new IntType(),
                    "float" => new FloatType(),
                    "bool" => new BoolType(),
                    "string" => new StringType(),
                    "dataframe" => new DataframeType(new List<string>(), new List<Type>(), new RecordType(new List<RecordField>())),
                    _ => throw new Exception($"Unknown type '{typeLit.TypeNode.Name}'")
                };
            }

            throw new Exception("Expected type node." + expr.Type);
        }

        public Type VisitNamedArgument(NamedArgumentNode expr)
        {
            // If expr.Value is a StringNode ("Bob"), Visit returns StringType
            Type valType = Visit(expr.Value);

            // Save the type on the NamedArgument node itself
            expr.SetType(valType);
            return expr.Type;
        }

        public Type VisitColumns(ColumnsNode expr)
        {
            Visit(expr.DataframeExpression);
            expr.SetType(new ArrayType(new StringType()));
            return expr.Type;
        }
        public Type VisitTypeLiteral(TypeLiteralNode expr)
        {
            return ResolveType(expr); // Just return the wrapped type
        }

        public Type VisitSlice(SliceNode expr)
        {
            Type sourceType = Visit(expr.Source);

            // Support both Arrays and Dataframes
            if (sourceType is not ArrayType && sourceType is not DataframeType)
                throw new Exception($"Slicing is only supported on arrays or dataframes, got {sourceType}");

            if (expr.Start != null)
            {
                if (Visit(expr.Start) is not IntType)
                    throw new Exception("Slice start index must be an integer");
            }

            if (expr.End != null)
            {
                if (Visit(expr.End) is not IntType)
                    throw new Exception("Slice end index must be an integer");
            }

            expr.SetType(sourceType); // df[start:end] returns a DataframeType
            return expr.Type;
        }
    }
}