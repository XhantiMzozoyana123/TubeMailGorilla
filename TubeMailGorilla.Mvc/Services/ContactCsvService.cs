namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Imports and exports the contact list as CSV, so leads can move between
/// the app and a spreadsheet. Mirrors the MAUI Unlocked ContactCsvService:
/// exported rows carry every importable field plus the activity columns;
/// snapshots and generated content are intentionally left out. Import accepts
/// header aliases, skips rows without a usable email, and relies on the
/// database's own dedupe (INSERT OR IGNORE), so re-importing an export is safe.
/// </summary>
public static partial class ContactCsvService
{
    private static readonly Dictionary<string, string> HeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["email"] = "email",
        ["emailaddress"] = "email",
        ["email_address"] = "email",
        ["mail"] = "email",
        ["name"] = "name",
        ["fullname"] = "name",
        ["full_name"] = "name",
        ["contactname"] = "name",
        ["contact_name"] = "name",
        ["firstname"] = "name",
        ["first_name"] = "name",
        ["lastname"] = "name",
        ["last_name"] = "name",
        ["channel"] = "channel",
        ["channelname"] = "channel",
        ["channel_name"] = "channel",
        ["youtuber"] = "channel",
        ["creator"] = "channel",
        ["videotitle"] = "videotitle",
        ["video_title"] = "videotitle",
        ["title"] = "videotitle",
        ["video"] = "videotitle",
        ["videodescription"] = "videodescription",
        ["video_description"] = "videodescription",
        ["description"] = "videodescription",
        ["descr"] = "videodescription",
        ["videourl"] = "videourl",
        ["video_url"] = "videourl",
        ["url"] = "videourl",
        ["link"] = "videourl",
        ["extractedat"] = "extractedat",
        ["extracted_at"] = "extractedat",
        ["extracted"] = "extractedat",
        ["dateadded"] = "extractedat",
        ["date_added"] = "extractedat",
        ["lastemailed"] = "lastemailed",
        ["last_emailed"] = "lastemailed",
        ["emailed"] = "lastemailed",
        ["isblocked"] = "isblocked",
        ["blocked"] = "isblocked",
    };

    private static readonly string[] ExportHeaders =
    {
        "name", "email", "channel", "video_title", "video_description", "video_url",
        "extracted_at", "is_blocked", "last_emailed"
    };

    /// <summary>The outcome of one import, for the summary message.</summary>
    public sealed record ImportResult(int Imported, int Duplicates, int Skipped);

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

    public static (List<Models.EmailContact> Contacts, int Skipped) Parse(string text)
    {
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
                continue;

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
}
