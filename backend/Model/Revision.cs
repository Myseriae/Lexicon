using System.ComponentModel.DataAnnotations;

namespace Lexicon.Model;

public class Revision
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50000)]
    public string Content { get; set; } = "";
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    public int VersionNumber { get; set; }
    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;
}