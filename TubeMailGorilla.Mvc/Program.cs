using Hangfire;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Hangfire.Server;
using Hangfire.Storage;
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
using TubeMailGorilla.Mvc.Models;
using TubeMailGorilla.Mvc.Services;

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

// -----------------------------------------------------------------------
// TubeMailGorilla Unlocked - web edition.
//
// The same local services the MAUI Unlocked app uses are registered here:
// a private SQLite store (leads, templates, senders, inbox), yt-dlp for
// YouTube search/transcripts, Ollama for inference and SMTP/IMAP for mail.
// There is no auth/payment/validation server - PaymentService and
// ValidationService always answer "everything included", exactly like the
// desktop edition, so no feature can be locked out.
// -----------------------------------------------------------------------
var unlockedDataDirectory = builder.Configuration["DataStorage:Directory"];
if (string.IsNullOrWhiteSpace(unlockedDataDirectory))
    unlockedDataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");

builder.Services.AddSingleton(new DatabaseService(
    Path.Combine(unlockedDataDirectory, "tubemailgorilla.db3")));

// Small preference store (the web equivalent of MAUI Preferences) and the
// yt-dlp binary location - both resolved once at startup.
WebPreferences.Configure(Path.Combine(unlockedDataDirectory, "websettings.json"));
YtDlp.Configure(builder.Configuration["YtDlp:Path"], builder.Environment.ContentRootPath);
// ffmpeg decodes video frames for lead snapshots - see VideoSnapshotService.
Ffmpeg.Configure(builder.Configuration["Ffmpeg:Path"], builder.Environment.ContentRootPath);

// Ollama inference is remote, so the HttpClient timeout is managed per call.
builder.Services.AddSingleton(new HttpClient { Timeout = Timeout.InfiniteTimeSpan });
builder.Services.AddSingleton(sp =>
{
    var settings = new LlmSettings();
    builder.Configuration.GetSection(nameof(LlmSettings)).Bind(settings);
    return new LLMService(settings, sp.GetRequiredService<HttpClient>());
});

builder.Services.AddSingleton<YouTubeSearchService>();
builder.Services.AddSingleton<YouTubeTranscriptService>();
builder.Services.AddSingleton<CaptionService>();
builder.Services.AddSingleton<VideoSnapshotService>();
builder.Services.AddSingleton<AIService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<ExtractService>();
builder.Services.AddSingleton<ValidationService>();
builder.Services.AddSingleton<PaymentService>();

// ---------------------------------------------------------------------------
// Hangfire background processing
//
// Extraction moved off the request thread because a single run routinely takes
// tens of minutes (video download + ffmpeg + several Ollama calls per video),
// which reliably blew past the browser / reverse-proxy timeout. Jobs now run on
// a background server and the Extract page polls their progress.
//
// Storage is in-memory. There is no maintained Hangfire provider for SQLite
// (the only one, Hangfire.SQLite, was last published in 2017 and is broken:
// jobs enqueue and persist, but the worker never dequeues them, because its
// schema predates the FetchedAt column the modern worker polls on). MySQL
// storage is available if durability across restarts is ever needed, and the
// app already supports MySQL. In-memory is a reasonable fit here because the
// site runs as a single container: there is no second instance that would need
// to see another's queue, and the recurring schedule is persisted separately in
// WebPreferences and re-applied on startup.
// ---------------------------------------------------------------------------
builder.Services.AddHangfire(config =>
{
    config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseMemoryStorage();
});
builder.Services.AddSingleton<ExtractionJobRunner>();
builder.Services.AddSingleton<ExtractionJobScheduler>();


// Storage provider logic is now handled within AddInfrastructure in the
// shared Infrastructure layer (DependencyInjection.cs), so the MVC app
// no longer needs to override ApplicationDbContext here.


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

// AddInfrastructure() sets the default authentication/challenge schemes to
// JWT Bearer (what the API and MAUI app need). The MVC site authenticates
// with the ASP.NET Core Identity application cookie instead, so restore the
// cookie defaults AFTER the infrastructure registration - otherwise every
// signed-in user still reads as anonymous and [Authorize] challenges with a
// bare 401 instead of redirecting to /Account/Login.
builder.Services.Configure<AuthenticationOptions>(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
});

var app = builder.Build();

// -----------------------------------------------------------------------
// Database bootstrap (migrations) + admin seed, mirroring the API's Program.cs
// -----------------------------------------------------------------------
// database must not crash the site at startup - the marketing/home pages
// still render; auth-dependent pages surface the underlying DB error instead.
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    // Seed the admin account only when a password is explicitly configured.
    //
    // This used to fall back to a hardcoded literal, so an operator who simply
    // forgot AdminSeed__Password got a working admin account with a password that
    // is in public git history. Silently provisioning a privileged account is
    // the wrong default: skip seeding and say so, and let the deployment decide.
    var adminEmail = builder.Configuration["AdminSeed:Email"];
    var adminPassword = builder.Configuration["AdminSeed:Password"];

    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        app.Logger.LogWarning(
            "AdminSeed:Password is not configured, so no admin account was created. " +
            "Set AdminSeed__Password (ADMIN_SEED_PASSWORD in .env) and restart to seed one.");
    }
    else
    {

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

        // The subscription claim is a leftover from the subscription build. On
        // SQLite it can fail: the InitialCreate migration declared
        // AspNetUserClaims.Id as an int identity column while the CLR entity
        // (IdentityUserClaim<string>) uses a string key, and reading it back
        // throws an InvalidCastException. That must not abort the rest of the
        // bootstrap, because the admin account has already been created by this
        // point and the unlocked site never reads this claim.
        try
        {
            var adminClaims = await userManager.GetClaimsAsync(adminUser);
            if (!adminClaims.Any(c => c.Type == SubscriptionClaim.Type && c.Value == SubscriptionClaim.Value))
            {
                await userManager.AddClaimAsync(adminUser, new Claim(SubscriptionClaim.Type, SubscriptionClaim.Value));
            }
        }
        catch (Exception claimEx)
        {
            app.Logger.LogWarning(claimEx,
                "Could not attach the subscription claim to the admin account. This only " +
                "affects the subscription edition; the unlocked site does not read it.");
        }
        }
}
catch (Exception ex)
{
    app.Logger.LogError(ex,
        "Database bootstrap failed (migrations/admin seed). The site will still start; " +
        "sign-in, registration and subscription pages will not work until the database is reachable.");
}

// Re-apply the persisted schedule at startup. Recurring jobs live in the
// Hangfire database, so this is usually redundant - but if that file is ever
// reset the operator's cron would otherwise vanish with no trace.
try
{
    using var scope = app.Services.CreateScope();
    var scheduler = scope.ServiceProvider.GetRequiredService<ExtractionJobScheduler>();

    var cronKeyword = WebPreferences.Get("ExtractCronKeyword", string.Empty);
    var cronPageLimit = WebPreferences.Get("ExtractDefaultPageLimit", 5);
    var cronDailyTime = WebPreferences.Get("ExtractCronDailyTime", string.Empty);
    var cronHourly = WebPreferences.Get("ExtractCronHourly", false);

    if (!string.IsNullOrWhiteSpace(cronKeyword))
    {
        scheduler.SetDailySchedule(cronDailyTime, cronKeyword, cronPageLimit);
        scheduler.SetHourlySchedule(cronHourly, cronKeyword, cronPageLimit);
        app.Logger.LogInformation(
            "Re-applied extraction schedule (daily='{Daily}', every5min={Hourly}) for '{Keyword}'.",
            cronDailyTime, cronHourly, cronKeyword);
    }
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Could not re-apply the scheduled extraction settings.");
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

// Start the background job server. This is what actually executes queued
// extractions - without it jobs sit in the queue forever and the Extract page
// shows "Queued" indefinitely.
app.UseHangfireServer(new BackgroundJobServerOptions
{
    // One worker on purpose. The heavy part of extraction (yt-dlp, ffmpeg, video
    // download) is process- and bandwidth-bound, and VideoSnapshotService already
    // serialises capture with a global lock, so extra workers would only contend.
    WorkerCount = 1,
    // Jobs are enqueued onto the "extraction" queue, so the worker has to be
    // told to listen there - it only polls the "default" queue otherwise, and the
    // job would sit queued forever.
    Queues = new[] { ExtractionJobScheduler.QueueName },
    // An extraction legitimately runs for tens of minutes. Hangfire's default
    // would abandon and retry it long before a video download plus several
    // Ollama calls finish, which is the timeout this change exists to remove.
    ShutdownTimeout = TimeSpan.FromMinutes(30),
    HeartbeatInterval = TimeSpan.FromSeconds(30)
});

// Dashboard for job history, retries and the recurring schedules.
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // NOT behind [Authorize] on purpose: every MVC page in the unlocked edition
    // is [AllowAnonymous], so authorising this would be a no-op or force the
    // dashboard open. Exposing job history on a public site is a real trade -
    // if you add sign-in later, protect this route. It is served on the same
    // port as the site, so your reverse-proxy rules apply.
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Extract}/{action=Index}/{id?}");

// JSON status endpoint the Extract page polls for a running job.
app.MapGet("/extract/status/{jobId}", (string jobId) =>
{
    var status = ExtractionRunStore.Get(jobId);
    return status is null
        ? Results.NotFound(new { error = "Unknown job." })
        : Results.Ok(new
        {
            status.JobId,
            status.Keyword,
            status.State,
            status.Message,
            status.TotalVideos,
            status.EmailsFound,
            status.Errors,
            status.ElapsedLabel,
            IsActive = status.IsActive
        });
});

app.Run();
