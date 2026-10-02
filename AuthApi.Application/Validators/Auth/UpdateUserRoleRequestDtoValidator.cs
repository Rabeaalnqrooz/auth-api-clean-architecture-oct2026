using AuthApi.Application.DTOs;
using FluentValidation;

namespace AuthApi.Application.Validators.Auth;

public class UpdateUserRoleRequestDtoValidator : AbstractValidator<UpdateUserRoleRequestDto>
{
    public UpdateUserRoleRequestDtoValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("الدور مطلوب")
            .Must(r => r.Equals("user", StringComparison.OrdinalIgnoreCase)
                     || r.Equals("admin", StringComparison.OrdinalIgnoreCase))
            .WithMessage("الدور يجب أن يكون user أو admin");
    }
}
//disabled ليس Role — يُدار عبر Status من مسارات Enable/Disable.