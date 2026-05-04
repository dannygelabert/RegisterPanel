using FluentValidation;

namespace RegisterPanel.Application.Commands.Auth.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("El email no tiene formato válido");

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
