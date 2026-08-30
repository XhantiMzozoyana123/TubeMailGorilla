using Microsoft.Extensions.DependencyInjection;
using TubeMailGorilla.Application.Interfaces;
using TubeMailGorilla.Application.Services;

namespace TubeMailGorilla.Application;

/// <summary>
/// Extension methods for registering Application-layer services with the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
                services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        return services;
    }
}
