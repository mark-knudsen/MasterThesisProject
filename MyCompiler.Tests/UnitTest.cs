using System.Diagnostics;
using MyCompiler;  // Make sure this matches the namespace
using Xunit;

//[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace MyCompiler.Tests
{
    public class UnitTest1
    {
        private StringWriter ReadWriteToConsole(string input)
        {
            Console.SetIn(new StringReader(input));

            var outputWriter = new StringWriter();
            Console.SetOut(outputWriter);
            return outputWriter;
        }

        private void RunCompilerAndCompare(StringWriter outputWriter, string expectedOutput)
        {
            // Act
            Program.Main(new string[] { "True" });

            // Assert
            var actualOutput = outputWriter.ToString();
            Assert.Contains(expectedOutput, actualOutput);
        }
        [Theory]
        [InlineData("2", "Result: 2")]
        [InlineData("222", "Result: 222")]
        [InlineData("13453453", "Result: 13453453")]
        public void TestCompiler_Number_ShouldReturnCorrectResults(string input, string expectedOutput)
        {
            var outputWriter = ReadWriteToConsole(input);
            RunCompilerAndCompare(outputWriter, expectedOutput);
        }

        [Theory]
        [InlineData("2+2", "Result: 4")]
        [InlineData("222+332", "Result: 554")]
        [InlineData("6-8", "Result: -2")]
        [InlineData("222-100", "Result: 122")]
        [InlineData("222*120", "Result: 26640")]
        [InlineData("4*8", "Result: 32")]
        [InlineData("222/37", "Result: 6")]
        [InlineData("60/6", "Result: 10")]
        [InlineData("2+2+2+2+2+2+2+2+2+2", "Result: 20")]
        [InlineData("2+3*4", "Result: 14")]
        [InlineData("(2+3)*4", "Result: 20")]
        public void TestCompiler_BinaryOperations_ReturnsCorrectResults(string input, string expectedOutput)
        {
            var outputWriter = ReadWriteToConsole(input);
            RunCompilerAndCompare(outputWriter, expectedOutput);
        }

        [Theory]
        [InlineData("true", "Result: True")]
        [InlineData("false", "Result: False")]
        public void TestCompiler_Boolean_ReturnsCorrectResults(string input, string expectedOutput)
        {
            var outputWriter = ReadWriteToConsole(input);
            RunCompilerAndCompare(outputWriter, expectedOutput);
        }

        [Theory]
        [InlineData("x=2", "Result: null")]
        [InlineData("x=2;", "Result: null")]
        [InlineData("x=true", "Result: null")]
        [InlineData("x=false;", "Result: null")]
        [InlineData("x=\"Harry Potter\";", "Result: null")]
        [InlineData("x=[1,2,3,4];", "Result: null")]
        public void TestCompiler_Assign_ReturnsCorrectResults(string input, string expectedOutput)
        {
            var outputWriter = ReadWriteToConsole(input);
            RunCompilerAndCompare(outputWriter, expectedOutput);
        }

        [Theory]
        [InlineData("x=2; x", "Result: 2")]
        [InlineData("x=true; x", "Result: true")]
        [InlineData("x=\"Harry the wizard\"; x", "Result: Harry the wizard")]
        [InlineData("le_variable2=500; le_variable2", "Result: 500")]
        [InlineData("x=2; x=3; x", "Result: 3")]
        [InlineData("x=2; y=4; z=5; x", "Result: 2")]
        [InlineData("x=2; y=4; z=5; y", "Result: 4")]
        [InlineData("x=2; y=4; z=5; z", "Result: 5")]
        [InlineData("x=2; y=true; z=5; y", "Result: true")]
        [InlineData("x=2; y=x; y", "Result: 2")]
        [InlineData("x=2; y=x; x=5; y", "Result: 2")]
        [InlineData("x=[1,2,3,4]; x", "Result: [1,2,3,4]")]
        public void TestCompiler_Id_ReturnsCorrectResults(string input, string expectedOutput)
        {
            var outputWriter = ReadWriteToConsole(input);
            RunCompilerAndCompare(outputWriter, expectedOutput);
        }

        [Theory]
        [InlineData("x=[1,2,3,4]; print(x[0]);", "0\n")]
        [InlineData("x=[5,6,7,8]; print(x[3]);", "8\n")]
        [InlineData("print([9,8,7][1]);", "8\n")]
        public void TestCompiler_ArrayIndex_ReturnsCorrectResults(string input, string expectedOutput)
        {
            var outputWriter = ReadWriteToConsole(input);
            RunCompilerAndCompare(outputWriter, expectedOutput);
        }

        [Theory]
        [InlineData("10>5", "Result: True")]
        [InlineData("10<5", "Result: False")]
        [InlineData("10>=10", "Result: True")]
        [InlineData("10>=11", "Result: False")]
        [InlineData("10<=10", "Result: True")]
        [InlineData("10<=9", "Result: False")]
        [InlineData("9==9", "Result: True")]
        [InlineData("9==10", "Result: False")]
        [InlineData("2+5==9-2", "Result: True")]
        [InlineData("(2+5)*3<=7*3", "Result: True")]
        [InlineData("2+5*3>=6*3", "Result: False")]
        [InlineData("\"harry potter\"==\"harry potter\"", "Result: True")]
        [InlineData("\"harry potter\"==\"harry potter\" == true", "Result: True")]
        [InlineData("\"harry potter\"!=\"harry potter\"", "Result: False")]
        [InlineData("\"harry G\"!=\"once upon a time\"", "Result: True")]
        public void TestCompiler_Comparison_ReturnsCorrectResults(string input, string expectedOutput)
        {
            var outputWriter = ReadWriteToConsole(input);
            RunCompilerAndCompare(outputWriter, expectedOutput);
        }

        [Theory]
        [InlineData("x=2; x++", "Result: 3")]
        [InlineData("x=5; x++", "Result: 6")]
        [InlineData("x=2; x--", "Result: 1")]
        [InlineData("x=5; x--", "Result: 4")]
        [InlineData("x=5; x++; x++", "Result: 7")]
        [InlineData("x=5; x--; x--", "Result: 3")]
        [InlineData("x=5; x++; x--", "Result: 5")]
        [InlineData("x=5; x--; x++", "Result: 5")]
        [InlineData("x=5; x--; x++; x++; x++; x+2*3", "Result: 13")]
        public void TestCompiler_IncrementDecrement_ReturnsCorrectResults(string input, string expectedOutput)
        {
            var outputWriter = ReadWriteToConsole(input);
            RunCompilerAndCompare(outputWriter, expectedOutput);
        }

        [Theory]
        [InlineData("\"hello world\"", "Result: hello world")]
        [InlineData("\"harry potter\"", "Result: harry potter")]
        [InlineData("\"harry potter\" + \" is a wizard\"", "Result: harry potter is a wizard")]

        public void TestCompiler_String_ReturnsCorrectResults(string input, string expectedOutput)
        {
            var outputWriter = ReadWriteToConsole(input);
            RunCompilerAndCompare(outputWriter, expectedOutput);
        }

        [Theory]
        [InlineData("random(1,10)", 1, 10)]
        [InlineData("random(1,100)", 1, 100)]
        [InlineData("random(100,500)", 100, 500)]
        [InlineData("random(10000,500000)", 10000, 500000)]

        public void TestCompiler_Random_ReturnsCorrectResults(string input, int minValue, int maxValue)
        {
            Console.SetIn(new StringReader(input));

            var outputWriter = new StringWriter();
            Console.SetOut(outputWriter);

            // Act
            Program.Main(new string[] { "True" });

            // Assert
            // 🔥 Find the line containing "Result:"
            var fullOutput = outputWriter.ToString();
            var resultLine = fullOutput
                .Split(Environment.NewLine)
                .First(line => line.StartsWith("Result"));

            // Extract the number after "Result:"
            var numberPart = resultLine.Replace("Result:", "").Trim();
            int result = int.Parse(numberPart);
            Assert.InRange(result, minValue, maxValue);
        }

        [Theory]
        [InlineData("if(2<100) 100", "Result: 100")]
        [InlineData("if(2<3) \"yes\" else \"no\"", "Result: yes")]
        [InlineData("if(5<3) \"yes\" else \"no\"", "Result: no")]
        [InlineData("if(\"dog\"==\"dog\") x=2 else x=5", "Result: null")]
        [InlineData("if(\"dog\"==\"dog\") 2 else x=5", "Result: 2")]
        [InlineData("if(\"dog\"!=\"dog\") x=2 else x=5; x", "Result: 5")]

        public void TestCompiler_If_ReturnsCorrectResults(string input, string expectedOutput)
        {
            var outputWriter = ReadWriteToConsole(input);
            RunCompilerAndCompare(outputWriter, expectedOutput);
        }

        [Theory]
        [InlineData("print(435368)", "Result: 435368")]
        [InlineData("print(574+8)", "Result: 582")]
        [InlineData("print(574+8 == 582)", "Result: true")]
        [InlineData("print(\"harry\")", "Result: harry")]
        [InlineData("print(\"Run\")", "Result: Run")]
        [InlineData("print(true)", "Result: true")]
        [InlineData("print(false)", "Result: false")]
        [InlineData("print(\"harry\" + \" potter\")", "Result: harry potter")]
        [InlineData("x=575; print(x)", "Result: 575")]
        [InlineData("x=false; print(x)", "Result: false")]
        [InlineData("x=\"a pot\"; print(x)", "Result: a pot")]

        public void TestCompiler_Print_ReturnsCorrectResults(string input, string expectedOutput)
        {
            var outputWriter = ReadWriteToConsole(input);
            RunCompilerAndCompare(outputWriter, expectedOutput);
        }

        [Theory]
        [InlineData("for(i=0; i<5; i++) 2", "Result: null")]
        [InlineData("for(i=0; i<5; i++;) 2", "Result: null")]
        [InlineData("for(i=100; i>10; i--) i*i", "Result: null")]

        public void TestCompiler_Loop_ReturnsCorrectResults(string input, string expectedOutput)
        {
            var outputWriter = ReadWriteToConsole(input);
            RunCompilerAndCompare(outputWriter, expectedOutput);
        }

        [Theory]
        [InlineData("for(i=0;i<5;i++)print(i)", "0\n1\n2\n3\n4\n5\n6\n7\n8\n9\n")]
        [InlineData("for(i=0;i<3;i++;)print(i)", "0\n1\n2\n")]
        [InlineData("for(i=2;i<5;i++)print(i)", "2\n3\n4\n")]
        public void ForLoop_WithPrint_PrintsCorrectNumbers(string input, string expectedOutput)
        {
            Console.SetIn(new StringReader(input));

            var outputWriter = new StringWriter();
            Console.SetOut(outputWriter);
            // Run your compiler
            Program.Main(new string[] { "True" });

            var output = Program.OutputBuffer.ToString();
            System.Console.WriteLine("output:" + output);

            var lines = outputWriter.ToString()
                .Split(Environment.NewLine)
                .Where(line => line.All(c => char.IsDigit(c))) // keep only number lines
                .ToArray();

            var filteredOutput = string.Join("\n", lines) + "\n";
            Assert.Equal(expectedOutput, filteredOutput);

            // // Get the captured output
            // var actualOutput = outputWriter.ToString();

            // // Assert that printed output matches exactly
            // Assert.Contains(expectedOutput, actualOutput);
        }

        [Theory]
        [InlineData("for(i=0;i<5;i++)print(i)", "0\n1\n2\n3\n4\n5\n6\n7\n8\n9\n")]
        [InlineData("for(i=0;i<3;i++;)print(i)", "0\n1\n2\n")]
        [InlineData("for(i=2;i<5;i++)print(i)", "2\n3\n4\n")]
        public void ForLoop_Array_PrintsCorrectResults(string input, string expectedOutput)
        {
            Console.SetIn(new StringReader(input));

            var outputWriter = new StringWriter();
            Console.SetOut(outputWriter);
            // Run your compiler
            Program.Main(new string[] { "True" });

            var output = Program.OutputBuffer.ToString();
            System.Console.WriteLine("output:" + output);

            var lines = outputWriter.ToString()
                .Split(Environment.NewLine)
                .Where(line => line.All(c => char.IsDigit(c))) // keep only number lines
                .ToArray();

            var filteredOutput = string.Join("\n", lines) + "\n";
            Assert.Equal(expectedOutput, filteredOutput);

            // // Get the captured output
            // var actualOutput = outputWriter.ToString();

            // // Assert that printed output matches exactly
            // Assert.Contains(expectedOutput, actualOutput);   
        }
    }
}