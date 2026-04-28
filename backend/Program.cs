using Lexicon.Data;
using Lexicon.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register your IDataHandler service
builder.Services.AddScoped<IDataHandler, EFDataHandler>();
builder.Services.AddScoped<IArticleService, ArticleService>();
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
                "http://localhost:5173",
                "http://localhost:80",
                "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
        // Do NOT add AllowCredentials yet (keep it simple for now)
    });
});

// sql
builder.Services.AddDbContext<LexiconDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Apply migrations on startup with retry logic
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LexiconDbContext>();
    var retries = 10;
    while (retries-- > 0)
    {
        try
        {
            db.Database.Migrate();
            Console.WriteLine("Migrations applied successfully.");
            break;
        }
        catch
        {
            if (retries == 0)
            {
                Console.WriteLine("Failed to apply migrations, retrying...");
                throw;
            }
            Console.WriteLine("Waiting 3 seconds to retry...");
            Thread.Sleep(3000);
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ⚠️ IMPORTANT: CORS must be before auth + controllers
app.UseHttpsRedirection();

app.UseCors("FrontendDev");

app.UseAuthorization();

app.MapControllers();

app.Run();