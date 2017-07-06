using System;
using System.Linq.Expressions;
using Qdata.Contract;

namespace QData.ExpressionProvider.Converters
{
    public class IntegerConverter : DefaultConverter
    {
        public IntegerConverter(Type target) : base(target)
        {
        }

        public override Expression ConvertToConstant(QNode node)
        {
            return Expression.Constant(Convert.ToInt32(node.Value));
        }
    }
}