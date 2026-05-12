using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Lexicon.Services.Auth;

public class TokenService : ITokenService
{
    private readonly string _validIssuer;
    private readonly string _validAudience;
    private readonly string _issuerSigningKey;
    private readonly int _expirationMinutes;

    public TokenService(IConfiguration configuration)
    {
        _validIssuer = configuration["Jwt:ValidIssuer"]
                       ?? throw new InvalidOperationException("Jwt:ValidIssuer is missing.");
        _validAudience = configuration["Jwt:ValidAudience"]
                         ?? throw new InvalidOperationException("Jwt:ValidAudience is missing.");
        _issuerSigningKey = configuration["Jwt:IssuerSigningKey"]
                            ?? throw new InvalidOperationException(
                                "Jwt:IssuerSigningKey is missing. " +
                                "Set it via: dotnet user-secrets set \"Jwt:IssuerSigningKey\" \"<secret>\"");
        _expirationMinutes = int.TryParse(
            configuration["Jwt:AccessTokenExpirationMinutes"], out var minutes)
            ? minutes : 15;
    }

    public string CreateAccessToken(IdentityUser user, string role)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject            = new ClaimsIdentity(BuildClaims(user, role)),
            Expires            = DateTime.UtcNow.AddMinutes(_expirationMinutes),
            Issuer             = _validIssuer,
            Audience           = _validAudience,
            SigningCredentials = BuildSigningCredentials()
        };

        // JsonWebTokenHandler is the .NET 10 replacement for the obsolete JwtSecurityTokenHandler.
        // CreateToken() returns the signed token string directly.
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private static List<Claim> BuildClaims(IdentityUser user, string role)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,  user.Id),
            new(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,  DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new(ClaimTypes.NameIdentifier,    user.Id),
            new(ClaimTypes.Name,              user.UserName!),
            new(ClaimTypes.Email,             user.Email!)
        };

        if (!string.IsNullOrEmpty(role))
            claims.Add(new Claim(ClaimTypes.Role, role));

        return claims;
    }

    private SigningCredentials BuildSigningCredentials()
        => new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_issuerSigningKey)),
            SecurityAlgorithms.HmacSha256);
}