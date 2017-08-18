namespace QData.QueryContainerProvider
{
    using Qdata.Contract;

    public static class QNodeExtentions
    {
        public static void Accept(this QNode node, IQNodeVisitor visitor)
        {
            var group = EnumResolver.ResolveNodeGroup(node.Type);
            if (group == NodeGroup.Binary)
            {
                AcceptBinary(node, visitor);
            }

            if (group == NodeGroup.Member)
            {
                AcceptMember(node, visitor);
            }

            if (group == NodeGroup.Querable)
            {
                AcceptQuerable(node, visitor);
            }

            if (group == NodeGroup.Method)
            {
                var method = EnumResolver.ResolveNodeType(node.Type);

                if (method == NodeType.Select)
                {
                    AcceptProjection(node, visitor);
                }
                else if (node.Argument == null)
                {
                    AcceptEmptyMethod(node, visitor);
                }
                else
                {
                    AcceptMethod(node, visitor);
                }
            }

            if (group == NodeGroup.Constant)
            {
                AcceptConstant(node, visitor);
            }
        }

        private static void AcceptBinary(QNode node, IQNodeVisitor visitor)
        {
            node.Operand.Accept(visitor);
            node.Argument.Accept(visitor);
            visitor.VisitBinary(node);
        }

        

        private static void AcceptMember(QNode node, IQNodeVisitor visitor)
        {
            visitor.VisitMember(node);
        }

        private static void AcceptQuerable(QNode node, IQNodeVisitor visitor)
        {
            
        }

        public static void AcceptMethod(QNode node, IQNodeVisitor visitor)
        {
            node.Operand.Accept(visitor);
            node.Argument.Accept(visitor);
            visitor.VisitMethod(node);
            
        }

        public static void AcceptEmptyMethod(QNode node, IQNodeVisitor visitor)
        {
            node.Operand.Accept(visitor);
            visitor.VisitEmptyMethod(node);
        }

        public static void AcceptProjection(QNode node, IQNodeVisitor visitor)
        {
            node.Operand.Accept(visitor);
            visitor.VisitProjection(node);
            }

        private static void AcceptConstant(QNode node, IQNodeVisitor visitor)
        {
            visitor.VisitConstant(node);
        }
    }
}