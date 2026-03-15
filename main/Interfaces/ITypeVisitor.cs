namespace MyCompiler
{
    public interface ITypeVisitor
    {
        Type VisitSequence(SequenceNodeExpr expr);
        Type VisitNumber(NumberNodeExpr expr);
        Type VisitFloat(FloatNodeExpr expr);
        Type VisitString(StringNodeExpr expr);
        Type VisitBoolean(BooleanNodeExpr expr);
        Type VisitBinary(BinaryOpNodeExpr expr);
        Type VisitComparison(ComparisonNodeExpr expr);
        Type VisitIncrement(IncrementNodeExpr expr);
        Type VisitDecrement(DecrementNodeExpr expr);
        Type VisitId(IdNodeExpr expr);
        Type VisitAssign(AssignNodeExpr expr);
        Type VisitRandom(RandomNodeExpr expr);
        Type VisitPrint(PrintNodeExpr expr);
        Type VisitIf(IfNodeExpr expr);
        Type VisitForLoop(ForLoopNodeExpr expr);
        Type VisitForEachLoop(ForEachLoopNodeExpr expr);
        Type VisitArray(ArrayNodeExpr expr);
        Type VisitIndex(IndexNodeExpr expr);
        Type VisitRound(RoundNodeExpr expr);
        // Type VisitFunctionDef(FunctionDefNode expr);
        // Type VisitFunctionCall(FunctionCallNode expr);
    }
}