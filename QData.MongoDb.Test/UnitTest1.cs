using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QData.MongoDb.Test
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Threading;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    using QData.ExpressionProvider;
    using QData.LinqConverter;
    using QData.LinqConverter.Extentions;
    using QData.Querable.Extentions;

    [TestClass]
    public class UnitTest1
    {
        private IMongoQueryable<Customer> baseCustomerQuery;

        private IMongoQueryable<Contract> baseContractQuery;

        private List<Customer> customers;

        private List<Contract> contracts;

        [TestInitialize()]
        public void Initialize()
        {
            this.contracts = new List<Contract>();
            this.contracts.Add(new Contract() { Id = 1, Descriptions = "test1" });
            this.contracts.Add(new Contract() { Id = 2, Descriptions = "test2" });
            this.contracts.Add(new Contract() { Id = 3, Descriptions = "test3" });
            this.contracts.Add(new Contract() { Id = 4, Descriptions = "test4" });

            this.customers = new List<Customer>();
            this.customers.Add(new Customer() { Id = 1, Vorname = "Otto", Nachname = "Fuchs", Age = 34 });
            this.customers.Add(new Customer() { Id = 2, Vorname = "Hans", Nachname = "Müller", Age = 32 });
            this.customers.Add(new Customer() { Id = 3, Vorname = "Dirk", Nachname = "Fischer", Age = 39 });
            this.customers.Add(new Customer() { Id = 4, Vorname = "Dirk", Nachname = "Berger", Age = 40 });
            this.customers.Add(new Customer() { Id = 5, Vorname = "Alex", Nachname = "Goldberg", Age = 31 });


            this.contracts[0].CustomerId = this.customers[0].Id;
            this.contracts[1].CustomerId = this.customers[0].Id;
            this.contracts[2].CustomerId = this.customers[1].Id;
            this.contracts[3].CustomerId = this.customers[0].Id;
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("test");
            database.DropCollection("Customer");
            database.DropCollection("Contract");
            var customerCollection = database.GetCollection<Customer>("Customer");
            if (customerCollection.Count(x => true) == 0)
            {
                foreach (var person in this.customers)
                {
                    customerCollection.InsertOne(person);
                }
            }

            var contractsCollection = database.GetCollection<Contract>("Contract");
            if (contractsCollection.Count(x => true) == 0)
            {
                foreach (var contract in this.contracts)
                {
                    contractsCollection.InsertOne(contract);
                }
            }

            (this.customers[0].Childs as List<Contract>).Add(this.contracts[0]);
            (this.customers[0].Childs as List<Contract>).Add(this.contracts[1]);
            (this.customers[0].Childs as List<Contract>).Add(this.contracts[3]);
            (this.customers[1].Childs as List<Contract>).Add(this.contracts[2]);

            this.baseCustomerQuery = customerCollection.AsQueryable();
            this.baseContractQuery = contractsCollection.AsQueryable();

        }

        [TestMethod]
        public void WhereTestMethod1()
        {

            var query = new EnumerableSource<Customer>().Where(x => x.Vorname.Contains("a"));
            var node = query.Serialize();
            
            var sourceExpression = new QNodeConverter(this.baseCustomerQuery).Convert(node);
            var source = this.baseCustomerQuery.Execute(sourceExpression) as List<Customer>;

            var targetExpression = new QNodeConverter(this.customers.AsQueryable()).Convert(node);
            var target = this.customers.AsQueryable().Execute(targetExpression) as List<Customer>;

            Assert.AreEqual(target.Count, source.Count);
            for (int i = 0; i < target.Count; i++)
            {
                Assert.AreEqual(target[i].Vorname, source[i].Vorname);
                Assert.AreEqual(target[i].Nachname, source[i].Nachname);
            }
        }

        [TestMethod]
        public void ProjectionTestMethod1()
        {

            var query = new EnumerableSource<Customer>().Select(x => new { name = x.Vorname});
            var node = query.Serialize();
            
            var sourceExpression = new QNodeConverter(this.baseCustomerQuery).Convert(node);
            var source = this.baseCustomerQuery.Execute(sourceExpression);

            var targetExpression = new QNodeConverter(this.customers.AsQueryable()).Convert(node);
            var target = this.customers.AsQueryable().Execute(targetExpression);

            //Assert.AreEqual(target.Count, source.Count);
            //for (int i = 0; i < target.Count; i++)
            //{
            //    Assert.AreEqual(target[i].Vorname, source[i].Vorname);
            //    Assert.AreEqual(target[i].Nachname, source[i].Nachname);
            //}

        }

        [TestMethod]
        public void JoinTestMethod1()
        {
            var joinQuery = from cust in this.baseCustomerQuery
                            join ctr in this.baseContractQuery on cust.Id equals ctr.CustomerId into joined select new Customer() 
                                                                                                                       {
                                                                                                                           Id = cust.Id,
                                                                                                                           Vorname = cust.Vorname,
                                                                                                                           Nachname = cust.Nachname,
                                                                                                                           Age = cust.Age,
                                                                                                                           Childs = joined
                                                                                                                       };
            //var r = joinQuery.ToList();
            var query = new EnumerableSource<Customer>().Where(x => x.Childs.Any(c => c.Descriptions.Contains("test"))).OrderByDescending(x => x.Nachname);
            var node = query.Serialize();
            var converter = new QNodeConverter(joinQuery);
            var expression = converter.Convert(node);
            var target = joinQuery.Execute(expression) as List<Customer>;
            var source = this.customers.AsQueryable().Execute(expression) as List<Customer>;

            Assert.AreEqual(target.Count,source.Count);
            for (int i = 0; i < target.Count; i++)
            {
                Assert.AreEqual(target[i].Vorname, source[i].Vorname);
                Assert.AreEqual(target[i].Nachname, source[i].Nachname);
            }

        }
    }
}
