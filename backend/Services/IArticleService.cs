using Lexicon.DTOs;

namespace Lexicon.Services;

public interface IArticleService
{
    IEnumerable<ArticleResponse> GetArticles();
    ArticleResponse? GetArticleById(int id);
    Task<ArticleResponse> AddArticleAsync(CreateArticleRequest request, string authorId);
    Task<bool> DeleteArticleAsync(int id);
    Task<bool> UpdateArticleAsync(int id, UpdateArticleRequest request);
    IEnumerable<ArticleResponse> Search(string query);
    Task<IEnumerable<RevisionResponse>> GetRevisionsAsync(int articleId);
    Task<IEnumerable<CollaboratorResponse>> GetCollaboratorsAsync(int articleId);
    Task<bool> AddCollaboratorAsync(int articleId, string userId);
    Task<bool> RemoveCollaboratorAsync(int articleId, string userId);
}
