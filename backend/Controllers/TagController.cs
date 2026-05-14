using Lexicon.Data;
using Lexicon.DTOs;
using Lexicon.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lexicon.Controllers;

[ApiController]
public class TagController : ControllerBase
{
    private readonly ITagRepository _tagRepository;

    public TagController(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    [HttpGet("api/tags")]
    public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
    {
        var tags = await _tagRepository.GetAllAsync();
        return Ok(tags);
    }

    [HttpPost("api/articles/{id}/tags")]
    [Authorize(Roles = $"{Lexicon.Services.Authentication.Roles.Editor}, {Lexicon.Services.Authentication.Roles.Admin}")]
    public async Task<IActionResult> AddTagToArticle(int id, [FromBody] AddTagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Tag name is required.");

        var success = await _tagRepository.AddTagToArticleAsync(id, request.Name);

        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("api/articles/{id}/tags/{tagId}")]
    [Authorize(Roles = $"{Lexicon.Services.Authentication.Roles.Editor}, {Lexicon.Services.Authentication.Roles.Admin}")]
    public async Task<IActionResult> RemoveTagFromArticle(int id, int tagId)
    {
        var success = await _tagRepository.RemoveTagFromArticleAsync(id, tagId);

        if (!success)
            return NotFound();

        return NoContent();
    }
}