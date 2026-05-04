using RegisterPanel.Application.Common;
using RegisterPanel.Application.DTOs.Auth;
using RegisterPanel.Application.Interfaces.Services;
using RegisterPanel.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace RegisterPanel.Application.Commands.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken ct)
    {
        // 1. Buscar usuario por email
        ApplicationUser? user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return Result<LoginResponse>.Failure("INVALID_CREDENTIALS", "Email o contraseña incorrectos");

        // 2. Verificar que el email está confirmado
        if (!user.EmailConfirmed)
            return Result<LoginResponse>.Failure("EMAIL_NOT_VERIFIED", "Debes verificar tu email antes de hacer login");

        // 3. Verificar que la cuenta está activa
        if (!user.IsActive)
            return Result<LoginResponse>.Failure("ACCOUNT_INACTIVE", "Tu cuenta ha sido desactivada");

        // 4. Verificar contraseña
        bool passwordValid = await _userManager.CheckPasswordAsync(user, command.Password);
        if (!passwordValid)
            return Result<LoginResponse>.Failure("INVALID_CREDENTIALS", "Email o contraseña incorrectos");

        // 5. Obtener roles
        IList<string> roles = await _userManager.GetRolesAsync(user);

        // 6. Generar JWT
        (string token, DateTimeOffset expiresAt) = _jwtService.GenerateToken(user, roles);

        return Result<LoginResponse>.Success(new LoginResponse(
            AccessToken: token,
            ExpiresAt:   expiresAt,
            UserId:      user.Id.ToString(),
            Email:       user.Email!,
            FirstName:   user.FirstName,
            LastName:    user.LastName,
            Roles:       roles.ToList().AsReadOnly()
        ));
    }
}
