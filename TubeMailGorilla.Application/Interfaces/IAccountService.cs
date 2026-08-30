using TubeMailGorilla.Application.DTOs;

namespace TubeMailGorilla.Application.Interfaces;

/// <summary>
/// Application-layer use-case contract for authentication/account operations.
/// </summary>
public interface IAccountService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserResponse?> GetCurrentUserAsync(string userId);
}
