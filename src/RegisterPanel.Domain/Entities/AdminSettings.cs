using RegisterPanel.Domain.Exceptions;

namespace RegisterPanel.Domain.Entities;

public class AdminSettings
{
    public int Id { get; private set; } // always 1 — singleton row

    public int MaxSimultaneousTrainers { get; private set; }

    public int MaxSimultaneousClientsPerTrainer { get; private set; }

    /// <summary>Minimum hours of advance notice required to cancel/reschedule without losing the credit. Default 24.</summary>
    public int MinHoursBeforeCancellation { get; private set; }

    /// <summary>If true, a late cancellation marks the credit as Forfeited instead of returning it. Default true.</summary>
    public bool AllowLateCancelWithPenalty { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public string UpdatedByUserId { get; private set; } = default!;

    private AdminSettings() { } // for EF Core

    public static AdminSettings CreateDefaults() => new()
    {
        Id = 1,
        MaxSimultaneousTrainers = 2,
        MaxSimultaneousClientsPerTrainer = 2,
        MinHoursBeforeCancellation = 24,
        AllowLateCancelWithPenalty = true,
        UpdatedAt = DateTimeOffset.UtcNow,
        UpdatedByUserId = "SYSTEM"
    };

    public void Update(
        int maxTrainers,
        int maxClientsPerTrainer,
        int minHoursBeforeCancellation,
        bool allowLateCancelWithPenalty,
        string updatedByUserId)
    {
        if (maxTrainers < 1)
            throw new DomainException("MaxSimultaneousTrainers must be >= 1");
        if (maxClientsPerTrainer < 1)
            throw new DomainException("MaxSimultaneousClientsPerTrainer must be >= 1");
        if (minHoursBeforeCancellation < 0)
            throw new DomainException("MinHoursBeforeCancellation cannot be negative");

        MaxSimultaneousTrainers = maxTrainers;
        MaxSimultaneousClientsPerTrainer = maxClientsPerTrainer;
        MinHoursBeforeCancellation = minHoursBeforeCancellation;
        AllowLateCancelWithPenalty = allowLateCancelWithPenalty;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }
}
