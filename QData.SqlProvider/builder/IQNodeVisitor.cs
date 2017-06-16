using Qdata.Json.Contract;

namespace QData.ExpressionProvider.Builder
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IQNodeVisitor
    {
        void VisitBinary(QNode node);

        void VisitMember(QNode node);

        void VisitQuerable(QNode node);

        void VisitMethod(QNode node);

        void EnterContext(QNode node);

        void LeaveContext(QNode node);

        void VisitConstant(QNode node);

        void VisitProjection(QNode node);

        void VisitEmptyMethod(QNode node);


        Stack<Expression> ContextExpression { get; set; }

    }
}
