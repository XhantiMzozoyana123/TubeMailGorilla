using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TubeMailGorilla.Maui.Models;

namespace TubeMailGorilla.Maui.Services;

public class DatabaseService
{
    private const string DatabaseFilename = "tubemailgorilla.db3";
    private const SQLite.SQLiteOpenFlags Flags =
        SQLite.SQLiteOpenFlags.ReadWrite |
        SQLite.SQLiteOpenFlags.Create |
        SQLite.SQLiteOpenFlags.SharedCache;

    private static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

    private SQLite.SQLiteAsyncConnection? _connection;

    public async Task InitializeAsync()
    {
        if (_connection != null) return;

        _connection = new SQLite.SQLiteAsyncConnection(DatabasePath, Flags);

        await _connection.CreateTableAsync<EmailContact>();
        await _connection.CreateTableAsync<Blocker>();
        await _connection.CreateTableAsync<Opener>();
        await _connection.CreateTableAsync<Inboxer>();
        await _connection.CreateTableAsync<Sender>();
        await _connection.CreateTableAsync<EmailTemplate>();
        await _connection.CreateTableAsync<MessageParameter>();
    }

    public async Task<List<EmailContact>> GetContactsAsync()
    {
        await InitializeAsync();
        return await _connection!.Table<EmailContact>().ToListAsync();
    }

    public async Task<EmailContact?> GetContactAsync(int id)
    {
        await InitializeAsync();
        return await _connection!.Table<EmailContact>().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<int> AddContactAsync(EmailContact contact)
    {
        await InitializeAsync();
        return await _connection!.InsertAsync(contact);
    }

    public async Task<int> AddContactsAsync(IEnumerable<EmailContact> contacts)
    {
        await InitializeAsync();
        return await _connection!.InsertAllAsync(contacts);
    }

    public async Task<int> UpdateContactAsync(EmailContact contact)
    {
        await InitializeAsync();
        contact.UpdatedAt = DateTime.Now;
        return await _connection!.UpdateAsync(contact);
    }

    public async Task<int> DeleteContactAsync(int id)
    {
        await InitializeAsync();
        return await _connection!.DeleteAsync<EmailContact>(id);
    }

    public async Task<int> DeleteAllContactsAsync()
    {
        await InitializeAsync();
        return await _connection!.DeleteAllAsync<EmailContact>();
    }

    public async Task<List<EmailContact>> SearchContactsAsync(string query)
    {
        await InitializeAsync();
        return await _connection!.Table<EmailContact>()
            .Where(c => c.Email.Contains(query) || c.Name.Contains(query) || c.Channel.Contains(query))
            .ToListAsync();
    }

    public async Task<List<Blocker>> GetBlockersAsync()
    {
        await InitializeAsync();
        return await _connection!.Table<Blocker>().ToListAsync();
    }

    public async Task<int> AddBlockerAsync(Blocker blocker)
    {
        await InitializeAsync();
        return await _connection!.InsertAsync(blocker);
    }

    public async Task<int> RemoveBlockerAsync(int id)
    {
        await InitializeAsync();
        return await _connection!.DeleteAsync<Blocker>(id);
    }

    public async Task<bool> IsBlockedAsync(string email)
    {
        await InitializeAsync();
        return await _connection!.Table<Blocker>().CountAsync(b => b.BlockedEmail == email) > 0;
    }

    public async Task<List<Opener>> GetOpenersAsync()
    {
        await InitializeAsync();
        return await _connection!.Table<Opener>().ToListAsync();
    }

    /// <summary>
    /// All icebreakers (openers) generated for one lead. Opener.EmailerId
    /// stores the lead's Id (EmailContact.Id).
    /// </summary>
    public async Task<List<Opener>> GetOpenersForLeadAsync(int leadId)
    {
        await InitializeAsync();
        return await _connection!.Table<Opener>()
            .Where(o => o.EmailerId == leadId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> CountOpenersForLeadAsync(int leadId)
    {
        await InitializeAsync();
        return await _connection!.Table<Opener>().CountAsync(o => o.EmailerId == leadId);
    }

    public async Task<int> DeleteOpenerAsync(int id)
    {
        await InitializeAsync();
        return await _connection!.DeleteAsync<Opener>(id);
    }

    public async Task<int> SaveOpenerAsync(Opener opener)
    {
        await InitializeAsync();
        return opener.Id == 0 ? await _connection!.InsertAsync(opener) : await _connection!.UpdateAsync(opener);
    }

    public async Task<List<Inboxer>> GetInboxAsync()
    {
        await InitializeAsync();
        return await _connection!.Table<Inboxer>().OrderByDescending(i => i.ReceivedAt).ToListAsync();
    }

    public async Task<List<Inboxer>> GetUnreadAsync()
    {
        await InitializeAsync();
        return await _connection!.Table<Inboxer>().Where(i => !i.IsRead).ToListAsync();
    }

    public async Task<int> SaveInboxAsync(Inboxer inbox)
    {
        await InitializeAsync();
        return inbox.Id == 0 ? await _connection!.InsertAsync(inbox) : await _connection!.UpdateAsync(inbox);
    }

    public async Task<int> MarkInboxAsReadAsync(int id)
    {
        await InitializeAsync();
        var inbox = await _connection!.Table<Inboxer>().FirstOrDefaultAsync(i => i.Id == id);
        if (inbox == null) return 0;
        inbox.IsRead = true;
        return await _connection!.UpdateAsync(inbox);
    }

    public async Task<List<Sender>> GetSendersAsync()
    {
        await InitializeAsync();
        return await _connection!.Table<Sender>().Where(s => s.IsActive).ToListAsync();
    }

    public async Task<int> SaveSenderAsync(Sender sender)
    {
        await InitializeAsync();
        return sender.Id == 0 ? await _connection!.InsertAsync(sender) : await _connection!.UpdateAsync(sender);
    }

    public async Task<List<Sender>> GetAllSendersAsync()
    {
        await InitializeAsync();
        return await _connection!.Table<Sender>().OrderBy(s => s.Id).ToListAsync();
    }

    public async Task<int> DeleteSenderAsync(int id)
    {
        await InitializeAsync();
        return await _connection!.DeleteAsync<Sender>(id);
    }

    public async Task<List<EmailTemplate>> GetTemplatesAsync()
    {
        await InitializeAsync();
        return await _connection!.Table<EmailTemplate>().ToListAsync();
    }

    public async Task<int> SaveTemplateAsync(EmailTemplate template)
    {
        await InitializeAsync();
        return template.Id == 0 ? await _connection!.InsertAsync(template) : await _connection!.UpdateAsync(template);
    }

    public async Task<int> DeleteTemplateAsync(int id)
    {
        await InitializeAsync();
        return await _connection!.DeleteAsync<EmailTemplate>(id);
    }

    // ---------------- Custom message parameters (email tokens) ----------------

    private static readonly MessageParameter[] DefaultParameters =
    {
        new() { Token = "f_name", Field = "first-name" },
        new() { Token = "l_name", Field = "last-name" },
        new() { Token = "icebreaker", Field = "icebreaker" },
        new() { Token = "title", Field = "video-title" },
        new() { Token = "descr", Field = "video-description" },
        new() { Token = "channel", Field = "channel-name" }
    };

    /// <summary>
    /// All custom message parameters. Seeds the built-in defaults on first use
    /// so [f_name], [icebreaker], etc. work out of the box.
    /// </summary>
    public async Task<List<MessageParameter>> GetMessageParametersAsync()
    {
        await InitializeAsync();
        var list = await _connection!.Table<MessageParameter>().OrderBy(p => p.Id).ToListAsync();
        if (list.Count == 0)
        {
            await _connection.InsertAllAsync(DefaultParameters);
            list = await _connection.Table<MessageParameter>().OrderBy(p => p.Id).ToListAsync();
        }
        return list;
    }

    public async Task<int> SaveMessageParameterAsync(MessageParameter parameter)
    {
        await InitializeAsync();
        return parameter.Id == 0
            ? await _connection!.InsertAsync(parameter)
            : await _connection!.UpdateAsync(parameter);
    }

    public async Task<int> DeleteMessageParameterAsync(int id)
    {
        await InitializeAsync();
        return await _connection!.DeleteAsync<MessageParameter>(id);
    }
}
