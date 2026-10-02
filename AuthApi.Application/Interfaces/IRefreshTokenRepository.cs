using AuthApi.Domain.Entities;

namespace AuthApi.Application.Interfaces;

/// <summary>
/// إدارة جلسات Refresh Token في قاعدة البيانات (ملاحظة 2).
/// </summary>
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId);

    /// <summary>إلغاء كل جلسات المستخدم (Logout من كل الأجهزة / Disable / سرقة توكن).</summary>
    Task RevokeAllForUserAsync(Guid userId);
}