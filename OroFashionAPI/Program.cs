using OroFashionAPI.Settings;
using OroFashionAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using OroFashionAPI.Setting;

// WebApplication.CreateBuilder = creates app builder
// everything below = BUILD phase (configure before start)
var builder = WebApplication.CreateBuilder(args);

// ══════════════════════════════════════════
// STEP 1: Read Configuration from appsettings.json
// ══════════════════════════════════════════

// Configure<T> = read section from appsettings.json → make injectable
// builder.Configuration = reads appsettings.json automatically
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// ══════════════════════════════════════════
// STEP 2: Register Services (Dependency Injection)
// ══════════════════════════════════════════

// AddSingleton = one instance for entire app (good for DB connections)
builder.Services.AddSingleton<MongoDBService>();

// AddScoped = new instance per HTTP request (good for business logic)
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductServices>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<FavoriteServices>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<NewsletterService>();

// ══════════════════════════════════════════
// STEP 3: JWT Authentication
// ══════════════════════════════════════════

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings").Get<JwtSettings>()!;

builder.Services.AddAuthentication(options =>
{
    // use JWT Bearer as default auth scheme
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    // return 401 when auth fails
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // check token came from our server
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        // check token is for our app
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        // check token not expired
        ValidateLifetime = true,
        // MOST IMPORTANT — verify signature with secret key
        // prevents token tampering (e.g. changing role to "admin")
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

// ══════════════════════════════════════════
// STEP 4: CORS — Allow Angular to Call This API
// ══════════════════════════════════════════

// CORS = browser blocks requests between different origins by default
// Angular: http://localhost:4200 → Backend: http://localhost:PORT
// this policy allows Angular's origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "http://localhost:4201",
                "http://localhost:4202"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ══════════════════════════════════════════
// STEP 5: Swagger — API Documentation UI
// ══════════════════════════════════════════

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ORO Fashion API",
        Version = "v1",
        Description = "Backend API for ORO Fashion e-commerce store"
    });

    // add JWT auth to Swagger UI (the Authorize button)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token here}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ══════════════════════════════════════════
// BUILD the app — no more service registration after this
// ══════════════════════════════════════════
var app = builder.Build();
// global error handler — always return JSON even on unhandled exceptions
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(
            "{\"message\": \"An internal server error occurred.\"}");
    });
});

// ══════════════════════════════════════════
// SEED DATA on startup
// ══════════════════════════════════════════

// seed admin account and products when app starts
// uses using var scope = ... to get services from DI container
using (var scope = app.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
    var productService = scope.ServiceProvider.GetRequiredService<ProductServices>();

    // create admin@orofashion.com / admin123 if not exists
    await authService.SeedAdminAsync();
    // seed initial products if collection is empty
    await productService.SeedProductAsync();
}

// ══════════════════════════════════════════
// MIDDLEWARE PIPELINE — ORDER MATTERS!
// ══════════════════════════════════════════

// show Swagger only in development mode
if (app.Environment.IsDevelopment())
{
    // /swagger/v1/swagger.json = raw API description
    app.UseSwagger();
    // /swagger = interactive documentation UI
    app.UseSwaggerUI();
}

// redirect http → https (security)
//app.UseHttpsRedirection();

// CORS must be BEFORE auth — Angular needs CORS headers on 401 responses too
app.UseCors("AllowAngular");

// read JWT token from Authorization header and validate it
// sets User (ClaimsPrincipal) on request if valid
// MUST be before UseAuthorization
app.UseAuthentication();

// check [Authorize] attributes — reject if not authenticated/authorized
// MUST be after UseAuthentication (needs User to be set first)
app.UseAuthorization();

// route requests to correct controller method
app.MapControllers();
app.UseDefaultFiles(); // serve index.html for Angular routes (SPA fallback)
app.UseStaticFiles(); // serve static files (Angular build output)
app.MapFallbackToFile("index.html"); // SPA fallback for Angular routes

// START — listen for HTTP requests (blocking call)
app.Run();