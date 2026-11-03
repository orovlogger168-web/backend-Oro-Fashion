using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OroFashionAPI.Models;

public class Newsletter
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id {get; set;}

    [BsonElement("email")]
    public string Email {get; set;} = string.Empty;

    [BsonElement("subscribeAt")]
    public DateTime SubscribeAt {get; set;} = DateTime.UtcNow;
}