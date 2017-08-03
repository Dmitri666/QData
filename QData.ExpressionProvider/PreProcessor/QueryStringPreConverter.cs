namespace QData.ExpressionProvider.PreProcessor
{
    using System.Collections.Generic;

    using Qdata.Contract;

    public class QueryStringPreConverter
    {
        private QNode CurrentParentNode { get; set; }

        private QNode QueryStringNode { get; set; }

        private QNode QueryStringParentNode { get; set; }

        public QNode ConvertNode(QNode root)
        {
            this.Visit(root);
            if (this.QueryStringNode == null)
            {
                return root;
            }
            
            var whereNode = this.ConvertQueryStringToWhere(this.QueryStringNode);
            if (this.QueryStringParentNode == null)
            {
                return whereNode;
            }

            this.QueryStringParentNode.Left = whereNode;
            return root;
        }

        private QNode ConvertQueryStringToWhere(QNode queryString)
        {
            QNode where = new QNode() { Type = NodeType.Method, Value = MethodType.Where, Left = queryString.Left };
            var constant = new QNode() { Type = NodeType.Constant, Value = queryString.Right.Value };
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
                var containNode = new QNode()
                                      {
                                          Type = NodeType.Binary,
                                          Value = BinaryType.Contains,
                                          Right = constant,
                                          Left = members[i]
                                      };
                if (i == 0)
                {
                    predicate = containNode;
                }
                else
                {
                    var orNode = new QNode()
                                     {
                                         Type = NodeType.Binary,
                                         Value = BinaryType.Or,
                                         Left = predicate,
                                         Right = containNode
                                     };
                    predicate = orNode;
                }
            }

            where.Right = predicate;

            return where;
        }

        private void Visit(QNode node)
        {
            if (node.Type == NodeType.Method)
            {
                var method = EnumResolver.ResolveMethod(node.Value);
                if (method == MethodType.QueryString)
                {
                    this.QueryStringNode = node;
                    this.QueryStringParentNode = this.CurrentParentNode;
                }
            }
            this.CurrentParentNode = node;
            if (node.Left != null)
            {
                this.Visit(node.Left);
            }
            if (node.Right != null)
            {
                this.Visit(node.Right);
            }
        }
    }
}