using Microsoft.Data.Sqlite;
using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Small ADO.NET helpers shared by the <see cref="DatabaseService"/> partials so
/// every table implementation reads the same way. Values are stored exactly as
/// sqlite-net-pcl stored them in the desktop edition (booleans as 0/1, DateTime
/// as ticks), so a desktop database file stays readable.
/// </summary>
internal static class DbHelpers
{
    internal static async Task<List<T>> QueryAsync<T>(SqliteConnection connection, string sql, Func<SqliteDataReader, T> map, Action<SqliteCommand>? bind = null)
    {
        var results = new List<T>();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        bind?.Invoke(command);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            results.Add(map(reader));
        return results;
    }

    internal static async Task<int> ExecuteAsync(SqliteConnection connection, string sql, Action<SqliteCommand> bind)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        bind(command);
        return await command.ExecuteNonQueryAsync();
    }

    internal static async Task<int> ScalarIntAsync(SqliteConnection connection, string sql, Action<SqliteCommand>? bind = null)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        bind?.Invoke(command);
        return Convert.ToInt32(await command.ExecuteScalarAsync() ?? 0);
    }

    /// <summary>
    /// Runs an INSERT, assigns the generated key to the model and returns 1 -
    /// matching sqlite-net-pcl's InsertAsync contract.
    /// </summary>
    internal static async Task<int> InsertAsync(SqliteConnection connection, string sql, Action<SqliteCommand> bind, Action<long> assignId)
    {
        await ExecuteAsync(connection, sql, bind);
        var id = await ScalarIntAsync(connection, "SELECT last_insert_rowid();");
        assignId(id);
        return 1;
    }

    internal static string? Text(SqliteDataReader r, int i) => r.IsDBNull(i) ? null : r.GetString(i);
    internal static DateTime Time(SqliteDataReader r, int i) => new(r.GetInt64(i));
    internal static DateTime? NullableTime(SqliteDataReader r, int i) => r.IsDBNull(i) ? null : new DateTime(r.GetInt64(i));
    internal static long Ticks(DateTime value) => value.Ticks;
    internal static object NullableTicks(DateTime? value) => value.HasValue ? value.Value.Ticks : DBNull.Value;
    internal static int Flag(bool value) => value ? 1 : 0;
    internal static object Value(string? value) => (object?)value ?? DBNull.Value;

    internal static EmailContact ReadContact(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0), Email = r.GetString(1), Name = Text(r, 2), Channel = Text(r, 3),
        VideoTitle = Text(r, 4), VideoDescription = Text(r, 5), ExtractedAt = Time(r, 6),
        IsBlocked = r.GetInt64(7) != 0, IsEmailer = r.GetInt64(8) != 0,
        LastEmailed = NullableTime(r, 9), UpdatedAt = Time(r, 10)
    };

    internal static Blocker ReadBlocker(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0), BlockedEmail = r.GetString(1), Reason = Text(r, 2), CreatedAt = Time(r, 3)
    };

    internal static Opener ReadOpener(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0), EmailerId = r.GetInt32(1), Text = r.GetString(2), CreatedAt = Time(r, 3)
    };

    internal static Inboxer ReadInboxer(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0), EmailerId = r.GetInt32(1), Subject = Text(r, 2), Body = Text(r, 3),
        IsRead = r.GetInt64(4) != 0, ReceivedAt = Time(r, 5), RepliedAt = NullableTime(r, 6)
    };

    internal static Sender ReadSender(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0), Name = r.GetString(1), EmailAddress = r.GetString(2), SmtpHost = Text(r, 3),
        SmtpPort = r.GetInt32(4), SmtpUser = Text(r, 5), SmtpPassword = Text(r, 6),
        IsActive = r.GetInt64(7) != 0, CreatedAt = Time(r, 8)
    };

    internal static EmailTemplate ReadTemplate(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0), Name = r.GetString(1), Subject = r.GetString(2), Body = r.GetString(3),
        CreatedAt = Time(r, 4), UpdatedAt = NullableTime(r, 5)
    };

    internal static MessageParameter ReadParameter(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0), Token = r.GetString(1), Field = r.GetString(2)
    };
}
