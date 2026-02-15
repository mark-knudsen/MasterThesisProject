using System;
using System.Collections.Generic;

namespace MyCompiler
{
    public class Interpreter : IVisitor<object>
    {
        private Dictionary<string, object> variables = new Dictionary<string, object>();

        // The entry point is now much cleaner
        public object Evaluate(Node node)
        {
            if (node == null) return null;
            return node.Accept(this);
        }

        public object Visit(SequenceNode node)
        {
            object result = null;
            foreach (var stmt in node.Statements)
            {
                result = stmt.Accept(this);
            }
            return result;
        }

        public object Visit(NumberNode node) => node.Value;

        public object Visit(StringNode node) => node.Value;

        public object Visit(BooleanNode node) => node.Value;

        public object Visit(IdNode node)
        {
            return variables.ContainsKey(node.Name) ? variables[node.Name] : "undefined";
        }

        public object Visit(AssignNode node)
        {
            var value = node.Expression.Accept(this);
            variables[node.Id] = value;
            return value;
        }

        public object Visit(BinaryOpNode node)
        {
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);

            if (left is int lInt && right is int rInt)
            {
                return node.Operator switch
                {
                    "+" => lInt + rInt,
                    "-" => lInt - rInt,
                    "*" => lInt * rInt,
                    "/" => lInt / rInt,
                    _ => "Invalid operator!"
                };
            }
            
            // String concatenation logic
            if (node.Operator == "+")
            {
                return left.ToString() + right.ToString();
            }

            return "Invalid binary expression!";
        }

        public object Visit(ComparisonNode node)
        {
            var left = node.Left.Accept(this);
            var right = node.Right.Accept(this);

            if (left is int l && right is int r)
            {
                return node.Operator switch
                {
                    "==" => l == r,
                    "!=" => l != r,
                    "<=" => l <= r,
                    ">=" => l >= r,
                    "<" => l < r,
                    ">" => l > r,
                    _ => "Invalid numeric operator!"
                };
            }

            if (left is string || right is string || left is bool)
            {
                return node.Operator switch
                {
                    "==" => left.ToString() == right.ToString(),
                    "!=" => left.ToString() != right.ToString(),
                    _ => "Invalid comparison operator!"
                };
            }

            return "Invalid comparison expression!";
        }

        public object Visit(IfNode node)
        {
            var condition = node.Condition.Accept(this);
            if (condition is bool b)
            {
                if (b) return node.ThenPart.Accept(this);
                else return node.ElsePart?.Accept(this);
            }
            return "Invalid condition!";
        }

        public object Visit(ForLoopNode node)
        {
            object result = null;
            // Initialization
            node.Initialization.Accept(this);

            // Condition, Step, Body
            while (true)
            {
                var condition = node.Condition.Accept(this);
                if (condition is bool b && b)
                {
                    result = node.Body.Accept(this);
                    node.Step.Accept(this);
                }
                else break;
            }
            return result;
        }

        public object Visit(IncrementNode node)
        {
            if (variables.ContainsKey(node.Id) && variables[node.Id] is int val)
            {
                variables[node.Id] = val + 1;
                return variables[node.Id];
            }
            return "Invalid increment operation!";
        }

        public object Visit(DecrementNode node)
        {
            if (variables.ContainsKey(node.Id) && variables[node.Id] is int val)
            {
                variables[node.Id] = val - 1;
                return variables[node.Id];
            }
            return "Invalid decrement operation!";
        }

        public object Visit(PrintNode node)
        {
            var val = node.Expression.Accept(this);
            Console.WriteLine(val?.ToString());
            return val;
        }

        public object Visit(RandomNode node)
        {
            var min = node.MinValue.Accept(this);
            var max = node.MaxValue.Accept(this);

            if (min is int minVal && max is int maxVal)
            {
                return new Random().Next(minVal, maxVal);
            }
            return "Invalid random arguments!";
        }
    }
}