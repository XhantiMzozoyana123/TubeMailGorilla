using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TubeMailGorilla.Application;
using TubeMailGorilla.Domain;
using TubeMailGorilla.Domain.Constants;
using TubeMailGorilla.Infrastructure;
using TubeMailGorilla.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Don't advertise the server technology in response headers (e.g. "Server: Kestrel").
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

// -----------------------------------------------------------------------
// Presentation layer – just wires up the outer composition root.
// All domain / infrastructure concerns are registered through the
// layer-specific extension methods below, keeping Program.cs thin.
// -----------------------------------------------------------------------

// ASP.NET Core MVC controllers
builder.Services.AddControllers();

// CORS — allow the React/web front-end (and Swagger dev UI) to call the API cross-origin.
// Origins are configurable via "Cors:AllowedOrigins"; defaults to the local dev server.
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebClient", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (origins is null or { Length: 0 })
        {
            origins = new[] { "http://localhost:3000", "https://localhost:3000" };
        }
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// Application layer (use-cases)
builder.Services.AddApplication();

// Subscription plan catalog (appsettings.json "SubscriptionPlans").
// This is the single source of truth for what users can buy; the website and
// the desktop app both read it from GET /api/payments/plans.
builder.Services.Configure<SubscriptionPlansOptions>(options =>
{
    options.Plans = builder.Configuration.GetSection("SubscriptionPlans")
        .Get<List<SubscriptionPlanDefinition>>() ?? new List<SubscriptionPlanDefinition>();
});

// FREE plan limits (non-paying users). Exposed to clients through
// GET /api/payments/entitlements; change here to re-tune the free tier.
builder.Services.Configure<FreePlanLimits>(builder.Configuration.GetSection("FreePlan"));

// Infrastructure layer (EF Core, Identity, JWT, repositories, token service, PayPal gateway)
builder.Services.AddInfrastructure(builder.Configuration);

// Authorization policies
// The "Subscribed" policy requires a claim with type "subscription" and value "active".
// After a successful PayPal payment, the capture flow adds this claim to the user (persisted
// in the Identity database). The next login issues a JWT containing the claim, and
// [Authorize(Policy = "Subscribed")] will succeed on subsequent calls.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Subscribed", policy =>
        policy.RequireClaim(SubscriptionClaim.Type, SubscriptionClaim.Value));
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TubeMail Gorilla API", Version = "v1" });

    // Enable "Authorize" button in Swagger UI for JWT bearer tokens
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. " +
                      "Enter 'Bearer' followed by a space and the JWT token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// -----------------------------------------------------------------------
// Database schema bootstrap
// EF Core migrations are applied at startup via MigrateAsync(). The MySQL
// database itself must already exist (create it with `dotnet ef database
// update` or via the Docker MySQL container's MYSQL_DATABASE env var).
// -----------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Apply EF Core migrations at startup so the database schema (Identity
    // tables + Subscriptions) stays in sync with the model. This replaces the
    // older EnsureCreated + raw-SQL approach, which couldn't evolve the schema
    // when entities change.
    await db.Database.MigrateAsync();
}

// -----------------------------------------------------------------------
// HTTP request pipeline
// -----------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS must run BEFORE HTTPS redirection so the browser's preflight (OPTIONS)
// requests are handled and never receive an HTTP->HTTPS redirect (which CORS
// forbids on preflight).
app.UseCors("WebClient");

// Only enforce HTTPS redirects in non-dev environments; the local React dev
// server calls the API over plain HTTP (localhost:5076).
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
