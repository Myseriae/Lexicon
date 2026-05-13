using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Lexicon.Model;

public class ArticleCollaborator
{
    [Key]
    [Column(Order = 0)]
    public int ArticleId { get; set; }

    [Key]
    [Column(Order = 1)]
    public string UserId { get; set; } = "";

    [ForeignKey("ArticleId")]
    public Article Article { get; set; } = null!;

    [ForeignKey("UserId")]
    public IdentityUser User { get; set; } = null!;
}
