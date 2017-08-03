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

        private List<KeyValuePair<Field, MethodType>> SortingRequest { get; set; }
            

        private QueryContainer QueryContainer => this.ContextQuery.Pop();

        public Func<Nest.SortDescriptor<T>, IPromise<IList<ISort>>> Sorting
        {
            get
            {
                return descriptor =>
                    {
                        foreach (var sorting in this.SortingRequest)
                        {
                            descriptor = sorting.Value == MethodType.OrderBy ? descriptor.Ascending(sorting.Key) : descriptor.Descending(sorting.Key);
                        }
                        return descriptor;
                    };
            }
            
        }

        public Func<Nest.QueryContainerDescriptor<T>, QueryContainer> Query
        {
            get
            {
                return descriptor => this.QueryContainer;

            }

        }

        public QNodeConverter()
        {
            this.ContextQuery = new Stack<QueryBase>();
            this.ContextField = new Stack<Field>();
            this.ContextValue = new Stack<object>();
            this.SortingRequest = new List<KeyValuePair<Field, MethodType>>();
        }

        public void VisitBinary(QNode node)
        {
            var left = this.ContextQuery.Pop();
            var right = this.ContextQuery.Pop();
            var binary = EnumResolver.ResolveBinary(node.Value);
            if (binary == BinaryType.And)
            {
                this.ContextQuery.Push(left && right);
            }
            else if (binary == BinaryType.Or)
            {
                this.ContextQuery.Push(left || right);
            }
            else if (binary == BinaryType.Contains)
            {
                var term = new TermQuery() {Field = this.ContextField.Pop() , Value = this.ContextValue.Pop()} ;
                this.ContextQuery.Push(term);
            }




        }

        public void VisitMember(QNode node)
        {
            this.ContextField.Push(new Field(Convert.ToString(node.Value)));
        }

        public void VisitQuerable(QNode node)
        {
            
        }

        public void VisitMethod(QNode node)
        {
            var member = this.ContextField.Pop();
            var method = EnumResolver.ResolveMethod(node.Value);
            switch (method)
            {
                case MethodType.OrderBy:
                case MethodType.OrderByDescending:
                    this.SortingRequest.Add(new KeyValuePair<Field, MethodType>(member, method));
                    break;
                case MethodType.Where:
                    break;
                default:
                    break;
            }


        }

        public void EnterContext(QNode node)
        {
            
        }

        public void LeaveContext(QNode node)
        {
            
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
    }
}