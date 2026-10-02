namespace AuthApi.Application.Options;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string AccessSecret { get; set; } = string.Empty;
    public string RefreshSecret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessExpiresInMinutes { get; set; } = 15;
    public int RefreshExpiresInDays { get; set; } = 7;
}