using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Qdata.Contract;

namespace QData.LinqConverter
{
    using System;

    public class EnumerableSource<T> : EnumerableQuery<T>, IEnumerableSource

    {
        
        public EnumerableSource()
            : base(new List<T>())
        {
            
        }

        public EnumerableSource(IEnumerable<T> enumerable)
            : base(enumerable)
        {
        }

        public EnumerableSource(Expression expression)
            : base(expression)
        {
        }


        


    }
}