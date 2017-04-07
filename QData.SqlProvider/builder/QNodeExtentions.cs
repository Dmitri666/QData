namespace QData.ExpressionProvider.builder
{
    using System;

    using Qdata.Json.Contract;

    using QData.Common;

    public static class QNodeExtentions
    {
        public static void Accept(this QNode node, IQNodeVisitor visitor)
        {
            if (node.Type == NodeType.Binary)
            {
                AcceptBinary(node,visitor);
            }

            if (node.Type == NodeType.Member)
            {
                AcceptMember(node,visitor);
            }

            if (node.Type == NodeType.Querable)
            {
                AcceptQuerable(node, visitor);
            }

            if (node.Type == NodeType.Method)
            {
                MethodType method;
                if (node.Value is long)
                {
                    method = (MethodType)Convert.ToInt16(node.Value);
                }
                else
                {
                    Enum.TryParse(Convert.ToString(node.Value), out method);
                }
                if (method == MethodType.Select)
                {
                    AcceptProjection(node, visitor);
                }
                else if (method == MethodType.Count && node.Right == null)
                {
                    AcceptCount(node, visitor);
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
            node.Left?.Accept(visitor);
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

        public static void AcceptCount(QNode node, IQNodeVisitor visitor)
        {
            node.Left.Accept(visitor);
            visitor.VisitCount();
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
