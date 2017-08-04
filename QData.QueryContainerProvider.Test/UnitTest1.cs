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
                new ConnectionSettings(new Uri("http://rnd-es-1a:9200")).DefaultIndex("wbv_harburg_shell_wasserwerksearchindex");
            connectionSettings.DisableDirectStreaming();
            var client = new ElasticClient(connectionSettings);
            
            var set = new QSet<WasserwerkSearchIndex>();
            var query = set.Where(x => x.Bezeichnung.Contains("a"));
            var node = set.Serialize(query);
            var d = new QueryContainerBuilder(client);
            var result = d.Convert<WasserwerkSearchIndex>(node);


        }
    }
}
