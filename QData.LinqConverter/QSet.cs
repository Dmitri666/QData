using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QData.LinqConverter
{
    using System.Linq.Expressions;

    using Qdata.Json.Contract;

    using QData.Common;

    public class QSet<T> : EnumerableQuery<T> , IQSet
        where T : IModelEntity
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

        public QDescriptor<T> ConvertToQDescriptor(IQueryable query)
        {
            var con = new ExpressionConverter();
            var root = con.Convert(query.Expression);
            return new QDescriptor<T>() { Root = root };
        }
    }
}
