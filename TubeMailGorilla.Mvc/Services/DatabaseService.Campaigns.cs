using Microsoft.Data.Sqlite;
using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>AI icebreakers (Opener) and received replies (Inboxer).</summary>
public partial class DatabaseService
{
    private const string InsertOpenerSql =
        $"INSERT INTO {OpenersTable} (EmailerId, Text, CreatedAt) VALUES (@EmailerId, @Text, @CreatedAt)";

    private const string InsertInboxSql =
        $"INSERT INTO {InboxTable} (EmailerId, Subject, Body, IsRead, ReceivedAt, RepliedAt) " +
        "VALUES (@EmailerId, @Subject, @Body, @IsRead, @ReceivedAt, @RepliedAt)";

    private const string UpdateInboxSql =
        $"UPDATE {InboxTable} SET EmailerId = @EmailerId, Subject = @Subject, Body = @Body, " +
        "IsRead = @IsRead, ReceivedAt = @ReceivedAt, RepliedAt = @RepliedAt WHERE Id = @Id";

    // ---------------------------------- Openers ----------------------------------

    public async Task<List<Opener>> GetOpenersAsync()
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.QueryAsync(connection,
            $"SELECT {OpenerCols} FROM {OpenersTable} ORDER BY Id", DbHelpers.ReadOpener);
    }

    public async Task<List<Opener>> GetOpenersForLeadAsync(int leadId)
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.QueryAsync(connection,
            $"SELECT {OpenerCols} FROM {OpenersTable} WHERE EmailerId = @leadId ORDER BY Id", DbHelpers.ReadOpener,
            c => c.Parameters.AddWithValue("@leadId", leadId));
    }

    public async Task<int> CountOpenersForLeadAsync(int leadId)
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.ScalarIntAsync(connection,
            $"SELECT COUNT(*) FROM {OpenersTable} WHERE EmailerId = @leadId",
            c => c.Parameters.AddWithValue("@leadId", leadId));
    }

    public async Task<int> DeleteOpenerAsync(int id)
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.ExecuteAsync(connection, $"DELETE FROM {OpenersTable} WHERE Id = @Id",
            c => c.Parameters.AddWithValue("@Id", id));
    }

    public async Task<int> SaveOpenerAsync(Opener opener)
    {
        await using var connection = await OpenAsync();

        if (opener.Id == 0)
        {
            return await DbHelpers.InsertAsync(connection, InsertOpenerSql, c =>
            {
                c.Parameters.AddWithValue("@EmailerId", opener.EmailerId);
                c.Parameters.AddWithValue("@Text", opener.Text ?? string.Empty);
                c.Parameters.AddWithValue("@CreatedAt", DbHelpers.Ticks(opener.CreatedAt));
            }, id => opener.Id = (int)id);
        }

        return await DbHelpers.ExecuteAsync(connection,
            $"UPDATE {OpenersTable} SET EmailerId = @EmailerId, Text = @Text, CreatedAt = @CreatedAt WHERE Id = @Id",
            c =>
            {
                c.Parameters.AddWithValue("@EmailerId", opener.EmailerId);
                c.Parameters.AddWithValue("@Text", opener.Text ?? string.Empty);
                c.Parameters.AddWithValue("@CreatedAt", DbHelpers.Ticks(opener.CreatedAt));
                c.Parameters.AddWithValue("@Id", opener.Id);
            });
    }

    // ----------------------------------- Inbox -----------------------------------

    public async Task<List<Inboxer>> GetInboxAsync()
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.QueryAsync(connection,
            $"SELECT {InboxCols} FROM {InboxTable} ORDER BY ReceivedAt DESC, Id DESC", DbHelpers.ReadInboxer);
    }

    public async Task<List<Inboxer>> GetUnreadAsync()
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.QueryAsync(connection,
            $"SELECT {InboxCols} FROM {InboxTable} WHERE IsRead = 0 ORDER BY ReceivedAt DESC, Id DESC", DbHelpers.ReadInboxer);
    }

    public async Task<int> SaveInboxAsync(Inboxer inbox)
    {
        await using var connection = await OpenAsync();

        if (inbox.Id == 0)
        {
            return await DbHelpers.InsertAsync(connection, InsertInboxSql, c => BindInbox(c, inbox),
                id => inbox.Id = (int)id);
        }

        return await DbHelpers.ExecuteAsync(connection, UpdateInboxSql, c =>
        {
            BindInbox(c, inbox);
            c.Parameters.AddWithValue("@Id", inbox.Id);
        });
    }

    public async Task<int> MarkInboxAsReadAsync(int id)
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.ExecuteAsync(connection,
            $"UPDATE {InboxTable} SET IsRead = 1 WHERE Id = @Id",
            c => c.Parameters.AddWithValue("@Id", id));
    }

    private static void BindInbox(SqliteCommand command, Inboxer inbox)
    {
        command.Parameters.AddWithValue("@EmailerId", inbox.EmailerId);
        command.Parameters.AddWithValue("@Subject", DbHelpers.Value(inbox.Subject));
        command.Parameters.AddWithValue("@Body", DbHelpers.Value(inbox.Body));
        command.Parameters.AddWithValue("@IsRead", DbHelpers.Flag(inbox.IsRead));
        command.Parameters.AddWithValue("@ReceivedAt", DbHelpers.Ticks(inbox.ReceivedAt));
        command.Parameters.AddWithValue("@RepliedAt", DbHelpers.NullableTicks(inbox.RepliedAt));
    }
}
