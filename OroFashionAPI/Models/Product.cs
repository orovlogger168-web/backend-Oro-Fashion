using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OroFashionAPI.Models;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id {get; set;}

    [BsonElement("name")]
    public string Name {get; set;} = string.Empty;

    [BsonElement("category")]
    public string Category {get; set;} = string.Empty;

    [BsonElement("subCategory")]
    public string SubCategory {get; set;} = string.Empty;

    [BsonElement("price")]
    public Decimal Price {get; set;}

    [BsonElement("stock")]
    public int Stock {get; set;}

    [BsonElement("image")]
    public string Image {get; set;} = string.Empty;

    [BsonElement("size")]
    //public string Sizes {get; set;} = string.Empty;
    public List<string> Sizes {get; set;} = new();
    [BsonElement("colors")]
    public List<string> Colors {get; set;} = new();

    [BsonElement("featured")]
    public bool? Featured {get; set;}

    [BsonElement("description")]
    public string Description {get; set;} = string.Empty;

    [BsonElement("isAvailable")]
    public bool IsAvailable {get; set;} = true;

    [BsonElement("createAt")]
    public DateTime CreateAt {get; set;} = DateTime.UtcNow;

    [BsonElement("updateAt")]
    public DateTime UpdateAt {get; set;} = DateTime.UtcNow;
}