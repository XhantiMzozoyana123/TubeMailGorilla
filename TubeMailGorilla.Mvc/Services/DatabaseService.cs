using Microsoft.Data.Sqlite;
using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Local SQLite store for the web edition - the equivalent of the desktop
/// edition's <c>DatabaseService</c> (which used sqlite-net-pcl).
///
/// The MVC project targets net8.0 and already references Microsoft.Data.Sqlite
/// through Microsoft.EntityFrameworkCore.Sqlite (the Identity/subscription
/// database), so this store is built on that provider rather than adding a
/// second SQLite stack (sqlite-net-pcl would collide on SQLitePCLRaw's
/// <c>Batteries_V2</c> type).
///
/// Table and column names mirror the desktop schema, and every method keeps the
/// same name, signature and semantics, so a desktop <c>tubemailgorilla.db3</c>
/// can be copied in and read as-is.
/// </summary>
public partial class DatabaseService
{
    private const string Contacts = "EmailContact";
    private const string BlockersTable = "Blocker";
    private const string OpenersTable = "Opener";
    private const string InboxTable = "Inboxer";
    private const string SendersTable = "Sender";
    private const string TemplatesTable = "EmailTemplate";
    private const string ParametersTable = "MessageParameter";

    private const string ContactCols = "Id, Email, Name, Channel, VideoTitle, VideoDescription, ExtractedAt, IsBlocked, IsEmailer, LastEmailed, UpdatedAt";
    private const string BlockerCols = "Id, BlockedEmail, Reason, CreatedAt";
    private const string OpenerCols = "Id, EmailerId, Text, CreatedAt";
    private const string InboxCols = "Id, EmailerId, Subject, Body, IsRead, ReceivedAt, RepliedAt";
    private const string SenderCols = "Id, Name, EmailAddress, SmtpHost, SmtpPort, SmtpUser, SmtpPassword, IsActive, CreatedAt";
    private const string TemplateCols = "Id, Name, Subject, Body, CreatedAt, UpdatedAt";
    private const string ParameterCols = "Id, Token, Field";

    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    public DatabaseService(string databasePath) => DatabasePath = databasePath;

    /// <summary>The SQLite file backing this store (shown on the Settings page).</summary>
    public string DatabasePath { get; }

    private string ConnectionString => new SqliteConnectionStringBuilder
    {
        DataSource = DatabasePath,
        Mode = SqliteOpenMode.ReadWriteCreate,
        Cache = SqliteCacheMode.Shared
    }.ToString();

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        await _initLock.WaitAsync();
        try
        {
            if (_initialized) return;

            var directory = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            await using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();

            await using (var pragma = connection.CreateCommand())
            {
                pragma.CommandText = "PRAGMA journal_mode = WAL;";
                await pragma.ExecuteNonQueryAsync();
            }

            await using var command = connection.CreateCommand();
            command.CommandText = $"""
                CREATE TABLE IF NOT EXISTS "{Contacts}" ("Id" INTEGER NOT NULL CONSTRAINT "PK_EmailContact" PRIMARY KEY AUTOINCREMENT, "Email" TEXT NOT NULL, "Name" TEXT, "Channel" TEXT, "VideoTitle" TEXT, "VideoDescription" TEXT, "ExtractedAt" INTEGER NOT NULL, "IsBlocked" INTEGER NOT NULL DEFAULT 0, "IsEmailer" INTEGER NOT NULL DEFAULT 0, "LastEmailed" INTEGER, "UpdatedAt" INTEGER NOT NULL);
                CREATE TABLE IF NOT EXISTS "{BlockersTable}" ("Id" INTEGER NOT NULL CONSTRAINT "PK_Blocker" PRIMARY KEY AUTOINCREMENT, "BlockedEmail" TEXT NOT NULL, "Reason" TEXT, "CreatedAt" INTEGER NOT NULL);
                CREATE TABLE IF NOT EXISTS "{OpenersTable}" ("Id" INTEGER NOT NULL CONSTRAINT "PK_Opener" PRIMARY KEY AUTOINCREMENT, "EmailerId" INTEGER NOT NULL, "Text" TEXT NOT NULL, "CreatedAt" INTEGER NOT NULL);
                CREATE TABLE IF NOT EXISTS "{InboxTable}" ("Id" INTEGER NOT NULL CONSTRAINT "PK_Inboxer" PRIMARY KEY AUTOINCREMENT, "EmailerId" INTEGER NOT NULL, "Subject" TEXT, "Body" TEXT, "IsRead" INTEGER NOT NULL DEFAULT 0, "ReceivedAt" INTEGER NOT NULL, "RepliedAt" INTEGER);
                CREATE TABLE IF NOT EXISTS "{SendersTable}" ("Id" INTEGER NOT NULL CONSTRAINT "PK_Sender" PRIMARY KEY AUTOINCREMENT, "Name" TEXT NOT NULL, "EmailAddress" TEXT NOT NULL, "SmtpHost" TEXT, "SmtpPort" INTEGER NOT NULL DEFAULT 587, "SmtpUser" TEXT, "SmtpPassword" TEXT, "IsActive" INTEGER NOT NULL DEFAULT 1, "CreatedAt" INTEGER NOT NULL);
                CREATE TABLE IF NOT EXISTS "{TemplatesTable}" ("Id" INTEGER NOT NULL CONSTRAINT "PK_EmailTemplate" PRIMARY KEY AUTOINCREMENT, "Name" TEXT NOT NULL, "Subject" TEXT NOT NULL, "Body" TEXT NOT NULL, "CreatedAt" INTEGER NOT NULL, "UpdatedAt" INTEGER);
                CREATE TABLE IF NOT EXISTS "{ParametersTable}" ("Id" INTEGER NOT NULL CONSTRAINT "PK_MessageParameter" PRIMARY KEY AUTOINCREMENT, "Token" TEXT NOT NULL, "Field" TEXT NOT NULL);
                CREATE INDEX IF NOT EXISTS "IX_EmailContact_Email" ON "{Contacts}" ("Email");
                """;
            await command.ExecuteNonQueryAsync();

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>Opens a connection to the lead store, creating the file/tables on first use.</summary>
    private async Task<SqliteConnection> OpenAsync()
    {
        await InitializeAsync();
        var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}
