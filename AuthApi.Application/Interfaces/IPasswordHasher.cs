namespace AuthApi.Application.Interfaces;

/// <summary>
/// تجريد هاش كلمة المرور (BCrypt لاحقاً في Infrastructure).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}