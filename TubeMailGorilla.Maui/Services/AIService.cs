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

    public async Task ExtractAllAsync(Emailer emailer)
    {
        var tasks = new[]
        {
            GetFullNameAsync(emailer.VideoDescription, emailer.VideoTranscript),
            GetCompanyAsync(emailer.VideoDescription, emailer.VideoTranscript),
            GetJobTitleAsync(emailer.VideoDescription, emailer.VideoTranscript),
            GetLocationAsync(emailer.VideoDescription, emailer.VideoTranscript),
            GetIndustryAsync(emailer.VideoDescription, emailer.VideoTranscript)
        };

        var results = await Task.WhenAll(tasks);
        emailer.FullName = results[0];
        emailer.Company = results[1];
        emailer.Job = results[2];
        emailer.Location = results[3];
        emailer.Industry = results[4];
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