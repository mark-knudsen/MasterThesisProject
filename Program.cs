using System;
using System.IO;
using System.Text;

namespace MyCompiler {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("=== My Language Shell ===");
            
            Interpreter interpreter = new Interpreter();

            while (true) {
                Console.Write("\n> ");
                string input = Console.ReadLine();
                if (input == "exit") break;

                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input))) {
                    Scanner scanner = new Scanner(stream);
                    Parser parser = new Parser(scanner);
                    
                    if (parser.Parse()) {
                        // 1. Run via Interpreter
                        Console.WriteLine("\n[Interpreter Result]");
                        object interpResult = parser.RootNode.Accept(interpreter);
                        Console.WriteLine(interpResult);

                        // 2. Run via LLVM JIT
                        // We create a fresh compiler per input to avoid module name conflicts
                        try {
                            LLVMCompiler compiler = new LLVMCompiler();
                            compiler.CompileAndRun(parser.RootNode);
                        } catch (Exception ex) {
                            Console.WriteLine($"LLVM Error: {ex.Message}");
                        }
                    } else {
                        Console.WriteLine("Syntax Error!");
                    }
                }
            }
        }
    }
}