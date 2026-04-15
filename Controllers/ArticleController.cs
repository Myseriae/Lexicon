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
}