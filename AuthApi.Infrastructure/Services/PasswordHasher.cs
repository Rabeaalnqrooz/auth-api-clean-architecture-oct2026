using AuthApi.Application.Interfaces;

namespace AuthApi.Infrastructure.Services;

/// <summary>
/// تنفيذ هاش كلمة المرور باستخدام BCrypt.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    // كلما زاد workFactor زادت القوة وزاد الزمن (11 شائع)
    private const int WorkFactor = 11;

    public string Hash(string password)
    {
        // BCrypt يولد Salt داخلياً مع الهاش
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string passwordHash)
    {
        // true إذا تطابق الباسورد مع الهاش
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}