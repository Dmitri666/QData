using System;
using System.Linq.Expressions;
using Qdata.Contract;

namespace QData.ExpressionProvider.Converters
{
    public class DefaultConverter
    {
        public DefaultConverter(Type target)
        {
            this.target = target;
        }

        protected Type target { get; set; }

        public virtual Expression ConvertToConstant(QNode node)
        {
            var valueType = node.Value.GetType();

            var nullableUnderlyingType = Nullable.GetUnderlyingType(target);
            if (nullableUnderlyingType == null)
            {
                if (target == valueType)
                {
                    return Expression.Constant(node.Value);
                }

                var value = Convert.ChangeType(node.Value, target);
                return Expression.Constant(value, target);
            }

            if (nullableUnderlyingType != valueType)
            {
                var value = Convert.ChangeType(node.Value, nullableUnderlyingType);
                var exp1 = Expression.Constant(value);
                return Expression.Convert(exp1, target);
            }
            else
            {
                var exp1 = Expression.Constant(node.Value);
                return Expression.Constant(Expression.Convert(exp1, target));
            }
        }
    }
}