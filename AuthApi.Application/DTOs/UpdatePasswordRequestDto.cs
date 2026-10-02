namespace AuthApi.Application.DTOs;

/// <summary>
/// تغيير كلمة المرور والمستخدم مسجّل دخول.
/// </summary>
public class UpdatePasswordRequestDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}