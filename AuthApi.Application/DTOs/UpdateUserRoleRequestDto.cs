namespace AuthApi.Application.DTOs;

/// <summary>
/// للأدمن: تغيير الدور إلى user أو admin فقط (ليس status).
/// </summary>
public class UpdateUserRoleRequestDto
{
    public string Role { get; set; } = string.Empty;
}