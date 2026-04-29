using Lexicon.Model;

namespace Lexicon.Data;

public interface IDataHandler
{
        Task<IEnumerable<Article>> GetArticlesAsync();
        Task<Article?> GetArticleByIdAsync(int id);
        Task<Article> AddArticleAsync(Article article);
        Task<bool> DeleteArticleAsync(int id);
        Task<bool> UpdateArticleAsync(int id, Article article);
}