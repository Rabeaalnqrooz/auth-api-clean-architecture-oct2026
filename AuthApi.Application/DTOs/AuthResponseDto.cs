namespace AuthApi.Application.DTOs;

/// <summary>
/// ما نرجعه للعميل بعد Register/Login (بدون توكنات في الـ Body).
/// التوكنات تُرسل في HttpOnly Cookies من الـ API.
/// </summary>
public class AuthResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public string Status { get; set; } = "active";
    public bool IsVerified { get; set; }
    public string? ProfilePic { get; set; }
}