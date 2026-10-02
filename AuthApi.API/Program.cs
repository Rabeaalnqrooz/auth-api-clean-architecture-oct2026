using AuthApi.API.Middleware;
using AuthApi.Application.Options;
using AuthApi.Application.Services;
using AuthApi.Application.Validators.Auth;
using AuthApi.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthApi.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ---------- Controllers + OpenAPI + Validation ----------
        builder.Services.AddControllers();
        builder.Services.AddOpenApi(); // .NET 9/10 — بدون Swagger UI التقليدي

        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestDtoValidator>();

        // ---------- Options + ValidateOnStart (ملاحظة 7) ----------
        builder.Services.AddOptions<JwtSettings>()
            .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
            .ValidateOnStart();
        builder.Services.AddSingleton<IValidateOptions<JwtSettings>, JwtSettingsValidator>();

        builder.Services.AddOptions<EmailSettings>()
            .Bind(builder.Configuration.GetSection(EmailSettings.SectionName))
            .ValidateOnStart();
        builder.Services.AddSingleton<IValidateOptions<EmailSettings>, EmailSettingsValidator>();

        builder.Services.AddOptions<CloudinarySettings>()
            .Bind(builder.Configuration.GetSection(CloudinarySettings.SectionName))
            .ValidateOnStart();
        builder.Services.AddSingleton<IValidateOptions<CloudinarySettings>, CloudinarySettingsValidator>();

        // ---------- Infrastructure + Application services ----------
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<AdminService>();

        // ---------- JWT ----------
        var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                  ?? throw new InvalidOperationException("Jwt settings missing");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.AccessSecret)),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = JwtRegisteredClaimNames.Sub
                };

                // قراءة Access Token من الكوكي
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue("accessToken", out var token))
                            context.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization();

        // ---------- CORS ----------
        var frontendUrl = builder.Configuration["FrontendUrl"] ?? "http://localhost:5173";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(frontendUrl)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // ---------- Rate limiting (طبقة إضافية مع OTP/Login) ----------
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("login", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("otp", opt =>
            {
                opt.PermitLimit = 3;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueLimit = 0;
            });

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(
                    """{"success":false,"detail":"محاولات كثيرة، حاول بعد دقيقة"}""",
                    token);
            };
        });

        var app = builder.Build();
   
        // ---------- Pipeline ----------
        app.UseMiddleware<ExceptionMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi(); // وثائق OpenAPI على /openapi/v1.json مثلاً
        }

        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseCors("AllowFrontend");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}