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

    public async Task<IEnumerable<Article>> GetArticlesAsync() => await _context.Articles.ToListAsync();

    public async Task<Article?> GetArticleByIdAsync(int id) => await _context.Articles.FindAsync(id);

    public async Task<Article> AddArticleAsync(Article article)
    {
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();
        return article;
    }

    public async Task<bool> DeleteArticleAsync(int id)
    {
        var article = await GetArticleByIdAsync(id);
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

        var Revision = new Revision
        {
            ArticleId = id,
            Content = previousContent,
            Summary = summary,
            VersionNumber = revisionCount + 1,
            SavedAt = DateTime.UtcNow
        };

        _context.Revisions.Add(Revision);

        existing.Title = article.Title;
        existing.Content = article.Content;
        existing.Summary = article.Summary;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Revision>> GetRevisionsAsync(int articleId) =>
        await _context.Revisions
            .Where(r => r.ArticleId == articleId)
            .OrderBy(r => r.VersionNumber)
            .ToListAsync();

    public async Task<IEnumerable<Article>> SearchArticlesAsync(string query) =>
        await _context.Articles
            .Where(a => a.Title.ToLower().Contains(query.ToLower()))
            .ToListAsync();
}
