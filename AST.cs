using System;
using System.Collections.Generic;

namespace MyCompiler
{
    // The base class for all nodes in your tree
    public abstract class Node { }

    // Intermediate category
    public abstract class ExpressionNode : Node { }
    public abstract class StatementNode : Node { }
    public abstract class FunctionNode : Node { }

    
    public class PrintNode : ExpressionNode {
        public ExpressionNode Expression { get; set; }
        public PrintNode(ExpressionNode expr)
        {
            Expression = expr;
        } 
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
    }

    // Represents a single number (e.g., 10)
    public class NumberNode : ExpressionNode {
        public int Value { get; set; }
        public NumberNode(int value) => Value = value;
    }

    // Represents a string (e.g., "Hello")
    public class StringNode : ExpressionNode {
        public string Value { get; set; }
        public StringNode(string value) => Value = value;
    }
    
    // Represents a variable name (e.g., x)
    public class IdNode : ExpressionNode {
        public string Name { get; set; }
        public IdNode(string name) => Name = name;
    }

    // Represents a math operation (e.g., 10 + 20)
    public class BinaryOpNode : ExpressionNode {
        public ExpressionNode Left { get; set; }
        public string Operator { get; set; }
        public ExpressionNode Right { get; set; }
        public BinaryOpNode(ExpressionNode left, string op, ExpressionNode right) {
            Left = left; Operator = op; Right = right;
        }
    }

    // Represents an assignment (e.g., x = 10)
    public class AssignNode : StatementNode {
        public string Id { get; set; }  // ID = expr  -->   x = 10 
        public ExpressionNode Expression { get; set; }
        public AssignNode(string id, ExpressionNode expr) {
            Id = id; Expression = expr;
        }
    }

    public class IncrementNode : StatementNode
    {
        public string Id { get; set; }
        public IncrementNode(string id)
        {
            Id = id;
        }
    }
    public class DecrementNode : StatementNode
    {
        public string Id { get; set; }
        public DecrementNode(string id)
        {
            Id = id;
        }
    }

    // A list of statements (the whole program)
    public class SequenceNode : Node {
        public List<Node> Statements { get; } = new List<Node>();
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
    }

    // Boolean
    public class BooleanNode : ExpressionNode
    {
        public bool Value { get; }

        public BooleanNode(bool value)
        {
            Value = value;
        }
    }
    public class ComparisonNode: ExpressionNode
    {
        public ExpressionNode Left;
        public string Operator { get; set; }

        public ExpressionNode Right;

        public ComparisonNode(ExpressionNode left, string op, ExpressionNode right)
        {
            Left = left; Operator = op; Right = right;
        }

    }

}
