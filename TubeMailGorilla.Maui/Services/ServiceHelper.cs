using Microsoft.Extensions.DependencyInjection;

namespace TubeMailGorilla.Maui.Services;

/// <summary>
/// Static service locator. Pages created by Shell's DataTemplate require a
/// parameterless constructor, so they resolve their dependencies from the
/// application service container through this helper.
/// </summary>
public static class ServiceHelper
{
    public static IServiceProvider? Services { get; private set; }

    public static void Initialize(IServiceProvider services) => Services = services;

    public static T GetService<T>() where T : class
    {
        if (Services is null)
            throw new InvalidOperationException(
                "Application services have not been initialized. Call ServiceHelper.Initialize before using pages.");
        return Services.GetRequiredService<T>();
    }
}