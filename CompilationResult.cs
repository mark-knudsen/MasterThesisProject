public class CompilationResult
{
    public MyCompiler.NodeExpr AST { get; set; }
    public string IR { get; set; }
    public object ExecutionResult { get; set; }
    public List<string> ConsoleOutput { get; set; } = new List<string>();
}
