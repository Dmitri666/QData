using Qdata.Json.Contract;
using QData.Common;

namespace QData.ExpressionProvider.Builder
{
    using System;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

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
                MethodType method = EnumResolver.ResolveMethod(node.Value);

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
            BinaryType binaryType = EnumResolver.ResolveBinary(node.Value);
            if (binaryType == BinaryType.Contains)
            {
                var left = visitor.ContextExpression.Peek();
                var containsMethod = left.Type.GetMethod("Contains", new Type[] { typeof(string) });
                if (containsMethod == null)
                {
                    var toStringMethod = typeof(object).GetMethod("ToString", new Type[] { });
                    var exp1 = Expression.Call(left, toStringMethod, null);
                    visitor.ContextExpression.Pop();
                    visitor.ContextExpression.Push(exp1);
                }
            }
                
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
