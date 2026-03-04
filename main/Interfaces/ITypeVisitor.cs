namespace MyCompiler
{
    public interface ITypeVisitor
    {
        MyType VisitNumber(NumberNodeExpr expr);
        // MyType VisitString(StringNodeExpr expr);
        MyType VisitBoolean(BooleanNodeExpr expr);
        MyType VisitComparison(ComparisonNodeExpr expr);
        MyType VisitIncrement(IncrementNodeExpr expr);
        MyType VisitDecrement(DecrementNodeExpr expr);
        MyType VisitId(IdNodeExpr expr);
        MyType VisitBinary(BinaryOpNodeExpr expr);
        MyType VisitAssign(AssignNodeExpr expr);
        MyType VisitSequence(SequenceNodeExpr expr);
        // MyType VisitRandom(RandomNodeExpr expr);
        // MyType VisitIf(IfNodeExpr expr);
        // MyType VisitPrint(PrintNodeExpr expr);
        MyType VisitForLoop(ForLoopNodeExpr expr);
        MyType VisitArray(ArrayNodeExpr expr);
        // MyType VisitIndex(IndexNodeExpr expr);
        // MyType VisitFunctionDef(FunctionDefNode expr);
        // MyType VisitFunctionCall(FunctionCallNode expr);
        // MyType VisitRound(RoundNodeExpr expr);
        // MyType VisitFloat(FloatNodeExpr expr);
    }
}