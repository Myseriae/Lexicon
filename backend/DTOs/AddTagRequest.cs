using System.ComponentModel.DataAnnotations;

namespace Lexicon.DTOs;

public class AddTagRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";
}