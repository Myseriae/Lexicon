using Lexicon.DTOs;
using Lexicon.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Lexicon.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    private const string RefreshTokenCookieName = "refreshToken";

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger      = logger;
    }

    // -------------------------------------------------------------------------
    // POST /api/auth/register
    // -------------------------------------------------------------------------

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(
            request.Email, request.UserName, request.Password);

        if (!result.Success)
        {
            foreach (var (key, message) in result.ErrorMessages)
                ModelState.AddModelError(key, message);
            return ValidationProblem();
        }

        return Created();
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
            return Unauthorized("Refresh token is invalid or expired. Please log in again.");

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

        // Always clear the cookie regardless of whether we found the token in DB.
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Path     = "/api/auth/refresh",
            SameSite = SameSiteMode.Strict,
            Secure   = true
        });

        return NoContent();
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Writes the refresh token as an httpOnly cookie restricted to /api/auth/refresh.
    /// </summary>
    private void SetRefreshTokenCookie(string refreshToken)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,                          // JS cannot read this
            Secure   = true,                          // HTTPS only — set false for local HTTP dev
            SameSite = SameSiteMode.Strict,           // not sent cross-site (CSRF protection)
            Path     = "/api/auth/refresh",           // cookie only sent to this path
            Expires  = DateTimeOffset.UtcNow.AddDays(7)
        };

        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, options);
    }
}