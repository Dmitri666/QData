namespace QData.QueryContainerProvider
{
    using Qdata.Contract;

    public interface IQNodeVisitor
    {
        void VisitBinary(QNode node);

        void VisitMember(QNode node);

        void VisitMethod(QNode node);

        void VisitConstant(QNode node);

        void VisitProjection(QNode node);

        void VisitEmptyMethod(QNode node);


        void SetConstantConverter(QNode node);

    }
}
