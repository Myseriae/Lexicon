using Lexicon.DTOs;
using Lexicon.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lexicon.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    private const string RefreshTokenCookieName = "refreshToken";

    public AuthController(
        IAuthService authService,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _authService = authService;
        _configuration = configuration;
        _environment = environment;
    }

    // -------------------------------------------------------------------------
    // POST /api/auth/register
    // -------------------------------------------------------------------------

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthTokenResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(
            request.Email, request.UserName, request.Password);

        if (!result.Success)
        {
            foreach (var (key, message) in result.ErrorMessages)
                ModelState.AddModelError(key, message);
            return ValidationProblem();
        }

        SetRefreshTokenCookie(result.RefreshToken);

        return Created("", new AuthTokenResponse(
            result.AccessToken,
            result.Email,
            result.UserName,
            result.Role));
    }

    // -------------------------------------------------------------------------
    // POST /api/auth/login
    // Returns: access token in body + refresh token as httpOnly cookie
    // -------------------------------------------------------------------------

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthTokenResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (!result.Success)
        {
            foreach (var (key, message) in result.ErrorMessages)
                ModelState.AddModelError(key, message);
            return ValidationProblem();
        }

        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(new AuthTokenResponse(
            result.AccessToken,
            result.Email,
            result.UserName,
            result.Role));
    }

    // -------------------------------------------------------------------------
    // POST /api/auth/refresh
    // Reads the httpOnly cookie, rotates the refresh token, returns a new access token
    // -------------------------------------------------------------------------

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokenResponse>> Refresh()
    {
        var incomingToken = Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrEmpty(incomingToken))
            return Unauthorized("No refresh token cookie present.");

        var result = await _authService.RefreshAsync(incomingToken);

        if (!result.Success)
        {
            ClearRefreshTokenCookie();
            return Unauthorized("Refresh token is invalid or expired. Please log in again.");
        }

        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(new AuthTokenResponse(
            result.AccessToken,
            result.Email,
            result.UserName,
            result.Role));
    }

    // -------------------------------------------------------------------------
    // POST /api/auth/logout
    // Revokes the refresh token in DB and clears the cookie
    // -------------------------------------------------------------------------

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var incomingToken = Request.Cookies[RefreshTokenCookieName];

        if (!string.IsNullOrEmpty(incomingToken))
            await _authService.LogoutAsync(incomingToken);

        ClearRefreshTokenCookie();

        return NoContent();
    }

    // -------------------------------------------------------------------------
    // GET /api/auth/verify
    // Returns user info if token is valid
    // -------------------------------------------------------------------------

    [HttpGet("verify")]
    [Authorize]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<AuthTokenResponse> Verify()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (email == null || userName == null || role == null)
        {
            return Unauthorized();
        }

        return Ok(new AuthTokenResponse(
            null, // Frontend already has the token
            email,
            userName,
            role));
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Writes the refresh token as an httpOnly cookie for auth endpoints.
    /// </summary>
    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            CreateRefreshTokenCookieOptions());
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete(
            RefreshTokenCookieName,
            CreateRefreshTokenCookieOptions(DateTimeOffset.UtcNow.AddDays(-1)));
    }

    private CookieOptions CreateRefreshTokenCookieOptions(DateTimeOffset? expires = null)
    {
        var expirationDays = int.TryParse(
            _configuration["Jwt:RefreshTokenExpirationDays"], out var days) ? days : 7;
        var isDevelopment = _environment.IsDevelopment();

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/api/auth",
            Expires = expires ?? DateTimeOffset.UtcNow.AddDays(expirationDays)
        };
    }
}
