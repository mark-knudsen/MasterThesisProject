using Xunit;
using System;
using System.IO;
using System.Text;
using LLVMSharp;
using LLVMSharp.Interop;
using System.Diagnostics;
using System.Globalization;

namespace MyCompiler;

public class CompilerIntegrationTestORC
{
    private readonly string _irFilePath = "output_actual_orc.ll";  // LLVM IR file
    private readonly string _binaryPath = "output_executable"; // Compiled binary file

    // to generate the actual executable
    // bash: clang -target x86_64-linux-gnu -o output_executable output_actual_orc.ll
    // bash: ./output_executable

    [Fact]
    public void Test_Dataframe_Print()
    {
        string input = "x=dataframe([\"name\", \"age\"], [{name: \"dan\", age: 30}, {name: \"alice\", age: 25}]); x";

        var output = new StringWriter();
        Console.SetOut(output);

        var result = CompilerFacade.Execute(input);

        string text = output.ToString();

        Assert.Contains("Dataframe (2 rows):", text);
        Assert.Contains("name", text);
        Assert.Contains("age", text);
        Assert.Contains("dan", text);
        Assert.Contains("alice", text);
    }

    [Theory]
    [InlineData("2+2", 4)]
    [InlineData("true", true)]
    [InlineData("\"harry\"", "harry")]
    public void TestCompiler_ReturnsValues(string input, object expected)
    {
        var result = CompilerFacade.Execute(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("2+2", "", "Result: 4")]
    [InlineData("2+3", "", "Result: 5")]
    [InlineData("true", "", "Result: true")]
    [InlineData("false", "", "Result: false")]
    [InlineData("\"harry\"", "", "Result: harry")]
    [InlineData("print(505)", "505", "")]
    [InlineData("print(214535)", "214535", "")]
    [InlineData("print(\"Harry\")", "Harry", "")] // we have to handle prints differently
    [InlineData("print(true)", "True", "")]
    [InlineData("print(false)", "False", "")]
    [InlineData("x=20; print(x)", "", "Result: 20")]
    [InlineData("x=\"Harry\"; print(\"Harry\")", "", "Result: Harry")]
    public void TestCompiler_Id_ReturnsCorrectResults(string input, string expectedOutput, string expectedReturnValue)
    {
        // var fn = jit.Lookup("main");
        // IntPtr result = fn();

        // RuntimeObject obj = Marshal.PtrToStructure<RuntimeObject>(result);
        // Assert.Equal(4, obj.AsInt());


        Console.SetIn(new StringReader(input)); // Simulate user input
        var outputWriter = new StringWriter();
        Console.SetOut(outputWriter);  // Capture any console output

        Program.Main(new string[] { "True" });

        // Step 2: Compile the LLVM IR to binary using clang
        CompileLLVMIRToBinary();
        if (!File.Exists(_binaryPath))
            throw new FileNotFoundException("Compiled binary not found.");

        // Step 3: Run the binary and capture output and exit code
        string outputAndExitCode = RunCompiledBinary(_binaryPath);

        // Step 4: Safely split the output and exit code
        string[] result = outputAndExitCode.Split(new[] { "\nExit Code: " }, StringSplitOptions.None);

        if (result.Length < 2)
            throw new Exception("Unexpected output format: could not find exit code.");

        string output = result[0];
        string exitValue = result[1];
        // int exitCode;
        // if (!int.TryParse(result[1], out exitCode))
        // {
        //     throw new Exception($"Failed to parse exit code: {result[1]}");
        // }

        // Step 5: Assert the output (printed) and exit code (return value)
        Assert.Equal(expectedOutput + "\n", output);  // Assert printed output
        //Assert.Equal(expectedReturnValue, exitValue);     // Assert the return value (exit code)
    }

    private string RunCompiledBinary(string binaryPath)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = binaryPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processStartInfo))
        {
            if (process == null)
                throw new InvalidOperationException("Failed to start process.");

            // Capture the standard output and standard error
            string output = process.StandardOutput.ReadToEnd(); // what ever the print func prints out
            string error = process.StandardError.ReadToEnd();

            // Wait for the process to exit
            process.WaitForExit();

            // Capture the exit code (return value of the main function)
            int exitCode = process.ExitCode;

            // For debugging: print the output and exit code
            Console.WriteLine($"Captured Output: {output}");
            Console.WriteLine($"Captured Exit Code: {exitCode}");

            // Return the output and exit code as a string
            return $"{output}\nExit Code: {exitCode}";
        }
    }

    private string CompileLLVMIRToBinary()
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "clang",  // Use clang to compile the LLVM IR to a binary
            Arguments = $"{_irFilePath} -target x86_64-linux-gnu -Wno-override-module -o {_binaryPath}", // Compile IR to binary
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processStartInfo))
        {
            if (process == null)
                throw new InvalidOperationException("Failed to start clang process.");

            // Capture the output and errors
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            // Print output and error for debugging
            Console.WriteLine("Clang Output:\n" + output);
            Console.WriteLine("Clang Error:\n" + error);

            // If there's any error, throw an exception
            if (!string.IsNullOrEmpty(error))
            {
                // If it's a warning, print and continue
                if (error.Contains("warning"))
                    Console.WriteLine("Warning during Clang compilation: " + error);
                else
                    throw new Exception($"Clang compilation failed with errors: {error}");
            }

            int exitCode = process.ExitCode;
            if (process.ExitCode != 0) // this should be the return value, we don't want to trhow an error tough
            {
                var d = process.ExitCode;
                Console.WriteLine(d);
                //throw new Exception($"Clang compilation failed: {error}");
            }

            // For debugging: print the output and exit code
            Console.WriteLine($"Captured Output: {output}");
            Console.WriteLine($"Captured Exit Code: {exitCode}");

            // Return the output and exit code as a string or you can also return as a tuple if needed
            return $"{output}\nExit Code: {exitCode}";
        }
    }
}
