
using System;
using System.Collections;
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

        private object eval_numeric_comparison_expr(int left, int right, string _operator)
        {
            switch (_operator)
            {
                case "==":
                    if (left == right) return true;
                        else return false;
                case "!=":
                    if (left != right) return true;
                        else return false;
                case "<=":
                     if (left <= right) return true;
                        else return false;
                case ">=": 
                    if (left >= right) return true;
                        else return false;
                case "<":
                     if (left < right) return true;
                        else return false;
                case ">": 
                    if (left > right) return true;
                        else return false;
                default:
                    return "Invalid numeric operator!";
            }
        }

        private object eval_string_comparison_expr(string left, string right, string _operator)
        {
            switch (_operator)
            {
                case "==":
                    if (left == right) return true;
                        else return false;
                case "!=":
                    if (left != right) return true;
                        else return false;
 
                default:
                    return "Invalid string operator!";
            }
        }
        private object eval_comparison_expr(ComparisonNode comparisonNode)
        {   
            var left = Evaluate(comparisonNode.Left);
            var right = Evaluate(comparisonNode.Right);   


            if (left is int && right is int)                
                return eval_numeric_comparison_expr((int)left, (int)right, comparisonNode.Operator);

            else if (left is string && right is string)
                return eval_string_comparison_expr((string)left, (string)right, comparisonNode.Operator);
            
            else if (left is bool && right is bool)
                return eval_string_comparison_expr(left.ToString(), right.ToString(), comparisonNode.Operator);


            return "Invalid comparison expression!";
        }

        private object eval_random_expr(RandomNode randomNode)
        {
            var minValue = Evaluate(randomNode.MinValue);
            var maxValue = Evaluate(randomNode.MaxValue);

            try
            {
                if (minValue is int && maxValue is int)
                    return new Random().Next((int)minValue, (int)maxValue);
                return "Invalid arguments!";
            }
            catch (System.Exception e)
            {
                return e;
            }
            
        }

        private object eval_assign_expr(AssignNode assignNode)
        {
            var expr  = Evaluate(assignNode.Expression);
            return null;
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

                case IdNode:
                    return ((IdNode)node).Name;

                case AssignNode:
                    return eval_assign_expr((AssignNode)node);

                case IfNode:
                    return -100;

                case BooleanNode:
                    return ((BooleanNode)node).Value;

                case ComparisonNode:
                    return eval_comparison_expr((ComparisonNode)node);

                case RandomNode:
                    return eval_random_expr((RandomNode)node);

                default:
                    return "Invalid node!";
            }
        }
    }
}