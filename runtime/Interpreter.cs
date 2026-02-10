
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MyCompiler
{

    public class Interpreter
    {
        private object eval_numaric_binary_expr(int left, int right, string _operator){
            switch (_operator)
            {
                case "+":
                    return left + right;
                case "-":
                    return left - right;
                case "*":
                    return left * right;
                case "/":
                    return left / right;
                    default:
                        return "Invalid operator!";
            }
        }

          private string eval_string_binary_expr(string left, string right, string _operator){
            switch (_operator)
            {
                case "+": 
                    return left + right;
                    default:
                        return "Invalid operator!";
            }
        }

        private object eval_binary_expr(BinaryOpNode binaryOpNode)
        {
            var left = Evaluate(binaryOpNode.Left);
            var right = Evaluate(binaryOpNode.Right);
            //Console.WriteLine("left: " +left);
            //Console.WriteLine("binaryOpNode.left: " + binaryOpNode.Left);
            
            if (left is int && right is int)
                return eval_numaric_binary_expr((int)left, (int)right, binaryOpNode.Operator);
            else if (left is string && right is string)
                return eval_string_binary_expr((string)left, (string)right, binaryOpNode.Operator);
            else if (left is int && right is string)
                return eval_string_binary_expr(left.ToString(), (string)right, binaryOpNode.Operator);
            else if (left is string && right is int)
                return eval_string_binary_expr((string)left, right.ToString(), binaryOpNode.Operator);
  
            return "Invalid binary expression!";
        }

        private object eval_sequence_expr(SequenceNode sequenceNode)
        {          
            return Evaluate(sequenceNode.Statements[0]);
        }

        public object Evaluate(Node node)
        {
            Console.WriteLine(node);
            switch (node)
            {
                case BinaryOpNode:
                    return eval_binary_expr((BinaryOpNode)node);

                case NumberNode:
                    return ((NumberNode)node).Value;

                case StringNode:
                    return ((StringNode)node).Value;

                case SequenceNode:
                    return eval_sequence_expr((SequenceNode)node);
                
                default:
                    return "Invalid node!";
            }
        }
    }
}