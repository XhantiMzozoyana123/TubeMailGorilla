using Microsoft.AspNetCore.Identity;

namespace TubeMailGorilla.Infrastructure.Models;

/// <summary>
/// ASP.NET Core Identity user entity used by the persistence infrastructure.
/// This is an infrastructure concern, kept separate from the Domain User entity.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}
