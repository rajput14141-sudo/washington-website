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
            textContent = $"Reset your password:\n\n{resetUrl}"
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
