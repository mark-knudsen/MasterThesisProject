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
    public abstract class NodeExpr
    {
        public abstract LLVMValueRef Accept(IExpressionVisitor visitor);
    }

    // Intermediate category
    public abstract class ExpressionNodeExpr : NodeExpr
    {
        public Type Type { get; protected set; }
        // Add this setter helper
        public void SetType(Type type) => Type = type;
    }

    public abstract class StatementNodeExpr : NodeExpr
    {
        public Type Type { get; protected set; }
        // Add this setter helper
        public void SetType(Type type) => Type = type;
    }

    //-----Built-in-function-nodes-----//

    // Represents print function
    public class PrintNodeExpr : StatementNodeExpr
    {
        public ExpressionNodeExpr Expression { get; set; }
        public PrintNodeExpr(ExpressionNodeExpr expr)
        {
            Expression = expr;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitPrintExpr(this);
    }

    // Represents Random function
    public class RandomNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr MinValue { get; set; }
        public ExpressionNodeExpr MaxValue { get; set; }

        public RandomNodeExpr(ExpressionNodeExpr minValue, ExpressionNodeExpr maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRandomExpr(this);
    }

    public class RoundNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr Value { get; }
        public ExpressionNodeExpr Decimals { get; }

        public RoundNodeExpr(ExpressionNodeExpr value, ExpressionNodeExpr decimals)
        {
            Value = value;
            Decimals = decimals;
            Type = new FloatType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRoundExpr(this);
    }

    //------Function-nodes------//
    public class FunctionDefNode : NodeExpr
    {
        public string Name { get; }
        public Type ReturnTypeName { get; set; } // "Int", "String", "Float", etc.
        public List<string> Parameters { get; set; }
        public NodeExpr Body { get; set; }

        public FunctionDefNode(string name, string returnType, List<string> parameters, NodeExpr body)
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

    public class FunctionCallNode : ExpressionNodeExpr
    {
        public string Name { get; }
        public List<ExpressionNodeExpr> Arguments { get; }

        public FunctionCallNode(string name, List<ExpressionNodeExpr> arguments)
        {
            Name = name;
            Arguments = arguments;
            // For now, we assume functions return Float to be safe with math
            Type = new FloatType();
        }

        // public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitFunctionCall(this);
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => throw new NotImplementedException();
    }

    public class FloatNodeExpr : ExpressionNodeExpr
    {
        public double Value { get; }
        public FloatNodeExpr(double value)
        {
            Value = value;
            Type = new FloatType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitFloatExpr(this);
    }
    // Represents a single number (e.g., 10)
    public class NumberNodeExpr : ExpressionNodeExpr
    {
        public int Value { get; set; }

        public NumberNodeExpr(int value)
        {
            Value = value;
            Type = new IntType();
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitNumberExpr(this);
    }

    // Represents a string (e.g., "Hello")
    public class StringNodeExpr : ExpressionNodeExpr
    {
        public string Value { get; set; }
        public StringNodeExpr(string value)
        {
            Value = value;
            Type = new StringType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitStringExpr(this);
    }

    // Boolean
    public class BooleanNodeExpr : ExpressionNodeExpr
    {
        public bool Value { get; }

        public BooleanNodeExpr(bool value)
        {
            Value = value;
            Type = new BoolType();
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitBooleanExpr(this);
    }

    public class NullNodeExpr : ExpressionNodeExpr
    {
        public NullNodeExpr()
        {
            Type = new NullType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitNullExpr(this);
    }

    // Represents a variable name (e.g., x)
    public class IdNodeExpr : ExpressionNodeExpr
    {
        public string Name { get; set; }
        public IdNodeExpr(string name) => Name = name;
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIdExpr(this);
    }

    // Represents a math operation (e.g., 10 + 20)
    public class BinaryOpNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr Left { get; set; }
        public string Operator { get; set; }
        public ExpressionNodeExpr Right { get; set; }
        public BinaryOpNodeExpr(ExpressionNodeExpr left, string op, ExpressionNodeExpr right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitBinaryExpr(this);
    }

    public class LogicalOpNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr Left { get; set; }
        public string Operator { get; set; }
        public ExpressionNodeExpr Right { get; set; }
        public LogicalOpNodeExpr(ExpressionNodeExpr left, string op, ExpressionNodeExpr right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitLogicalOpExpr(this);
    }

    // Represents an assignment (e.g., x = 10)
    public class AssignNodeExpr : StatementNodeExpr
    {
        public string Id { get; set; }  // ID = expr  -->   x = 10 
        public ExpressionNodeExpr Expression { get; set; }
        public AssignNodeExpr(string id, ExpressionNodeExpr expr)
        {
            Id = id; Expression = expr;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitAssignExpr(this);
    }

    public class IncrementNodeExpr : StatementNodeExpr
    {
        public string Id { get; set; }
        public IncrementNodeExpr(string id)
        {
            Id = id;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIncrementExpr(this);
    }
    public class DecrementNodeExpr : StatementNodeExpr
    {
        public string Id { get; set; }
        public DecrementNodeExpr(string id)
        {
            Id = id;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitDecrementExpr(this);
    }

    // A list of statements (the whole program)
    public class SequenceNodeExpr : NodeExpr
    {
        public List<NodeExpr> Statements { get; } = new List<NodeExpr>();

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitSequenceExpr(this);
    }

    // IF statement
    public class IfNodeExpr : StatementNodeExpr
    {
        public ExpressionNodeExpr Condition;
        public NodeExpr ThenPart;
        public NodeExpr ElsePart;

        public IfNodeExpr(ExpressionNodeExpr cond, NodeExpr thenP, NodeExpr elseP = null)
        {
            Condition = cond;
            ThenPart = thenP;
            ElsePart = elseP;
            Type = new VoidType();
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIfExpr(this);
    }

    public class ForLoopNodeExpr : StatementNodeExpr
    {
        public StatementNodeExpr Initialization;   // for(x=0;x<5;x++)x

        public ExpressionNodeExpr Condition;
        public StatementNodeExpr Step;
        public NodeExpr Body; // Changed from ExpressionNodeExpr

        public ForLoopNodeExpr(StatementNodeExpr init, ExpressionNodeExpr cond, StatementNodeExpr step, NodeExpr body)
        {
            Initialization = init;
            Condition = cond;
            Step = step;
            Body = body;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitForLoopExpr(this);
    }

    public class ForEachLoopNodeExpr : StatementNodeExpr
    {
        public IdNodeExpr Iterator { get; }      // e.g., "item"
        public ExpressionNodeExpr Array { get; } // e.g., "arr"
        public NodeExpr Body { get; }

        public ForEachLoopNodeExpr(IdNodeExpr iterator, ExpressionNodeExpr array, NodeExpr body)
        {
            Iterator = iterator;
            Array = array;
            Body = body;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitForEachLoopExpr(this);
    }

    public class ComparisonNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr Left;
        public string Operator { get; set; }

        public ExpressionNodeExpr Right;

        public ComparisonNodeExpr(ExpressionNodeExpr left, string op, ExpressionNodeExpr right)
        {
            Left = left;
            Operator = op;
            Right = right;
            Type = new BoolType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitComparisonExpr(this);
    }

    public class ArrayNodeExpr : ExpressionNodeExpr
    {
        public List<ExpressionNodeExpr> Elements { get; }
        public Type ElementType { get; set; }

        public ArrayNodeExpr(List<ExpressionNodeExpr> elements)
        {
            Elements = elements;

            if (elements.Count > 0)
                ElementType = elements[0].Type; // infer type from first element
            else
                ElementType = new IntType();

            Type = new ArrayType(ElementType);
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitArrayExpr(this);
    }

    public class CopyArrayNodeExpr : CopyNodeExpr
    {
        public CopyArrayNodeExpr(ExpressionNodeExpr source) : base(source) { }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitCopyExpr(this);
    }

    public class IndexNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr ArrayExpression { get; }
        public ExpressionNodeExpr IndexExpression { get; }

        public IndexNodeExpr(ExpressionNodeExpr arrayExpr, ExpressionNodeExpr indexExpr)
        {
            ArrayExpression = arrayExpr;
            IndexExpression = indexExpr;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIndexExpr(this);
    }

    public class IndexAssignNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr ArrayExpression { get; }
        public ExpressionNodeExpr IndexExpression { get; }
        public ExpressionNodeExpr AssignExpression { get; }

        public IndexAssignNodeExpr(ExpressionNodeExpr arrayExpr, ExpressionNodeExpr indexExpr, ExpressionNodeExpr assignExpression)
        {
            ArrayExpression = arrayExpr;
            IndexExpression = indexExpr;
            AssignExpression = assignExpression;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIndexAssignExpr(this);
    }

    public class WhereNodeExpr : ExpressionNodeExpr
    {
        public IdNodeExpr IteratorId;
        public ExpressionNodeExpr ArrayExpr;
        public ExpressionNodeExpr Condition;

        public WhereNodeExpr(IdNodeExpr iteratorId, ExpressionNodeExpr arrayExpr, ExpressionNodeExpr condition)
        {
            IteratorId = iteratorId;
            ArrayExpr = arrayExpr;
            Condition = condition;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitWhereExpr(this);
    }

    public class MapNodeExpr : ExpressionNodeExpr
    {
        public IdNodeExpr IteratorId;

        public ExpressionNodeExpr ArrayExpr;
        public ExpressionNodeExpr Assignment;

        public MapNodeExpr(IdNodeExpr iteratorId, ExpressionNodeExpr arrayExpr, ExpressionNodeExpr assignment)
        {
            IteratorId = iteratorId;
            ArrayExpr = arrayExpr;
            Assignment = assignment;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMapExpr(this);
    }

    public class ReadCsvNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr FileNameExpr { get; set; }

        // Change this to only take ONE parameter
        public ReadCsvNodeExpr(ExpressionNodeExpr fileNameExpr)
        {
            FileNameExpr = fileNameExpr;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor)
            => visitor.VisitReadCsvExpr(this);
    }


    public class ToCsvNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr Expression { get; set; }   // e.g., variable or object
        public ExpressionNodeExpr FileNameExpr { get; set; } // expression producing string

        public ToCsvNodeExpr(ExpressionNodeExpr expr, ExpressionNodeExpr fileNameExpr)
        {
            Expression = expr;
            FileNameExpr = fileNameExpr;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitToCsvExpr(this);
    }

    public class AddNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr ArrayExpression { get; }
        public ExpressionNodeExpr AddExpression { get; }

        public AddNodeExpr(ExpressionNodeExpr arrayExpr, ExpressionNodeExpr addExpression)
        {
            ArrayExpression = arrayExpr;
            AddExpression = addExpression;
            Type = new ArrayType(ArrayExpression.Type);
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitAddExpr(this);
    }
    public class AddRangeNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr ArrayExpression { get; }
        public ExpressionNodeExpr AddRangeExpression { get; }

        public AddRangeNodeExpr(ExpressionNodeExpr arrayExpr, ExpressionNodeExpr addRangeExpression)
        {
            ArrayExpression = arrayExpr;
            AddRangeExpression = addRangeExpression;
            Type = new ArrayType(ArrayExpression.Type);
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitAddRangeExpr(this);
    }

    public class RemoveNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr ArrayExpression { get; }
        public ExpressionNodeExpr RemoveExpression { get; }

        public RemoveNodeExpr(ExpressionNodeExpr arrayExpr, ExpressionNodeExpr removeExpression)
        {
            ArrayExpression = arrayExpr;
            RemoveExpression = removeExpression;
            Type = new ArrayType(ArrayExpression.Type);
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRemoveExpr(this);
    }

    public class RemoveRangeNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr ArrayExpression { get; }
        public ExpressionNodeExpr RemoveRangeExpression { get; }

        public RemoveRangeNodeExpr(ExpressionNodeExpr arrayExpr, ExpressionNodeExpr removeRangeExpression)
        {
            ArrayExpression = arrayExpr;
            RemoveRangeExpression = removeRangeExpression;
            Type = new ArrayType(ArrayExpression.Type);
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRemoveRangeExpr(this);
    }

    public class LengthNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr ArrayExpression { get; }

        public LengthNodeExpr(ExpressionNodeExpr arrayExpr)
        {
            ArrayExpression = arrayExpr;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitLengthExpr(this);
    }

    public class MinNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr ArrayExpression { get; }

        public MinNodeExpr(ExpressionNodeExpr arrayExpr)
        {
            ArrayExpression = arrayExpr;
            Type = new FloatType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMinExpr(this);
    }

    public class MaxNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr ArrayExpression { get; }

        public MaxNodeExpr(ExpressionNodeExpr arrayExpr)
        {
            ArrayExpression = arrayExpr;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMaxExpr(this);
    }

    public class MeanNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr ArrayExpression { get; }

        public MeanNodeExpr(ExpressionNodeExpr arrayExpr)
        {
            ArrayExpression = arrayExpr;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitMeanExpr(this);
    }

    public class SumNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr ArrayExpression { get; }

        public SumNodeExpr(ExpressionNodeExpr arrayExpr)
        {
            ArrayExpression = arrayExpr;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitSumExpr(this);
    }

    public class UnaryOpNodeExpr : ExpressionNodeExpr
    {
        public string Operator { get; }
        public ExpressionNodeExpr Operand { get; }

        public UnaryOpNodeExpr(string operatorSymbol, ExpressionNodeExpr operand)
        {
            Operator = operatorSymbol;
            Operand = operand;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitUnaryOpExpr(this);
    }

    public class RecordField
    {
        public string Label { get; set; }
        public ExpressionNodeExpr Value { get; set; }
    }

    public class RecordNodeExpr : ExpressionNodeExpr
    {
        public List<RecordField> Fields { get; } = new List<RecordField>();
        public List<Type> ElementTypes { get; } = new List<Type>();

        public RecordNodeExpr(List<NamedArgumentNodeExpr> valuesArray)
        {
            // 1. Cast the inputs to ArrayNodeExpr to get into their internal lists
            var labelNodes = valuesArray.Select(v => new StringNodeExpr(v.Name)).ToList();
            var valueNodes = valuesArray;

            if (labelNodes == null || valueNodes == null)
                throw new Exception("Record requires two arrays: labels and values.");

            if (labelNodes.Count != valueNodes.Count)
                throw new Exception("Record labels and values count mismatch.");

            // 2. Zip them together into the Fields list
            for (int i = 0; i < labelNodes.Count; i++)
            {
                // Ensure the label is actually a StringNodeExpr
                if (labelNodes[i] is StringNodeExpr strNode)
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
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRecordExpr(this);
    }

    public class RecordFieldNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr IdRecord { get; }
        public string IdField { get; }

        public RecordFieldNodeExpr(ExpressionNodeExpr idRecord, string idField)
        {
            Console.WriteLine("record constructor: " + idRecord);
            IdRecord = idRecord;
            IdField = idField;
            Type = new IntType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRecordFieldExpr(this);
    }

    public class RecordFieldAssignNodeExpr : StatementNodeExpr
    {
        public ExpressionNodeExpr IdRecord { get; }
        public string IdField { get; }
        public ExpressionNodeExpr AssignExpression { get; }

        public RecordFieldAssignNodeExpr(ExpressionNodeExpr idRecord, string idField, ExpressionNodeExpr assignExpression)
        {
            IdRecord = idRecord;
            IdField = idField;
            AssignExpression = assignExpression;
            Type = new VoidType();
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRecordFieldAssignExpr(this);
    }

    public class CopyRecordNodeExpr : CopyNodeExpr
    {
        public CopyRecordNodeExpr(ExpressionNodeExpr source) : base(source) { }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitCopyRecordExpr(this);
    }

    public class CopyNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr Source { get; }

        public CopyNodeExpr(ExpressionNodeExpr source)
        {
            Source = source;
            Type = source.Type;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitCopyExpr(this);
    }

    public class AddFieldNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr Record { get; }
        public string FieldName { get; }
        public ExpressionNodeExpr Value { get; }

        public AddFieldNodeExpr(ExpressionNodeExpr record, string fieldName, ExpressionNodeExpr value)
        {
            Record = record;
            FieldName = fieldName;
            Value = value;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitAddFieldExpr(this);
    }

    public class RemoveFieldNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr Record { get; }
        public string FieldName { get; }

        public RemoveFieldNodeExpr(ExpressionNodeExpr record, string fieldName)
        {
            Record = record;
            FieldName = fieldName;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitRemoveFieldExpr(this);
    }


    public class NamedArgumentNodeExpr : ExpressionNodeExpr
    {
        public string Name { get; }
        public ExpressionNodeExpr Value { get; }

        public NamedArgumentNodeExpr(string name, ExpressionNodeExpr value)
        {
            Name = name;
            Value = value;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitNamedArgumentExpr(this);
    }

    public class DataframeNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr Columns { get; private set; }
        public ExpressionNodeExpr DataPointers { get; private set; }
        public ExpressionNodeExpr DataTypes { get; private set; }

        public DataframeNodeExpr(List<ExpressionNodeExpr> args)
        {
            foreach (var arg in args)
            {
                if (arg is NamedArgumentNodeExpr named)
                {
                    switch (named.Name)
                    {
                        case "columns":
                            if (Columns != null) throw new Exception("columns assigned twice");
                            Columns = named.Value;
                            break;
                        case "data":
                            if (DataPointers != null) throw new Exception("data assigned twice");
                            DataPointers = named.Value;
                            break;
                        default:
                            throw new Exception($"Unknown argument {named.Name}");
                    }
                }
                else
                {
                    if (Columns == null) Columns = arg;
                    else if (DataPointers == null) DataPointers = arg;
                    else throw new Exception("Too many positional arguments for dataframe");
                }
            }

            if (Columns == null || DataPointers == null)
                throw new Exception("Missing required dataframe arguments");

            Type = CreateDataframe();
        }

        private DataframeType CreateDataframe()
        {
            var columns = (Columns as ArrayNodeExpr)?.Elements.OfType<StringNodeExpr>().Select(s => s.Value).ToList();
            List<List<object>> dataPointers = (DataPointers as ArrayNodeExpr)?.Elements.OfType<ArrayNodeExpr>().Select(arr => arr.Elements.Select(value =>
            {
                if (value is NumberNodeExpr num) return (object)num.Value;
                if (value is FloatNodeExpr flt) return (object)flt.Value;
                if (value is StringNodeExpr str) return (object)str.Value;
                if (value is BooleanNodeExpr boolean) return (object)boolean.Value;
                throw new Exception("Unsupported dataframe data value type.");
            }).ToList()).ToList();

            var dataTypes = (DataTypes as ArrayNodeExpr)?.Elements.Select(dt =>
            {
                if (dt is StringNodeExpr) return new StringType() as Type;
                if (dt is NumberNodeExpr) return new IntType() as Type;
                if (dt is FloatNodeExpr) return new FloatType() as Type;
                if (dt is BooleanNodeExpr) return new BoolType() as Type;
                throw new Exception("Unsupported data type in dataframe.");
            }).ToList();

            if (dataTypes == null)
            {
                var firstRow = (DataPointers as ArrayNodeExpr)?.Elements.OfType<ArrayNodeExpr>().FirstOrDefault();
                if (firstRow != null)
                {
                    var inferred = firstRow.Elements.Select(value =>
                    {
                        if (value is StringNodeExpr) return new StringType() as Type;
                        if (value is NumberNodeExpr) return new IntType() as Type;
                        if (value is FloatNodeExpr) return new FloatType() as Type;
                        if (value is BooleanNodeExpr) return new BoolType() as Type;
                        throw new Exception("Unsupported dataframe data value type.");
                    }).ToList();

                    dataTypes = inferred;
                    DataTypes = new ArrayNodeExpr(
                         inferred
                             .Select(type => new StringNodeExpr(type.ToString()))
                             .Cast<ExpressionNodeExpr>()
                             .ToList()
                     );
                }
            }

            if (columns == null || dataPointers == null || dataTypes == null)
                throw new Exception("Invalid dataframe definition.");

            return new DataframeType(columns, dataPointers, dataTypes);
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitDataframeExpr(this);
    }

    public class ShowDataframeNodeExpr : ExpressionNodeExpr
    {
        public ExpressionNodeExpr Source { get; private set; }
        public List<ExpressionNodeExpr> Columns { get; private set; }

        public ShowDataframeNodeExpr(ExpressionNodeExpr source, List<ExpressionNodeExpr> columns)
        {
            Source = source;
            Columns = columns;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitShowDataframeExpr(this);
    }
}

