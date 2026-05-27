using Lexicon.Model;

namespace Lexicon.Data;

public interface IArticleRepository
{
    Task<IEnumerable<Article>> GetArticlesAsync(string? tag = null);
    Task<Article?> GetArticleByIdAsync(int id);
    Task<Article> AddArticleAsync(Article article);
    Task<bool> DeleteArticleAsync(int id);
    Task<bool> UpdateArticleAsync(int id, Article article);
    Task<IEnumerable<Article>> SearchArticlesAsync(string query);
    Task<IEnumerable<ArticleCollaborator>> GetCollaboratorsAsync(int articleId);
    Task<bool> AddCollaboratorAsync(int articleId, string userId);
    Task<bool> RemoveCollaboratorAsync(int articleId, string userId);
    Task<bool> IsCollaboratorAsync(int articleId, string userId);
}
