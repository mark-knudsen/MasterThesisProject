namespace MyCompiler;

interface ICompiler
{
    public object Run(Node expr, bool generateIR = false, bool useStopWatch = false);
    public Context GetContext();
    public void ClearContext();
}