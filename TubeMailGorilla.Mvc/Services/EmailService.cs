using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Services;

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

        // Built-in names are always available, even when a user removed the
        // corresponding Settings row. Custom/renamed shortcodes are applied
        // afterwards and intentionally take precedence.
        var aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = fields["name"],
            ["full-name"] = fields["name"],
            ["email"] = fields["email"],
            ["channel"] = fields["channel"],
            ["channel-name"] = fields["channel-name"],
            ["first-name"] = fields["first-name"],
            ["last-name"] = fields["last-name"],
            ["video-title"] = fields["video-title"],
            ["video-description"] = fields["video-description"],
            ["icebreaker"] = fields["icebreaker"],
            ["ice-breaker"] = fields["icebreaker"]
        };

        foreach (var alias in aliases)
            result = result.Replace("[" + alias.Key + "]", alias.Value, StringComparison.OrdinalIgnoreCase);

        foreach (var p in parameters)
        {
            var token = p.Token?.Trim().Trim('[', ']') ?? string.Empty;
            if (token.Length == 0)
                continue;

            // snapshot_ai and snapshot_random are built-in tokens backed by the
            // lead's video rather than a contact field, and are resolved during
            // send. Shortcodes are freely renameable, so a user can create one
            // with either name; without this guard that row would replace the
            // bare token here and the email would ship with no image and no
            // warning.
            if (token.Equals(SnapshotAiToken.TrimStart('['), StringComparison.OrdinalIgnoreCase) ||
                token.Equals(SnapshotRandomToken.TrimStart('['), StringComparison.OrdinalIgnoreCase) ||
                SnapshotIndexTokenPattern.IsMatch("[" + token + "]"))
                continue;

            var value = fields.TryGetValue(p.Field?.Trim() ?? string.Empty, out var matched)
                ? matched
                : string.Empty;

            result = result.Replace("[" + token + "]", value, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    // ---- Snapshot token helpers (mirror MAUI Unlocked EmailService) ----

    /// <summary>
    /// The email body token that embeds an AI-chosen video snapshot. The optional
    /// {...} text is the instruction telling the vision model what to look for,
    /// e.g. [snapshot_ai={Select the frame that most needs editing.}].
    /// </summary>
    public const string SnapshotAiToken = "[snapshot_ai";

    /// <summary>
    /// The email body token that embeds one of the lead's snapshots picked at
    /// random. No AI and no vision model involved - just a frame from their own
    /// video, so it always works.
    /// </summary>
    public const string SnapshotRandomToken = "[snapshot_random";

    // The size VideoSnapshotService captures frames at, so a token shows the
    // stored image at its true resolution instead of upscaling it. The CSS in
    // BuildSnapshotImageHtml caps it on narrow screens regardless.
    private const int DefaultRandomWidth = 320;
    private const int DefaultRandomHeight = 180;

    /// <summary>
    /// Matches a [snapshot_random] token. No instruction is involved, so the
    /// braces form is not accepted - a stray {...} here would be a typo
    /// rather than an instruction.
    /// </summary>
    private static readonly Regex SnapshotRandomTokenPattern = new(
        @"\[snapshot_random\s*\]",
        RegexOptions.IgnoreCase);

    /// <summary>
    /// Matches a [snapshot_ai] token and captures its instruction. The
    /// instruction is optional, so both [snapshot_ai] and [snapshot_ai={...}]
    /// are accepted. The "=" is optional too and any whitespace around it is
    /// allowed, because a user typing the token by hand writes spaces.
    /// </summary>
    private static readonly Regex SnapshotAiTokenPattern = new(
        @"\[snapshot_ai\s*(?:=\s*\{(?<instruction>[^}]*)\})?\s*\]",
        RegexOptions.IgnoreCase);

    /// <summary>
    /// Matches a [snapshot_N] token, where N is the frame number. 1-based and
    /// deliberately digits-only: [snapshot_1] is the first frame the capture
    /// took, which matches the "1 of N" counter in the contact editor.
    /// </summary>
    private static readonly Regex SnapshotIndexTokenPattern = new(
        @"\[snapshot_(?<index>[1-9]\d*)\s*\]",
        RegexOptions.IgnoreCase);

    /// <summary>
    /// Builds the HTML for a snapshot image from its stored base64 payload.
    ///
    /// The image is inlined as a base64 data URI rather than attached or hosted,
    /// because a cold email cannot rely on the recipient's mail client fetching
    /// remote images - most block them by default, which would leave a broken
    /// image exactly where the pitch is supposed to be.
    ///
    /// The payload is stored as bare base64 but a data: URI prefix is tolerated.
    /// max-width caps it on narrow phone screens, and display:block stops the
    /// baseline gap under the image. Returns an empty string when there is no
    /// image, so an unavailable token collapses to nothing in the email.
    /// </summary>
    private static string BuildSnapshotImageHtml(string? base64Image)
    {
        if (string.IsNullOrWhiteSpace(base64Image))
            return string.Empty;

        // Stored as bare base64, but tolerate a data-URI prefix.
        var payload = base64Image.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            ? base64Image[(base64Image.IndexOf(',') + 1)..]
            : base64Image;

        return $"<img src=\"data:image/jpeg;base64,{payload}\" " +
               $"width=\"{DefaultRandomWidth}\" height=\"{DefaultRandomHeight}\" " +
               "alt=\"A still from your video\" " +
               "style=\"display:block;max-width:100%;height:auto;border:0;\" />";
    }

    /// <summary>True when the text contains a [snapshot_ai] token.</summary>
    public static bool ContainsSnapshotAiToken(string? text) =>
        !string.IsNullOrEmpty(text) && SnapshotAiTokenPattern.IsMatch(text);

    /// <summary>True when the text contains a [snapshot_random] token.</summary>
    public static bool ContainsSnapshotRandomToken(string? text) =>
        !string.IsNullOrEmpty(text) && SnapshotRandomTokenPattern.IsMatch(text);

    /// <summary>True when the text contains a [snapshot_N] token.</summary>
    public static bool ContainsSnapshotIndexToken(string? text) =>
        !string.IsNullOrEmpty(text) && SnapshotIndexTokenPattern.IsMatch(text);

    /// <summary>
    /// Replaces every [snapshot_random] token with one of the lead's own frames.
    /// No AI and no vision model involved - just a frame from their video, so it
    /// always works.
    /// </summary>
    public static string PersonalizeSnapshotRandom(string text, IReadOnlyList<string> snapshots)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        if (!SnapshotRandomTokenPattern.IsMatch(text))
            return text;

        if (snapshots is null || snapshots.Count == 0)
            return SnapshotRandomTokenPattern.Replace(text, string.Empty);

        // Random.Shared rather than new Random(): the send loop runs on one
        // thread in a tight sequence, and a fresh Random() seeded from the clock
        // can hand out the same value for several calls in a row.
        var chosen = snapshots[Random.Shared.Next(snapshots.Count)];
        return SnapshotRandomTokenPattern.Replace(text, BuildSnapshotImageHtml(chosen));
    }

    /// <summary>
    /// Replaces every [snapshot_N] token with the Nth frame of the lead's own
    /// video (1-based). An index past the end of the list (or a lead with no
    /// frames at all) renders nothing, the same as every other snapshot token,
    /// rather than failing the send.
    /// </summary>
    public static string PersonalizeSnapshotIndexed(string text, IReadOnlyList<string> snapshots)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        if (!SnapshotIndexTokenPattern.IsMatch(text))
            return text;

        if (snapshots is null || snapshots.Count == 0)
            return SnapshotIndexTokenPattern.Replace(text, string.Empty);

        return SnapshotIndexTokenPattern.Replace(text, match =>
        {
            if (!int.TryParse(match.Groups["index"].Value, out var index))
                return string.Empty;

            // 1-based authoring, 0-based storage.
            var position = index - 1;
            if (position < 0 || position >= snapshots.Count)
                return string.Empty;

            return BuildSnapshotImageHtml(snapshots[position]);
        });
    }

    /// <summary>
    /// Replaces every [snapshot_ai] token with the HTML for the image the vision
    /// model picked for this contact.
    ///
    /// The web edition has no ComfyUI/vision image pick wired in (unlike the
    /// desktop MAUI build), so it falls back to the first frame - the same
    /// result a lead with no vision model configured would get - rather than
    /// failing the send. Kept async so the call site and signature match the
    /// desktop edition.
    /// </summary>
    public static Task<string> PersonalizeSnapshotAiAsync(
        string text, EmailContact contact, AIService ai, IReadOnlyList<string> snapshots)
    {
        if (string.IsNullOrEmpty(text) || !SnapshotAiTokenPattern.IsMatch(text))
            return Task.FromResult(text);

        if (snapshots is null || snapshots.Count == 0)
            return Task.FromResult(SnapshotAiTokenPattern.Replace(text, string.Empty));

        return Task.FromResult(
            SnapshotAiTokenPattern.Replace(text, BuildSnapshotImageHtml(snapshots[0])));
    }

    public static string ToHtmlBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return string.Empty;

        if (Regex.IsMatch(body, @"<\s*(html|body|div|p|br|a|strong|em|ul|ol|li)\b", RegexOptions.IgnoreCase))
            return body;

        var paragraphs = Regex.Split(body.Replace("\r\n", "\n").Replace('\r', '\n'), @"\n\s*\n")
            .Select(paragraph => Regex.Replace(paragraph.Trim(), @"\s*\n\s*", "<br>"))
            .Where(paragraph => !string.IsNullOrWhiteSpace(paragraph))
            .Select(paragraph => $"<p>{WebUtility.HtmlEncode(paragraph).Replace("&lt;br&gt;", "<br>")}</p>");

        return string.Join(string.Empty, paragraphs);
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