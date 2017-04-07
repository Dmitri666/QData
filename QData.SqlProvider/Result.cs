namespace QData.ExpressionProvider
{
    using System.Linq.Expressions;

    public class Result
    {
        public Expression Expression { get; set; }

        
        public bool HasProjection { get; set; }
    }
}
