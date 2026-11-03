using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace OroFashionAPI.Models;

public class Favorite
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id {get; set;}

    [BsonElement("userId")]
    public string UserId {get; set;} = string.Empty;

    [BsonElement("productIds")]
    public List<string> ProductIds {get; set;} = new();

    [BsonElement("updateAt")]
    public DateTime UpdateAt {get; set;} = DateTime.UtcNow; 
}