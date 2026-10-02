# Auth API Template (.NET 10 — Clean Architecture)

قالب مصادقة جاهز لإعادة الاستخدام مبني بـ Clean Architecture.

## المميزات

- ASP.NET Core Web API (.NET 10)
- JWT في HttpOnly Cookies
- Refresh Token مخزّن في قاعدة البيانات (Rotation + Revocation)
- تسجيل + تأكيد إيميل + نسيت كلمة المرور (OTP)
- FluentValidation
- Options مع ValidateOnStart
- فصل Role و Status
- Rate limiting على Login / OTP
- ExceptionMiddleware مع ProblemDetails

## هيكل المشاريع

| المشروع | الوظيفة |
|---------|---------|
| AuthApi.Domain | Entities + Enums |
| AuthApi.Application | DTOs, Interfaces, Services, Validators |
| AuthApi.Infrastructure | EF Core, Email, Cloudinary, JWT |
| AuthApi.API | Controllers + Middleware |

## المتطلبات

- .NET 10 SDK
- SQL Server / SQLEXPRESS / LocalDB
- (اختياري) SMTP لإرسال الإيميل
- (اختياري) حساب Cloudinary لصور البروفايل

## الإعداد

1. انسخ المستودع أو استخدم **Use this template**
2. اضبط الأسرار عبر **User Secrets** أو `appsettings.Development.json` (غير مرفوع على Git)
3. حدّث Connection String حسب جهازك

### مثال User Secrets

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=AuthApiDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False"
  },
  "Jwt": {
    "AccessSecret": "your-long-secret-at-least-32-chars",
    "RefreshSecret": "your-long-refresh-secret-32-chars-min"
  },
  "EmailSettings": {
    "SenderEmail": "...",
    "Username": "...",
    "Password": "..."
  },
  "CloudinarySettings": {
    "CloudName": "...",
    "ApiKey": "...",
    "ApiSecret": "..."
  }
}