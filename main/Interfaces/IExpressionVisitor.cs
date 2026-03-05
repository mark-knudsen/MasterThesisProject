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
        LLVMValueRef VisitArrayExpr(ArrayNodeExpr expr);
        // LLVMValueRef VisitIndexExpr(IndexNodeExpr expr);
        // LLVMValueRef VisitRoundExpr(RoundNodeExpr expr);


        // // Visit for define and usage of created functions
        // LLVMValueRef VisitFunctionDef(FunctionDefNode node);
        // LLVMValueRef VisitFunctionCall(FunctionCallNode node);
    }
}