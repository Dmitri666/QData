using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using Qdata.Contract;

namespace QData.QueryContainerProvider
{
    internal class QNodeConverter<T> : IQNodeVisitor where T : class
    {
        public QNodeConverter()
        {
            this.ContextQuery = new Stack<QueryBase>();
            this.ContextField = new Stack<Field>();
            this.ContextValue = new Stack<object>();
            this.SortingRequest = new List<KeyValuePair<Field, NodeType>>();
            this.AllField = new List<Field>();
        }

        private Stack<QueryBase> ContextQuery { get; }
        private Stack<Field> ContextField { get; }
        private Stack<object> ContextValue { get; }
        private List<KeyValuePair<Field, NodeType>> SortingRequest { get; }
        private QueryContainer QueryContainer => this.ContextQuery.Pop();

        private Func<SortDescriptor<T>, IPromise<IList<ISort>>> Sorting
        {
            get
            {
                return descriptor =>
                {
                    foreach (var sorting in this.SortingRequest)
                    {
                        descriptor = sorting.Value == NodeType.OrderBy
                            ? descriptor.Ascending(sorting.Key)
                            : descriptor.Descending(sorting.Key);
                    }
                    return descriptor;
                };
            }
        }

        public Func<SearchDescriptor<T>, ISearchRequest> Query
        {
            get { return descriptor => descriptor.Query(this.ResultQuery).Sort(this.Sorting); }
        }

       

        public Func<SearchDescriptor<T>, ISearchRequest> GetPageQuery(int skip, int take)
        {
            return descriptor => descriptor.Query(this.ResultQuery).Sort(this.Sorting).Skip(skip).Take(take);
        }

        private Func<QueryContainerDescriptor<T>, QueryContainer> ResultQuery
        {
            get
            {
                if (this.ContextQuery.Count == 0)
                {
                    return x => x;
                }

                return descriptor => this.QueryContainer;
            }
        }

        public List<Field> AllField { get; set; }

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
                    var query = new MatchQuery {Field = field, Query = value.ToString()};
                    this.ContextQuery.Push(query);
                }
                    break;
                case NodeType.NotEqual:
                {
                    var field = this.ContextField.Pop();
                    var value = this.ContextValue.Pop();
                    var query = new MatchQuery {Field = field, Query = value.ToString()};
                    this.ContextQuery.Push(!query);
                }
                    break;
                case NodeType.GreaterThan:
                {
                    var field = this.ContextField.Pop();
                    var value = this.ContextValue.Pop();
                    var query = new TermRangeQuery {Field = field, GreaterThan = value.ToString()};
                    this.ContextQuery.Push(query);
                }
                    break;
                case NodeType.GreaterThanOrEqual:
                {
                    var field = this.ContextField.Pop();
                    var value = this.ContextValue.Pop();
                    var query = new TermRangeQuery {Field = field, GreaterThanOrEqualTo = value.ToString()};
                    this.ContextQuery.Push(query);
                }
                    break;
                case NodeType.LessThan:
                {
                    var field = this.ContextField.Pop();
                    var value = this.ContextValue.Pop();
                    var query = new TermRangeQuery {Field = field, LessThan = value.ToString()};
                    this.ContextQuery.Push(query);
                }
                    break;
                case NodeType.LessThanOrEqual:
                {
                    var field = this.ContextField.Pop();
                    var value = this.ContextValue.Pop();
                    var query = new TermRangeQuery {Field = field, LessThanOrEqualTo = value.ToString()};
                    this.ContextQuery.Push(query);
                }
                    break;
                default:
                    throw new NotImplementedException(binary.ToString());
            }
        }

        public void VisitMember(QNode node)
        {
            var field = new Field(ToCamelCase(Convert.ToString(node.Value)));
            this.ContextField.Push(field);
            this.AllField.Add(field);
        }

        public void VisitMethod(QNode node)
        {
            var method = EnumResolver.ResolveNodeType(node.Type);
            switch (method)
            {
                case NodeType.OrderBy:
                case NodeType.OrderByDescending:
                    var member = this.ContextField.Pop();
                    var query = this.ContextQuery.Pop();
                    this.SortingRequest.Add(new KeyValuePair<Field, NodeType>(member, method));
                    break;
                case NodeType.Where:
                    break;
                case NodeType.Contains:
                {
                    var value = string.Format("*{0}*", this.ContextValue.Pop());
                    var term = new QueryStringQuery {DefaultField = this.ContextField.Pop(), Query = value};
                    this.ContextQuery.Push(term);
                }
                    break;
                case NodeType.StartsWith:
                {
                    var term = new PrefixQuery {Field = this.ContextField.Pop(), Value = this.ContextValue.Pop()};
                    this.ContextQuery.Push(term);
                }
                    break;
                case NodeType.EndsWith:
                {
                    var value = string.Format("*{0}", this.ContextValue.Pop());
                    var term = new WildcardQuery {Field = this.ContextField.Pop(), Value = value};
                    this.ContextQuery.Push(term);
                }
                    break;
                case NodeType.In:
                {
                    var value = this.ContextValue.Pop() as List<string>;
                    var terms = new TermsQuery {Field = this.ContextField.Pop(), Terms = value};
                    this.ContextQuery.Push(terms);
                }
                    break;
                case NodeType.NotIn:
                {
                    var value = this.ContextValue.Pop() as List<string>;
                    var terms = !new TermsQuery {Field = this.ContextField.Pop(), Terms = value};
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