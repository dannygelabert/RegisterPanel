using RegisterPanel.Application.Common;
using MediatR;

namespace RegisterPanel.Application.Commands.Auth.ResetPassword;

public record ResetPasswordCommand(
    string UserId,
    string Token,
    string NewPassword
) : IRequest<Result>;
