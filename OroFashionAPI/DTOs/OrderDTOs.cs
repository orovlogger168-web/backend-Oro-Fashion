namespace OroFashionAPI.DTOs;

public class CreateOrderItemDTO
{
    public string ProductId {get; set;} = string.Empty;
    public string ProductName {get; set;} = string.Empty;
    public string ProductImage {get; set;} = string.Empty;
    public decimal Price {get; set;}
    public int Quantity {get; set;}
    public string Size {get; set;} = string.Empty;
    public string Colors {get; set;} = string.Empty;
    public decimal LineTotal {get; set;}
}
public class CreateOrderDTO
{
    public string CustomerName {get; set;} = string.Empty;
    public string CustomerEmail {get; set;} = string.Empty;
    public string CustomerPhone {get; set;} = string.Empty;
    public string ShippingAddress {get; set;} = string.Empty;
    public List<CreateOrderItemDTO> Items {get; set;} = new();
    public decimal Subtotal {get; set;}
    public decimal DeliveryFee {get; set;}
    public decimal Total {get; set;}
    public string PaymentMethod {get; set;} = string.Empty;
    public string Status {get; set;} = "pending";
}
public class UpdateOrderStatusDTO
{
    public string Status {get; set;} = string.Empty;
}