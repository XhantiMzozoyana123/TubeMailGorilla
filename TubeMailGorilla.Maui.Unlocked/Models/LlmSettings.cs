namespace TubeMailGorilla.Maui.Unlocked.Models;

/// <summary>
/// Configuration for the local Ollama LLM used for data extraction.
/// The app POSTs prompts to Ollama's /api/chat endpoint on this machine -
/// only the base URL and model NAME are configured here; the weights live
/// inside Ollama (ollama pull), nothing is downloaded by the app itself.
/// </summary>
public class LlmSettings
{
    /// <summary>
    /// Base URL of the Ollama server (no trailing slash).
    /// Defaults to the local machine's Ollama daemon.
    /// </summary>
    public string OllamaBaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>Name of the model to request on the Ollama server.</summary>
    public string OllamaModel { get; set; } = "qwen3-vl:4b";

    /// <summary>
    /// OPTIONAL vision model, used when a lead's video frames must actually be
    /// looked at - the "Identify Areas of Improvement" analysis and the
    /// <c>[snapshot_ai]</c> email token. Leave it empty and both fall back to
    /// working from the transcript and video metadata instead, which is the
    /// default state with the text-only llama3.
    ///
    /// To enable it, pull a vision model on the server and name it here:
    /// <code>ollama pull qwen3-vl:4b</code> then "qwen3-vl:4b".
    ///
    /// This workload is Qwen-only: both OllamaModel and OllamaVisionModel
    /// default to qwen3-vl:4b, so text extraction AND the video review run on
    /// the same 4B vision-language model with no second model resident.
    ///
    /// Size matters here: this host is CPU-only and RAM-constrained, so pick the
    /// smallest model that reasons well enough. Qwen3-VL comes in 2b (1.9GB),
    /// 4b (3.3GB) and 8b (6.1GB), all with image input and a 256K context. Start
    /// at 4b; move to 8b only if frame selection or the edit prompt come out weak.
    /// Qwen3-VL needs Ollama 0.12.7 or newer on the server.
    ///
    /// Note this model only SEES. It picks frames and writes an edit prompt; it
    /// cannot render an image. Rendering is <see cref="IImageGenerationService"/>.
    /// </summary>
    public string OllamaVisionModel { get; set; } = "qwen3-vl:4b";

    /// <summary>
    /// How many snapshot frames may be attached to a single vision request.
    /// Each frame is roughly 20KB of base64 and the host has no GPU, so a small
    /// evenly spread sample is the only practical number.
    /// </summary>
    public int MaxImagesPerRequest { get; set; } = 4;

    /// <summary>
    /// Maximum number of tokens the model may generate per call.
    ///
    /// This caps THINKING + answer together, not just the answer: qwen3 models
    /// emit a reasoning pass first (measured ~450 tokens for a simple
    /// extraction, ~750 for a video review) and a cap below that returns an
    /// empty response. 1500 covers the biggest extraction plus a short answer.
    /// </summary>
    public int MaxTokens { get; set; } = 1500;

    /// <summary>
    /// Sampling temperature (lower = more deterministic, better for extraction).
    ///
    /// Deliberately low. The answers wanted here are a name, a company or a job
    /// title - there is no creative variation to sample, and at 0.6 the 3B model
    /// would often loop the value instead of stopping after it.
    /// </summary>
    public float Temperature { get; set; } = 0.2f;

    /// <summary>
    /// Hard cap on the prompt length (characters) sent to the model, so a long video
    /// transcript never overflows the remote model's context window.
    /// </summary>
    public int MaxInputCharacters { get; set; } = 8000;

    /// <summary>
    /// Hard cap (seconds) on a single inference HTTP call. A stuck generation can
    /// never block an extraction indefinitely - it is cancelled and reported as an
    /// error. Sized to survive a cold model load on the VPS plus CPU-bound
    /// generation (llama3:8B has no GPU there), which routinely exceeds 120s.
    /// </summary>
    public int InferenceTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Context window (tokens) requested per Ollama call (options.num_ctx).
    ///
    /// Ollama defaults to 4096 tokens, which is TOO SMALL for the video review:
    /// four snapshot frames plus the transcript prompt measured ~4700 tokens and
    /// Ollama rejected the request with a 400 ("exceeds the available context
    /// size"), surfacing as "Could not generate an analysis." 8192 leaves
    /// headroom for 4 frames (~1.1K tokens each on qwen3-vl) plus the full
    /// MaxInputCharacters prompt. Raise this if frames or the prompt grow.
    /// </summary>
    public int OllamaContextSize { get; set; } = 8192;

    /// <summary>
    /// How long (minutes) the Ollama server keeps the model loaded after a call
    /// (sent as keep_alive). Ollama's default is 5 minutes; past that every
    /// request pays a full multi-GB model reload before generating, which is the
    /// main cause of inference timeouts. 0 = unload immediately, -1 = never unload.
    /// </summary>
    public int ModelKeepAliveMinutes { get; set; } = 60;

    // ---- Local model files are gone: inference goes through the Ollama --------
    // server (OllamaBaseUrl / OllamaModel above). The Qwen2.5-VL GGUF path is
    // intentionally not used any more.

    /// <summary>
    /// Base URL of the local ComfyUI server (no trailing slash). Used ONLY by
    /// [snapshot_ai] to repaint a frame per the {instruction}. Empty (default)
    /// means no image model: the token composites the lead's real frames
    /// instead. Set to e.g. "http://127.0.0.1:8188" once ComfyUI is running.
    /// </summary>
    public string ComfyUiBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// ComfyUI checkpoint loaded in the backend (e.g. "v1-5-pruned-emaonly.safetensors",
    /// "sdXL_base_1.0.safetensors"). Must exist in ComfyUI/models/checkpoints.
    /// Empty = use whatever checkpoint ComfyUI already has loaded.
    /// </summary>
    public string ComfyUiCheckpoint { get; set; } = string.Empty;

    /// <summary>
    /// img2img denoise strength 0..1. Low (~0.35-0.5) keeps the creator's frame
    /// recognizable with a light re-grade; high (~0.65-0.8) repaints harder per
    /// the instruction but drifts from the footage. 0.55 is the middle ground.
    /// </summary>
    public float ComfyUiDenoise { get; set; } = 0.55f;

    /// <summary>
    /// Sampling steps for the ComfyUI KSampler. 20-30 is the sweet spot for
    /// SD1.5/SDXL img2img at email sizes; higher = slower, marginally cleaner.
    /// </summary>
    public int ComfyUiSteps { get; set; } = 25;

    /// <summary>
    /// Hard cap (seconds) on one ComfyUI queue run: prompt submit + websocket
    /// wait + image fetch. A stuck queue entry must never stall a campaign -
    /// it is cancelled and the pipeline falls back to real frames.
    /// </summary>
    public int ComfyUiTimeoutSeconds { get; set; } = 300;
}