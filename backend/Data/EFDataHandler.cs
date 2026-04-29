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

    public async Task<bool> UpdateArticleAsync(int id, Article article)
    {
        var existing = await GetArticleByIdAsync(id);
        if (existing == null) return false;

        existing.Title = article.Title;
        existing.Content = article.Content;
        existing.Summary = article.Summary;
        await _context.SaveChangesAsync();
        return true;
    }
}
