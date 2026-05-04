using RegisterPanel.Application.Common;
using RegisterPanel.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace RegisterPanel.Application.Commands.Auth.ResetPassword;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        if (!Guid.TryParse(command.UserId, out Guid _))
            return Result.Failure("INVALID_TOKEN", "El enlace es inválido o ha expirado");

        ApplicationUser? user = await _userManager.FindByIdAsync(command.UserId);
        if (user is null)
            return Result.Failure("INVALID_TOKEN", "El enlace es inválido o ha expirado");

        string decodedToken = Uri.UnescapeDataString(command.Token);
        IdentityResult result = await _userManager.ResetPasswordAsync(user, decodedToken, command.NewPassword);

        if (!result.Succeeded)
            return Result.Failure("INVALID_TOKEN", "El enlace es inválido o ha expirado");

        return Result.Success();
    }
}
