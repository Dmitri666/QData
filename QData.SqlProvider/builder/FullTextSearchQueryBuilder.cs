using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using QData.ExpressionProvider.builder;

namespace QData.ExpressionProvider.Builder
{
    internal class FullTextSearchQueryBuilder
    {
        private IQueryable query;
        public FullTextSearchQueryBuilder(IQueryable query, string queryString)
        {
            this.query = query;
            ContextExpression = new Stack<Expression>();
            Parameter = Expression.Parameter(query.ElementType, "x");
            QueryString = Expression.Constant(queryString);
            MemberContext = new Stack<MemberExpression>();
            Members = new List<MemberExpression>();
        }

        private ParameterExpression Parameter { get; }
        private Stack<Expression> ContextExpression { get; set; }
        private ConstantExpression QueryString { get; set; }
        private Stack<MemberExpression> MemberContext { get; }
        private List<MemberExpression> Members { get; }

        public Expression GetExpression()
        {
            foreach (var property in Parameter.Type.GetProperties())
            {
                BuildMembers(property);
            }

            var containsExpressions = Members.Select(memberExpression => GetContainExpression(memberExpression)).ToList();

            Expression result = null;
            foreach (var containsExpression in containsExpressions)
            {
                if (result == null)
                {
                    result = containsExpression;
                }
                else
                {
                    result = Expression.Or(result,containsExpression);
                }
                
            }
            result = Expression.Lambda(result, this.Parameter);
            var types = new List<Type> { this.query.Expression.Type.GenericTypeArguments[0] };
            result = Expression.Call(typeof(Queryable), "Where", types.ToArray(), this.query.Expression, Expression.Quote(result));

            return result;
        }

        public Expression GetContainExpression(MemberExpression member)
        {
            Expression resultExpression;
            
            var nullable = Nullable.GetUnderlyingType(member.Type);
            if (nullable != null)
            {
                Type memberType = nullable;
                var method = memberType.GetMethod("Contains", new Type[] { typeof(string) });
                if (method != null)
                {
                    resultExpression = Expression.Call(member, method, this.QueryString);
                }
                else
                {
                    var toStringMethod = member.Type.GetMethod("ToString", new Type[] { });

                    var exp1 = Expression.Call(member, toStringMethod, null);

                    resultExpression = Expression.Call(exp1, Methods.Contains, this.QueryString);

                    var exp = Expression.NotEqual(member, Expression.Constant(null, member.Type));
                    resultExpression = Expression.AndAlso(exp, resultExpression);
                }
                

            }
            else
            {
                var method = member.Type.GetMethod("Contains", new Type[] { typeof(string) });
                if (method != null)
                {
                    resultExpression = Expression.Call(member, method, this.QueryString);
                }
                else
                {
                    var toStringMethod = member.Type.GetMethod("ToString", new Type[] { });
                    var exp1 = Expression.Call(member, toStringMethod, null);

                    resultExpression = Expression.Call(exp1, Methods.Contains, this.QueryString);
                }
                


                
            }

            return resultExpression;

        }

        private void BuildMembers(PropertyInfo property)
        {
            Type propertyType;
            var nullable = Nullable.GetUnderlyingType(property.PropertyType);
            if (nullable != null)
            {
                propertyType = nullable;
            }
            else
            {
                propertyType = property.PropertyType;
            }

            if (propertyType.IsPrimitive || propertyType == typeof (string) ||
                propertyType == typeof (DateTime))
            //if(property.PropertyType.IsValueType)
            {
                BuildMember(property);
            }
            else
            {
                var properties = property.PropertyType.GetProperties(BindingFlags.Public
                                                                     | BindingFlags.Instance
                                                                     | BindingFlags.DeclaredOnly);

                if (MemberContext.Count > 0)
                {
                    var member = Expression.PropertyOrField(MemberContext.Peek(), property.Name);
                    MemberContext.Push(member);
                }
                else
                {
                    var member = Expression.PropertyOrField(Parameter, property.Name);
                    MemberContext.Push(member);
                }


                foreach (var childProperty in properties)
                {
                    BuildMembers(childProperty);
                }
                MemberContext.Pop();
            }
        }

        private void BuildMember(PropertyInfo property)
        {
            if (MemberContext.Count > 0)
            {
                var member = Expression.PropertyOrField(MemberContext.Peek(), property.Name);
                Members.Add(member);
            }
            else
            {
                var member = Expression.PropertyOrField(Parameter, property.Name);
                Members.Add(member);
            }
        }
    }
}