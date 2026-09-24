using System.Net.Http.Json;
using System.Text.Json.Serialization;
using TubeMailGorilla.Maui.Unlocked.Models;

namespace TubeMailGorilla.Maui.Unlocked.Services;

/// <summary>
/// Remote LLM inference backed by Ollama hosted on the TubeMailGorilla VPS
/// (POST {OllamaBaseUrl}/api/generate, non-streaming). No model is downloaded
/// or loaded locally - inference needs internet access to the Ollama server.
/// The public surface (GenerateTextAsync, EnsureReadyAsync, IsReady, Status,
/// StartModelWarmup) is unchanged so AIService and ExtractPage work as-is.
/// Concurrent calls are serialized so bursts never overload the VPS.
/// </summary>
public class LLMService
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

    private readonly LlmSettings _settings;
    private readonly HttpClient _http;
    private readonly SemaphoreSlim _inferenceLock = new(1, 1);
    private int _warmupStarted;

    public LLMService(LlmSettings? settings = null, HttpClient? http = null)
    {
        _settings = settings ?? new LlmSettings();
        _http = http ?? new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
    }

    /// <summary>
    /// Pings the Ollama server (GET /api/tags) so the UI can report connectivity
    /// upfront. Returns false (and the AI fields stay empty in AIService) when the
    /// server cannot be reached.
    /// </summary>
    public async Task<bool> EnsureReadyAsync()
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            using var response = await _http.GetAsync(
                $"{BaseUrl}/api/tags", timeout.Token);
            if (!response.IsSuccessStatusCode)
            {
                Status = $"Ollama server unreachable ({(int)response.StatusCode}).";
                IsReady = false;
                return false;
            }
            IsReady = true;
            Status = "Ollama ready.";
            return true;
        }
        catch (Exception ex)
        {
            IsReady = false;
            Status = $"Ollama server unreachable: {ex.Message}";
            return false;
        }
    }

    /// <summary>True once the Ollama server has answered a health check.</summary>
    public bool IsReady { get; private set; }

    /// <summary>Compat only - the remote server never downloads. Always false.</summary>
    public bool IsDownloading => false;

    /// <summary>Compat only - no local download exists. Always 0.</summary>
    public double DownloadProgress => 0;

    /// <summary>Human-readable status message surfaced to the UI.</summary>
    public string Status { get; private set; } = "Ollama not initialized";

    /// <summary>Describes the remote endpoint (compat for callers showing ModelPath).</summary>
    public string ModelPath => $"{BaseUrl} (model: {_settings.OllamaModel})";

    /// <summary>
    /// Pings the Ollama server in the background at startup so the first
    /// extraction already knows whether AI fields will be available.
    /// </summary>
    public void StartModelWarmup()
    {
        if (Interlocked.Exchange(ref _warmupStarted, 1) == 1)
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                if (!await EnsureReadyAsync())
                    return;

                // GET /api/tags does not load the model. Run one tiny generation so
                // the weights are resident before the first real request - a cold
                // model load alone can take longer than the inference timeout.
                await GenerateTextAsync("ping");
            }
            catch (Exception ex)
            {
                Status = $"Ollama warmup failed: {ex.Message}";
            }
        });
    }

    /// <summary>
    /// Runs a one-shot, non-streaming completion for the given prompt against the
    /// VPS Ollama server and returns the generated text. Returns an
    /// "LLM Error: ..." string on failure so callers / the UI can surface it
    /// (AIService deliberately drops those). maxTokens caps generation for short
    /// outputs (e.g. icebreakers) so they finish well inside the inference
    /// timeout; null uses the configured MaxTokens.
    /// </summary>
    public async Task<string> GenerateTextAsync(string prompt, int? maxTokens = null)
    {
        await _inferenceLock.WaitAsync();
        try
        {
            // The prompt carries the (possibly long) video transcript. Cap it so it
            // can never overflow the remote model's context window.
            var maxChars = _settings.MaxInputCharacters;
            if (prompt.Length > maxChars)
                prompt = prompt[..maxChars];

            var request = new OllamaGenerateRequest
            {
                Model = _settings.OllamaModel,
                System = SYSTEM_PROMPT,
                Prompt = prompt,
                Stream = false,
                KeepAlive = FormatKeepAlive(),
                Options = new OllamaOptions
                {
                    Temperature = _settings.Temperature,
                    NumPredict = maxTokens ?? _settings.MaxTokens
                }
            };

            // A stuck remote generation must never freeze an extraction, so a hard
            // timeout cancels the HTTP call.
            using var timeout = new CancellationTokenSource(
                TimeSpan.FromSeconds(_settings.InferenceTimeoutSeconds));
            try
            {
                using var response = await _http.PostAsJsonAsync(
                    $"{BaseUrl}/api/generate", request, timeout.Token);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await SafeReadBodyAsync(response);
                    return $"LLM Error: Ollama returned {(int)response.StatusCode} {response.ReasonPhrase}{(string.IsNullOrEmpty(body) ? "" : $" - {body}")}";
                }

                var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(timeout.Token);
                var text = result?.Response?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(text))
                    return "LLM Error: No response generated.";
                IsReady = true;
                Status = "Ollama ready.";
                return text;
            }
            catch (OperationCanceledException)
            {
                return $"LLM Error: Inference timed out after {_settings.InferenceTimeoutSeconds}s.";
            }
        }
        catch (Exception ex)
        {
            IsReady = false;
            Status = $"Ollama server unreachable: {ex.Message}";
            return $"LLM Error: {ex.Message}";
        }
        finally
        {
            _inferenceLock.Release();
        }
    }

    /// <summary>
    /// keep_alive for the Ollama request: how long the server keeps the model
    /// loaded after this call. Without it Ollama unloads after its default 5
    /// minutes, so the next icebreaker pays a full multi-GB cold load again -
    /// which alone can exceed the inference timeout on the VPS.
    /// </summary>
    private string FormatKeepAlive()
    {
        var minutes = _settings.ModelKeepAliveMinutes;
        if (minutes < 0) return "-1";   // keep until Ollama restarts
        if (minutes == 0) return "0";   // unload immediately after this call
        return $"{minutes}m";
    }

    private string BaseUrl => (_settings.OllamaBaseUrl ?? string.Empty).TrimEnd('/');

    private static async Task<string> SafeReadBodyAsync(HttpResponseMessage response)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync();
            return body.Length > 300 ? body[..300] : body;
        }
        catch
        {
            return string.Empty;
        }
    }

    private sealed class OllamaGenerateRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
        [JsonPropertyName("system")]
        public string System { get; set; } = string.Empty;
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;
        [JsonPropertyName("stream")]
        public bool Stream { get; set; }
        [JsonPropertyName("keep_alive")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? KeepAlive { get; set; }
        [JsonPropertyName("options")]
        public OllamaOptions? Options { get; set; }
    }

    private sealed class OllamaOptions
    {
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }
        [JsonPropertyName("num_predict")]
        public int NumPredict { get; set; }
    }

    private sealed class OllamaGenerateResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; set; }
    }
}