namespace MyCompiler;

interface ICompiler
{
    public object Run(Node expr, bool generateIR = false);
    public Context GetContext();
    public void ClearContext();
}