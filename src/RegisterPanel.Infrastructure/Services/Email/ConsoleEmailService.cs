using RegisterPanel.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RegisterPanel.Infrastructure.Services.Email;

/// <summary>
/// Development email service that writes verification and password reset links to the console
/// instead of sending real emails.
/// </summary>
public sealed class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;
    private readonly string _baseUrl;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _baseUrl = configuration["App:BaseUrl"]
            ?? throw new InvalidOperationException("App:BaseUrl is not configured.");
    }

    public Task SendEmailVerificationAsync(string toEmail, string userId, string encodedToken)
    {
        string link = $"{_baseUrl}/auth/verify-email?userId={userId}&token={encodedToken}";
        _logger.LogInformation(
            "[DEV] Email verification link for {Email}: {Link}",
            toEmail,
            link);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string toEmail, string userId, string encodedToken)
    {
        string link = $"{_baseUrl}/auth/reset-password?userId={userId}&token={encodedToken}";
        _logger.LogInformation(
            "[DEV] Password reset link for {Email}: {Link}",
            toEmail,
            link);
        return Task.CompletedTask;
    }
}
