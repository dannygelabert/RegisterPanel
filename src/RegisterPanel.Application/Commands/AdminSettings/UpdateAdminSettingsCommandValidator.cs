using FluentValidation;

namespace RegisterPanel.Application.Commands.AdminSettings;

public sealed class UpdateAdminSettingsCommandValidator : AbstractValidator<UpdateAdminSettingsCommand>
{
    public UpdateAdminSettingsCommandValidator()
    {
        RuleFor(x => x.MaxSimultaneousTrainers)
            .GreaterThanOrEqualTo(1).WithMessage("MaxSimultaneousTrainers must be >= 1.")
            .LessThanOrEqualTo(20).WithMessage("MaxSimultaneousTrainers must be <= 20.");

        RuleFor(x => x.MaxSimultaneousClientsPerTrainer)
            .GreaterThanOrEqualTo(1).WithMessage("MaxSimultaneousClientsPerTrainer must be >= 1.")
            .LessThanOrEqualTo(20).WithMessage("MaxSimultaneousClientsPerTrainer must be <= 20.");

        RuleFor(x => x.MinHoursBeforeCancellation)
            .GreaterThanOrEqualTo(0).WithMessage("MinHoursBeforeCancellation cannot be negative.")
            .LessThanOrEqualTo(168).WithMessage("MinHoursBeforeCancellation must be <= 168 (1 week).");

        RuleFor(x => x.UpdatedByUserId)
            .NotEmpty().WithMessage("UpdatedByUserId is required.");
    }
}
