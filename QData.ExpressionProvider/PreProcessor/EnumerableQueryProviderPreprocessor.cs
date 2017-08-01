using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QData.ExpressionProvider.PreProcessor
{
    using Qdata.Contract;

    public class EnumerableQueryProviderPreprocessor
    {
        private QNode Querable { get; set; }

       
        private QNode QueryString { get; set; }

        private QNode QueryStringParent { get; set; }

        private QNode Parent { get; set; }
        public QNode Prepare(QNode node)
        {
            this.Visit(node);
            this.PrepareQueryString();
            return node;
        }

        private void Visit(QNode node)
        {
            if (node.Type == NodeType.Querable)
            {
                this.Querable = node;
            }
            if (node.Type == NodeType.Method)
            {
                var method = EnumResolver.ResolveMethod(node.Value);
                if (method == MethodType.QueryString)
                {
                    this.QueryString = node;
                    this.QueryStringParent = this.Parent;
                }
            }

            this.Parent = node;
            if (node.Left != null)
            {
                this.Visit(node.Left);
            }
            if (node.Right != null)
            {
                this.Visit(node.Right);
            }

        }

        private void PrepareQueryString()
        {
            if (this.QueryString == null)
            {
                return;
            }
            var where = this.ConvertQueryStringToWhere(this.QueryString);
            this.QueryStringParent.Left = where;
            where.Left = this.Querable;


        }

        private QNode ConvertQueryStringToWhere(QNode queryString)
        {
            QNode where = new QNode() { Type = NodeType.Method, Value = MethodType.Where };
            var constant = new QNode() {Type = NodeType.Constant, Value = queryString.Right.Value};
            var members = new List<QNode>();
            QNode temp = queryString.Right.Right;
            while (temp != null)
            {
                members.Add(temp);
                var temp1 = temp.Right;
                temp.Right = null;
                temp = temp1;

            }

            QNode predicate = null;
            for (int i = 0; i < members.Count; i++)
            {
                var containNode = new QNode() { Type = NodeType.Binary, Value = BinaryType.Contains, Right = constant, Left = members[i] };
                if (i == 0)
                {
                    predicate = containNode;

                }
                else
                {
                    var orNode = new QNode() { Type = NodeType.Binary, Value = BinaryType.Or, Left = predicate, Right = containNode };
                    predicate = orNode;

                }
            }

            where.Right = predicate;

            return where;

        }
    }
}
