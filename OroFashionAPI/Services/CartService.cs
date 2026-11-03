using MongoDB.Driver;
using OroFashionAPI.Models;
using OroFashionAPI.DTOs;

namespace OroFashionAPI.Services;

public class CartService
{
    private readonly MongoDBService _mongodb;
    public CartService(MongoDBService mongodb)
    {
        _mongodb = mongodb;
    }
    //get user cart from Mongodb
    public async Task<Cart> GetCartAsync(string userId) =>
    await _mongodb.Carts
    .Find(c => c.UserId == userId)
    .FirstOrDefaultAsync();
    // sync Angular cart to MongoDB (replace entire cart)
    public async Task SyncCartAsync(string userId, SyncCartDTO dto)
    {
        var items = dto.Items.Select(i => new CartItem
        {
           ProductId = i.ProductId,
           Quantity = i.Quantity,
           Size = i.Size,
           Color = i.Color
        }).ToList();
        var existing = await GetCartAsync(userId);
        if(existing == null)
        {
            var newCart = new Cart
            {
                // create new cart document for this user
                UserId = userId,
                Items = items
            };
            await _mongodb.Carts.InsertOneAsync(newCart);
        }
        else
        {
            // replace existing cart items
            var update = Builders<Cart>.Update
            .Set(c => c.Items, items)
            .Set(c => c.UpdateAt, DateTime.UtcNow);
            await _mongodb.Carts.UpdateOneAsync(c => c.UserId == userId, update);
        }
    }
    // clear cart after order placed
    public async Task ClearCartAsync(string userId)
    {
        var update = Builders<Cart>.Update
        .Set(c => c.Items, new List<CartItem>())
        .Set(c => c.UpdateAt, DateTime.UtcNow);
        await _mongodb.Carts.UpdateOneAsync(c => c.UserId == userId, update);
    }
}