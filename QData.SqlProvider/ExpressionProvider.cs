// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionProvider.cs" company="">
//   
// </copyright>
// <summary>
//   The repository impl.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using QData.ExpressionProvider.builder;
using QData.ExpressionProvider.Builder;

namespace QData.ExpressionProvider
{
    using System.Linq;
    using System.Linq.Expressions;

    using Qdata.Json.Contract;

    using QData.Common;

    /// <summary>
    ///     The repository impl.
    /// </summary>
    /// <typeparam name="TEntity">
    /// </typeparam>
    public class ExpressionProvider
    {
        #region Fields

        private readonly QDescriptorConverter converter;

        #endregion

        #region Constructors and Destructors

        public ExpressionProvider(IQueryable query)
        {
            this.converter = new QDescriptorConverter(query.Expression);
        }

        #endregion

        #region Public Methods and Operators

        public Expression ConvertToExpression(QDescriptor descriptor)
        {
            descriptor.Root.Accept(this.converter);
            return this.converter.ContextExpression.Pop();
        }

        #endregion
    }
}