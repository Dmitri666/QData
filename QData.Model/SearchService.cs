// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Repository.cs" company="">
//   
// </copyright>
// <summary>
//   The repository.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QData.SearchService
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;

    using Qdata.Json.Contract;

    using QData.Common;
    using QData.ExpressionProvider;

    public class SearchService
    {
        public object Search<TM>(QDescriptor<TM> descriptor, IQueryable<TM> query) where TM : IModelEntity
        {
            var provider = new ExpressionProvider<TM>(query);
            var expression = provider.ConvertToExpression(descriptor);

            var data = query.Provider.CreateQuery(expression);

            var listType = typeof(List<>);
            var targetType = listType.MakeGenericType(data.ElementType);
            var result = Activator.CreateInstance(targetType);
            var methodInfo = targetType.GetMethod("AddRange");
            methodInfo.Invoke(result, new object[] { data });

            return result;
        }

        public int Count<TM>(QDescriptor<TM> descriptor, IQueryable<TM> query) where TM : IModelEntity
        {
            var provider = new ExpressionProvider<TM>(query);
            
            var expression = provider.ConvertToExpression(descriptor);
            var parameter = Expression.Parameter(query.ElementType, string.Format("x"));
            var lambda = Expression.Lambda(Expression.Constant(true), parameter);
            var countExp = Expression.Call(typeof(Queryable), "Count", new[] { query.ElementType }, expression, lambda);


            var data = query.Provider.CreateQuery(countExp);

            

            return 0;
        }


    }
}