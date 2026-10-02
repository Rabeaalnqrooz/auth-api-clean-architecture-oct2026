using AuthApi.Application.Interfaces;
using AuthApi.Application.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AuthApi.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly string _frontendUrl;

    public EmailService(IOptions<EmailSettings> options, IConfiguration config)
    {
        _settings = options.Value;
        _frontendUrl = config["FrontendUrl"] ?? "http://localhost:5173";
    }

    public async Task SendEmailVerificationAsync(string toEmail, string firstName, string token)
    {
        var link = $"{_frontendUrl}/verify/{token}";
        var subject = "تأكيد البريد الإلكتروني";
        var body = $@"
            <h2>مرحباً {firstName}</h2>
            <p>اضغط لتأكيد بريدك:</p>
            <a href=""{link}"">تأكيد الإيميل</a>
            <p>الرابط صالح 24 ساعة.</p>";

        await SendAsync(toEmail, subject, body);
    }

    public async Task SendOtpAsync(string toEmail, string firstName, string otp)
    {
        var subject = "رمز إعادة تعيين كلمة المرور";
        var body = $@"
            <h2>مرحباً {firstName}</h2>
            <p>رمز التحقق:</p>
            <h1>{otp}</h1>
            <p>صالح لمدة 10 دقائق.</p>";

        await SendAsync(toEmail, subject, body);
    }

    private async Task SendAsync(string toEmail, string subject, string html)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = html };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.Username, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}