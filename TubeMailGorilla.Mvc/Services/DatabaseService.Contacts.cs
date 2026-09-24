using Microsoft.Data.Sqlite;
using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>Leads - the EmailContact table.</summary>
public partial class DatabaseService
{
    private const string InsertContactSql =
        $"INSERT INTO {Contacts} (Email, Name, Channel, VideoTitle, VideoDescription, ExtractedAt, IsBlocked, IsEmailer, LastEmailed, UpdatedAt) " +
        "VALUES (@Email, @Name, @Channel, @VideoTitle, @VideoDescription, @ExtractedAt, @IsBlocked, @IsEmailer, @LastEmailed, @UpdatedAt)";

    private const string UpdateContactSql =
        $"UPDATE {Contacts} SET Email = @Email, Name = @Name, Channel = @Channel, VideoTitle = @VideoTitle, " +
        "VideoDescription = @VideoDescription, ExtractedAt = @ExtractedAt, IsBlocked = @IsBlocked, " +
        "IsEmailer = @IsEmailer, LastEmailed = @LastEmailed, UpdatedAt = @UpdatedAt WHERE Id = @Id";

    private static void BindContact(SqliteCommand command, EmailContact contact)
    {
        command.Parameters.AddWithValue("@Email", contact.Email ?? string.Empty);
        command.Parameters.AddWithValue("@Name", DbHelpers.Value(contact.Name));
        command.Parameters.AddWithValue("@Channel", DbHelpers.Value(contact.Channel));
        command.Parameters.AddWithValue("@VideoTitle", DbHelpers.Value(contact.VideoTitle));
        command.Parameters.AddWithValue("@VideoDescription", DbHelpers.Value(contact.VideoDescription));
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

    public async Task<int> AddContactAsync(EmailContact contact)
    {
        await using var connection = await OpenAsync();
        return await DbHelpers.InsertAsync(connection, InsertContactSql,
            c => BindContact(c, contact), id => contact.Id = (int)id);
    }

    public async Task<int> AddContactsAsync(IEnumerable<EmailContact> contacts)
    {
        await using var connection = await OpenAsync();
        var count = 0;
        foreach (var contact in contacts)
        {
            await DbHelpers.InsertAsync(connection, InsertContactSql,
                c => BindContact(c, contact), id => contact.Id = (int)id);
            count++;
        }
        return count;
    }

    public async Task<int> UpdateContactAsync(EmailContact contact)
    {
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
