using System.Collections.Generic;

namespace Lexicon.DTOs;

public class ArticleResponse
{
    public int Id { get; set; }
    public string AuthorId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string? Summary { get; set; }
    public DateTime Created { get; set; }
    public List<string> CollaboratorIds { get; set; } = new List<string>();
}
