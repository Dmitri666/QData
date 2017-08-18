using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qdata.Contract;

namespace QData.LinqConverter
{
    public class DynamicSource
    {
        private QNode query;

        public DynamicSource()
        {
            this.query = new QNode() {Type = NodeType.Querable};
        }

        public QNode Query => this.query;

        public DynamicSource QueryString(string query, List<string> fields)
        {
            var queryString = new QNode()
            {
                Type = NodeType.QueryString,
                Argument = new QNode() {Type = NodeType.Constant, Value = query}
            };
            List<QNode> memberArray =
                fields.Select(member => new QNode() {Type = NodeType.Member, Value = member}).ToList();
            QNode first = null;
            QNode current = null;
            foreach (var member in memberArray)
            {
                if (first == null)
                {
                    first = member;
                    current = member;
                }
                else
                {
                    current.Argument = member;
                    current = member;
                }
            }
            queryString.Argument.Argument = first;
            queryString.Operand = this.query;
            this.query = queryString;
            return this;
        }

        public DynamicSource Where(List<DynamicFilter> filters)
        {
            this.Where(filters, NodeType.And);
            return this;
        }

        public DynamicSource Where(List<DynamicFilter> filters, NodeType logicalOperator)
        {

            return this;
        }
    }
}