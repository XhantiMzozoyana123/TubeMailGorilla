using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TubeMailGorilla.Maui.Models;
using TubeMailGorilla.Maui.Services;

namespace TubeMailGorilla.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Same font stack as the TubeMailGorilla.Web site:
                // body = Plus Jakarta Sans, headings = Outfit,
                // CTA buttons = Rajdhani, status/mono = JetBrains Mono.
                fonts.AddFont("PlusJakartaSans-Regular.ttf", "PlusJakartaSans");
                fonts.AddFont("Outfit-SemiBold.ttf", "Outfit");
                fonts.AddFont("Rajdhani-SemiBold.ttf", "Rajdhani");
                fonts.AddFont("JetBrainsMono-Regular.ttf", "JetBrainsMono");
            });

        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<YouTubeSearchService>();
        builder.Services.AddSingleton<YouTubeTranscriptService>();
        builder.Services.AddSingleton<CaptionService>();
        builder.Services.AddSingleton<ExtractService>();

        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<IConfiguration>(sp =>
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(typeof(MauiProgram).Assembly.Location) ?? "")
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();
            return configuration;
        });

        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<AuthService>();
        // Dedicated HttpClient instance for payments - AuthService mutates the
        // Authorization header of the shared HttpClient singleton, which would
        // strip payment requests' bearer tokens.
        builder.Services.AddHttpClient<PaymentService>();
        builder.Services.AddHttpClient<ValidationService>();
        builder.Services.AddTransient<ViewModels.AuthViewModel>();

        builder.Services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var settings = new LlmSettings();
            config.GetSection(nameof(LlmSettings)).Bind(settings);
            var llm = new LLMService(settings);
            // Begin the first-run model download in the background so it is ready to use
            // by the time the user starts an extraction.
            llm.StartModelWarmup();
            return llm;
        });
        builder.Services.AddSingleton(sp => new AIService(sp.GetRequiredService<LLMService>()));
        builder.Services.AddSingleton(sp => new EmailService(sp.GetRequiredService<DatabaseService>()));

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        ServiceHelper.Initialize(app.Services);

        return app;
    }
}