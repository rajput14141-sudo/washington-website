using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CarWash.Api.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_configuration["Email:Host"]) &&
        !string.IsNullOrWhiteSpace(_configuration["Email:FromAddress"]);

    public async Task SendPasswordResetAsync(string recipientEmail, string resetUrl)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("Password reset email is not configured.");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _configuration["Email:FromName"] ?? "Mr.WashingTon Car Wash",
            _configuration["Email:FromAddress"]));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = "Reset your Mr.WashingTon password";
        message.Body = new TextPart("plain")
        {
            Text = $"Use this link to reset your password:\n\n{resetUrl}\n\nIf you did not request this, you can ignore this email."
        };

        var port = _configuration.GetValue("Email:Port", 587);
        var useSsl = _configuration.GetValue("Email:UseSsl", false);
        using var client = new SmtpClient();
        await client.ConnectAsync(
            _configuration["Email:Host"],
            port,
            useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable);

        var username = _configuration["Email:Username"];
        if (!string.IsNullOrWhiteSpace(username))
            await client.AuthenticateAsync(username, _configuration["Email:Password"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}