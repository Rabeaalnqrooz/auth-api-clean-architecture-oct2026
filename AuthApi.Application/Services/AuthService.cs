using AuthApi.Application.Common;
using AuthApi.Application.DTOs;
using AuthApi.Application.Exceptions;
using AuthApi.Application.Interfaces;
using AuthApi.Domain.Entities;
using AuthApi.Domain.Enums;

namespace AuthApi.Application.Services;

/// <summary>
/// كل منطق المصادقة: تسجيل، دخول، توكنات، OTP، بروفايل.
/// Controllers تستدعي هذه الخدمة فقط — بدون try/catch داخلي.
/// </summary>
public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    // هاش BCrypt حقيقي فقط لموازنة زمن Verify عندما الإيميل غير موجود (ملاحظة 8)
    // استبدله بناتج: _passwordHasher.Hash("DummyPassword_ForTimingOnly_X9!")
    private const string DummyPasswordHash =
        "$2a$11$hCt7./yxrlU1gC0YFRaLgOKbnagDyZDpb1k4m/bzS7z2Amz3aJgju";

    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
    private const int MaxFailedLoginAttempts = 5;
    private const int MaxFailedOtpAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        ICloudinaryService cloudinaryService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
        _cloudinaryService = cloudinaryService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    // =========================================================
    // 1) التسجيل
    // =========================================================
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // فحص التكرار على مستوى العمل (FluentValidation فحصت الشكل مسبقاً)
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            throw new BadRequestException("البريد الإلكتروني مستخدم بالفعل");

        // توكن التحقق: خام للإيميل + هاش في DB
        var rawToken = SecurityHelper.GenerateRandomToken();
        var hashedToken = SecurityHelper.HashToken(rawToken);
  
        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.ToLower().Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            EmailVerifyToken = hashedToken,
            EmailVerifyTokenExpiry = DateTime.UtcNow.AddHours(24),
            Role = UserRole.User,           // صلاحية فقط
            Status = UserStatus.Active,     // حالة الحساب منفصلة (ملاحظة 4)
            IsVerified = false
        };

        await _userRepository.AddAsync(user);

        // نرسل التوكن الخام — في DB مخزّن الهاش فقط
        await _emailService.SendEmailVerificationAsync(
            user.Email, user.FirstName, rawToken);

        return MapToAuthResponse(user);
    }

    // =========================================================
    // 2) تأكيد الإيميل
    // =========================================================
    public async Task VerifyEmailAsync(string rawToken)
    {
        var hashedToken = SecurityHelper.HashToken(rawToken);
        var user = await _userRepository.GetByEmailVerifyTokenAsync(hashedToken);

        if (user == null)
            throw new BadRequestException("رابط التحقق غير صالح");

        if (user.EmailVerifyTokenExpiry < DateTime.UtcNow)
            throw new BadRequestException("رابط التحقق منتهي الصلاحية");

        user.IsVerified = true;
        user.EmailVerifyToken = null;       // استهلاك التوكن
        user.EmailVerifyTokenExpiry = null;

        await _userRepository.UpdateAsync(user);
    }

    // =========================================================
    // 3) تسجيل الدخول (timing + lockout + refresh في DB)
    // =========================================================
    public async Task<(AuthResponseDto User, string AccessToken, string RefreshToken)> LoginAsync(
        LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        // قفل زمني ساري
        if (user != null && user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            throw new UnauthorizedException("الحساب مقفل مؤقتاً. حاول لاحقاً");

        // دائماً Verify — حتى لو المستخدم null (ضد timing attack — ملاحظة 8)
        var hashToVerify = user?.PasswordHash ?? DummyPasswordHash;
        var passwordOk = _passwordHasher.Verify(request.Password, hashToVerify);

        if (user == null || !passwordOk)
        {
            if (user != null)
            {
                user.FailedLoginAttempts += 1;

                if (user.FailedLoginAttempts >= MaxFailedLoginAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                    user.Status = UserStatus.Locked;
                    user.FailedLoginAttempts = 0;
                }

                await _userRepository.UpdateAsync(user);
            }

            // رسالة موحّدة — لا تكشف إن الإيميل موجود
            throw new UnauthorizedException("بيانات الدخول غير صحيحة");
        }

        if (user.Status == UserStatus.Disabled)
            throw new UnauthorizedException("هذا الحساب معطل");

        if (user.Status == UserStatus.Locked &&
            user.LockoutEnd.HasValue &&
            user.LockoutEnd > DateTime.UtcNow)
            throw new UnauthorizedException("الحساب مقفل مؤقتاً. حاول لاحقاً");

        if (!user.IsVerified)
            throw new BadRequestException("يرجى تأكيد بريدك الإلكتروني أولاً");

        // نجاح: صفّر المحاولات
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        if (user.Status == UserStatus.Locked)
            user.Status = UserStatus.Active;

        await _userRepository.UpdateAsync(user);

        // Access = JWT قصير
        var accessToken = _tokenService.GenerateAccessToken(user);

        // Refresh = عشوائي + هاش في DB (ملاحظة 2)
        var rawRefresh = SecurityHelper.GenerateRandomToken(64);
        var refreshHash = SecurityHelper.HashToken(rawRefresh);

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshHash,
            ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime),
            CreatedAt = DateTime.UtcNow
        });

        // الخام يذهب للكوكي — الهاش فقط في DB
        return (MapToAuthResponse(user), accessToken, rawRefresh);
    }

    // =========================================================
    // 4) تجديد التوكن (Rotation + كشف إعادة الاستخدام)
    // =========================================================
    public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(
        string incomingRawRefreshToken)
    {
        if (string.IsNullOrWhiteSpace(incomingRawRefreshToken))
            throw new UnauthorizedException("انتهت الجلسة");

        var incomingHash = SecurityHelper.HashToken(incomingRawRefreshToken);
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(incomingHash);

        if (stored == null)
            throw new UnauthorizedException("جلسة غير صالحة، يرجى تسجيل الدخول مجدداً");

        // توكن ملغي يُعاد استخدامه → احتمال سرقة: ألغِ كل الجلسات
        if (stored.RevokedAt != null)
        {
            await _refreshTokenRepository.RevokeAllForUserAsync(stored.UserId);
            throw new UnauthorizedException("تم اكتشاف إعادة استخدام توكن. سجّل الدخول مجدداً");
        }

        if (stored.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("انتهت صلاحية الجلسة، يرجى تسجيل الدخول مجدداً");

        var user = stored.User ?? await _userRepository.GetByIdAsync(stored.UserId);

        if (user == null || user.Status != UserStatus.Active)
            throw new UnauthorizedException("الحساب غير موجود أو معطل");

        // Rotation: ألغِ القديم + أنشئ جديداً
        var newRaw = SecurityHelper.GenerateRandomToken(64);
        var newHash = SecurityHelper.HashToken(newRaw);

        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByTokenHash = newHash;

        await _refreshTokenRepository.UpdateAsync(stored);
        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newHash,
            ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime),
            CreatedAt = DateTime.UtcNow
        });

        var newAccess = _tokenService.GenerateAccessToken(user);
        return (newAccess, newRaw);
    }

    // =========================================================
    // 5) نسيت كلمة المرور
    // =========================================================
    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        // لا نكشف إن الإيميل موجود — نخرج بصمت
        if (user == null)
            return;

        var rawOtp = SecurityHelper.GenerateOtp();
        var hashedOtp = SecurityHelper.HashToken(rawOtp);

        user.Otp = hashedOtp;
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
        user.FailedOtpAttempts = 0;

        await _userRepository.UpdateAsync(user);
        await _emailService.SendOtpAsync(user.Email, user.FirstName, rawOtp);
    }

    // =========================================================
    // 6) التحقق من OTP (مع عداد محاولات)
    // =========================================================
    public async Task VerifyResetOtpAsync(VerifyOtpRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
            throw new BadRequestException("طلب غير صالح");

        if (user.FailedOtpAttempts >= MaxFailedOtpAttempts)
            throw new BadRequestException("تم تجاوز عدد المحاولات. اطلب رمزاً جديداً");

        if (string.IsNullOrEmpty(user.Otp) || user.OtpExpiry < DateTime.UtcNow)
            throw new BadRequestException("رمز التحقق غير صحيح أو منتهي الصلاحية");

        var hashedOtp = SecurityHelper.HashToken(request.Otp);

        if (user.Otp != hashedOtp)
        {
            user.FailedOtpAttempts += 1;
            await _userRepository.UpdateAsync(user);
            throw new BadRequestException("رمز التحقق غير صحيح أو منتهي الصلاحية");
        }

        // نجاح التحقق — الاستهلاك النهائي عند ResetPassword
        user.FailedOtpAttempts = 0;
        await _userRepository.UpdateAsync(user);
    }

    // =========================================================
    // 7) إعادة تعيين كلمة المرور (استهلاك OTP + إلغاء الجلسات)
    // =========================================================
    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
            throw new BadRequestException("طلب غير صالح");

        if (user.FailedOtpAttempts >= MaxFailedOtpAttempts)
            throw new BadRequestException("تم تجاوز عدد المحاولات. اطلب رمزاً جديداً");

        var hashedOtp = SecurityHelper.HashToken(request.Otp);

        if (string.IsNullOrEmpty(user.Otp) ||
            user.Otp != hashedOtp ||
            user.OtpExpiry < DateTime.UtcNow)
        {
            user.FailedOtpAttempts += 1;
            await _userRepository.UpdateAsync(user);
            throw new BadRequestException("رمز التحقق غير صحيح أو منتهي الصلاحية");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.Otp = null;              // استهلاك
        user.OtpExpiry = null;
        user.FailedOtpAttempts = 0;

        await _userRepository.UpdateAsync(user);

        // أي جلسة قديمة تصبح غير صالحة
        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id);
    }

    // =========================================================
    // 8) GetMe
    // =========================================================
    public async Task<UserDto> GetMeAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        return MapToUserDto(user);
    }

    // =========================================================
    // 9) تحديث كلمة المرور (مسجّل دخول)
    // =========================================================
    public async Task UpdatePasswordAsync(Guid userId, UpdatePasswordRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new BadRequestException("كلمة المرور الحالية غير صحيحة");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _userRepository.UpdateAsync(user);

        // اختياري قوي: إلغاء كل الجلسات بعد تغيير الباسورد
        await _refreshTokenRepository.RevokeAllForUserAsync(userId);
    }

    // =========================================================
    // 10) تحديث البروفايل (+ صورة اختيارية)
    // =========================================================
    public async Task<UserDto> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequestDto request,
        Stream? imageStream = null,
        string? imageFileName = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("المستخدم غير موجود");

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName.Trim();

        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName.Trim();

        if (request.Address != null)
            user.Address = request.Address.Trim();

        if (request.City != null)
            user.City = request.City.Trim();

        if (request.ZipCode != null)
            user.ZipCode = request.ZipCode.Trim();

        if (request.PhoneNo != null)
            user.PhoneNo = request.PhoneNo.Trim();

        if (imageStream != null && !string.IsNullOrEmpty(imageFileName))
        {
            var (url, publicId) = await _cloudinaryService.UploadImageAsync(imageStream, imageFileName);
            await _cloudinaryService.DeleteImageAsync(user.ProfilePublicId);
            user.ProfilePic = url;
            user.ProfilePublicId = publicId;
        }

        await _userRepository.UpdateAsync(user);
        return MapToUserDto(user);
    }

    // =========================================================
    // 11) Logout — إلغاء في DB وليس مسح كوكي فقط
    // =========================================================
    public async Task LogoutAsync(string? incomingRawRefreshToken)
    {
        if (string.IsNullOrWhiteSpace(incomingRawRefreshToken))
            return;

        var hash = SecurityHelper.HashToken(incomingRawRefreshToken);
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(hash);

        if (stored == null || stored.RevokedAt != null)
            return;

        stored.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(stored);
    }

    // =========================================================
    // 12) إعادة إرسال OTP
    // =========================================================
    public async Task ResendResetOtpAsync(ForgotPasswordRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            return;

        var rawOtp = SecurityHelper.GenerateOtp();
        var hashedOtp = SecurityHelper.HashToken(rawOtp);

        user.Otp = hashedOtp;
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
        user.FailedOtpAttempts = 0;

        await _userRepository.UpdateAsync(user);
        await _emailService.SendOtpAsync(user.Email, user.FirstName, rawOtp);
    }

    // -------------------- Mapping --------------------
    private static AuthResponseDto MapToAuthResponse(User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Role = user.Role.ToString().ToLowerInvariant(),
        Status = user.Status.ToString().ToLowerInvariant(),
        IsVerified = user.IsVerified,
        ProfilePic = user.ProfilePic
    };

    private static UserDto MapToUserDto(User user) => new()
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