using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Qdata.Json.Contract;

namespace QData.ExpressionProvider.Converters
{
    public class ArrayConverter : BaseConstantConverter
    {
        public ArrayConverter(Type target) : base(target)
        {

        }

        public override ConstantExpression ConvertToConstant(QNode node)
        {
            var valueType = node.Value.GetType();
            if (valueType.IsGenericType)
            {

                var list = (List<string>) node.Value;
                if (target == typeof (long))
                {
                    var converted = new List<Int64>();
                    list.ForEach(x => converted.Add(Convert.ToInt64(x)));
                    var exp = Expression.Constant(converted);
                    return exp;
                }
                if (target == typeof (DateTime))
                {
                    var converted = new List<DateTime>();
                    list.ForEach(x => converted.Add(Convert.ToDateTime(x)));
                    var exp = Expression.Constant(converted);
                    return exp;
                }

                //var listType = typeof(List<>);
                //var concreteType = listType.MakeGenericType(memberType);
                //var valueList = Activator.CreateInstance(concreteType);
                //var methodInfo = valueList.GetType().GetMethod("Add");
                //foreach (var stringValue in (List<string>)node.Value)
                //{
                //    var value = ConvertConstant(stringValue, propertyMap.DestinationPropertyType, out operatorType);
                //    methodInfo.Invoke(valueList, new object[] { value });
                //}
                
            }

            if (valueType == typeof(JArray))
            {
                var listType = typeof(List<>);
                var concreteType = listType.MakeGenericType(target);
                var valueList = Activator.CreateInstance(concreteType);
                var methodInfo = valueList.GetType().GetTypeInfo().GetMethod("Add");
                foreach (var value in (JArray)node.Value)
                {
                    methodInfo.Invoke(valueList, new object[] { value.ToObject(target) });
                }
                return Expression.Constant(valueList);
                
            }

            throw new NotImplementedException(string.Format("ArrayConverter :SourceValue {0} TargetType {1}",
                    node.Value,
                    target));
        }
    }
}
