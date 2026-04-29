namespace Lexicon.DTOs;

public class ArticleResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string? Summary { get; set; }
    public DateTime Created { get; set; }
}
