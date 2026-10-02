using AuthApi.Application.Interfaces;
using AuthApi.Infrastructure.Persistence;
using AuthApi.Infrastructure.Repositories;
using AuthApi.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthApi.Infrastructure;

/// <summary>
/// نقطة تسجيل خدمات Infrastructure في DI.
/// تُستدعى من Program.cs: builder.Services.AddInfrastructure(...)
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ----- DbContext + SQL Server -----
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        // ----- Repositories -----
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // ----- Infrastructure services -----
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();

        return services;
    }
}