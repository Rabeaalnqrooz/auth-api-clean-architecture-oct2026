namespace AuthApi.Application.DTOs;

/// <summary>
/// بيانات تسجيل الدخول.
/// </summary>
public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}