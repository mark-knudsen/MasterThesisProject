namespace MyCompiler
{
    public interface ITypeVisitor
    {
        Type VisitSequence(SequenceNode expr);
        Type VisitNumber(NumberNode expr);
        Type VisitFloat(FloatNode expr);
        Type VisitString(StringNode expr);
        Type VisitBoolean(BooleanNode expr);
        Type VisitNull(NullNode expr);
        Type VisitBinary(BinaryOpNode expr);
        Type VisitLogical(LogicalOpNode expr);
        Type VisitComparison(ComparisonNode expr);
        Type VisitIncrement(IncrementNode expr);
        Type VisitDecrement(DecrementNode expr);
        Type VisitId(IdNode expr);
        Type VisitAssign(AssignNode expr);
        Type VisitRandom(RandomNode expr);
        Type VisitPrint(PrintNode expr);
        Type VisitIf(IfNode expr);
        Type VisitForLoop(ForLoopNode expr);
        Type VisitForEachLoop(ForEachLoopNode expr);
        Type VisitArray(ArrayNode expr);
        Type VisitIndex(IndexNode expr);
        Type VisitIndexAssign(IndexAssignNode expr);
        Type VisitAdd(AddNode expr);
        Type VisitAddRange(AddRangeNode expr);
        Type VisitRemove(RemoveNode expr);
        Type VisitRemoveRange(RemoveRangeNode expr);
        Type VisitLength(LengthNode expr);
        Type VisitMin(MinNode expr);
        Type VisitMax(MaxNode expr);
        Type VisitMean(MeanNode expr);
        Type VisitSum(SumNode expr);
        Type VisitWhere(WhereNode expr);
        Type VisitMap(MapNode expr);
        Type VisitReadCsv(ReadCsvNode expr);
        Type VisitRound(RoundNode expr);
        Type VisitRecord(RecordNode expr);
        Type VisitRecordField(RecordFieldNode expr);
        Type VisitRecordFieldAssign(RecordFieldAssignNode expr);
        Type VisitCopy(CopyNode expr);
        Type VisitDataframe(DataframeNode expr);
        Type VisitShowDataframe(ShowDataframeNode expr);
        Type VisitColumns(ColumnsNode expr);
        Type VisitNamedArgument(NamedArgumentNode expr);
        Type VisitTypeLiteral(TypeLiteralNode expr);
        Type VisitSqrt(SqrtNode expr);


        // Type VisitFunctionDef(FunctionDefNode expr);
        // Type VisitFunctionCall(FunctionCallNode expr);
    }
}