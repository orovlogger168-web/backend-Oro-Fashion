using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OroFashionAPI.Models;

public class OrderItems
{
    [BsonElement("productId")]
    public string ProductId {get; set;} = string.Empty;

    [BsonElement("productName")]
    public string ProductName {get; set;} = string.Empty;

    [BsonElement("productImage")]
    public string ProductImage {get; set;} = string.Empty;

    [BsonElement("price")]
    public decimal Price {get; set;}

    [BsonElement("quantity")]
    public int Quantity {get; set;}

    [BsonElement("size")]
    public string Size {get; set;} = string.Empty;

    [BsonElement("color")]
    public string color {get; set;} = string.Empty;

    [BsonElement("lineTotal")]
    public decimal LineTotal {get; set;}
}
public class Order
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id {get; set;} = string.Empty;

    [BsonElement("orderRef")]
    public string OrderRef {get; set;} = string.Empty;

    [BsonElement("userId")]
    public string UserId {get; set;} = string.Empty;

    [BsonElement("customerName")]
    public string CustomerName {get; set;} = string.Empty;

    [BsonElement("customerEmail")]
    public string CustomerEmail {get; set;} = string.Empty;

    [BsonElement("customerPhone")]
    public string CustomerPhone {get; set;} = string.Empty;

    [BsonElement("customerAddress")]
    public string CustomerAddress {get; set;} = string.Empty;

    [BsonElement("items")]
    public List<OrderItems> Items {get; set;} = new();

    [BsonElement("subTotal")]
    public decimal SubTotal {get; set;}

    [BsonElement("deliveryFee")]
    public decimal DeliveryFee {get; set;}

    [BsonElement("total")]
    public decimal Total {get; set;} = new();

    [BsonElement("paymentMethod")]
    public string PaymentMethod {get; set;} = string.Empty;

    [BsonElement("status")]
    public string Status {get; set;} = "pending";

    [BsonElement("createAt")]
    public DateTime CreateAt {get; set;} = DateTime.UtcNow;

    [BsonElement("updateAt")]
    public DateTime UpdateAt {get; set;} = DateTime.UtcNow;

    [BsonElement("shippingAddress")]
    public string ShippingAddress {get; set;} = string.Empty;
}