using Lexicon.DTOs;
using Lexicon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public ActionResult<IEnumerable<ArticleResponse>> GetArticles([FromQuery] string? tag)
        => Ok(_articleService.GetArticles(tag));

    [HttpGet("{articleId}")]
    public ActionResult<ArticleResponse> GetArticle(int articleId)
    {
        var article = _articleService.GetArticleById(articleId);
        if (article == null) return NotFound();
        return Ok(article);
    }

    [HttpGet("search")]
    public ActionResult<IEnumerable<ArticleResponse>> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(_articleService.GetArticles());

        return Ok(_articleService.Search(query));
    }

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
        try
        {
            var created = await _articleService.AddArticleAsync(request);
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