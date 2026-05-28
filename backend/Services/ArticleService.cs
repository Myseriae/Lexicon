using Lexicon.Data;
using Lexicon.DTOs;
using Lexicon.Model;
using Microsoft.Extensions.Logging;
using Lexicon.Services.Auth;

namespace Lexicon.Services;

public class ArticleService : IArticleService
{
    private readonly IArticleRepository _articleRepository;
    private readonly IRevisionRepository _revisionRepository;
    private readonly IWikipediaService _wikipediaService;
    private readonly IAuthService _authService;
    private readonly ILogger<ArticleService> _logger;

    public ArticleService(IArticleRepository articleRepository, IRevisionRepository revisionRepository, IWikipediaService wikipediaService, IAuthService authService, ILogger<ArticleService> logger)
    {
        _articleRepository = articleRepository;
        _revisionRepository = revisionRepository;
        _wikipediaService = wikipediaService;
        _authService = authService;
        _logger = logger;
    }

    private async Task<string> GetUsernameByIdAsync(string userId)
    {
        try
        {
            return await _authService.GetUsernameByIdAsync(userId) ?? userId;
        }
        catch
        {
            return userId;
        }
    }

    private async Task<ArticleResponse> ToResponseAsync(Article article)
    {
        var authorUsername = await GetUsernameByIdAsync(article.AuthorId);
        return new ArticleResponse
        {
            Id = article.Id,
            AuthorUsername = authorUsername,
            Title = article.Title,
            Content = article.Content,
            Summary = article.Summary,
            Created = article.Created,
            Tags = article.Tags.Select(t => new TagResponse
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        };
    }

    private async Task<ArticleResponse> ToResponseWithCollaboratorsAsync(Article article)
    {
        var collaborators = await _articleRepository.GetCollaboratorsAsync(article.Id);
        var authorUsername = await GetUsernameByIdAsync(article.AuthorId);
        return new ArticleResponse
        {
            Id = article.Id,
            AuthorUsername = authorUsername,
            Title = article.Title,
            Content = article.Content,
            Summary = article.Summary,
            Created = article.Created,
            CollaboratorIds = collaborators.Select(c => c.UserId).ToList(),
            Tags = article.Tags.Select(t => new TagResponse
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        };
    }

    private ArticleResponse ToResponse(Article article, Dictionary<string, string> usernameMap)
    {
        return new ArticleResponse
        {
            Id = article.Id,
            AuthorUsername = usernameMap.GetValueOrDefault(article.AuthorId, article.AuthorId),
            Title = article.Title,
            Content = article.Content,
            Summary = article.Summary,
            Created = article.Created,
            Tags = article.Tags?.Select(t => new TagResponse { Id = t.Id, Name = t.Name }).ToList() ?? new List<TagResponse>(),
            CollaboratorIds = article.Collaborators?.Select(c => c.UserId).ToList() ?? new List<string>()
        };
    }

    public async Task<IEnumerable<ArticleResponse>> GetArticlesAsync(string? tag = null)
    {
        var articles = (await _articleRepository.GetArticlesAsync(tag) ?? new List<Article>()).ToList();
        var usernameMap = await _authService.GetUsernamesByIdsAsync(articles.Select(a => a.AuthorId));
        return articles.Select(a => ToResponse(a, usernameMap)).ToList();
    }

    public async Task<ArticleResponse?> GetArticleByIdAsync(int id)
    {
        var article = await _articleRepository.GetArticleByIdAsync(id);
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
            var saved = await _articleRepository.AddArticleAsync(article);
            return await ToResponseAsync(saved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add article");
            throw;
        }
    }

    public async Task<bool> DeleteArticleAsync(int id, string userId, bool isAdmin)
    {
        var article = await _articleRepository.GetArticleByIdAsync(id);
        if (article == null) return false;

        if (!await CanEditArticle(article, userId, isAdmin))
        {
            throw new UnauthorizedAccessException($"User {userId} does not have permission to delete article {id}");
        }

        try
        {
            return await _articleRepository.DeleteArticleAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete article with ID {Id}", id);
            throw;
        }
    }

    public async Task<bool> UpdateArticleAsync(int id, UpdateArticleRequest request, string userId, bool isAdmin)
    {
        var current = await _articleRepository.GetArticleByIdAsync(id);
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
            var existingRevisions = await _revisionRepository.GetRevisionsAsync(id);
            var revision = new Revision
            {
                ArticleId = id,
                Content = current.Content,
                Summary = request.Summary,
                VersionNumber = existingRevisions.Count() + 1,
                SavedAt = DateTime.UtcNow
            };
            await _revisionRepository.AddRevisionAsync(revision);

            return await _articleRepository.UpdateArticleAsync(id, article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update article with ID {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<ArticleResponse>> SearchAsync(string query)
    {
        var articles = (await _articleRepository.SearchArticlesAsync(query) ?? new List<Article>()).ToList();
        var usernameMap = await _authService.GetUsernamesByIdsAsync(articles.Select(a => a.AuthorId));
        return articles.Select(a => ToResponse(a, usernameMap)).ToList();
    }

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
        var revisions = await _revisionRepository.GetRevisionsAsync(articleId);
        return revisions.Select(ToRevisionResponse);
    }

    private static CollaboratorResponse ToCollaboratorResponse(ArticleCollaborator collaborator) => new CollaboratorResponse
    {
        UserId = collaborator.UserId,
        UserName = collaborator.User.UserName ?? ""
    };

    public async Task<IEnumerable<CollaboratorResponse>> GetCollaboratorsAsync(int articleId)
    {
        await GetExistingArticleAsync(articleId);

        var collaborators = await _articleRepository.GetCollaboratorsAsync(articleId);

        return collaborators.Select(ToCollaboratorResponse);
    }

    public async Task<bool> AddCollaboratorAsync(
        int articleId,
        string collaboratorUserId,
        string currentUserId,
        bool isAdmin)
    {
        var article = await GetExistingArticleAsync(articleId);

        if (!isAdmin && article.AuthorId != currentUserId)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId} is not allowed to manage collaborators for article {articleId}"
            );
        }

        try
        {
            return await _articleRepository.AddCollaboratorAsync(articleId, collaboratorUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add collaborator");
            throw;
        }
    }

    public async Task<bool> AddCollaboratorByUsernameAsync(
        int articleId,
        string username,
        string currentUserId,
        bool isAdmin)
    {
        var collaboratorUserId = await _authService.GetUserIdByUsernameAsync(username);

        if (collaboratorUserId == null)
        {
            throw new ArgumentException($"User with username '{username}' not found");
        }

        return await AddCollaboratorAsync(
            articleId,
            collaboratorUserId,
            currentUserId,
            isAdmin
        );
    }

    public async Task<bool> RemoveCollaboratorAsync(
        int articleId,
        string collaboratorUserId,
        string currentUserId,
        bool isAdmin)
    {
        var article = await GetExistingArticleAsync(articleId);

        if (!isAdmin && article.AuthorId != currentUserId)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId} is not allowed to manage collaborators for article {articleId}"
            );
        }

        try
        {
            return await _articleRepository.RemoveCollaboratorAsync(articleId, collaboratorUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove collaborator");
            throw;
        }
    }

    public async Task<bool> IsCollaboratorAsync(int articleId, string userId)
    {
        await GetExistingArticleAsync(articleId);

        try
        {
            return await _articleRepository.IsCollaboratorAsync(articleId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check collaborator status");
            throw;
        }
    }

    private async Task<bool> CanEditArticle(Article article, string userId, bool isAdmin)
    {
        if (isAdmin) return true;
        if (article.AuthorId == userId) return true;
        if (await _articleRepository.IsCollaboratorAsync(article.Id, userId)) return true;
        return false;
    }
    
    private async Task<Article> GetExistingArticleAsync(int articleId)
    {
        var article = await _articleRepository.GetArticleByIdAsync(articleId);

        if (article == null)
        {
            throw new KeyNotFoundException($"Article with ID {articleId} not found");
        }

        return article;
    }
}
