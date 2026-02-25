namespace MyCompiler;
interface ICompiler
{
    public object Run(NodeExpr expr, bool generateIR = false);
}