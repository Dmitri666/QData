using MongoDB.Bson.Serialization.Attributes;

namespace QData.MongoDb.Test
{
    public class Contract
    {
        [BsonId]
        public int Id { get; set; }

        [BsonElement("descriptions")]
        public string Descriptions { get; set; }

        [BsonElement("customerId")]
        public int CustomerId { get; set; }

        
    }
}
