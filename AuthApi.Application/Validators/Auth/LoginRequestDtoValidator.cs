using AuthApi.Application.DTOs;
using FluentValidation;

namespace AuthApi.Application.Validators.Auth;

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة");

        // في Login لا نفرض الطول بقوة حتى لا نسهّل التخمين برسائل مختلفة
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة");
    }
}