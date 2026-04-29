using Lexicon.DTOs;
using Lexicon.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lexicon.Controllers;

[ApiController]
[Route("[controller]")]
public class ArticleController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticleController(IArticleService articleService)
    {
        _articleService = articleService;
    }

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

    [HttpPost]
    public async Task<ActionResult<ArticleResponse>> CreateArticle(CreateArticleRequest request)
    {
        try
        {
            var created = await _articleService.AddArticleAsync(request);

            return CreatedAtAction(
                nameof(GetArticle),
                new { articleId = created.Id },
                created);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Failed to save article.");
        }
    }

    [HttpDelete("{articleId}")]
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

    [HttpPut("{articleId}")]
    public async Task<IActionResult> UpdateArticle(int articleId, UpdateArticleRequest request)
    {
        try
        {
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

    [HttpGet("search")]
    public ActionResult<IEnumerable<ArticleResponse>> Search(string query)
        => Ok(_articleService.Search(query));

    [HttpGet("{articleId}/revisions")]
    public async Task<ActionResult<IEnumerable<RevisionResponse>>> GetRevisions(int articleId) =>
        Ok(await _articleService.GetRevisionsAsync(articleId));
}
