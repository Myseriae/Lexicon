using Lexicon.Model;

namespace Lexicon.Data;

public interface IRevisionRepository
{
    Task<IEnumerable<Revision>> GetRevisionsAsync(int articleId);
    Task<Revision?> GetRevisionAsync(int articleId, int revisionId);
    Task AddRevisionAsync(Revision revision);
}
