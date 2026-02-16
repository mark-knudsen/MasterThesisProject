using LLVMSharp.Interop;
using LLVMSharp;

namespace MyCompiler
{
    public interface IExpressionVisitor
    {
        
        LLVMValueRef VisitBinaryExpr(BinaryOpNodeExpr expr);
        // LLVMValueRef VisitCall(CallExpression expr);
        // LLVMValueRef VisitFor(ForExpression expr);
        // LLVMValueRef VisitFunction(FunctionExpression expr);
        // LLVMValueRef VisitIf(IfExpression expr);
        LLVMValueRef VisitNumberExpr(NumberNodeExpr expr);
        LLVMValueRef VisitStringExpr(StringNodeExpr expr);
        LLVMValueRef VisitIncrementExpr(IncrementNodeExpr expr);
        LLVMValueRef VisitDecrementExpr(DecrementNodeExpr expr);
        LLVMValueRef VisitSequenceExpr(SequenceNodeExpr expr);
        // LLVMValueRef VisitPrototype(PrototypeExpression expr);
        LLVMValueRef VisitIdExpr(IdNodeExpr expr);
        LLVMValueRef VisitAssignExpr(AssignNodeExpr expr);
        // LLVMValueRef VisitExtern(ExternExpression expr);
        // LLVMValueRef VisitUnary(UnaryExpression expr);
        // LLVMValueRef VisitVarInExpression(VarInExpression expr);
    }
    
}