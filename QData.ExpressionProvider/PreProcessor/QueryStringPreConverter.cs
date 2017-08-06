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

            this.QueryStringParentNode.Caller = whereNode;
            return root;
        }

        private QNode ConvertQueryStringToWhere(QNode queryString)
        {
            QNode where = new QNode() { Type = NodeType.Where, Caller = queryString.Caller };
            var constant = new QNode() { Type = NodeType.Constant, Value = queryString.Argument.Value };
            var members = new List<QNode>();
            QNode temp = queryString.Argument.Argument;
            while (temp != null)
            {
                members.Add(temp);
                var temp1 = temp.Argument;
                temp.Argument = null;
                temp = temp1;
            }

            QNode predicate = null;
            for (int i = 0; i < members.Count; i++)
            {
                var containNode = new QNode()
                                      {
                                          Type = NodeType.Contains,
                                          Argument = constant,
                                          Caller = members[i]
                                      };
                if (i == 0)
                {
                    predicate = containNode;
                }
                else
                {
                    var orNode = new QNode()
                                     {
                                         Type = NodeType.Or,
                                         Caller = predicate,
                                         Argument = containNode
                                     };
                    predicate = orNode;
                }
            }

            where.Argument = predicate;

            return where;
        }

        private void Visit(QNode node)
        {
            var nodeType = EnumResolver.ResolveNodeType(node.Type);
            var group = EnumResolver.ResolveNodeGroup(nodeType);
            if (group == NodeGroup.Method)
            {
                if (nodeType == NodeType.QueryString)
                {
                    this.QueryStringNode = node;
                    this.QueryStringParentNode = this.CurrentParentNode;
                }
            }
            this.CurrentParentNode = node;
            if (node.Caller != null)
            {
                this.Visit(node.Caller);
            }
            if (node.Argument != null)
            {
                this.Visit(node.Argument);
            }
        }
    }
}