namespace RegisterPanel.Application.Interfaces.Services;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    IReadOnlyList<string> Roles { get; }
}
