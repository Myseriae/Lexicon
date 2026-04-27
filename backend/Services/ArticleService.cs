using Lexicon.Data;
using Lexicon.Model;

namespace Lexicon.Services;

public class ArticleService : IArticleService
{
    private readonly IDataHandler _dataHandler;

    public ArticleService(IDataHandler dataHandler)
    {
        _dataHandler = dataHandler;
    }

    public IEnumerable<Article> GetArticles() => _dataHandler.GetArticles();

    public Article? GetArticleById(int id) => _dataHandler.GetArticleById(id);

    public Article AddArticle(Article article) => _dataHandler.AddArticle(article);

    public bool DeleteArticle(int id) => _dataHandler.DeleteArticle(id);

    public bool UpdateArticle(int id, Article article) => _dataHandler.UpdateArticle(id, article);

    public IEnumerable<Article> Search(string query)
        => _dataHandler.GetArticles()
            .Where(a => a.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
}
