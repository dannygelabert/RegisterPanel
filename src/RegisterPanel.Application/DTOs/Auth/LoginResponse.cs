namespace RegisterPanel.Application.DTOs.Auth;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles
);
