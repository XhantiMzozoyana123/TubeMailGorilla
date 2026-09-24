using Microsoft.Data.Sqlite;
using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>Sending accounts - the Sender table (SMTP credentials per account).</summary>
public partial class DatabaseService
{
    private const string InsertSenderSql =
        $"INSERT INTO {SendersTable} (Name, EmailAddress, SmtpHost, SmtpPort, SmtpUser, SmtpPassword, IsActive, CreatedAt) " +
        "VALUES (@Name, @EmailAddress, @SmtpHost, @SmtpPort, @SmtpUser, @SmtpPassword, @IsActive, @CreatedAt)";

    private const string UpdateSenderSql =
        $"UPDATE {SendersTable} SET Name = @Name, EmailAddress = @EmailAddress, SmtpHost = @SmtpHost, " +
        "SmtpPort = @SmtpPort, SmtpUser = @SmtpUser, SmtpPassword = @SmtpPassword, IsActive = @IsActive, " +
        "CreatedAt = @CreatedAt WHERE Id = @Id";

    /// <summary>Active sending accounts only - what a campaign may rotate through.</summary>
    public async Task<List<Sender>> GetSendersAsync()
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.QueryAsync(connection,
            $"SELECT {SenderCols} FROM {SendersTable} WHERE IsActive = 1 ORDER BY Id", DbHelpers.ReadSender);
    }

    public async Task<List<Sender>> GetAllSendersAsync()
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.QueryAsync(connection,
            $"SELECT {SenderCols} FROM {SendersTable} ORDER BY Id", DbHelpers.ReadSender);
    }

    public async Task<int> SaveSenderAsync(Sender sender)
    {
        await using var connection = await OpenAsync();

        if (sender.Id == 0)
        {
            return await DbHelpers.InsertAsync(connection, InsertSenderSql, c => BindSender(c, sender),
                id => sender.Id = (int)id);
        }

        return await DbHelpers.ExecuteAsync(connection, UpdateSenderSql, c =>
        {
            BindSender(c, sender);
            c.Parameters.AddWithValue("@Id", sender.Id);
        });
    }

    public async Task<int> DeleteSenderAsync(int id)
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.ExecuteAsync(connection, $"DELETE FROM {SendersTable} WHERE Id = @Id",
            c => c.Parameters.AddWithValue("@Id", id));
    }

    private static void BindSender(SqliteCommand command, Sender sender)
    {
        command.Parameters.AddWithValue("@Name", sender.Name ?? string.Empty);
        command.Parameters.AddWithValue("@EmailAddress", sender.EmailAddress ?? string.Empty);
        command.Parameters.AddWithValue("@SmtpHost", DbHelpers.Value(sender.SmtpHost));
        command.Parameters.AddWithValue("@SmtpPort", sender.SmtpPort);
        command.Parameters.AddWithValue("@SmtpUser", DbHelpers.Value(sender.SmtpUser));
        command.Parameters.AddWithValue("@SmtpPassword", DbHelpers.Value(sender.SmtpPassword));
        command.Parameters.AddWithValue("@IsActive", DbHelpers.Flag(sender.IsActive));
        command.Parameters.AddWithValue("@CreatedAt", DbHelpers.Ticks(sender.CreatedAt));
    }
}
