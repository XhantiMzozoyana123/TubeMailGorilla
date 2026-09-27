using Microsoft.Data.Sqlite;
using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>Leads - the EmailContact table.</summary>
public partial class DatabaseService
{
    // INSERT OR IGNORE makes duplicate rejection atomic when the unique index on
    // the normalized email exists (see EnsureUniqueContactEmailsAsync). The row count
    // is then 0 for a lead that is already stored, which is how the caller tells a
    // new lead from a duplicate.
    private const string InsertContactSql =
        $"INSERT OR IGNORE INTO {Contacts} (Email, Name, Channel, VideoTitle, VideoDescription, VideoUrl, VideoSnapshotJson, VideoSnapshotTimestampsJson, ExtractedAt, IsBlocked, IsEmailer, LastEmailed, UpdatedAt) " +
        "VALUES (@Email, @Name, @Channel, @VideoTitle, @VideoDescription, @VideoUrl, @VideoSnapshotJson, @VideoSnapshotTimestampsJson, @ExtractedAt, @IsBlocked, @IsEmailer, @LastEmailed, @UpdatedAt)";

    private const string UpdateContactSql =
        $"UPDATE {Contacts} SET Email = @Email, Name = @Name, Channel = @Channel, VideoTitle = @VideoTitle, " +
        "VideoDescription = @VideoDescription, VideoUrl = @VideoUrl, VideoSnapshotJson = @VideoSnapshotJson, " +
        "VideoSnapshotTimestampsJson = @VideoSnapshotTimestampsJson, ExtractedAt = @ExtractedAt, IsBlocked = @IsBlocked, " +
        "IsEmailer = @IsEmailer, LastEmailed = @LastEmailed, UpdatedAt = @UpdatedAt WHERE Id = @Id";

    private static void BindContact(SqliteCommand command, EmailContact contact)
    {
        command.Parameters.AddWithValue("@Email", contact.Email ?? string.Empty);
        command.Parameters.AddWithValue("@Name", DbHelpers.Value(contact.Name));
        command.Parameters.AddWithValue("@Channel", DbHelpers.Value(contact.Channel));
        command.Parameters.AddWithValue("@VideoTitle", DbHelpers.Value(contact.VideoTitle));
        command.Parameters.AddWithValue("@VideoDescription", DbHelpers.Value(contact.VideoDescription));
        command.Parameters.AddWithValue("@VideoUrl", DbHelpers.Value(contact.VideoUrl));
        // The list properties serialize into these columns; read them back.
        command.Parameters.AddWithValue("@VideoSnapshotJson", DbHelpers.Value(contact.VideoSnapshotJson));
        command.Parameters.AddWithValue("@VideoSnapshotTimestampsJson", DbHelpers.Value(contact.VideoSnapshotTimestampsJson));
        command.Parameters.AddWithValue("@ExtractedAt", DbHelpers.Ticks(contact.ExtractedAt));
        command.Parameters.AddWithValue("@IsBlocked", DbHelpers.Flag(contact.IsBlocked));
        command.Parameters.AddWithValue("@IsEmailer", DbHelpers.Flag(contact.IsEmailer));
        command.Parameters.AddWithValue("@LastEmailed", DbHelpers.NullableTicks(contact.LastEmailed));
        command.Parameters.AddWithValue("@UpdatedAt", DbHelpers.Ticks(contact.UpdatedAt));
    }

    public async Task<List<EmailContact>> GetContactsAsync()
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.QueryAsync(connection, $"SELECT {ContactCols} FROM {Contacts}", DbHelpers.ReadContact);
    }

    public async Task<EmailContact?> GetContactAsync(int id)
    {
        await using var connection = await OpenAsync();
        var contacts = await DbHelpers.QueryAsync(connection,
            $"SELECT {ContactCols} FROM {Contacts} WHERE Id = @Id", DbHelpers.ReadContact,
            c => c.Parameters.AddWithValue("@Id", id));
        return contacts.FirstOrDefault();
    }

    /// <summary>
    /// Stores a lead, ignoring an email that is already present. Returns 1 when
    /// the row was inserted and 0 when it was a duplicate, so extraction can
    /// report new leads rather than re-reporting ones it already has.
    /// </summary>
    public async Task<int> AddContactAsync(EmailContact contact)
    {
        await using var connection = await OpenAsync();
        contact.Email = NormalizeEmail(contact.Email);

        var rows = await DbHelpers.ExecuteAsync(connection, InsertContactSql,
            c => BindContact(c, contact));

        if (rows > 0)
        {
            var id = await DbHelpers.ScalarIntAsync(connection, "SELECT last_insert_rowid();");
            contact.Id = id;
        }

        return rows;
    }

    public async Task<int> AddContactsAsync(IEnumerable<EmailContact> contacts)
    {
        await using var connection = await OpenAsync();
        var count = 0;
        foreach (var contact in contacts)
        {
            contact.Email = NormalizeEmail(contact.Email);
            count += await DbHelpers.ExecuteAsync(connection, InsertContactSql,
                c => BindContact(c, contact));
        }
        return count;
    }

    /// <summary>
    /// Keeps the oldest row per normalized email and enforces that with a unique
    /// index, so extraction cannot create the same lead twice. This also cleans up
    /// databases written by older versions that allowed duplicates.
    /// </summary>
    private async Task EnsureUniqueContactEmailsAsync()
    {
        await using var connection = await OpenAsync();

        await DbHelpers.ExecuteAsync(connection,
            $"""
            DELETE FROM {Contacts}
            WHERE Id NOT IN (
                SELECT MIN(Id)
                FROM {Contacts}
                WHERE TRIM(COALESCE(Email, '')) <> ''
                GROUP BY LOWER(TRIM(Email))
            )
            AND TRIM(COALESCE(Email, '')) <> ''
            """,
            _ => { });

        // An expression index (not a plain column index) is required so that the
        // uniqueness covers trimmed + case-insensitive emails.
        await DbHelpers.ExecuteAsync(connection,
            $"CREATE UNIQUE INDEX IF NOT EXISTS UX_{Contacts}_NormalizedEmail ON {Contacts} (LOWER(TRIM(Email)))",
            _ => { });
    }

    private static string NormalizeEmail(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();

    public async Task<int> UpdateContactAsync(EmailContact contact)
    {
        contact.Email = NormalizeEmail(contact.Email);
        contact.UpdatedAt = DateTime.Now;
        await using var connection = await OpenAsync();
        return await DbHelpers.ExecuteAsync(connection, UpdateContactSql, c =>
        {
            BindContact(c, contact);
            c.Parameters.AddWithValue("@Id", contact.Id);
        });
    }

    public async Task<int> DeleteContactAsync(int id)
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.ExecuteAsync(connection, $"DELETE FROM {Contacts} WHERE Id = @Id",
            c => c.Parameters.AddWithValue("@Id", id));
    }

    public async Task<int> DeleteAllContactsAsync()
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.ExecuteAsync(connection, $"DELETE FROM {Contacts}", _ => { });
    }

    public async Task<List<EmailContact>> SearchContactsAsync(string query)
    {
        await using var connection = await OpenAsync();
        var like = $"%{query}%";
        return await DbHelpers.QueryAsync(connection,
            $"SELECT {ContactCols} FROM {Contacts} WHERE Email LIKE @q OR Name LIKE @q OR Channel LIKE @q",
            DbHelpers.ReadContact, c => c.Parameters.AddWithValue("@q", like));
    }
}
