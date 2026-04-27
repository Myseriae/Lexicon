using Microsoft.EntityFrameworkCore;
using Lexicon.Model;

namespace Lexicon.Data;

public class LexiconDbContext : DbContext
{
    public LexiconDbContext(DbContextOptions<LexiconDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Article> Articles { get; set; }
    public DbSet<Revision> Revisions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>()
            .HasData(new Article { Id = 1, Title = "C#", Content = "A modern programming language." },
                new Article { Id = 2, Title = "ASP.NET Core", Content = "You use it to build a web application." },
                new Article { Id = 3, Title = "REST API", Content = "Used to build RESTful APIs." });
    }
}