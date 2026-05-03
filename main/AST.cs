using System;
using System.Collections.Generic;
using LLVMSharp.Interop;
using LLVMSharp;

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
        public List<ExpressionNode> Arguments { get; }

        public RandomNode(List<ExpressionNode> args)
        {
            Arguments = args;
        }

        public ExpressionNode MinValue => Arguments[0];
        public ExpressionNode MaxValue => Arguments[1];
        public ExpressionNode Decimals => Arguments.Count > 2 ? Arguments[2] : null;

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
        public ExpressionNode Left { get; set; }
        public string Operator { get; }
        public ExpressionNode Right { get; set; }
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
        public ExpressionNode Left { get; set; } // HACK, I would argue this should never be set again
        public string Operator { get; }
        public ExpressionNode Right { get; set; }
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
        public ExpressionNode Id { get; }
        public IncrementNode(ExpressionNode id)
        {
            Id = id;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIncrement(this);
    }

    public class DecrementNode : StatementNode
    {
        public ExpressionNode Id { get; }
        public DecrementNode(ExpressionNode id)
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
        public ExpressionNode SourceExpression { get; } // e.g., "arr"
        public Node Body { get; }

        public ForEachLoopNode(IdNode iterator, ExpressionNode sourceExpression, Node body)
        {
            Iterator = iterator;
            SourceExpression = sourceExpression;
            Body = body;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitForEachLoop(this);
    }

    public class ComparisonNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; } // HACK, I would argue this should never be set again
        public string Operator { get; }
        public ExpressionNode Right { get; set; }

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

        public IndexNode(ExpressionNode source, ExpressionNode index)
        {
            SourceExpression = source;
            IndexExpression = index;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIndex(this);
    }

    public class IndexAssignNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }
        public ExpressionNode IndexExpression { get; }
        public ExpressionNode AssignExpression { get; }

        public IndexAssignNode(ExpressionNode array, ExpressionNode index, ExpressionNode assignExpression)
        {
            ArrayExpression = array;
            IndexExpression = index;
            AssignExpression = assignExpression;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIndexAssign(this);
    }

    public class WhereNode : ExpressionNode
    {
        public IdNode IteratorId;
        public ExpressionNode SourceExpression;
        public ExpressionNode Condition;

        public WhereNode(IdNode iteratorId, ExpressionNode sourceExpression, ExpressionNode condition)
        {
            IteratorId = iteratorId;
            SourceExpression = sourceExpression;
            Condition = condition;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitWhere(this);
    }

    public class MapNode : ExpressionNode
    {
        public IdNode IteratorId;

        public ExpressionNode SourceExpression;
        public ExpressionNode Assignment;

        public MapNode(IdNode iteratorId, ExpressionNode sourceExpression, ExpressionNode assignment)
        {
            IteratorId = iteratorId;
            SourceExpression = sourceExpression;
            Assignment = assignment;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMap(this);
    }

    public class ReadCsvNode : ExpressionNode
    {
        public ExpressionNode SchemaExpression { get; set; }
        public ExpressionNode FileNameExpression { get; }

        public ReadCsvNode(List<ExpressionNode> args)
        {
            // Find the actual string literal (e.g., "test.csv")
            FileNameExpression = args.FirstOrDefault(a => a is StringNode);

            // Find the record/schema ([index: int...])
            SchemaExpression = args.FirstOrDefault(a => a is RecordNode);

            // If the parser didn't find them by type, fallback to positions
            if (FileNameExpression == null && args.Count >= 2)
            {
                FileNameExpression = args[1];
                SchemaExpression = args[0];
            }
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitReadCsv(this);
    }

    public class ToCsvNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }   // e.g., variable or object
        public ExpressionNode FileNameExpression { get; } // expression producing string

        public ToCsvNode(ExpressionNode expr, ExpressionNode fileName)
        {
            SourceExpression = expr;
            FileNameExpression = fileName;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitToCsv(this);
    }

    public class AddNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }
        public ExpressionNode AddExpression { get; }

        public AddNode(ExpressionNode source, ExpressionNode addExpression)
        {
            SourceExpression = source;
            AddExpression = addExpression;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitAdd(this);
    }

    public class AddRangeNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }
        public ExpressionNode AddRangeExpression { get; }

        public AddRangeNode(ExpressionNode source, ExpressionNode addRangeExpression)
        {
            SourceExpression = source;
            AddRangeExpression = addRangeExpression;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitAddRange(this);
    }

    public class RemoveNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }
        public ExpressionNode RemoveExpression { get; }

        public RemoveNode(ExpressionNode array, ExpressionNode removeExpression)
        {
            SourceExpression = array;
            RemoveExpression = removeExpression;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRemove(this);
    }

    public class RemoveRangeNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }
        public ExpressionNode RemoveRangeExpression { get; }

        public RemoveRangeNode(ExpressionNode array, ExpressionNode removeRangeExpression)
        {
            SourceExpression = array;
            RemoveRangeExpression = removeRangeExpression;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRemoveRange(this);
    }

    public class LengthNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public LengthNode(ExpressionNode array)
        {
            ArrayExpression = array;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitLength(this);
    }

    public class MinNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public MinNode(ExpressionNode array)
        {
            ArrayExpression = array;
            Type = new FloatType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMin(this);
    }

    public class MaxNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public MaxNode(ExpressionNode array)
        {
            ArrayExpression = array;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMax(this);
    }

    public class MeanNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public MeanNode(ExpressionNode array)
        {
            ArrayExpression = array;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMean(this);
    }

    public class SumNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public SumNode(ExpressionNode array)
        {
            ArrayExpression = array;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitSum(this);
    }

    public class CorrelationNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }
        public ExpressionNode TargetExpression { get; }

        public CorrelationNode(ExpressionNode source, ExpressionNode target)
        {
            SourceExpression = source;
            TargetExpression = target;
            Type = new FloatType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitCorrelation(this);
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

    public class FieldNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }
        public string IdField { get; }

        public FieldNode(ExpressionNode sourceExpression, string idField)
        {
            SourceExpression = sourceExpression;
            IdField = idField;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitField(this);
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
        public ExpressionNode SourceExpression { get; }

        public CopyNode(ExpressionNode source)
        {
            SourceExpression = source;
            Type = source.Type;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitCopy(this);
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

    public enum DataframeMode
    {
        SchemaOnly,   // columns + types
        DataDriven    // columns + data (types inferred)
    }

    public class DataframeNode : ExpressionNode
    {
        public ExpressionNode Columns { get; }
        public ExpressionNode Data { get; }
        public ExpressionNode DataTypes { get; set; }

        public DataframeNode(List<NamedArgumentNode> args)
        {
            var positional = new List<ExpressionNode>();

            foreach (var arg in args)
            {
                if (arg.Name == null)
                {
                    positional.Add(arg.Value);
                    continue;
                }

                switch (arg.Name)
                {
                    case "columns":
                        Columns = arg.Value;
                        break;

                    case "data":
                        Data = arg.Value;
                        break;

                    case "type":
                    case "types":
                        DataTypes = arg.Value;
                        break;

                    default:
                        throw new Exception($"Unknown dataframe argument '{arg.Name}'");
                }
            }

            bool hasData = Data != null;
            bool hasTypes = DataTypes != null;

            if (positional.Count > 2)
                throw new Exception("Too many positional arguments for dataframe");

            if (Columns == null && positional.Count > 0)
                Columns = positional[0];

            if (positional.Count > 1)
            {
                var second = positional[1];

                // Decide if it's data or types based on shape
                if (IsTypeArray(second))
                    DataTypes = second; // we never set types if we don't get it as an argument
                else
                    Data = second;
            }

            Type = BuildDataframeType();

            Console.WriteLine("Yo the dat types in the ctor exist: ");
            Console.WriteLine(DataTypes is not null);
            // foreach (var item in (DataTypes as ArrayNode).Elements)
            // {
            //     Console.WriteLine("the data types: " + item);
            // }
        }

        private Type Infer(object value)
        {
            return value switch
            {
                int => new IntType(),
                double => new FloatType(),
                string => new StringType(),
                bool => new BoolType(),
                _ => throw new Exception("Unsupported type for inference")
            };
        }

        private bool IsTypeArray(ExpressionNode node)
        {
            if (node is not ArrayNode arr)
                return false;

            return arr.Elements.All(e => e is TypeNode || e is TypeLiteralNode);
        }
        private DataframeType BuildDataframeType()
        {
            var columns = ExtractColumns();

            if ((Data as ArrayNode).Elements.Count != 0)
            {
                var data = ExtractData();
                var inferredTypes = InferTypes(data);

                var argumentNodes = new List<ExpressionNode>();
                foreach (var item in inferredTypes)
                {
                    argumentNodes.Add(new NamedArgumentNode(item.ToString(), InferNodeFromString(item.ToString())));
                }
                DataTypes = new ArrayNode(argumentNodes);

                return new DataframeType(columns, data, inferredTypes);
            }

            var types = ExtractTypes();

            return new DataframeType(
                columns,
                new List<List<object>>(), // empty data
                types
            );
        }

        private ExpressionNode InferNodeFromString(string val)
        {
            return val switch
            {
                "string" => new StringNode(""),
                "int" => new NumberNode(0),
                "float" => new FloatNode(0),
                "bool" => new BooleanNode(false),
                _ => throw new Exception($"Unsupported value type: {val.GetType().Name}")
            };
        }
        private List<string> ExtractColumns()
        {
            var arr = Columns as ArrayNode
                ?? throw new Exception("columns must be an array");

            return arr.Elements.Select(e =>
            {
                if (e is StringNode s)
                    return s.Value;

                throw new Exception("column names must be strings");
            }).ToList();
        }
        private List<List<object>> ExtractData()
        {
            var arr = Data as ArrayNode
                ?? throw new Exception("data must be an array");

            return arr.Elements.Select(row =>
            {
                if (row is not ArrayNode rowArr)
                    throw new Exception("each row must be an array");

                return rowArr.Elements.Select(ValueOf).ToList();
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
        private List<Type> ExtractTypes()
        {
            var arr = DataTypes as ArrayNode
                ?? throw new Exception("type must be an array");

            return arr.Elements.Select(node =>
            {
                if (node is TypeLiteralNode t)
                    return InferFromString(t.TypeNode.Name); // or t.Type

                throw new Exception("types must be type literals");
            }).ToList();
        }
        private List<Type> InferTypes(List<List<object>> data)
        {
            if (data.Count == 0)
                throw new Exception("Cannot infer types from empty data");

            return data[0].Select(Infer).ToList();
        }

        private Type InferFromString(string value)
        {
            return value switch
            {
                "int" => new IntType(),
                "double" => new FloatType(),
                "string" => new StringType(),
                "bool" => new BoolType(),
                _ => throw new Exception("Unsupported type for inference")
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
                _ => throw new Exception("Unsupported type for inference")
            };
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor)
            => visitor.VisitDataframe(this);
    }


    // public class DataframeNode : ExpressionNode
    // {
    //     public ExpressionNode Columns { get; private set; }
    //     public ExpressionNode Data { get; private set; }
    //     public ExpressionNode DataTypes { get; private set; }

    //     public DataframeMode Mode { get; private set; }

    //     public DataframeNode(List<NamedArgumentNode> args)
    //     {
    //         var positional = new List<ExpressionNode>();

    //         // -------------------------
    //         // 1. Collect arguments
    //         // -------------------------
    //         foreach (var arg in args)
    //         {
    //             if (arg.Name == null)
    //             {
    //                 positional.Add(arg.Value);
    //                 continue;
    //             }

    //             switch (arg.Name)
    //             {
    //                 case "columns":
    //                     Columns = arg.Value;
    //                     break;

    //                 case "data":
    //                     Data = arg.Value;
    //                     break;

    //                 case "type":
    //                 case "types":
    //                     DataTypes = arg.Value;
    //                     break;

    //                 default:
    //                     throw new Exception($"Unknown dataframe argument '{arg.Name}'");
    //             }
    //         }

    //         // -------------------------
    //         // 2. Positional fallback
    //         // -------------------------
    //         if (Columns == null && positional.Count > 0)
    //             Columns = positional[0];

    //         if (Data == null && positional.Count > 1)
    //             Data = positional[1];

    //         // -------------------------
    //         // 3. Mode resolution
    //         // -------------------------
    //         bool hasData = Data != null;
    //         bool hasTypes = DataTypes != null;

    //         if (hasData && hasTypes)
    //             throw new Exception("Cannot specify both 'data' and 'type' in dataframe");

    //         if (Columns == null)
    //             throw new Exception("Dataframe requires 'columns'");

    //         if (!hasData && !hasTypes)
    //             throw new Exception("Dataframe must specify either 'data' or 'type'");

    //         Mode = hasData ? DataframeMode.DataDriven : DataframeMode.SchemaOnly;

    //         if (!hasData) Data = new ArrayNode(new List<ExpressionNode>()); // empty
    //         System.Console.WriteLine("the data in ctor: " + Data);

    //         // -------------------------
    //         // 4. Build runtime type
    //         // -------------------------
    //         Type = CreateDataframeType();
    //     }

    //     private DataframeType CreateDataframeType()
    //     {
    //         var columns = ExtractColumns();

    //         if (Mode == DataframeMode.DataDriven)
    //         {
    //             var data = ExtractData();
    //             var inferredTypes = InferTypes(data);

    //             return new DataframeType(columns, data, inferredTypes);
    //         }

    //         // Schema-only mode
    //         var explicitTypes = ExtractTypes();
    //         var emptyData = new List<List<object>>();

    //         return new DataframeType(columns, emptyData, explicitTypes);
    //     }
    //     private List<string> ExtractColumns()
    //     {
    //         if (Columns is not ArrayNode arr)
    //             throw new Exception("columns must be an array");

    //         return arr.Elements.Select(e =>
    //         {
    //             if (e is StringNode s)
    //                 return s.Value;

    //             throw new Exception("column names must be strings");
    //         }).ToList();
    //     }
    //     private List<List<object>> ExtractData()
    //     {
    //         if (Data is not ArrayNode outer)
    //             throw new Exception("data must be an array of rows");

    //         return outer.Elements.Select(row =>
    //         {
    //             if (row is not ArrayNode inner)
    //                 throw new Exception("each row must be an array");

    //             return inner.Elements.Select(ValueOf).ToList();
    //         }).ToList();
    //     }
    //     private object ValueOf(ExpressionNode node)
    //     {
    //         return node switch
    //         {
    //             NumberNode n => n.Value,
    //             FloatNode f => f.Value,
    //             StringNode s => s.Value,
    //             BooleanNode b => b.Value,
    //             _ => throw new Exception($"Unsupported value: {node.GetType().Name}")
    //         };
    //     }
    //     private List<Type> ExtractTypes()
    //     {
    //         if (DataTypes is not ArrayNode arr)
    //             throw new Exception("type must be an array");

    //         return arr.Elements.Select(node =>
    //         {
    //             System.Console.WriteLine("extract type for node: " + node);
    //             System.Console.WriteLine("extract type for node: " + node.Type);
    //             if (node is TypeLiteralNode t)
    //             {
    //                 var val = InferTypeFromString(t.TypeNode.Name);
    //                 System.Console.WriteLine("the infered type is: " + val);
    //                 return val;
    //             }


    //             throw new Exception("types must be type literals");
    //         }).ToList();
    //     }
    //     private List<Type> InferTypes(List<List<object>> data)
    //     {
    //         if (data.Count == 0)
    //             throw new Exception("Cannot infer types from empty data");

    //         return data[0].Select(Infer).ToList();
    //     }

    //     private Type InferTypeFromString(string val)
    //     {
    //         return val switch
    //         {
    //             "string" => new StringType(),
    //             "int" => new IntType(),
    //             "float" => new FloatType(),
    //             "bool" => new BoolType()
    //         };
    //     }

    //     private Type Infer(object value)
    //     {
    //         return value switch
    //         {
    //             int => new IntType(),
    //             double => new FloatType(),
    //             string => new StringType(),
    //             bool => new BoolType(),
    //             _ => throw new Exception("Unsupported type for inference")
    //         };
    //     }
    //     public override LLVMValueRef Accept(IExpressionVisitor visitor)
    //         => visitor.VisitDataframe(this);
    // }

    public class ColumnsNode : ExpressionNode
    {
        public ExpressionNode DataframeExpression { get; }

        public ColumnsNode(ExpressionNode dataframe)
        {
            DataframeExpression = dataframe;
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

    public class SqrtNode : ExpressionNode
    {
        public ExpressionNode Value { get; }

        public SqrtNode(ExpressionNode value)
        {
            Value = value;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitSqrt(this);
    }

    public class CastNode : ExpressionNode
    {
        public ExpressionNode Expression { get; }
        public Type FromType { get; }
        public Type ToType { get; }

        public CastNode(ExpressionNode expr, Type fromType, Type toType)
        {
            Expression = expr;
            ToType = toType;
            FromType = fromType;
            SetType(toType);
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitCast(this);
    }

    public class ExponentialMathFuncNode : ExpressionNode
    {
        public ExpressionNode Value { get; }

        public ExponentialMathFuncNode(ExpressionNode value)
        {
            Value = value;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitExponentialMathFunc(this);
    }

    public class PowNode : ExpressionNode
    {
        public ExpressionNode Value { get; }
        public ExpressionNode Power { get; }

        public PowNode(ExpressionNode value, ExpressionNode power)
        {
            Value = value;
            Power = power;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitPow(this);
    }

    public class LogNode : ExpressionNode
    {
        public ExpressionNode Value { get; }

        public LogNode(ExpressionNode value)
        {
            Value = value;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitLog(this);
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

}