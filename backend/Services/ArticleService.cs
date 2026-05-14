using Lexicon.Data;
using Lexicon.DTOs;
using Lexicon.Model;
using Microsoft.Extensions.Logging;
using Lexicon.Services.Auth;

namespace Lexicon.Services;

public class ArticleService : IArticleService
{
    private readonly IDataHandler _dataHandler;
    private readonly IWikipediaService _wikipediaService;
    private readonly IAuthService _authService;
    private readonly ILogger<ArticleService> _logger;

    public ArticleService(IDataHandler dataHandler, IWikipediaService wikipediaService, IAuthService authService, ILogger<ArticleService> logger)
    {
        _dataHandler = dataHandler;
        _wikipediaService = wikipediaService;
        _authService = authService;
        _logger = logger;
    }

    private static ArticleResponse ToResponse(Article article) => new ArticleResponse
    {
        Id = article.Id,
        AuthorId = article.AuthorId,
        Title = article.Title,
        Content = article.Content,
        Summary = article.Summary,
        Created = article.Created
    };

    private async Task<ArticleResponse> ToResponseWithCollaboratorsAsync(Article article)
    {
        var collaborators = await _dataHandler.GetCollaboratorsAsync(article.Id);
        return new ArticleResponse
        {
            Id = article.Id,
            AuthorId = article.AuthorId,
            Title = article.Title,
            Content = article.Content,
            Summary = article.Summary,
            Created = article.Created,
            CollaboratorIds = collaborators.Select(c => c.UserId).ToList()
        };
    }

    public async Task<IEnumerable<ArticleResponse>> GetArticlesAsync()
    {
        var articles = await _dataHandler.GetArticlesAsync();
        return (articles ?? new List<Article>())
            .Select(ToResponse);
    }

    public async Task<ArticleResponse?> GetArticleByIdAsync(int id)
    {
        var article = await _dataHandler.GetArticleByIdAsync(id);
        return article == null ? null : await ToResponseWithCollaboratorsAsync(article);
    }

    public async Task<ArticleResponse> AddArticleAsync(CreateArticleRequest request, string authorId)
    {
        var article = new Article
        {
            Title = request.Title,
            Content = request.Content,
            Summary = request.Summary,
            AuthorId = authorId
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
            _logger.LogError(ex, "Failed to add article");
            throw;
        }
    }

    public async Task<bool> DeleteArticleAsync(int id, string userId, bool isAdmin)
    {
        var article = await _dataHandler.GetArticleByIdAsync(id);
        if (article == null) return false;

        if (!await CanEditArticle(article, userId, isAdmin))
        {
            throw new UnauthorizedAccessException($"User {userId} does not have permission to delete article {id}");
        }

        try
        {
            return await _dataHandler.DeleteArticleAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete article with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> UpdateArticleAsync(int id, UpdateArticleRequest request, string userId, bool isAdmin)
    {
        var current = await _dataHandler.GetArticleByIdAsync(id);
        if (current == null) return false;

        if (!await CanEditArticle(current, userId, isAdmin))
        {
            throw new UnauthorizedAccessException($"User {userId} does not have permission to update article {id}");
        }

        var article = new Article
        {
            Title = request.Title,
            Content = request.Content,
            Summary = request.Summary
        };

        try
        {
            return await _dataHandler.UpdateArticleAsync(id, article, current.Content, current.Summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update article with ID {Id}", id);
            throw;
        }
    }

    public IEnumerable<ArticleResponse> Search(string query)
        => _dataHandler.SearchArticlesAsync(query).GetAwaiter().GetResult()
            .Select(ToResponse);

    private static RevisionResponse ToRevisionResponse(Revision revision) => new RevisionResponse
    {
        Id = revision.Id,
        ArticleId = revision.ArticleId,
        Content = revision.Content,
        Summary = revision.Summary,
        VersionNumber = revision.VersionNumber,
        SavedAt = revision.SavedAt
    };

    public async Task<IEnumerable<RevisionResponse>> GetRevisionsAsync(int articleId)
    {
        var revisions = await _dataHandler.GetRevisionsAsync(articleId);
        return revisions.Select(ToRevisionResponse);
    }

    private static CollaboratorResponse ToCollaboratorResponse(ArticleCollaborator collaborator) => new CollaboratorResponse
    {
        UserId = collaborator.UserId,
        UserName = collaborator.User.UserName ?? ""
    };

    public async Task<IEnumerable<CollaboratorResponse>> GetCollaboratorsAsync(int articleId)
    {
        var collaborators = await _dataHandler.GetCollaboratorsAsync(articleId);
        return collaborators.Select(ToCollaboratorResponse);
    }

    public async Task<bool> AddCollaboratorAsync(int articleId, string userId)
    {
        try
        {
            return await _dataHandler.AddCollaboratorAsync(articleId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add collaborator");
            throw;
        }
    }

    public async Task<bool> AddCollaboratorByUsernameAsync(int articleId, string username)
    {
        var userId = await _authService.GetUserIdByUsernameAsync(username);
        if (userId == null)
        {
            throw new ArgumentException($"User with username '{username}' not found");
        }

        return await AddCollaboratorAsync(articleId, userId);
    }

    public async Task<bool> RemoveCollaboratorAsync(int articleId, string userId)
    {
        try
        {
            return await _dataHandler.RemoveCollaboratorAsync(articleId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove collaborator");
            throw;
        }
    }

    public async Task<bool> IsCollaboratorAsync(int articleId, string userId)
    {
        try
        {
            return await _dataHandler.IsCollaboratorAsync(articleId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check collaborator status");
            throw;
        }
    }

    private async Task<bool> CanEditArticle(Article article, string userId, bool isAdmin)
    {
        // Admin can always edit
        if (isAdmin) return true;

        // Author can edit
        if (article.AuthorId == userId) return true;

        // Collaborator can edit
        if (await _dataHandler.IsCollaboratorAsync(article.Id, userId)) return true;

        return false;
    }
}
