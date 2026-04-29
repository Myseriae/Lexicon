using Lexicon.Model;
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
    public ActionResult<IEnumerable<Article>> GetArticles()
        => Ok(_articleService.GetArticles());

    [HttpGet("{articleId}")]
    public ActionResult<Article> GetArticle(int articleId)
    {
        var article = _articleService.GetArticleById(articleId);
        if (article == null) return NotFound();

        return Ok(article);
    }

    [HttpPost]
    public async Task<ActionResult<Article>> CreateArticle(Article article)
    {
        try
        {
            var created = await _articleService.AddArticleAsync(article);

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
    public async Task<IActionResult> UpdateArticle(int articleId, Article updatedArticle)
    {
        try
        {
            var success = await _articleService.UpdateArticleAsync(articleId, updatedArticle);
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
    public ActionResult<IEnumerable<Article>> Search(string query)
        => Ok(_articleService.Search(query));
}
