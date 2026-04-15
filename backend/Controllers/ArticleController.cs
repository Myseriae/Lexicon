using Lexicon.Data;
using Lexicon.Model;
using Microsoft.AspNetCore.Mvc;

namespace Lexicon.Controllers;

[ApiController]
[Route("[controller]")]
public class ArticleController : ControllerBase
{
    private readonly IDataHandler _dataHandler;

    public ArticleController(IDataHandler dataHandler)
    {
        _dataHandler = dataHandler;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Article>> GetArticles()
        => Ok(_dataHandler.GetArticles());

    [HttpGet("{articleId}")]
    public ActionResult<Article> GetArticle(int articleId)
    {
        var article = _dataHandler.GetArticleById(articleId);
        if (article == null) return NotFound();

        return Ok(article);
    }

    [HttpPost]
    public ActionResult<Article> CreateArticle(Article article)
    {
        var created = _dataHandler.AddArticle(article);

        return CreatedAtAction(
            nameof(GetArticle),
            new { articleId = created.Id },
            created);
    }

    [HttpDelete("{articleId}")]
    public IActionResult DeleteArticle(int articleId)
    {
        var success = _dataHandler.DeleteArticle(articleId);
        if (!success) return NotFound();

        return NoContent();
    }
    
    
    [HttpPut("{articleId}")]
    public IActionResult UpdateArticle(int articleId, Article updatedArticle)
    {
        var success = _dataHandler.UpdateArticle(articleId, updatedArticle);
        if (!success) return NotFound();

        return NoContent();
    }
    
    [HttpGet("search")]
    public ActionResult<IEnumerable<Article>> Search(string query)
    {
        var result = _dataHandler
            .GetArticles()
            .Where(a => a.Title.Contains(query, StringComparison.OrdinalIgnoreCase));

        return Ok(result);
    }
}