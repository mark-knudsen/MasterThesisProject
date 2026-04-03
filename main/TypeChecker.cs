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

        public Type Check(NodeExpr node)
        {
            if (_debug) Console.WriteLine("we type checking");
            return Visit(node);
        }

        private Type Visit(NodeExpr node)
        {
            var name = node.GetType().Name;
            if (_debug) Console.WriteLine("visiting: " + name.Substring(0, name.Length - 8));
            return node switch // it says the last numbers node is null
            {
                NumberNodeExpr n => VisitNumber(n),
                StringNodeExpr str => VisitString(str),
                BooleanNodeExpr b => VisitBoolean(b),
                NullNodeExpr nul => VisitNull(nul),
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
                //CopyArrayNodeExpr clo => VisitCopyArray(clo),
                IndexNodeExpr idx => VisitIndex(idx),
                IndexAssignNodeExpr idxa => VisitIndexAssign(idxa),
                WhereNodeExpr whe => VisitWhere(whe),
                MapNodeExpr map => VisitMap(map),
                ReadCsvNodeExpr read => VisitReadCsv(read),

                ToCsvNodeExpr tocsv => VisitToCsv(tocsv),
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
                RecordNodeExpr rec => VisitRecord(rec),
                RecordFieldNodeExpr recf => VisitRecordField(recf),
                RecordFieldAssignNodeExpr reca => VisitRecordFieldAssign(reca),
                CopyRecordNodeExpr copr => VisitCopyRecord(copr),
                CopyNodeExpr cop => VisitCopy(cop),
                AddFieldNodeExpr radd => VisitAddField(radd),
                RemoveFieldNodeExpr rrem => VisitRemoveField(rrem),
                DataframeNodeExpr df => VisitDataframe(df),
                ColumnsNodeExpr cols => VisitColumns(cols),
                ShowDataframeNodeExpr showdf => VisitShowDataframe(showdf),
                NamedArgumentNodeExpr namedArg => VisitNamedArgument(namedArg),
                TypeLiteralNodeExpr typeLit => VisitTypeLiteral(typeLit),

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
            return expr.Type;
        }
        public Type VisitString(StringNodeExpr expr)
        {
            expr.SetType(new StringType());
            return expr.Type;
        }

        public Type VisitBoolean(BooleanNodeExpr expr)
        {
            expr.SetType(new BoolType());
            return expr.Type;
        }

        public Type VisitNull(NullNodeExpr expr)
        {
            expr.SetType(new NullType());
            return expr.Type;
        }

        public Type VisitId(IdNodeExpr expr)
        {
            var entry = _context.Get(expr.Name);
            // if(_debug) Console.WriteLine(" we looking at id: " + expr.Name + " and its type is " + entry?.Type);
            // if(_debug) Console.WriteLine("id type: " + entry.Type); // prof we can get records type at type check time

            if (entry == null)
                throw new Exception($"type check - Undefined variable '{expr.Name}'");

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
                return expr.Type;
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

        public Type VisitIncrement(IncrementNodeExpr expr)
        {
            var entry = _context.Get(expr.Id);
            if (entry == null) throw new Exception($"Variable {expr.Id} not defined");

            // FIX: Change entry.Value.Type to entry.Type
            var type = entry.Type;

            expr.SetType(type);
            return expr.Type;
        }

        public Type VisitDecrement(DecrementNodeExpr expr)
        {
            var entry = _context.Get(expr.Id);
            if (entry == null) throw new Exception($"Variable {expr.Id} not defined");

            // FIX: Change entry.Value.Type to entry.Type
            var type = entry.Type;

            expr.SetType(type);
            return expr.Type;
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
            expr.SetType(valType); // <--- ADD THIS LINE
            return valType;
        }

        public Type VisitRandom(RandomNodeExpr expr)
        {
            // var valueTypeMin = Visit(expr.MinValue); // I don't think visiting these does anything
            // var valueTypeMax = Visit(expr.MaxValue);

            expr.SetType(new IntType());
            return expr.Type;
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

            Console.WriteLine("then typee:" + thenType);

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

        public Type VisitCopyArray(CopyArrayNodeExpr expr)
        {
            var sourceArray = Visit(expr.Source);
            expr.SetType(sourceArray as ArrayType);
            return expr.Type;
        }

        public Type VisitCopy(CopyNodeExpr expr)
        {
            Visit(expr.Source);
            expr.SetType(expr.Source.Type);
            return expr.Type;
        }

        // Handle arr[0]
        // Inside TypeChecker.cs
        public Type VisitIndex(IndexNodeExpr expr)
        {
            Visit(expr.SourceExpression);
            Visit(expr.IndexExpression);

            Type inferred = new IntType();
            if (expr.SourceExpression is IdNodeExpr idNode) // In the parser whe instantiate the index with an IdNode, the rest of the code should never be true
            {
                var entry = _context.Get(idNode.Name);
                if (entry?.Type is ArrayType arrType)
                    inferred = entry.ElementType ?? arrType.ElementType ?? new IntType();
                else if (entry?.Type is DataframeType dfType)
                    inferred = dfType.RowType;
            }
            // else if (expr.SourceExpression is ArrayNodeExpr arrayLiteral)
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

        public Type VisitIndexAssign(IndexAssignNodeExpr expr)
        {
            Visit(expr.ArrayExpression);
            Visit(expr.IndexExpression);
            Visit(expr.AssignExpression);

            if (expr.ArrayExpression is IdNodeExpr idNode)
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
            return expr.Type;
        }

        public Type VisitRound(RoundNodeExpr expr)
        {
            Visit(expr.Value);
            expr.SetType(new FloatType());
            return expr.Type;
        }

        public Type VisitFloat(FloatNodeExpr expr)
        {
            expr.SetType(new FloatType());
            return expr.Type;
        }

        public Type VisitWhere(WhereNodeExpr expr)
        {
            var arrayType = Visit(expr.SourceExpr);

            Console.WriteLine("we got source: " + expr.SourceExpr.Type);

            // we need to assign the lambda value and give it the specific type

            if (arrayType is ArrayType arrType)
            {
                if (arrType.ElementType is StringType)
                    Visit(new AssignNodeExpr(expr.IteratorId.Name, new StringNodeExpr("")));
                else if (arrType.ElementType is BoolType)
                    Visit(new AssignNodeExpr(expr.IteratorId.Name, new BooleanNodeExpr(false)));
                else
                    Visit(new AssignNodeExpr(expr.IteratorId.Name, new NumberNodeExpr(0)));
            }
            else if (arrayType is DataframeType dfType)
            {
                var q = new List<NamedArgumentNodeExpr>();
                for (int i = 0; i < dfType.ColumnNames.Count; i++)
                {
                    if (dfType.DataTypes[i] is StringType)
                        q.Add(new NamedArgumentNodeExpr(dfType.ColumnNames[i], new StringNodeExpr("")));
                    else if (dfType.DataTypes[i] is BoolType)
                        q.Add(new NamedArgumentNodeExpr(dfType.ColumnNames[i], new BooleanNodeExpr(false)));
                    else
                        q.Add(new NamedArgumentNodeExpr(dfType.ColumnNames[i], new NumberNodeExpr(0)));
                }

                var d = new RecordNodeExpr(q);

                Visit(new AssignNodeExpr(expr.IteratorId.Name, d));
                Console.WriteLine(" we got data frame, now to get the records and assign to iterator");
            }

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

        public Type VisitMap(MapNodeExpr expr)
        {
            // if(_debug) Console.WriteLine("The array node in map: " + expr.SourceExpr);
            Visit(new AssignNodeExpr(expr.IteratorId.Name, new NumberNodeExpr(0)));

            Visit(expr.IteratorId);
            Visit(expr.Assignment);
            var arrayType = Visit(expr.SourceExpr);

            if (arrayType is not ArrayType && arrayType is not DataframeType)
                throw new Exception("map can only be used on arrays");

            // 2. Determine element type (adjust depending on your language)
            expr.SetType(expr.SourceExpr.Type);
            return expr.Type;
        }
        public Type VisitReadCsv(ReadCsvNodeExpr expr)
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


        public Type VisitToCsv(ToCsvNodeExpr expr)
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


        public Type VisitAdd(AddNodeExpr expr)
        {
            var arrayType = Visit(expr.SourceExpression);
            var addType = Visit(expr.AddExpression);

            // check if the array and the node are of the same type

            if (arrayType is DataframeType dfType)
            {
                if (addType is not RecordType recType)
                    throw new Exception("add can only add records to dataframes");

                // Check if the record fields match the dataframe columns
                var dfColumns = dfType.ColumnNames;
                var recFields = recType.RecordFields.Select(f => f.Label).ToList();

                if (_debug) Console.WriteLine("df columns: " + string.Join(", ", dfColumns));
                if (_debug) Console.WriteLine("rec fields: " + string.Join(", ", recFields));

                //recType.RecordFields.Insert(0, new RecordField("index", new NumberNodeExpr(0)) { Type = new IntType() });

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
                throw new Exception("add can only be used on arrays and dataframes");

            expr.SetType(expr.SourceExpression.Type);
            return expr.Type;
        }

        public Type VisitAddRange(AddRangeNodeExpr expr)
        {
            Visit(expr.SourceExpression);
            Visit(expr.AddRangeExpression);
            expr.SetType(expr.SourceExpression.Type);
            return expr.Type;
        }

        public Type VisitRemove(RemoveNodeExpr expr)
        {
            Visit(expr.SourceExpression);
            Visit(expr.RemoveExpression);
            expr.SetType(expr.SourceExpression.Type);
            return expr.Type;
        }

        public Type VisitRemoveRange(RemoveRangeNodeExpr expr)
        {
            Visit(expr.SourceExpression);
            Visit(expr.RemoveRangeExpression);
            expr.SetType(expr.SourceExpression.Type);
            return expr.Type;
        }

        public Type VisitLength(LengthNodeExpr expr)
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

        public Type VisitMin(MinNodeExpr expr)
        {
            var arrayElementType = (Visit(expr.ArrayExpression) as ArrayType).ElementType;
            ValidType(arrayElementType);

            expr.SetType(arrayElementType);
            return expr.Type;
        }

        public Type VisitMax(MaxNodeExpr expr)
        {
            var arrayElementType = (Visit(expr.ArrayExpression) as ArrayType).ElementType;
            ValidType(arrayElementType);

            expr.SetType(arrayElementType);
            return expr.Type;
        }

        public Type VisitMean(MeanNodeExpr expr)
        {
            var arrayElementType = (Visit(expr.ArrayExpression) as ArrayType).ElementType;
            ValidType(arrayElementType);

            expr.SetType(new FloatType());
            return expr.Type;
        }

        public Type VisitSum(SumNodeExpr expr)
        {
            var arrayElementType = (Visit(expr.ArrayExpression) as ArrayType).ElementType;
            ValidType(arrayElementType);

            expr.SetType(arrayElementType);
            return expr.Type;
        }

        public Type VisitUnaryOp(UnaryOpNodeExpr expr)
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

        public Type VisitRecord(RecordNodeExpr expr)
        {
            foreach (var field in expr.Fields)
            {
                // 1. Visit the node (NamedArgument or Value)
                Type fieldType = Visit(field.Value);

                // 2. Explicitly store the type in the node instance
                // This ensures field.Value.Type is not null later
                field.Value.SetType(fieldType);

                if (_debug) Console.WriteLine($"Field {field.Label} resolved to {fieldType}");
            }

            Console.WriteLine("we done with visit record");

            // 3. Create the RecordType using the now-populated fields
            var recordType = new RecordType(expr.Fields);
            expr.SetType(recordType);

            return recordType;
        }

        public Type VisitRecordField(RecordFieldNodeExpr expr)
        {
            // 1. Visit the record source (could be an Id, an Index df[2], a Function call, etc.)
            Type recordSourceType = Visit(expr.IdRecord);

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
                {
                    throw new Exception($"Field '{expr.IdField}' not found in record.");
                }
            }
            else
            {
                throw new Exception($"Cannot access field '{expr.IdField}' on non-record type: {recordSourceType}");
            }

            if (_debug) Console.WriteLine($"Resolved field {expr.IdField} to type: {resolvedFieldType}");

            expr.SetType(resolvedFieldType);
            return resolvedFieldType;
        }

        public Type VisitRecordFieldAssign(RecordFieldAssignNodeExpr expr)
        {
            Visit(expr.AssignExpression);
            Visit(expr.IdRecord);

            expr.SetType(new VoidType());
            return expr.Type;
        }

        public Type VisitCopyRecord(CopyRecordNodeExpr expr)
        {
            Visit(expr.Source);
            expr.SetType(expr.Source.Type);
            return expr.Type;
        }

        public Type VisitAddField(AddFieldNodeExpr expr)
        {
            Visit(expr.Record);
            Visit(expr.Value);
            expr.SetType(expr.Value.Type);
            return expr.Type;
        }

        public Type VisitRemoveField(RemoveFieldNodeExpr expr)
        {
            Visit(expr.Record);
            expr.SetType(new VoidType());
            return expr.Type;
        }

        public Type VisitDataframe(DataframeNodeExpr expr)
        {
            for (int i = 0; i < expr.Rows.Elements.Count; i++)
            {
                (expr.Rows.Elements[i] as RecordNodeExpr).Fields.Insert(0, new RecordField() { Value = new NumberNodeExpr(i), Label = "index" });
            }

            Console.WriteLine("type check on df the type is: " + expr.Type);

            expr.Columns.Elements.Insert(0, new StringNodeExpr("index"));

            // 1. Visit Columns and Rows arrays to resolve their basic types
            var columnsType = Visit(expr.Columns) as ArrayType;
            var rowsType = Visit(expr.Rows) as ArrayType;

            if (expr.Rows.Elements.Count == 0)
                throw new Exception("Dataframe must have at least one row to infer types.");

            // 2. Force the first row to resolve its internal fields
            var firstRowNode = expr.Rows.Elements[0] as RecordNodeExpr;
            RecordType actualRowType = VisitRecord(firstRowNode) as RecordType;

            // 3. Extract Column Names
            var names = expr.Columns.Elements
                .OfType<StringNodeExpr>()
                .Select(s => s.Value)
                .ToList();

            // 4. Map Names to Types and populate the Node's DataTypes list
            var types = new List<Type>();
            expr.DataTypes.Clear(); // Clear any existing just in case

            foreach (var name in names)
            {
                var field = actualRowType.RecordFields.FirstOrDefault(f => f.Label == name);
                if (field == null) throw new Exception($"Field '{name}' not found.");

                if (field.Value.Type == null)
                    throw new Exception($"Field '{name}' still has no type after visiting!");

                types.Add(field.Value.Type);
                expr.DataTypes.Add(field.Value.Type); // Fill the list on the node
            }

            // 5. Finalize the node's type
            var dfType = new DataframeType(names, types, actualRowType);
            expr.SetType(dfType);
            Console.WriteLine("final df type: " + expr.Type);
            return dfType;
        }

        public Type VisitNamedArgument(NamedArgumentNodeExpr expr)
        {
            // If expr.Value is a StringNodeExpr ("Bob"), Visit returns StringType
            Type valType = Visit(expr.Value);

            // Save the type on the NamedArgument node itself
            expr.SetType(valType);

            return valType;
        }

        public Type VisitShowDataframe(ShowDataframeNodeExpr expr)
        {
            Visit(expr.Source);
            return expr.Type;
        }


        public Type VisitColumns(ColumnsNodeExpr expr)
        {
            Visit(expr.DataframeExpression);
            expr.SetType(new ArrayType(new StringType()));
            return expr.Type;
        }
        public Type VisitTypeLiteral(TypeLiteralNodeExpr node)
        {
            return node.Value; // Just return the wrapped type

        }
    }
}