using System.CodeDom;
using Newtonsoft.Json.Linq;
using QData.ExpressionProvider.Builder;
using QData.ExpressionProvider.Converters;

namespace QData.ExpressionProvider.builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Qdata.Json.Contract;

    using QData.Common;

    public class QDescriptorConverter : IQNodeVisitor
    {
        #region Fields

        /// <summary>
        ///     The count.
        /// </summary>
        private int parameterPrefix;

        private int OderByCount { get; set; }

        #endregion

        #region Properties


        private Expression Query { get; }

        /// <summary>
        ///     Gets the current.
        /// </summary>
        public Stack<Expression> ContextExpression { get; set; }

        /// <summary>
        ///     Gets or sets the param expression.
        /// </summary>
        private Stack<ParameterExpression> ContextParameters { get; }

        private BaseConstantConverter ConstantConverter { get; set; }



        #endregion
        public QDescriptorConverter(Expression query)
        {
            this.ContextExpression = new Stack<Expression>();
            this.ContextParameters = new Stack<ParameterExpression>();
            this.Query = query;
        }

        public void SetConstantConverter(QNode node)
        {
            var left = this.ContextExpression.Peek();
            BinaryType binaryType = EnumResolver.ResolveBinary(node.Value);
            switch (binaryType)
            {
                case BinaryType.Contains:
                    var containsMethod = left.Type.GetMethod("Contains", new Type[] {typeof (string)});
                    if (containsMethod == null)
                    {
                        var toStringMethod = typeof (object).GetMethod("ToString", new Type[] {});
                        var exp1 = Expression.Call(left, toStringMethod, null);
                        this.ContextExpression.Pop();
                        this.ContextExpression.Push(exp1);
                        this.ConstantConverter = new DefaultConverter(exp1.Type);
                    }
                    else
                    {
                        this.ConstantConverter = new DefaultConverter(left.Type);
                    }

                    break;
                case BinaryType.StartsWith:
                    var startsWithMethod = left.Type.GetMethod("StartsWith", new Type[] {typeof (string)});
                    if (startsWithMethod == null)
                    {
                        var toStringMethod = typeof (object).GetMethod("ToString", new Type[] {});
                        var exp1 = Expression.Call(left, toStringMethod, null);
                        this.ContextExpression.Pop();
                        this.ContextExpression.Push(exp1);
                        this.ConstantConverter = new DefaultConverter(exp1.Type);
                    }
                    else
                    {
                        this.ConstantConverter = new DefaultConverter(left.Type);
                    }

                    break;
                case BinaryType.EndsWith:
                    var endsWithMethod = left.Type.GetMethod("EndsWith", new Type[] {typeof (string)});
                    if (endsWithMethod == null)
                    {
                        var toStringMethod = typeof (object).GetMethod("ToString", new Type[] {});
                        var exp1 = Expression.Call(left, toStringMethod, null);
                        this.ContextExpression.Pop();
                        this.ContextExpression.Push(exp1);
                        this.ConstantConverter = new DefaultConverter(exp1.Type);
                    }
                    else
                    {
                        this.ConstantConverter = new DefaultConverter(left.Type);
                    }

                    break;
                case BinaryType.In:
                case BinaryType.NotIn:
                    this.ConstantConverter = new ArrayConverter(left.Type);
                    break;
                case BinaryType.Skip:
                case BinaryType.Take:
                    this.ConstantConverter = new IntegerConverter(left.Type);
                    break;
                case BinaryType.And:
                case BinaryType.Or:
                    break;
                case BinaryType.Equal:
                case BinaryType.GreaterThan:
                case BinaryType.GreaterThanOrEqual:
                case BinaryType.LessThan:
                case BinaryType.LessThanOrEqual:
                case BinaryType.NotEqual:
                    this.ConstantConverter = new DefaultConverter(left.Type);
                    break;
                default:
                    throw new NotImplementedException(binaryType.ToString());


            }

        }

        public void VisitMember(QNode node)
        {
            this.ContextExpression.Push(
                node.Left == null
                    ? Expression.PropertyOrField(this.ContextParameters.Peek(), Convert.ToString(node.Value))
                    : Expression.PropertyOrField(this.ContextExpression.Pop(), Convert.ToString(node.Value)));
        }

        public void VisitQuerable(QNode node)
        {
            this.ContextExpression.Push(this.Query);
        }

        public void VisitMethod(QNode node)
        {
            var right = this.ContextExpression.Pop();
            var left = this.ContextExpression.Pop();

            var lambda = Expression.Lambda(right, this.ContextParameters.Peek());

            var exp = this.BuildMethodCallExpression(EnumResolver.ResolveMethod(node.Value), left, lambda);
            this.ContextExpression.Push(exp);
        }

        public void VisitEmptyMethod(QNode node)
        {
            var left = this.ContextExpression.Pop();
            var method = EnumResolver.ResolveMethod(node.Value);
            if (method == MethodType.ToString)
            {
                var toStringMethod = typeof(object).GetMethod("ToString", new Type[] {});
                var exp = Expression.Call(left, toStringMethod, null);
                this.ContextExpression.Push(exp);
            }
            else
            {
                var types = new List<Type>() { left.Type.GetTypeInfo().IsGenericType ? left.Type.GenericTypeArguments[0] : left.Type };
                var exp = Expression.Call(typeof(Enumerable), method.ToString(), types.ToArray(), left);
                this.ContextExpression.Push(exp);
            }

        }

        public void EnterContext(QNode node)
        {
            Type parameterType;
            var expression = this.ContextExpression.Peek();
            parameterType = expression.Type.GetTypeInfo().IsGenericType
                                ? expression.Type.GenericTypeArguments[0]
                                : expression.Type.GetTypeInfo().UnderlyingSystemType;

            var parameter = Expression.Parameter(parameterType, string.Format("x{0}", this.parameterPrefix++));
            this.ContextParameters.Push(parameter);
        }

        public void LeaveContext(QNode node)
        {
            this.ContextParameters.Pop();
        }

        public void VisitProjection(QNode node)
        {
            var left = this.ContextExpression.Pop();

            var bindingNodes = new Dictionary<string, QNode>();
            var bindingPropertyNode = node.Right;
            while (bindingPropertyNode != null)
            {
                bindingNodes.Add(Convert.ToString(bindingPropertyNode.Value), bindingPropertyNode.Right);
                bindingPropertyNode = bindingPropertyNode.Left;
            }

            Type projectorType = null;
            var dynamicProperties = new List<DynamicProperty>();
            var bindingExpressions = new Dictionary<string, Expression>();
            foreach (var bindingNode in bindingNodes)
            {
                bindingNode.Value.Accept(this);
                var member = this.ContextExpression.Pop();
                bindingExpressions.Add(Convert.ToString(bindingNode.Key), member);
                var property = new DynamicProperty(Convert.ToString(bindingNode.Key), member.Type);
                dynamicProperties.Add(property);
            }

            projectorType = ClassFactory.Instance.GetDynamicClass(dynamicProperties);

            var resultProperties = projectorType.GetTypeInfo().DeclaredProperties;
            var bindingsSource = new List<Expression>();
            var bindingsProperties = new List<PropertyInfo>();
            foreach (var binding in bindingExpressions)
            {
                bindingsProperties.Add(resultProperties.FirstOrDefault(x => x.Name == binding.Key));
                bindingsSource.Add(binding.Value);
            }

            var bindings = new List<MemberBinding>();
            for (var i = 0; i < bindingsSource.Count; i++)
            {
                var exp = Expression.Bind(bindingsProperties[i], bindingsSource[i]);
                bindings.Add(exp);
            }

            var lambda =
                Expression.Lambda(
                    Expression.MemberInit(Expression.New(projectorType.GetTypeInfo().GetConstructor(Type.EmptyTypes)), bindings),
                    this.ContextParameters.Peek());

            var result = this.BuildMethodCallExpression(MethodType.Select, left, lambda);

            this.ContextExpression.Push(result);

        }

        public void VisitConstant(QNode node)
        {
            var constantExpression = this.ConstantConverter.ConvertToConstant(node);
            this.ContextExpression.Push(constantExpression);
        }

        public void VisitBinary(QNode node)
        {
            var right = this.ContextExpression.Pop();
            var left = this.ContextExpression.Pop();
            BinaryType op = EnumResolver.ResolveBinary(node.Value);
            var exp = this.BuildBinaryExpression(op, left, right);
            this.ContextExpression.Push(exp);
        }

        private Expression BuildBinaryExpression(BinaryType binary, Expression left, Expression right)
        {
            if (binary == BinaryType.And)
            {
                return Expression.AndAlso(left, right);
            }

            if (binary == BinaryType.Or)
            {
                return Expression.OrElse(left, right);
            }

            if (binary == BinaryType.Equal)
            {
                return Expression.Equal(left, right);
            }

            if (binary == BinaryType.NotEqual)
            {
                return Expression.NotEqual(left, right);
            }

            if (binary == BinaryType.GreaterThan)
            {
                return Expression.GreaterThan(left, right);
            }

            if (binary == BinaryType.GreaterThanOrEqual)
            {
                return Expression.GreaterThanOrEqual(left, right);
            }

            if (binary == BinaryType.LessThan)
            {
                return Expression.LessThan(left, right);
            }

            if (binary == BinaryType.LessThanOrEqual)
            {
                return Expression.LessThanOrEqual(left, right);
            }

            if (binary == BinaryType.Contains)
            {
                if (left.Type != typeof(string))
                {
                    left = Expression.Call(left, "ToString", null);
                }

                return Expression.Call(left, Methods.Contains, (ConstantExpression)right);
            }

            if (binary == BinaryType.StartsWith)
            {
                if (left.Type != typeof(string))
                {
                    left = Expression.Call(left, "ToString", null);
                }

                return Expression.Call(left, Methods.StartsWith, (ConstantExpression)right);
            }

            if (binary == BinaryType.EndsWith)
            {
                if (left.Type != typeof(string))
                {
                    left = Expression.Call(left, "ToString", null);
                }

                return Expression.Call(left, Methods.EndsWith, (ConstantExpression)right);
            }

            if (binary == BinaryType.In)
            {
                var method = right.Type.GetTypeInfo().GetMethod("Contains");
                return Expression.Call(right, method, left);
            }

            if (binary == BinaryType.NotIn)
            {
                var method = right.Type.GetTypeInfo().GetMethod("Contains");
                var call = Expression.Call(right, method, left);
                return Expression.Not(call);
            }

            if (binary == BinaryType.Take)
            {
                var types = new List<Type>() { left.Type.GetTypeInfo().IsGenericType ? left.Type.GenericTypeArguments[0] : left.Type };
                return Expression.Call(typeof(Queryable), "Take", types.ToArray(), left, right);
            }

            if (binary == BinaryType.Skip)
            {
                var types = new List<Type>() { left.Type.GetTypeInfo().IsGenericType ? left.Type.GenericTypeArguments[0] : left.Type };
                return Expression.Call(typeof(Queryable), "Skip", types.ToArray(), left, right);
            }
            throw new Exception(binary.ToString());
        }

        private MethodCallExpression BuildMethodCallExpression(
            MethodType method,
            Expression caller,
            LambdaExpression argument)
        {
            var types = new List<Type> { caller.Type.GetTypeInfo().IsGenericType ? caller.Type.GenericTypeArguments[0] : caller.Type };

            if (method == MethodType.Any)
            {
                return Expression.Call(typeof(Enumerable), "Any", types.ToArray(), caller, argument);
            }
            if (method == MethodType.Count)
            {
                return Expression.Call(typeof(Enumerable), "Count", types.ToArray(), caller, argument);
            }

            if (method == MethodType.OrderBy || method == MethodType.OrderByDescending)
            {
                var methodName = method.ToString();
                if (this.OderByCount > 0)
                {
                    methodName = methodName.Replace("OderBy", "ThenBy");
                }
                types.Add(argument.ReturnType);
                this.OderByCount++;
                return Expression.Call(
                    typeof(Queryable),
                    methodName,
                    types.ToArray(),
                    caller,
                    Expression.Quote(argument));
            }

            if (method == MethodType.Where)
            {
                return Expression.Call(typeof(Queryable), "Where", types.ToArray(), caller, Expression.Quote(argument));
            }

            if (method == MethodType.Select)
            {
                types.Add(argument.ReturnType);
                return Expression.Call(typeof(Queryable), "Select", types.ToArray(), caller, Expression.Quote(argument));
            }

            throw new Exception(method.ToString());
        }


    }
}