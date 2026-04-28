namespace Lexicon.Model;

public class Article
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string? Summary { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
    public List<Revision> Revisions { get; set; } = new List<Revision>();
}