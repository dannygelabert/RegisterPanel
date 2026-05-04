namespace RegisterPanel.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string userId, string encodedToken);
    Task SendPasswordResetAsync(string toEmail, string userId, string encodedToken);
}
