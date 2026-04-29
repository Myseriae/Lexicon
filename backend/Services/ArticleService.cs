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

    public IEnumerable<Article> GetArticles() => _dataHandler.GetArticlesAsync().GetAwaiter().GetResult();

    public Article? GetArticleById(int id) => _dataHandler.GetArticleByIdAsync(id).GetAwaiter().GetResult();

    public async Task<Article> AddArticleAsync(Article article)
    {
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
            return await _dataHandler.AddArticleAsync(article);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteArticleAsync(int id)
    {
        try
        {
            return await _dataHandler.DeleteArticleAsync(id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<bool> UpdateArticleAsync(int id, Article article)
    {
        try
        {
            return await _dataHandler.UpdateArticleAsync(id, article);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public IEnumerable<Article> Search(string query)
        => _dataHandler.GetArticlesAsync().GetAwaiter().GetResult()
            .Where(a => a.Title.Contains(query, StringComparison.OrdinalIgnoreCase));
}
