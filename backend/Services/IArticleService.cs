using Lexicon.Model;

namespace Lexicon.Services;

public interface IArticleService
{
    IEnumerable<Article> GetArticles();
    Article? GetArticleById(int id);
    Task<Article> AddArticleAsync(Article article);
    bool DeleteArticle(int id);
    bool UpdateArticle(int id, Article article);
    IEnumerable<Article> Search(string query);
}
