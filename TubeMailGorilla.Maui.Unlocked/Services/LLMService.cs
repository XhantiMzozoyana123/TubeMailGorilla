using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TubeMailGorilla.Maui.Unlocked.Models;

namespace TubeMailGorilla.Maui.Unlocked.Services;

/// <summary>
/// LLM inference through a local Ollama server (HTTP, no LLamaSharp/GGUF files).
/// The app posts prompts to Ollama's /api/chat endpoint on this machine, so
/// nothing depends on the VPS and no data leaves the machine - only the base
/// URL and model NAME are configured, the weights live inside Ollama.
///
/// The public surface (GenerateTextAsync, GenerateAdvisoryAsync, EnsureReadyAsync,
/// IsReady, Status, StartModelWarmup, SelectImageSample, SupportsVision,
/// VisionModel, ModelPath) is unchanged, so AIService, ExtractPage and the
/// send loop work as-is.
///
/// Threading: requests are serialized behind one lock to keep memory use and
/// keep_alive behaviour predictable. The readiness probe is idempotent -
/// concurrent callers share one probe task rather than hammering /api/tags.
/// </summary>
public class LLMService : IDisposable
{
    // Applied to every request so the model returns ONLY the raw data asked for -
    // never a summary or description of the provided text.
    private const string SYSTEM_PROMPT =
        "You are a strict data-extraction engine. " +
        "Return ONLY the raw data value the user asks for. " +
        "Rules: " +
        "- NEVER summarize, paraphrase, quote, or list anything from the provided text. " +
        "- NEVER explain or add context around the answer. " +
        "- Your ENTIRE response must be the single data value and nothing else - typically 1 to 5 words. " +
        "- NEVER output lists, bullet points, asterisks, quotation marks, or multiple values. " +
        "- No introductions ('Here is', 'Sure', 'The raw data value'). " +
        "- If the requested data cannot be found as a clear, explicit value in the text, " +
        "output NOTHING at all - an empty response. Do NOT write BLANK, UNKNOWN, N/A, or NONE. " +
        "Do NOT substitute related content. Do NOT guess. Empty means empty.";

    /// <summary>
    /// Used for advisory tasks (editing / engagement critique). The extraction
    /// system prompt above is actively wrong for those: it bans bullets, lists
    /// and multi-line output, and forces an empty answer when the data is not a
    /// single explicit value. An advisor needs the exact opposite behaviour.
    /// </summary>
    private const string ADVISORY_SYSTEM_PROMPT =
        "You are a senior YouTube editor and retention strategist. " +
        "You give specific, concrete, actionable editing advice about a video. " +
        "Rules: " +
        "- Ground every point in the material you are given. " +
        "- Be specific and practical, never vague platitudes like 'make it better' or 'add energy'. " +
        "- Do not invent details you were not told; if something is unknown, say so briefly. " +
        "- Write plain text. No markdown headers, no bold/asterisks, no emoji. " +
        "- Keep it tight and skimmable.";

    // Deliberately NOT the DI-registered HttpClient: that one lives in the MAUI
    // container and this service is constructed directly with `new`. Infinite
    // timeout here as well - the per-call budget is enforced by a
    // CancellationTokenSource in InferAsync (InferenceTimeoutSeconds).
    private static readonly HttpClient Http = new() { Timeout = Timeout.InfiniteTimeSpan };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly LlmSettings _settings;
    private readonly SemaphoreSlim _inferenceLock = new(1, 1);

    // The readiness probe runs exactly once when it succeeds; failures are
    // retried on the next call (the server may simply not have been up yet).
    private readonly object _loadGate = new();
    private Task<bool>? _probeTask;
    private volatile string[]? _knownModels;
    private bool _disposed;
    private int _warmupStarted;

    public LLMService(LlmSettings? settings = null)
    {
        _settings = settings ?? new LlmSettings();
    }

    /// <summary>Base URL of the Ollama server, without a trailing slash.</summary>
    private string BaseUrl => (_settings.OllamaBaseUrl ?? string.Empty).Trim().TrimEnd('/');

    /// <summary>
    /// Probes the Ollama server (GET /api/tags) and reports whether the app can
    /// run its AI features. Returns false (and leaves <see cref="Status"/>
    /// explaining why) when the server is down or the configured model is not
    /// pulled, so the UI can say so instead of failing part-way through an
    /// extraction.
    /// </summary>
    public async Task<bool> EnsureReadyAsync()
    {
        try
        {
            return await EnsureLoadedAsync();
        }
        catch (Exception ex)
        {
            IsReady = false;
            Status = $"Ollama check failed: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// Probes the server and (when reachable) sends a 1-token request so the
    /// model is loaded into memory before the first real extraction pays the
    /// cold-load cost.
    /// </summary>
    public void StartModelWarmup()
    {
        if (Interlocked.Exchange(ref _warmupStarted, 1) == 1)
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                if (!await EnsureLoadedAsync())
                    return;

                // Force the model into RAM. The result text is irrelevant.
                await InferAsync(
                    "hi",
                    "You are a helpful assistant.",
                    images: null,
                    maxTokens: 1,
                    model: ActiveModel);
            }
            catch (Exception ex)
            {
                Status = $"Model warmup failed: {ex.Message}";
            }
        });
    }

    /// <summary>True once the server and model have been probed successfully.</summary>
    public bool IsReady { get; private set; }

    /// <summary>Human-readable status message surfaced to the UI.</summary>
    public string Status { get; private set; } = "LLM not initialized";

    /// <summary>The Ollama model in use, or a placeholder before it loads.</summary>
    public string ModelPath => IsReady ? $"ollama/{ActiveModel}" : "(not loaded)";

    /// <summary>
    /// The vision model NAME configured on the Ollama server, or empty when
    /// none. A vision model is OPTIONAL: without one the app is fully
    /// functional, the analysis just works from the transcript and metadata
    /// instead of the frames.
    ///
    /// llama3:latest is text-only, so with only the default model pulled this
    /// is empty and vision stays off. Set OllamaVisionModel to a pulled
    /// vision-capable model (e.g. "qwen2.5vl:3b") to enable it.
    /// </summary>
    public string VisionModel => _settings.OllamaVisionModel?.Trim() ?? string.Empty;

    /// <summary>
    /// True when a vision model is configured AND present on the Ollama server.
    /// Before the first probe it only checks the configuration, mirroring the
    /// old "file exists" semantics; the probe then tightens it.
    /// </summary>
    public bool SupportsVision
    {
        get
        {
            var model = VisionModel;
            if (model.Length == 0)
                return false;

            var known = _knownModels;
            if (known is null)
                return true; // not probed yet - assume configured means available

            return Array.Exists(known, n => ModelsEqual(n, model));
        }
    }

    /// <summary>
    /// Model used for text work (extraction, icebreakers, advisory without
    /// frames).
    /// </summary>
    private string ActiveModel
    {
        get
        {
            var configured = _settings.OllamaModel?.Trim() ?? string.Empty;
            return configured.Length > 0 ? configured : "llama3:latest";
        }
    }

    /// <summary>
    /// The exact subset of frames a request will actually carry, evenly spread
    /// across <paramref name="base64Images"/>.
    ///
    /// Public because the model picks a frame by *position*, not by data: callers
    /// that need to map the model's answer back to a real snapshot (the email
    /// [snapshot_ai] image) must index this same list, otherwise the chosen
    /// index refers to a frame that was never sent.
    /// </summary>
    public IReadOnlyList<string> SelectImageSample(IReadOnlyList<string> base64Images)
    {
        var max = Math.Max(1, _settings.MaxImagesPerRequest);

        if (base64Images.Count <= max)
            return base64Images.ToList();

        // A max of 1 would make the step calculation divide by zero, and the
        // spread is meaningless anyway - just take the first frame.
        if (max == 1)
            return new[] { base64Images[0] };

        var picked = new List<string>(max);
        var step = (double)(base64Images.Count - 1) / (max - 1);

        for (var i = 0; i < max; i++)
        {
            var index = (int)Math.Round(i * step);
            if (index < 0 || index >= base64Images.Count)
                continue;
            picked.Add(base64Images[index]);
        }

        return picked;
    }

    /// <summary>
    /// Runs a one-shot completion and returns the generated text, or an
    /// "LLM Error: ..." string describing what went wrong. AIService deliberately
    /// drops those strings rather than treating them as data.
    /// </summary>
    public async Task<string> GenerateTextAsync(
        string prompt,
        int? maxTokens = null,
        string? systemPrompt = null,
        IReadOnlyList<string>? base64Images = null)
    {
        await _inferenceLock.WaitAsync();
        try
        {
            if (!await EnsureLoadedAsync())
                return $"LLM Error: {Status}";

            // The prompt carries the (possibly long) video transcript. Cap it so
            // it can never overflow the model's context window.
            var maxChars = _settings.MaxInputCharacters;
            if (prompt.Length > maxChars)
                prompt = prompt[..maxChars];

            var images = ResolveImages(base64Images);

            // Frames go to the vision model; text-only work uses the primary one.
            var model = images is { Count: > 0 } && VisionModel.Length > 0
                ? VisionModel
                : ActiveModel;

            var text = await InferAsync(
                prompt,
                systemPrompt ?? SYSTEM_PROMPT,
                images,
                maxTokens ?? _settings.MaxTokens,
                model);

            if (string.IsNullOrEmpty(text))
                return "LLM Error: No response generated.";

            IsReady = true;
            return text;
        }
        catch (Exception ex)
        {
            IsReady = false;
            Status = $"Inference failed: {ex.Message}";
            return $"LLM Error: {ex.Message}";
        }
        finally
        {
            _inferenceLock.Release();
        }
    }

    /// <summary>
    /// Runs an advisory completion (editing / engagement critique) under the
    /// advisory persona rather than the strict extraction one, and attaches the
    /// snapshot frames when a vision model is configured.
    ///
    /// Kept separate from <see cref="GenerateTextAsync"/> so no caller has to
    /// know that advisory work needs a different system prompt - using the
    /// extraction persona here would return an empty string, because that prompt
    /// bans lists and multi-line output.
    /// </summary>
    public Task<string> GenerateAdvisoryAsync(
        string prompt,
        int maxTokens,
        IReadOnlyList<string>? base64Images = null)
        => GenerateTextAsync(prompt, maxTokens, ADVISORY_SYSTEM_PROMPT, base64Images);

    /// <summary>
    /// Checks (once, cached) that the Ollama server is up and the configured
    /// model is pulled. Failures are NOT cached, so starting the server later
    /// heals on the next call.
    /// </summary>
    private Task<bool> EnsureLoadedAsync()
    {
        lock (_loadGate)
        {
            if (_probeTask is { IsCompleted: true } done)
            {
                if (done.Result)
                    return done;

                // The probe failed (server down, model not pulled) - forget it
                // so the next call retries instead of caching the failure
                // forever. The server may simply not have been up yet.
                _probeTask = null;
            }

            return _probeTask ??= Task.Run(ProbeOllamaAsync);
        }
    }

    /// <summary>
    /// GET /api/tags lists the models the Ollama server has pulled. The result
    /// doubles as the <see cref="SupportsVision"/> availability check.
    /// </summary>
    private async Task<bool> ProbeOllamaAsync()
    {
        if (_disposed)
            return false;

        Status = "Checking Ollama server...";

        try
        {
            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(Math.Max(5, _settings.InferenceTimeoutSeconds)));

            using var response = await Http.GetAsync(
                $"{BaseUrl}/api/tags", cts.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                IsReady = false;
                Status = $"Ollama server returned {(int)response.StatusCode} on {BaseUrl}/api/tags. Is 'ollama serve' running?";
                return false;
            }

            var json = await response.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);
            var tags = JsonSerializer.Deserialize<OllamaTagsResponse>(json, JsonOptions);

            var names = tags?.Models?
                .Select(m => m.Name ?? string.Empty)
                .Where(n => n.Length > 0)
                .ToArray() ?? Array.Empty<string>();
            _knownModels = names;

            var wanted = ActiveModel;
            if (!Array.Exists(names, n => ModelsEqual(n, wanted)))
            {
                IsReady = false;
                Status =
                    $"Model '{wanted}' not found in Ollama. Pull it with: ollama pull {wanted}";
                return false;
            }

            IsReady = true;
            if (Status.StartsWith("Checking Ollama", StringComparison.Ordinal))
                Status = SupportsVision
                    ? $"Ollama ready (model {wanted}, vision {VisionModel})."
                    : $"Ollama ready (model {wanted}).";

            return true;
        }
        catch (OperationCanceledException)
        {
            IsReady = false;
            Status = $"Ollama server not reachable at {BaseUrl} (timed out).";
            return false;
        }
        catch (Exception ex)
        {
            IsReady = false;
            Status = $"Ollama server not reachable at {BaseUrl}: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// One non-streaming POST /api/chat with a fresh conversation (system +
    /// user turn only), so the previous call's context can never contaminate
    /// this answer. Ollama applies the model's own chat template. The
    /// per-request budget (InferenceTimeoutSeconds) is enforced by a
    /// CancellationTokenSource because the shared HttpClient has no timeout.
    /// </summary>
    private async Task<string> InferAsync(
        string prompt,
        string systemPrompt,
        IReadOnlyList<string>? images,
        int maxTokens,
        string model)
    {
        var request = new OllamaChatRequest
        {
            Model = model,
            Stream = false,
            KeepAliveMinutes = _settings.ModelKeepAliveMinutes,
            Options = new OllamaChatOptions
            {
                NumPredict = Math.Max(1, maxTokens),
                Temperature = _settings.Temperature
            },
            Messages =
            {
                new OllamaMessage { Role = "system", Content = systemPrompt },
                new OllamaMessage
                {
                    Role = "user",
                    Content = prompt,
                    Images = images is { Count: > 0 } ? images : null
                }
            }
        };

        var json = JsonSerializer.Serialize(request, JsonOptions);
        var timeoutSeconds = Math.Max(5, _settings.InferenceTimeoutSeconds);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await Http.PostAsync(
                $"{BaseUrl}/api/chat", content, cts.Token).ConfigureAwait(false);

            var body = await response.Content.ReadAsStringAsync(CancellationToken.None)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                // Ollama reports failures as {"error":"..."}; surface its text
                // rather than a bare status code.
                var detail = TryExtractError(body);
                throw new InvalidOperationException(
                    detail ?? $"Ollama returned {(int)response.StatusCode} on /api/chat");
            }

            var parsed = JsonSerializer.Deserialize<OllamaChatResponse>(body, JsonOptions);
            return parsed?.Message?.Content ?? string.Empty;
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException($"Inference timed out after {timeoutSeconds}s");
        }
    }

    /// <summary>
    /// Picks the frames to hand to the vision model. Sending every one is
    /// impractical: a 10 minute lead has 30 base64 JPEGs (~20KB each), which
    /// would swamp the request. A small, evenly spread sample keeps it bounded
    /// and still covers the whole video.
    /// </summary>
    private List<string>? ResolveImages(IReadOnlyList<string>? base64Images)
    {
        if (!SupportsVision || base64Images is null || base64Images.Count == 0)
            return null;

        return SelectImageSample(base64Images).ToList();
    }

    /// <summary>
    /// Ollama lists tags as "name:tag" and always reports ":latest"; the
    /// config may omit the tag. Normalizing both sides makes "llama3" and
    /// "llama3:latest" the same model while still distinguishing sizes
    /// ("llama3:8b" is NOT "llama3:latest").
    /// </summary>
    private static bool ModelsEqual(string configured, string listed)
    {
        static string Normalize(string value)
        {
            var v = value.Trim();
            return v.EndsWith(":latest", StringComparison.OrdinalIgnoreCase)
                ? v[..^":latest".Length]
                : v;
        }

        return string.Equals(
            Normalize(configured), Normalize(listed), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Pulls the "error" field out of an Ollama failure body, if any.</summary>
    private static string? TryExtractError(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return null;

        try
        {
            return JsonSerializer.Deserialize<OllamaErrorResponse>(body, JsonOptions)?.Error;
        }
        catch (JsonException)
        {
            return null; // non-JSON body - fall back to the status code
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _inferenceLock.Dispose();
    }

    // ---- Ollama wire format ---------------------------------------------------

    private sealed class OllamaChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<OllamaMessage> Messages { get; set; } = new();

        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        [JsonPropertyName("options")]
        public OllamaChatOptions? Options { get; set; }

        [JsonPropertyName("keep_alive")]
        public int KeepAliveMinutes { get; set; } = 60;
    }

    private sealed class OllamaMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public IReadOnlyList<string>? Images { get; set; }
    }

    private sealed class OllamaChatOptions
    {
        [JsonPropertyName("num_predict")]
        public int NumPredict { get; set; }

        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }
    }

    private sealed class OllamaChatResponse
    {
        [JsonPropertyName("message")]
        public OllamaMessage? Message { get; set; }
    }

    private sealed class OllamaTagsResponse
    {
        [JsonPropertyName("models")]
        public List<OllamaTag>? Models { get; set; }
    }

    private sealed class OllamaTag
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class OllamaErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }






}
