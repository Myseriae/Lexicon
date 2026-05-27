using System.Security.Cryptography;
using Lexicon.Data;
using Lexicon.Model;
using Lexicon.Services.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lexicon.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly LexiconDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<IdentityUser> userManager,
        ITokenService tokenService,
        LexiconDbContext context,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager   = userManager;
        _tokenService  = tokenService;
        _context       = context;
        _configuration = configuration;
        _logger        = logger;
    }

    // -------------------------------------------------------------------------
    // Register
    // -------------------------------------------------------------------------

    public async Task<AuthResult> RegisterAsync(string email, string username, string password)
    {
        var user = new IdentityUser
        {
            UserName = username,
            Email = email
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var failure = new AuthResult(false, "", "", email, username, "");
        
            foreach (var error in result.Errors)
                failure.ErrorMessages[error.Code] = error.Description;

            return failure;
        }

        // Assign default role
        await _userManager.AddToRoleAsync(user, Roles.Editor);

        var role = Roles.Editor;

        var accessToken  = _tokenService.CreateAccessToken(user, role);
        var refreshToken = await CreateAndStoreRefreshTokenAsync(user.Id);

        _logger.LogInformation("User '{UserName}' registered.", username);

        return new AuthResult(
            true,
            accessToken,
            refreshToken,
            email,
            username,
            role
        );
    }

    // -------------------------------------------------------------------------
    // Login
    // -------------------------------------------------------------------------

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Fail(email, "Bad credentials");

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
            return Fail(email, "Bad credentials");

        var roles      = await _userManager.GetRolesAsync(user);
        var role       = roles.FirstOrDefault() ?? string.Empty;
        var accessToken  = _tokenService.CreateAccessToken(user, role);
        var refreshToken = await CreateAndStoreRefreshTokenAsync(user.Id);

        _logger.LogInformation("User '{UserName}' logged in.", user.UserName);
        
        return new AuthResult(
            true,
            accessToken,
            refreshToken,
            user.Email!,
            user.UserName!,
            role
        );
    }

    // -------------------------------------------------------------------------
    // Refresh — validates old token, rotates it, issues new access token
    // -------------------------------------------------------------------------

    public async Task<AuthResult> RefreshAsync(string incomingRefreshToken)
    {
        var stored = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == incomingRefreshToken);

        if (stored is null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh attempt with invalid or expired token.");
            return Fail(string.Empty, "Invalid refresh token");
        }

        // Revoke the used token immediately (rotation — prevents reuse).
        stored.IsRevoked = true;
        await _context.SaveChangesAsync();

        var user = await _userManager.FindByIdAsync(stored.UserId);
        if (user is null)
            return Fail(string.Empty, "User not found");

        var roles        = await _userManager.GetRolesAsync(user);
        var role         = roles.FirstOrDefault() ?? string.Empty;
        var newAccess    = _tokenService.CreateAccessToken(user, role);
        var newRefresh   = await CreateAndStoreRefreshTokenAsync(user.Id);

        _logger.LogInformation("Tokens rotated for user '{UserId}'.", user.Id);

        return new AuthResult(
            true,
            newAccess,
            newRefresh,
            user.Email!,
            user.UserName!,
            role);
    }

    // -------------------------------------------------------------------------
    // Logout — revokes refresh token so it can no longer be rotated
    // -------------------------------------------------------------------------

    /// <summary>Revokes the refresh token so it can no longer be used.</summary>
    public async Task<bool> LogoutAsync(string incomingRefreshToken)
    {
        var stored = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == incomingRefreshToken);

        if (stored is null || stored.IsRevoked)
            return false;

        stored.IsRevoked = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Refresh token revoked for user '{UserId}'.", stored.UserId);
        return true;
    }

    /// <summary>Finds a user by username and returns their ID.</summary>
    public async Task<string?> GetUserIdByUsernameAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        return user?.Id;
    }

    /// <summary>Finds a user by ID and returns their username.</summary>
    public async Task<string?> GetUsernameByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.UserName;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Generates a cryptographically random 32-byte token, stores it in the DB,
    /// and returns the Base64 string to put in the cookie.
    /// </summary>
    private async Task<string> CreateAndStoreRefreshTokenAsync(string userId)
    {
        var expirationDays = int.TryParse(
            _configuration["Jwt:RefreshTokenExpirationDays"], out var days) ? days : 7;

        // RandomNumberGenerator produces a cryptographically secure random value.
        var tokenBytes  = RandomNumberGenerator.GetBytes(32);
        var tokenString = Convert.ToBase64String(tokenBytes);

        var refreshToken = new RefreshToken
        {
            Token     = tokenString,
            UserId    = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return tokenString;
    }

    private static AuthResult Fail(string email, string errorDescription)
    {
        var result = new AuthResult(false, string.Empty, string.Empty, email, string.Empty, string.Empty);
        result.ErrorMessages["AuthError"] = errorDescription;
        return result;
    }
}
