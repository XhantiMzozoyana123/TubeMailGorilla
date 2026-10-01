using System.Text;
using LLama;
using LLama.Common;
using LLama.Native;
using LLama.Sampling;
using TubeMailGorilla.Maui.Unlocked.Models;

namespace TubeMailGorilla.Maui.Unlocked.Services;

/// <summary>
/// Local LLM inference through LLamaSharp (llama.cpp bindings). There is no
/// remote server: the model is a GGUF file on this machine and runs on the host's
/// own GPU, so nothing here depends on the VPS being reachable.
///
/// The public surface (GenerateTextAsync, GenerateAdvisoryAsync, EnsureReadyAsync,
/// IsReady, Status, StartModelWarmup, SelectImageSample) is unchanged, so
/// AIService, ExtractPage and the send loop work as-is.
///
/// Threading: a llama.cpp context is not thread-safe, so every inference is
/// serialized behind one lock. Loading is idempotent - concurrent callers share
/// the single load task rather than each opening the model again.
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

    private readonly LlmSettings _settings;
    private readonly SemaphoreSlim _inferenceLock = new(1, 1);

    // Loading is guarded by a plain lock plus a cached Task, so a burst of
    // callers at startup all await ONE load instead of each opening the GGUF.
    private readonly object _loadGate = new();
    private Task<bool>? _loadTask;
    private LLamaWeights? _weights;
    private LLamaContext? _context;
    private StatefulExecutorBase? _executor;
    private bool _disposed;
    private int _warmupStarted;

    public LLMService(LlmSettings? settings = null)
    {
        _settings = settings ?? new LlmSettings();
    }

    /// <summary>
    /// Loads the chat model if it is not loaded yet, and reports whether the app
    /// can run its AI features. Returns false (and leaves <see cref="Status"/>
    /// explaining why) when no model file is present, so the UI can say so
    /// instead of failing part-way through an extraction.
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
            Status = $"Model load failed: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// Loads the model on a background thread at startup so the first
    /// extraction does not pay the load cost.
    /// </summary>
    public void StartModelWarmup()
    {
        if (Interlocked.Exchange(ref _warmupStarted, 1) == 1)
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                await EnsureLoadedAsync();
            }
            catch (Exception ex)
            {
                Status = $"Model warmup failed: {ex.Message}";
            }
        });
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

    /// <summary>True once the model has loaded and can generate.</summary>
    public bool IsReady { get; private set; }

    /// <summary>Human-readable status message surfaced to the UI.</summary>
    public string Status { get; private set; } = "LLM not initialized";

    /// <summary>The chat model file in use, or a placeholder before it loads.</summary>
    public string ModelPath => IsReady ? ChatModelFile : "(not loaded)";

    /// <summary>
    /// The vision model file, or empty when none is configured. A vision model
    /// is OPTIONAL: without one the app is fully functional, the analysis just
    /// works from the transcript and metadata instead of the frames.
    ///
    /// This is a SEPARATE file from the chat model. llama.cpp pairs a vision
    /// projector with a language model, so a text-only GGUF cannot see images no
    /// matter how it is loaded.
    /// </summary>
    public string VisionModel => _settings.VisionModelPath?.Trim() ?? string.Empty;

    /// <summary>
    /// True when a vision model file is configured AND it exists on disk.
    /// Checking existence here means a typo'd path degrades to "no vision"
    /// rather than failing every request.
    /// </summary>
    public bool SupportsVision
    {
        get
        {
            var path = VisionModel;
            return path.Length > 0 && File.Exists(path);
        }
    }

    /// <summary>
    /// Where the chat GGUF lives: an explicit setting, or the default file name
    /// inside the models folder.
    /// </summary>
    public string ChatModelFile
    {
        get
        {
            var configured = _settings.ChatModelPath?.Trim() ?? string.Empty;
            if (configured.Length > 0)
                return configured;

            return Path.Combine(ModelDirectory, _settings.ModelFileName);
        }
    }

    /// <summary>
    /// Folder holding model files. Defaults under the app's data directory so a
    /// normal install needs no configuration.
    /// </summary>
    public string ModelDirectory
    {
        get
        {
            var configured = _settings.ModelDirectory?.Trim() ?? string.Empty;
            if (configured.Length > 0)
                return configured;

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TubeMailGorillaUnlocked", "models");
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
    ///
    /// llama.cpp is synchronous, so the work is pushed to a background thread to
    /// keep the UI responsive.
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

            var text = await Infer(
                prompt,
                systemPrompt ?? SYSTEM_PROMPT,
                images,
                maxTokens ?? _settings.MaxTokens);

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
    /// Loads the chat model (and the vision model when configured) exactly once.
    /// Concurrent callers await the same task, so a burst at startup cannot open
    /// the same GGUF several times over.
    /// </summary>
    private Task<bool> EnsureLoadedAsync()
    {
        lock (_loadGate)
        {
            if (_executor is not null && _context is not null)
                return Task.FromResult(true);

            return _loadTask ??= Task.Run(LoadModels);
        }
    }


    /// <summary>
    /// Reads the GGUF files and builds a fresh executor. Runs on a background
    /// thread; a llama.cpp load is synchronous and takes seconds, even from a
    /// warm page cache.
    /// </summary>
    private bool LoadModels()
    {
        try
        {
            if (_disposed)
                return false;

            Status = "Loading model...";

            var modelFile = ChatModelFile;
            if (!File.Exists(modelFile))
            {
                IsReady = false;
                Status = $"No model found at {modelFile}. Place a .gguf there or set ChatModelPath.";
                return false;
            }

            var contextSize = (uint)Math.Max(512u, _settings.ContextSize);

            // ModelParams is also the context parameter set in this version, so
            // one object configures both the load and the KV cache size.
            var modelParams = new ModelParams(modelFile)
            {
                ContextSize = contextSize,
                // -1 offloads every layer the GPU can hold, which is what you want
                // on a card with enough VRAM. Lower it if the model does not fit.
                GpuLayerCount = _settings.GpuLayerCount
            };

            var weights = LLamaWeights.LoadFromFile(modelParams);
            var context = weights.CreateContext(modelParams);

            // The vision projector is optional. When it loads, the executor
            // becomes multimodal and can encode images via LoadMedia.
            MtmdWeights? clip = null;
            var visionFile = VisionModel;
            if (visionFile.Length > 0 && File.Exists(visionFile))
            {
                try
                {
                    clip = MtmdWeights.LoadFromFile(
                        visionFile, weights, new MtmdContextParams());
                }
                catch (Exception ex)
                {
                    // A broken projector must not stop text generation.
                    Status = $"Vision model not loaded ({ex.Message}); continuing without images.";
                }
            }

            var executor = clip is not null
                ? new InteractiveExecutor(context, clip, null)
                : new InteractiveExecutor(context, null);

            lock (_loadGate)
            {
                // The executor does not own a disposable of its own - the
                // context and weights below are what must be freed.
                _context?.Dispose();
                _weights?.Dispose();

                _weights = weights;
                _context = context;
                _executor = executor;
            }

            IsReady = true;
            if (Status.StartsWith("Loading model", StringComparison.Ordinal))
                Status = clip is not null ? "Model ready (vision enabled)." : "Model ready.";

            return true;
        }
        catch (Exception ex)
        {
            IsReady = false;
            Status = $"Model load failed: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// One inference pass over a fresh session, so the previous call's KV cache
    /// can never contaminate this answer. The model's own chat template is
    /// applied to the system and user turns, so formatting matches what the
    /// GGUF was trained for.
    /// </summary>
    private async Task<string> Infer(
        string prompt,
        string systemPrompt,
        IReadOnlyList<string>? base64Images,
        int maxTokens)
    {
        var executor = _executor
            ?? throw new InvalidOperationException("Model is not loaded.");

        if (executor is not InteractiveExecutor interactive)
            throw new InvalidOperationException("Unexpected executor type.");

        // Images are encoded by the vision projector and attached to the
        // executor BEFORE the prompt runs, so the model actually sees the frames.
        // LoadMedia returns the embed; the executor's Embeds list is what the
        // inference loop reads.
        if (base64Images is { Count: > 0 } && interactive.ClipModel is { } projector)
        {
            interactive.Embeds.Clear();
            foreach (var b64 in base64Images)
            {
                try
                {
                    interactive.Embeds.Add(projector.LoadMedia(Convert.FromBase64String(b64)));
                }
                catch
                {
                    // One undecodable frame must not sink the whole request.
                }
            }
        }

        try
        {
            // A fresh session per call: reusing one would carry the previous
            // prompt's KV cache and contaminate the next answer. The model's own
            // chat template is applied, so the system and user turns are
            // formatted the way the GGUF was trained for.
            var session = new ChatSession(executor);
            session.AddSystemMessage(systemPrompt);
            session.AddUserMessage(prompt);

            var inference = new InferenceParams
            {
                MaxTokens = Math.Max(1, maxTokens),
                AntiPrompts = AntiPrompts,
                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = _settings.Temperature
                }
            };

            var text = new StringBuilder();

            // ChatAsync applies the model's own chat template to the session
            // history and streams the reply back as plain text fragments.
            var reply = new ChatHistory.Message(AuthorRole.Assistant, string.Empty);

            await foreach (var fragment in session.ChatAsync(reply, inference).ConfigureAwait(false))
                text.Append(fragment);

            return text.ToString();
        }
        finally
        {
            // Release the projector between calls, or each request would keep
            // every frame it ever saw resident in VRAM.
            interactive.Embeds.Clear();
            interactive.ClipModel?.ClearMedia();
        }
    }

    /// <summary>
    /// Stops generation at the end of a turn. Instruction-tuned GGUFs emit one of
    /// these after their answer; without them a short JSON reply runs on into
    /// invented extra turns.
    /// </summary>
    private static readonly string[] AntiPrompts = { "<|eot_id|>", "<|end_of_text|>", "<|im_end|>" };

    /// <summary>
    /// Picks the frames to hand to a vision model. Sending every one is
    /// impractical: a 10 minute lead has 30 base64 JPEGs (~20KB each), which is
    /// a large encode and would swamp the projector. A small, evenly spread
    /// sample keeps the request bounded and still covers the whole video.
    /// </summary>
    private List<string>? ResolveImages(IReadOnlyList<string>? base64Images)
    {
        if (!SupportsVision || base64Images is null || base64Images.Count == 0)
            return null;

        // Only a multimodal executor can take images. Without a projector
        // loaded, the request stays text-only rather than failing.
        if (_executor is not InteractiveExecutor { IsMultiModal: true })
            return null;

        return SelectImageSample(base64Images).ToList();
    }

    public void Dispose()
    {
        _disposed = true;
        _executor = null;
        _context?.Dispose();
        _weights?.Dispose();
        _inferenceLock.Dispose();
    }
}
