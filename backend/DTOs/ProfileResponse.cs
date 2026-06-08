namespace Lexicon.DTOs;

public class ProfileResponse
{
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public List<ProfileArticleResponse> Articles { get; set; } = new();
}
