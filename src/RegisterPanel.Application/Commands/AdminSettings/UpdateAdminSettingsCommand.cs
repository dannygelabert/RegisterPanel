using RegisterPanel.Application.Common;
using MediatR;

namespace RegisterPanel.Application.Commands.AdminSettings;

public sealed record UpdateAdminSettingsCommand(
    int MaxSimultaneousTrainers,
    int MaxSimultaneousClientsPerTrainer,
    int MinHoursBeforeCancellation,
    bool AllowLateCancelWithPenalty,
    string UpdatedByUserId
) : IRequest<Result>;
