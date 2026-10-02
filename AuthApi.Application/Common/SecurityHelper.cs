using System.Security.Cryptography;
using System.Text;

namespace AuthApi.Application.Common;

/// <summary>
/// دوال مساعدة: توليد توكنات عشوائية + هاش SHA256.
/// </summary>
public static class SecurityHelper
{
    /// <summary>
    /// توكن عشوائي (تحقق إيميل / Refresh Token).
    /// bytes=32 افتراضي، ولـ Refresh يُفضّل 64.
    /// </summary>
    public static string GenerateRandomToken(int bytes = 32)
    {
        var randomBytes = new byte[bytes];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToHexString(randomBytes).ToLowerInvariant();
    }

    /// <summary>
    /// SHA256 — لتخزين Email token / OTP / Refresh Token Hash.
    /// </summary>
    public static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// OTP من 6 أرقام.
    /// </summary>
    public static string GenerateOtp()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    }
}