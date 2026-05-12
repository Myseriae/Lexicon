using Lexicon.Services.Authentication;

namespace Lexicon.Services.Auth;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string username, string password);
    Task<AuthResult> LoginAsync(string email, string password);

    /// <summary>
    /// Validates the incoming refresh token, revokes it, issues a new access token,
    /// and stores a new refresh token (rotation).
    /// </summary>
    Task<AuthResult> RefreshAsync(string refreshToken);

    /// <summary>Revokes the refresh token so it can no longer be used.</summary>
    Task<bool> LogoutAsync(string refreshToken);
}