namespace OroFashionAPI.DTOs;

public class CartItemDTO
{
    public string ProductId {get; set;} = string.Empty;
    public int Quantity {get; set;}
    public string Size {get; set;} = string.Empty;
    public string Color {get; set;} = string.Empty;
}
public class SyncCartDTO
{
    public List<CartItemDTO> Items {get; set;} = new();
}