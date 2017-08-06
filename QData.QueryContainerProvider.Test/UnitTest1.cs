using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QData.LinqConverter.Extentions;

namespace QData.QueryContainerProvider.Test
{
    using System.Linq;

    using Nest;

    using Qdata.Contract;

    using QData.LinqConverter;

    

    [TestClass]
    public class UnitTest1
    {
        private ElasticClient client;
        private List<TestIndex> persons;

        [TestInitialize()]
        public void Initialize()
        {
            var connectionSettings =
                     new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("index");
            connectionSettings.DisableDirectStreaming();
            client = new ElasticClient(connectionSettings);
            persons = new List<TestIndex>();
            persons.Add(new TestIndex() { Id = 1, Vorname = "Otto", Nachname = "Fuchs" ,Age = 34 });
            persons.Add(new TestIndex() { Id = 2, Vorname = "Hans", Nachname = "Müller" , Age = 32 });
            persons.Add(new TestIndex() { Id = 3, Vorname = "Dirk", Nachname = "Fischer",Age = 39 });
            persons.Add(new TestIndex() { Id = 4, Vorname = "Dirk", Nachname = "Berger", Age = 40 });
            persons.Add(new TestIndex() { Id = 5, Vorname = "Alex", Nachname = "Goldberg", Age = 31 });
            client.Bulk(descriptor =>
            {
                return descriptor.CreateMany<TestIndex>(persons, (createDescriptor, index) =>
                {
                    foreach (var person in persons)
                    {
                        createDescriptor.Document(person);
                    }
                    return createDescriptor;
                });
            });
        }

        [TestMethod]
        public void TestMethod1()
        {
            
            
            var set = new EnumerableSource<TestIndex>();
            var query = set.Where(x => x.Vorname == "Dirk" || x.Nachname.Contains("b"));
            var node = query.Serialize();
            var d = new QueryContainerBuilder(client);
            var result = d.Convert<TestIndex>(node);


        }

        [TestMethod]
        public void WhereTestMethod1()
        {
            Expression<Func<TestIndex, bool>> predicate =
                (x) =>
                    (x.Vorname == "Dirk" && x.Nachname == "Fischer") ||
                    (x.Nachname.Contains("m") && x.Vorname.Contains("n"));

            var set = new EnumerableSource<TestIndex>();
            var query = set.Where(predicate);
            var node = query.Serialize();
            var d = new QueryContainerBuilder(client);
            var result = d.Convert<TestIndex>(node).ToList().OrderBy(x => x.Nachname).OrderBy(x => x.Vorname).ToList();

            var source = this.persons.Where(x => (x.Vorname == "Dirk" && x.Nachname == "Fischer") || (x.Nachname.ToLower().Contains("m") && x.Vorname.ToLower().Contains("n"))).OrderBy(x => x.Nachname).OrderBy(x => x.Vorname).ToList();
            Assert.AreEqual(result.Count, source.Count());
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(result[i].Vorname, source[i].Vorname);
                Assert.AreEqual(result[i].Nachname, source[i].Nachname);
            }


        }

        [TestMethod]
        public void InTestMethod1()
        {
            var ages = new List<int>() { 39, 31 };
            Expression<Func<TestIndex, bool>> predicate = x => ages.Contains(x.Age);



            var set = new EnumerableSource<TestIndex>();
            var query = set.Where(predicate);
            var node = query.Serialize();
            var d = new QueryContainerBuilder(client);
            var result = d.Convert<TestIndex>(node).OrderBy(x => x.Nachname).OrderBy(x => x.Vorname).ToList();
            var source = this.persons.AsQueryable().Where(predicate).OrderBy(x => x.Nachname).OrderBy(x => x.Vorname).ToList();
            Assert.AreEqual(result.Count, source.Count());
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(result[i].Vorname, source[i].Vorname);
                Assert.AreEqual(result[i].Nachname, source[i].Nachname);
            }

        }

        [TestMethod]
        public void SortTest1()
        {
            var set = new EnumerableSource<TestIndex>();
            var query = set.OrderBy(x => x.Vorname).OrderByDescending(x => x.Nachname);
            var node = query.Serialize();
            var d = new QueryContainerBuilder(client);
            var result = d.Convert<TestIndex>(node).ToList();
            var sorted = result.OrderBy(x => x.Vorname).ThenByDescending(x => x.Nachname).ToList();
            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreSame(result[i].Vorname, sorted[i].Vorname);
                Assert.AreSame(result[i].Nachname, sorted[i].Nachname);
            }

        }
    }
}
