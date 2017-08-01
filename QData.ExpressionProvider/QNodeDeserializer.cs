// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionProvider.cs" company="">
//   
// </copyright>
// <summary>
//   The repository impl.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using System.Linq.Expressions;
using Qdata.Contract;
using QData.ExpressionProvider.Builder;

namespace QData.ExpressionProvider
{
    using System.Collections.Generic;

    using QData.ExpressionProvider.PreProcessor;

    /// <summary>
    ///     The repository impl.
    /// </summary>
    /// <typeparam name="TEntity">
    /// </typeparam>
    public class QNodeDeserializer
    {
        #region Fields

        private readonly IQueryable query;

        #endregion

        #region Constructors and Destructors

        public QNodeDeserializer(IQueryable query)
        {
            this.query = query;
        }

        #endregion

        #region Public Methods and Operators

        public Expression Deserialize(QNode descriptor)
        {
            QNode root = descriptor;
            var providerType = this.query.Provider.GetType();
            if (providerType.Name.Contains("DbQueryProvider") || providerType.Name.Contains("EnumerableQuery"))
            {
                root = new DbQueryProviderPreprocessor().Prepare(root);
            }
            var converter = new QNodeConverter(this.query);
            
            root.Accept(converter);
            return converter.ContextExpression.Pop();
        }

        
        #endregion
    }
}