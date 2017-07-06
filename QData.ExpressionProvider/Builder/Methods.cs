using System.Reflection;

namespace QData.ExpressionProvider.Builder
{
    public static class Methods
    {
        public static MethodInfo Contains;
        public static MethodInfo StartsWith;
        public static MethodInfo EndsWith;

        static Methods()
        {
            Contains = typeof (string).GetMethod("Contains");

            StartsWith = typeof (string).GetMethod("StartsWith", new[] {typeof (string)});

            EndsWith = typeof (string).GetMethod("EndsWith", new[] {typeof (string)});
        }
    }
}