using MongoDB.Bson.Serialization.Attributes;

namespace WebDbFirst.Models
{
    public class CustomerMDB : Customer
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string _id { get; set; }
        public string? LocationHotel { get; set; }
        public string? Price { get; set; }

    }
}
