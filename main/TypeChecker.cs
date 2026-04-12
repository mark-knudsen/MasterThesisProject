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
                NamedArgumentNode namedArg => VisitNamedArgument(namedArg),
                TypeLiteralNode typeLit => VisitTypeLiteral(typeLit),

                _ => throw new NotSupportedException($"Type check not implemented for {node.GetType().Name}")
            };
        }

        public Type VisitForEachLoop(ForEachLoopNode expr)
        {
            var collectionType = Visit(expr.Source);
            RecordType rowType = null;

            // Determine the type of 'item'
            if (collectionType is DataframeType df) rowType = df.RowType;
            else if (collectionType is ArrayType arr && arr.ElementType is RecordType rec) rowType = rec;

            if (rowType == null) throw new Exception("ForEach requires a Dataframe or Array of Records");

            //_context = _context.Add(expr.Iterator.Name, null, rowType);
            var testEntry = _context.Get(expr.Iterator.Name);
            Console.WriteLine($"[DEBUG] Internal Check: Found '{expr.Iterator.Name}'? " + (testEntry != null));

            //_context = _context.Add(expr.Iterator.Name, null, rowType);
            //var testEntry = _context.Get(expr.Iterator.Name);
            //if (_debug) Console.WriteLine($"[DEBUG] Internal Check: Found '{expr.Iterator.Name}'? " + (testEntry != null));

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
            var entry = _context.Get(expr.Name);

            if (entry == null)
                throw new Exception($"type check - Undefined variable '{expr.Name}'");

            expr.SetType(entry.Type);
            return entry.Type;
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

            expr.SetType(entry.Type);
            return expr.Type;
        }

        public Type VisitDecrement(DecrementNode expr)
        {
            var entry = _context.Get(expr.Id);
            if (entry == null) throw new Exception($"Variable {expr.Id} not defined");

            expr.SetType(entry.Type);
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
            Visit(expr.MinValue);
            Visit(expr.MaxValue);

            expr.SetType(new IntType());
            return expr.Type;
        }

        public Type VisitIf(IfNode expr)
        {
            var condType = Visit(expr.Condition);

            // 1. Condition Check
            if (condType is not BoolType)
                throw new Exception("If condition must be Bool: " + condType);

            // Use .Then and .Else to match your Node definition
            Type thenType = Visit(expr.ThenPart);

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
            return expr.Type;
        }

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

        // Handle [1, 2, 3]
        public Type VisitArray(ArrayNode expr)
        {
            if (expr.Elements.Count > 0)
                expr.ElementType = Visit(expr.Elements[0]);

            var arrayType = new ArrayType(expr.ElementType);
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
            // 1. Visit children first to resolve their types
            Type sourceType = Visit(expr.SourceExpression);
            Visit(expr.IndexExpression);

            Type inferred = new IntType(); // Default

            if (sourceType is ArrayType arrType)
            {
                inferred = arrType.ElementType;
            }
            else if (sourceType is DataframeType dfType)
            {
                // Use the RowType we carefully built in VisitDataframe
                inferred = dfType.RowType;
            }

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

        public ExpressionNode ResolveDataType(Type type)
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
                var rowFields = new List<NamedArgumentNode>();

                for (int i = 0; i < dfType.ColumnNames.Count; i++)
                {
                    var colName = dfType.ColumnNames[i];
                    var colType = dfType.DataTypes[i];

                    ExpressionNode defaultVal = ResolveDataType(colType);
                    rowFields.Add(new NamedArgumentNode(colName, defaultVal));
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
            Type elementType;

            // 1. Determine what the iterator 'x' represents
            if (sourceType is ArrayType arrayType)
                elementType = arrayType.ElementType;
            else if (sourceType is DataframeType dfType)
                elementType = dfType.RowType;
            else
                throw new Exception("Map source must be an array or a dataframe.");

            var previousContext = _context;

            // 2. Inject 'x' into the context with the correct type (Primitive or Record)
            _context = _context.Add(expr.IteratorId.Name, default, null!, elementType);

            try // there should not be an empty try catch in the type checker
            {
                // Visit the iterator ID to ensure it's registered
                Visit(expr.IteratorId);

                // 3. Visit the transformation (the lambda body)
                // If x is a Record, x.name will now resolve correctly because x has RowType
                var bodyType = Visit(expr.Assignment);

                // 4. The result of a Map is always an Array of whatever the body returns
                //var resultType = new ArrayType(bodyType);


                Type resultType;

                // NEW LOGIC HERE:
                // If the map transformation results in a Record, we produce a Dataframe.
                // Otherwise (like mapping to a list of strings), we produce an Array.
                if (bodyType is RecordType recType)
                {
                    // Create a Dataframe type based on the record's schema
                    resultType = sourceType;
                }
                else
                {
                    resultType = new ArrayType(bodyType);
                }

                expr.SetType(resultType);
                return resultType;
            }
            finally
            {
                _context = previousContext;
            }
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
                else if (double.TryParse(rawValue, System.Globalization.CultureInfo.InvariantCulture, out _))
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

            var recordNode = new RecordNode(new List<NamedArgumentNode>()) { Fields = fields };

            return recordNode;
        }

        public Type VisitReadCsv(ReadCsvNode expr)
        {
            Visit(expr.FileNameExpr);

            if (expr.SchemaExpr == null)
            {
                string path = (expr.FileNameExpr as StringNode).Value;
                expr.SchemaExpr = BuildRecordNodeFromCsv(path);
            }

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

            throw new Exception($"read_csv requires a record template, but got {schemaType?.GetType().Name}");
        }

        public Type VisitToCsv(ToCsvNode expr)
        {
            // 1. Get the types of the arguments
            Type exprType = Visit(expr.Expression);
            Type pathType = Visit(expr.FileNameExpr);

            // 2. Semantic Check: Is the first argument actually a Dataframe?
            if (exprType is not DataframeType)
            {
                throw new Exception($"to_csv() error: First argument must be a Dataframe, but got {exprType?.GetType().Name}");
            }

            // 3. Semantic Check: Is the second argument a String?
            if (pathType is not StringType)
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

            if (arrayType is DataframeType dfType)
            {
                if (addType is not RecordType recType)
                    throw new Exception("Add can only add records to dataframes");

                // Check if the record fields match the dataframe columns
                var dfColumns = dfType.ColumnNames;
                var recFields = recType.RecordFields.Select(f => f.Label).ToArray();

                if (dfColumns.Count != recType.RecordFields.Count)
                    throw new Exception("Record fields count must match the dataframe columns count");

                if (!dfColumns.All(col => recFields.Contains(col)))
                    throw new Exception("Record fields do not match dataframe columns");
            }
            else if (arrayType is ArrayType arrType)
            {
                var arrayElementType = ((ArrayType)arrayType).ElementType;

                if (arrayElementType.GetType() != addType.GetType())
                    throw new Exception($"Can't add {addType} value to a {arrayElementType} array");
            }
            else
                throw new Exception("Add can only be used on arrays and dataframes");

            expr.SetType(new VoidType());
            return expr.Type;
        }

        public Type VisitAddRange(AddRangeNode expr)
        {
            Visit(expr.SourceExpression);
            Visit(expr.AddRangeExpression);

            if (expr.AddRangeExpression is ArrayNode arrayNode)
            {
                foreach (var item in arrayNode.Elements)
                {
                    if ((item.Type as RecordType).RecordFields.Count != (expr.SourceExpression as ArrayNode).Elements.Count)
                        throw new Exception("Not all records has fields equal to the amount of columns in the dataframe");
                }
            }

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

        public Type VisitRecordField(RecordFieldNode expr)
        {
            // 1. Visit the record source (could be an Id, an Index df[2], a Function call, etc.)
            Type recordSourceType = Visit(expr.IdRecord);

            if (_debug) Console.WriteLine("idrecord: " + expr.IdField + " type: " + expr.IdRecord.Type);

            // 2. Initialize a default (or null)
            Type resolvedFieldType = null;

            // 3. Check if the source actually resolved to a RecordType
            if (recordSourceType is RecordType recType)
            {
                // 4. Look up the field label in the record definition
                var field = recType.RecordFields.FirstOrDefault(f => f.Label == expr.IdField);
                if (field != null)
                {
                    // Use the stored type from the field
                    resolvedFieldType = field.Value?.Type ?? field.Type;
                }
                else
                    throw new Exception($"Field '{expr.IdField}' not found in record.");
            }
            else
                throw new Exception($"Cannot access field '{expr.IdField}' on non-record type: {recordSourceType}");

            if (_debug) Console.WriteLine($"Resolved field {expr.IdField} to type: {resolvedFieldType}");

            expr.SetType(resolvedFieldType);
            return resolvedFieldType;
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
            // List<Type> columnTypes;
            RecordType rowType = new RecordType(new List<RecordField>());
            // 1. Extract and Normalize Columns
            var columnNames = expr.Columns.Elements.OfType<StringNode>().Select(c => c.Value).ToArray();

            // 3. Now determine types based on the ALREADY MODIFIED columns
            List<Type> columnTypes = new List<Type>();
            if (expr.Rows != null && expr.Rows.Elements.Count > 0)
            {
                var inferredRowType = Visit(expr.Rows.Elements[0]) as RecordType;
                foreach (var name in columnNames)
                {
                    var field = inferredRowType.RecordFields.FirstOrDefault(f => f.Label == name);
                    columnTypes.Add(field?.Type ?? new IntType()); // Default index to Int
                }
                rowType = inferredRowType;
            }
            else
            {
                // Explicit types for empty dataframe
                if (expr.DataTypes == null) throw new Exception("Empty dataframe requires 'types'.");

                Visit(expr.DataTypes);

                columnTypes = expr.DataTypes.Elements.Select(e => ResolveType(e)).ToList();

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

            return valType;
        }

        int IndexOf(IReadOnlyList<string> list, string value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == value)
                    return i;
            }
            return -1;
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
                    int idx = IndexOf(sourceType.ColumnNames, name);
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
                    throw new Exception("Show columns must be string literals (e.g., \"name\")");
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
            return ResolveType(expr); // Just return the wrapped type
        }
    }
}