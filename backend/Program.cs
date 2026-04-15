using Lexicon.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register your IDataHandler service
builder.Services.AddSingleton<IDataHandler, InMemoryData>();

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// ✅ CORS (future-proof for JWT/cookies later)
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
        // Do NOT add AllowCredentials yet (keep it simple for now)
    });
});

var app = builder.Build();

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