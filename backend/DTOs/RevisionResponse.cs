namespace Lexicon.DTOs;

public class RevisionResponse
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public string Content { get; set; } = "";
    public string? Summary { get; set; }
    public int VersionNumber { get; set; }
    public DateTime SavedAt { get; set; }
}