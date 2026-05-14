using Microsoft.EntityFrameworkCore;
using Lexicon.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Lexicon.Data;

public class LexiconDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public LexiconDbContext(DbContextOptions<LexiconDbContext> options)
        : base(options)
    {
    }

    public DbSet<Article> Articles { get; set; }
    public DbSet<Revision> Revisions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tag>()
            .Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();

        modelBuilder.Entity<Article>()
            .HasMany(a => a.Tags)
            .WithMany(t => t.Articles)
            .UsingEntity<Dictionary<string, object>>(
                "ArticleTags",
                j => j.HasOne<Tag>()
                    .WithMany()
                    .HasForeignKey("TagId")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<Article>()
                    .WithMany()
                    .HasForeignKey("ArticleId")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("ArticleId", "TagId");
                    j.ToTable("ArticleTags");
                });

        modelBuilder.Entity<Article>()
            .HasData(
                new Article { Id = 1, Title = "C#", Content = "A modern programming language.", Created = new DateTime(2024, 1, 1) },
                new Article { Id = 2, Title = "ASP.NET Core", Content = "You use it to build a web application.", Created = new DateTime(2024, 1, 1) },
                new Article { Id = 3, Title = "REST API", Content = "Used to build RESTful APIs.", Created = new DateTime(2024, 1, 1) }
            );
    }
}