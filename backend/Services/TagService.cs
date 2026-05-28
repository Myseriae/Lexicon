using Lexicon.Data;
using Lexicon.DTOs;

namespace Lexicon.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _repo;

    public TagService(ITagRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<TagResponse>> GetAllAsync()
    {
        var tags = await _repo.GetAllAsync();
        return tags.Select(t => new TagResponse { Id = t.Id, Name = t.Name });
    }

    public Task<bool> AddTagToArticleAsync(int articleId, string tagName)
        => _repo.AddTagToArticleAsync(articleId, tagName);

    public Task<bool> RemoveTagFromArticleAsync(int articleId, int tagId)
        => _repo.RemoveTagFromArticleAsync(articleId, tagId);
}
