using Microsoft.AspNetCore.Identity;

namespace Lexicon.Services.Auth;

public interface ITokenService
{
    /// <summary>Creates a signed short-lived JWT access token for the given user and role.</summary>
    string CreateAccessToken(IdentityUser user, string role);
}