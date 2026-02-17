using System;
using System.IO;
using System.Text;
using LLVMSharp;
using LLVMSharp.Interop;

namespace MyCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- AST Compiler Shell ---");
            Interpreter interpreter = new Interpreter();
            Compiler compiler = new Compiler();
            object result = null;

            while (true)
            {
                Console.Write("\n> ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;
                if (input == "exit") break;

                try
                {
                    using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
                    {
                        Scanner scanner = new Scanner(stream);
                        Parser parser = new Parser(scanner);

                        // 1. Attempt to Parse
                        if (parser.Parse() && parser.RootNode != null)
                        {
                            Console.WriteLine("AST Structure:");
                            PrintNode(parser.RootNode, 0);

                            // 2. Attempt to Compile and Run
                            // We wrap this in a try/catch too in case of LLVM errors
                            try
                            {
                                result = compiler.Run(parser.RootNode);

                                if (result == null)
                                    Console.WriteLine("Result: null");
                                else
                                    Console.WriteLine($"Result: {result} {(result is string s ? $"(Length: {s.Length})" : "")}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Compiler Error: {ex.Message}");
                            }
                        }
                        else
                        {
                            // Parser usually prints its own errors, but we provide a fallback
                            Console.WriteLine("Syntax Error: The input could not be parsed into an AST.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Critical Error: {ex.Message}");
                }
            }
        }

        // Simple recursive function to visualize the tree
        static void PrintNode(NodeExpr node, int indent)
        {
            if (node == null) return;

            string space = new string(' ', indent * 2);

            if (node is SequenceNodeExpr seq)
            {
                // We don't increment indent here usually, as it's just a container
                foreach (var stmt in seq.Statements) PrintNode(stmt, indent);
            }
            else if (node is IfNodeExpr ifNode)
            {
                Console.WriteLine($"{space} IF Statement:");
                Console.WriteLine($"{space} Condition:");
                PrintNode(ifNode.Condition, indent + 2);

                Console.WriteLine($"{space} Then:");
                PrintNode(ifNode.ThenPart, indent + 2);

                if (ifNode.ElsePart != null)
                {
                    Console.WriteLine($"{space} Else:");
                    PrintNode(ifNode.ElsePart, indent + 2);
                }
            }
            else if (node is AssignNodeExpr assign)
            {
                Console.WriteLine($"{space} Assignment: {assign.Id} =");
                PrintNode(assign.Expression, indent + 1);
            }
            else if (node is BinaryOpNodeExpr bin)
            {
                Console.WriteLine($"{space} Op: {bin.Operator}");
                PrintNode(bin.Left, indent + 1);
                PrintNode(bin.Right, indent + 1);
            }
            else if (node is ComparisonNodeExpr comp)
            {
                Console.WriteLine($"{space} Comparison: {comp.Operator}");
                PrintNode(comp.Left, indent + 1);
                PrintNode(comp.Right, indent + 1);
            }
            else if (node is BooleanNodeExpr b)
            {
                Console.WriteLine($"{space}Boolean: {b.Value}");
            }
            else if (node is NumberNodeExpr num)
            {
                Console.WriteLine($"{space}Literal: {num.Value}");
            }
            else if (node is IdNodeExpr id)
            {
                Console.WriteLine($"{space}Variable: {id.Name}");
            }
            else if (node is StringNodeExpr str)
            {
                Console.WriteLine($"{space}String: \"{str.Value}\"");
            }
            else if (node is ForLoopNodeExpr forNode)
            {
                Console.WriteLine($"{space}FOR Loop:");
                Console.WriteLine($"{space}  Init:");
                PrintNode(forNode.Initialization, indent + 2);
                Console.WriteLine($"{space}  Condition:");
                PrintNode(forNode.Condition, indent + 2);
                Console.WriteLine($"{space}  Step:");
                PrintNode(forNode.Step, indent + 2);
                Console.WriteLine($"{space}  Body:");
                PrintNode(forNode.Body, indent + 2);
            }
            else if (node is PrintNodeExpr pn)
            {
                Console.WriteLine($"{space}PRINT Statement:");
                PrintNode(pn.Expression, indent + 1);
            }
        }
    }
}