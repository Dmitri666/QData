﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.Linq;

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

            var data = query.Provider.CreateQuery(expression).Expression;

            

            return 0;
        }


    }
}