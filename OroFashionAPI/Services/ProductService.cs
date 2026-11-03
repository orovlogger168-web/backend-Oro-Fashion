using MongoDB.Driver;
using OroFashionAPI.Models;
using OroFashionAPI.DTOs;
using MongoDB.Driver.Core.Misc;

namespace OroFashionAPI.Services;

public class ProductServices
{
    private readonly MongoDBService _mongodb;

    public ProductServices(MongoDBService mongodb)
    {
        _mongodb = mongodb;
    }
    //get all avaiable product
    // _ => true = no filter = match all (like SELECT * FROM products)
    public async Task<List<Product>> GetAllAsync()=>
    await _mongodb.Products
    .Find(p => p.IsAvailable)
    .ToListAsync();

    // get products filtered by category ("men", "women", "kids")
    // && = AND — both conditions must be true
    public async Task<List<Product>> GetByCategoryAsync(string category) =>
    await _mongodb.Products
    .Find(p => p.Category == category.ToLower() && p.IsAvailable)
    .ToListAsync();

    //get only featured product for home page
    public async Task<List<Product>> GetByFeaturedAsync() =>
    await _mongodb.Products
    .Find(p => p.Featured == true && p.IsAvailable)
    .ToListAsync();

    //Get one product by its MongoDB ObjectId
    //return null if not found (FirstOrDefaultAsync)
    public async Task<Product> GetByIdAsync(string id) =>
    await _mongodb.Products
    .Find(p => p.Id == id)
    .FirstOrDefaultAsync();

    //search product by name category subcategory and description
    //"i" option = case-insentive ("setec" match "Setec")
    public async Task<List<Product>> SearchAsync(string query)
    {
        //BsonRegularExpression = MongoDB regex search
        if(string.IsNullOrWhiteSpace(query))
        return await GetAllAsync();

        //Or = match if any of these condition is true
        var filter = Builders<Product>.Filter.Or(
            Builders<Product>.Filter.Regex(
                p => p.Name, new MongoDB.Bson.BsonRegularExpression(query, "i")
            ),
            Builders<Product>.Filter.Regex(
                p => p.Category, new MongoDB.Bson.BsonRegularExpression(query, "i")
            ),
            Builders<Product>.Filter.Regex(
                p => p.SubCategory, new MongoDB.Bson.BsonRegularExpression(query, "i")
            ),
            Builders<Product>.Filter.Regex(
                p => p.Description, new MongoDB.Bson.BsonRegularExpression(query, "i")
            )
        );
        return await _mongodb.Products.Find(filter).ToListAsync();
    }

    public async Task<Product> CreateAsync(CreateProductDTO dto)
    {
        var product = new Product
        {
            Name = dto.Name.Trim(),
            Category = dto.Category,
            SubCategory = dto.Subcategory,
            Price = dto.Price,
            Stock = dto.Stock,
            Image = dto.Image,
            Sizes = dto.Sizes,
            Colors = dto.Colors,
            Featured = dto.Feature,
            Description = dto.Description
        };
        await _mongodb.Products.InsertOneAsync(product);
        return product;
    }
    // admin: update an existing product
    public async Task<bool> UpdateAsync(string id, CreateProductDTO dto)
    {
        var update = Builders<Product>.Update
        .Set(p => p.Name, dto.Name)
        .Set(p => p.Category, dto.Category)
        .Set(p => p.SubCategory, dto.Subcategory)
        .Set(p => p.Price, dto.Price)
        .Set(p => p.Stock, dto.Stock)
        .Set(p => p.Image, dto.Image)
        .Set(p => p.Sizes, dto.Sizes)
        .Set(p => p.Colors, dto.Colors)
        .Set(p => p.Featured, dto.Feature)
        .Set(p => p.Description, dto.Description)
        .Set(p => p.UpdateAt, DateTime.UtcNow);

        var result = await _mongodb.Products.UpdateOneAsync(p => p.Id == id, update);
        return result.ModifiedCount > 0;
    }
    //admin: soft delete (hide from shop, keep data) 
    public async Task<bool> DeleteAsync(string id)
    {
        var update = Builders<Product>.Update
        .Set(p => p.IsAvailable, false)
        .Set(p => p.UpdateAt, DateTime.UtcNow);

        var result = await _mongodb.Products.UpdateOneAsync(p => p.Id == id, update);
        return result.ModifiedCount > 0;
    }
    //call after order placed - reduce stock by quantity and ordered
    public async Task DecrementStockAsync(string productId, int quantity)
    {
        var update = Builders<Product>.Update
        //Inc = mongoDB increase operator - adds negative value to decrement
        .Inc(p => p.Stock, -quantity)
        //set updateAt to track when stock add
        .Inc(p => p.UpdateAt, DateTime.UtcNow);

        await _mongodb.Products.UpdateOneAsync  (p => p.Id == productId, update);
    }
    public async Task SeedProductAsync()
    {
        //_ => true is read every document if it true
        var count = await _mongodb.Products.CountDocumentsAsync(_ => true);
        //if product already exists then return so method end immediately because we don't want duplicate products
        if(count > 0)
        return ;

        var products = new List<Product>
        {
            new() { Name="Midnight Tailored Blazer", Category="women", SubCategory="Outerwear", Price=58, Stock=14, Image="https://images.unsplash.com/photo-1529139574466-a303027c1d8b?auto=format&fit=crop&w=800&q=80", Sizes=new(){"S","M","L"}, Colors=new(){"Black","White"}, Featured=true, Description="Sharp monochrome blazer for city evenings and polished office looks." },
            new() { Name="Minimal Luxe Tee", Category="men", SubCategory="Tops", Price=24, Stock=30, Image="https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?auto=format&fit=crop&w=800&q=80", Sizes=new(){"S","M","L","XL"}, Colors=new(){"Black","White"}, Featured=true, Description="A premium cotton tee with a luxury drape and crisp silhouette." },
            new() { Name="Urban Street Cargo", Category="men", SubCategory="Bottoms", Price=36, Stock=20, Image="https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=800&q=80", Sizes=new(){"M","L","XL"}, Colors=new(){"Black","Stone"}, Description="Relaxed cargo pants that keep the silhouette clean and bold." },
            new() { Name="Soft Bloom Dress", Category="women", SubCategory="Dresses", Price=42, Stock=12, Image="https://images.unsplash.com/photo-1496747611176-843222e1e57c?auto=format&fit=crop&w=800&q=80", Sizes=new(){"S","M","L"}, Colors=new(){"White","Sand"}, Description="Lightweight modern dress made for refined weekend styling." },
            new() { Name="Junior Mono Hoodie", Category="kids", SubCategory="Outerwear", Price=26, Stock=17, Image="https://images.unsplash.com/photo-1519340241574-2cec6aef0c01?auto=format&fit=crop&w=800&q=80", Sizes=new(){"4Y","6Y","8Y"}, Colors=new(){"Black","Cream"}, Featured=true, Description="Cozy hoodie for kids with soft interior lining and modern fit." },
            new() { Name="Classic Pleated Skirt", Category="women", SubCategory="Bottoms", Price=30, Stock=22, Image="https://images.unsplash.com/photo-1492707892479-7bc8d5a4ee93?auto=format&fit=crop&w=800&q=80", Sizes=new(){"S","M","L"}, Colors=new(){"Black","Grey"}, Description="Elegant pleats with movement, texture, and minimal detail." },
            new() { Name="Elevated Oxford Shirt", Category="men", SubCategory="Shirts", Price=34, Stock=16, Image="https://images.unsplash.com/photo-1603252109303-2751441dd157?auto=format&fit=crop&w=800&q=80", Sizes=new(){"M","L","XL"}, Colors=new(){"White","Black"}, Description="A clean Oxford shirt that works for smart casual or formal outfits." },
            new() { Name="Signature Trench Coat", Category="women", SubCategory="Outerwear", Price=68, Stock=10, Image="https://images.unsplash.com/photo-1524504388940-b1c1722653e1?auto=format&fit=crop&w=800&q=80", Sizes=new(){"S","M","L"}, Colors=new(){"Bone","Black"}, Featured=true, Description="Statement trench with premium detailing and a strong silhouette." },
            new() { Name="Khmer Hol Phamoung", Category="men", SubCategory="Tops", Price=290, Stock=19, Image="assets/images/IMG_2350.PNG", Sizes=new(){"S","M","L","XL","XXL"}, Colors=new(){"White","Black"}, Description="Traditional Khmer fabric with modern styling." },
            new() { Name="Dark Blue Suit", Category="men", SubCategory="Tops", Price=120, Stock=19, Image="assets/images/IMG_2359.JPG", Sizes=new(){"S","M","L","XL","XXL"}, Colors=new(){"White","Black"}, Description="Elegant dark blue suit for formal occasions." }
        };
        await _mongodb.Products.InsertManyAsync(products);
    }
}