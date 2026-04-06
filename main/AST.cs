using System;
using System.Collections.Generic;
using LLVMSharp.Interop;
using LLVMSharp;
using System.Linq.Expressions;

namespace MyCompiler
{
    // public enum MyType
    // {
    //     Int,
    //     String,
    //     Bool,
    //     Float,
    //     Array,
    //     None
    // }
    // The base class for all NodeExprs in your tree
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

    //------Function-nodes------//
    public class FunctionDefNode : Node
    {
        public string Name { get; }
        public Type ReturnTypeName { get; set; } // "Int", "String", "Float", etc.
        public List<string> Parameters { get; set; }
        public Node Body { get; set; }

        public FunctionDefNode(string name, string returnType, List<string> parameters, Node body)
        {
            Name = name;

            ReturnTypeName = returnType.ToLower() switch
            {
                "int" => new IntType(),
                "float" => new FloatType(),
                "double" => new FloatType(),
                "bool" => new BoolType(),
                "string" => new StringType(),
                "array" => new ArrayType(new IntType()), // Temporarily default element type until type-analysis updates it
                _ => new VoidType()
            };

            Parameters = parameters;
            Body = body;
        }

        //public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitFunctionDef(this);
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => throw new NotImplementedException();
    }

    public class FunctionCallNode : ExpressionNode
    {
        public string Name { get; }
        public List<ExpressionNode> Arguments { get; }

        public FunctionCallNode(string name, List<ExpressionNode> arguments)
        {
            Name = name;
            Arguments = arguments;
            // For now, we assume functions return Float to be safe with math
            Type = new FloatType();
        }

        // public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitFunctionCall(this);
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => throw new NotImplementedException();
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
        public int Value { get; set; }

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
        public string Value { get; set; }
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
        public string Name { get; set; }
        public IdNode(string name) => Name = name;
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitId(this);
    }

    // Represents a math operation (e.g., 10 + 20)
    public class BinaryOpNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }
        public string Operator { get; set; }
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
        public ExpressionNode Left { get; set; }
        public string Operator { get; set; }
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
        public string Id { get; set; }  // ID = expr  -->   x = 10 
        public ExpressionNode Expression { get; set; }
        public AssignNode(string id, ExpressionNode expr)
        {
            Id = id; Expression = expr;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitAssign(this);
    }

    public class IncrementNode : StatementNode
    {
        public string Id { get; set; }
        public IncrementNode(string id)
        {
            Id = id;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIncrement(this);
    }
    public class DecrementNode : StatementNode
    {
        public string Id { get; set; }
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
        public ExpressionNode Condition;
        public Node ThenPart;
        public Node ElsePart;

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
        public StatementNode Initialization;   // for(x=0;x<5;x++)x

        public ExpressionNode Condition;
        public StatementNode Step;
        public Node Body; // Changed from ExpressionNodeExpr

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
        public ExpressionNode Array { get; } // e.g., "arr"
        public Node Body { get; }

        public ForEachLoopNode(IdNode iterator, ExpressionNode array, Node body)
        {
            Iterator = iterator;
            Array = array;
            Body = body;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitForEachLoop(this);
    }

    public class ComparisonNode : ExpressionNode
    {
        public ExpressionNode Left;
        public string Operator { get; set; }

        public ExpressionNode Right;

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
        public IdNode IteratorId;
        public ExpressionNode SourceExpr;
        public ExpressionNode Condition;

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
        public IdNode IteratorId;

        public ExpressionNode SourceExpr;
        public ExpressionNode Assignment;

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
        public ExpressionNode FileNameExpr { get; set; }

        public ReadCsvNode(List<ExpressionNode> args)
        {
            // Find the actual string literal (e.g., "test.csv")
            this.FileNameExpr = args.FirstOrDefault(a => a is StringNode);

            // Find the record/schema ([index: int...])
            this.SchemaExpr = args.FirstOrDefault(a => a is RecordNode);

            // If the parser didn't find them by type, fallback to positions
            if (this.FileNameExpr == null && args.Count >= 2)
            {
                this.FileNameExpr = args[1];
                this.SchemaExpr = args[0];
            }
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor)
            => visitor.VisitReadCsv(this);
    }


    public class ToCsvNode : ExpressionNode
    {
        public ExpressionNode Expression { get; set; }   // e.g., variable or object
        public ExpressionNode FileNameExpr { get; set; } // expression producing string

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
            Type = new ArrayType(SourceExpression.Type);
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
            Type = new ArrayType(SourceExpression.Type);
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
        public List<RecordField> Fields { get; } = new List<RecordField>();
        public List<Type> ElementTypes { get; } = new List<Type>();

        public RecordNode(List<NamedArgumentNode> valuesArray)
        {
            // 1. Cast the inputs to ArrayNodeExpr to get into their internal lists
            var labelNodes = valuesArray.Select(v => new StringNode(v.Name)).ToList();
            var valueNodes = valuesArray;

            if (labelNodes == null || valueNodes == null)
                throw new Exception("Record requires two arrays: labels and values.");

            if (labelNodes.Count != valueNodes.Count)
                throw new Exception("Record labels and values count mismatch.");

            // 2. Zip them together into the Fields list
            for (int i = 0; i < labelNodes.Count; i++)
            {
                // Ensure the label is actually a StringNodeExpr
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

    // public class CopyRecordNode : CopyNode
    // {
    //     public CopyRecordNode(ExpressionNode source) : base(source) { }

    //     public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitCopyRecord(this);
    // }

    // public class CopyDataframeNode : CopyNode
    // {
    //     public CopyDataframeNode(ExpressionNode source) : base(source) { }

    //     public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitCopyDataframe(this);
    // }

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
        // Change these from ArrayNode to ExpressionNode
        public ExpressionNode Columns { get; private set; }
        public ExpressionNode Rows { get; private set; }
        public List<Type> DataTypes { get; private set; } = new List<Type>();

        public DataframeNode(List<ExpressionNode> args)
        {
            for (int i = 0; i < args.Count; i++)
            {
                var currentArg = args[i];

                if (currentArg is NamedArgumentNode named)
                {
                    // Remove the "as ArrayNode" cast. Use the raw ExpressionNode.
                    var actualValue = named.Value;

                    if (named.Name == "columns") Columns = actualValue;
                    else if (named.Name == "data" || named.Name == "rows") Rows = actualValue;
                    else if (string.IsNullOrEmpty(named.Name))
                    {
                        if (i == 0) Columns = actualValue;
                        else if (i == 1) Rows = actualValue;
                    }
                }
                else
                {
                    // Accept any ExpressionNode positionally
                    if (i == 0) Columns = currentArg;
                    else if (i == 1) Rows = currentArg;
                }
            }

            if (Columns == null) throw new Exception("dataframe requires 'columns' expression");
            if (Rows == null) throw new Exception("dataframe requires 'data' expression");
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitDataframe(this);
    }


    public class ColumnsNode : ExpressionNode
    {
        public ExpressionNode DataframeExpression { get; private set; }

        public ColumnsNode(ExpressionNode dataframeExpr)
        {
            DataframeExpression = dataframeExpr;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitColumns(this);
    }

    public class ShowDataframeNode : ExpressionNode
    {
        public ExpressionNode Source { get; private set; }
        public List<ExpressionNode> Columns { get; private set; }

        public ShowDataframeNode(ExpressionNode source, List<ExpressionNode> columns)
        {
            Source = source;
            Columns = columns;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitShowDataframe(this);
    }
    // For accessing fields like row.name
    // public class PropertyAccessNode : ExpressionNode
    // {
    //     public ExpressionNode Source { get; }
    //     public string IdField { get; }

    //     public PropertyAccessNode(ExpressionNode source, string propertyName)
    //     {
    //         Source = source;
    //         IdField = propertyName;
    //     }
    //     public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitPropertyAccess(this);
    // }

    // // For grabbing the 'Rows' array out of a Dataframe pointer
    // public class InternalDataframeFieldNode : ExpressionNode
    // {
    //     public ExpressionNode Source { get; }
    //     public int FieldIndex { get; } // 0=Cols, 1=Rows, 2=Types

    //     public InternalDataframeFieldNode(ExpressionNode source, int index, Type type)
    //     {
    //         Source = source;
    //         FieldIndex = index;
    //         this.Type = type;
    //     }
    //     public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitInternalDataframeField(this);
    // }

    public class TypeLiteralNode : ExpressionNode
    {
        public Type Value { get; }

        public TypeLiteralNode(Type value)
        {
            Value = value;
            Type = value; // The type of a type literal is the type itself
        }


        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitTypeLiteral(this);
    }

}

