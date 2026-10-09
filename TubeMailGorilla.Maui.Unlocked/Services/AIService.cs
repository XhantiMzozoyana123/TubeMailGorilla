using System.Text.RegularExpressions;
using TubeMailGorilla.Maui.Unlocked.Models;

namespace TubeMailGorilla.Maui.Unlocked.Services;

public class AIService
{
    // Display-size bounds for the [snapshot_ai] email image. An email is read on
    // a phone first, so anything wider than 640px forces horizontal scrolling,
    // and anything under 240px is too small to judge the creator's footage.
    private const int MinImageWidth = 240;
    private const int MaxImageWidth = 640;
    private const int MinImageHeight = 135;
    private const int MaxImageHeight = 360;

    // 16:9, matching the aspect ratio the snapshots are captured at.
    private const int DefaultImageWidth = 480;
    private const int DefaultImageHeight = 270;

    // Token budgets for an icebreaker. qwen3 emits a reasoning pass before the
    // answer (measured 500-650 tokens on qwen3-vl:4b), so the cap covers
    // thinking AND the 1-2 sentence answer. A hard custom prompt can still
    // think past the first budget and come back empty; the retry buys 50% more
    // room (~9 tokens/second measured keeps it well inside the 300s budget).
    private const int IcebreakerMaxTokens = 1200;
    private const int IcebreakerRetryTokens = 1800;

    // The default LLMService system prompt is the strict DATA-EXTRACTION one -
    // it demands a single value of 1-5 words and an empty answer when unsure.
    // An icebreaker is 1-2 creative SENTENCES, so the model spends most of its
    // thinking budget reconciling that conflict (measured: custom-instruction
    // prompts burned past even the 1800-token retry and came back empty,
    // surfacing as the generic "AI timed out" dialog). Icebreakers get their
    // own copywriter persona - the same reasoning LLMService applies to its
    // advisory prompt, which bans the extraction persona for creative work.
    private const string IcebreakerSystemPrompt =
        "You are an expert cold-email copywriter. " +
        "Answer with ONLY the short piece of copy the user asks for - typically one or two sentences. " +
        "No preamble, no explanations, no lists, no quotation marks around the answer.";

    private readonly LLMService _llm;
    private readonly IImageGenerationService _imageGeneration;

    /// <summary>
    /// True when a vision model is available for frame picking. The send page
    /// uses this for its progress text; the pipeline itself falls back to the
    /// first frame when false, so the token still renders an image.
    /// </summary>
    public bool VisionAvailable => _llm.SupportsVision;

    /// <param name="llm">Used for reasoning: picking frames and writing the edit prompt.</param>
    /// <param name="imageGeneration">
    /// Renders the finished image. Optional on purpose - with no image model
    /// configured the service falls back to compositing the lead's real frames,
    /// so the token still produces an honest image. Defaults to the no-op
    /// implementation rather than null so callers never have to null-check.
    /// </param>
    public AIService(LLMService llm, IImageGenerationService? imageGeneration = null)
    {
        _llm = llm;
        _imageGeneration = imageGeneration ?? new NullImageGenerationService();
    }

    /// <summary>
    /// Generates a personalized cold-email first line (icebreaker) for a lead.
    /// Uses whatever context the lead has - name, channel, video title and
    /// description - so every opener references the creator's actual content.
    /// Returns null when generation fails (never persist an error string).
    /// </summary>
    public async Task<string?> GenerateIcebreakerAsync(EmailContact contact)
        => await GenerateIcebreakerAsync(contact, string.Empty);

    /// <summary>
    /// Generates an icebreaker while honoring the user's custom instructions.
    /// The instruction supplements the contact context and never replaces the
    /// safety constraints that keep the result suitable for an email opener.
    /// </summary>
    public async Task<string?> GenerateIcebreakerAsync(EmailContact contact, string customInstructions)
    {
        try
        {
            var instructions = string.IsNullOrWhiteSpace(customInstructions)
                ? string.Empty
                : $"\nCustom instructions from the user (follow them unless they ask for a greeting, sign-off, contact details, or more than 2 sentences):\n{customInstructions.Trim()}\n";

            var prompt = $@"
You are an expert cold-email copywriter helping a freelance video editor land YouTube creators as retainer clients.

Write ONE personalized first-line icebreaker (the opening sentence of a cold email) for the creator below.

Creator context:
- Name: {(string.IsNullOrWhiteSpace(contact.Name) ? "unknown" : contact.Name)}
- Channel: {(string.IsNullOrWhiteSpace(contact.Channel) ? "unknown" : contact.Channel)}
- Latest video title: {(string.IsNullOrWhiteSpace(contact.VideoTitle) ? "unknown" : contact.VideoTitle)}
- Video description: {(string.IsNullOrWhiteSpace(contact.VideoDescription) ? "unknown" : Truncate(contact.VideoDescription, 500))}
{instructions}
Rules:
- 1 to 2 sentences maximum
- Must reference something SPECIFIC about their channel or latest video
- Complimentary but genuine - never generic ('I love your content' is banned)
- No greeting (Hi/Hey), no sign-off, no mention of editing services yet
- Plain text only, no quotes, no emojis

Return ONLY the icebreaker text.";
            var result = await _llm.GenerateTextAsync(
                prompt, maxTokens: IcebreakerMaxTokens, systemPrompt: IcebreakerSystemPrompt);
            // Budget covers qwen3's thinking trace PLUS the 1-2 sentence answer:
            // reasoning models spend tokens thinking before writing anything, and
            // a tight cap (160) meant the budget ran out mid-thought, returning an
            // empty response the caller treated as failure.
            var icebreaker = CleanIcebreaker(result);
            if (icebreaker is not null)
                return icebreaker;

            // Empty response = the thinking trace ate the whole budget (qwen3-vl
            // ignores Ollama's think=false and /no_think - ollama/ollama#16945).
            // One retry with 50% more room turns that case into a success. Probe
            // failures and timeouts are NOT retried - they would just double the
            // wait before failing the same way.
            if (result.StartsWith("LLM Error: No response", StringComparison.OrdinalIgnoreCase))
            {
                var retry = await _llm.GenerateTextAsync(
                    prompt, maxTokens: IcebreakerRetryTokens, systemPrompt: IcebreakerSystemPrompt);
                icebreaker = CleanIcebreaker(retry);
            }
            return icebreaker;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// A frame the vision model picked for an email, together with the display
    /// dimensions it chose for it.
    ///
    /// This is the payload of the <c>[snapshot_ai]</c> email token. It carries the
    /// image data rather than markup so the HTML is built in one place (see
    /// <c>EmailService.BuildSnapshotImageHtml</c>). The model is asked to choose
    /// an image and a size, never to write HTML, and echoing a large base64 blob
    /// back through a text model is unreliable anyway.
    /// </summary>
    public class SnapshotAiImage
    {
        public SnapshotAiImage(string base64Image, int width, int height, bool isGenerated = false)
        {
            Base64Image = base64Image;
            Width = width;
            Height = height;
            IsGenerated = isGenerated;
        }

        /// <summary>Base64 image data (no data-URI prefix).</summary>
        public string Base64Image { get; }

        /// <summary>Display width in pixels, chosen by the model.</summary>
        public int Width { get; }

        /// <summary>Display height in pixels, chosen by the model.</summary>
        public int Height { get; }

        /// <summary>
        /// True when an image model rendered this, false when the lead's real
        /// frames were composited instead. The difference matters: a generated
        /// image is only a truthful "here is my edit" when a model actually ran,
        /// so callers may want to word the email differently for each.
        /// </summary>
        public bool IsGenerated { get; }

        /// <summary>
        /// The mime type for the data URI. A generated image is normally PNG;
        /// the composited fallback is JPEG, which is what the frames are.
        /// </summary>
        public string MimeType => IsGenerated ? "image/png" : "image/jpeg";
    }

    /// <summary>
    /// Picks the frames of a lead's video that best match what the caller wants
    /// to show, and composites them into ONE base64 JPEG.
    ///
    /// The [snapshot_ai] token lets a template say in plain English what the image
    /// is for - "show a before and after" or "pick the clips that need editing" -
    /// and that instruction drives the vision model's choice. One instruction can
    /// ask for a single frame, a before/after pair, or a grid of clips; the model
    /// answers with a list of frame positions plus the layout to use.
    ///
    /// Only the *decision* comes back from the model. The frames are resolved
    /// locally by position and drawn by <see cref="SnapshotImageComposer"/>,
    /// because the stack cannot generate an image - it can only choose between
    /// real ones. Every pixel in the result is genuinely from the lead's video.
    ///
    /// Always returns an image when the lead has snapshots: with vision the
    /// model picks the frames matching the instruction, without vision (or on
    /// failure) the first sampled frame is composited instead. Returns null
    /// only when there is nothing to show (no snapshots at all) so the token
    /// renders as nothing rather than leaking an error into a cold email.
    /// </summary>
    public async Task<SnapshotAiImage?> GenerateSnapshotAiImageAsync(
        EmailContact contact,
        IReadOnlyList<string> snapshots,
        string? instruction = null)
    {
        var frames = CleanFrames(snapshots);
        if (frames.Count == 0)
            return null;

        // Must be the exact list that goes over the wire, so the reported
        // positions index frames the model actually saw. CleanFrames already
        // ran, so SelectImageSample only thins the list - positions stay
        // aligned with what BuildImageAsync resolves.
        var sent = _llm.SelectImageSample(frames);

        // No vision model (not configured, not pulled, or Ollama down): the AI
        // cannot choose, but the token must still show the creator's footage
        // rather than vanishing. Fall back to the first sampled frame so the
        // email always carries an honest image.
        if (!_llm.SupportsVision)
            return ComposeFallback(sent);

        var goal = string.IsNullOrWhiteSpace(instruction)
            ? "Pick the frames that most clearly show what could be improved."
            : TrimInstruction(instruction);

        var prompt = $@"
You are a video editor planning a proof-of-concept image for a cold email to a
YouTube creator, from a freelance video editor offering their services.

What the image must show: {goal}

Video context:
- Title: {(string.IsNullOrWhiteSpace(contact.VideoTitle) ? "unknown" : contact.VideoTitle)}
- Channel: {(string.IsNullOrWhiteSpace(contact.Channel) ? "unknown" : contact.Channel)}

You have been given {sent.Count} frame(s), numbered 1 to {sent.Count} in order.

Do two things:

1. Pick the frames to use in ""selectedSnapshots"". Put the weakest-looking frame
   first and the strongest last.
   - 1 frame when the goal asks for a single image.
   - 2 frames when the goal asks for a before and after, or a comparison.
   - Up to {SnapshotImageComposer.MaxFrames} frames when the goal asks for several clips.
   - Only frames you can actually see. Never invent an index.

2. Write ""editPrompt"": the image-editing instruction describing how those frames
   should be turned into the finished image. Describe the edit, the look and the
   layout. Do not mention frame numbers, the creator, or that these are
   screenshots. Keep it under 40 words.

Also choose the display size for the finished image.
Rules:
- Answer with ONE line of JSON and nothing else. No explanation, no prose.
- Format:
  {{""selectedSnapshots"": [1, 2], ""editPrompt"": ""<your edit instruction>"", ""width"": <px>, ""height"": <px>}}
- width between 240 and {SnapshotImageComposer.MaxWidth}, height between 135 and 360.

Return only that JSON object.";

        try
        {
            var result = await _llm.GenerateAdvisoryAsync(
                prompt,
                maxTokens: 1200,
                base64Images: sent);

            return await BuildImageAsync(result, frames, sent, _imageGeneration);
        }
        catch
        {
            // Vision call failed (timeout, server down, bad response): still
            // show the creator's footage rather than shipping no image.
            return ComposeFallback(sent);
        }
    }

    /// <summary>
    /// Strips anything that is not image data from stored frames: data-URI
    /// prefixes ("data:image/jpeg;base64,..."), whitespace and newlines folded
    /// into the stored string, and blank entries. Without this a frame saved
    /// with a prefix builds "data:...;base64,data:..." (no inbox renders it),
    /// and folded whitespace makes SkiaSharp/the data-URI fail to decode, so
    /// the composer returns null and the token silently renders nothing.
    /// </summary>
    private static List<string> CleanFrames(IReadOnlyList<string>? frames)
    {
        var cleaned = new List<string>();
        if (frames is null)
            return cleaned;

        foreach (var frame in frames)
        {
            if (string.IsNullOrWhiteSpace(frame))
                continue;

            var value = frame.Trim();
            var comma = value.IndexOf(',');
            if (value.StartsWith("data:image", StringComparison.OrdinalIgnoreCase) && comma >= 0)
                value = value[(comma + 1)..];

            value = new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
            if (value.Length == 0)
                continue;

            cleaned.Add(value);
        }

        return cleaned;
    }

    /// <summary>
    /// The no-AI fallback for [snapshot_ai]: composite the sampled frames (or
    /// just pass through the single one) so the token always renders the
    /// lead's real footage. Returns null only when there is genuinely nothing
    /// to show.
    /// </summary>
    private static SnapshotAiImage? ComposeFallback(IReadOnlyList<string> sent)
    {
        var usable = CleanFrames(sent);
        if (usable.Count == 0)
            return null;

        var composed = SnapshotImageComposer.ComposeBase64(usable);
        if (composed is null)
            return null;

        // A single frame passes through untouched, so measure the composer
        // output rather than assuming the default size.
        return new SnapshotAiImage(composed, DefaultImageWidth, DefaultImageHeight, isGenerated: false);
    }

    /// raising watch time and engagement - the analysis a freelance editor
    /// would put in a review before quoting a retainer.
    ///
    /// Input is whatever is available: the lead's snapshots (when a vision
    /// model is configured), the video transcript, and the video's title,
    /// description and length. A video with no transcript and no vision model
    /// yields very little, and the prompt says so rather than inviting the
    /// model to invent findings.
    ///
    /// Returns null on failure so the UI can show a retry instead of storing an
    /// error string as if it were analysis.
    /// </summary>
    public async Task<string?> GenerateVideoImprovementsAsync(
        EmailContact contact,
        string transcript,
        IReadOnlyList<string>? snapshots = null)
    {
        try
        {
            // Duration is known exactly: the last snapshot is the final frame taken.
            var stamps = contact.VideoSnapshotTimestamps;
            var durationSeconds = stamps.Count > 0 ? stamps[^1] : 0d;
            var duration = durationSeconds > 0
                ? TimeSpan.FromSeconds(durationSeconds)
                : TimeSpan.Zero;

            var transcriptText = string.IsNullOrWhiteSpace(transcript)
                ? "(no transcript available for this video)"
                : Truncate(transcript.Trim(), 4000);

            var visionNote = _llm.SupportsVision
                ? "Sample frames from across the video are attached. Comment on what you can actually see in them " +
                  "(framing, cuts, text on screen, visual pacing) and do not claim to see anything that is not visible."
                : "NOTE: you were NOT given the video frames - only the transcript and metadata. " +
                  "Base your advice on the script, structure, topics and pacing you can infer from that text, " +
                  "and do not describe what the video visually looks like.";

            var prompt = $@"
Review this YouTube video and give the editor specific changes that would raise watch time, retention and engagement.

Video details:
- Title: {(string.IsNullOrWhiteSpace(contact.VideoTitle) ? "unknown" : contact.VideoTitle)}
- Channel: {(string.IsNullOrWhiteSpace(contact.Channel) ? "unknown" : contact.Channel)}
- Length: {(duration > TimeSpan.Zero ? $"{(int)duration.TotalMinutes}m {duration.Seconds}s" : "unknown")}
- Description: {(string.IsNullOrWhiteSpace(contact.VideoDescription) ? "unknown" : Truncate(contact.VideoDescription, 700))}

Transcript:
{transcriptText}

{visionNote}

Answer under these four headings, with at most 2 short points under each. Be brief - one sentence per point.

HOOK - the opening line or first shot to change, and why viewers leave now.
PACING - the one cut or reorder that would most improve retention.
PACKAGING - the specific title and thumbnail change.
CTA - the end screen or call to action to add, and where.

Reference the transcript where relevant. Skip a heading only if you genuinely have nothing to say.";

            // Token budget must cover qwen3's thinking trace AND the answer:
            // reasoning models emit a thinking pass first (measured ~750 tokens
            // for this review prompt), so the old 260 cap was consumed entirely
            // by thinking and the response came back empty - reported to the user
            // as "Could not generate an analysis". 1200 was validated against the
            // local qwen3-vl:4b: thinking (~750) + the four-section answer (~250)
            // finishes with done=stop. Generation runs at roughly 50 tok/s
            // locally, so worst case is well inside the 300s inference timeout.
            var result = await _llm.GenerateAdvisoryAsync(
                prompt,
                maxTokens: 1200,
                base64Images: snapshots);

            return CleanAdvice(result);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Normalises the advisory response: drops LLM error strings (never present
    /// an error as advice) and strips the markdown decoration models add even
    /// when told not to.
    /// </summary>
    private static string? CleanAdvice(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        if (input.StartsWith("LLM Error", StringComparison.OrdinalIgnoreCase)) return null;
        if (input.StartsWith("No response generated", StringComparison.OrdinalIgnoreCase)) return null;

        var lines = input.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var cleaned = new List<string>(lines.Length);

        foreach (var raw in lines)
        {
            var line = raw.TrimEnd();

            // Models still emit "**HEADING**" or "## Heading" despite the
            // instructions; normalise so the UI can style the headings.
            line = line.Replace("**", "").Trim();
            if (line.StartsWith("###")) line = line.TrimStart('#').Trim();
            if (line.StartsWith("##")) line = line.TrimStart('#').Trim();
            if (line.StartsWith("#")) line = line.TrimStart('#').Trim();

            cleaned.Add(line);
        }

        var text = string.Join("\n", cleaned);

        // Collapse runs of blank lines and trim the ends.
        while (text.Contains("\n\n\n")) text = text.Replace("\n\n\n", "\n\n");
        text = text.Trim();

        return text.Length == 0 ? null : text;
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    /// <summary>
    /// Turns the model's plan into the finished image.
    ///
    /// Preferred path: an image model renders the frames using the model's
    /// editPrompt. Fallback path: no image model, or it failed, so the lead's
    /// real frames are composited instead. The fallback is what ships today and
    /// it is always honest - every pixel is genuinely from their video - whereas
    /// a generated image is only truthful when the model is actually running.
    ///
    /// Small models wrap JSON in prose or a code fence even when told not to, so
    /// values are read with regexes rather than a strict parse, and anything
    /// unusable falls back to the first frame rather than failing the send.
    /// </summary>
    private static async Task<SnapshotAiImage?> BuildImageAsync(
        string response,
        IReadOnlyList<string> allFrames,
        IReadOnlyList<string> sentFrames,
        IImageGenerationService imageGeneration)
    {
        // Clean once up front: both the fallback picks and the composer need
        // pure base64, and dirty frames are the silent killer (SkiaSharp decode
        // fails -> composer returns null -> token renders nothing).
        var cleanSent = CleanFrames(sentFrames);
        var cleanAll = CleanFrames(allFrames);

        // Empty/error/refusal answers skip straight to the first-frame fallback
        // instead of attempting a parse that cannot succeed.
        if (string.IsNullOrWhiteSpace(response)
            || response.StartsWith("LLM Error", StringComparison.OrdinalIgnoreCase)
            || LooksLikeRefusal(response))
            return ComposeFallback(cleanSent.Count > 0 ? cleanSent : cleanAll);

        var json = ExtractFirstJsonObject(response);
        var width = Math.Clamp(ReadInt(json ?? string.Empty, "width", DefaultImageWidth), MinImageWidth, MaxImageWidth);
        var height = Math.Clamp(ReadInt(json ?? string.Empty, "height", DefaultImageHeight), MinImageHeight, MaxImageHeight);
        var editPrompt = ReadString(json ?? string.Empty, "editPrompt");

        var chosen = new List<string>();

        if (json is not null)
        {
            foreach (var index in ReadIndexes(json))
            {
                // A position the model invented must not be honoured, and the
                // set is capped so a greedy model cannot produce a huge strip.
                // Index into the CLEANED list: positions refer to what the
                // model saw (SelectImageSample output), and CleanFrames
                // preserves order while only dropping blanks, so positions
                // stay aligned unless blanks were dropped - hence the clamp.
                if (index < 1 || index > cleanSent.Count)
                    continue;
                if (chosen.Contains(cleanSent[index - 1]))
                    continue;

                chosen.Add(cleanSent[index - 1]);
                if (chosen.Count >= SnapshotImageComposer.MaxFrames)
                    break;
            }
        }

        // No usable answer at all: the first sampled frame is always valid, so
        // the email still shows the creator's footage rather than nothing.
        if (chosen.Count == 0)
            chosen.Add(cleanSent.Count > 0 ? cleanSent[0] : cleanAll.FirstOrDefault() ?? string.Empty);

        if (imageGeneration.IsConfigured && !string.IsNullOrWhiteSpace(editPrompt))
        {
            try
            {
                var generated = await imageGeneration.GenerateAsync(chosen, editPrompt, width, height);
                if (!string.IsNullOrWhiteSpace(generated))
                    return new SnapshotAiImage(generated, width, height, isGenerated: true);
            }
            catch
            {
                // A failed generation must not cost the recipient their image;
                // fall through to composing the real frames.
            }
        }

        var composed = SnapshotImageComposer.ComposeBase64(chosen);
        if (composed is null)
            return ComposeFallback(cleanSent.Count > 0 ? cleanSent : cleanAll);

        return new SnapshotAiImage(composed, width, height, isGenerated: false);
    }

    /// <summary>
    /// A string value from the model's JSON, unescaped and trimmed. Small models
    /// wrap generated text in quotes and escape inner quotes, so a plain split
    /// is not enough.
    /// </summary>
    private static string ReadString(string json, string key)
    {
        var match = Regex.Match(
            json, $"\"{key}\"\\s*:\\s*\"(?<value>(?:[^\"\\\\]|\\\\.)*)\"", RegexOptions.IgnoreCase);

        if (!match.Success)
            return string.Empty;

        var value = match.Groups["value"].Value
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\")
            .Replace("\\n", " ")
            .Replace("\\r", " ");

        return value.Trim();
    }

    /// <summary>
    /// The frame numbers in the "indexes" array, in the order the model listed
    /// them. Read positionally rather than via a JSON parser because a small
    /// model may emit single quotes, a trailing comma, or the values unquoted.
    /// </summary>
    private static List<int> ReadIndexes(string json)
    {
        var indexes = new List<int>();

        // Accepts both key spellings: "selectedSnapshots" is what the prompt
        // asks for, "indexes" is what the model may echo back instead.
        // Quotation marks are optional because a small model may emit
        // "indexes", 'indexes', or indexes.
        var array = Regex.Match(
            json, "[\"']?(?:selectedSnapshots|indexes)[\"']?\\s*:\\s*\\[(?<items>[^\\]]*)\\]", RegexOptions.IgnoreCase);
        if (!array.Success)
            return indexes;

        foreach (Match item in Regex.Matches(array.Groups["items"].Value, @"\d+"))
        {
            if (int.TryParse(item.Value, out var value))
                indexes.Add(value);
        }

        return indexes;
    }

    /// <summary>
    /// The first balanced {...} block in the response, so a model that prefixes
    /// its answer with "Sure, here you go:" is still understood.
    /// </summary>
    private static string? ExtractFirstJsonObject(string text)
    {
        var start = text.IndexOf('{');
        if (start < 0) return null;

        var depth = 0;
        for (var i = start; i < text.Length; i++)
        {
            if (text[i] == '{') depth++;
            else if (text[i] == '}')
            {
                depth--;
                if (depth == 0)
                    return text[start..(i + 1)];
            }
        }

        return null;
    }

    private static int ReadInt(string json, string key, int fallback)
    {
        var match = Regex.Match(
            json, $"\"{key}\"\\s*:\\s*(\\d+)", RegexOptions.IgnoreCase);

        return match.Success && int.TryParse(match.Groups[1].Value, out var value)
            ? value
            : fallback;
    }

    /// <summary>
    /// True when the vision answer reads like a refusal or blind reply rather
    /// than a frame pick ("I cannot see/view the images", "as an AI", "unable
    /// to see", "no image(s) provided"). Small/quantized vision models say
    /// this when the image payload did not attach or they cannot ground it -
    /// treating it as a pick would either ship no image or, worse, trust
    /// hallucinated positions.
    /// </summary>
    private static bool LooksLikeRefusal(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return true;

        var text = response.ToLowerInvariant();
        return text.Contains("cannot see")
            || text.Contains("can't see")
            || text.Contains("can not see")
            || text.Contains("unable to see")
            || text.Contains("unable to view")
            || text.Contains("cannot view")
            || text.Contains("can't view")
            || text.Contains("no image")
            || text.Contains("no images")
            || text.Contains("images were not")
            || text.Contains("image was not")
            || text.Contains("as an ai")
            || text.Contains("as a text");
    }

    /// <summary>
    /// Reduces a token instruction to a single short line. Tokens are written on
    /// one line in the composer, but a pasted one can carry newlines, which would
    /// otherwise break up the prompt the vision model reads.
    /// </summary>
    private static string TrimInstruction(string instruction)
    {
        var flat = instruction.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Trim();
        return Truncate(flat, 300);
    }

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