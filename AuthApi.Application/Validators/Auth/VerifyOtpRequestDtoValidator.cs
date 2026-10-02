using AuthApi.Application.DTOs;
using FluentValidation;

namespace AuthApi.Application.Validators.Auth;

public class VerifyOtpRequestDtoValidator : AbstractValidator<VerifyOtpRequestDto>
{
    public VerifyOtpRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة");

        // OTP عندنا 6 أرقام بالضبط
        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("رمز التحقق مطلوب")
            .Length(6).WithMessage("رمز التحقق يجب أن يكون 6 أرقام")
            .Matches(@"^\d{6}$").WithMessage("رمز التحقق يجب أن يكون أرقاماً فقط");
    }
}
//Matches(@"^\d{6}$") = أرقام فقط من 000000 إلى 999999 (طول 6).