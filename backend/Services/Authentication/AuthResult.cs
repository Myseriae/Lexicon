namespace Lexicon.Services.Authentication;

public record AuthResult(
    bool Success,
    string AccessToken,
    string RefreshToken,
    string Email,
    string UserName,
    string Role)
{
    public Dictionary<string, string> ErrorMessages { get; init; } = new Dictionary<string, string>();
}