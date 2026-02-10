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

    // Boolean
    public class BooleanNode : ExpressionNode
    {
        public bool Value { get; }

        public BooleanNode(bool value)
        {
            Value = value;
        }

        // Optional: Useful for debugging
        //public override string ToString() => Value.ToString().ToLower();
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
