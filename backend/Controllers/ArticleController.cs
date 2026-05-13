using Lexicon.DTOs;
using Lexicon.Services;
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

    // -------------------------------------------------------------------------
    // Write endpoints — require authentication (Editor or Admin)
    // -------------------------------------------------------------------------

    [HttpPost]
    [Authorize(Roles = $"{Lexicon.Services.Authentication.Roles.Editor}, {Lexicon.Services.Authentication.Roles.Admin}")]
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
    [Authorize(Roles = $"{Lexicon.Services.Authentication.Roles.Editor}, {Lexicon.Services.Authentication.Roles.Admin}")]
    public async Task<IActionResult> UpdateArticle(int articleId, UpdateArticleRequest request)
    {
        try
        {
            // Full authorization (owner vs admin check) will be added when the
            // AuthorId service layer is wired up in the collaborators feature.
            var success = await _articleService.UpdateArticleAsync(articleId, request);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Failed to update article.");
        }
    }

    [HttpDelete("{articleId}")]
    [Authorize(Roles = $"{Lexicon.Services.Authentication.Roles.Editor}, {Lexicon.Services.Authentication.Roles.Admin}")]
    public async Task<IActionResult> DeleteArticle(int articleId)
    {
        try
        {
            var success = await _articleService.DeleteArticleAsync(articleId);
            if (!success) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Failed to delete article.");
        }
    }
}