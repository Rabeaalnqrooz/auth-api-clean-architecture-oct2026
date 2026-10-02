namespace AuthApi.Application.DTOs;

/// <summary>
/// بيانات المستخدم الكاملة للعرض (GetMe / Admin / Profile).
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public string Status { get; set; } = "active";
    public bool IsVerified { get; set; }
    public string? ProfilePic { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? PhoneNo { get; set; }
    public DateTime CreatedAt { get; set; }
}