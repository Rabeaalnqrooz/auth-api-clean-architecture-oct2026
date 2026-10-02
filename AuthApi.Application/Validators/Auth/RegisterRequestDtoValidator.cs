using AuthApi.Application.DTOs;
using FluentValidation;

namespace AuthApi.Application.Validators.Auth;

/// <summary>
/// قواعد التحقق من بيانات التسجيل.
/// </summary>
public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        // الاسم الأول: مطلوب + حد أدنى معقول
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("الاسم الأول مطلوب")
            .MinimumLength(2).WithMessage("الاسم الأول يجب أن يكون حرفين على الأقل")
            .MaximumLength(50).WithMessage("الاسم الأول طويل جداً");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("الاسم الأخير مطلوب")
            .MinimumLength(2).WithMessage("الاسم الأخير يجب أن يكون حرفين على الأقل")
            .MaximumLength(50).WithMessage("الاسم الأخير طويل جداً");

        // صيغة إيميل صحيحة
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة")
            .MaximumLength(200);

        // باسورد: يمنع "1" أو فاضي
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة")
            .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل")
            .MaximumLength(100);
    }
}
/// <summary>
///NotEmptyمش null ومش فاضي
///EmailAddressشكل إيميل مقبول
///MinimumLength(8)الحد الأدنى لكلمة المرور
///WithMessageرسالة تظهر في رد 400
/// </summary>