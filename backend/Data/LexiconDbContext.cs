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
    public DbSet<ArticleCollaborator> ArticleCollaborators { get; set; }
    
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var seedUser = new IdentityUser
        {
            Id = "seed-user",
            UserName = "seeduser",
            NormalizedUserName = "SEEDUSER",
            Email = "seed@test.com",
            NormalizedEmail = "SEED@TEST.COM",
            EmailConfirmed = true,
            SecurityStamp = "STATIC_SECURITY_STAMP",
            ConcurrencyStamp = "STATIC_CONCURRENCY_STAMP",
            PasswordHash = "AQAAAAIAAYagAAAAEL7yNihVlaTYLNGbmmYcPSdNZ9x5oCOZRC13du/DRVrQ+QGkJpqVdLFPBol4Bf8ZDg=="
        };

        modelBuilder.Entity<IdentityUser>().HasData(seedUser);

        modelBuilder.Entity<Article>()
            .HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(a => a.AuthorId)
            .IsRequired();

        modelBuilder.Entity<Article>().HasData(
            new Article
            {
                Id = 1,
                Title = "C#",
                Content = "A modern programming language.",
                Created = new DateTime(2024, 1, 1),
                AuthorId = "seed-user"
            },
            new Article
            {
                Id = 2,
                Title = "ASP.NET Core",
                Content = "You use it to build a web application.",
                Created = new DateTime(2024, 1, 1),
                AuthorId = "seed-user"
            },
            new Article
            {
                Id = 3,
                Title = "REST API",
                Content = "Used to build RESTful APIs.",
                Created = new DateTime(2024, 1, 1),
                AuthorId = "seed-user"
            });

        modelBuilder.Entity<ArticleCollaborator>()
            .HasKey(ac => new { ac.ArticleId, ac.UserId });

        modelBuilder.Entity<ArticleCollaborator>()
            .HasOne(ac => ac.Article)
            .WithMany(a => a.Collaborators)
            .HasForeignKey(ac => ac.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ArticleCollaborator>()
            .HasOne(ac => ac.User)
            .WithMany()
            .HasForeignKey(ac => ac.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}