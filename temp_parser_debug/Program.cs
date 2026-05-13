using System;
using System.IO;
using MyCompiler;
class P
{
    static void Main()
    {
        var input = "df=dataframe(schema={name: string, age: int, hasJob: bool}, rows=[{\"Alice\", 25, true},{\"Bob\", 30, false},{\"Charlie\", 22, true}]);";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(input));
        var scanner = new Scanner(stream);
        var parser = new Parser(scanner);
        if (!parser.Parse() || parser.RootNode == null)
        {
            Console.WriteLine("parse failed");
            return;
        }
        Console.WriteLine(parser.RootNode.GetType().Name);
        if (parser.RootNode is SequenceNode seq)
        {
            foreach (var stmt in seq.Statements)
                Console.WriteLine(stmt.GetType().Name);
            if (seq.Statements.Count > 0 && seq.Statements[0] is AssignNode assign && assign.Expression is DataframeNode df)
            {
                Console.WriteLine($"Schema null={df.Schema==null}, Columns null={df.Columns==null}, Types null={df.Types==null}, Rows null={df.Rows==null}");
                if (df.Schema != null)
                    Console.WriteLine($"Schema type={df.Schema.GetType().Name}, fields={df.Schema.Fields.Count}");
            }
        }
    }
}
