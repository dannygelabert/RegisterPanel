using RegisterPanel.Application.Common;
using RegisterPanel.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace RegisterPanel.Application.Commands.Auth.VerifyEmail;

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public VerifyEmailCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> Handle(VerifyEmailCommand command, CancellationToken ct)
    {
        if (!Guid.TryParse(command.UserId, out Guid _))
            return Result.Failure("INVALID_TOKEN", "El enlace de verificación es inválido o ha expirado");

        ApplicationUser? user = await _userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return Result.Failure("INVALID_TOKEN", "El enlace de verificación es inválido o ha expirado");

        // Idempotent: already verified counts as success
        if (user.EmailConfirmed)
            return Result.Success();

        string decodedToken = Uri.UnescapeDataString(command.Token);
        IdentityResult result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
            return Result.Failure("INVALID_TOKEN", "El enlace de verificación es inválido o ha expirado");

        return Result.Success();
    }
}
