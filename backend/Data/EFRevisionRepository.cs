using Lexicon.Model;
using Microsoft.EntityFrameworkCore;

namespace Lexicon.Data;

public class EFRevisionRepository : IRevisionRepository
{
    private readonly LexiconDbContext _context;

    public EFRevisionRepository(LexiconDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Revision>> GetRevisionsAsync(int articleId)
    {
        return await _context.Revisions
            .Where(r => r.ArticleId == articleId)
            .OrderBy(r => r.VersionNumber)
            .ToListAsync();
    }
}
