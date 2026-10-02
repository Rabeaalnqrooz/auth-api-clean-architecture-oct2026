using AuthApi.Application.DTOs;
using AuthApi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthApi.API.Controllers;

[ApiController]
[Route("api/v1/user")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IHostEnvironment _env;

    public AuthController(AuthService authService, IHostEnvironment env)
    {
        _authService = authService;
        _env = env;
    }

    // إعدادات الكوكي حسب البيئة (ملاحظة 5)
    private CookieOptions AccessCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = !_env.IsDevelopment(),
        SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
        Path = "/",
        Expires = DateTime.UtcNow.AddMinutes(15)
    };

    private CookieOptions RefreshCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = !_env.IsDevelopment(),
        SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
        Path = "/",
        Expires = DateTime.UtcNow.AddDays(7)
    };

    private CookieOptions DeleteCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = !_env.IsDevelopment(),
        SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
        Path = "/"
    };

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var user = await _authService.RegisterAsync(request);
        return Ok(new { success = true, user });
    }

    [HttpGet("verify/{token}")]
    public async Task<IActionResult> VerifyEmail(string token)
    {
        await _authService.VerifyEmailAsync(token);
        return Ok(new { success = true, message = "تم تأكيد البريد الإلكتروني بنجاح" });
    }

    [EnableRateLimiting("login")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var (user, accessToken, refreshToken) = await _authService.LoginAsync(request);

        Response.Cookies.Append("accessToken", accessToken, AccessCookieOptions());
        Response.Cookies.Append("refreshToken", refreshToken, RefreshCookieOptions());

        return Ok(new { success = true, user });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            throw new UnauthorizedAccessException("انتهت الجلسة");

        var (access, refresh) = await _authService.RefreshTokenAsync(refreshToken);

        Response.Cookies.Append("accessToken", access, AccessCookieOptions());
        Response.Cookies.Append("refreshToken", refresh, RefreshCookieOptions());

        return Ok(new { success = true, message = "تم تجديد الجلسة" });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync(Request.Cookies["refreshToken"]);
        Response.Cookies.Delete("accessToken", DeleteCookieOptions());
        Response.Cookies.Delete("refreshToken", DeleteCookieOptions());
        return Ok(new { success = true, message = "تم تسجيل الخروج بنجاح" });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var user = await _authService.GetMeAsync(GetCurrentUserId());
        return Ok(new { success = true, user });
    }

    [EnableRateLimiting("otp")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok(new { success = true, message = "إذا كان الإيميل موجوداً، سيتم إرسال رمز التحقق" });
    }

    [EnableRateLimiting("otp")]
    [HttpPost("verify-reset-otp")]
    public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyOtpRequestDto request)
    {
        await _authService.VerifyResetOtpAsync(request);
        return Ok(new { success = true, message = "رمز التحقق صحيح" });
    }

    [EnableRateLimiting("otp")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok(new { success = true, message = "تم تغيير كلمة المرور بنجاح" });
    }

    [EnableRateLimiting("otp")]
    [HttpPost("resend-reset-otp")]
    public async Task<IActionResult> ResendResetOtp([FromBody] ForgotPasswordRequestDto request)
    {
        await _authService.ResendResetOtpAsync(request);
        return Ok(new { success = true, message = "إذا كان الإيميل موجوداً، سيتم إرسال رمز تحقق جديد" });
    }

    [Authorize]
    [HttpPut("update-password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequestDto request)
    {
        await _authService.UpdatePasswordAsync(GetCurrentUserId(), request);
        return Ok(new { success = true, message = "تم تحديث كلمة المرور بنجاح" });
    }

    [Authorize]
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromForm] UpdateProfileRequestDto request,
        IFormFile? profilePic)
    {
        Stream? stream = null;
        string? fileName = null;

        if (profilePic is { Length: > 0 })
        {
            stream = profilePic.OpenReadStream();
            fileName = profilePic.FileName;
        }

        var user = await _authService.UpdateProfileAsync(
            GetCurrentUserId(), request, stream, fileName);

        return Ok(new { success = true, user });
    }

    private Guid GetCurrentUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
              ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var userId))
            throw new UnauthorizedAccessException("مستخدم غير صالح");

        return userId;
    }
}