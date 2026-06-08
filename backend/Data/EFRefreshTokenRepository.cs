using Microsoft.EntityFrameworkCore;
using Lexicon.Model;

namespace Lexicon.Data;

public class EFRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly LexiconDbContext _context;

    public EFRefreshTokenRepository(LexiconDbContext context) => _context = context;

    public Task<RefreshToken?> GetByTokenAsync(string token) =>
        _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);

    public async Task AddAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAsync(RefreshToken refreshToken)
    {
        refreshToken.IsRevoked = true;
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllForUserAsync(string userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
            token.IsRevoked = true;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredAndRevokedAsync(string userId)
    {
        var stale = await _context.RefreshTokens
            .Where(r => r.UserId == userId && (r.IsRevoked || r.ExpiresAt < DateTime.UtcNow))
            .ToListAsync();
        _context.RefreshTokens.RemoveRange(stale);
        await _context.SaveChangesAsync();
    }
}
