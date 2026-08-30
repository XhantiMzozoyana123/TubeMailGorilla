using TubeMailGorilla.Domain.Entities;

namespace TubeMailGorilla.Domain.Interfaces;

/// <summary>
/// Defines the contract for issuing security tokens.
/// Implementations live in the Infrastructure layer, keeping the
/// Domain layer free of any token/JWT-framework dependencies.
/// </summary>
public interface ITokenService
{
    string GenerateToken(User user, IList<string> roles, IList<ClaimInfo>? claims = null);
}
