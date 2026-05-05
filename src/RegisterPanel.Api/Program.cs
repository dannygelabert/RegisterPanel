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

builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();

    if (context.HostingEnvironment.IsDevelopment())
        config.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
    else
        config.WriteTo.Console(new CompactJsonFormatter());
});

// Must register before AddApiServices so that AddIdentity doesn't overwrite JWT schemes
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMediatR(
    cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

builder.Services.AddApiServices(builder.Configuration);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

WebApplication app = builder.Build();

app.UseApiMiddleware();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    using IServiceScope scope = app.Services.CreateScope();
    RegisterPanelDbContext db = scope.ServiceProvider.GetRequiredService<RegisterPanelDbContext>();
    await db.Database.MigrateAsync();
}

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

