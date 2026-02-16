using System;
using System.Collections.Generic;
using LLVMSharp.Interop;
using LLVMSharp;

namespace MyCompiler
{
    public enum MyType
    {
        Int,
        String,
        None
    }
    // The base class for all NodeExprs in your tree
    public abstract class NodeExpr
    {
        public abstract LLVMValueRef Accept(IExpressionVisitor visitor);
    }

    // Intermediate category
    public abstract class ExpressionNodeExpr : NodeExpr
    {
        public MyType Type { get; protected set; }
    }

    public abstract class StatementNodeExpr : NodeExpr { }
    public abstract class FunctionNodeExpr : NodeExpr { }
    
    public class PrintNodeExpr : ExpressionNodeExpr {
        public ExpressionNodeExpr Expression { get; set; }
        public PrintNodeExpr(ExpressionNodeExpr expr)
        {
            Expression = expr;
        } 
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => throw new NotImplementedException();
    }

    // Represents Random function
    public class RandomNodeExpr : FunctionNodeExpr {
        public ExpressionNodeExpr MinValue { get; set; }
        public ExpressionNodeExpr MaxValue { get; set; }

        public RandomNodeExpr(ExpressionNodeExpr minValue, ExpressionNodeExpr maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        } 
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => throw new NotImplementedException();
    }

    // Represents a single number (e.g., 10)
    public class NumberNodeExpr : ExpressionNodeExpr
    {
        public int Value { get; set; }

        public NumberNodeExpr(int value)
        {
            Value = value;
            Type = MyType.Int;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitNumberExpr(this);
    }

    // Represents a string (e.g., "Hello")
    public class StringNodeExpr : ExpressionNodeExpr {
        public string Value { get; set; }
        public StringNodeExpr(string value)
        {
            Value = value;
            Type = MyType.String;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitStringExpr(this);
    }

    // Represents a variable name (e.g., x)
    public class IdNodeExpr : ExpressionNodeExpr {
        public string Name { get; set; }
        public IdNodeExpr(string name) => Name = name;
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitIdExpr(this);
    }

    // Represents a math operation (e.g., 10 + 20)
    public class BinaryOpNodeExpr : ExpressionNodeExpr {
        public ExpressionNodeExpr Left { get; set; }
        public string Operator { get; set; }
        public ExpressionNodeExpr Right { get; set; }
        public BinaryOpNodeExpr(ExpressionNodeExpr left, string op, ExpressionNodeExpr right) {
            Left = left; Operator = op; Right = right;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => visitor.VisitBinaryExpr(this);
    }

    // Represents an assignment (e.g., x = 10)
    public class AssignNodeExpr : StatementNodeExpr {
        public string Id { get; set; }  // ID = expr  -->   x = 10 
        public ExpressionNodeExpr Expression { get; set; }
        public AssignNodeExpr(string id, ExpressionNodeExpr expr) {
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
    public class IfNodeExpr : StatementNodeExpr {
        public ExpressionNodeExpr Condition;
        public NodeExpr ThenPart;
        public NodeExpr ElsePart; // Can be null
        
        public IfNodeExpr(ExpressionNodeExpr cond, NodeExpr thenP, NodeExpr elseP = null) {
            Condition = cond;
            ThenPart = thenP;
            ElsePart = elseP;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => throw new NotImplementedException();
    }

    public class ForLoopNodeExpr : StatementNodeExpr
    {
        public StatementNodeExpr Initialization;   // for(x=0;x<5;x++)x

        public ExpressionNodeExpr Condition;
        public StatementNodeExpr Step;
        public ExpressionNodeExpr Body;

        public ForLoopNodeExpr(StatementNodeExpr init, ExpressionNodeExpr cond, StatementNodeExpr step, ExpressionNodeExpr body)
        {
            Initialization = init;
            Condition = cond;
            Step = step;    
            Body = body;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => throw new NotImplementedException();
    }

    // Boolean
    public class BooleanNodeExpr : ExpressionNodeExpr
    {
        public bool Value { get; }

        public BooleanNodeExpr(bool value)
        {
            Value = value;
        }
        public override LLVMValueRef Accept(IExpressionVisitor visitor) => throw new NotImplementedException();
    }
    public class ComparisonNodeExpr: ExpressionNodeExpr
    {
        public ExpressionNodeExpr Left;
        public string Operator { get; set; }

        public ExpressionNodeExpr Right;

        public ComparisonNodeExpr(ExpressionNodeExpr left, string op, ExpressionNodeExpr right)
        {
            Left = left; Operator = op; Right = right;
        }

        public override LLVMValueRef Accept(IExpressionVisitor visitor) => throw new NotImplementedException();
    }

}
