using AuthApi.Application.Interfaces;
using AuthApi.Domain.Entities;
using AuthApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Infrastructure.Repositories;

/// <summary>
/// إدارة جلسات Refresh Token (تخزين هاش + إلغاء + Rotation).
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        // Include User: مفيد في Refresh لفحص Status بدون استعلام إضافي
        return await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId)
    {
        return await _context.RefreshTokens
            .Where(x =>
                x.UserId == userId &&
                x.RevokedAt == null &&
                x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task RevokeAllForUserAsync(Guid userId)
    {
        // نجلب الجلسات غير الملغاة فقط
        var tokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync();

        var now = DateTime.UtcNow;

        foreach (var token in tokens)
        {
            token.RevokedAt = now;
        }

        await _context.SaveChangesAsync();
    }
}