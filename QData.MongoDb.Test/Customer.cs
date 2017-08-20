using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace QData.MongoDb.Test
{
    public class Customer
    {
        public Customer()
        {
            this.Contracts = new List<Contract>();
        }

        [BsonId]
        public int Id { get; set; }

        [BsonElement("vorname")]
        public string Vorname { get; set; }

        [BsonElement("nachname")]
        public string Nachname { get; set; }

        [BsonElement("age")]
        public int Age { get; set; }

        [BsonElement("contracts")]
        public IEnumerable<Contract> Contracts { get; set; }
    }
}