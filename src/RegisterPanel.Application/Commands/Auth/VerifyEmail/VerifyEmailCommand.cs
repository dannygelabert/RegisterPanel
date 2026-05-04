using RegisterPanel.Application.Common;
using MediatR;

namespace RegisterPanel.Application.Commands.Auth.VerifyEmail;

public record VerifyEmailCommand(string UserId, string Token) : IRequest<Result>;
