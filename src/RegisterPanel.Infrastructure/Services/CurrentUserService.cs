using System.Security.Claims;
using RegisterPanel.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace RegisterPanel.Infrastructure.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    public Guid UserId { get; }
    public string Email { get; }
    public IReadOnlyList<string> Roles { get; }

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        string? sub = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        UserId = sub is not null ? Guid.Parse(sub) : Guid.Empty;

        Email = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        List<string> roles = accessor.HttpContext?.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? [];

        Roles = roles;
    }
}
