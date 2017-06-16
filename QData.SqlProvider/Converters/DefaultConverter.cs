using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Qdata.Json.Contract;

namespace QData.ExpressionProvider.Converters
{
    public class DefaultConverter : BaseConstantConverter
    {
        public DefaultConverter(Type target) : base(target)
        {

        }

        public override ConstantExpression ConvertToConstant(QNode node)
        {
            var valueType = node.Value.GetType();

            var nullableUnderlyingType = Nullable.GetUnderlyingType(target);
            if (nullableUnderlyingType == null)
            {

                if (target == valueType)
                {
                    return Expression.Constant(node.Value);

                }
                return Expression.Constant(node.Value, target);
            }

            if (nullableUnderlyingType != valueType)
            {
                var value = Convert.ChangeType(node.Value, nullableUnderlyingType);
                var exp1 = Expression.Constant(value);
                return Expression.Constant(Expression.Convert(exp1, target));
            }
            else
            {
                var exp1 = Expression.Constant(node.Value);
                return Expression.Constant(Expression.Convert(exp1, target));
            }

        }
    }
}
