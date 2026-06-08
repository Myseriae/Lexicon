using Lexicon.Model;

namespace Lexicon.Data;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task RevokeAsync(RefreshToken refreshToken);           // sets IsRevoked=true, saves
    Task RevokeAllForUserAsync(string userId);
    Task DeleteExpiredAndRevokedAsync(string userId);     // Q11 cleanup
}
