using MongoDB.Driver;
using OroFashionAPI.Models;
using OroFashionAPI.DTOs;

namespace OroFashionAPI.Services;

public class ReviewService
{
    private readonly MongoDBService _mongodb;
    public ReviewService(MongoDBService mongodb)
    {
        _mongodb = mongodb;
    }
    //get all review for a specific product
    public async Task<List<Review>> GetByProductAsync(string productId) =>
    await _mongodb.Reviews
    .Find(r => r.ProductId == productId)
    .SortByDescending(r => r.CreateAt)
    .ToListAsync();
    public async Task<double> GetAverageRatingAsync(string productId)
    {
        // get average rating for a product
        var reviews = await GetByProductAsync(productId);
        if(!reviews.Any()) return 0;
        // .Average() = sum all ratings and divide by count
        // r => r.Rating = for each review, use the Rating value
        return reviews.Average(r => r.Rating);
    }

    public async Task<(bool success, string message, Review? review)>
    CreateAsync(string userId, string userName, CreateReviewDTO dto)
    {
        if(dto.Rating < 1 || dto.Rating > 5)
        return (false, "Rating must be between 1 and 5.", null);

        //check if user already reviewed this product
        var alreadyReviewed = await _mongodb.Reviews
        .Find(r => r.ProductId == dto.ProductId && r.UserId == userId)
        .AnyAsync();

        if(alreadyReviewed)
        return (false, "You already reviewed this product.", null);

        var review = new Review
        {
            ProductId = dto.ProductId,
            UserId = userId,
            UserName = userName,
            Rating = dto.Rating,
            Comment = dto.Comment.Trim()
        };
        await _mongodb.Reviews.InsertOneAsync(review);
        return (true, "Review submitted.", review);
    }

    public async Task<bool> DeleteAsync(string reviewId) =>
    (await _mongodb.Reviews.DeleteOneAsync(r => r.Id == reviewId)).DeletedCount > 0;
}