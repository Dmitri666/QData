﻿namespace QData.ExpressionProvider.builder
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

        



        #endregion
        public QDescriptorConverter(Expression query)
        {
            this.ContextExpression = new Stack<Expression>();
            this.ContextParameters = new Stack<ParameterExpression>();
            this.Query = query;
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
            MethodType method;
            if (node.Value is long)
            {
                method = (MethodType)Convert.ToInt16(node.Value);
            }
            else
            {
                Enum.TryParse(Convert.ToString(node.Value), out method);
            }

            var right = this.ContextExpression.Pop();
            var left = this.ContextExpression.Pop();

            var lambda = Expression.Lambda(right, this.ContextParameters.Peek());

            var exp = this.BuildMethodCallExpression(method, left, lambda);
            this.ContextExpression.Push(exp);
        }

        public void VisitEmptyMethod(QNode node)
        {
            var left = this.ContextExpression.Pop();
            var types = new List<Type>() { left.Type.IsGenericType ? left.Type.GenericTypeArguments[0] : left.Type };
            var method = EnumResolver.ResolveMethod(node.Value);
            var exp = Expression.Call(typeof(Enumerable), method.ToString(), types.ToArray(),left);
            this.ContextExpression.Push(exp);
        }

        public void EnterContext(QNode node)
        {
            Type parameterType;
            var expression = this.ContextExpression.Peek();
            parameterType = expression.Type.IsGenericType
                                ? expression.Type.GenericTypeArguments[0]
                                : expression.Type.UnderlyingSystemType;

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

            var bindingNodes = new Dictionary<string,QNode>();
            var bindingPropertyNode = node.Right;
            while (bindingPropertyNode != null)
            {
                bindingNodes.Add(Convert.ToString(bindingPropertyNode.Value),bindingPropertyNode.Right);
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
                var property = new DynamicProperty(Convert.ToString(bindingNode.Key),member.Type);
                dynamicProperties.Add(property);
            }

            projectorType = ClassFactory.Instance.GetDynamicClass(dynamicProperties);

            var resultProperties = projectorType.GetProperties();
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
                    Expression.MemberInit(Expression.New(projectorType.GetConstructor(Type.EmptyTypes)), bindings),
                    this.ContextParameters.Peek());

            var result = this.BuildMethodCallExpression(MethodType.Select, left, lambda);

            this.ContextExpression.Push(result);

        }

        public void VisitConstant(QNode node)
        {
            Type valueType = null;
            if (node.Value.GetType().IsGenericType)
            {
                valueType = node.Value.GetType().GetGenericArguments()[0];
            }
            else
            {
                valueType = node.Value.GetType();
            }

            var memberType = this.ContextExpression.Peek().Type;

            //take skip
            if (typeof(IQueryable).IsAssignableFrom(memberType))
            {
                var exp = Expression.Constant(Convert.ToInt32(node.Value));
                this.ContextExpression.Push(exp);
                return;
            }
            if (valueType != memberType)
            {
                if (node.Value.GetType().IsGenericType)
                {
                    var list = (List<string>)node.Value;
                    if (memberType == typeof(long))
                    {
                        var exp = Expression.Constant(list.ConvertAll(Convert.ToInt64));
                        this.ContextExpression.Push(exp);
                    }
                    else if (memberType == typeof(DateTime))
                    {
                        var exp = Expression.Constant(list.ConvertAll(Convert.ToDateTime));
                        this.ContextExpression.Push(exp);
                    }
                    else
                    {
                        //var listType = typeof(List<>);
                        //var concreteType = listType.MakeGenericType(memberType);
                        //var valueList = Activator.CreateInstance(concreteType);
                        //var methodInfo = valueList.GetType().GetMethod("Add");
                        //foreach (var stringValue in (List<string>)node.Value)
                        //{
                        //    var value = ConvertConstant(stringValue, propertyMap.DestinationPropertyType, out operatorType);
                        //    methodInfo.Invoke(valueList, new object[] { value });
                        //}
                        throw new Exception(string.Format("VisitConstantNode :Type {0}", memberType));
                    }
                }
                else
                {
                    var value = Convert.ChangeType(node.Value, memberType);
                    var exp1 = Expression.Constant(value);
                    this.ContextExpression.Push(exp1);
                }
            }
            else
            {
                var exp = Expression.Constant(node.Value, this.ContextExpression.Peek().Type);
                this.ContextExpression.Push(exp);
            }
        }

        public void VisitBinary(QNode node)
        {
            var right = this.ContextExpression.Pop();
            var left = this.ContextExpression.Pop();
            BinaryType op;
            if (node.Value is long)
            {
                op = (BinaryType)Convert.ToInt16(node.Value);
            }
            else
            {
                Enum.TryParse(Convert.ToString(node.Value), out op);
            }
            var exp = this.BuildBinaryExpression(op, left, right);
            this.ContextExpression.Push(exp);
        }

        private Expression BuildBinaryExpression(BinaryType binary, Expression left, Expression right)
        {
            if (binary == BinaryType.And)
            {
                return Expression.And(left, right);
            }

            if (binary == BinaryType.Or)
            {
                return Expression.Or(left, right);
            }

            if (binary == BinaryType.Equal)
            {
                return Expression.Equal(left, right);
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
                var containsMethod = typeof(string).GetMethod("Contains");
                return Expression.Call(left, containsMethod, (ConstantExpression)right);
            }

            if (binary == BinaryType.StartsWith)
            {
                var startsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
                return Expression.Call(left, startsWithMethod, (ConstantExpression)right);
            }

            if (binary == BinaryType.EndsWith)
            {
                var endsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
                return Expression.Call(left, endsWithMethod, (ConstantExpression)right);
            }

            if (binary == BinaryType.In)
            {
                var method = right.Type.GetMethod("Contains");
                return Expression.Call(right, method, left);
            }

            if (binary == BinaryType.Take)
            {
                var types = new List<Type>() { left.Type.IsGenericType ? left.Type.GenericTypeArguments[0] : left.Type };
                return Expression.Call(typeof(Queryable), "Take", types.ToArray(), left, right);
            }

            if (binary == BinaryType.Skip)
            {
                var types = new List<Type>() { left.Type.IsGenericType ? left.Type.GenericTypeArguments[0] : left.Type };
                return Expression.Call(typeof(Queryable), "Skip", types.ToArray(), left, right);
            }
            throw new Exception(binary.ToString());
        }

        private MethodCallExpression BuildMethodCallExpression(
            MethodType method,
            Expression caller,
            LambdaExpression argument)
        {
            var types = new List<Type> { caller.Type.IsGenericType ? caller.Type.GenericTypeArguments[0] : caller.Type };

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