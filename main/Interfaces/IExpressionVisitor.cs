using LLVMSharp.Interop;
using LLVMSharp;

namespace MyCompiler
{
    public interface IExpressionVisitor
    {
        LLVMValueRef VisitSequence(SequenceNode expr);
        LLVMValueRef VisitNumber(NumberNode expr);
        LLVMValueRef VisitFloat(FloatNode expr);
        LLVMValueRef VisitString(StringNode expr);
        LLVMValueRef VisitBoolean(BooleanNode expr);
        LLVMValueRef VisitNull(NullNode expr);
        LLVMValueRef VisitUnaryOp(UnaryOpNode expr);
        LLVMValueRef VisitBinary(BinaryOpNode expr);
        LLVMValueRef VisitLogicalOp(LogicalOpNode expr);
        LLVMValueRef VisitComparison(ComparisonNode expr);
        LLVMValueRef VisitIncrement(IncrementNode expr);
        LLVMValueRef VisitDecrement(DecrementNode expr);
        LLVMValueRef VisitId(IdNode expr);
        LLVMValueRef VisitAssign(AssignNode expr);
        LLVMValueRef VisitRandom(RandomNode expr);
        LLVMValueRef VisitPrint(PrintNode expr);
        LLVMValueRef VisitIf(IfNode expr);
        LLVMValueRef VisitForLoop(ForLoopNode expr);
        LLVMValueRef VisitForEachLoop(ForEachLoopNode expr);
        LLVMValueRef VisitArray(ArrayNode expr);
        LLVMValueRef VisitIndex(IndexNode expr);
        LLVMValueRef VisitIndexAssign(IndexAssignNode expr);
        LLVMValueRef VisitAdd(AddNode expr);
        LLVMValueRef VisitAddRange(AddRangeNode expr);
        LLVMValueRef VisitRemove(RemoveNode expr);
        LLVMValueRef VisitRemoveRange(RemoveRangeNode expr);
        LLVMValueRef VisitLength(LengthNode expr);
        LLVMValueRef VisitMin(MinNode expr);
        LLVMValueRef VisitMax(MaxNode expr);
        LLVMValueRef VisitMean(MeanNode expr);
        LLVMValueRef VisitSum(SumNode expr);
        LLVMValueRef VisitCorrelation(CorrelationNode expr);
        LLVMValueRef VisitWhere(WhereNode expr);
        LLVMValueRef VisitMap(MapNode expr);

        LLVMValueRef VisitReadCsv(ReadCsvNode expr);
        LLVMValueRef VisitToCsv(ToCsvNode expr);

        LLVMValueRef VisitRound(RoundNode expr);
        LLVMValueRef VisitRecord(RecordNode expr);
        LLVMValueRef VisitField(FieldNode expr);
        LLVMValueRef VisitRecordFieldAssign(RecordFieldAssignNode expr);
        LLVMValueRef VisitCopy(CopyNode expr);
        LLVMValueRef VisitDataframe(DataframeNode expr);
        LLVMValueRef VisitColumns(ColumnsNode expr);
        LLVMValueRef VisitShowDataframe(ShowDataframeNode expr);
        LLVMValueRef VisitNamedArgument(NamedArgumentNode expr);
        LLVMValueRef VisitTypeLiteral(TypeLiteralNode expr);
        LLVMValueRef VisitSqrt(SqrtNode expr);
        LLVMValueRef VisitLog(LogNode expr);
        LLVMValueRef VisitPow(PowNode expr);
        LLVMValueRef VisitExponentialMathFunc(ExponentialMathFuncNode expr);
        LLVMValueRef VisitCast(CastNode expr);

        LLVMValueRef VisitAddField(AddFieldNode expr);
        LLVMValueRef VisitRemoveField(RemoveFieldNode expr);

        // // Visit for define and usage of created functions
        // LLVMValueRef VisitFunctionDef(FunctionDefNode node);
        // LLVMValueRef VisitFunctionCall(FunctionCallNode node);
    }
}