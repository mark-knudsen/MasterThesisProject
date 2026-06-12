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
        public List<ExpressionNode> Arguments { get; }
        public ExpressionNode MinValue { get; }
        public ExpressionNode MaxValue { get; }
        public ExpressionNode Decimals { get; set; }
        public RandomNode(List<ExpressionNode> args)
        {
            Arguments = args;
            MinValue = Arguments[0];
            MaxValue = Arguments[1];
            Decimals = Arguments.Count > 2 ? Arguments[2] : null;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRandom(this);
    }

    public class RoundNode : ExpressionNode
    {
        public ExpressionNode Value { get; set; }
        public ExpressionNode Decimals { get; set; }

        public RoundNode(List<ExpressionNode> args)
        {
            Value = args[0];
            Decimals = args.Count > 1 ? args[1] : new NumberNode(0);
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRound(this);
    }

    public class FloatNode : ExpressionNode
    {
        public double Value { get; }
        public FloatNode(double value)
        {
            Value = value;
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
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitBoolean(this);
    }

    public class NullNode : ExpressionNode
    {
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
        public ExpressionNode Expression { get; set; }
        public AssignNode(string id, ExpressionNode expr)
        {
            Id = id;
            Expression = expr;
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
        public List<Node> Nodes { get; } = new List<Node>();

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitSequence(this);
    }

    // IF statement
    public class IfNode : StatementNode
    {
        public ExpressionNode Condition { get; }
        public Node ThenPart { get; }
        public Node ElsePart { get; }
        public IfNode(ExpressionNode cond, Node thenPart, Node elsePart = null)
        {
            Condition = cond;
            ThenPart = thenPart;
            ElsePart = elsePart;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIf(this);
    }

    public class ForLoopNode : StatementNode
    {
        public StatementNode Initialization { get; }   // for(x=0;x<5;x++)
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
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitComparison(this);
    }

    public class ArrayNode : ExpressionNode
    {
        public List<ExpressionNode> Elements { get; }
        public Type ElementType { get; set; }
        public ulong? Capacity;
        public ArrayNode(List<ExpressionNode> elements, TypeNode typeNode = null)
        {
            Elements = elements;
            ElementType = TypeChecker.ResolveTypeNode(typeNode);
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitArray(this);
    }

    public class IndexNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }
        public ExpressionNode IndexExpression { get; }
        // NEW FLAG
        public bool SkipBoundsCheck { get; set; } = false;

        public IndexNode(ExpressionNode sourceExpr, ExpressionNode indexExpr)
        {
            SourceExpression = sourceExpr;
            IndexExpression = indexExpr;
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
        public ExpressionNode TransformExpr { get; }

        public MapNode(IdNode iteratorId, ExpressionNode sourceExpr, ExpressionNode transformExpr)
        {
            IteratorId = iteratorId;
            SourceExpr = sourceExpr;

            if (transformExpr is ArrayNode arrayNode)
            {
                List<FieldNode> namedArguments = new List<FieldNode>();
                foreach (var arr in arrayNode.Elements)
                {
                    if (arr is IdNode strNode)
                    {
                        namedArguments.Add(new FieldNode(
                            new FieldNode(iteratorId, strNode.Name),
                            strNode.Name));
                        /* OLD way before changing to fieldnode!  
                            namedArguments.Add(new NamedArgumentNode(
                                strNode.Name,
                                new FieldNode(iteratorId, strNode.Name)
                            )); 
                        */
                    }
                    else
                        throw new Exception("Map array elements must be identifiers");
                }

                TransformExpr = new RecordNode(namedArguments);   //  { name, age, iscool}

            }
            else
                TransformExpr = transformExpr;                 //  {name= x.name, age= x.age, iscool= x.iscool}
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor)
            => visitor.VisitMap(this);
    }
    public class ReadCsvNode : ExpressionNode
    {
        public ExpressionNode FileNameExpr { get; }
        public NamedArgumentNode SchemaExpr { get; set; }

        public ReadCsvNode(List<Node> args)
        {
            // Find the actual string literal (e.g., "test.csv")
            FileNameExpr = args.FirstOrDefault(a => a is StringNode) as ExpressionNode;

            // Find the record/schema ([index: int...])
            SchemaExpr = args.FirstOrDefault(a => a is NamedArgumentNode) as NamedArgumentNode;

            // If the parser didn't find them by type, fallback to positions
            if (FileNameExpr == null && args.Count >= 2)
            {
                FileNameExpr = args[1] as ExpressionNode;
                SchemaExpr = args[0] as NamedArgumentNode;
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
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRemoveRange(this);
    }

    public class LengthNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public LengthNode(ExpressionNode arrayExpr)
        {
            ArrayExpression = arrayExpr;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitLength(this);
    }

    public class MinNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public MinNode(ExpressionNode arrayExpr)
        {
            ArrayExpression = arrayExpr;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMin(this);
    }

    public class MaxNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public MaxNode(ExpressionNode arrayExpr)
        {
            ArrayExpression = arrayExpr;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMax(this);
    }

    public class MeanNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public MeanNode(ExpressionNode arrayExpr)
        {
            ArrayExpression = arrayExpr;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMean(this);
    }

    public class SumNode : ExpressionNode
    {
        public ExpressionNode ArrayExpression { get; }

        public SumNode(ExpressionNode arrayExpr)
        {
            ArrayExpression = arrayExpr;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitSum(this);
    }

    public class CorrelationNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }
        public ExpressionNode TargetExpression { get; }

        public CorrelationNode(ExpressionNode sourceExpr, ExpressionNode targetExpr)
        {
            SourceExpression = sourceExpr;
            TargetExpression = targetExpr;
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
        public List<RecordField> Fields { get; set; }
        public RecordNode(List<FieldNode> valuesArray)
        {
            Fields = new List<RecordField>();
            if (valuesArray == null) return;

            for (int i = 0; i < valuesArray.Count; i++)
            {
                var arg = valuesArray[i];
                // For rows like {"Alice", 25}, arg.Name is null.
                // We assign a placeholder so the field object isn't broken.
                string label = arg.IdField ?? $"item{i + 1}";

                Fields.Add(new RecordField
                {
                    Label = label,
                    Value = arg.SourceExpression
                });
            }
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
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRecordFieldAssign(this);
    }

    public class CopyNode : ExpressionNode
    {
        public ExpressionNode SourceExpression { get; }

        public CopyNode(ExpressionNode source)
        {
            SourceExpression = source;
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

    public class DataframeNode : ExpressionNode
    {
        // The raw arguments passed directly from the parser
        public List<NamedArgumentNode> Arguments { get; }

        // These become semantic metadata fields populated exclusively by the Typechecker
        public RecordNode Schema { get; internal set; }
        public ArrayNode Columns { get; internal set; }
        public ArrayNode Rows { get; internal set; }
        public ArrayNode Types { get; internal set; }

        public DataframeNode(List<NamedArgumentNode> args)
        {
            Arguments = args;
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

    public class SliceNode : ExpressionNode
    {
        public ExpressionNode Source { get; }
        public ExpressionNode Start { get; } // Can be null for [:20]
        public ExpressionNode End { get; }   // Can be null for [10:]

        public SliceNode(ExpressionNode source, ExpressionNode start, ExpressionNode end)
        {
            Source = source;
            Start = start;
            End = end;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitSlice(this);
    }
}
