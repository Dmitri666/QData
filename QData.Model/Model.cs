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
                List<object> result = new List<object>();
                IEnumerator enumerator = data.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    result.Add(enumerator.Current);
                }
                return result;
            }
            
            return this.mapperConfiguration.CreateMapper().Map<IEnumerable<TM>>(data);

        }


        #endregion
    }
}