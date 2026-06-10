using System.Data.Common;
using Lexicon.Data;
using Lexicon.Services;
using Lexicon.Services.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LexiconTest;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:ValidIssuer"] = "lexicon-test-issuer",
                ["Jwt:ValidAudience"] = "lexicon-test-audience",
                ["Jwt:IssuerSigningKey"] = "lexicon-test-signing-key-with-32-chars",
                ["Jwt:AccessTokenExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"] = "7",
                ["SA_PASSWORD"] = "DummyPassword123!"  // Dummy value for testing (not used, SQLite is used instead)
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<LexiconDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<LexiconDbContext>>();
            services.RemoveAll<LexiconDbContext>();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddSingleton(_connection);
            services.AddDbContext<LexiconDbContext>(options => options.UseSqlite(_connection));

            services.AddSingleton<IWikipediaService, StubWikipediaService>();
        });
    }

    /// <summary>
    /// Resets the database to a clean state by dropping and recreating all tables, then re-seeds roles.
    /// Call this between tests to ensure isolation without recreating the host.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LexiconDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<AuthSeeder>();
        
        // Drop all tables
        await context.Database.EnsureDeletedAsync();
        
        // Recreate schema
        await context.Database.EnsureCreatedAsync();
        
        // Re-seed roles
        await seeder.SeedRolesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection?.Dispose();
            _connection = null;
        }
    }
}
