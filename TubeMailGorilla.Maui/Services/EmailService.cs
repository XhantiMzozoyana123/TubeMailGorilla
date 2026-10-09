using System.Net;
using System.Net.Mail;
using System.Text;
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

            // Many inboxes (Gmail/Outlook) strip data: URIs, so a <img
            // src="data:..."> built by a snapshot token arrives with no image.
            // Convert every embedded data-URI into a CID attachment
            // (LinkedResource) and rewrite src="cid:..." so it renders.
            var (htmlBody, embeddedImages) = ExtractEmbeddedImages(message.Body);

            using var mail = new MailMessage
            {
                From = new MailAddress(message.EmailFrom, message.FromName),
                Subject = message.Subject,
                IsBodyHtml = true
            };
            mail.To.Add(message.EmailTo);

            if (embeddedImages.Count == 0)
            {
                mail.Body = htmlBody;
            }
            else
            {
                var htmlView = AlternateView.CreateAlternateViewFromString(
                    htmlBody, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Text.Html);
                foreach (var linked in embeddedImages)
                    htmlView.LinkedResources.Add(linked);
                mail.AlternateViews.Add(htmlView);
                // Keep Body in sync for clients that ignore AlternateViews.
                mail.Body = htmlBody;
            }

            await smtp.SendMailAsync(mail);

            foreach (var linked in embeddedImages)
                linked.Dispose();

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Pulls &lt;img src="data:image/...;base64,..."&gt; tags out of the HTML
    /// body into <see cref="LinkedResource"/> attachments. Returns the rewritten
    /// HTML (src="cid:...") plus the resources the caller must attach/dispose.
    /// A body without data-URIs comes back untouched with an empty list.
    /// </summary>
    private static (string Html, List<LinkedResource> Resources) ExtractEmbeddedImages(string? html)
    {
        var resources = new List<LinkedResource>();
        if (string.IsNullOrEmpty(html))
            return (html ?? string.Empty, resources);

        var index = 0;
        var rewritten = DataUriImagePattern.Replace(html, match =>
        {
            var mime = match.Groups["mime"].Value.ToLowerInvariant();
            var base64 = match.Groups["data"].Value;
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64);
            }
            catch
            {
                return match.Value;
            }

            if (bytes.Length == 0)
                return match.Value;

            var contentType = mime switch
            {
                "image/png" => "image/png",
                "image/gif" => "image/gif",
                _ => System.Net.Mime.MediaTypeNames.Image.Jpeg,
            };

            var contentId = $"snapshot{index}@tubemailgorilla";
            index++;

            var stream = new MemoryStream(bytes, writable: false);
            var linked = new LinkedResource(stream, contentType)
            {
                ContentId = contentId,
                TransferEncoding = System.Net.Mime.TransferEncoding.Base64
            };
            resources.Add(linked);

            var tag = match.Value;
            var replaced = DataUriSrcPattern.Replace(tag, $"src=\"cid:{contentId}\"");
            return replaced;
        });

        return (rewritten, resources);
    }

    private static readonly Regex DataUriImagePattern = new(
        @"<img\b[^>]*\bsrc\s*=\s*[""']data:(?<mime>image/(?:jpeg|jpg|png|gif));base64,(?<data>[A-Za-z0-9+/=\s]+)[""'][^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static readonly Regex DataUriSrcPattern = new(
        @"src\s*=\s*[""']data:image/(?:jpeg|jpg|png|gif);base64,[A-Za-z0-9+/=\s]+[""']",
        RegexOptions.IgnoreCase | RegexOptions.Singleline);

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

            var value = fields.TryGetValue(p.Field?.Trim() ?? string.Empty, out var matched)
                ? matched
                : string.Empty;

            result = result.Replace("[" + token + "]", value, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    /// <summary>
    /// Produces a responsive HTML email from plain text. Blank lines become
    /// paragraphs and single newlines become line breaks, so the textbox body
    /// the user typed arrives with the same line breaks. Real authored HTML
    /// (html/body/div/p/ul/ol/li/a structure) passes through unchanged, while
    /// inline tags alone must NOT skip wrapping - that skip is what collapsed
    /// every newline into one line.
    /// </summary>
    public static string ToHtmlBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return string.Empty;

        if (Regex.IsMatch(body, @"<\s*(html|body|div|p|ul|ol|li|table|a)\b", RegexOptions.IgnoreCase))
            return body;

        var paragraphs = Regex.Split(body.Replace("\r\n", "\n").Replace('\r', '\n'), @"\n\s*\n")
            .Select(paragraph => Regex.Replace(paragraph.Trim(), @"\s*\n\s*", "<br>"))
            .Where(paragraph => !string.IsNullOrWhiteSpace(paragraph))
            .Select(EncodeParagraphPreservingImages);

        return string.Join(string.Empty, paragraphs);
    }

    /// <summary>
    /// Any snapshot &lt;img&gt; tag, wherever it sits. Used to shield the tag
    /// from HTML-encoding: when the token is inline with text ("Hi ... &lt;img&gt;")
    /// the whole paragraph gets encoded, which shows as raw "&lt;img src=...&gt;"
    /// text in the inbox instead of an image.
    /// </summary>
    private static readonly Regex InlineImagePattern = new(
        @"<img\b[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline);

    /// <summary>
    /// HTML-encodes a paragraph but leaves injected snapshot &lt;img&gt; tags as
    /// real markup. Only the text around each tag is encoded, then the tags are
    /// spliced back in.
    /// </summary>
    private static string EncodeParagraphPreservingImages(string paragraph)
    {
        if (!InlineImagePattern.IsMatch(paragraph))
            return $"<p>{WebUtility.HtmlEncode(paragraph).Replace("&lt;br&gt;", "<br>")}</p>";

        var html = new StringBuilder();
        var position = 0;

        foreach (Match match in InlineImagePattern.Matches(paragraph))
        {
            var textPart = paragraph[position..match.Index];
            if (!string.IsNullOrEmpty(textPart))
                html.Append(WebUtility.HtmlEncode(textPart).Replace("&lt;br&gt;", "<br>"));
            html.Append(match.Value);
            position = match.Index + match.Length;
        }

        var tail = paragraph[position..];
        if (!string.IsNullOrEmpty(tail))
            html.Append(WebUtility.HtmlEncode(tail).Replace("&lt;br&gt;", "<br>"));

        return $"<p>{html}</p>";
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