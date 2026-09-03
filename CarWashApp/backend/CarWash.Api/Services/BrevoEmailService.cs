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
                        <h2>Reset your password</h2>
                        <p>
                            <a href="{{encodedResetUrl}}">Reset Password</a>
                        </p>
                        <p>{{encodedResetUrl}}</p>
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
