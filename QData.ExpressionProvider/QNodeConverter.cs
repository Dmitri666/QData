// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionProvider.cs" company="">
//   
// </copyright>
// <summary>
//   The repository impl.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QData.ExpressionProvider
{
    using System.Linq;
    using System.Linq.Expressions;

    using Qdata.Contract;

    using QData.ExpressionProvider.Builder;
    using QData.ExpressionProvider.PreProcessor;

    public class QNodeConverter
    {
        private readonly IQueryable query;

        public QNodeConverter(IQueryable query)
        {
            this.query = query;
        }

        public Expression Convert(QNode descriptor)
        {
            QNode root = new PreConverter().Prepare(descriptor);
            var converter = new Builder.QNodeConverter(this.query);
            root.Accept(converter);
            return converter.ContextExpression.Pop();
        }
    }
}