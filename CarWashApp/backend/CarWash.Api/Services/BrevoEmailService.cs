using System.Net;
using System.Text;
using System.Text.Json;

namespace CarWash.Api.Services;

public class BrevoEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public BrevoEmailService(
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_configuration["Brevo:ApiKey"]);

    public async Task SendPasswordResetAsync(
        string recipientEmail,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Brevo:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Brevo email is not configured.");

        var encodedResetUrl = WebUtility.HtmlEncode(resetUrl);
        var htmlContent = $$"""
            <div style="font-family:Arial,sans-serif;line-height:1.6;color:#1e293b">
                            <h2>Reset your password</h2>
                            <p>Click the button below to reset your password:</p>
                            <p style="margin:24px 0">
                                <a href="{{encodedResetUrl}}" style="background:#4f6bed;color:#ffffff;padding:12px 24px;text-decoration:none;border-radius:8px;display:inline-block;font-weight:700">
                                    Reset Password
                                </a>
                            </p>
                            <p>If the button doesn't work, copy and paste this URL:</p>
              <p style="overflow-wrap:anywhere">{{encodedResetUrl}}</p>
              <p>If you did not request a password reset, you can ignore this email.</p>
            </div>
            """;

        var body = new
        {
            sender = new
            {
                name = "Mr.WashingTon Car Wash",
                email = "support@mrwashington.in"
            },
            to = new[]
            {
                new { email = recipientEmail }
            },
            subject = "Reset your password",
            htmlContent,
            textContent = $"Reset your password by opening this link:\n\n{resetUrl}\n\nIf you did not request this, you can ignore this email.",
            headers = new Dictionary<string, string>
            {
                ["X-Mailin-Track-Click"] = "0"
            },
            tags = new[] { "password-reset" }
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.brevo.com/v3/smtp/email");

        request.Headers.Add("api-key", apiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
