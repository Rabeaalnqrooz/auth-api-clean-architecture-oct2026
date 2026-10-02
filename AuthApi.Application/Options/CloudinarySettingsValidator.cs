using Microsoft.Extensions.Options;

namespace AuthApi.Application.Options;

public class CloudinarySettingsValidator : IValidateOptions<CloudinarySettings>
{
    public ValidateOptionsResult Validate(string? name, CloudinarySettings options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.CloudName))
            errors.Add("CloudinarySettings:CloudName مطلوب");

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            errors.Add("CloudinarySettings:ApiKey مطلوب");

        if (string.IsNullOrWhiteSpace(options.ApiSecret))
            errors.Add("CloudinarySettings:ApiSecret مطلوب");

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}