using RegisterPanel.Domain.Entities;
using RegisterPanel.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace RegisterPanel.Infrastructure.Persistence.Repositories;

public sealed class AdminSettingsRepository : IAdminSettingsRepository
{
    private readonly RegisterPanelDbContext _context;

    public AdminSettingsRepository(RegisterPanelDbContext context)
    {
        _context = context;
    }

    public async Task<AdminSettings> GetAsync(CancellationToken ct = default)
    {
        AdminSettings? settings = await _context.AdminSettings
            .SingleOrDefaultAsync(ct);

        if (settings is null)
            throw new InvalidOperationException(
                "AdminSettings row is missing. Ensure DbSeeder.SeedAsync has been called on startup.");

        return settings;
    }

    public Task UpdateAsync(AdminSettings settings, CancellationToken ct = default)
    {
        // Entity is already tracked by the DbContext; SaveChanges is the caller's responsibility.
        _context.AdminSettings.Update(settings);
        return Task.CompletedTask;
    }
}
