using FluentValidation;
using RegisterPanel.Api.Extensions;
using RegisterPanel.Application;
using RegisterPanel.Application.Behaviors;
using RegisterPanel.Infrastructure.Extensions;
using RegisterPanel.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Formatting.Compact;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ── Logging ───────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();
    // NOTE: Per-request TraceId enrichment is handled by UseSerilogRequestLogging()
    // and manually in GlobalExceptionMiddleware via context.TraceIdentifier.

    if (context.HostingEnvironment.IsDevelopment())
        config.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
    else
        config.WriteTo.Console(new CompactJsonFormatter());
});

// ── Infrastructure layer ──────────────────────────────────────────────────────
// Must register before AddApiServices so that AddIdentity doesn't overwrite JWT schemes
builder.Services.AddInfrastructure(builder.Configuration);

// ── Application layer ─────────────────────────────────────────────────────────
builder.Services.AddMediatR(
    cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

// ── API services (JWT auth — must come after AddInfrastructure/AddIdentity) ───
builder.Services.AddApiServices(builder.Configuration);

// ── OpenAPI / Scalar ──────────────────────────────────────────────────────────
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

// ─────────────────────────────────────────────────────────────────────────────
WebApplication app = builder.Build();

app.UseApiMiddleware();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

// ── Auto-migration (Development only) ────────────────────────────────────────
// ⚠️ MANUAL STEP REQUIRED (production): run migrations manually before deploying:
//   dotnet ef database update --project RegisterPanel.Infrastructure --startup-project RegisterPanel.Api
if (app.Environment.IsDevelopment())
{
    using IServiceScope scope = app.Services.CreateScope();
    RegisterPanelDbContext db = scope.ServiceProvider.GetRequiredService<RegisterPanelDbContext>();
    await db.Database.MigrateAsync();
}

// ── Seeding (all environments) ───────────────────────────────────────────────
// ⚠️ MANUAL STEP REQUIRED (production): if you run migrations manually, also run the seeder
//   or INSERT INTO admin_settings manually with Id=1 and the desired defaults before first boot.
{
    using IServiceScope seedScope = app.Services.CreateScope();
    RegisterPanelDbContext seedDb = seedScope.ServiceProvider.GetRequiredService<RegisterPanelDbContext>();
    await DbSeeder.SeedAsync(seedDb, seedScope.ServiceProvider);
}

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "RegisterPanel API";
        options.Theme = ScalarTheme.Purple;
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecuritySchemes = ["Bearer"]
        };
    });
}

app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
   .WithName("Health")
   .WithTags("System");

app.Run();

