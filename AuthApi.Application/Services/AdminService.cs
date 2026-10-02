using AuthApi.Application.DTOs;
using AuthApi.Application.Exceptions;
using AuthApi.Application.Interfaces;
using AuthApi.Domain.Enums;

namespace AuthApi.Application.Services;

/// <summary>
/// عمليات الأدمن على المستخدمين.
/// Disable/Enable تعدّل Status فقط — لا تلمس Role (ملاحظة 4).
/// </summary>
public class AdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AdminService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<PagedUsersResponseDto> GetPagedUsersAsync(int page = 1, int limit = 10)
    {
        var (users, total) = await _userRepository.GetPagedAsync(page, limit);

        return new PagedUsersResponseDto
        {
            Users = users.Select(MapToUserDto),
            Total = total,
            Page = page,
            Limit = limit,
            Pages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit)
        };
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        return MapToUserDto(user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        await _refreshTokenRepository.RevokeAllForUserAsync(id);
        await _userRepository.DeleteAsync(user);
    }

    public async Task UpdateUserRoleAsync(Guid id, UpdateUserRoleRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var newRole))
            throw new BadRequestException("الدور غير صالح. يجب أن يكون User أو Admin");

        // Disabled ليس Role
        user.Role = newRole;
        await _userRepository.UpdateAsync(user);
    }

    public async Task DisableUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        // Role يبقى كما هو (Admin يبقى Admin)
        await _refreshTokenRepository.RevokeAllForUserAsync(id);
        user.Status = UserStatus.Disabled;
        await _userRepository.UpdateAsync(user);
    }

    public async Task EnableUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        // لا نرجع Role إلى User — نفعّل الحالة فقط
        user.Status = UserStatus.Active;
        user.LockoutEnd = null;
        user.FailedLoginAttempts = 0;
        await _userRepository.UpdateAsync(user);
    }

    private static UserDto MapToUserDto(Domain.Entities.User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Role = user.Role.ToString().ToLowerInvariant(),
        Status = user.Status.ToString().ToLowerInvariant(),
        IsVerified = user.IsVerified,
        ProfilePic = user.ProfilePic,
        Address = user.Address,
        City = user.City,
        ZipCode = user.ZipCode,
        PhoneNo = user.PhoneNo,
        CreatedAt = user.CreatedAt
    };
}