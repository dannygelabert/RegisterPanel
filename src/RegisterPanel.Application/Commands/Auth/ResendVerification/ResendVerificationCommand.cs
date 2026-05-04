using RegisterPanel.Application.Common;
using MediatR;

namespace RegisterPanel.Application.Commands.Auth.ResendVerification;

public record ResendVerificationCommand(string Email) : IRequest<Result>;
