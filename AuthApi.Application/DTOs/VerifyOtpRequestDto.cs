namespace AuthApi.Application.DTOs;

public class VerifyOtpRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}