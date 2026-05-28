using Lexicon.DTOs;
using Lexicon.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lexicon.Controllers;

[ApiController]
[Route("api")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet("tags")]
    public async Task<ActionResult<IEnumerable<TagResponse>>> GetTags()
    {
        var tags = await _tagService.GetAllAsync();
        return Ok(tags);
    }

    [HttpPost("articles/{id}/tags")]
    [Authorize(Roles = $"{Lexicon.Services.Authentication.Roles.Editor}, {Lexicon.Services.Authentication.Roles.Admin}")]
    public async Task<IActionResult> AddTagToArticle(int id, [FromBody] AddTagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Tag name is required.");

        var success = await _tagService.AddTagToArticleAsync(id, request.Name);

        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("articles/{id}/tags/{tagId}")]
    [Authorize(Roles = $"{Lexicon.Services.Authentication.Roles.Editor}, {Lexicon.Services.Authentication.Roles.Admin}")]
    public async Task<IActionResult> RemoveTagFromArticle(int id, int tagId)
    {
        var success = await _tagService.RemoveTagFromArticleAsync(id, tagId);

        if (!success)
            return NotFound();

        return NoContent();
    }
}
