using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QData.QueryContainerProvider.Test
{
    using System.Linq;

    using Nest;

    using Qdata.Contract;

    using QData.LinqConverter;

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var connectionSettings =
                new ConnectionSettings().InferMappingFor<TestDto>(i => i.IndexName("my-projects").TypeName("project"))
                    .EnableDebugMode()
                    .PrettyJson()
                    .RequestTimeout(TimeSpan.FromMinutes(2));

            var client = new ElasticClient(connectionSettings);
            
            var set = new QSet<TestDto>();
            var query = set.OrderBy(x => x.Vorname).OrderByDescending(x => x.Nachname);
            var node = set.Serialize(query);
            var d = new QueryContainerBuilder(client);
            var result = d.Convert<TestDto>(node);


        }
    }
}
