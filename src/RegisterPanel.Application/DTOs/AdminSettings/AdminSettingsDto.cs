namespace RegisterPanel.Application.DTOs.AdminSettings;

public record AdminSettingsDto(
    int MaxSimultaneousTrainers,
    int MaxSimultaneousClientsPerTrainer,
    int MinHoursBeforeCancellation,
    bool AllowLateCancelWithPenalty,
    DateTimeOffset UpdatedAt,
    string UpdatedByUserId
);
