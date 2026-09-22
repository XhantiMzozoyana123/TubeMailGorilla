using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TubeMailGorilla.Application;
using TubeMailGorilla.Application.DTOs;
using TubeMailGorilla.Domain;
using TubeMailGorilla.Domain.Constants;
using TubeMailGorilla.Domain.Interfaces;
using TubeMailGorilla.Infrastructure;
using TubeMailGorilla.Infrastructure.Data;
using TubeMailGorilla.Infrastructure.Models;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------------------
// Presentation layer - MVC website sharing the same Domain / Application /
// Infrastructure layers as the API. Same MySQL Identity database, same
// services, same subscription model - just a different front end.
// -----------------------------------------------------------------------

builder.Services.AddControllersWithViews();

// Application layer (use-cases: accounts, subscriptions, extraction usage)
builder.Services.AddApplication();

// Subscription plan catalog + FREE plan limits (same keys as the API)
builder.Services.Configure<SubscriptionPlansOptions>(options =>
{
    options.Plans = builder.Configuration.GetSection("SubscriptionPlans")
        .Get<List<SubscriptionPlanDefinition>>() ?? new List<SubscriptionPlanDefinition>();
});
builder.Services.Configure<FreePlanLimits>(builder.Configuration.GetSection("FreePlan"));

// Infrastructure layer (EF Core, Identity, JWT token service, PayPal gateway)
builder.Services.AddInfrastructure(builder.Configuration);

// ASP.NET Core Identity sign-in via auth cookies for the website. Shares the
// same IdentityUser store as the API, so a user registered on one front end
// can sign in on both. The API keeps issuing JWTs for the MAUI desktop app.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Authorization policies - identical meaning to the API: "Subscribed" is the
// persisted "subscription": "active" Identity claim (granted by PayPal capture
// or the admin seed).
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Subscribed", policy =>
        policy.RequireClaim(SubscriptionClaim.Type, SubscriptionClaim.Value));
});

var app = builder.Build();

// -----------------------------------------------------------------------
// Database bootstrap (migrations) + admin seed, mirroring the API's Program.cs
// -----------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var adminEmail = builder.Configuration["AdminSeed:Email"] ?? "admin@tubemailgorilla.com";
    var adminPassword = builder.Configuration["AdminSeed:Password"] ?? "Adm1nGorilla!";

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    const string adminRoleName = "Admin";
    if (!await roleManager.RoleExistsAsync(adminRoleName))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRoleName));
    }

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser is null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FullName = "Administrator"
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Failed to seed the admin account: " + string.Join("; ", createResult.Errors.Select(e => e.Description)));
        }
    }

    if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
    {
        await userManager.AddToRoleAsync(adminUser, adminRoleName);
    }

    var adminClaims = await userManager.GetClaimsAsync(adminUser);
    if (!adminClaims.Any(c => c.Type == SubscriptionClaim.Type && c.Value == SubscriptionClaim.Value))
    {
        await userManager.AddClaimAsync(adminUser, new Claim(SubscriptionClaim.Type, SubscriptionClaim.Value));
    }
}

// -----------------------------------------------------------------------
// HTTP request pipeline
// -----------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
