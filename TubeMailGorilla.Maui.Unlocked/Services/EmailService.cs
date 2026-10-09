using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using TubeMailGorilla.Maui.Unlocked.Models;

namespace TubeMailGorilla.Maui.Unlocked.Services;

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
            // Underscore spellings. The task-facing token list names them this
            // way ([f_name], [l_name]), and creators type them that way too -
            // both spellings resolve to the same value.
            ["f_name"] = GetFirstName(contact.Name),
            ["l_name"] = GetLastName(contact.Name),
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
            ["f_name"] = fields["f_name"],
            ["l_name"] = fields["l_name"],
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

    /// <summary>
    /// The email body token that embeds an AI-chosen video snapshot. The optional
    /// {...} text is the instruction telling the vision model what to look for,
    /// e.g. <c>[snapshot_ai={Select the frame that most needs editing.}]</c>.
    ///
    /// Declared here (rather than beside the other snapshot_ai members further
    /// down) because Personalize above has to know the name to stop a same-named
    /// user shortcode from swallowing the token.
    /// </summary>
    public const string SnapshotAiToken = "[snapshot_ai";

    /// <summary>
    /// The email body token that embeds one of the lead's snapshots picked at
    /// random. No AI and no vision model involved - just a frame from their own
    /// video, so it always works.
    /// </summary>
    public const string SnapshotRandomToken = "[snapshot_random";

    // The size VideoSnapshotService captures frames at, so the random token shows
    // the stored image at its true resolution instead of upscaling it. The CSS in
    // BuildSnapshotImageHtml caps it on narrow screens regardless.
    private const int DefaultRandomWidth = 320;
    private const int DefaultRandomHeight = 180;

    /// <summary>
    /// Matches a [snapshot_random] token. No instruction is involved, so the
    /// braces form is not accepted - a stray <c>{...}</c> here would be a typo
    /// rather than an instruction.
    /// </summary>
    private static readonly Regex SnapshotRandomTokenPattern = new(
        @"\[snapshot_random\s*\]",
        RegexOptions.IgnoreCase);

    /// <summary>
    /// Matches a [snapshot_ai] token and captures its instruction. The
    /// instruction is optional, so both <c>[snapshot_ai]</c> and
    /// <c>[snapshot_ai={...}]</c> are accepted. The "=" is optional too and any
    /// whitespace around it is allowed, because a user typing the token by hand
    /// writes spaces. The instruction may contain any character except a closing
    /// brace, so ordinary sentence punctuation is safe; an unterminated brace is
    /// simply not matched, which leaves the text untouched rather than eating
    /// the rest of the email.
    /// </summary>
    private static readonly Regex SnapshotAiTokenPattern = new(
        @"\[snapshot_ai\s*(?:=\s*\{(?<instruction>[^}]*)\})?\s*\]",
        RegexOptions.IgnoreCase);

    /// <summary>
    /// Builds the HTML for a [snapshot_ai] token from the frame the vision model
    /// chose.
    ///
    /// The image is inlined as a base64 data URI rather than attached or hosted,
    /// because the token has to survive as plain text in a stored template and a
    /// cold email cannot rely on the recipient's mail client fetching remote
    /// images - most block them by default, which would leave a broken image
    /// exactly where the pitch is supposed to be.
    ///
    /// The width and height are the model's choice, and both are written out so
    /// the client reserves the space before the image decodes. max-width caps it
    /// on narrow phone screens, and display:block stops the baseline gap under
    /// the image. Returns an empty string when there is no image, so an
    /// unavailable token collapses to nothing in the email.
    /// </summary>
    public static string BuildSnapshotImageHtml(AIService.SnapshotAiImage? image)
    {
        if (image is null || string.IsNullOrWhiteSpace(image.Base64Image))
            return string.Empty;

        var width = Math.Clamp(image.Width, 1, 2000);
        var height = Math.Clamp(image.Height, 1, 2000);

        // The mime type comes from the image itself: PNG when an image model
        // rendered it, JPEG for the composited frames. Getting this wrong makes
        // some clients render a broken image.
        return $"<img src=\"data:{image.MimeType};base64,{image.Base64Image}\" " +
               $"width=\"{width}\" height=\"{height}\" " +
               "alt=\"A still from your video\" " +
               "style=\"display:block;max-width:100%;height:auto;border:0;\" />";
    }

    /// <summary>
    /// Replaces every [snapshot_ai] token in <paramref name="text"/> with the HTML
    /// for the image the vision model picked for this contact.
    ///
    /// Each occurrence carries its own instruction and is resolved separately, so
    /// a template can show a "before" frame and an "after" frame in one email. A
    /// contact with no snapshots - or an app with no vision model configured -
    /// simply renders no image rather than failing the send.
    /// </summary>
    public static async Task<string> PersonalizeSnapshotAiAsync(
        string text,
        EmailContact contact,
        AIService ai,
        IReadOnlyList<string> snapshots)
    {
        if (string.IsNullOrEmpty(text) || ai is null)
            return text;
        if (!SnapshotAiTokenPattern.IsMatch(text))
            return text;

        // No frames means no work: skip the (slow) vision call entirely.
        if (snapshots is null || snapshots.Count == 0)
            return SnapshotAiTokenPattern.Replace(text, string.Empty);

        // Resolved one occurrence at a time rather than via a regex evaluator,
        // because each match needs its own awaited vision call and
        // Regex.ReplaceAsync does not accept an async evaluator.
        var result = new StringBuilder();
        var position = 0;

        foreach (Match match in SnapshotAiTokenPattern.Matches(text))
        {
            result.Append(text, position, match.Index - position);

            var instruction = match.Groups["instruction"].Success
                ? match.Groups["instruction"].Value
                : null;

            var image = await ai.GenerateSnapshotAiImageAsync(contact, snapshots, instruction);
            result.Append(BuildSnapshotImageHtml(image));

            position = match.Index + match.Length;
        }

        result.Append(text, position, text.Length - position);
        return result.ToString();
    }

    /// <summary>
    /// True when the text contains a [snapshot_ai] token, so the send loop can
    /// warn that a vision model must be configured for it to produce an image.
    /// </summary>
    public static bool ContainsSnapshotAiToken(string? text) =>
        !string.IsNullOrEmpty(text) && SnapshotAiTokenPattern.IsMatch(text);

    /// <summary>
    /// Replaces every [snapshot_random] token with one of the lead's snapshots
    /// chosen at random.
    ///
    /// Deliberately has no AI in it. Unlike [snapshot_ai] this needs no vision
    /// model and no remote call, so it works on every lead and costs nothing -
    /// the snapshot is already sitting in the database as base64.
    ///
    /// Each occurrence picks independently, so a template with the token twice
    /// shows two different frames, and every recipient of a campaign gets a
    /// different image. That is the point of the token: it stops a bulk send
    /// from putting the identical picture in front of every creator.
    ///
    /// A contact with no snapshots renders nothing rather than a broken image.
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

        // No vision call and no compositing: the stored frame is already a
        // displayable image, and re-encoding it would only lose quality.
        // IsGenerated is false because nothing was generated.
        return SnapshotRandomTokenPattern.Replace(
            text,
            BuildSnapshotImageHtml(new AIService.SnapshotAiImage(chosen, DefaultRandomWidth, DefaultRandomHeight)));
    }

    /// <summary>
    /// True when the text contains a [snapshot_random] token.
    /// </summary>
    public static bool ContainsSnapshotRandomToken(string? text) =>
        !string.IsNullOrEmpty(text) && SnapshotRandomTokenPattern.IsMatch(text);

    /// <summary>
    /// Matches a [snapshot_N] token, where N is the frame number. 1-based and
    /// deliberately digits-only: <c>[snapshot_1]</c> is the first frame the
    /// capture took, which matches the "1 of N" counter in the contact editor.
    /// The zero-padded and 0 spellings are simply not matched, so they render
    /// as literal text rather than silently picking a different frame than the
    /// author expected.
    /// </summary>
    private static readonly Regex SnapshotIndexTokenPattern = new(
        @"\[snapshot_(?<index>[1-9]\d*)\s*\]",
        RegexOptions.IgnoreCase);

    /// <summary>
    /// Replaces every <c>[snapshot_N]</c> token with the Nth frame of the
    /// lead's own video (1-based).
    ///
    /// Unlike [snapshot_ai] there is no model call and unlike [snapshot_random]
    /// the choice is not left to chance: the author picks the exact moment they
    /// want shown - typically to frame the pitch around a specific scene. An
    /// index past the end of the list (or a lead with no frames at all) renders
    /// nothing, the same as every other snapshot token, rather than failing the
    /// send.
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

            return BuildSnapshotImageHtml(
                new AIService.SnapshotAiImage(snapshots[position], DefaultRandomWidth, DefaultRandomHeight));
        });
    }

    /// <summary>
    /// True when the text contains a [snapshot_N] token.
    /// </summary>
    public static bool ContainsSnapshotIndexToken(string? text) =>
        !string.IsNullOrEmpty(text) && SnapshotIndexTokenPattern.IsMatch(text);

    /// <summary>
    /// Produces a responsive HTML email from plain text. Blank lines become
    /// paragraphs and single newlines become line breaks, so the textbox body
    /// the user typed arrives with the same line breaks. Real authored HTML
    /// (html/body/div/p/ul/ol/li/table/a structure) passes through unchanged, while
    /// inline tags alone must NOT skip wrapping - that skip is what collapsed
    /// every newline into one line.
    /// </summary>
    public static string ToHtmlBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return string.Empty;

        if (Regex.IsMatch(body, @"<\s*(html|body|div|p|ul|ol|li|table|a)\b", RegexOptions.IgnoreCase))
            return body;

        return WrapParagraphs(body);
    }

    /// <summary>
    /// An injected &lt;img&gt; on a line of its own. PersonalizeSnapshotAiAsync
    /// puts one there, so a plain-text template that gained an image still has
    /// its remaining text paragraphed instead of arriving as one unstyled run.
    /// </summary>
    private static readonly Regex StandaloneImagePattern = new(
        @"^[ \t]*<img\b[^>]*>[ \t]*$",
        RegexOptions.IgnoreCase | RegexOptions.Multiline);

    private static string WrapParagraphs(string text)
    {
        var hasImage = StandaloneImagePattern.IsMatch(text);

        if (!hasImage)
            return ConvertParagraphs(text);

        var html = new StringBuilder();
        var position = 0;

        foreach (Match match in StandaloneImagePattern.Matches(text))
        {
            html.Append(ConvertParagraphs(text[position..match.Index]));
            html.Append(match.Value);
            position = match.Index + match.Length;
        }

        html.Append(ConvertParagraphs(text[position..]));
        return html.ToString();
    }

    private static string ConvertParagraphs(string text)
    {
        var paragraphs = Regex.Split(text.Replace("\r\n", "\n").Replace('\r', '\n'), @"\n\s*\n")
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