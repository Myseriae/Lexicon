using Lexicon.Model;
using Microsoft.EntityFrameworkCore;

namespace Lexicon.Data;

public class EFDataHandler : IDataHandler
{
    private readonly LexiconDbContext _context;

    public EFDataHandler(LexiconDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Article>> GetArticlesAsync(string? tag = null)
    {
        var query = _context.Articles
            .Include(a => a.Tags)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(a =>
                a.Tags.Any(t => EF.Functions.Like(t.Name, tag)));
        }

        return await query.ToListAsync();
    }

    public async Task<Article?> GetArticleByIdAsync(int id)
    {
        return await _context.Articles
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Article> AddArticleAsync(Article article)
    {
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        return article;
    }

    public async Task<bool> DeleteArticleAsync(int id)
    {
        var article = await _context.Articles
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article == null) return false;

        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateArticleAsync(int id, Article article, string previousContent, string? summary = null)
    {
        var existing = await _context.Articles.FindAsync(id);
        if (existing == null) return false;

        var revisionCount = await _context.Revisions
            .CountAsync(r => r.ArticleId == id);

        var revision = new Revision
        {
            ArticleId = id,
            Content = previousContent,
            Summary = summary,
            VersionNumber = revisionCount + 1,
            SavedAt = DateTime.UtcNow
        };

        _context.Revisions.Add(revision);

        existing.Title = article.Title;
        existing.Content = article.Content;
        existing.Summary = article.Summary;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Revision>> GetRevisionsAsync(int articleId)
    {
        return await _context.Revisions
            .Where(r => r.ArticleId == articleId)
            .OrderBy(r => r.VersionNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<Article>> SearchArticlesAsync(string query)
    {
        var pattern = $"%{query}%";

        return await _context.Articles
            .Include(a => a.Tags)
            .Where(a =>
                EF.Functions.Like(a.Title, pattern) ||
                EF.Functions.Like(a.Content, pattern) ||
                a.Tags.Any(t => EF.Functions.Like(t.Name, pattern)))
            .ToListAsync();
    }

    public async Task<IEnumerable<ArticleCollaborator>> GetCollaboratorsAsync(int articleId) =>
        await _context.ArticleCollaborators
            .Where(ac => ac.ArticleId == articleId)
            .Include(ac => ac.User)
            .ToListAsync();

    public async Task<bool> AddCollaboratorAsync(int articleId, string userId)
    {
        if (await _context.ArticleCollaborators.AnyAsync(ac => ac.ArticleId == articleId && ac.UserId == userId))
            return false; // Already a collaborator

        var collaborator = new ArticleCollaborator { ArticleId = articleId, UserId = userId };
        _context.ArticleCollaborators.Add(collaborator);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveCollaboratorAsync(int articleId, string userId)
    {
        var collaborator = await _context.ArticleCollaborators
            .FirstOrDefaultAsync(ac => ac.ArticleId == articleId && ac.UserId == userId);
        if (collaborator == null) return false;

        _context.ArticleCollaborators.Remove(collaborator);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsCollaboratorAsync(int articleId, string userId) =>
        await _context.ArticleCollaborators
            .AnyAsync(ac => ac.ArticleId == articleId && ac.UserId == userId);
}
