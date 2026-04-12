namespace MyCompiler;

interface ICompiler
{
    public object Run(Node expr, bool generateIR = false, bool useStopWatch = false, bool showAllColumns = false, bool showAllRows = false);
    public Context GetContext();
    public void ClearContext();
}