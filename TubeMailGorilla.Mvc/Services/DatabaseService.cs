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

    private const string ContactCols = "Id, Email, Name, Channel, VideoTitle, VideoDescription, VideoUrl, VideoSnapshotJson, VideoSnapshotTimestampsJson, ExtractedAt, IsBlocked, IsEmailer, LastEmailed, UpdatedAt";
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
                CREATE TABLE IF NOT EXISTS "{Contacts}" ("Id" INTEGER NOT NULL CONSTRAINT "PK_EmailContact" PRIMARY KEY AUTOINCREMENT, "Email" TEXT NOT NULL, "Name" TEXT, "Channel" TEXT, "VideoTitle" TEXT, "VideoDescription" TEXT, "VideoUrl" TEXT, "VideoSnapshotJson" TEXT, "VideoSnapshotTimestampsJson" TEXT, "ExtractedAt" INTEGER NOT NULL, "IsBlocked" INTEGER NOT NULL DEFAULT 0, "IsEmailer" INTEGER NOT NULL DEFAULT 0, "LastEmailed" INTEGER, "UpdatedAt" INTEGER NOT NULL);
                CREATE TABLE IF NOT EXISTS "{BlockersTable}" ("Id" INTEGER NOT NULL CONSTRAINT "PK_Blocker" PRIMARY KEY AUTOINCREMENT, "BlockedEmail" TEXT NOT NULL, "Reason" TEXT, "CreatedAt" INTEGER NOT NULL);
                CREATE TABLE IF NOT EXISTS "{OpenersTable}" ("Id" INTEGER NOT NULL CONSTRAINT "PK_Opener" PRIMARY KEY AUTOINCREMENT, "EmailerId" INTEGER NOT NULL, "Text" TEXT NOT NULL, "CreatedAt" INTEGER NOT NULL);
                CREATE TABLE IF NOT EXISTS "{InboxTable}" ("Id" INTEGER NOT NULL CONSTRAINT "PK_Inboxer" PRIMARY KEY AUTOINCREMENT, "EmailerId" INTEGER NOT NULL, "Subject" TEXT, "Body" TEXT, "IsRead" INTEGER NOT NULL DEFAULT 0, "ReceivedAt" INTEGER NOT NULL, "RepliedAt" INTEGER);
                CREATE TABLE IF NOT EXISTS "{SendersTable}" ("Id" INTEGER NOT NULL CONSTRAINT "PK_Sender" PRIMARY KEY AUTOINCREMENT, "Name" TEXT NOT NULL, "EmailAddress" TEXT NOT NULL, "SmtpHost" TEXT, "SmtpPort" INTEGER NOT NULL DEFAULT 587, "SmtpUser" TEXT, "SmtpPassword" TEXT, "IsActive" INTEGER NOT NULL DEFAULT 1, "CreatedAt" INTEGER NOT NULL);
                CREATE TABLE IF NOT EXISTS "{TemplatesTable}" ("Id" INTEGER NOT NULL CONSTRAINT "PK_EmailTemplate" PRIMARY KEY AUTOINCREMENT, "Name" TEXT NOT NULL, "Subject" TEXT NOT NULL, "Body" TEXT NOT NULL, "CreatedAt" INTEGER NOT NULL, "UpdatedAt" INTEGER);
                CREATE TABLE IF NOT EXISTS "{ParametersTable}" ("Id" INTEGER NOT NULL CONSTRAINT "PK_MessageParameter" PRIMARY KEY AUTOINCREMENT, "Token" TEXT NOT NULL, "Field" TEXT NOT NULL);
                CREATE INDEX IF NOT EXISTS "IX_EmailContact_Email" ON "{Contacts}" ("Email");
                """;
            await command.ExecuteNonQueryAsync();

            // An existing database predates the snapshot columns. CREATE TABLE IF NOT
            // EXISTS is a no-op for it, so add anything missing - SQLite has no
            // "ADD COLUMN IF NOT EXISTS", hence the PRAGMA check.
            await AddMissingContactColumnsAsync(connection);

            // Runs outside the schema block because it opens its own connection. 

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }

        // Enforce one row per email address. Separate from InitializeAsync's locked
        // section because it opens its own connection (OpenAsync -> InitializeAsync).
        await EnsureUniqueContactEmailsAsync();
    }

    /// <summary>
    /// Brings an already-created EmailContact table up to the current column set.
    /// Runs on every startup: the PRAGMA lookup is one cheap query per column and
    /// the ALTER is skipped entirely once the column exists.
    /// </summary>
    private static async Task AddMissingContactColumnsAsync(SqliteConnection connection)
    {
        var expected = new (string Name, string Definition)[]
        {
            ("VideoUrl", "TEXT"),
            ("VideoSnapshotJson", "TEXT"),
            ("VideoSnapshotTimestampsJson", "TEXT")
        };

        await using var columns = connection.CreateCommand();
        columns.CommandText = $"PRAGMA table_info({Contacts});";

        var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var reader = await columns.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
                present.Add(reader.GetString(1));
        }

        foreach (var (name, definition) in expected)
        {
            if (present.Contains(name)) continue;

            await using var alter = connection.CreateCommand();
            // SQLite cannot add a NOT NULL column without a default, and every
            // one of these is nullable, so the bare type is enough.
            alter.CommandText = $"ALTER TABLE {Contacts} ADD COLUMN \"{name}\" {definition};";
            await alter.ExecuteNonQueryAsync();
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
