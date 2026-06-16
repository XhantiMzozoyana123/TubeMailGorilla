using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TubeMailGorillaApplication.Interfaces;
using TubeMailGorillaDomain.Interfaces;
using TubeMailGorillaInfrastructure.Data;
using TubeMailGorillaInfrastructure.External;
using TubeMailGorillaInfrastructure.Services;

namespace TubeMailGorilla
{
    internal static class Program
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        // Path where the Llama 3 GGUF model should be placed
        // Download from: https://huggingface.co/bartowski/Llama-3.2-1B-Instruct-GGUF/resolve/main/Llama-3.2-1B-Instruct-Q4_K_M.gguf
        // Or use a larger model: https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q4_K_M.gguf
        private static readonly string ModelPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Models",
            "Llama-3.2-1B-Instruct-Q4_K_M.gguf");

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            Application.Run(ServiceProvider.GetRequiredService<Forms.TubeMailGorilla>());
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Logging
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Information));

            // Database
            services.AddDbContext<InfrastructureDbContext>(options =>
                options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TubeMailGorilla;Trusted_Connection=True;TrustServerCertificate=True;"));

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Application Services
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IYouTubeSearchService, YouTubeSearchService>();
            services.AddScoped<ICaptionService, CaptionService>();
            services.AddScoped<IAIExtractorService, AIExtractorService>();

            // Local LLM AI Service (in-process, no separate server required) - Singleton so model is loaded once
            services.AddSingleton<IAIService>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<LocalAIService>>();
                return new LocalAIService(ModelPath, logger);
            });

            // Forms
            services.AddScoped<Forms.TubeMailGorilla>();
            services.AddTransient<Forms.Accounts>();
            services.AddTransient<Forms.Add_Account>();
            services.AddTransient<Forms.AI_Assistant>();
            services.AddTransient<Forms.Blocked_Email_Addresses>();
            services.AddTransient<Forms.Commentor>();
            services.AddTransient<Forms.Data_Controls>();
            services.AddTransient<Forms.Direct_Messenger>();
            services.AddTransient<Forms.Follow_Up>();
            services.AddTransient<Forms.Icebreakers>();
            services.AddTransient<Forms.Proxies>();
            services.AddTransient<Forms.Settings>();
            services.AddTransient<Forms.Templates>();
        }
    }
}
