using LLVMSharp.Interop;
using LLVMSharp;

namespace MyCompiler
{
    public interface IExpressionVisitor
    {
        LLVMValueRef VisitSequenceExpr(SequenceNodeExpr expr);
        LLVMValueRef VisitNumberExpr(NumberNodeExpr expr);
        LLVMValueRef VisitFloatExpr(FloatNodeExpr expr);
        LLVMValueRef VisitStringExpr(StringNodeExpr expr);
        LLVMValueRef VisitBooleanExpr(BooleanNodeExpr expr);
        LLVMValueRef VisitUnaryOpExpr(UnaryOpNodeExpr expr);
        LLVMValueRef VisitBinaryExpr(BinaryOpNodeExpr expr);
        LLVMValueRef VisitComparisonExpr(ComparisonNodeExpr expr);
        LLVMValueRef VisitIncrementExpr(IncrementNodeExpr expr);
        LLVMValueRef VisitDecrementExpr(DecrementNodeExpr expr);
        LLVMValueRef VisitIdExpr(IdNodeExpr expr);
        LLVMValueRef VisitAssignExpr(AssignNodeExpr expr);
        LLVMValueRef VisitRandomExpr(RandomNodeExpr expr);
        LLVMValueRef VisitPrintExpr(PrintNodeExpr expr);
        LLVMValueRef VisitIfExpr(IfNodeExpr expr);
        LLVMValueRef VisitForLoopExpr(ForLoopNodeExpr expr);
        LLVMValueRef VisitForEachLoopExpr(ForEachLoopNodeExpr expr);
        LLVMValueRef VisitArrayExpr(ArrayNodeExpr expr);
        LLVMValueRef VisitIndexExpr(IndexNodeExpr expr);
        LLVMValueRef VisitAddExpr(AddNodeExpr expr);
        LLVMValueRef VisitAddRangeExpr(AddRangeNodeExpr expr);
        LLVMValueRef VisitRemoveExpr(RemoveNodeExpr expr);
        LLVMValueRef VisitRemoveRangeExpr(RemoveRangeNodeExpr expr);
        LLVMValueRef VisitLengthExpr(LengthNodeExpr expr);
        LLVMValueRef VisitMinExpr(MinNodeExpr expr);
        LLVMValueRef VisitMaxExpr(MaxNodeExpr expr);
        LLVMValueRef VisitMeanExpr(MeanNodeExpr expr);
        LLVMValueRef VisitSumExpr(SumNodeExpr expr);
        LLVMValueRef VisitWhereExpr(WhereNodeExpr expr);
        LLVMValueRef VisitRoundExpr(RoundNodeExpr expr);

        // // Visit for define and usage of created functions
        // LLVMValueRef VisitFunctionDef(FunctionDefNode node);
        // LLVMValueRef VisitFunctionCall(FunctionCallNode node);
    }
}