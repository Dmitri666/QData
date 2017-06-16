using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Qdata.Json.Contract;

namespace QData.ExpressionProvider.Converters
{
    public abstract class BaseConstantConverter
    {
        protected Type target { get; set; }

        public BaseConstantConverter(Type target)
        {
            this.target = target;
        }
        public abstract ConstantExpression ConvertToConstant(QNode node);
    }
}
