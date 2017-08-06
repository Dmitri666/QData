using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QData.ExpressionProvider.PreProcessor
{
    using Qdata.Contract;

    public class MemberNodeExpander
    {
        private QNode CurrentParent { get; set; }
        public QNode ConvertNode(QNode root)
        {
            this.Visit(root);
            return root;
        }

        private void Visit(QNode node)
        {
            if (node.Type == NodeType.Member)
            {
                var member = Convert.ToString(node.Value);
                var members = member.Split('.');
                if (members.Length > 1)
                {
                    var currentLeft = node.Caller;
                    node.Value = members[members.Length - 1];
                    
                    var currentParentNode = node;
                    for (int i = members.Length - 2; i >= 0; i--)
                    {
                        var newNode = new QNode() { Type = NodeType.Member ,Value = members[i] };
                        currentParentNode.Caller = newNode;
                        currentParentNode = newNode;
                        if (i == 0)
                        {
                            newNode.Caller = currentLeft;
                        }
                    }

                }
            }

            this.CurrentParent = node;

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
