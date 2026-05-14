using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Lexicon.Model;

public class Article
{
    public int Id { get; set; }
    public string AuthorId { get; set; } = "";

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = "";

    [Required]
    [MaxLength(50000)]
    public string Content { get; set; } = "";

    [MaxLength(1000)]
    public string? Summary { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public List<Revision> Revisions { get; set; } = new List<Revision>();
    public List<ArticleCollaborator> Collaborators { get; set; } = new List<ArticleCollaborator>();
    
    public List<Tag> Tags { get; set; } = new();
    
}