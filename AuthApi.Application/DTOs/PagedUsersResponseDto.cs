namespace AuthApi.Application.DTOs;

/// <summary>
/// رد قائمة المستخدمين مع ترقيم صفحات (Admin).
/// </summary>
public class PagedUsersResponseDto
{
    public IEnumerable<UserDto> Users { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Pages { get; set; }
}