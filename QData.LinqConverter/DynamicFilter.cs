using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qdata.Contract;

namespace QData.LinqConverter
{
    public class DynamicFilter
    {
        public string FieldName { get; set; }

        public NodeType Operator { get; set; }

        public object Value { get; set; }

        public DynamicFilter(string fieldName, NodeType compareOperator, object value)
        {
            this.FieldName = fieldName;
            this.Operator = compareOperator;
            this.Value = value;
        }
    }
}
