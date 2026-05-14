using System.ComponentModel.DataAnnotations;

namespace Lexicon.Model;

public class Tag
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    public List<Article> Articles { get; set; } = new();
}