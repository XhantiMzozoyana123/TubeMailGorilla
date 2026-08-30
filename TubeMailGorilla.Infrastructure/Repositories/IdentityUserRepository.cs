using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TubeMailGorilla.Domain;
using TubeMailGorilla.Domain.Entities;
using TubeMailGorilla.Domain.Interfaces;
using TubeMailGorilla.Infrastructure.Models;

namespace TubeMailGorilla.Infrastructure.Repositories;

/// <summary>
/// Infrastructure implementation of <see cref="IUserRepository"/> backed by
/// ASP.NET Core Identity. Maps between the Domain <see cref="User"/> entity
/// and the Identity <see cref="ApplicationUser"/> persistence model.
/// </summary>
public class IdentityUserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityUserRepository(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email) is not null;
    }

    public async Task<User?> CreateAsync(string email, string userName, string password, string? fullName)
    {
        var appUser = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            FullName = fullName
        };

        var result = await _userManager.CreateAsync(appUser, password);
        if (!result.Succeeded)
        {
            return null;
        }

        return MapToDomain(appUser);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var appUser = await _userManager.FindByEmailAsync(email);
        return appUser is null ? null : MapToDomain(appUser);
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        var appUser = await _userManager.FindByIdAsync(id);
        return appUser is null ? null : MapToDomain(appUser);
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(
            email, password, isPersistent: false, lockoutOnFailure: false);
        return result.Succeeded;
    }

        public async Task<IList<string>> GetRolesAsync(string userId)
    {
        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser is null)
        {
            return Array.Empty<string>();
        }

        return await _userManager.GetRolesAsync(appUser);
    }

    public async Task<IList<ClaimInfo>> GetClaimsAsync(string userId)
    {
        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser is null)
        {
            return Array.Empty<ClaimInfo>();
        }

        var claims = await _userManager.GetClaimsAsync(appUser);
        return claims.Select(c => new ClaimInfo(c.Type, c.Value)).ToArray();
    }

    public async Task AddClaimAsync(string userId, string claimType, string claimValue)
    {
        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser is null)
        {
            return;
        }

        await _userManager.AddClaimAsync(appUser, new Claim(claimType, claimValue));
    }

    public async Task<bool> HasClaimAsync(string userId, string claimType, string claimValue)
    {
        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser is null)
        {
            return false;
        }

        var claims = await _userManager.GetClaimsAsync(appUser);
        return claims.Any(c => c.Type == claimType && c.Value == claimValue);
    }

    public async Task RemoveClaimAsync(string userId, string claimType, string claimValue)
    {
        var appUser = await _userManager.FindByIdAsync(userId);
        if (appUser is null)
        {
            return;
        }

        // Remove ALL matching claims (duplicates may exist from legacy data).
        var claims = await _userManager.GetClaimsAsync(appUser);
        foreach (var claim in claims.Where(c => c.Type == claimType && c.Value == claimValue))
        {
            await _userManager.RemoveClaimAsync(appUser, claim);
        }
    }

    private static User MapToDomain(ApplicationUser appUser)
    {
        return new User
        {
            Id = appUser.Id,
            Email = appUser.Email ?? string.Empty,
            UserName = appUser.UserName ?? string.Empty,
            FullName = appUser.FullName
        };
    }
}
