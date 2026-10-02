namespace AuthApi.Domain.Enums;

/// <summary>
/// حالة الحساب — منفصلة عن الصلاحية (Role).
/// </summary>
public enum UserStatus
{
    /// <summary>الحساب يعمل ويمكنه تسجيل الدخول.</summary>
    Active = 0,

    /// <summary>معطّل يدوياً من الأدمن.</summary>
    Disabled = 1,

    /// <summary>مقفل مؤقتاً بعد محاولات دخول فاشلة.</summary>
    Locked = 2
}