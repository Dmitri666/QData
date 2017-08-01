using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Qdata.Contract;

namespace QData.LinqConverter
{
    using System;

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

        public QNode Serialize(IQueryable query)
        {
            return new ExpressionSerializer().Serialize(query.Expression);
        }

        
    }
}