using MongoDB.Driver;
using OroFashionAPI.Models;
using OroFashionAPI.DTOs;
using OroFashionAPI.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OroFashionAPI.Services;

public class AuthService
{
    //this is DI use for access database and apply Jwt for secure because can't create database by itself
    private readonly MongoDBService _mongodb;
    private readonly JwtSettings __jwtSetting;
    
    //This constructor is telling ASP.NET: I need database + JWT settings to work, please provide them
    //MongoDBService mongodb use to give access to database use for insert find login update ...
    //IOptions<JwtSettings> jwtSettings use to secure database and we can also write as
    //JwtSetting jwtSetting but use IOption<> better because it config from appsetting.json
    public AuthService(MongoDBService mongodb, IOptions<JwtSettings> jwtSettings)
    {
        _mongodb = mongodb;
        __jwtSetting = jwtSettings.Value;
    }

    //async is the method that can run without blocking the program
    //await is wait for this database operation to finish, but don't freeze the whole program
    //SeedAdminAsync is method that create default admin user when system start
    //.AnyAsync is check database that is there already admin?
    //exist is something present or available in the database
    public async Task SeedAdminAsync()
    {
        //check if any admin exists
        var adminExists = await _mongodb.Users
        .Find(u => u.Role == "Admin")
        .AnyAsync();

        if (!adminExists)
        {
            var admin = new User
            {
                FullName = "ORO Admin",
                Email = "admin@orofashion.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 12),
                Phone = "069973981",
                Address = "Street 278, Sangkat olympic, Khan Boeng Keng Kang, Phnom Penh",
                Role = "Admin"
            };
            await _mongodb.Users.InsertOneAsync(admin);
        }
    }
    public async Task<(bool success, string message, AuthResponseDTO? data)>
    RegisterAsync(RegisterDTO dto)
    {
        //check Email not already used
        // .ToLower().Trim() = normalize email for comparison
        var exists = await _mongodb.Users   
        .Find(u => u.Email == dto.Email.ToLower().Trim())
        .AnyAsync();

        if(exists)
        return (false, "Email already exists.", null);

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
            Phone = dto.Phone,
            Address = dto.Address,
            Role = "Customer"
        };
        //save to mongodb
        await _mongodb.Users.InsertOneAsync(user);

        //auto login after register
        var token = GenerateJwtToken(user);
        var response = BuildAuthResponse(user, token);

        return (true, "Account created successfully.", response);
    }

    public async Task<(bool success, string message, AuthResponseDTO? data)>
    LoginAsync(LoginDTO dto)
    {
        //find email
        var user = await _mongodb.Users
        .Find(u => u.Email == dto.Email.ToLower().Trim() && !u.IsDeleted)
        .FirstOrDefaultAsync();

        if(user == null)
            return (false, "Invalid email or password", null);
        

        // verify password against stored hash
        // BCrypt.Verify = re-hashes and compares — cannot reverse hash
        var valid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if(!valid)
            return (false, "Invalid email or password", null);

        var update = Builders<User>.Update
        .Set(u => u.LastLoginAt, DateTime.UtcNow);
        await _mongodb.Users.UpdateOneAsync(u => u.Id == user.Id, update);

        //generate token and build response
        var token = GenerateJwtToken(user);
        var response = BuildAuthResponse(user, token);

        return (true, "Welcome Back", response);
    }
    public async Task<bool> UpdateProfileAsync(string UserId, UpdateProfileDTO dto)
    {
        // Builders<User>.Update = creates an update operation
        // .Set(u => u.FullName, dto.FullName) = set this field to this value
        // chaining .Set() = update multiple fields in one DB call
        var update = Builders<User>.Update
        .Set(u => u.FullName, dto.FullName.Trim())
        .Set(u => u.Phone, dto.Phone)
        .Set(u => u.Address, dto.Address);

        var result = await _mongodb.Users.UpdateOneAsync(
            u => u.Id == UserId, update
        );
        // ModifiedCount = how many documents were changed
        return result.ModifiedCount > 0;
    }
    public async Task<List<User>> GetAllUsersAsync()
    {
        // get all users for admin dashboard (customers only)
        return await _mongodb.Users
            .Find(u => !u.IsDeleted)
            .ToListAsync();
    }
    // generates the JWT token string that Angular stores in localStorage
    private string GenerateJwtToken(User user)
    {
        // create signing key from secret string
        // Encoding.UTF8.GetBytes = convert string to byte array (JWT needs bytes)
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(__jwtSetting.SecretKey)
        );
        // SigningCredentials = key + algorithm used to sign token
        // HmacSha256 = industry standard for JWT — very secure
        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256
        );
        // claims = data embedded inside the JWT token
        // Angular can read these without calling the database
        //claim mean put inside token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        //build the complete token object
        var token = new JwtSecurityToken(
            issuer: __jwtSetting.Issuer, //OroFashionAPI
            audience: __jwtSetting.Audience, //OroFashionClient
            claims: claims,
            // token expires 7 days from now
            expires: DateTime.UtcNow.AddDays(__jwtSetting.ExpiryInDays),
            signingCredentials: credentials
        );
        // WriteToken = converts token object to string "eyJhbGciOiJIUzI1NiIs..."
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    // builds the response DTO — safe data only (no password hash)
    private AuthResponseDTO BuildAuthResponse(User user, string token)
    {
        return new AuthResponseDTO
        {
            Token = token,
            UserId = user.Id!,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Address,
            Address = user.Address,
            Role = user.Role,
        };
    }
}