// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Repository.cs" company="">
//   
// </copyright>
// <summary>
//   The repository.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Qdata.Json.Contract;
using QData.Common;
using QData.ExpressionProvider;

namespace QData.Querable.DataService
{
    public class DataService
    {
        public object Search<TM>(QDescriptor<TM> descriptor, IQueryable<TM> source) where TM : IModelEntity
        {
            var provider = new ExpressionProvider<TM>(source);
            var expression = provider.ConvertToExpression(descriptor);

            
            var data = source.Provider.CreateQuery(expression);

            var listType = typeof(List<>);
            var targetType = listType.MakeGenericType(data.ElementType);
            var result = Activator.CreateInstance(targetType);
            var methodInfo = targetType.GetMethod("AddRange");
            methodInfo.Invoke(result, new object[] { data });

            return result;
        }

        public Page GetPage<TM>(QDescriptor<TM> descriptor, IQueryable<TM> source, int skip,int take) where TM : IModelEntity
        {
            var provider = new ExpressionProvider<TM>(source);
            
            var expression = provider.ConvertToExpression(descriptor);
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