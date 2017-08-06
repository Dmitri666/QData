namespace QData.QueryContainerProvider
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Nest;

    using Qdata.Contract;

    

    internal class QNodeConverter<T> : IQNodeVisitor where T: class
    {
        private Stack<QueryBase> ContextQuery { get; set; }

        private Stack<Field> ContextField { get; set; }

        private Stack<object> ContextValue { get; set; }

        private List<KeyValuePair<Field, NodeType>> SortingRequest { get; set; }
            

        private QueryContainer QueryContainer => this.ContextQuery.Pop();

        public Func<Nest.SortDescriptor<T>, IPromise<IList<ISort>>> Sorting
        {
            get
            {
                return descriptor =>
                    {
                        foreach (var sorting in this.SortingRequest)
                        {
                            descriptor = sorting.Value == NodeType.OrderBy ? descriptor.Ascending(sorting.Key) : descriptor.Descending(sorting.Key);
                        }
                        return descriptor;
                    };
            }
            
        }

        public Func<Nest.QueryContainerDescriptor<T>, QueryContainer> Query
        {
            get
            {
                if (this.ContextQuery.Count == 0)
                {
                    return x => x;
                }

                return descriptor =>
                {
                    return this.QueryContainer;
                };

            }

        }

        public QNodeConverter()
        {
            this.ContextQuery = new Stack<QueryBase>();
            this.ContextField = new Stack<Field>();
            this.ContextValue = new Stack<object>();
            this.SortingRequest = new List<KeyValuePair<Field, NodeType>>();
        }

        public void VisitBinary(QNode node)
        {
            var binary = EnumResolver.ResolveNodeType(node.Type);
            switch (binary)
            {
                case NodeType.And:
                {
                    var left = this.ContextQuery.Pop();
                    var right = this.ContextQuery.Pop();
                    this.ContextQuery.Push(left && right);
                }
                    break;
                case NodeType.Or:
                {
                    var left = this.ContextQuery.Pop();
                    var right = this.ContextQuery.Pop();
                    this.ContextQuery.Push(left || right);
                }
                    break;
                case NodeType.Equal:
                    {
                        var field = this.ContextField.Pop();
                        var value = this.ContextValue.Pop();
                        var query = new MatchQuery() {Field = field ,Query = value.ToString() };
                        this.ContextQuery.Push(query);
                        
                    }
                    break;
                case NodeType.NotEqual:
                    {
                        var field = this.ContextField.Pop();
                        var value = this.ContextValue.Pop();
                        var query = new MatchQuery() { Field = field, Query = value.ToString() };
                        this.ContextQuery.Push(!query);

                    }
                    break;
                case NodeType.GreaterThan:
                    {
                        var field = this.ContextField.Pop();
                        var value = this.ContextValue.Pop();
                        var query = new TermRangeQuery(){ Field = field, GreaterThan = value.ToString() };
                        this.ContextQuery.Push(query);
                        
                    }
                    break;
                case NodeType.GreaterThanOrEqual:
                    {
                        var field = this.ContextField.Pop();
                        var value = this.ContextValue.Pop();
                        var query = new TermRangeQuery() { Field = field, GreaterThanOrEqualTo = value.ToString() };
                        this.ContextQuery.Push(query);

                    }
                    break;
                case NodeType.LessThan:
                    {
                        var field = this.ContextField.Pop();
                        var value = this.ContextValue.Pop();
                        var query = new TermRangeQuery() { Field = field, LessThan = value.ToString() };
                        this.ContextQuery.Push(query);

                    }
                    break;
                case NodeType.LessThanOrEqual:
                    {
                        var field = this.ContextField.Pop();
                        var value = this.ContextValue.Pop();
                        var query = new TermRangeQuery() { Field = field, LessThanOrEqualTo = value.ToString() };
                        this.ContextQuery.Push(query);

                    }
                    break;
                default:
                    throw new NotImplementedException(binary.ToString());
            }






        }

        public void VisitMember(QNode node)
        {
            this.ContextField.Push(new Field(ToCamelCase(Convert.ToString(node.Value))));
        }

        public void VisitMethod(QNode node)
        {

            var method = EnumResolver.ResolveNodeType(node.Type);
            switch (method)
            {
                case NodeType.OrderBy:
                case NodeType.OrderByDescending:
                    var member = this.ContextField.Pop();
                    this.SortingRequest.Add(new KeyValuePair<Field, NodeType>(member, method));
                    break;
                case NodeType.Where:
                    break;
                case NodeType.Contains:
                {
                    var value = string.Format("*{0}*", this.ContextValue.Pop());
                    var term = new WildcardQuery() {Field = this.ContextField.Pop(), Value = value};
                    this.ContextQuery.Push(term);
                }
                    break;
                case NodeType.StartsWith:
                {
                    var term = new PrefixQuery() {Field = this.ContextField.Pop(), Value = this.ContextValue.Pop() };
                    this.ContextQuery.Push(term);
                }
                    break;
                case NodeType.EndsWith:
                    {
                        var value = string.Format("*{0}", this.ContextValue.Pop());
                        var term = new WildcardQuery() { Field = this.ContextField.Pop(), Value = value };
                        this.ContextQuery.Push(term);
                    }
                    break;
                case NodeType.In:
                {
                    var value = this.ContextValue.Pop() as List<string>;
                    var terms = new TermsQuery() { Field = this.ContextField.Pop() ,Terms = value};
                    this.ContextQuery.Push(terms);
                }
                    break;
                case NodeType.NotIn:
                    {
                        var value = this.ContextValue.Pop() as List<string>;
                        var terms = !new TermsQuery() { Field = this.ContextField.Pop(), Terms = value };
                        this.ContextQuery.Push(terms);
                    }
                    break;
                default:
                    throw new NotImplementedException(method.ToString());
            }


        }

        public void VisitConstant(QNode node)
        {
            this.ContextValue.Push(node.Value);
        }

        public void VisitProjection(QNode node)
        {
            throw new NotImplementedException();
        }

        public void VisitEmptyMethod(QNode node)
        {
            throw new NotImplementedException();
        }

        public void SetConstantConverter(QNode node)
        {
            throw new NotImplementedException();
        }

        private static string ToCamelCase(string fieldName)
        {
            return $"{fieldName.Substring(0, 1).ToLower()}{fieldName.Substring(1)}";
        }
    }
}