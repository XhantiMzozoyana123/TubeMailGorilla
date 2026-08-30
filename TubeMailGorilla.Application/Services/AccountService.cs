using TubeMailGorilla.Application.DTOs;
using TubeMailGorilla.Application.Interfaces;
using TubeMailGorilla.Domain;
using TubeMailGorilla.Domain.Interfaces;


namespace TubeMailGorilla.Application.Services;

/// <summary>
/// Application-layer use-case that orchestrates authentication operations.
/// It depends only on Domain abstractions (IUserRepository, ITokenService)
/// so the business rules remain independent of any framework or persistence technology.
/// </summary>
public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AccountService(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            return new AuthResponse { Success = false, Message = "User already exists" };
        }

        var user = await _userRepository.CreateAsync(
            request.Email,
            request.Email, // Username defaults to email
            request.Password,
            request.FullName);

        if (user is null)
        {
            return new AuthResponse { Success = false, Message = "Failed to create user" };
        }

        var roles = await _userRepository.GetRolesAsync(user.Id);
        var claims = await _userRepository.GetClaimsAsync(user.Id);
        var token = _tokenService.GenerateToken(user, roles, claims);

        return new AuthResponse
        {
            Success = true,
            Message = "Register successful",
            Token = token
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        if (!await _userRepository.ValidateCredentialsAsync(request.Email, request.Password))
        {
            return new AuthResponse { Success = false, Message = "Invalid login attempt" };
        }

        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
        {
            return new AuthResponse { Success = false, Message = "User not found" };
        }

                var roles = await _userRepository.GetRolesAsync(user.Id);
        var claims = await _userRepository.GetClaimsAsync(user.Id);
        var token = _tokenService.GenerateToken(user, roles, claims);

        return new AuthResponse
        {
            Success = true,
            Message = "Login successful",
            Token = token
        };
    }

    public async Task<UserResponse?> GetCurrentUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Success = true
        };
    }
}
