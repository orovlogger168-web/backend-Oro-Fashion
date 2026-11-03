using System.Data.SqlTypes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OroFashionAPI.Models;

public class CartItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id {get; set;}

    [BsonElement("productId")]
    public string ProductId {get; set;} = string.Empty;

    [BsonElement("quantity")]
    public int Quantity {get; set;}

    [BsonElement("size")]
    public string Size {get; set;} = string.Empty;

    [BsonElement("color")]
    public string Color {get; set;} = string.Empty;
}
public class Cart
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id {get; set;}

    [BsonElement("userId")]
    public string UserId {get; set;} = string.Empty;

    [BsonElement("items")]
    public List<CartItem> Items {get; set;} = new();

    [BsonElement("updateAt")]
    public DateTime UpdateAt {get; set;} = DateTime.UtcNow;
}