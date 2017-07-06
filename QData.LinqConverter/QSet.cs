using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Qdata.Contract;

namespace QData.LinqConverter
{
    public class QSet<T> : EnumerableQuery<T>, IQSet

    {
        public QSet()
            : base(new List<T>())
        {
        }

        public QSet(IEnumerable<T> enumerable)
            : base(enumerable)
        {
        }

        public QSet(Expression expression)
            : base(expression)
        {
        }

        public QDescriptor ConvertToQDescriptor(IQueryable query)
        {
            var con = new ExpressionConverter();
            var root = con.Convert(query.Expression);
            return new QDescriptor {Root = root};
        }
    }
}