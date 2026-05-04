using RegisterPanel.Application.Common;
using RegisterPanel.Application.Interfaces.Services;
using RegisterPanel.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace RegisterPanel.Application.Commands.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand command, CancellationToken ct)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(command.Email);

        // Always return success — never reveal whether the email is registered
        if (user is null || !user.EmailConfirmed)
            return Result.Success();

        string token = await _userManager.GeneratePasswordResetTokenAsync(user);
        string encodedToken = Uri.EscapeDataString(token);

        await _emailService.SendPasswordResetAsync(user.Email!, user.Id.ToString(), encodedToken);

        return Result.Success();
    }
}
