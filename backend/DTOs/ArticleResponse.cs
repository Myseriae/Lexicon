namespace Lexicon.DTOs;

public class ArticleResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string? Summary { get; set; }
    public DateTime Created { get; set; }

    public List<TagResponse> Tags { get; set; } = new();
}

public class TagResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}