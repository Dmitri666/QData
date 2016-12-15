// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Repository.cs" company="">
//   
// </copyright>
// <summary>
//   The repository.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using Qdata.Json.Contract;
using QData.SqlProvider;

namespace QData.Model
{
    using System;

    using QData.Common;

    public class Model<TM>
        where TM : IModelEntity
    {
        #region Constructors and Destructors

        private readonly MapperConfiguration mapperConfiguration;
        private Type SourceType { get; set; }
        
        public Model(MapperConfiguration mapping)

        {
            this.mapperConfiguration = mapping;
        }

        #endregion

        #region Public Properties

        public object Find(QDescriptor descriptor, IQueryable query)
        {

            var provider = new ExpressionProvider(this.mapperConfiguration,query.Expression);
            var epression = provider.ConvertToExpression(descriptor);

            var data = query.Provider.CreateQuery(epression);

            if (descriptor.IsProjection)
            { 
                var listType = typeof(List<>);
                var targetType = listType.MakeGenericType(data.ElementType);
                var result = Activator.CreateInstance(targetType);
                var methodInfo = targetType.GetMethod("AddRange");
                methodInfo.Invoke(result, new object[] { data });
                
                return result;
            }
            
            return this.mapperConfiguration.CreateMapper().Map<IEnumerable<TM>>(data);

        }


        #endregion
    }
}