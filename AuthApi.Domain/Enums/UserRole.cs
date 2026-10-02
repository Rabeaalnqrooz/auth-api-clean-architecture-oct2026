namespace AuthApi.Domain.Enums;

/// <summary>
/// صلاحية المستخدم داخل النظام.
/// ملاحظة مهمة: Disabled ليست Role.
/// حالة الحساب تُدار عبر UserStatus.
/// </summary>
public enum UserRole
{
    User = 0,
    Admin = 1
}