using Lexicon.DTOs;

namespace Lexicon.Services;

public interface ITagService
{
    Task<IEnumerable<TagResponse>> GetAllAsync();
    Task<bool> AddTagToArticleAsync(int articleId, string tagName);
    Task<bool> RemoveTagFromArticleAsync(int articleId, int tagId);
}
