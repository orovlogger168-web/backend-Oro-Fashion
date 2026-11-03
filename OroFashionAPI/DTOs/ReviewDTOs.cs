namespace OroFashionAPI.DTOs;

public class CreateReviewDTO
{
    public string ProductId {get; set;} = string.Empty;
    public int Rating {get; set;}
    public string Comment {get; set;} = string.Empty;
}