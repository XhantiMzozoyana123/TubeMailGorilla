using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>The send blocklist - the Blocker table.</summary>
public partial class DatabaseService
{
    public async Task<List<Blocker>> GetBlockersAsync()
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.QueryAsync(connection,
            $"SELECT {BlockerCols} FROM {BlockersTable} ORDER BY Id", DbHelpers.ReadBlocker);
    }

    public async Task<int> AddBlockerAsync(Blocker blocker)
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.InsertAsync(connection,
            $"INSERT INTO {BlockersTable} (BlockedEmail, Reason, CreatedAt) VALUES (@BlockedEmail, @Reason, @CreatedAt)",
            c =>
            {
                c.Parameters.AddWithValue("@BlockedEmail", blocker.BlockedEmail ?? string.Empty);
                c.Parameters.AddWithValue("@Reason", DbHelpers.Value(blocker.Reason));
                c.Parameters.AddWithValue("@CreatedAt", DbHelpers.Ticks(blocker.CreatedAt));
            },
            id => blocker.Id = (int)id);
    }

    public async Task<int> RemoveBlockerAsync(int id)
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.ExecuteAsync(connection, $"DELETE FROM {BlockersTable} WHERE Id = @Id",
            c => c.Parameters.AddWithValue("@Id", id));
    }

    public async Task<bool> IsBlockedAsync(string email)
    {
        await using var connection = await OpenAsync();
        var count = await DbHelpers.ScalarIntAsync(connection,
            $"SELECT COUNT(*) FROM {BlockersTable} WHERE BlockedEmail = @email",
            c => c.Parameters.AddWithValue("@email", email ?? string.Empty));
        return count > 0;
    }
}
