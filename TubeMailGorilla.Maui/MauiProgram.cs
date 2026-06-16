using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TubeMailGorilla.Maui.ViewModels;
using TubeMailGorilla.Maui.Views;
using TubeMailGorillaApplication.Interfaces;
using TubeMailGorillaDomain.Interfaces;
using TubeMailGorillaInfrastructure.Data;
using TubeMailGorillaInfrastructure.External;
using TubeMailGorillaInfrastructure.Services;

namespace TubeMailGorilla.Maui;

public static class MauiProgram
{
    private static readonly string ModelPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "TubeMailGorilla",
        "Models",
        "Llama-3.2-1B-Instruct-Q4_K_M.gguf");

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Database
        builder.Services.AddDbContext<InfrastructureDbContext>(options =>
            options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TubeMailGorilla;Trusted_Connection=True;TrustServerCertificate=True;"));

        // Unit of Work
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Application Services
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IYouTubeSearchService, YouTubeSearchService>();
        builder.Services.AddScoped<ICaptionService, CaptionService>();
        builder.Services.AddScoped<IAIExtractorService, AIExtractorService>();

        // Local LLM AI Service
        builder.Services.AddSingleton<IAIService>(sp =>
        {
            var logger = sp.GetService<ILogger<LocalAIService>>();
            return new LocalAIService(ModelPath, logger);
        });

        // ViewModels
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<TemplatesViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<AccountsViewModel>();
        builder.Services.AddTransient<BlockedEmailsViewModel>();
        builder.Services.AddTransient<ProxiesViewModel>();
        builder.Services.AddTransient<AIAssistantViewModel>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<TemplatesPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<AccountsPage>();
        builder.Services.AddTransient<BlockedEmailsPage>();
        builder.Services.AddTransient<ProxiesPage>();
        builder.Services.AddTransient<AIAssistantPage>();

        return builder.Build();
    }
}