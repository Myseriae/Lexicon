using Lexicon.Model;
using Microsoft.EntityFrameworkCore;

namespace Lexicon.Data;

public class EFTagRepository : ITagRepository
{
    private readonly LexiconDbContext _context;

    public EFTagRepository(LexiconDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        return await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Tag> GetOrCreateAsync(string name)
    {
        var cleanName = name.Trim();

        var existing = await _context.Tags
            .FirstOrDefaultAsync(t => t.Name == cleanName);

        if (existing != null)
            return existing;

        var tag = new Tag { Name = cleanName };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return tag;
    }

    public async Task<bool> AddTagToArticleAsync(int articleId, string tagName)
    {
        var article = await _context.Articles
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            return false;

        var tag = await GetOrCreateAsync(tagName);

        if (!article.Tags.Any(t => t.Id == tag.Id))
            article.Tags.Add(tag);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveTagFromArticleAsync(int articleId, int tagId)
    {
        var article = await _context.Articles
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == articleId);

        if (article == null)
            return false;

        var tag = article.Tags.FirstOrDefault(t => t.Id == tagId);

        if (tag == null)
            return false;

        article.Tags.Remove(tag);
        await _context.SaveChangesAsync();

        return true;
    }
}