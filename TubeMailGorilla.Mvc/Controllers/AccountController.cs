using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TubeMailGorilla.Application.DTOs;
using TubeMailGorilla.Application.Interfaces;
using TubeMailGorilla.Domain;
using TubeMailGorilla.Domain.Interfaces;
using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Controllers;

/// <summary>
/// Cookie-authenticated account pages for the MVC website. Reuses the same
/// Application-layer <see cref="IAccountService"/> use-cases as the API, but
/// signs the user in with ASP.NET Core Identity cookies instead of JWTs.
/// </summary>
[Authorize]
public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        IAccountService accountService,
        ISubscriptionService subscriptionService,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _accountService = accountService;
        _subscriptionService = subscriptionService;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Same use-case as the API's /api/auth/login: validates credentials
        // against the shared Identity store and issues a token payload (the
        // token is not used by the website - cookies handle web sessions).
        var response = await _accountService.LoginAsync(new LoginRequest(model.Email, model.Password));
        if (!response.Success)
        {
            ModelState.AddModelError(string.Empty, response.Message ?? "Invalid login attempt.");
            return View(model);
        }

        await SignInWithClaimsAsync(model.Email, response.Token);
        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var response = await _accountService.RegisterAsync(new RegisterRequest
        {
            Email = model.Email,
            Password = model.Password,
            FullName = model.FullName
        });

        if (!response.Success)
        {
            ModelState.AddModelError(string.Empty, response.Message ?? "Registration failed.");
            return View(model);
        }

        await SignInWithClaimsAsync(model.Email, response.Token);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    /// <summary>
    /// Signs the user in with the cookie scheme. Roles + persisted claims
    /// (including "subscription": "active") flow into the cookie principal,
    /// so [Authorize(Policy = "Subscribed")] works with no token plumbing.
    /// The freshly-issued JWT is carried as an extra claim for API interop.
    /// </summary>
    private async Task SignInWithClaimsAsync(string email, string? token)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return;

        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        if (principal.Identity is ClaimsIdentity identity && !string.IsNullOrEmpty(token))
        {
            identity.AddClaim(new Claim("app_jwt", token));
        }

        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });
    }
}
