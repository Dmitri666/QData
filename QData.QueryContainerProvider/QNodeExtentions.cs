namespace QData.QueryContainerProvider
{
    using Qdata.Contract;

    public static class QNodeExtentions
    {
        public static void Accept(this QNode node, IQNodeVisitor visitor)
        {
            if (node.Type == NodeType.Binary)
            {
               
                AcceptBinary(node, visitor);
            
                
            }

            if (node.Type == NodeType.Member)
            {
                AcceptMember(node, visitor);
            }

            if (node.Type == NodeType.Querable)
            {
                AcceptQuerable(node, visitor);
            }

            if (node.Type == NodeType.Method)
            {
                var method = EnumResolver.ResolveMethod(node.Value);

                if (method == MethodType.Select)
                {
                    AcceptProjection(node, visitor);
                }
                else if (node.Right == null)
                {
                    AcceptEmptyMethod(node, visitor);
                }
                else
                {
                    AcceptMethod(node, visitor);
                }
            }

            if (node.Type == NodeType.Constant)
            {
                AcceptConstant(node, visitor);
            }
        }

        private static void AcceptBinary(QNode node, IQNodeVisitor visitor)
        {
            node.Left.Accept(visitor);
            node.Right.Accept(visitor);
            visitor.VisitBinary(node);
        }

        

        private static void AcceptMember(QNode node, IQNodeVisitor visitor)
        {
            visitor.VisitMember(node);
        }

        private static void AcceptQuerable(QNode node, IQNodeVisitor visitor)
        {
            visitor.VisitQuerable(node);
        }

        public static void AcceptMethod(QNode node, IQNodeVisitor visitor)
        {
            node.Left.Accept(visitor);
            visitor.EnterContext(node);
            node.Right.Accept(visitor);
            visitor.VisitMethod(node);
            visitor.LeaveContext(node);
        }

        public static void AcceptEmptyMethod(QNode node, IQNodeVisitor visitor)
        {
            node.Left.Accept(visitor);
            visitor.VisitEmptyMethod(node);
        }

        public static void AcceptProjection(QNode node, IQNodeVisitor visitor)
        {
            node.Left.Accept(visitor);
            visitor.EnterContext(node);
            visitor.VisitProjection(node);
            visitor.LeaveContext(node);
        }

        private static void AcceptConstant(QNode node, IQNodeVisitor visitor)
        {
            visitor.VisitConstant(node);
        }
    }
}