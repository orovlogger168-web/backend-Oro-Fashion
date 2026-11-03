using System.Globalization;

namespace OroFashionAPI.DTOs;

public class CreateProductDTO
{
    public string Name {get; set;} = string.Empty;
    public string Category {get; set;} = string.Empty;
    public string Subcategory {get; set;} = string.Empty;
    public decimal Price {get; set;}
    public int Stock {get; set;}
    public string Image {get; set;} = string.Empty;
    public List<string> Sizes {get; set;} = new();
    public List<string> Colors {get; set;} = new();
    public bool? Feature {get; set;}
    public string Description {get; set;} = string.Empty;
}
public class UpdateStockDTO
{
    public string ProductId {get; set;} = string.Empty;
    public int Quantity {get; set;}
}