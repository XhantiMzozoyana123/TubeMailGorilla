using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using TubeMailGorilla.Infrastructure.Data;

namespace TubeMailGorilla.Infrastructure;

/// <summary>
/// EF Core design-time factory. Lets the `dotnet ef` CLI build the DbContext for
/// `migrations add` / `database update` without booting the whole web application.
///
/// The connection string is taken from the `DefaultConnection` environment variable,
/// falling back to the default below. Example:
///   $env:DefaultConnection = "server=...;..."   (PowerShell)
///   export DefaultConnection="server=...;..."   (bash / VPS)
///
/// The server version is pinned to MySQL 8.0 for design-time so migration files can be
/// generated without a live connection. (Runtime connection in the app still auto-detects.)
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DefaultConnection")
                        ?? "server=46.202.170.203;database=tubemailgorilladb;user=xhanti;password=Xhanti123!;port=3306;SslMode=none;AllowPublicKeyRetrieval=true";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;

        return new ApplicationDbContext(options);
    }
}