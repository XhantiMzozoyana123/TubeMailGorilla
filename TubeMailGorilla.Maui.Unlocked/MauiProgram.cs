using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TubeMailGorilla.Maui.Unlocked.Models;
using TubeMailGorilla.Maui.Unlocked.Services;

namespace TubeMailGorilla.Maui.Unlocked;

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
        // The unlocked edition never talks to an auth, payment or validation
        // server - these services resolve locally and instantly, and they
        // always report "everything included".
        builder.Services.AddSingleton<PaymentService>();
        builder.Services.AddSingleton<ValidationService>();

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

#if WINDOWS
        // TEMPORARY crash/startup logging (remove once startup crash is fixed).
        var crashLogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TubeMailGorillaUnlocked", "startup-crash.log");
        Directory.CreateDirectory(Path.GetDirectoryName(crashLogPath)!);
        void LogCrash(string source, Exception ex) =>
            File.AppendAllText(crashLogPath, $"[{DateTime.Now:HH:mm:ss.fff}] {source}: {ex}\r\n\r\n");
        void LogBread(string message) =>
            File.AppendAllText(crashLogPath, $"[{DateTime.Now:HH:mm:ss.fff}] BREADCRUMB: {message}\r\n");
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            LogCrash("AppDomain.UnhandledException", (Exception)e.ExceptionObject);
        TaskScheduler.UnobservedTaskException += (_, e) =>
            LogCrash("UnobservedTaskException", e.Exception);
        Microsoft.UI.Xaml.Application.Current.UnhandledException += (_, e) =>
        {
            LogCrash("XamlUnhandledException", e.Exception);
            e.Handled = true; // keep the process alive so the log is flushed
        };
        LogBread("CreateMauiApp complete");
        _ = typeof(App).TypeInitializer; // force App type load
        App.TraceStartup += m => LogBread(m);
#endif

        return app;
    }
}