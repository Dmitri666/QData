using System.Reflection;

namespace QData.ExpressionProvider.Builder
{
    using System;

    public static class Methods
    {
        public static MethodInfo Contains;
        public static MethodInfo StartsWith;
        public static MethodInfo EndsWith;
        public static MethodInfo ToLower;

        static Methods()
        {
            Contains = typeof (string).GetMethod("Contains");

            StartsWith = typeof (string).GetMethod("StartsWith", new[] {typeof (string)});

            EndsWith = typeof (string).GetMethod("EndsWith", new[] {typeof (string)});

            ToLower = typeof(string).GetMethod("ToLower", new Type[0]);
        }
    }
}