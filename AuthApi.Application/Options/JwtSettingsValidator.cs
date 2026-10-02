using Microsoft.Extensions.Options;

namespace AuthApi.Application.Options;

public class JwtSettingsValidator : IValidateOptions<JwtSettings>
{
    public ValidateOptionsResult Validate(string? name, JwtSettings options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.AccessSecret) || options.AccessSecret.Length < 32)
            errors.Add("Jwt:AccessSecret مطلوب ويجب ألا يقل عن 32 حرفاً");

        if (string.IsNullOrWhiteSpace(options.RefreshSecret) || options.RefreshSecret.Length < 32)
            errors.Add("Jwt:RefreshSecret مطلوب ويجب ألا يقل عن 32 حرفاً");

        if (string.IsNullOrWhiteSpace(options.Issuer))
            errors.Add("Jwt:Issuer مطلوب");

        if (string.IsNullOrWhiteSpace(options.Audience))
            errors.Add("Jwt:Audience مطلوب");

        if (options.AccessExpiresInMinutes <= 0)
            errors.Add("Jwt:AccessExpiresInMinutes يجب أن يكون أكبر من صفر");

        if (options.RefreshExpiresInDays <= 0)
            errors.Add("Jwt:RefreshExpiresInDays يجب أن يكون أكبر من صفر");

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}