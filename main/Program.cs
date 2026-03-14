using System;
using System.IO;
using System.Text;
using LLVMSharp;
using LLVMSharp.Interop;
using System.Globalization;

namespace MyCompiler
{
    public class Program
    {
        // public static int ManagedPrint(int value)
        // {
        //     OutputBuffer.AppendLine(value.ToString());
        //     return value;
        // }
        // public static StringBuilder OutputBuffer = new();

        static void PrintVariableTable(ICompiler compiler)
        {
            Console.WriteLine("Variable table:\n");
            var context = compiler.GetContext();
            var elements = context.GetAll();

            foreach (var item in elements)
            {
                Console.WriteLine($"Variable: {item.Key} - Type: {item.Value.Type}");
            }
            Console.WriteLine("Count: " + elements.Count);
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("--- AST Compiler Shell ---");
    
            ICompiler compiler = new CompilerOrc();
            bool KeepRunning;
            bool Debug = args.Length > 0 && args[0] == "True";
            KeepRunning = !Debug;
            bool multipleLines = true;

            StringBuilder userInput = new StringBuilder();

            // Multi-line input loop with Shift + Enter detection
            do
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("> "); // Prompt for input

                var currentLine = new StringBuilder();
                bool isComplete = false;

                if (multipleLines)
                {
                    // Reading input line by line
                    while (!isComplete)
                    {
                        var key = Console.ReadKey(intercept: true); // intercept key press

                        if (key.Key == ConsoleKey.Enter)  // Handle Enter key
                        {
                            if ((key.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt)
                            {
                                // If Shift + Enter is pressed, submit the command
                                isComplete = true;
                            }
                            else
                            {
                                // Otherwise, treat it as a new line
                                currentLine.Append(Environment.NewLine);
                                Console.WriteLine(); // Move to the next line
                                continue; // Exit for the new line
                            }
                        }
                        else if (key.Key == ConsoleKey.Backspace)  // Handle Backspace key
                        {
                            if (currentLine.Length > 0)
                            {
                                currentLine.Length--;  // Remove last character
                                Console.Write("\b \b"); // Clear the character on the console
                            }
                        }
                        else
                        {
                            currentLine.Append(key.KeyChar);  // Add character to the current line
                            Console.Write(key.KeyChar);  // Display character on console
                        }
                    }
                }
                else
                {
                    var input = Console.ReadLine();
                    currentLine.Append(input);
                }

                // Add the current line to user input
                userInput.Append(currentLine.ToString().Trim());
                currentLine.Clear();

                if (string.IsNullOrWhiteSpace(userInput.ToString()))
                {
                    userInput.Clear();
                    continue;
                }

                // Check for special command before accepting the input (e.g., "exit")
                if (userInput.ToString() == "exit")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Exiting...");
                    break;  // Exit the loop when "exit" command is entered
                }

                // Check for special table or clear commands
                if (userInput.ToString() == "table")
                {
                    PrintVariableTable(compiler);
                    userInput.Clear();
                    continue;
                }

                if (userInput.ToString() == "clearTable")
                {
                    compiler.ClearContext();
                    Console.WriteLine("\ntable is cleared");
                    userInput.Clear();
                    continue;
                }

                try
                {
                    // Process the input (parse and generate IR)
                    using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(userInput.ToString())))
                    {
                        Scanner scanner = new Scanner(stream);
                        Parser parser = new Parser(scanner);

                        if (parser.Parse() && parser.RootNode != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("\nAST Structure:");
                            PrintNode(parser.RootNode, 0); // Print AST

                            try
                            {
                                // Run the IR and print the result
                                object result = compiler.Run(parser.RootNode, Debug);

                                if (result is int[])
                                {
                                    foreach (var item in result as int[])
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.WriteLine(item);
                                    }
                                }
                                else if (result == null)
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("Result: null");
                                }
                                else
                                {
                                    // Use InvariantCulture to ensure dots instead of commas
                                    string formatted = string.Format(CultureInfo.InvariantCulture, "{0}", result);
                                    string suffix = (result is string s) ? $" (Length: {s.Length})" : "";

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"Result: {formatted}{suffix}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine($"Compiler Error: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.WriteLine("Syntax Error: The input could not be parsed into an AST.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"Critical Error: {ex.Message}");
                }

                // Clear input for the next round
                userInput.Clear();
            } while (KeepRunning);
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

                case RandomNodeExpr rn:
                    Console.WriteLine($"{space}Min value: {rn.MinValue}, Max value: {rn.MaxValue}");
                    break;

                case RoundNodeExpr ro:
                    Console.WriteLine($"{space}Value: {ro.Value}, Decimals: {ro.Decimals}");
                    break;

                case ArrayNodeExpr ar:
                    Console.WriteLine($"{space}Elements: {ar.Elements}, Decimals: {ar.ElementType}");
                    break;

                case IndexNodeExpr ind:
                    Console.WriteLine($"{space}Array: ");
                    PrintNode(ind.ArrayExpression, indent + 1);
                    PrintNode(ind.IndexExpression, indent + 1);
                    break;

                case WhereNodeExpr whe:
                    Console.WriteLine($"{space}Where: {whe.IteratorName}");
                    Console.WriteLine($"{space}Iterator name: {whe.IteratorName}");
                    PrintNode(whe.ArrayNodeExpr, indent + 1);
                    PrintNode(whe.Condition, indent + 1);
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