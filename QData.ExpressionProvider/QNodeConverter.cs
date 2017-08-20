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
        private readonly QNodeConverterSettings settings;

        public QNodeConverter()
        {
            this.settings = new QNodeConverterSettings();
        }
        public QNodeConverter(QNodeConverterSettings settings)
        {
            this.settings = settings;
        }

        public Expression Convert(IQueryable baseQuery, QNode descriptor)
        {
            QNode root = new PreConverter().Prepare(descriptor);
            var converter = new Builder.QNodeConverter(baseQuery, this.settings);
            root.Accept(converter);
            return converter.ContextExpression.Pop();
        }
    }
}