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
            Console.WriteLine("\nVariable table:\n");
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
            try
            {
                Console.WriteLine("--- AST Compiler Shell ---");

                ICompiler compiler = new CompilerOrc();
                bool KeepRunning;
                //bool Debug = args.Length > 0 && args[0] == "True";
                bool Debug = true;
                KeepRunning = true;

#if LINUX
                bool multipleLines = false;
                string exitText = "e";
#else
                bool multipleLines = false;
                string exitText = "exit";
#endif

                StringBuilder userInput = new StringBuilder();

                // Multi-line input loop with Shift + Enter detection
                do
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write("> "); // Prompt for input

                    var lines = new List<string>() { "\n" };
                    var currentLine = new StringBuilder();
                    int cursorPosition = 0;
                    bool isComplete = false;

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
                                    //Console.WriteLine();
                                }
                                else
                                {
                                    // Otherwise, treat it as a new line inside multi-line input
                                    lines.Add(currentLine.ToString());
                                    currentLine.Clear();
                                    cursorPosition = 0;
                                    Console.WriteLine();
                                    continue;
                                }
                            }
                            else if (key.Key == ConsoleKey.Backspace)
                            {
                                if (cursorPosition > 0)
                                {
                                    cursorPosition--;
                                    currentLine.Remove(cursorPosition, 1);
                                }
                            }
                            else if (key.Key == ConsoleKey.Delete)
                            {
                                if (cursorPosition < currentLine.Length)
                                {
                                    currentLine.Remove(cursorPosition, 1);
                                }
                            }
                            else if (key.Key == ConsoleKey.LeftArrow)
                            {
                                if (cursorPosition > 0)
                                {
                                    cursorPosition--;
                                }
                            }
                            else if (key.Key == ConsoleKey.RightArrow)
                            {
                                if (cursorPosition < currentLine.Length)
                                {
                                    cursorPosition++;
                                }
                            }
                            else if (key.Key == ConsoleKey.Home)
                            {
                                cursorPosition = 0;
                            }
                            else if (key.Key == ConsoleKey.End)
                            {
                                cursorPosition = currentLine.Length;
                            }
                            else if (!char.IsControl(key.KeyChar))
                            {
                                currentLine.Insert(cursorPosition, key.KeyChar);
                                cursorPosition++;
                            }

                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write("> " + currentLine.ToString() + " ");
                            Console.SetCursorPosition(2 + cursorPosition, Console.CursorTop);
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
                        Console.WriteLine();
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
                    if (userInput.ToString() == exitText)
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
                                // Use a specific color for AST, then RESET immediately
                                if (Debug)
                                {
                                    // 1. Set the color
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    //Console.WriteLine("\nAST Structure:");

                                    // 2. Print the tree
                                    PrintNode(parser.RootNode, 0);

                                    // 3. RESET IMMEDIATELY before doing the dangerous work (compiler.Run)
                                    // Console.ResetColor();
                                }

                                try
                                {
                                    // Now if this crashes, the console is already back to Gray/Default
                                    object result = compiler.Run(parser.RootNode, Debug);

                                    // Your HandleArray2 returns a string, so 'result is int[]' is no longer needed
                                    if (result == null)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine("Result: null");
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        // HandleArray2 already formats the string, so we just print it
                                        string formatted = string.Format(CultureInfo.InvariantCulture, "{0}", result);
                                        Console.WriteLine($"Result: {formatted}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red; // Brighter red for visibility
                                    Console.WriteLine($"Compiler Error: {ex.Message}");
                                }
                                finally
                                {
                                    Console.ResetColor(); // Final safety reset
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine($"Critical Error: {ex.Message}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                    // Clear input for the next round
                    userInput.Clear();
                    Console.ForegroundColor = ConsoleColor.Gray;
                } while (KeepRunning);

            }
            finally
            {
                // Absolute safety reset for when the app exits or hits a truly fatal error
                Console.ResetColor();
            }
        }

        static void PrintNode(Node node, int indent)
        {
            if (node == null) return;
            string space = new string(' ', indent * 2);

            switch (node)
            {
                case SequenceNode seq:
                    foreach (var stmt in seq.Statements) PrintNode(stmt, indent);
                    break;

                case AssignNode assign:
                    Console.WriteLine($"{space}Assignment: {assign.Id} =");
                    PrintNode(assign.Expression, indent + 1);
                    break;

                // --- Dataframe & Records ---
                case DataframeNode df:
                    Console.WriteLine($"{space}DATAFRAME:");
                    Console.WriteLine($"{space}  Columns:");
                    PrintNode(df.Columns, indent + 2);
                    Console.WriteLine($"{space}  Rows:");
                    PrintNode(df.Rows, indent + 2);
                    break;

                case RecordNode rec:
                    Console.WriteLine($"{space}RECORD {{");
                    foreach (var field in rec.Fields)
                    {
                        Console.WriteLine($"{space}  Field: {field.Label}");
                        PrintNode(field.Value, indent + 2);
                    }
                    Console.WriteLine($"{space}}}");
                    break;

                case RecordFieldNode rf:
                    Console.WriteLine($"{space}Get Field: {rf.IdField} from:");
                    PrintNode(rf.IdRecord, indent + 1);
                    break;

                case RecordFieldAssignNode rfa:
                    Console.WriteLine($"{space}Set Field: {rfa.IdField} =");
                    PrintNode(rfa.AssignExpression, indent + 1);
                    Console.WriteLine($"{space}  On Record:");
                    PrintNode(rfa.IdRecord, indent + 1);
                    break;

                // --- CSV Operations ---
                case ReadCsvNode csv:
                    Console.WriteLine($"{space}READ CSV:");
                    Console.WriteLine($"{space}  Path:");
                    PrintNode(csv.FileNameExpr, indent + 2);
                    Console.WriteLine($"{space}  Schema:");
                    PrintNode(csv.SchemaExpr, indent + 2);
                    break;

                case ToCsvNode toCsv:
                    Console.WriteLine($"{space}TO CSV:");
                    Console.WriteLine($"{space}  Source:");
                    PrintNode(toCsv.Expression, indent + 2);
                    Console.WriteLine($"{space}  Destination:");
                    PrintNode(toCsv.FileNameExpr, indent + 2);
                    break;

                // --- Functional (Map / Where) ---
                case MapNode map:
                    Console.WriteLine($"{space}MAP (Iterator: {map.IteratorId.Name}):");
                    Console.WriteLine($"{space}  Source:");
                    PrintNode(map.SourceExpr, indent + 2);
                    Console.WriteLine($"{space}  Transformation:");
                    PrintNode(map.Assignment, indent + 2);
                    break;

                case WhereNode whe:
                    Console.WriteLine($"{space}WHERE (Iterator: {whe.IteratorId.Name}):");
                    Console.WriteLine($"{space}  Source:");
                    PrintNode(whe.SourceExpr, indent + 2);
                    Console.WriteLine($"{space}  Predicate:");
                    PrintNode(whe.Condition, indent + 2);
                    break;

                // --- Array Modifications ---
                case AddNode add:
                    Console.WriteLine($"{space}ARRAY ADD:");
                    PrintNode(add.SourceExpression, indent + 1);
                    PrintNode(add.AddExpression, indent + 1);
                    break;

                case AddRangeNode addR:
                    Console.WriteLine($"{space}ARRAY ADD RANGE:");
                    PrintNode(addR.SourceExpression, indent + 1);
                    PrintNode(addR.AddRangeExpression, indent + 1);
                    break;

                case RemoveNode rem:
                    Console.WriteLine($"{space}ARRAY REMOVE:");
                    PrintNode(rem.SourceExpression, indent + 1);
                    PrintNode(rem.RemoveExpression, indent + 1);
                    break;

                case IndexAssignNode idxAsgn:
                    Console.WriteLine($"{space}INDEX ASSIGNMENT:");
                    Console.WriteLine($"{space}  Target:");
                    PrintNode(idxAsgn.ArrayExpression, indent + 2);
                    Console.WriteLine($"{space}  Index:");
                    PrintNode(idxAsgn.IndexExpression, indent + 2);
                    Console.WriteLine($"{space}  New Value:");
                    PrintNode(idxAsgn.AssignExpression, indent + 2);
                    break;

                // --- Field Modification (Dynamic) ---
                case AddFieldNode af:
                    Console.WriteLine($"{space}ADD FIELD '{af.FieldName}':");
                    PrintNode(af.Record, indent + 1);
                    PrintNode(af.Value, indent + 1);
                    break;

                case RemoveFieldNode rmf:
                    Console.WriteLine($"{space}REMOVE FIELD '{rmf.FieldName}' from:");
                    PrintNode(rmf.Record, indent + 1);
                    break;

                // --- Literals & Basic Nodes ---
                case TypeLiteralNode t:
                    Console.WriteLine($"{space}Type Literal: {t.Type}");
                    break;

                case NamedArgumentNode na:
                    Console.WriteLine($"{space}Named Arg: {na.Name} =>");
                    PrintNode(na.Value, indent + 1);
                    break;

                case CopyNode cp:
                    Console.WriteLine($"{space}COPY:");
                    PrintNode(cp.Source, indent + 1);
                    break;

                case ArrayNode ar:
                    Console.WriteLine($"{space}ARRAY (Size: {ar.Elements.Count}):");
                    foreach (var el in ar.Elements) PrintNode(el, indent + 1);
                    break;

                case StringNode str:
                    Console.WriteLine($"{space}String: \"{str.Value}\"");
                    break;

                case NumberNode num:
                    Console.WriteLine($"{space}Int: {num.Value}");
                    break;

                case FloatNode f:
                    Console.WriteLine($"{space}Float: {f.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                    break;

                case IdNode id:
                    Console.WriteLine($"{space}Variable: {id.Name}");
                    break;

                case IfNode ifNode:
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

                // --- Aggregate Array Operations ---
                case LengthNode len:
                    Console.WriteLine($"{space}LENGTH OF:");
                    PrintNode(len.ArrayExpression, indent + 1);
                    break;

                case MinNode min:
                    Console.WriteLine($"{space}MIN OF:");
                    PrintNode(min.ArrayExpression, indent + 1);
                    break;

                case MaxNode max:
                    Console.WriteLine($"{space}MAX OF:");
                    PrintNode(max.ArrayExpression, indent + 1);
                    break;

                case MeanNode mean:
                    Console.WriteLine($"{space}MEAN OF:");
                    PrintNode(mean.ArrayExpression, indent + 1);
                    break;

                case SumNode sum:
                    Console.WriteLine($"{space}SUM OF:");
                    PrintNode(sum.ArrayExpression, indent + 1);
                    break;

                // --- Unary Operations (e.g., -x) ---
                case UnaryOpNode unary:
                    Console.WriteLine($"{space}Unary Op: {unary.Operator}");
                    PrintNode(unary.Operand, indent + 1);
                    break;

                default:
                    Console.WriteLine($"{space}Node: {node.GetType().Name}");
                    break;
            }
        }

    }
}