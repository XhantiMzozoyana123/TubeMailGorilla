using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TubeMailGorilla.Application.DTOs;
using TubeMailGorilla.Application.Interfaces;

namespace TubeMailGorilla.Api.Controllers;

/// <summary>
/// Presentation-layer controller that exposes authentication endpoints.
/// It is a thin orchestration layer: receives HTTP requests, delegates to
/// the Application-layer <see cref="IAccountService"/> use-cases, and
/// translates results into HTTP responses.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AuthController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Registers a new user account and returns a JWT token on success.
    /// Contract: POST /api/auth/register  { Email, Password, FullName? }
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await _accountService.RegisterAsync(request);

        if (response.Success)
        {
            return Ok(response);
        }

        return UnprocessableEntity(response);
    }

    /// <summary>
    /// Authenticates the user and returns a JWT token on success.
    /// Contract: POST /api/auth/login  { Email, Password }
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _accountService.LoginAsync(request);

        if (response.Success)
        {
            return Ok(response);
        }

        return UnprocessableEntity(response);
    }

    /// <summary>
    /// Returns the currently authenticated user.
    /// Contract: GET /api/auth/user (Bearer token required)
    /// </summary>
    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("nameid")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var response = await _accountService.GetCurrentUserAsync(userId);

        if (response is null)
        {
            return Unauthorized();
        }

        return Ok(response);
    }
}
