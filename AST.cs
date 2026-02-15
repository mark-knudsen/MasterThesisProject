using System;
using System.Collections.Generic;

namespace MyCompiler
{
    public interface IVisitor<T>
    {
        T Visit(SequenceNode node);
        T Visit(NumberNode node);
        T Visit(StringNode node);
        T Visit(IdNode node);
        T Visit(BinaryOpNode node);
        T Visit(AssignNode node);
        T Visit(IfNode node);
        T Visit(ForLoopNode node);
        T Visit(BooleanNode node);
        T Visit(ComparisonNode node);
        T Visit(PrintNode node);
        T Visit(IncrementNode node);
        T Visit(DecrementNode node);
        T Visit(RandomNode node);
    }

    public abstract class Node 
    { 
        public abstract T Accept<T>(IVisitor<T> visitor);
    }
    public abstract class ExpressionNode : Node { }
    public abstract class StatementNode : Node { }
    public abstract class FunctionNode : Node { }

    // Example of how to update one node (Do this for ALL nodes):
    public class NumberNode : ExpressionNode {
        public int Value { get; set; }
        public NumberNode(int value) => Value = value;
        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }


    
    public class PrintNode : ExpressionNode {
        public ExpressionNode Expression { get; set; }
        public PrintNode(ExpressionNode expr)
        {
            Expression = expr;
        } 
        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    // Represents Random function
    public class RandomNode : FunctionNode {
        public ExpressionNode MinValue { get; set; }
        public ExpressionNode MaxValue { get; set; }

        public RandomNode(ExpressionNode minValue, ExpressionNode maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        } 
        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }


    // Represents a string (e.g., "Hello")
    public class StringNode : ExpressionNode {
        public string Value { get; set; }
        public StringNode(string value) => Value = value;
        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    // Represents a variable name (e.g., x)
    public class IdNode : ExpressionNode {
        public string Name { get; set; }
        public IdNode(string name) => Name = name;
        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    // Represents a math operation (e.g., 10 + 20)
    public class BinaryOpNode : ExpressionNode {
        public ExpressionNode Left { get; set; }
        public string Operator { get; set; }
        public ExpressionNode Right { get; set; }
        public BinaryOpNode(ExpressionNode left, string op, ExpressionNode right) {
            Left = left; Operator = op; Right = right;
        }
        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    // Represents an assignment (e.g., x = 10)
    public class AssignNode : StatementNode {
        public string Id { get; set; }  // ID = expr  -->   x = 10 
        public ExpressionNode Expression { get; set; }
        public AssignNode(string id, ExpressionNode expr) {
            Id = id; Expression = expr;
        }
        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class IncrementNode : StatementNode
    {
        public string Id { get; set; }
        public IncrementNode(string id) => Id = id;
        
        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class DecrementNode : StatementNode
    {
        public string Id { get; set; }
        public DecrementNode(string id) => Id = id;
        
        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    // A list of statements (the whole program)
    public class SequenceNode : Node {
        public List<Node> Statements { get; } = new List<Node>();
        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    // IF statement
    public class IfNode : StatementNode {
        public ExpressionNode Condition;
        public Node ThenPart;
        public Node ElsePart; // Can be null
        
        public IfNode(ExpressionNode cond, Node thenP, Node elseP = null) {
            Condition = cond;
            ThenPart = thenP;
            ElsePart = elseP;
        }
        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    public class ForLoopNode : StatementNode
    {
        public StatementNode Initialization;   // for(x=0;x<5;x++)x

        public ExpressionNode Condition;
        public StatementNode Step;
        public ExpressionNode Body;

        public ForLoopNode(StatementNode init, ExpressionNode cond, StatementNode step, ExpressionNode body)
        {
            Initialization = init;
            Condition = cond;
            Step = step;    
            Body = body;
        }
        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

    // Boolean
    public class BooleanNode : ExpressionNode
    {
        public bool Value { get; }

        public BooleanNode(bool value) => Value = value;
        
        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }
    public class ComparisonNode: ExpressionNode
    {
        public ExpressionNode Left;
        public string Operator { get; set; }

        public ExpressionNode Right;

        public ComparisonNode(ExpressionNode left, string op, ExpressionNode right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
        public override T Accept<T>(IVisitor<T> visitor) => visitor.Visit(this);
    }

}
