using Lexicon.Model;

namespace Lexicon.Data;

public interface IDataHandler
{
        Task<IEnumerable<Article>> GetArticlesAsync();
        Task<Article?> GetArticleByIdAsync(int id);
        Task<Article> AddArticleAsync(Article article);
        Task<bool> DeleteArticleAsync(int id);
        Task<bool> UpdateArticleAsync(int id, Article article, string previousContent, string? summary = null);
        Task<IEnumerable<Article>> SearchArticlesAsync(string query);
        Task<IEnumerable<Revision>> GetRevisionsAsync(int articleId);
        Task<IEnumerable<ArticleCollaborator>> GetCollaboratorsAsync(int articleId);
        Task<bool> AddCollaboratorAsync(int articleId, string userId);
        Task<bool> RemoveCollaboratorAsync(int articleId, string userId);
        Task<bool> IsCollaboratorAsync(int articleId, string userId);
}