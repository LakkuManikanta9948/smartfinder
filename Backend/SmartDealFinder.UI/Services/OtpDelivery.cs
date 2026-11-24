using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SmartDealFinder.Service;

namespace SmartDealFinder.UI.Services;

public sealed class DevelopmentOtpDelivery(ILogger<DevelopmentOtpDelivery> logger) : IOtpDelivery
{
    public Task<string?> SendAsync(string phoneNumber, string code, CancellationToken cancellationToken)
    {
        logger.LogInformation("Development-only OTP for {PhoneNumber}: {Code}", phoneNumber, code);
        return Task.FromResult<string?>(code);
    }
}

public sealed class HttpSmsOtpDelivery(HttpClient httpClient, IConfiguration configuration) : IOtpDelivery
{
    public async Task<string?> SendAsync(string phoneNumber, string code, CancellationToken cancellationToken)
    {
        var endpoint = configuration["Sms:Endpoint"];
        var token = configuration["Sms:ApiToken"];
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri)
            || endpointUri.Scheme != Uri.UriSchemeHttps
            || string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Configure Sms:Endpoint (HTTPS) and Sms:ApiToken before sending production OTP messages.");

        using var request = new HttpRequestMessage(HttpMethod.Post, endpointUri)
        {
            Content = JsonContent.Create(new
            {
                to = phoneNumber,
                sender = configuration["Sms:SenderId"],
                message = $"Your SmartCompare verification code is {code}. It expires in five minutes."
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return null;
    }
}
