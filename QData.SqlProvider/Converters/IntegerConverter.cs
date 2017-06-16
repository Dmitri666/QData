using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Qdata.Json.Contract;

namespace QData.ExpressionProvider.Converters
{
    public class IntegerConverter : BaseConstantConverter
    {
        public IntegerConverter(Type target) : base(target)
        {

        }

        public override ConstantExpression ConvertToConstant(QNode node)
        {
            return Expression.Constant(System.Convert.ToInt32(node.Value));
        }
    }
}
