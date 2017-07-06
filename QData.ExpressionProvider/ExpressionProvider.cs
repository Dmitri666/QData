﻿// --------------------------------------------------------------------------------------------------------------------
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
using QData.ExpressionProvider.builder;
using QData.ExpressionProvider.Builder;

namespace QData.ExpressionProvider
{
    /// <summary>
    ///     The repository impl.
    /// </summary>
    /// <typeparam name="TEntity">
    /// </typeparam>
    public class ExpressionProvider
    {
        #region Fields

        private readonly IQueryable query;

        #endregion

        #region Constructors and Destructors

        public ExpressionProvider(IQueryable query)
        {
            this.query = query;
        }

        #endregion

        #region Public Methods and Operators

        public Expression ConvertToExpression(QDescriptor descriptor)
        {
            var converter = new QDescriptorConverter(query);
            descriptor.Root.Accept(converter);
            return converter.ContextExpression.Pop();
        }

        #endregion
    }
}