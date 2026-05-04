using RegisterPanel.Application.Common;
using MediatR;

namespace RegisterPanel.Application.Commands.Auth.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;
