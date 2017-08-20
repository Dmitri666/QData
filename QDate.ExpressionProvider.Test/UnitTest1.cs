using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QData.ExpressionProvider;
using QData.LinqConverter;
using QData.LinqConverter.Extentions;
using QData.Querable.Extentions;

namespace QDate.ExpressionProvider.Test
{
    [TestClass]
    public class UnitTest1
    {
        private List<PersonDto> persons;
        [TestInitialize()]
        public void Initialize()
        {
            this.persons = new List<PersonDto>();
            this.persons.Add(new PersonDto() { Vorname = "Hans", Nachname = "Müller"});
            this.persons.Add(new PersonDto() { Vorname = "Dirk", Nachname = "Fischer" });
            this.persons.Add(new PersonDto() { Vorname = "Stefan", Nachname = "Heinrich" });
            this.persons[0].Mitarbeiters.Add(this.persons[1]);
            this.persons[0].Mitarbeiters.Add(this.persons[2]);
            this.persons[1].Leiter = this.persons[0];
            this.persons[2].Leiter = this.persons[0];
        }

        [TestMethod]
        public void TestMethod1()
        {
            var query = new EnumerableSource<PersonDto>().Where(x => x.Vorname.Contains("a"));
            var node = query.Serialize();
            var converter = new QNodeConverter();
            var expression = converter.Convert(this.persons.AsQueryable(),node);
            var result = this.persons.AsQueryable().Execute(expression);

        }

        [TestMethod]
        public void InTestMethod()
        {
            var list = new List<string>() { "Hans" , "Dirk" };
            var query = new EnumerableSource<PersonDto>().Where(x => list.Contains(x.Vorname));
            var node = query.Serialize();
            var converter = new QNodeConverter();
            var expression = converter.Convert(this.persons.AsQueryable(),node);
            var result = this.persons.AsQueryable().Execute(expression);

        }

        [TestMethod]
        public void StartWithMethod1()
        {
            var query = new EnumerableSource<PersonDto>().Where(x => x.Vorname.StartsWith("a"));
            var node = query.Serialize();
            var converter = new QNodeConverter();
            var expression = converter.Convert(this.persons.AsQueryable(),node);
            var result = this.persons.AsQueryable().Execute(expression);

        }

        [TestMethod]
        public void EndWithMethod1()
        {
            var query = new EnumerableSource<PersonDto>().Where(x => x.Vorname.EndsWith("a"));
            var node = query.Serialize();
            var converter = new QNodeConverter();
            var expression = converter.Convert(this.persons.AsQueryable(),node);
            var result = this.persons.AsQueryable().Execute(expression);

        }

        [TestMethod]
        public void TakeMethod1()
        {
            var query = new EnumerableSource<PersonDto>().Where(x => x.Vorname.EndsWith("a")).OrderBy(x => x.Vorname).Take(1);
            var node = query.Serialize();
            var converter = new QNodeConverter();
            var expression = converter.Convert(this.persons.AsQueryable(),node);
            var result = this.persons.AsQueryable().Execute(expression);

        }

        [TestMethod]
        public void SkipMethod1()
        {
            var query = new EnumerableSource<PersonDto>().Where(x => x.Vorname.EndsWith("a")).OrderBy(x => x.Vorname).Skip(1); 
            var node = query.Serialize();
            var converter = new QNodeConverter();
            var expression = converter.Convert(this.persons.AsQueryable(),node);
            var result = this.persons.AsQueryable().Execute(expression);

        }

        [TestMethod]
        public void CountMethod1()
        {
            var query = new EnumerableSource<PersonDto>().Where(x => x.Mitarbeiters.Count > 0);
            var node = query.Serialize();
            var converter = new QNodeConverter();
            var expression = converter.Convert(this.persons.AsQueryable(),node);
            var result = this.persons.AsQueryable().Execute(expression);

        }

        [TestMethod]
        public void QueryStringMethod1()
        {
            var query = new EnumerableSource<PersonDto>().QueryString("a",dto => new object[] {dto.Vorname,dto.Nachname}).Where(x => x.Mitarbeiters.Count > 0);
            var node = query.Serialize();
            var converter = new QNodeConverter();
            var expression = converter.Convert(this.persons.AsQueryable(),node);
            var result = this.persons.AsQueryable().Execute(expression);

        }

        [TestMethod]
        public void QueryStringMethod2()
        {
            var query = new EnumerableSource<PersonDto>().QueryString("a", dto => new object[] { dto.Vorname, dto.Nachname, dto.Leiter.Vorname });
            var node = query.Serialize();
            var converter = new QNodeConverter();
            var expression = converter.Convert(this.persons.AsQueryable(), node);
            var result = this.persons.AsQueryable().Execute(expression);

        }

        [TestMethod]
        public void QueryStringMethod3()
        {
            var source = new DynamicSource().QueryString("a", new List<string>() { "Vorname" , "Nachname" , "Leiter.Vorname" });
            var node = source.Query;
            var converter = new QNodeConverter();
            var expression = converter.Convert(this.persons.AsQueryable(),node);
            var result = this.persons.AsQueryable().Execute(expression);

        }
    }
}
