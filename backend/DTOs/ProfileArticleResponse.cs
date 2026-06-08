namespace Lexicon.DTOs;

public class ProfileArticleResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Summary { get; set; }
    public DateTime Created { get; set; }
}
