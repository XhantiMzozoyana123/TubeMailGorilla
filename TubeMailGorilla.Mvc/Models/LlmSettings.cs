namespace TubeMailGorilla.Mvc.Models;

/// <summary>
/// Configuration for the VPS-hosted Ollama LLM used for data extraction.
/// The app sends prompts to Ollama's /api/generate endpoint - no model is
/// downloaded or loaded locally, so extraction requires internet access to
/// reach the Ollama server.
/// </summary>
public class LlmSettings
{
    /// <summary>
    /// Base URL of the Ollama server (no trailing slash).
    /// Local dev default is the on-machine daemon; the VPS compose file points
    /// this at the remote Ollama host instead.
    /// </summary>
    public string OllamaBaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>Name of the model to request on the Ollama server.</summary>
    public string OllamaModel { get; set; } = "llama3:latest";

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
    /// Note this model only SEES. It picks frames and writes an edit prompt; it
    /// cannot render an image. Rendering is <see cref="Services.IImageGenerationService"/>.
    /// </summary>
    public string OllamaVisionModel { get; set; } = "qwen3-vl:4b";

    /// <summary>
    /// How many snapshot frames may be attached to a single vision request.
    /// Each frame is roughly 20KB of base64 and the host has no GPU, so a small
    /// evenly spread sample is the only practical number.
    /// </summary>
    public int MaxImagesPerRequest { get; set; } = 4;

    /// Maximum number of tokens the model may generate per call.
    ///
    /// This caps THINKING + answer together, not just the answer: qwen3 models
    /// emit a reasoning pass first and a cap below that returns an empty response.
    /// 1500 covers the biggest extraction plus a short answer.
    /// </summary>
    public int MaxTokens { get; set; } = 1500;

    /// <summary>
    /// Sampling temperature (lower = more deterministic, better for extraction).
    /// Deliberately low: extraction answers are a name or title, not creative copy.
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
    /// 8192 leaves headroom for 4 frames plus the full MaxInputCharacters prompt.
    /// </summary>
    public int OllamaContextSize { get; set; } = 8192;

    /// <summary>
    /// How long (minutes) the Ollama server keeps the model loaded after a call
    /// (sent as keep_alive). Ollama's default is 5 minutes; past that every
    /// request pays a full multi-GB model reload before generating, which is the
    /// main cause of inference timeouts. 0 = unload immediately, -1 = never unload.
    /// </summary>
    public int ModelKeepAliveMinutes { get; set; } = 60;

    // ---- Legacy on-device (LLamaSharp) settings, kept so old appsettings.json
    // files still bind without errors. They are ignored by the Ollama LLMService. ----
    public string ModelUrl { get; set; } =
        "https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q4_K_M.gguf";
    public string ModelFileName { get; set; } = "Llama-3.2-3B-Instruct-Q4_K_M.gguf";
    public uint ContextSize { get; set; } = 4096;
    public int GpuLayerCount { get; set; } = 0;
    public int ModelLoadTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Base URL of the local ComfyUI server (no trailing slash). Used ONLY by
    /// [snapshot_ai] to repaint a frame per the {instruction}. Empty (default)
    /// means no image model: the token composites the lead's real frames
    /// instead. Set to e.g. "http://127.0.0.1:8188" once ComfyUI is running.
    /// </summary>
    public string ComfyUiBaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// ComfyUI checkpoint in ComfyUI/models/checkpoints.
    /// Empty = use whatever checkpoint ComfyUI already has loaded.
    /// </summary>
    public string ComfyUiCheckpoint { get; set; } = "v1-5-pruned-emaonly.safetensors";

    /// <summary>img2img denoise strength 0..1. 0.55 keeps the frame recognizable.</summary>
    public float ComfyUiDenoise { get; set; } = 0.55f;

    /// <summary>Sampling steps for the ComfyUI KSampler. 20-30 is the sweet spot.</summary>
    public int ComfyUiSteps { get; set; } = 25;

    /// <summary>Hard cap (seconds) on one ComfyUI queue run. Falls back to real frames.</summary>
    public int ComfyUiTimeoutSeconds { get; set; } = 300;
}