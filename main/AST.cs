using System;
using System.Collections.Generic;
using LLVMSharp.Interop;
using LLVMSharp;
using System.Linq.Expressions;

namespace MyCompiler
{
    // The base class for all Node in your tree
    public abstract class Node
    {
        public abstract LLVMValueRef Accept(IExpressionVisitor visitor);
    }

    // Intermediate category
    public abstract class ExpressionNode : Node
    {
        public Type Type { get; protected set; }
        // Add this setter helper
        public void SetType(Type type) => Type = type;
    }

    public abstract class StatementNode : Node
    {
        public Type Type { get; protected set; }
        // Add this setter helper
        public void SetType(Type type) => Type = type;
    }

    //-----Built-in-function-nodes-----//

    // Represents print function
    public class PrintNode : StatementNode
    {
        public ExpressionNode Expression { get; set; }
        public PrintNode(ExpressionNode expr)
        {
            Expression = expr;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitPrint(this);
    }

    // Represents Random function
    public class RandomNode : ExpressionNode
    {
        public ExpressionNode MinValue { get; set; }
        public ExpressionNode MaxValue { get; set; }

        public RandomNode(ExpressionNode minValue, ExpressionNode maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRandom(this);
    }

    public class RoundNode : ExpressionNode
    {
        public ExpressionNode Value { get; }
        public ExpressionNode Decimals { get; }

        public RoundNode(ExpressionNode value, ExpressionNode decimals)
        {
            Value = value;
            Decimals = decimals;
            Type = new FloatType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRound(this);
    }

    public class FloatNode : ExpressionNode
    {
        public double Value { get; }
        public FloatNode(double value)
        {
            Value = value;
            Type = new FloatType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitFloat(this);
    }
    // Represents a single number (e.g., 10)
    public class NumberNode : ExpressionNode
    {
        public int Value { get; }

        public NumberNode(int value)
        {
            Value = value;
            Type = new IntType();
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitNumber(this);
    }

    // Represents a string (e.g., "Hello")
    public class StringNode : ExpressionNode
    {
        public string Value { get; }
        public StringNode(string value)
        {
            Value = value;
            Type = new StringType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitString(this);
    }

    // Boolean
    public class BooleanNode : ExpressionNode
    {
        public bool Value { get; }

        public BooleanNode(bool value)
        {
            Value = value;
            Type = new BoolType();
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitBoolean(this);
    }

    public class NullNode : ExpressionNode
    {
        public NullNode()
        {
            Type = new NullType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitNull(this);
    }

    // Represents a variable name (e.g., x)
    public class IdNode : ExpressionNode
    {
        public string Name { get; }
        public IdNode(string name) => Name = name;
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitId(this);
    }

    // Represents a math operation (e.g., 10 + 20)
    public class BinaryOpNode : ExpressionNode
    {
        public ExpressionNode Left { get; }
        public string Operator { get; }
        public ExpressionNode Right { get; }
        public BinaryOpNode(ExpressionNode left, string op, ExpressionNode right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitBinary(this);
    }

    public class LogicalOpNode : ExpressionNode
    {
        public ExpressionNode Left { get; }
        public string Operator { get; }
        public ExpressionNode Right { get; }
        public LogicalOpNode(ExpressionNode left, string op, ExpressionNode right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitLogicalOp(this);
    }

    // Represents an assignment (e.g., x = 10)
    public class AssignNode : StatementNode
    {
        public string Id { get; }  // ID = expr  -->   x = 10 
        public ExpressionNode Expression { get; }
        public AssignNode(string id, ExpressionNode expr)
        {
            Id = id; Expression = expr;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitAssign(this);
    }

    public class IncrementNode : StatementNode
    {
        public string Id { get; }
        public IncrementNode(string id)
        {
            Id = id;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIncrement(this);
    }
    public class DecrementNode : StatementNode
    {
        public string Id { get; }
        public DecrementNode(string id)
        {
            Id = id;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitDecrement(this);
    }

    // A list of statements (the whole program)
    public class SequenceNode : Node
    {
        public List<Node> Statements { get; } = new List<Node>();

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitSequence(this);
    }

    // IF statement
    public class IfNode : StatementNode
    {
        public ExpressionNode Condition { get; }
        public Node ThenPart { get; }
        public Node ElsePart { get; }
        public IfNode(ExpressionNode cond, Node thenP, Node elseP = null)
        {
            Condition = cond;
            ThenPart = thenP;
            ElsePart = elseP;
            Type = new VoidType();
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIf(this);
    }

    public class ForLoopNode : StatementNode
    {
        public StatementNode Initialization { get; }   // for(x=0;x<5;x++)x
        public ExpressionNode Condition { get; }
        public StatementNode Step { get; }
        public Node Body; // Changed from ExpressionNode

        public ForLoopNode(StatementNode init, ExpressionNode cond, StatementNode step, Node body)
        {
            Initialization = init;
            Condition = cond;
            Step = step;
            Body = body;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitForLoop(this);
    }

    public class ForEachLoopNode : StatementNode
    {
        public IdNode Iterator { get; }      // e.g., "item"
        public ExpressionNode Source { get; } // e.g., "arr"
        public Node Body { get; }

        public ForEachLoopNode(IdNode iterator, ExpressionNode source, Node body)
        {
            Iterator = iterator;
            Source = source;
            Body = body;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitForEachLoop(this);
    }

    public class ComparisonNode : ExpressionNode
    {
        public ExpressionNode Left { get; }
        public string Operator { get; }
        public ExpressionNode Right { get; }

        public ComparisonNode(ExpressionNode left, string op, ExpressionNode right)
        {
            Left = left;
            Operator = op;
            Right = right;
            Type = new BoolType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitComparison(this);
    }

    public class ArrayNode : ExpressionNode
    {
        public List<ExpressionNode> Elements { get; }
        public Type ElementType { get; set; }
        public uint? Capacity { get; }

        public ArrayNode(List<ExpressionNode> elements)
        {
            Elements = elements;

            if (elements.Count > 0)
                ElementType = elements[0].Type; // infer type from first element
            else
                ElementType = new IntType();

            Type = new ArrayType(ElementType);
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitArray(this);
    }

    public class CopyArrayNode : CopyNode
    {
        public CopyArrayNode(ExpressionNode source) : base(source) { }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitCopy(this);
    }

    public class IndexNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }
        public ExpressionNode IndexExpression { get; }

        public IndexNode(ExpressionNode sourceExpr, ExpressionNode indexExpr)
        {
            SourceExpression = sourceExpr;
            IndexExpression = indexExpr;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIndex(this);
    }

    public class IndexAssignNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }
        public ExpressionNode IndexExpression { get; }
        public ExpressionNode AssignExpression { get; }

        public IndexAssignNode(ExpressionNode arrayExpr, ExpressionNode indexExpr, ExpressionNode assignExpression)
        {
            ArrayExpression = arrayExpr;
            IndexExpression = indexExpr;
            AssignExpression = assignExpression;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIndexAssign(this);
    }

    public class WhereNode : ExpressionNode
    {
        public IdNode IteratorId { get; }
        public ExpressionNode SourceExpr { get; }
        public ExpressionNode Condition { get; }

        public WhereNode(IdNode iteratorId, ExpressionNode sourceExpr, ExpressionNode condition)
        {
            IteratorId = iteratorId;
            SourceExpr = sourceExpr;
            Condition = condition;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitWhere(this);
    }

    public class MapNode : ExpressionNode
    {
        public IdNode IteratorId { get; }
        public ExpressionNode SourceExpr { get; }
        public ExpressionNode Assignment { get; }

        public MapNode(IdNode iteratorId, ExpressionNode sourceExpr, ExpressionNode assignment)
        {
            IteratorId = iteratorId;
            SourceExpr = sourceExpr;
            Assignment = assignment;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMap(this);
    }

    public class ReadCsvNode : ExpressionNode
    {
        public ExpressionNode SchemaExpr { get; set; }
        public ExpressionNode FileNameExpr { get; }

        public ReadCsvNode(List<ExpressionNode> args)
        {
            // Find the actual string literal (e.g., "test.csv")
            FileNameExpr = args.FirstOrDefault(a => a is StringNode);

            // Find the record/schema ([index: int...])
            SchemaExpr = args.FirstOrDefault(a => a is RecordNode);

            // If the parser didn't find them by type, fallback to positions
            if (FileNameExpr == null && args.Count >= 2)
            {
                FileNameExpr = args[1];
                SchemaExpr = args[0];
            }
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitReadCsv(this);
    }

    public class ToCsvNode : ExpressionNode
    {
        public ExpressionNode Expression { get; }   // e.g., variable or object
        public ExpressionNode FileNameExpr { get; } // expression producing string

        public ToCsvNode(ExpressionNode expr, ExpressionNode fileNameExpr)
        {
            Expression = expr;
            FileNameExpr = fileNameExpr;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitToCsv(this);
    }

    public class AddNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }
        public ExpressionNode AddExpression { get; }

        public AddNode(ExpressionNode sourceExpr, ExpressionNode addExpression)
        {
            SourceExpression = sourceExpr;
            AddExpression = addExpression;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitAdd(this);
    }

    public class AddRangeNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }
        public ExpressionNode AddRangeExpression { get; }

        public AddRangeNode(ExpressionNode sourceExpr, ExpressionNode addRangeExpression)
        {
            SourceExpression = sourceExpr;
            AddRangeExpression = addRangeExpression;
            Type = new ArrayType(SourceExpression.Type);
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitAddRange(this);
    }

    public class RemoveNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }
        public ExpressionNode RemoveExpression { get; }

        public RemoveNode(ExpressionNode arrayExpr, ExpressionNode removeExpression)
        {
            SourceExpression = arrayExpr;
            RemoveExpression = removeExpression;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRemove(this);
    }

    public class RemoveRangeNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }
        public ExpressionNode RemoveRangeExpression { get; }

        public RemoveRangeNode(ExpressionNode arrayExpr, ExpressionNode removeRangeExpression)
        {
            SourceExpression = arrayExpr;
            RemoveRangeExpression = removeRangeExpression;
            Type = new ArrayType(SourceExpression.Type);
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRemoveRange(this);
    }

    public class LengthNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public LengthNode(ExpressionNode arrayExpr)
        {
            ArrayExpression = arrayExpr;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitLength(this);
    }

    public class MinNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public MinNode(ExpressionNode arrayExpr)
        {
            ArrayExpression = arrayExpr;
            Type = new FloatType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMin(this);
    }

    public class MaxNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public MaxNode(ExpressionNode arrayExpr)
        {
            ArrayExpression = arrayExpr;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMax(this);
    }

    public class MeanNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public MeanNode(ExpressionNode arrayExpr)
        {
            ArrayExpression = arrayExpr;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMean(this);
    }

    public class SumNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public SumNode(ExpressionNode arrayExpr)
        {
            ArrayExpression = arrayExpr;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitSum(this);
    }

    public class UnaryOpNode : ExpressionNode
    {
        public string Operator { get; }
        public ExpressionNode Operand { get; }

        public UnaryOpNode(string operatorSymbol, ExpressionNode operand)
        {
            Operator = operatorSymbol;
            Operand = operand;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitUnaryOp(this);
    }

    public class RecordField
    {
        public string Label { get; set; }

        // Used during codegen
        public ExpressionNode Value { get; set; }

        // Filled during typechecking
        public Type Type { get; set; }
    }

    public class RecordNode : ExpressionNode
    {
        public List<RecordField> Fields { get; set; } = new List<RecordField>();
        public List<Type> ElementTypes { get; } = new List<Type>();

        public RecordNode(List<NamedArgumentNode> valuesArray)
        {
            // 1. Cast the inputs to ArrayNode to get into their internal lists
            var labelNodes = valuesArray.Select(v => new StringNode(v.Name)).ToList();
            var valueNodes = valuesArray;

            if (labelNodes == null || valueNodes == null)
                throw new Exception("Record requires two arrays: labels and values.");

            if (labelNodes.Count != valueNodes.Count)
                throw new Exception("Record labels and values count mismatch.");

            // 2. Zip them together into the Fields list
            for (int i = 0; i < labelNodes.Count; i++)
            {
                // Ensure the label is actually a StringNode
                if (labelNodes[i] is StringNode strNode)
                {
                    Fields.Add(new RecordField
                    {
                        Label = strNode.Value,
                        Value = valueNodes[i]
                    });
                    ElementTypes.Add(valueNodes[i].Type);
                }
                else
                {
                    throw new Exception("Record labels must be string literals.");
                }
            }
            Type = new RecordType(Fields);
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRecord(this);
    }

    public class RecordFieldNode : ExpressionNode
    {
        public ExpressionNode IdRecord { get; }
        public string IdField { get; }

        public RecordFieldNode(ExpressionNode idRecord, string idField)
        {
            IdRecord = idRecord;
            IdField = idField;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRecordField(this);
    }

    public class RecordFieldAssignNode : StatementNode
    {
        public ExpressionNode IdRecord { get; }
        public string IdField { get; }
        public ExpressionNode AssignExpression { get; }

        public RecordFieldAssignNode(ExpressionNode idRecord, string idField, ExpressionNode assignExpression)
        {
            IdRecord = idRecord;
            IdField = idField;
            AssignExpression = assignExpression;
            Type = new VoidType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRecordFieldAssign(this);
    }

    public class CopyNode : ExpressionNode
    {
        public ExpressionNode Source { get; }

        public CopyNode(ExpressionNode source)
        {
            Source = source;
            Type = source.Type;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitCopy(this);
    }

    public class AddFieldNode : ExpressionNode
    {
        public ExpressionNode Record { get; }
        public string FieldName { get; }
        public ExpressionNode Value { get; }

        public AddFieldNode(ExpressionNode record, string fieldName, ExpressionNode value)
        {
            Record = record;
            FieldName = fieldName;
            Value = value;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitAddField(this);
    }

    public class RemoveFieldNode : ExpressionNode
    {
        public ExpressionNode Record { get; }
        public string FieldName { get; }

        public RemoveFieldNode(ExpressionNode record, string fieldName)
        {
            Record = record;
            FieldName = fieldName;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRemoveField(this);
    }

    public class NamedArgumentNode : ExpressionNode
    {
        public string Name { get; }
        public ExpressionNode Value { get; }

        public NamedArgumentNode(string name, ExpressionNode value)
        {
            Name = name;
            Value = value;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitNamedArgument(this);
    }

    public class DataframeNode : ExpressionNode
    {
        public ArrayNode Columns { get; }
        public ArrayNode Rows { get; }
        public ArrayNode DataTypes { get; }

        public DataframeNode(List<NamedArgumentNode> args)
        {
            ArrayNode columns = null;
            ArrayNode rows = null;
            ArrayNode types = null;

            int positionalIndex = 0;

            foreach (var arg in args)
            {
                // ALWAYS unwrap if NamedArgumentNode
                if (arg is NamedArgumentNode named)
                {
                    if (named.Value is not ArrayNode value)
                        throw new Exception("Dataframe arguments must be arrays.");

                    if (named.Name != null)
                    {
                        // --- Named arguments ---
                        switch (named.Name)
                        {
                            case "columns": columns = value; break;
                            case "rows":
                            case "data": rows = value; break;
                            case "types":
                            case "type": types = value; break;
                            default:
                                throw new Exception($"Unknown dataframe argument '{named.Name}'");
                        }
                    }
                    else
                    {
                        // --- Positional arguments ---
                        switch (positionalIndex)
                        {
                            case 0: columns = value; break;
                            case 1: rows = value; break;
                            case 2: types = value; break;
                            default:
                                throw new Exception("Too many positional arguments for dataframe");
                        }
                        positionalIndex++;
                    }
                }
                else
                {
                    throw new Exception("Unexpected argument type in dataframe");
                }
            }

            Columns = columns ?? throw new Exception("Dataframe requires 'columns'");
            Rows = rows ?? new ArrayNode(new List<ExpressionNode>());
            DataTypes = types;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitDataframe(this);
    }

    public class ColumnsNode : ExpressionNode
    {
        public ExpressionNode DataframeExpression { get; }

        public ColumnsNode(ExpressionNode dataframeExpr)
        {
            DataframeExpression = dataframeExpr;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitColumns(this);
    }

    public class ShowDataframeNode : ExpressionNode
    {
        public ExpressionNode Source { get; }
        public List<ExpressionNode> Columns { get; }

        public ShowDataframeNode(ExpressionNode source, List<ExpressionNode> columns)
        {
            Source = source;
            Columns = columns;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitShowDataframe(this);
    }

    public class TypeNode : ExpressionNode
    {
        public string Name { get; }
        public TypeNode(string name) => Name = name;

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => throw new NotImplementedException(); // not used in codegen
    }

    public class TypeLiteralNode : ExpressionNode
    {
        public TypeNode TypeNode { get; }

        public TypeLiteralNode(TypeNode typeNode)
        {
            TypeNode = typeNode;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitTypeLiteral(this);
    }
}
