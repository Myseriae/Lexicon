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

        try
        {
            return _dataHandler.AddArticle(article);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public bool DeleteArticle(int id)
    {
        try
        {
            return _dataHandler.DeleteArticle(id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public bool UpdateArticle(int id, Article article)
    {
        try
        {
            return _dataHandler.UpdateArticle(id, article);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public IEnumerable<Article> Search(string query)
        => _dataHandler.GetArticles()
            .Where(a => a.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
}
