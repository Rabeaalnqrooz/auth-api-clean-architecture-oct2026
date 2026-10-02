using AuthApi.Application.DTOs;
using FluentValidation;

namespace AuthApi.Application.Validators.Auth;

public class UpdatePasswordRequestDtoValidator : AbstractValidator<UpdatePasswordRequestDto>
{
    public UpdatePasswordRequestDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("كلمة المرور الحالية مطلوبة");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة")
            .MinimumLength(8).WithMessage("كلمة المرور الجديدة يجب أن تكون 8 أحرف على الأقل")
            .MaximumLength(100);
    }
}