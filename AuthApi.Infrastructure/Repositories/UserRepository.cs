using AuthApi.Application.Interfaces;
using AuthApi.Domain.Entities;
using AuthApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Infrastructure.Repositories;

/// <summary>
/// تنفيذ الوصول لجدول Users عبر EF Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    // نحقن DbContext (مسجّل لاحقاً كـ Scoped)
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        // FirstOrDefaultAsync ترجع null إذا ما لقت
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        // نوحّد الإيميل لـ lower حتى البحث ثابت
        var normalized = email.Trim().ToLowerInvariant();

        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalized);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        // تأكد أن الإيميل مخزّن بشكل موحّد
        user.Email = user.Email.Trim().ToLowerInvariant();

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        // بسبب Cascade: RefreshTokens المرتبطة تُحذف تلقائياً
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return await _context.Users
            .AnyAsync(u => u.Email == normalized);
    }

    public async Task<User?> GetByEmailVerifyTokenAsync(string hashedToken)
    {
        // نبحث بالهاش وليس بالتوكن الخام
        return await _context.Users
            .FirstOrDefaultAsync(u => u.EmailVerifyToken == hashedToken);
    }

    public async Task<(IEnumerable<User> Users, int Total)> GetPagedAsync(int page, int limit)
    {
        // حماية بسيطة من قيم غير منطقية
        if (page < 1) page = 1;
        if (limit < 1) limit = 10;
        if (limit > 100) limit = 100;

        var query = _context.Users.AsQueryable();

        var total = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (users, total);
    }
}
/// <summary>
/// GetByEmailAsyncLogin / Forgot / OTP
/// ExistsByEmailAsyncRegister بدون جلب الصف كاملاً
/// GetByEmailVerifyTokenAsyncتأكيد الإيميل
/// GetPagedAsyncAdmin مع Skip/Take
/// SaveChangesAsyncيحفظ التغييرات فعلياً في SQL
/// </summary>