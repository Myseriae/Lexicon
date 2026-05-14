using Lexicon.Model;

namespace Lexicon.Data;

public interface ITagRepository
{
    Task<IEnumerable<Tag>> GetAllAsync();
    Task<Tag> GetOrCreateAsync(string name);
    Task<bool> AddTagToArticleAsync(int articleId, string tagName);
    Task<bool> RemoveTagFromArticleAsync(int articleId, int tagId);
}