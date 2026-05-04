using RegisterPanel.Application.Common;
using RegisterPanel.Application.DTOs.Auth;
using MediatR;

namespace RegisterPanel.Application.Commands.Auth.Login;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<LoginResponse>>;
