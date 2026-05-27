using Lexicon.Model;

namespace Lexicon.Data;

public interface IRevisionRepository
{
    Task<IEnumerable<Revision>> GetRevisionsAsync(int articleId);
    Task AddRevisionAsync(Revision revision);
}
