using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using TubeMailGorilla.Maui.Models;

namespace TubeMailGorilla.Maui.Services;

public class EmailService
{
    private readonly DatabaseService _db;

    public EmailService(DatabaseService db)
    {
        _db = db;
    }

    public async Task<bool> SendEmailAsync(MessengerDto message)
    {
        try
        {
            var blockers = await _db.GetBlockersAsync();
            if (blockers.Any(b => b.BlockedEmail == message.EmailTo))
                return false;

            using var smtp = new SmtpClient(message.SmtpHost, message.SmtpPort);
            smtp.Credentials = new NetworkCredential(message.SmtpUser, message.SmtpPassword);
            smtp.EnableSsl = true;

            using var mail = new MailMessage
            {
                From = new MailAddress(message.EmailFrom, message.FromName),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true
            };
            mail.To.Add(message.EmailTo);

            await smtp.SendMailAsync(mail);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ValidateEmailAsync(string email)
    {
        try
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                return false;

            var domain = email.Split('@')[1];

            // Gmail/Googlemail domains always accept mail - skip the slow DNS
            // MX lookup entirely so extraction never stalls on it.
            if (domain.Equals("gmail.com", StringComparison.OrdinalIgnoreCase) ||
                domain.Equals("googlemail.com", StringComparison.OrdinalIgnoreCase))
                return true;

            try
            {
                // Hard cap the DNS lookup so a slow/unresponsive DNS server
                // cannot stall the extraction pipeline indefinitely.
                var dnsTask = Dns.GetHostEntryAsync(domain);
                var winner = await Task.WhenAny(dnsTask, Task.Delay(TimeSpan.FromSeconds(5)));
                return winner == dnsTask && dnsTask.IsCompletedSuccessfully;
            }
            catch
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    public string ExtractEmails(string text)
    {
        try
        {
            var reg = new System.Text.RegularExpressions.Regex(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,6}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var matches = reg.Matches(text);
            if (matches.Count == 0)
                return string.Empty;
            return matches.Cast<Match>().Select(m => m.Value).Distinct().FirstOrDefault() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public string ExtractPhoneNumbers(string text)
    {
        try
        {
            var reg = new System.Text.RegularExpressions.Regex(@"\+?\d[\d -]{8,}\d");
            var matches = reg.Matches(text);
            if (matches.Count == 0)
                return string.Empty;
            return matches.Cast<Match>().Select(m => m.Value).Distinct().FirstOrDefault() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Reads up to <paramref name="maxMessages"/> messages from the given account's IMAP
    /// inbox and stores them as Inboxer entries. Returns the messages that were saved.
    /// </summary>
    public async Task<List<Inboxer>> FetchInboxMessagesAsync(Sender account, int maxMessages)
    {
        var result = new List<Inboxer>();
        try
        {
            using var client = new MailKit.Net.Imap.ImapClient();
            await client.ConnectAsync(
                account.SmtpHost ?? "imap.gmail.com",
                993,
                MailKit.Security.SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(account.SmtpUser ?? account.EmailAddress, account.SmtpPassword);
            await client.Inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);

            var uids = await client.Inbox.SearchAsync(MailKit.Search.SearchQuery.All);
            foreach (var uid in uids.OrderByDescending(u => u.Id).Take(maxMessages))
            {
                var msg = await client.Inbox.GetMessageAsync(uid);
                var inbox = new Inboxer
                {
                    EmailerId = 0,
                    Subject = msg.Subject ?? string.Empty,
                    Body = msg.TextBody ?? msg.HtmlBody ?? string.Empty,
                    ReceivedAt = msg.Date.UtcDateTime,
                    IsRead = false
                };
                await _db.SaveInboxAsync(inbox);
                result.Add(inbox);
            }

            await client.DisconnectAsync(true);
        }
        catch
        {
            // Best effort: if IMAP cannot be reached, return whatever was already saved.
        }
        return result;
    }

    /// <summary>
    /// Replaces every "[token]" in <paramref name="text"/> with the per-recipient value
    /// resolved from <paramref name="contact"/> using the given customizable parameters.
    /// <paramref name="icebreaker"/> is the AI-generated personalized first line (Opener)
    /// for this contact - it always resolves the built-in [icebreaker] / [ice-breaker]
    /// tokens, whether or not they appear in <paramref name="parameters"/>.
    /// </summary>
    public static string Personalize(string text, EmailContact contact, IEnumerable<MessageParameter> parameters, string? icebreaker = null)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["email"] = contact.Email?.Trim() ?? string.Empty,
            ["name"] = contact.Name?.Trim() ?? string.Empty,
            ["first-name"] = GetFirstName(contact.Name),
            ["last-name"] = GetLastName(contact.Name),
            ["channel"] = contact.Channel?.Trim() ?? string.Empty,
            ["channel-name"] = contact.Channel?.Trim() ?? string.Empty,
            ["video-title"] = contact.VideoTitle?.Trim() ?? string.Empty,
            ["video-description"] = contact.VideoDescription?.Trim() ?? string.Empty,
            ["icebreaker"] = icebreaker?.Trim() ?? string.Empty
        };

        var result = text;
        foreach (var p in parameters)
        {
            var token = p.Token?.Trim() ?? string.Empty;
            if (token.Length == 0)
                continue;

            var value = fields.TryGetValue(p.Field?.Trim() ?? string.Empty, out var matched)
                ? matched
                : string.Empty;

            result = result.Replace("[" + token + "]", value, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    private static string GetFirstName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;
        return name.Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? string.Empty;
    }

    private static string GetLastName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;
        var parts = name.Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .ToArray();
        return parts.Length == 0 ? string.Empty : string.Join(" ", parts);
    }
}

public class MessengerDto
{
    public string EmailFrom { get; set; } = string.Empty;
    public string? FromName { get; set; }
    public string EmailTo { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
}