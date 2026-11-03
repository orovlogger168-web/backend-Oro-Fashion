using MongoDB.Driver;
using OroFashionAPI.Models;

namespace OroFashionAPI.Services;

// NewsletterService = handles newsletter email subscriptions
public class NewsletterService
{
    private readonly MongoDBService _mongodb;
    public NewsletterService(MongoDBService mongodb)
    {
        _mongodb = mongodb;
    }
    // subscribe an email to newsletter
    public async Task<(bool success, string message)> SubscribeAsync(string email)
    {
         // check if already subscribed
        var exists = await _mongodb.Newletters
        .Find(n => n.Email == email.ToLower().Trim())
        .AnyAsync();
        if(exists)
        return (false, "Email already subsribed.");
        await _mongodb.Newletters.InsertOneAsync(new Newsletter
        {
            Email = email.ToLower().Trim()
        });
        return (true, "Subscribe successfully");
    }
    // admin: get all subscribers
    public async Task<List<Newsletter>> GetAllAsync() =>
    await _mongodb.Newletters
    .Find(_ => true)
    .SortByDescending(n => n.SubscribeAt)
    .ToListAsync();
}