using Lexicon.Data;
using Lexicon.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Lexicon.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register your IDataHandler service
builder.Services.AddScoped<IDataHandler, EFDataHandler>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthSeeder>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddHttpClient<IWikipediaService, WikipediaService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "LexiconApp/1.0 (educational project)");
});

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// ✅ CORS (future-proof for JWT/cookies later)
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173/",  // Vite dev server
                "http://localhost:80")    // Docker nginx
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();          // Required for httpOnly cookie to be sent cross-origin
    });
});

// sql
builder.Services.AddDbContext<LexiconDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

AddAuthentication();
AddIdentity();

var app = builder.Build();

// Apply migrations on startup with retry logic
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LexiconDbContext>();
    var retries = 3;
    while (retries-- > 0)
    {
        try
        {
            await db.Database.MigrateAsync();
            Console.WriteLine("Migrations applied successfully.");
            break;
        }
        catch
        {
            if (retries == 0) throw;
            Console.WriteLine("Waiting for database, retrying...");
            await Task.Delay(3000);
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}


app.UseCors("FrontendDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

void AddAuthentication()
{
    var validIssuer = builder.Configuration["Jwt:ValidIssuer"]
                      ?? throw new InvalidOperationException("Jwt:ValidIssuer is missing.");
    var validAudience = builder.Configuration["Jwt:ValidAudience"]
                        ?? throw new InvalidOperationException("Jwt:ValidAudience is missing.");
    var issuerSigningKey = builder.Configuration["Jwt:IssuerSigningKey"]
                           ?? throw new InvalidOperationException(
                               "Jwt:IssuerSigningKey is missing. " +
                               "Set it via: dotnet user-secrets set Jwt:IssuerSigningKey" + 
                               "<secret>");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ClockSkew                = TimeSpan.Zero,
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = validIssuer,
                ValidAudience            = validAudience,
                IssuerSigningKey         = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(issuerSigningKey))
            };
        });
}

void AddIdentity()
{
    builder.Services
        .AddIdentityCore<IdentityUser>(options =>
        {
            options.Password.RequireDigit           = false;
            options.Password.RequiredLength         = 6;
            options.Password.RequireLowercase       = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase       = false;
            options.User.RequireUniqueEmail         = true;
        })
        .AddRoles<IdentityRole>()               // must come before AddEntityFrameworkStores
        .AddEntityFrameworkStores<LexiconDbContext>();
}
