using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QData.ExpressionProvider.builder
{
    public static class Methods
    {
        public static MethodInfo Contains;
        public static MethodInfo StartsWith;
        public static MethodInfo EndsWith;


        static Methods()
        {
            Methods.Contains = typeof (string).GetMethod("Contains");

            Methods.StartsWith = typeof(string).GetMethod("StartsWith",new[] { typeof(string) });

            Methods.EndsWith = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });

        }

    }
}
