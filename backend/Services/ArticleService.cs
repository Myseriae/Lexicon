using Lexicon.Data;
using Lexicon.DTOs;
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

    private static ArticleResponse ToResponse(Article article) => new ArticleResponse
    {
        Id = article.Id,
        Title = article.Title,
        Content = article.Content,
        Summary = article.Summary,
        Created = article.Created
    };

    public IEnumerable<ArticleResponse> GetArticles()
        => _dataHandler.GetArticlesAsync().GetAwaiter().GetResult().Select(ToResponse);

    public ArticleResponse? GetArticleById(int id)
    {
        var article = _dataHandler.GetArticleByIdAsync(id).GetAwaiter().GetResult();
        return article == null ? null : ToResponse(article);
    }

    public async Task<ArticleResponse> AddArticleAsync(CreateArticleRequest request)
    {
        var article = new Article
        {
            Title = request.Title,
            Content = request.Content,
            Summary = request.Summary
        };

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
            var saved = await _dataHandler.AddArticleAsync(article);
            return ToResponse(saved);
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

    public async Task<bool> UpdateArticleAsync(int id, UpdateArticleRequest request)
    {
        var article = new Article
        {
            Title = request.Title,
            Content = request.Content
        };

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

    public IEnumerable<ArticleResponse> Search(string query)
        => _dataHandler.GetArticlesAsync().GetAwaiter().GetResult()
            .Where(a => a.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Select(ToResponse);
}
