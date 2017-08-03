﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QData.QueryContainerProvider
{
    using Nest;

    using Qdata.Contract;

    
    public class QueryContainerBuilder
    {
        private ElasticClient client;

        public QueryContainerBuilder(ElasticClient client)
        {
            this.client = client;
        }

        public QueryContainer Convert<T>(QNode descriptor) where T : class
        {
            var converter = new QNodeConverter<T>();
            descriptor.Accept(converter);
            var result = this.client.Search<T>(s => s.Sort(converter.Sorting).Query(converter.Query));
            return null;
        }
    }
}