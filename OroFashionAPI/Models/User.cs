using MongoDB.Bson;

using MongoDB.Bson.Serialization.Attributes;

namespace OroFashionAPI.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id {get; set;}

    [BsonElement("fullName")]
    public string FullName {get; set;} = string.Empty;

    [BsonElement("email")]
    public string Email {get; set;} = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash {get; set;} = string.Empty;

    [BsonElement("phone")]
    public string Phone {get; set;} = string.Empty;

    [BsonElement("address")]
    public string Address {get; set;} = string.Empty;

    [BsonElement("role")]
    public string Role {get; set;} = "Customer";

    [BsonElement("createAt")]
    public DateTime CreateAt {get; set;} = DateTime.UtcNow;

    [BsonElement("lastLoginAt")]
    public DateTime? LastLoginAt {get; set;}

    [BsonElement("isDeleted")]
    public bool IsDeleted {get; set;} = false;
}