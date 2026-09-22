using TubeMailGorilla.Maui.Models;

namespace TubeMailGorilla.Maui.Services;

public class AIService
{
    private readonly LLMService _llm;

    public AIService(LLMService llm)
    {
        _llm = llm;
    }

    /// <summary>
    /// Generates a personalized cold-email first line (icebreaker) for a lead.
    /// Uses whatever context the lead has - name, channel, video title and
    /// description - so every opener references the creator's actual content.
    /// Returns null when generation fails (never persist an error string).
    /// </summary>
    public async Task<string?> GenerateIcebreakerAsync(EmailContact contact)
    {
        try
        {
            var prompt = $@"
You are an expert cold-email copywriter helping a freelance video editor land YouTube creators as retainer clients.

Write ONE personalized first-line icebreaker (the opening sentence of a cold email) for the creator below.

Creator context:
- Name: {(string.IsNullOrWhiteSpace(contact.Name) ? "unknown" : contact.Name)}
- Channel: {(string.IsNullOrWhiteSpace(contact.Channel) ? "unknown" : contact.Channel)}
- Latest video title: {(string.IsNullOrWhiteSpace(contact.VideoTitle) ? "unknown" : contact.VideoTitle)}
- Video description: {(string.IsNullOrWhiteSpace(contact.VideoDescription) ? "unknown" : Truncate(contact.VideoDescription, 500))}

Rules:
- 1 to 2 sentences maximum
- Must reference something SPECIFIC about their channel or latest video
- Complimentary but genuine - never generic ('I love your content' is banned)
- No greeting (Hi/Hey), no sign-off, no mention of editing services yet
- Plain text only, no quotes, no emojis

Return ONLY the icebreaker text.";
            var result = await _llm.GenerateTextAsync(prompt);
            return CleanIcebreaker(result);
        }
        catch
        {
            return null;
        }
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    private static string? CleanIcebreaker(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;
        // Never persist raw LLM/API error output as an icebreaker.
        if (input.StartsWith("LLM Error", StringComparison.OrdinalIgnoreCase))
            return null;
        if (input.StartsWith("No response generated", StringComparison.OrdinalIgnoreCase))
            return null;

        var clean = input.Trim().Trim('"');
        // Collapse accidental multi-line responses into a single first line block.
        clean = clean.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
        while (clean.Contains("  "))
            clean = clean.Replace("  ", " ");
        return string.IsNullOrWhiteSpace(clean) ? null : clean.Trim();
    }

    public async Task<string> GetFullNameAsync(string description, string subtitles)
    {
        try
        {
            var prompt = BuildNamePrompt($"{description} {subtitles}", "full name");
            var result = await _llm.GenerateTextAsync(prompt);
            return CleanResult(result);
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<string> GetCompanyAsync(string description, string subtitles)
    {
        try
        {
            var prompt = BuildNamePrompt($"{description} {subtitles}", "company");
            var result = await _llm.GenerateTextAsync(prompt);
            return CleanResult(result);
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<string> GetJobTitleAsync(string description, string subtitles)
    {
        try
        {
            var prompt = BuildNamePrompt($"{description} {subtitles}", "job title");
            var result = await _llm.GenerateTextAsync(prompt);
            return CleanResult(result);
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<string> GetLocationAsync(string description, string subtitles)
    {
        try
        {
            var prompt = BuildNamePrompt($"{description} {subtitles}", "location");
            var result = await _llm.GenerateTextAsync(prompt);
            return CleanResult(result);
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<string> GetIndustryAsync(string description, string subtitles)
    {
        try
        {
            var prompt = @"
You are a classification system.

Classify the industry into ONE of these exact values:
Technology, Finance, Healthcare, Education, Retail, Manufacturing, Energy, Transportation, Entertainment, Hospitality, Other

Rules:
- Return ONLY one word
- No explanation
- No punctuation

Text:
" + $"{description} {subtitles}";
            
            var result = await _llm.GenerateTextAsync(prompt);
            var clean = CleanResult(result);
            return ParseIndustry(clean);
        }
        catch
        {
            return "Other";
        }
    }

    /// <summary>
    /// Extracts all AI fields (name, company, job title, location, industry) for a
    /// lead in a SINGLE inference call. The previous version made five separate
    /// calls, each re-sending the full transcript through the local CPU model -
    /// that serialized the extraction loop for minutes per video and made the
    /// progress bar appear stuck. One call with a compact JSON answer is ~5x faster.
    /// Fields not found stay as empty strings.
    /// </summary>
    public async Task ExtractAllAsync(Emailer emailer)
    {
        var text = $"{emailer.VideoDescription} {emailer.VideoTranscript}";

        var prompt = $@"
You are a data-extraction engine. Extract the following about the CREATOR of the text below.

Return ONLY a single compact JSON object with exactly these keys:
{{""name"":"""",""company"":"""",""job"":"""",""location"":"""",""industry"":""""}}

Rules:
- Use an empty string for anything not explicitly stated in the text.
- industry must be ONE of: Technology, Finance, Healthcare, Education, Retail, Manufacturing, Energy, Transportation, Entertainment, Hospitality, Other.
- No explanations, no markdown, no code fences - ONLY the JSON object.

Text:
{text}";

        var result = await _llm.GenerateTextAsync(prompt);

        var extracted = ParseLeadJson(result);
        emailer.FullName = extracted.TryGetValue("name", out var n) ? n : string.Empty;
        emailer.Company = extracted.TryGetValue("company", out var c) ? c : string.Empty;
        emailer.Job = extracted.TryGetValue("job", out var j) ? j : string.Empty;
        emailer.Location = extracted.TryGetValue("location", out var l) ? l : string.Empty;
        emailer.Industry = extracted.TryGetValue("industry", out var i) ? ParseIndustry(i) : "Other";
    }

    /// <summary>Parses the compact JSON returned by the model, tolerating code
    /// fences and prose around it. Returns an empty dictionary on any failure.</summary>
    private static Dictionary<string, string> ParseLeadJson(string input)
    {
        var empty = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(input) || input.StartsWith("LLM Error", StringComparison.OrdinalIgnoreCase))
            return empty;

        try
        {
            var start = input.IndexOf('{');
            var end = input.LastIndexOf('}');
            if (start < 0 || end <= start)
                return empty;

            using var doc = System.Text.Json.JsonDocument.Parse(input[start..(end + 1)]);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                    result[prop.Name] = prop.Value.GetString() ?? string.Empty;
            }
            return result;
        }
        catch
        {
            return empty;
        }
    }

    private string BuildNamePrompt(string text, string target)
    {
        return $@"
Extract ONLY the person's {target} from the text below.

Examples of CORRECT behavior:
Text: ""Hi, I'm Sarah Mitchell, founder of CraftCo."" -> Sarah Mitchell
Text: ""In todays video we tour the house and show the kitchen."" -> (empty response, nothing output)

Examples of WRONG behavior (never do these):
- Outputting a list of quotes from the text
- Outputting a summary or description
- Outputting ""UNKNOWN"", ""BLANK"", ""N/A"", or ""NONE"" when not found (output nothing instead)

Return only the {target} itself - a few words maximum.

IMPORTANT: Only return a {target} that is EXPLICITLY stated in the text (e.g. introduced with ""my name is"", ""I'm"", ""this is""). Do NOT guess, invent, or pick a common name. Do NOT write words like ""Nothing"", ""None"", ""Unknown"", ""Blank"" or ""Not found"" - when it is not stated, your entire response must be a completely empty string with zero characters.

Text:
{text}";
    }

    private string CleanResult(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        // Never store a raw LLM/API error message as extracted data.
        if (input.StartsWith("LLM Error", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        var trimmed = input.Trim();
        // Belt-and-braces: if the model echoes a placeholder or a preamble, treat as not found.
        if (trimmed.Equals("UNKNOWN", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("BLANK", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("NONE", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("NOTHING", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("NOT FOUND", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("NO NAME", StringComparison.OrdinalIgnoreCase))
            return string.Empty;
        if (trimmed.StartsWith("Here is", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("The raw data", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("Sure", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        // Reject list-style output ("* item" lines, bullets, quotes) - that is a
        // summary dump, not the single data value requested.
        if (trimmed.Contains('*') || trimmed.Contains("\n- ") ||
            trimmed.StartsWith("- ") || trimmed.StartsWith("\""))
            return string.Empty;

        // The model ignored the "empty when not found" instruction and rambled -
        // a real data value is a few words, so discard anything longer.
        var wordCount = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        if (wordCount > 6)
            return string.Empty;

        return trimmed.Replace("\n", "").Replace("\r", "");
    }

    private string ParseIndustry(string value)
    {
        var validIndustries = new[]
        {
            "Technology", "Finance", "Healthcare", "Education", "Retail",
            "Manufacturing", "Energy", "Transportation", "Entertainment",
            "Hospitality", "Other"
        };

        foreach (var industry in validIndustries)
        {
            if (value.Equals(industry, StringComparison.OrdinalIgnoreCase))
                return industry;
        }

        return "Other";
    }
}