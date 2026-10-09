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
        builder.Services.AddSingleton<VideoSnapshotService>();
        builder.Services.AddSingleton<ExtractService>();

        builder.Services.AddHttpClient();

        // The shared HttpClient must not carry HttpClient's default 100 second
        // timeout. LLMService applies its own per-request budget
        // (InferenceTimeoutSeconds, currently 300s) through a
        // CancellationTokenSource, and a CPU-only Ollama host needs ~190s just
        // to answer the longer video-review prompt. A 100s ceiling here silently
        // killed those calls first and the "Inference timed out after 300s"
        // error message then blamed the wrong limit.
        builder.Services.AddSingleton(_ => new HttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan
        });
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

        // Probe the local Ollama server in the background so the model is ready
        // before the first extraction. Inference is Ollama's /api/chat over
        // HTTP on this machine - no GGUF file and no VPS.
        builder.Services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var settings = new LlmSettings();
            config.GetSection(nameof(LlmSettings)).Bind(settings);
            return settings;
        });
        builder.Services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<LlmSettings>();
            var llm = new LLMService(settings);
            llm.StartModelWarmup();
            return llm;
        });
        // Image generation is an optional capability wired to a local ComfyUI
        // server. ComfyUiImageGenerationService self-gates on ComfyUiBaseUrl:
        // empty (the default) means IsConfigured is false and it never renders,
        // so AIService composites the lead's real frames - identical to the old
        // no-op. Set ComfyUiBaseUrl + ComfyUiCheckpoint in appsettings.json once
        // ComfyUI is running and the SAME registration starts repainting the
        // picked frame per the {instruction}, so the [snapshot_ai] prompt
        // actually changes the picture.
        builder.Services.AddSingleton<IImageGenerationService, ComfyUiImageGenerationService>();
        builder.Services.AddSingleton(sp => new AIService(
            sp.GetRequiredService<LLMService>(),
            sp.GetRequiredService<IImageGenerationService>()));
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