using AuthApi.Domain.Entities;

namespace AuthApi.Application.Interfaces;

/// <summary>
/// إنشاء Access Token (JWT).
/// Refresh أصبح Random + DB، لكن نبقي الواجهة مرنة.
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(User user);
}