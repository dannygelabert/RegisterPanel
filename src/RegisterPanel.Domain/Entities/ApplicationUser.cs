using Microsoft.AspNetCore.Identity;

namespace RegisterPanel.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
