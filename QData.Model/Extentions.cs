// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Repository.cs" company="">
//   
// </copyright>
// <summary>
//   The repository.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QData.Querable.Extentions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using QData.Common;

    public static class Extentions
    {
        public static object Execute(this IQueryable source,Expression expression) 
        {
            var data = source.Provider.CreateQuery(expression);

            var listType = typeof(List<>);
            var targetType = listType.MakeGenericType(data.ElementType);
            var result = Activator.CreateInstance(targetType);
            var methodInfo = targetType.GetMethod("AddRange");
            methodInfo.Invoke(result, new object[] { data });

            return result;
        }

        public static Page GetPage(this IQueryable source, Expression expression, int skip,int take) 
        {
            var type = expression.Type.GenericTypeArguments[0];
            var countExp = Expression.Call(typeof(Queryable), "Count", new[] { type }, expression);
            var skipExpression = Expression.Call(typeof(Queryable), "Skip", new[] { type }, expression, Expression.Constant(skip));
            var takeExpression = Expression.Call(typeof(Queryable), "Take", new[] { type }, skipExpression, Expression.Constant(take));
            

            var data = source.Provider.CreateQuery(takeExpression);

            var listType = typeof(List<>);
            var targetType = listType.MakeGenericType(data.ElementType);
            var result = Activator.CreateInstance(targetType);
            var methodInfo = targetType.GetMethod("AddRange");
            methodInfo.Invoke(result, new object[] { data });






            return new Page()
            {
                Total = (int)source.Provider.Execute(countExp),
                Data = result
            };
        }


    }
}