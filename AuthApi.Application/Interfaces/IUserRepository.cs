using AuthApi.Domain.Entities;

namespace AuthApi.Application.Interfaces;

/// <summary>
/// عقد الوصول لبيانات المستخدمين.
/// التنفيذ الفعلي في Infrastructure بـ EF Core.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<bool> ExistsByEmailAsync(string email);

    /// <summary>البحث بتوكن تأكيد الإيميل (المخزّن كهاش).</summary>
    Task<User?> GetByEmailVerifyTokenAsync(string hashedToken);

    /// <summary>قائمة مع ترقيم صفحات للأدمن.</summary>
    Task<(IEnumerable<User> Users, int Total)> GetPagedAsync(int page, int limit);
}