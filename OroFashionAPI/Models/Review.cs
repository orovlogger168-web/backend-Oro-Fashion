using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OroFashionAPI.Models;

public class Review
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id {get; set;}

    [BsonElement("productId")]
    public string ProductId {get; set;} = string.Empty;

    [BsonElement("userId")]
    public string UserId {get; set;} = string.Empty;

    [BsonElement("userName")]
    public string UserName {get; set;} = string.Empty;

    [BsonElement("rating")]
    public int Rating {get; set;}

    [BsonElement("comment")]
    public string Comment {get; set;} = string.Empty;

    [BsonElement("createAt")]
    public DateTime CreateAt {get; set;} = DateTime.UtcNow;
}