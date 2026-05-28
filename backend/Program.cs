using System.Text;
using Lexicon.Data;
using Lexicon.Services;
using Lexicon.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// Load .env (for local dev without Docker). Does not override vars already set by the OS/Docker.
DotNetEnv.Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// --- Application services ---
builder.Services.AddScoped<IArticleRepository, EFArticleRepository>();
builder.Services.AddScoped<IRevisionRepository, EFRevisionRepository>();
builder.Services.AddScoped<ITagRepository, EFTagRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, EFRefreshTokenRepository>();
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
// Substitute {SA_PASSWORD} placeholder in the connection string with the value from
// the environment (set by .env locally or by Docker Compose in production).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is missing.");
var saPassword = builder.Configuration["SA_PASSWORD"]
                 ?? throw new InvalidOperationException(
                     "SA_PASSWORD is missing. Add it to .env or set it as an environment variable.");
connectionString = connectionString.Replace("{SA_PASSWORD}", saPassword);

builder.Services.AddDbContext<LexiconDbContext>(options =>
    options.UseSqlServer(connectionString));

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

    if (app.Environment.IsEnvironment("Testing"))
    {
        await db.Database.EnsureCreatedAsync();
        await seeder.SeedRolesAsync();
    }
    else
    {
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
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
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
    var isTesting = builder.Environment.IsEnvironment("Testing");
    var validIssuer = isTesting
                      ? "lexicon-test-issuer"
                      : builder.Configuration["Jwt:ValidIssuer"]
                      ?? throw new InvalidOperationException("Jwt:ValidIssuer is missing.");
    var validAudience = isTesting
                        ? "lexicon-test-audience"
                        : builder.Configuration["Jwt:ValidAudience"]
                        ?? throw new InvalidOperationException("Jwt:ValidAudience is missing.");
    var issuerSigningKey = isTesting
                           ? "lexicon-test-signing-key-with-32-chars"
                           : builder.Configuration["Jwt:IssuerSigningKey"]
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

public partial class Program;
