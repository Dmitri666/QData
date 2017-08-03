using System;
using System.Linq.Expressions;
using Qdata.Contract;

namespace QData.ExpressionProvider.Converters
{
    public class ToLowerConverter : DefaultConverter
    {
        public ToLowerConverter(Type target) :base(target)
        {
        }

        

        public override Expression ConvertToConstant(QNode node)
        {
            string value = "";
            var valueType = node.Value.GetType();
            if (valueType != typeof(string))
            {
                value = Convert.ToString(node.Value);

            }
            else
            {
                value = (string)node.Value;
            }

            value = value.ToLower();

            return Expression.Constant(value);

        }
    }
}