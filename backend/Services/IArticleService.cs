using Lexicon.DTOs;

namespace Lexicon.Services;

public interface IArticleService
{
    Task<IEnumerable<ArticleResponse>> GetArticlesAsync(string? tag = null);
    Task<ArticleResponse?> GetArticleByIdAsync(int id);
    Task<ArticleResponse> AddArticleAsync(CreateArticleRequest request, string authorId);
    Task<bool> DeleteArticleAsync(int id, string userId, bool isAdmin);
    Task<bool> UpdateArticleAsync(int id, UpdateArticleRequest request, string userId, bool isAdmin);
    Task<IEnumerable<ArticleResponse>> SearchAsync(string query);
    Task<IEnumerable<RevisionResponse>> GetRevisionsAsync(int articleId);
    Task<IEnumerable<CollaboratorResponse>> GetCollaboratorsAsync(int articleId);
    Task<bool> AddCollaboratorAsync(int articleId, string userId);
    Task<bool> AddCollaboratorByUsernameAsync(int articleId, string username);
    Task<bool> RemoveCollaboratorAsync(int articleId, string userId);
    Task<bool> IsCollaboratorAsync(int articleId, string userId);
}
