namespace CarWash.Api.Services;

public interface IEmailService
{
    bool IsConfigured { get; }
    Task SendPasswordResetAsync(string recipientEmail, string resetUrl, CancellationToken cancellationToken = default);
}