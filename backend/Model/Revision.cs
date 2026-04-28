namespace Lexicon.Model;

public class Revision
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    public DateTime SavedAt { get; set; } = DateTime.Now;
    public int VersionNumber { get; set; }
    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;
}