using RegisterPanel.Domain.Entities;

namespace RegisterPanel.Application.Interfaces.Services;

public interface IJwtService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateToken(ApplicationUser user, IList<string> roles);
}
