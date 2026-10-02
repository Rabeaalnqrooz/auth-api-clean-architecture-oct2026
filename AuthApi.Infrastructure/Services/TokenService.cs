using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthApi.Application.Interfaces;
using AuthApi.Application.Options;
using AuthApi.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Infrastructure.Services;

/// <summary>
/// إنشاء Access Token (JWT قصير العمر).
/// Refresh Token عندنا Random + DB وليس JWT طويل الأمد.
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwt;

    public TokenService(IOptions<JwtSettings> jwtOptions)
    {
        // تأتي من appsettings بعد ValidateOnStart
        _jwt = jwtOptions.Value;
    }

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwt.AccessSecret));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        // Claims تُقرأ لاحقاً في [Authorize] و GetCurrentUserId
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()), // "Admin" / "User"
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessExpiresInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
/// <summary>
/// Sub = UserIdلاستخراج المستخدم الحالي
/// ClaimTypes.Roleحتى [Authorize(Roles = "Admin")] يشتغل
/// مدة قصيرة (15 د)تقليل ضرر سرقة الـ Access Token
/// Refresh منفصلملاحظة 2: قابل للإلغاء في DB
/// </summary>