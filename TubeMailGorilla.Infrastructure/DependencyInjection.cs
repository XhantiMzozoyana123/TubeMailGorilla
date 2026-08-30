using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TubeMailGorilla.Domain;
using TubeMailGorilla.Domain.Interfaces;
using TubeMailGorilla.Infrastructure.Data;
using TubeMailGorilla.Infrastructure.Models;
using TubeMailGorilla.Infrastructure.Repositories;
using TubeMailGorilla.Infrastructure.Services;
using TubeMailGorilla.Infrastructure.Gateways;

namespace TubeMailGorilla.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure-layer services (database,
/// Identity, authentication, repositories, token service) with the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind strongly-typed JwtSettings from configuration
        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));

                        // Database context
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection")));
        });

        // ASP.NET Core Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // JWT Bearer authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtSettings = configuration.GetSection(nameof(JwtSettings));
            var secret = jwtSettings["Secret"] ?? "defaultSecret12345default";
            var issuer = jwtSettings["Issuer"] ?? "TubeMailGorillaAPI";
            var audience = jwtSettings["Audience"] ?? "TubeMailGorillaClient";

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
            };
        });

                // Domain abstractions implemented in Infrastructure
        services.AddScoped<IUserRepository, IdentityUserRepository>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPaymentGateway, PayPalGateway>();

        // PayPal configuration
        services.Configure<PayPalSettings>(configuration.GetSection(nameof(PayPalSettings)));

        // Server-side subscription pricing (client amounts are ignored)
        services.Configure<PricingSettings>(configuration.GetSection(nameof(PricingSettings)));

        // Repositories
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        return services;
    }
}
