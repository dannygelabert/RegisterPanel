using RegisterPanel.Domain.Entities;
using RegisterPanel.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RegisterPanel.Infrastructure.Persistence;

public static class DbSeeder
{
    /// <summary>
    /// Seeds all initial reference data: AdminSettings, roles, and default admin user.
    /// Safe to call on every startup — every step is idempotent.
    /// </summary>
    public static async Task SeedAsync(RegisterPanelDbContext context, IServiceProvider serviceProvider)
    {
        await SeedAdminSettingsAsync(context);
        await SeedRolesAsync(serviceProvider);
        await SeedAdminUserAsync(serviceProvider);
    }

    private static async Task SeedAdminSettingsAsync(RegisterPanelDbContext context)
    {
        if (!await context.AdminSettings.AnyAsync())
        {
            context.AdminSettings.Add(AdminSettings.CreateDefaults());
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        RoleManager<ApplicationRole> roleManager =
            serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        string[] roleNames = [Roles.Admin, Roles.Trainer, Roles.Client];
        foreach (string roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole(roleName));
            }
        }
    }

    private static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        UserManager<ApplicationUser> userManager =
            serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        IConfiguration configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        string email = configuration["Admin:Email"]
            ?? throw new InvalidOperationException("Admin:Email is not configured");
        string password = configuration["Admin:Password"]
            ?? throw new InvalidOperationException("Admin:Password is not configured");

        ApplicationUser? existing = await userManager.FindByEmailAsync(email);
        if (existing is not null) return;

        ApplicationUser admin = new()
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        IdentityResult result = await userManager.CreateAsync(admin, password);
        if (!result.Succeeded)
        {
            string errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to seed admin user: {errors}");
        }

        await userManager.AddToRoleAsync(admin, Roles.Admin);
    }
}
