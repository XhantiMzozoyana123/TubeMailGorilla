namespace TubeMailGorilla.Maui.Unlocked.Services;

/// <summary>
/// Imports and exports the contact list as CSV, so leads can move between
/// the app and a spreadsheet. Exported rows carry every importable field
/// (name, email, channel, video title/description/url) plus the activity
/// columns (extracted/blocked/emailed timestamps); snapshots and generated
/// content (openers, blockers) are intentionally left out - snapshots are
/// multi-kilobyte base64 blobs and icebreakers regenerate from the contact.
/// Import is the mirror: header aliases are accepted, rows without a usable
/// email are skipped, and duplicates are rejected by the database's own
/// dedupe (INSERT OR IGNORE), so re-importing an export is always safe.
/// </summary>
public static class ContactCsvService
{
    /// <summary>
    /// Header aliases accepted on import, normalised to lower-case with
    /// underscores, dashes and spaces stripped. First column wins when two
    /// headers map to the same field.
    /// </summary>
    private static readonly Dictionary<string, string> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        // email
        ["email"] = "email",
        ["emailaddress"] = "email",
        ["email_address"] = "email",
        ["mail"] = "email",
        // name
        ["name"] = "name",
        ["fullname"] = "name",
        ["full_name"] = "name",
        ["contactname"] = "name",
        ["contact_name"] = "name",
        ["firstname"] = "name",
        ["first_name"] = "name",
        ["lastname"] = "name",
        ["last_name"] = "name",
        // channel
        ["channel"] = "channel",
        ["channelname"] = "channel",
        ["channel_name"] = "channel",
        ["youtuber"] = "channel",
        ["creator"] = "channel",
        // video title
        ["videotitle"] = "videotitle",
        ["video_title"] = "videotitle",
        ["title"] = "videotitle",
        ["video"] = "videotitle",
        // video description
        ["videodescription"] = "videodescription",
        ["video_description"] = "videodescription",
        ["description"] = "videodescription",
        ["descr"] = "videodescription",
        // video url
        ["videourl"] = "videourl",
        ["video_url"] = "videourl",
        ["url"] = "videourl",
        ["link"] = "videourl",
        // timestamps
        ["extractedat"] = "extractedat",
        ["extracted_at"] = "extractedat",
        ["extracted"] = "extractedat",
        ["dateadded"] = "extractedat",
        ["date_added"] = "extractedat",
        ["lastemailed"] = "lastemailed",
        ["last_emailed"] = "lastemailed",
        ["emailed"] = "lastemailed",
        // blocked flag
        ["isblocked"] = "isblocked",
        ["blocked"] = "isblocked",
    };

    private static readonly string[] ExportHeaders =
    {
        "name", "email", "channel", "video_title", "video_description", "video_url",
        "extracted_at", "is_blocked", "last_emailed"
    };

    /// <summary>The outcome of one import, for the summary dialogue.</summary>
    public sealed record ImportResult(int Imported, int Duplicates, int Skipped);

    /// <summary>
    /// Serialises contacts to CSV text (UTF-8 with BOM so Excel detects the
    /// encoding). Fields containing commas, quotes or newlines are quoted
    /// per RFC 4180.
    /// </summary>
    public static string Build(IEnumerable<Models.EmailContact> contacts)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append('\uFEFF'); // BOM: makes Excel open the file as UTF-8
        sb.AppendLine(string.Join(",", ExportHeaders));

        foreach (var c in contacts)
        {
            sb.AppendLine(string.Join(",", new[]
            {
                Escape(c.Name),
                Escape(c.Email),
                Escape(c.Channel),
                Escape(c.VideoTitle),
                Escape(c.VideoDescription),
                Escape(c.VideoUrl),
                Escape(FormatDate(c.ExtractedAt)),
                Escape(c.IsBlocked ? "yes" : "no"),
                Escape(FormatDate(c.LastEmailed)),
            }));
        }

        return sb.ToString();
    }


    /// <summary>
    /// Parses CSV text into contacts. The header is the first non-blank row;
    /// rows without a usable email are counted as skipped, never imported.
    /// Throws <see cref="FormatException"/> when the file has no header row
    /// or no recognised columns, with a message safe to show the user.
    /// </summary>
    public static (List<Models.EmailContact> Contacts, int Skipped) Parse(string text)
    {
        // Strip a leading BOM: our own exports start with one for Excel, and
        // without this the first header cell ("﻿name") matches no alias, so
        // the whole first column would silently drop on re-import.
        var rows = SplitRows((text ?? string.Empty).TrimStart((char)65279));

        var headerIndex = rows.FindIndex(r => r.Any(f => !string.IsNullOrWhiteSpace(f)));
        if (headerIndex < 0)
            throw new FormatException("That file looks empty - no header row was found. Export your contacts first to get a correctly formatted CSV.");

        var columns = MapColumns(rows[headerIndex]);
        if (columns.Count == 0)
            throw new FormatException("None of the columns were recognised. The header should include at least an email column (export your contacts first to get a template).");

        var contacts = new List<Models.EmailContact>();
        var skipped = 0;

        for (var i = headerIndex + 1; i < rows.Count; i++)
        {
            var fields = rows[i];
            if (fields.All(string.IsNullOrWhiteSpace))
                continue; // blank line - not a contact, not an error

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (field, index) in columns)
            {
                if (index >= fields.Count) continue;
                var value = fields[index].Trim();
                if (!string.IsNullOrEmpty(value) && !values.ContainsKey(field))
                    values[field] = value;
            }

            if (!values.TryGetValue("email", out var email) || !LooksLikeEmail(email))
            {
                skipped++;
                continue;
            }

            contacts.Add(new Models.EmailContact
            {
                Email = email,
                Name = values.GetValueOrDefault("name"),
                Channel = values.GetValueOrDefault("channel"),
                VideoTitle = values.GetValueOrDefault("videotitle"),
                VideoDescription = values.GetValueOrDefault("videodescription"),
                VideoUrl = values.GetValueOrDefault("videourl"),
                ExtractedAt = ParseDate(values.GetValueOrDefault("extractedat")) ?? DateTime.Now,
                LastEmailed = ParseDate(values.GetValueOrDefault("lastemailed")),
                IsBlocked = ParseBool(values.GetValueOrDefault("isblocked")),
                UpdatedAt = DateTime.Now,
            });
        }

        return (contacts, skipped);
    }

    /// <summary>
    /// Maps recognised header cells to (field, column index). Unknown
    /// columns are ignored so extra spreadsheet columns never break import.
    /// </summary>
    private static List<(string Field, int Index)> MapColumns(List<string> header)
    {
        var columns = new List<(string, int)>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < header.Count; i++)
        {
            var normalised = new string(header[i].Trim().ToLowerInvariant()
                .Where(ch => ch != '_' && ch != '-' && ch != ' ').ToArray());
            if (HeaderAliases.TryGetValue(normalised, out var field) && seen.Add(field))
                columns.Add((field, i));
        }

        return columns;
    }

    /// <summary>
    /// Splits CSV text into rows of fields, honouring RFC 4180 quoting
    /// (quoted fields may contain commas, newlines and doubled quotes).
    /// </summary>
    private static List<List<string>> SplitRows(string text)
    {
        var rows = new List<List<string>>();
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        current.Append('"'); // escaped quote
                        i++;
                    }
                    else
                    {
                        inQuotes = false; // closing quote
                    }
                }
                else
                {
                    current.Append(ch);
                }
            }
            else if (ch == '"')
            {
                inQuotes = true;
            }
            else if (ch == ',')
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else if (ch == '\r' || ch == '\n')
            {
                if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    i++; // CRLF counts as one break
                fields.Add(current.ToString());
                current.Clear();
                rows.Add(fields);
                fields = new List<string>();
            }
            else
            {
                current.Append(ch);
            }
        }

        // Trailing content without a final newline is still a row.
        if (current.Length > 0 || fields.Count > 0)
        {
            fields.Add(current.ToString());
            rows.Add(fields);
        }

        return rows;
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r')
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
    }

    private static string FormatDate(DateTime value) =>
        value == default ? string.Empty : value.ToString("o");

    private static string FormatDate(DateTime? value) =>
        value is null || value == default ? string.Empty : value.Value.ToString("o");

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return DateTime.TryParse(value, out var parsed) ? parsed : null;
    }

    private static bool ParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var v = value.Trim().ToLowerInvariant();
        return v is "yes" or "true" or "1" or "y" or "blocked";
    }

    /// <summary>
    /// Rejects obvious non-emails (no @, no domain dot, spaces) so junk rows
    /// are skipped at import instead of failing the database insert.
    /// </summary>
    private static bool LooksLikeEmail(string value)
    {
        var at = value.IndexOf('@');
        return at > 0
            && at < value.Length - 1
            && value.IndexOf('@', at + 1) < 0
            && value.IndexOf(' ') < 0
            && value.IndexOf('.', at) > at + 1;
    }
}
