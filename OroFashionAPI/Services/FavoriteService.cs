using MongoDB.Driver;
using OroFashionAPI.Models;

namespace OroFashionAPI.Services;

public class FavoriteServices
{
    private readonly MongoDBService _mongodb;
    public FavoriteServices(MongoDBService mongodb)
    {
        _mongodb = mongodb;
    }
    // get the list of favorited product IDs for a user
    public async Task<List<string>> GetFavoriteIdAsync(string userId)
    {
        var favorite = await _mongodb.Favorites
        .Find(f => f.UserId == userId)
        .FirstOrDefaultAsync();
        //?. is null condition and ?? is if the left side is null so we use left side
        return favorite?.ProductIds ?? new List<string>();
    }
    public async Task<List<string>> ToggleFavoriteAsync(string userId, string ProductId)
    {
        var existing = await _mongodb.Favorites
        .Find(f => f.UserId == userId)
        .FirstOrDefaultAsync();
        if(existing == null)
        {
            var newFavorite = new Favorite
            {
                UserId = userId,
                ProductIds = new List<string> {ProductId}
            };
            await _mongodb.Favorites.InsertOneAsync(newFavorite);
            return newFavorite.ProductIds;
        }
        List<string> updateIds;
        if (existing.ProductIds.Contains(ProductId))
        {
            // already favorited — remove it
            // .Where() = filter out this productId (like .filter() in JavaScript)
            updateIds = existing.ProductIds.Where(id => id != ProductId).ToList();
        }
        else
        {
            // not favorited — add it
            // new list with existing items + new productId
            updateIds = new List<string>(existing.ProductIds) {ProductId};
        }
        //update the document in mongodb
        var update = Builders<Favorite>.Update
        .Set(f => f.ProductIds, updateIds)
        .Set(f => f.UpdateAt, DateTime.UtcNow);
        await _mongodb.Favorites.UpdateOneAsync(f => f.UserId == userId, update);
        return updateIds;
    }
    public async Task ClearAsync(string userId)
    {
        var update = Builders<Favorite>.Update
        .Set(f => f.ProductIds, new List<string>())
        .Set(f => f.UpdateAt, DateTime.UtcNow);
        await _mongodb.Favorites.UpdateOneAsync(f => f.UserId == userId, update);
    }
}