using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QData.LinqConverter
{
    using Qdata.Json.Contract;

    public static class IQuerableExtentions
    {
        public static QDescriptor ToQDescriptor<T>(this IQueryable<T> query)
        {
            var con = new ExpressionConverter();
            var root = con.Convert(query.AsQueryable().Expression);
            return new QDescriptor() { Root = root, IsProjection = con.IsProjection };
        }
    }
}
