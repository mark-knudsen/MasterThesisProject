using System;
using System.IO;
using System.Text;
using LLVMSharp;
using LLVMSharp.Interop;
using System.Globalization;

namespace MyCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- AST Compiler Shell ---");
            Compiler compiler = new Compiler();

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

                        if (parser.Parse() && parser.RootNode != null)
                        {
                            Console.WriteLine("AST Structure:");
                            PrintNode(parser.RootNode, 0);

                            try
                            {
                                object result = compiler.Run(parser.RootNode);

                                // --- Consolidated Result Printing ---
                                if (result == null)
                                {
                                    Console.WriteLine("Result: null");
                                }
                                else
                                {
                                    // Use InvariantCulture to ensure dots instead of commas
                                    string formatted = string.Format(CultureInfo.InvariantCulture, "{0}", result);

                                    // Add a little extra info if it's a string, otherwise just the value
                                    string suffix = (result is string s) ? $" (Length: {s.Length})" : "";

                                    Console.WriteLine($"Result: {formatted}{suffix}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Compiler Error: {ex.Message}");
                            }
                        }
                        else
                        {
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

        static void PrintNode(NodeExpr node, int indent)
        {
            if (node == null) return;
            string space = new string(' ', indent * 2);

            switch (node)
            {
                case SequenceNodeExpr seq:
                    foreach (var stmt in seq.Statements) PrintNode(stmt, indent);
                    break;

                case IfNodeExpr ifNode:
                    Console.WriteLine($"{space}IF Statement:");
                    PrintNode(ifNode.Condition, indent + 2);
                    Console.WriteLine($"{space}Then:");
                    PrintNode(ifNode.ThenPart, indent + 2);
                    if (ifNode.ElsePart != null)
                    {
                        Console.WriteLine($"{space}Else:");
                        PrintNode(ifNode.ElsePart, indent + 2);
                    }
                    break;

                case AssignNodeExpr assign:
                    Console.WriteLine($"{space}Assignment: {assign.Id} =");
                    PrintNode(assign.Expression, indent + 1);
                    break;

                case BinaryOpNodeExpr bin:
                    Console.WriteLine($"{space}Op: {bin.Operator}");
                    PrintNode(bin.Left, indent + 1);
                    PrintNode(bin.Right, indent + 1);
                    break;

                case FloatNodeExpr f: // Added missing Float case
                    Console.WriteLine($"{space}Float Literal: {f.Value.ToString(CultureInfo.InvariantCulture)}");
                    break;

                case NumberNodeExpr num:
                    Console.WriteLine($"{space}Int Literal: {num.Value}");
                    break;

                case BooleanNodeExpr b:
                    Console.WriteLine($"{space}Boolean: {b.Value}");
                    break;

                case StringNodeExpr str:
                    Console.WriteLine($"{space}String: \"{str.Value}\"");
                    break;

                case IdNodeExpr id:
                    Console.WriteLine($"{space}Variable: {id.Name}");
                    break;

                case PrintNodeExpr pn:
                    Console.WriteLine($"{space}PRINT Statement:");
                    PrintNode(pn.Expression, indent + 1);
                    break;

                case ForLoopNodeExpr forNode:
                    Console.WriteLine($"{space}FOR Loop:");
                    PrintNode(forNode.Initialization, indent + 2);
                    PrintNode(forNode.Condition, indent + 2);
                    PrintNode(forNode.Step, indent + 2);
                    PrintNode(forNode.Body, indent + 2);
                    break;
            }
        }
    }
}