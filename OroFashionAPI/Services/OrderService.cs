using MongoDB.Driver;
using OroFashionAPI.Models;
using OroFashionAPI.DTOs;

namespace OroFashionAPI.Services;

public class OrderService
{
    private readonly MongoDBService _mongodb;
    private readonly ProductServices _productService;

    public OrderService(MongoDBService mongodb, ProductServices productService)
    {
        _mongodb = mongodb;
        _productService = productService;
    }
    //create a new order and save to mongodb
    //userId extracted from jwt token in controller - not from angular body
    public async Task<Order> CreateAsync(string userId, CreateOrderDTO dto)
{
    var orderRef = $"ORO-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

    var items = dto.Items.Select(i => new OrderItems
    {
        ProductId = i.ProductId,
        ProductName = i.ProductName,
        ProductImage = i.ProductImage,
        Price = i.Price,
        Quantity = i.Quantity,
        Size = i.Size,
        color = i.Colors,
        LineTotal = i.LineTotal
    }).ToList();

    var subtotal = items.Sum(i => i.LineTotal);

    var order = new Order
    {
        OrderRef = orderRef,
        UserId = userId,
        CustomerName = dto.CustomerName,
        CustomerEmail = dto.CustomerEmail,
        CustomerPhone = dto.CustomerPhone,
        ShippingAddress = dto.ShippingAddress,
        Items = items,
        SubTotal = dto.Subtotal,
        DeliveryFee = dto.DeliveryFee,
        Total = dto.Total,
        PaymentMethod = dto.PaymentMethod,
        Status = dto.Status == "paid" ? "paid" : "pending"
    };

    await _mongodb.Orders.InsertOneAsync(order);

    // safely decrement stock — skip if productId is invalid
    foreach (var item in dto.Items)
    {
        try
        {
            // only decrement if productId looks like a valid MongoDB ObjectId
            if (!string.IsNullOrEmpty(item.ProductId) && item.ProductId.Length == 24)
            {
                await _productService.DecrementStockAsync(item.ProductId, item.Quantity);
            }
        }
        catch (Exception ex)
        {
            // log but don't fail the order
            Console.WriteLine($"Stock decrement failed for {item.ProductId}: {ex.Message}");
        }
    }

    return order;
}

    // get all orders for one specific user (for profile page)
    public async Task<List<Order>> GetUserOrderAsync(string userId) =>
        await _mongodb.Orders
        .Find(o => o.UserId == userId)
        .SortByDescending(o => o.CreateAt)
        .ToListAsync();
    
    // get ALL orders for admin dashboard
    public async Task<List<Order>> GetAllOrderAsync() => 
    await _mongodb.Orders
    .Find(_ => true)
    .SortByDescending(o => o.CreateAt)
    .ToListAsync();
    // get one specific order by MongoDB ObjectId
    public async Task<Order?> GetByIdAsync(string id) =>
    await _mongodb.Orders
    .Find(o => o.Id == id)
    .FirstOrDefaultAsync();
    // admin: update order status
    public async Task<bool> UpdateStatusAsync(string id, string status)
    {
        var update = Builders<Order>.Update
        .Set(o => o.Status, status)
        .Set(o => o.UpdateAt, DateTime.UtcNow);

        var result = await _mongodb.Orders.UpdateOneAsync(o => o.Id == id, update);
        return result.ModifiedCount > 0;
    }
}