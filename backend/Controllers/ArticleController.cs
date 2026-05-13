using Lexicon.DTOs;
using Lexicon.Services;
using Lexicon.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lexicon.Controllers;

[ApiController]
[Route("api/articles")]
public class ArticleController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticleController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    // -------------------------------------------------------------------------
    // Read endpoints — public (no authentication required)
    // -------------------------------------------------------------------------

    [HttpGet]
    public ActionResult<IEnumerable<ArticleResponse>> GetArticles()
        => Ok(_articleService.GetArticles());

    [HttpGet("{articleId}")]
    public ActionResult<ArticleResponse> GetArticle(int articleId)
    {
        var article = _articleService.GetArticleById(articleId);
        if (article == null) return NotFound();
        return Ok(article);
    }

    [HttpGet("search")]
    public ActionResult<IEnumerable<ArticleResponse>> Search(string query)
        => Ok(_articleService.Search(query));

    [HttpGet("{articleId}/revisions")]
    public async Task<ActionResult<IEnumerable<RevisionResponse>>> GetRevisions(int articleId)
        => Ok(await _articleService.GetRevisionsAsync(articleId));

    [HttpGet("{articleId}/collaborators")]
    [Authorize(Roles = $"{Roles.Editor}, {Roles.Admin}")]
    public async Task<ActionResult<IEnumerable<CollaboratorResponse>>> GetCollaborators(int articleId)
        => Ok(await _articleService.GetCollaboratorsAsync(articleId));

    [HttpGet("{articleId}/collaborators/{userId}/is-collaborator")]
    [Authorize(Roles = $"{Roles.Editor}, {Roles.Admin}")]
    public async Task<ActionResult<bool>> IsCollaborator(int articleId, string userId)
        => Ok(await _articleService.IsCollaboratorAsync(articleId, userId));

    // -------------------------------------------------------------------------
    // Write endpoints — require authentication (Editor or Admin)
    // -------------------------------------------------------------------------

    [HttpPost]
    [Authorize(Roles = $"{Roles.Editor}, {Roles.Admin}")]
    public async Task<ActionResult<ArticleResponse>> CreateArticle(CreateArticleRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var created = await _articleService.AddArticleAsync(request, userId);
            return CreatedAtAction(nameof(GetArticle), new { articleId = created.Id }, created);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Failed to save article.");
        }
    }

    [HttpPut("{articleId}")]
    [Authorize(Roles = $"{Roles.Editor}, {Roles.Admin}")]
    public async Task<IActionResult> UpdateArticle(int articleId, UpdateArticleRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole(Roles.Admin);

        try
        {
            var success = await _articleService.UpdateArticleAsync(articleId, request, userId, isAdmin);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Failed to update article.");
        }
    }

    [HttpDelete("{articleId}")]
    [Authorize(Roles = $"{Roles.Editor}, {Roles.Admin}")]
    public async Task<IActionResult> DeleteArticle(int articleId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var isAdmin = User.IsInRole(Roles.Admin);

        try
        {
            var success = await _articleService.DeleteArticleAsync(articleId, userId, isAdmin);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Failed to delete article.");
        }
    }

    [HttpPost("{articleId}/collaborators/{userId}")]
    [Authorize(Roles = $"{Roles.Editor}, {Roles.Admin}")]
    public async Task<IActionResult> AddCollaborator(int articleId, string userId)
    {
        try
        {
            var success = await _articleService.AddCollaboratorAsync(articleId, userId);
            if (!success) return BadRequest("User is already a collaborator or invalid user.");
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Failed to add collaborator.");
        }
    }

    [HttpDelete("{articleId}/collaborators/{userId}")]
    [Authorize(Roles = $"{Roles.Editor}, {Roles.Admin}")]
    public async Task<IActionResult> RemoveCollaborator(int articleId, string userId)
    {
        try
        {
            var success = await _articleService.RemoveCollaboratorAsync(articleId, userId);
            if (!success) return NotFound("Collaborator not found.");
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Failed to remove collaborator.");
        }
    }
}