using RegisterPanel.Application.Common;
using RegisterPanel.Application.DTOs.Auth;
using MediatR;

namespace RegisterPanel.Application.Commands.Auth.Register;

public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber
) : IRequest<Result<RegisterResponse>>;
