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
        public static StringBuilder OutputBuffer = new();

        // public static int ManagedPrint(int value)
        // {
        //     OutputBuffer.AppendLine(value.ToString());
        //     return value;
        // }

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
            //bool Debug = args.Length > 0 && args[0] == "True";
            bool Debug = true;
            KeepRunning = true;
            bool multipleLines = false;

            StringBuilder userInput = new StringBuilder();

            // Multi-line input loop with Shift + Enter detection
            do
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("> "); // Prompt for input

                var lines = new List<string>();
                var currentLine = new StringBuilder();
                int cursorPosition = 0;
                bool isComplete = false;
                int promptStartLeft = Console.CursorLeft;
                int promptStartTop = Console.CursorTop;
                int prevRenderedLength = 0;

                void RenderCurrentLine()
                {
                    Console.SetCursorPosition(promptStartLeft, Console.CursorTop);
                    Console.Write(currentLine.ToString());
                    if (prevRenderedLength > currentLine.Length)
                    {
                        Console.Write(new string(' ', prevRenderedLength - currentLine.Length));
                    }
                    prevRenderedLength = currentLine.Length;
                    Console.SetCursorPosition(promptStartLeft + cursorPosition, Console.CursorTop);
                }

                if (multipleLines)
                {
                    // Reading input line by line with arrow/edit support
                    while (!isComplete)
                    {
                        var key = Console.ReadKey(intercept: true); // intercept key press

                        if (key.Key == ConsoleKey.Enter)
                        {
                            if ((key.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt)
                            {
                                // If Alt + Enter is pressed, submit the command
                                isComplete = true;
                                Console.WriteLine();
                            }
                            else
                            {
                                // Otherwise, treat it as a new line inside multi-line input
                                lines.Add(currentLine.ToString());
                                currentLine.Clear();
                                cursorPosition = 0;
                                prevRenderedLength = 0;
                                Console.WriteLine();
                                promptStartLeft = 0; // no prompt for continuation lines
                                continue;
                            }
                        }
                        else if (key.Key == ConsoleKey.Backspace)
                        {
                            if (cursorPosition > 0)
                            {
                                cursorPosition--;
                                currentLine.Remove(cursorPosition, 1);
                                RenderCurrentLine();
                            }
                        }
                        else if (key.Key == ConsoleKey.Delete)
                        {
                            if (cursorPosition < currentLine.Length)
                            {
                                currentLine.Remove(cursorPosition, 1);
                                RenderCurrentLine();
                            }
                        }
                        else if (key.Key == ConsoleKey.LeftArrow)
                        {
                            if (cursorPosition > 0)
                            {
                                cursorPosition--;
                                Console.SetCursorPosition(promptStartLeft + cursorPosition, Console.CursorTop);
                            }
                        }
                        else if (key.Key == ConsoleKey.RightArrow)
                        {
                            if (cursorPosition < currentLine.Length)
                            {
                                cursorPosition++;
                                Console.SetCursorPosition(promptStartLeft + cursorPosition, Console.CursorTop);
                            }
                        }
                        else if (key.Key == ConsoleKey.Home)
                        {
                            cursorPosition = 0;
                            Console.SetCursorPosition(promptStartLeft, Console.CursorTop);
                        }
                        else if (key.Key == ConsoleKey.End)
                        {
                            cursorPosition = currentLine.Length;
                            Console.SetCursorPosition(promptStartLeft + cursorPosition, Console.CursorTop);
                        }
                        else if (!char.IsControl(key.KeyChar))
                        {
                            currentLine.Insert(cursorPosition, key.KeyChar);
                            cursorPosition++;
                            RenderCurrentLine();
                        }
                    }
                }
                else
                {
                    var input = Console.ReadLine();
                    currentLine.Append(input);
                }

                // Add the current lines to user input
                if (multipleLines)
                {
                    if (currentLine.Length > 0)
                        lines.Add(currentLine.ToString());

                    userInput.Append(string.Join(Environment.NewLine, lines).Trim());
                    lines.Clear();
                }
                else
                {
                    userInput.Append(currentLine.ToString().Trim());
                }

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
                    Console.ResetColor();
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

                if (userInput.ToString() == "verbose")
                {
                    Debug = !Debug;
                    if (Debug)
                        Console.WriteLine("\n verbose on");
                    else
                        Console.WriteLine("\n verbose off");

                    userInput.Clear();
                    continue;
                }

                if (userInput.ToString() == "multi lines")
                {
                    multipleLines = !multipleLines;
                    if (multipleLines)
                        Console.WriteLine("\n multi lines on");
                    else
                        Console.WriteLine("\n multi lines off");

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
                            if (Debug) Console.WriteLine("\nAST Structure:");
                            if (Debug) PrintNode(parser.RootNode, 0); // Print AST

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
                Console.ForegroundColor = ConsoleColor.Gray;
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