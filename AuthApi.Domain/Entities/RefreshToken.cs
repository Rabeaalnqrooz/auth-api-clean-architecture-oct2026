namespace AuthApi.Domain.Entities;

/// <summary>
/// سجل Refresh Token في قاعدة البيانات.
/// نخزّن الهاش فقط — ليس التوكن الخام.
/// يدعم Rotation وكشف إعادة الاستخدام (ملاحظة 2).
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>SHA256 للتوكن الخام.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>null = ما زال صالحاً / غير ملغي.</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>عند Rotation: هاش التوكن الجديد الذي استبدل هذا.</summary>
    public string? ReplacedByTokenHash { get; set; }

    public string? CreatedByIp { get; set; }

    /// <summary>خاصية حسابية — غير مخزّنة في DB بالضرورة كعمود.</summary>
    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
}