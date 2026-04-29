using System.ComponentModel.DataAnnotations;

namespace Lexicon.DTOs;

public class UpdateArticleRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    [Required]
    [MaxLength(50000)]
    public string Content { get; set; } = "";
}
