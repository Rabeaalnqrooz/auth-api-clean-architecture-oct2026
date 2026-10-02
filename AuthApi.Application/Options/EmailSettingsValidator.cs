using Microsoft.Extensions.Options;

namespace AuthApi.Application.Options;

public class EmailSettingsValidator : IValidateOptions<EmailSettings>
{
    public ValidateOptionsResult Validate(string? name, EmailSettings options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.SmtpServer))
            errors.Add("EmailSettings:SmtpServer مطلوب");

        if (options.SmtpPort <= 0)
            errors.Add("EmailSettings:SmtpPort غير صالح");

        if (string.IsNullOrWhiteSpace(options.SenderEmail))
            errors.Add("EmailSettings:SenderEmail مطلوب");

        if (string.IsNullOrWhiteSpace(options.Username))
            errors.Add("EmailSettings:Username مطلوب");

        if (string.IsNullOrWhiteSpace(options.Password))
            errors.Add("EmailSettings:Password مطلوب");

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}