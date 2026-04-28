using Lexicon.Data;
using Lexicon.Model;

namespace Lexicon.Services;

public class ArticleService : IArticleService
{
    private readonly IDataHandler _dataHandler;
    private readonly IWikipediaService _wikipediaService;

    public ArticleService(IDataHandler dataHandler, IWikipediaService wikipediaService)
    {
        _dataHandler = dataHandler;
        _wikipediaService = wikipediaService;
    }

    public IEnumerable<Article> GetArticles() => _dataHandler.GetArticles();

    public Article? GetArticleById(int id) => _dataHandler.GetArticleById(id);

    public async Task<Article> AddArticleAsync(Article article)
    {
        // Only fetch summary if none exists
        if (string.IsNullOrWhiteSpace(article.Summary))
        {
            var summary = await _wikipediaService.GetSummaryAsync(article.Title);

            if (!string.IsNullOrWhiteSpace(summary))
            {
                article.Summary = summary;
            }
        }

        return _dataHandler.AddArticle(article);
    }

    public bool DeleteArticle(int id) => _dataHandler.DeleteArticle(id);

    public bool UpdateArticle(int id, Article article) => _dataHandler.UpdateArticle(id, article);

    public IEnumerable<Article> Search(string query)
        => _dataHandler.GetArticles()
            .Where(a => a.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
}
