using LLVMSharp.Interop;
using LLVMSharp;

namespace MyCompiler
{
    public interface IExpressionVisitor
    {

        LLVMValueRef VisitBinaryExpr(BinaryOpNodeExpr expr);
        LLVMValueRef VisitFloatExpr(FloatNodeExpr expr);
        LLVMValueRef VisitBooleanExpr(BooleanNodeExpr expr);
        // LLVMValueRef VisitCall(CallExpression expr);
        LLVMValueRef VisitForLoopExpr(ForLoopNodeExpr expr);
        // LLVMValueRef VisitFunction(FunctionExpression expr);
        LLVMValueRef VisitPrintExpr(PrintNodeExpr expr);
        LLVMValueRef VisitRandomExpr(RandomNodeExpr expr);
        LLVMValueRef VisitRoundExpr(RoundNodeExpr expr);
        LLVMValueRef VisitIfExpr(IfNodeExpr expr);
        LLVMValueRef VisitComparisonExpr(ComparisonNodeExpr expr);
        LLVMValueRef VisitNumberExpr(NumberNodeExpr expr);
        LLVMValueRef VisitStringExpr(StringNodeExpr expr);
        LLVMValueRef VisitIncrementExpr(IncrementNodeExpr expr);
        LLVMValueRef VisitDecrementExpr(DecrementNodeExpr expr);
        LLVMValueRef VisitSequenceExpr(SequenceNodeExpr expr);
        // LLVMValueRef VisitPrototype(PrototypeExpression expr);
        LLVMValueRef VisitIdExpr(IdNodeExpr expr);
        LLVMValueRef VisitAssignExpr(AssignNodeExpr expr);
        LLVMValueRef VisitArrayExpr(ArrayNodeExpr expr);
        // LLVMValueRef VisitExtern(ExternExpression expr);
        // LLVMValueRef VisitUnary(UnaryExpression expr);
        // LLVMValueRef VisitVarInExpression(VarInExpression expr);


        // Visit for define and usage of created functions
        LLVMValueRef VisitFunctionDef(FunctionDefNode node);
        LLVMValueRef VisitFunctionCall(FunctionCallNode node);
    }

}