namespace MyCompiler
{
    public interface ITypeVisitor
    {
        MyType VisitSequence(SequenceNodeExpr expr);
        MyType VisitNumber(NumberNodeExpr expr);
        MyType VisitFloat(FloatNodeExpr expr);
        MyType VisitString(StringNodeExpr expr);
        MyType VisitBoolean(BooleanNodeExpr expr);
        MyType VisitBinary(BinaryOpNodeExpr expr);
        MyType VisitComparison(ComparisonNodeExpr expr);
        MyType VisitIncrement(IncrementNodeExpr expr);
        MyType VisitDecrement(DecrementNodeExpr expr);
        MyType VisitId(IdNodeExpr expr);
        MyType VisitAssign(AssignNodeExpr expr);
        MyType VisitRandom(RandomNodeExpr expr);
        MyType VisitPrint(PrintNodeExpr expr);
        MyType VisitIf(IfNodeExpr expr);
        MyType VisitForLoop(ForLoopNodeExpr expr);
        MyType VisitArray(ArrayNodeExpr expr);
        MyType VisitIndex(IndexNodeExpr expr);
        MyType VisitRound(RoundNodeExpr expr);
        // MyType VisitFunctionDef(FunctionDefNode expr);
        // MyType VisitFunctionCall(FunctionCallNode expr);
    }
}