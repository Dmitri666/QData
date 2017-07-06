using System;
using Newtonsoft.Json;

namespace Qdata.Contract
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

        public static BinaryType ResolveBinary(object value)
        {
            BinaryType op;
            if (value is long)
            {
                op = (BinaryType)Convert.ToInt16(value);
            }
            else
            {
                Enum.TryParse(Convert.ToString(value), out op);
            }

            return op;
        }

        
    }
}
