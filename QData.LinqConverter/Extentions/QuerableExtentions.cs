using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QData.LinqConverter.Extentions
{
    using System.Linq.Expressions;
    using System.Reflection;

    public static class QuerableExtentions
    {
        static readonly MethodInfo queryStringWithFieldsMethodInfo = typeof(QuerableExtentions).GetMethodInfo(m => m.Name == "QueryString" && m.GetParameters().Length > 2);
        public static IQueryable<TSource> QueryString<TSource>(this EnumerableQuery<TSource> source, string query, Expression<Func<TSource, object[]>> fields)
        {
            
            return CreateQueryMethodCall(source, queryStringWithFieldsMethodInfo, Expression.Constant(query), fields);
        }

        static IQueryable<TSource> CreateQueryMethodCall<TSource>(IQueryable<TSource> source, MethodInfo method, params Expression[] arguments)
        {
            var callExpression = Expression.Call(null, method.MakeGenericMethod(typeof(TSource)), new[] { source.Expression }.Concat(arguments));
            return source.Provider.CreateQuery<TSource>(callExpression);
        }

        static MethodInfo GetMethodInfo(this Type type, Func<MethodInfo, bool> predicate)
        {
            return type.GetTypeInfo().DeclaredMethods.Single(predicate);
        }
    }
}
