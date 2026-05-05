using RegisterPanel.Application.Interfaces;
using RegisterPanel.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RegisterPanel.Infrastructure.Persistence;

public class RegisterPanelDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IUnitOfWork
{
    public RegisterPanelDbContext(DbContextOptions<RegisterPanelDbContext> options) : base(options) { }

    public DbSet<AdminSettings> AdminSettings => Set<AdminSettings>();

    public Task<Application.Common.Result<TResult>> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<Application.Common.Result<TResult>>> operation,
        CancellationToken cancellationToken = default)
    {
        return Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction tx =
                await Database.BeginTransactionAsync(cancellationToken);
            Application.Common.Result<TResult> result = await operation(cancellationToken);
            if (result.IsSuccess)
                await tx.CommitAsync(cancellationToken);
            else
                await tx.RollbackAsync(cancellationToken);
            return result;
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // required for Identity tables

        // IdentityDbContext hard-codes PascalCase table names that override UseSnakeCaseNamingConvention.
        // Rename them explicitly so the schema is fully snake_case.
        modelBuilder.Entity<ApplicationUser>().ToTable("asp_net_users");
        modelBuilder.Entity<ApplicationRole>().ToTable("asp_net_roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("asp_net_user_roles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("asp_net_user_claims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("asp_net_user_logins");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("asp_net_role_claims");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("asp_net_user_tokens");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RegisterPanelDbContext).Assembly);

        // Force timestamp with time zone for all DateTimeOffset properties
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (IMutableProperty property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset) ||
                    property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetColumnType("timestamp with time zone");
                }
            }
        }
    }
}
