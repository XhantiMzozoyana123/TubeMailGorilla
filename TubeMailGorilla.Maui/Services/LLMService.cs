using System.Text;
using LLama;
using LLama.Common;
using LLama.Sampling;
using TubeMailGorilla.Maui.Models;

namespace TubeMailGorilla.Maui.Services;

/// <summary>
/// Local, fully offline LLM inference backed by LLamaSharp (llama.cpp for .NET).
/// The Llama 3 GGUF model is shipped with the published app and loaded from a folder
/// next to the executable; if it is not bundled (e.g. a fresh development clone) it is
/// downloaded automatically on first use and cached in the machine's local application
/// data folder. Weights are loaded once and reused for all calls. Inference is serialized
/// internally (the underlying LLamaContext is not thread-safe), which is safe because
/// AIService.ExtractAllAsync fans out concurrent calls.
/// </summary>
public class LLMService
{
    private const int DOWNLOAD_BUFFER_SIZE = 81920;

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
    private readonly SemaphoreSlim _inferenceLock = new(1, 1);
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private readonly SemaphoreSlim _downloadLock = new(1, 1);

    private LLamaWeights? _model;
    private StatelessExecutor? _executor;
    private int _warmupStarted;

    public LLMService(LlmSettings? settings = null)
    {
        _settings = settings ?? new LlmSettings();
    }

    /// <summary>True once the model weights have been loaded into memory.</summary>
    public bool IsReady { get; private set; }

    /// <summary>True while the model file is being downloaded on first run.</summary>
    public bool IsDownloading { get; private set; }

    /// <summary>Download progress as a percentage (0 - 100).</summary>
    public double DownloadProgress { get; private set; }

    /// <summary>Human-readable status message surfaced to the UI.</summary>
    public string Status { get; private set; } = "LLM not initialized";

    /// <summary>Absolute path the model file is stored at (bundled, or app-data if absent).</summary>
    public string ModelPath => ResolveModelPath();

    /// <summary>
    /// Begins the optional model download in the background so that by the time the user
    /// starts an extraction the GGUF file is usually already on disk. The weights are only
    /// loaded lazily on the first inference to avoid a startup memory spike.
    /// </summary>
    public void StartModelWarmup()
    {
        if (Interlocked.Exchange(ref _warmupStarted, 1) == 1)
            return;

        _ = Task.Run(EnsureModelAsync);
    }

    /// <summary>
    /// Runs a one-shot, stateless completion for the given prompt against the local model
    /// and returns the generated text. Returns an "LLM Error: ..." string on failure so
    /// callers / the UI can surface it (AIService deliberately drops those).
    /// </summary>
    public async Task<string> GenerateTextAsync(string prompt)
    {
        await EnsureModelAsync();

        await _inferenceLock.WaitAsync();
        try
        {
            var executor = await GetOrCreateExecutorAsync();
            if (executor is null)
                return "LLM Error: Model failed to load. See status for details.";

            // The prompt carries the (possibly long) video transcript. Cap it so it can
            // never overflow the fixed context window.
            var maxChars = _settings.MaxInputCharacters;
            if (prompt.Length > maxChars)
                prompt = prompt[..maxChars];

            var inferenceParams = new InferenceParams
            {
                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = _settings.Temperature
                },
                MaxTokens = _settings.MaxTokens,
                // Stop at the Llama 3 end-of-turn token so we never echo trailing markers.
                AntiPrompts = new List<string> { "<|eot_id|>" }
            };

            var builder = new StringBuilder();
            await foreach (var token in executor.InferAsync(prompt, inferenceParams))
            {
                builder.Append(token);
            }
            return builder.ToString().Trim();
        }
        catch (Exception ex)
        {
            return $"LLM Error: {ex.Message}";
        }
        finally
        {
            _inferenceLock.Release();
        }
    }

    private static string GetModelDirectory()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrEmpty(root))
            root = Path.GetTempPath();
        return Path.Combine(root, "TubeMailGorilla", "Models");
    }

    /// <summary>
    /// Path of the GGUF shipped alongside the app binary (production publish). In the
    /// self-contained Windows/macOS build the model sits next to the executable/bundle;
    /// depending on the build layout it may be directly at the output root or under a
    /// Resources\Models subfolder, so both are checked.
    /// </summary>
    private string GetBundledModelPath()
    {
        var candidates = new[]
        {
            AppContext.BaseDirectory,
            Path.Combine(AppContext.BaseDirectory, "Resources", "Models")
        };
        foreach (var dir in candidates)
        {
            var candidate = Path.Combine(dir, _settings.ModelFileName);
            if (File.Exists(candidate))
                return candidate;
        }
        return Path.Combine(AppContext.BaseDirectory, _settings.ModelFileName);
    }

    /// <summary>
    /// Resolves the model file to load: the bundled copy when it exists (production),
    /// otherwise the app-data cached copy (dev / manual download).
    /// </summary>
    private string ResolveModelPath()
    {
        var bundled = GetBundledModelPath();
        if (File.Exists(bundled))
            return bundled;
        return Path.Combine(GetModelDirectory(), _settings.ModelFileName);
    }

    /// <summary>
    /// Makes sure a model file is available locally. Prefers a bundled GGUF shipped with
    /// the app; otherwise downloads it once into the app-data folder (first-run fallback,
    /// used when the model is not bundled, e.g. a fresh development clone).
    /// </summary>
    private async Task<string> EnsureModelAsync()
    {
        var bundled = GetBundledModelPath();
        if (File.Exists(bundled))
            return bundled;

        var directory = GetModelDirectory();
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, _settings.ModelFileName);

        if (File.Exists(path))
            return path;

        await _downloadLock.WaitAsync();
        try
        {
            if (File.Exists(path))
                return path;
            await DownloadModelAsync(path);
        }
        finally
        {
            _downloadLock.Release();
        }

        return path;
    }

    private async Task DownloadModelAsync(string destination)
    {
        var partialPath = destination + ".part";
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(60) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("TubeMailGorilla.Maui/1.0");

        IsDownloading = true;
        Status = $"Downloading {_settings.ModelFileName}...";
        DownloadProgress = 0;

        try
        {
            using var response = await client.GetAsync(_settings.ModelUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var totalLength = response.Content.Headers.ContentLength;

            await using (var source = await response.Content.ReadAsStreamAsync())
            await using (var target = new FileStream(partialPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var buffer = new byte[DOWNLOAD_BUFFER_SIZE];
                long downloaded = 0;
                int read;
                while ((read = await source.ReadAsync(buffer)) > 0)
                {
                    await target.WriteAsync(buffer.AsMemory(0, read));
                    downloaded += read;
                    if (totalLength.HasValue && totalLength.Value > 0)
                    {
                        DownloadProgress = Math.Round((double)downloaded / totalLength.Value * 100, 1);
                        Status = $"Downloading {_settings.ModelFileName}: {DownloadProgress:0.#}%";
                    }
                }
                await target.FlushAsync();
            }

            if (File.Exists(destination))
                File.Delete(destination);
            File.Move(partialPath, destination);

            DownloadProgress = 100;
            Status = "Model ready.";
        }
        finally
        {
            IsDownloading = false;
        }
    }

    /// <summary>Loads the GGUF weights into memory once and caches a stateless executor.</summary>
    private async Task<StatelessExecutor?> GetOrCreateExecutorAsync()
    {
        await _loadLock.WaitAsync();
        try
        {
            if (_executor is not null)
                return _executor;

            var modelPath = ResolveModelPath();
            if (!File.Exists(modelPath))
                return null;

            Status = "Loading model...";

            var parameters = new ModelParams(modelPath)
            {
                ContextSize = _settings.ContextSize,
                GpuLayerCount = _settings.GpuLayerCount
            };

            _model?.Dispose();
            _model = await LLamaWeights.LoadFromFileAsync(parameters);
            _executor = new StatelessExecutor(_model, parameters)
            {
                ApplyTemplate = true,
                SystemMessage = SYSTEM_PROMPT
            };

            IsReady = true;
            Status = "Model ready.";
            return _executor;
        }
        catch (Exception ex)
        {
            Status = $"Failed to load model: {ex.Message}";
            return null;
        }
        finally
        {
            _loadLock.Release();
        }
    }
}