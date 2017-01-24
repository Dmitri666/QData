// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionProvider.cs" company="">
//   
// </copyright>
// <summary>
//   The repository impl.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq.Expressions;

using Qdata.Json.Contract;
using QData.SqlProvider.builder;

namespace QData.SqlProvider
{
    using System.Linq;

    using QData.Common;
    /// <summary>
    ///     The repository impl.
    /// </summary>
    /// <typeparam name="TEntity">
    /// </typeparam>
    public class ExpressionProvider<TM> where TM : IModelEntity
    {
        #region Fields

        private readonly QDescriptorConverter converter;

        #endregion

        #region Constructors and Destructors

        public ExpressionProvider(IQueryable<TM> query)
        {
            this.converter = new QDescriptorConverter(query.Expression);
        }

        #endregion

        #region Public Methods and Operators

        public Expression ConvertToExpression(QDescriptor<TM> descriptor)
        {
            descriptor.Root.Accept(this.converter);
            return this.converter.ContextExpression.Pop();
        }

        #endregion
    }
}