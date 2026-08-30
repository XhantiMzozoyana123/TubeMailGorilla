using TubeMailGorilla.Domain.Entities;

namespace TubeMailGorilla.Domain.Interfaces;

/// <summary>
/// Defines the contract for user persistence operations.
/// Implementations live in the Infrastructure layer, keeping the
/// Domain layer free of any persistence-framework dependencies.
/// </summary>
public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<User?> CreateAsync(string email, string userName, string password, string? fullName);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(string id);
    Task<bool> ValidateCredentialsAsync(string email, string password);
    Task<IList<string>> GetRolesAsync(string userId);
    Task<IList<ClaimInfo>> GetClaimsAsync(string userId);
    Task AddClaimAsync(string userId, string claimType, string claimValue);
    Task<bool> HasClaimAsync(string userId, string claimType, string claimValue);
    Task RemoveClaimAsync(string userId, string claimType, string claimValue);
}

