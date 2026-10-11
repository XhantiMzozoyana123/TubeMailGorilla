namespace TubeMailGorilla.Mvc.Services;

public static partial class ContactCsvService
{
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
