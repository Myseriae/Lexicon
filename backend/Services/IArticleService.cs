using Lexicon.Model;

namespace Lexicon.Services;

public interface IArticleService
{
    IEnumerable<Article> GetArticles();
    Article? GetArticleById(int id);
    Task<Article> AddArticleAsync(Article article);
    Task<bool> DeleteArticleAsync(int id);
    Task<bool> UpdateArticleAsync(int id, Article article);
    IEnumerable<Article> Search(string query);
}
