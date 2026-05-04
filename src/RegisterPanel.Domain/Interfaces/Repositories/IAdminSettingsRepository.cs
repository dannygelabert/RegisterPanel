using RegisterPanel.Domain.Entities;

namespace RegisterPanel.Domain.Interfaces.Repositories;

public interface IAdminSettingsRepository
{
    /// <summary>
    /// Returns the single AdminSettings row. Throws if the row is missing (should never happen after seeding).
    /// </summary>
    Task<AdminSettings> GetAsync(CancellationToken ct = default);

    Task UpdateAsync(AdminSettings settings, CancellationToken ct = default);
}
