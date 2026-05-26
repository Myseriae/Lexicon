using System.Text;
using Lexicon.Data;
using Lexicon.Services;
using Lexicon.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// --- Application services ---
builder.Services.AddScoped<IArticleRepository, EFArticleRepository>();
builder.Services.AddScoped<IRevisionRepository, EFRevisionRepository>();
builder.Services.AddScoped<ITagRepository, EFTagRepository>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddHttpClient<IWikipediaService, WikipediaService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("LexiconApp/1.0 (educational project)");
});

// --- Auth services ---
// Scoped because UserManager (their dependency) is Scoped.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<AuthSeeder>();

// --- OpenAPI / Swagger ---
builder.Services.AddOpenApi();

// --- CORS (credentials enabled for httpOnly refresh token cookie) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:80")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();  // Required for the httpOnly cookie to work cross-origin
    });
});

// --- Database ---
builder.Services.AddDbContext<LexiconDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// --- JWT Authentication ---
AddAuthentication();

// --- ASP.NET Identity ---
AddIdentity();

// ===========================================================================
var app = builder.Build();
// ===========================================================================

// Apply migrations and seed roles on startup.
using (var scope = app.Services.CreateScope())
{
    var db      = scope.ServiceProvider.GetRequiredService<LexiconDbContext>();
    var seeder  = scope.ServiceProvider.GetRequiredService<AuthSeeder>();
    var retries = 3;
    while (retries-- > 0)
    {
        try
        {
            await db.Database.MigrateAsync();
            Console.WriteLine("Migrations applied successfully.");
            await seeder.SeedRolesAsync();
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

if (app.Environment.IsDevelopment())
    app.MapOpenApi();
else
    app.UseHttpsRedirection();

app.UseCors("FrontendDev");

app.UseAuthentication();   // ← must come before UseAuthorization
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

// ---------------------------------------------------------------------------
// Local functions for service registration
// ---------------------------------------------------------------------------

void AddAuthentication()
{
    var validIssuer = builder.Configuration["Jwt:ValidIssuer"]
                      ?? throw new InvalidOperationException("Jwt:ValidIssuer is missing.");
    var validAudience = builder.Configuration["Jwt:ValidAudience"]
                        ?? throw new InvalidOperationException("Jwt:ValidAudience is missing.");
    var issuerSigningKey = builder.Configuration["Jwt:IssuerSigningKey"]
                           ?? throw new InvalidOperationException(
                               "Jwt:IssuerSigningKey is missing. " +
                               "Set it via: dotnet user-secrets set \"Jwt:IssuerSigningKey\" \"<secret>\"");

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
