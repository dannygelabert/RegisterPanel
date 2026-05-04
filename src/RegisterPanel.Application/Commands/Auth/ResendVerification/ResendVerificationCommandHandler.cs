using RegisterPanel.Application.Common;
using RegisterPanel.Application.Interfaces.Services;
using RegisterPanel.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace RegisterPanel.Application.Commands.Auth.ResendVerification;

public sealed class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public ResendVerificationCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<Result> Handle(ResendVerificationCommand command, CancellationToken ct)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(command.Email);

        // Always return success — never reveal whether the email is registered or verified
        if (user is null || user.EmailConfirmed)
            return Result.Success();

        string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        string encodedToken = Uri.EscapeDataString(token);

        await _emailService.SendEmailVerificationAsync(user.Email!, user.Id.ToString(), encodedToken);

        return Result.Success();
    }
}
