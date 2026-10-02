using AuthApi.Application.DTOs;
using FluentValidation;

namespace AuthApi.Application.Validators.Auth;

public class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("رمز التحقق مطلوب")
            .Length(6).WithMessage("رمز التحقق يجب أن يكون 6 أرقام")
            .Matches(@"^\d{6}$").WithMessage("رمز التحقق يجب أن يكون أرقاماً فقط");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة")
            .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل")
            .MaximumLength(100);
    }
}