namespace Lexicon.DTOs;

/// <summary>
/// Returned in the response body on login and refresh.
/// The refresh token itself is NOT included here — it is in the httpOnly cookie.
/// </summary>
public record AuthTokenResponse(
    string? AccessToken,
    string Email,
    string UserName,
    string Role);