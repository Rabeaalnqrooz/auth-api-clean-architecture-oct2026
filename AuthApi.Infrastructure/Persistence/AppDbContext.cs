using AuthApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Infrastructure.Persistence;

/// <summary>
/// بوابة EF Core إلى قاعدة البيانات.
/// كل DbSet = جدول.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =========================
        // User
        // =========================
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);

            // الإيميل مطلوب + فريد (ما يتكرر)
            entity.Property(x => x.Email)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.HasIndex(x => x.Email)
                  .IsUnique();

            entity.Property(x => x.FirstName)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(x => x.LastName)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(x => x.PasswordHash)
                  .IsRequired();

            // Role و Status تُخزَّن كأرقام enum افتراضياً
            entity.Property(x => x.Role)
                  .IsRequired();

            entity.Property(x => x.Status)
                  .IsRequired();

            entity.Property(x => x.ProfilePic)
                  .HasMaxLength(500);

            entity.Property(x => x.ProfilePublicId)
                  .HasMaxLength(200);

            entity.Property(x => x.Address)
                  .HasMaxLength(300);

            entity.Property(x => x.City)
                  .HasMaxLength(100);

            entity.Property(x => x.ZipCode)
                  .HasMaxLength(20);

            entity.Property(x => x.PhoneNo)
                  .HasMaxLength(30);

            // علاقة: User 1 → * RefreshTokens
            entity.HasMany(x => x.RefreshTokens)
                  .WithOne(x => x.User)
                  .HasForeignKey(x => x.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            // Cascade: حذف المستخدم يحذف جلساته
        });

        // =========================
        // RefreshToken
        // =========================
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(x => x.Id);

            // الهاش فريد — كل توكن صف واحد
            entity.Property(x => x.TokenHash)
                  .IsRequired()
                  .HasMaxLength(128);

            entity.HasIndex(x => x.TokenHash)
                  .IsUnique();

            entity.Property(x => x.ReplacedByTokenHash)
                  .HasMaxLength(128);

            entity.Property(x => x.CreatedByIp)
                  .HasMaxLength(64);

            // IsActive خاصية حسابية — لا نحتاج عموداً لها
            entity.Ignore(x => x.IsActive);
        });
    }
}