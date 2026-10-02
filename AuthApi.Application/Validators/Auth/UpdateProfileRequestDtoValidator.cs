using AuthApi.Application.DTOs;
using FluentValidation;

namespace AuthApi.Application.Validators.Auth;

public class UpdateProfileRequestDtoValidator : AbstractValidator<UpdateProfileRequestDto>
{
    public UpdateProfileRequestDtoValidator()
    {
        // الحقول اختيارية، لكن إذا أُرسلت يجب أن تكون صالحة
        RuleFor(x => x.FirstName)
            .MinimumLength(2).WithMessage("الاسم الأول قصير جداً")
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MinimumLength(2).WithMessage("الاسم الأخير قصير جداً")
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.PhoneNo)
            .MaximumLength(30)
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNo));

        RuleFor(x => x.City)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.Address)
            .MaximumLength(300)
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.ZipCode)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.ZipCode));
    }
}
//.When(...) = طبّق القاعدة فقط إذا المستخدم أرسل قيمة.