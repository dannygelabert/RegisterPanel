using RegisterPanel.Application.Common;
using RegisterPanel.Application.DTOs.Auth;
using RegisterPanel.Application.Interfaces;
using RegisterPanel.Application.Interfaces.Services;
using RegisterPanel.Domain.Entities;
using RegisterPanel.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace RegisterPanel.Application.Commands.Auth.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand command, CancellationToken ct)
    {
        ApplicationUser? existing = await _userManager.FindByEmailAsync(command.Email);
        if (existing is not null)
            return Result<RegisterResponse>.Failure("EMAIL_ALREADY_IN_USE", "El email ya está registrado");

        ApplicationUser? createdUser = null;

        Result<RegisterResponse> txResult = await _unitOfWork.ExecuteInTransactionAsync<RegisterResponse>(
            async innerCt =>
            {
                ApplicationUser user = new()
                {
                    Id = Guid.NewGuid(),
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    Email = command.Email,
                    UserName = command.Email,
                    EmailConfirmed = false,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                IdentityResult createResult = await _userManager.CreateAsync(user, command.Password);
                if (!createResult.Succeeded)
                {
                    string errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return Result<RegisterResponse>.Failure("REGISTRATION_FAILED", errors);
                }

                IdentityResult roleResult = await _userManager.AddToRoleAsync(user, Roles.Client);
                if (!roleResult.Succeeded)
                {
                    string errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    return Result<RegisterResponse>.Failure("REGISTRATION_FAILED", errors);
                }

                await _unitOfWork.SaveChangesAsync(innerCt);

                createdUser = user;
                return Result<RegisterResponse>.Success(new RegisterResponse(
                    user.Id,
                    user.Email!,
                    "Registro completado. Revisa tu email para activar la cuenta."
                ));
            }, ct);

        if (!txResult.IsSuccess)
            return txResult;

        string token = await _userManager.GenerateEmailConfirmationTokenAsync(createdUser!);
        string encodedToken = Uri.EscapeDataString(token);
        await _emailService.SendEmailVerificationAsync(createdUser!.Email!, createdUser!.Id.ToString(), encodedToken);

        return txResult;
    }
}

