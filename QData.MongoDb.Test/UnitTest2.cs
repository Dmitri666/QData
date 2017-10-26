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
    public class UnitTest2
    {
        private IMongoQueryable<Customer> baseCustomerQuery;

        //private IMongoQueryable<Contract> baseContractQuery;

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

            ((List<Contract>)this.customers[0].Contracts).Add(this.contracts[0]);
            ((List<Contract>)this.customers[0].Contracts).Add(this.contracts[1]);
            ((List<Contract>)this.customers[0].Contracts).Add(this.contracts[2]);
            ((List<Contract>)this.customers[2].Contracts).Add(this.contracts[3]);
            
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("test");
            database.DropCollection("Customer");
            database.DropCollection("Contract");
            var customerCollection = database.GetCollection<Customer>("Customer");
            
                foreach (var person in this.customers)
                {
                    customerCollection.InsertOne(person);
                }
            
            this.baseCustomerQuery = customerCollection.AsQueryable();
           

        }

        [TestMethod]
        public void WhereTestMethod1()
        {

            var query = new EnumerableSource<Customer>().Where(x => x.Vorname.ToLower().Contains("d"));
            var node = query.Serialize();
            
            var sourceExpression = new QNodeConverter().Convert(this.baseCustomerQuery,node);
            var source = this.baseCustomerQuery.Execute(sourceExpression) as List<Customer>;

            var targetExpression = new QNodeConverter(new QNodeConverterSettings() {QueryStringIgnoreCase = false }).Convert(this.customers.AsQueryable(), node);
            var target = this.customers.AsQueryable().Execute(targetExpression) as List<Customer>;

            Assert.AreEqual(target.Count, source.Count);
            for (int i = 0; i < target.Count; i++)
            {
                Assert.AreEqual(target[i].Vorname, source[i].Vorname);
                Assert.AreEqual(target[i].Nachname, source[i].Nachname);
            }
        }

        [TestMethod]
        public void WhereTestMethod2()
        {

            var query = new EnumerableSource<Customer>().Where(x => x.Vorname.ToLower() == "otto");
            var node = query.Serialize();

            var sourceExpression = new QNodeConverter().Convert(this.baseCustomerQuery, node);
            var source = this.baseCustomerQuery.Execute(sourceExpression) as List<Customer>;

            var targetExpression = new QNodeConverter(new QNodeConverterSettings() { QueryStringIgnoreCase = false }).Convert(this.customers.AsQueryable(), node);
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
            var v = this.baseCustomerQuery.SelectMany(x => x.Contracts).Where(c => c.Descriptions.ToLower().Contains("test")).ToList();
            var query = new EnumerableSource<Customer>().Select(x => new  { Vorname = x.Vorname, Contracts = x.Contracts });
            var node = query.Serialize();
            
            var sourceExpression = new QNodeConverter().Convert(this.baseCustomerQuery,node);
            var source = this.baseCustomerQuery.Execute(sourceExpression);

            var targetExpression = new QNodeConverter().Convert(this.customers.AsQueryable(),node);
            var target = this.customers.AsQueryable().Execute(targetExpression);

            //Assert.AreEqual(target.Count, source.Count);
            //for (int i = 0; i < target.Count; i++)
            //{
            //    Assert.AreEqual(target[i].Vorname, source[i].Vorname);
            //    Assert.AreEqual(target[i].Nachname, source[i].Nachname);
            //}

        }

        
    }
}
