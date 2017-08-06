using System;

namespace QData.ExpressionProvider.Builder
{
    using Qdata.Contract;

    internal static class QNodeExtentions
    {
        public static void Accept(this QNode node, QNodeConverter visitor)
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
                switch (method)
                {
                    case NodeType.Select:
                        AcceptProjection(node, visitor);
                        break;
                    case NodeType.Where:
                    case NodeType.Any:
                    case NodeType.OrderBy:
                    case NodeType.OrderByDescending:
                        AcceptLambdaArgumentMethod(node, visitor);
                        break;
                    case NodeType.Contains:
                    case NodeType.In:
                    case NodeType.NotIn:
                    case NodeType.StartsWith:
                    case NodeType.EndsWith:
                    case NodeType.Take:
                    case NodeType.Skip:
                        AcceptValueArgumentMethod(node, visitor);
                        break;
                    case NodeType.ToString:
                        AcceptEmptyMethod(node, visitor);
                        break;
                    case NodeType.Count:
                        if (node.Argument == null)
                        {
                            AcceptEmptyMethod(node, visitor);
                        }
                        else
                        {
                            AcceptLambdaArgumentMethod(node, visitor);
                        }
                        break;
                    default:
                        throw new NotImplementedException(method.ToString());
                }

            }

            if (group == NodeGroup.Constant)
            {
                AcceptConstant(node, visitor);
            }
        }

        private static void AcceptBinary(QNode node, QNodeConverter visitor)
        {
            node.Caller.Accept(visitor);
            if (node.Argument.Type == NodeType.Constant)
            {
                visitor.SetBinaryConstantConverter(node);
            }


            node.Argument.Accept(visitor);
            visitor.VisitBinary(node);
        }

        private static void AcceptMember(QNode node, QNodeConverter visitor)
        {
            node.Caller?.Accept(visitor);
            visitor.VisitMember(node);
        }

        private static void AcceptQuerable(QNode node, QNodeConverter visitor)
        {
            visitor.VisitQuerable(node);
        }

        public static void AcceptLambdaArgumentMethod(QNode node, QNodeConverter visitor)
        {
            node.Caller.Accept(visitor);
            visitor.EnterContext(node);
            node.Argument.Accept(visitor);
            visitor.VisitLambdaMethod(node);
            visitor.LeaveContext(node);
        }

        public static void AcceptValueArgumentMethod(QNode node, QNodeConverter visitor)
        {
            node.Caller.Accept(visitor);
            if (node.Argument.Type == NodeType.Constant)
            {
                visitor.SetMethodConstantConverter(node);
            }
            node.Argument.Accept(visitor);
            visitor.VisitMethod(node);
        }

        public static void AcceptEmptyMethod(QNode node, QNodeConverter visitor)
        {
            node.Caller.Accept(visitor);
            visitor.VisitEmptyMethod(node);
        }

        public static void AcceptProjection(QNode node, QNodeConverter visitor)
        {
            node.Caller.Accept(visitor);
            visitor.EnterContext(node);
            visitor.VisitProjection(node);
            visitor.LeaveContext(node);
        }

        private static void AcceptConstant(QNode node, QNodeConverter visitor)
        {
            visitor.VisitConstant(node);
        }
    }
}