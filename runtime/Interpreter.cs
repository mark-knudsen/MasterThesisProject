using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MyCompiler
{
    public class Interpreter
    {
        Dictionary<string, object> variables = new Dictionary<string, object>();

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
            var result = (object)null;
            foreach (var stmt in sequenceNode.Statements)
            {
                result = Evaluate(stmt);
            }
            return result;
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
            variables[assignNode.Id] = expr;
            
            return expr;
        }

        private object eval_if_expr(IfNode ifNode)
        {
            var condition = Evaluate(ifNode.Condition);
            if (condition is bool)
            {
                if ((bool)condition) return Evaluate(ifNode.ThenPart);
                else if (ifNode.ElsePart != null) return Evaluate(ifNode.ElsePart);
                else return null;
            }
            return "Invalid condition!";
        }

        private object eval_for_loop_expr(ForLoopNode forLoopNode)
        {
            object result = null;   
  
            for (Evaluate(forLoopNode.Initialization); (bool)Evaluate(forLoopNode.Condition); Evaluate(forLoopNode.Step)) 
            {
                result = Evaluate(forLoopNode.Body);
            }
    
            return result;
        }

        private object eval_increment_variable(string id)
        {
            if (variables.ContainsKey(id) && variables[id] is int)
            {
                variables[id] = (int)variables[id] + 1;
                return variables[id];
            }
            return "Invalid increment operation!";
        }

        private object eval_decrement_variable(string id)
        {
            if (variables.ContainsKey(id) && variables[id] is int)
            {
                variables[id] = (int)variables[id] - 1;
                return variables[id];
            }
            return "Invalid decrement operation!";
        }

        public object Evaluate(Node node)
        {

            Console.WriteLine(node);
            switch (node)
            {
                case BinaryOpNode binaryOpNode:
                    return eval_binary_expr(binaryOpNode);

                case NumberNode numberNode:
                    return numberNode.Value;

                case StringNode stringNode:
                    return stringNode.Value;

                case SequenceNode seq:
                    return eval_sequence_expr(seq);

                case IdNode idNode:
                    return variables.ContainsKey(idNode.Name) ? variables[idNode.Name] : "undefined";

                case AssignNode assignNode:
                    return eval_assign_expr(assignNode);

                case IncrementNode incrementNode:
                    return eval_increment_variable(incrementNode.Id);

                case DecrementNode decrementNode:
                    return eval_decrement_variable(decrementNode.Id);

                case IfNode ifNode:
                    return eval_if_expr(ifNode);

                case BooleanNode booleanNode:
                    return booleanNode.Value;

                case ForLoopNode forLoopNode:
                    return eval_for_loop_expr(forLoopNode);

                case ComparisonNode comparisonNode:
                    return eval_comparison_expr(comparisonNode);

                case RandomNode randomNode:
                    return eval_random_expr(randomNode);

                default:
                    return "Invalid node!";
            }
        }
    }
}