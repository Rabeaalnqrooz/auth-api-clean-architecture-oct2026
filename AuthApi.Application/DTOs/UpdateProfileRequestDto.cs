namespace AuthApi.Application.DTOs;

/// <summary>
/// تحديث الملف الشخصي.
/// الصورة تُرسل كـ IFormFile من الـ Controller منفصلة عن هذا الـ DTO.
/// </summary>
public class UpdateProfileRequestDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? PhoneNo { get; set; }
}