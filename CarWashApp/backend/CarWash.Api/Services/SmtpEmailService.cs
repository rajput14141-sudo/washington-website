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
        !string.IsNullOrWhiteSpace(_configuration["Email:FromAddress"]) &&
        !string.IsNullOrWhiteSpace(_configuration["Email:Username"]) &&
        !string.IsNullOrWhiteSpace(_configuration["Email:Password"]);

    public async Task SendPasswordResetAsync(
        string recipientEmail,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            throw new InvalidOperationException("Password reset email is not configured.");

        var host = _configuration["Email:Host"]!;
        var fromAddress = _configuration["Email:FromAddress"]!;
        var username = _configuration["Email:Username"]!;
        var password = _configuration["Email:Password"]!;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _configuration["Email:FromName"] ?? "Mr.WashingTon Car Wash",
            fromAddress));    
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = "Reset your Mr.WashingTon password";
        message.Body = new TextPart("plain")
        {
            Text = $"Use this link to reset your password:\n\n{resetUrl}\n\nIf you did not request this, you can ignore this email."
        };

        var port = _configuration.GetValue("Email:Port", 587);
        var socketOptions = _configuration.GetValue("Email:UseSsl", false)
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(90));

        using var client = new SmtpClient { Timeout = 30000 };
        await client.ConnectAsync(host, port, socketOptions, timeout.Token);
        await client.AuthenticateAsync(username, password, timeout.Token);
        await client.SendAsync(message, timeout.Token);
        await client.DisconnectAsync(true, timeout.Token);
    }
}