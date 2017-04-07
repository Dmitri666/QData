using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QData.Common;

namespace QData.ExpressionProvider.builder
{
    public class EnumResolver
    {
        public static MethodType ResolveMethod(object value)
        {
            MethodType method;
            if (value is long)
            {
                method = (MethodType)Convert.ToInt16(value);
            }
            else
            {
                Enum.TryParse(Convert.ToString(value), out method);
            }

            return method;
        }
    }
}
