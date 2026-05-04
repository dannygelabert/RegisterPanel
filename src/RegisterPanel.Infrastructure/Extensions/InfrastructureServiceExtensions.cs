using RegisterPanel.Application.Interfaces;
using RegisterPanel.Application.Interfaces.Services;
using RegisterPanel.Domain.Entities;
using RegisterPanel.Domain.Interfaces.Repositories;
using RegisterPanel.Infrastructure.Persistence;
using RegisterPanel.Infrastructure.Persistence.Repositories;
using RegisterPanel.Infrastructure.Services;
using RegisterPanel.Infrastructure.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace RegisterPanel.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<RegisterPanelDbContext>(options =>
        {
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Default"),
                    npgsql =>
                    {
                        npgsql.MigrationsAssembly("RegisterPanel.Infrastructure");
                        npgsql.EnableRetryOnFailure(maxRetryCount: 3);
                    })
                .UseSnakeCaseNamingConvention(); // all table and column names in snake_case

            // Log SQL queries only in Development
            options.EnableSensitiveDataLogging(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development");
        });

        // Identity (must register before JWT)
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false; // relaxed — verify later in BE-015
        })
        .AddEntityFrameworkStores<RegisterPanelDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<RegisterPanelDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IEmailService, ConsoleEmailService>();
        services.AddSingleton<IJwtService, JwtService>();
        services.AddScoped<IAdminSettingsRepository, AdminSettingsRepository>();

        return services;
    }
}
