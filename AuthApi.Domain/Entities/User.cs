using AuthApi.Domain.Enums;

namespace AuthApi.Domain.Entities;

/// <summary>
/// كيان المستخدم الأساسي في النظام.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // -------------------- بيانات أساسية --------------------
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>هاش كلمة المرور — لا نخزّن النص الأصلي أبداً.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    // -------------------- صلاحية + حالة (مفصولة) --------------------
    public UserRole Role { get; set; } = UserRole.User;
    public UserStatus Status { get; set; } = UserStatus.Active;

    public bool IsVerified { get; set; } = false;

    // -------------------- تحقق الإيميل --------------------
    public string? EmailVerifyToken { get; set; }
    public DateTime? EmailVerifyTokenExpiry { get; set; }

    // -------------------- OTP لإعادة تعيين كلمة المرور --------------------
    public string? Otp { get; set; }
    public DateTime? OtpExpiry { get; set; }

    /// <summary>محاولات OTP الفاشلة — ضد التخمين (ملاحظة 8).</summary>
    public int FailedOtpAttempts { get; set; } = 0;

    // -------------------- أمان تسجيل الدخول --------------------
    /// <summary>محاولات الدخول الفاشلة المتتالية.</summary>
    public int FailedLoginAttempts { get; set; } = 0;

    /// <summary>نهاية فترة القفل المؤقت إن وُجدت.</summary>
    public DateTime? LockoutEnd { get; set; }

    // -------------------- الملف الشخصي --------------------
    public string? ProfilePic { get; set; }
    public string? ProfilePublicId { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? PhoneNo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // -------------------- علاقات --------------------
    /// <summary>جلسات Refresh Token المرتبطة بالمستخدم.</summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}