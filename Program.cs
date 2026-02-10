using System;
using System.IO;
using System.Text;

namespace MyCompiler {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("--- AST Compiler Shell ---");
            Interpreter interpreter = new Interpreter();

            while (true) {
                Console.Write("\n> ");
                string input = Console.ReadLine();
                if (input == "exit") break;

                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input))) {
                    Scanner scanner = new Scanner(stream);
                    Parser parser = new Parser(scanner);
                    
                    if (parser.Parse()) {
                        Console.WriteLine("AST Structure:");
                        PrintNode(parser.RootNode, 0);
                    } else {
                        Console.WriteLine("Syntax Error!");
                    }
                    object result = interpreter.Evaluate(parser.RootNode);
                    Console.WriteLine("Result: " + result);
                }
            }

        }

        // Simple recursive function to visualize the tree
        static void PrintNode(Node node, int indent) {
            if (node == null) return;

            string space = new string(' ', indent * 2);

            if (node is SequenceNode seq) {
                // We don't increment indent here usually, as it's just a container
                foreach (var stmt in seq.Statements) PrintNode(stmt, indent);
            } 
            else if (node is IfNode ifNode) {
                Console.WriteLine($"{space}IF Statement:");
                Console.WriteLine($"{space}  Condition:");
                PrintNode(ifNode.Condition, indent + 2);
                
                Console.WriteLine($"{space}  Then:");
                PrintNode(ifNode.ThenPart, indent + 2);

                if (ifNode.ElsePart != null) {
                    Console.WriteLine($"{space}  Else:");
                    PrintNode(ifNode.ElsePart, indent + 2);
                }
            }
            else if (node is AssignNode assign) {
                Console.WriteLine($"{space}Assignment: {assign.Id} =");
                PrintNode(assign.Expression, indent + 1);
            } 
            else if (node is BinaryOpNode bin) {
                Console.WriteLine($"{space}Op: {bin.Operator}");
                PrintNode(bin.Left, indent + 1);
                PrintNode(bin.Right, indent + 1);
            } 
            else if (node is ComparisonNode comp) {
                Console.WriteLine($"{space}Comparison: {comp.Operator}");
                PrintNode(comp.Left, indent + 1);
                PrintNode(comp.Right, indent + 1);
            }
            else if (node is BooleanNode b) {
                Console.WriteLine($"{space}Boolean: {b.Value}");
            }
            else if (node is NumberNode num) {
                Console.WriteLine($"{space}Literal: {num.Value}");
            } 
            else if (node is IdNode id) {
                Console.WriteLine($"{space}Variable: {id.Name}");
            } 
            else if (node is StringNode str) {
                Console.WriteLine($"{space}String: \"{str.Value}\"");
            }
        }
    }
}