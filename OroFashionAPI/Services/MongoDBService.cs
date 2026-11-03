using MongoDB.Driver;
using OroFashionAPI.Models;
using OroFashionAPI.Setting;

using Microsoft.Extensions.Options;

namespace OroFashionAPI.Services;

public class MongoDBService
{
    //IMongoDatabase = Represents a MongoDB database.
    //_database = Variable name that stores the database object.
    private readonly IMongoDatabase _database;
    public MongoDBService(IOptions<MongoDBSettings> settings)
    {
        // MongoClient = establishes connection to MongoDB server
        // settings.Value = unwraps IOptions to get actual MongoDBSettings object
        var client = new MongoClient(settings.Value.ConnectionString);
        // GetDatabase = selects OroFashionDB (creates it on first write if not exists)
        // _database = client send databaseName to database because client contain databaseName
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }
    // <User> Tells MongoDB to map documents to the User model.
    // Users The property name (a shortcut) that you use in C# code.
    // "Users" The actual MongoDB collection name in the database.
    // Users is not used to store the data itself. Instead, it is a property
            // that gives you access to the Users collection, allowing you to insert,
            //  read, update, and delete User documents easily.
    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
    public IMongoCollection<Order> Orders => _database.GetCollection<Order>("Orders");
    public IMongoCollection<Favorite> Favorites => _database.GetCollection<Favorite>("Favorites");
    public IMongoCollection<Review> Reviews => _database.GetCollection<Review>("Reviews");
    public IMongoCollection<Cart> Carts => _database.GetCollection<Cart>("Carts");
    public IMongoCollection<Newsletter> Newletters =>  _database.GetCollection<Newsletter>("Newletters");
}