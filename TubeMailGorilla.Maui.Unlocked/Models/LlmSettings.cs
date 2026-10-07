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
    /// Size matters here: this host is CPU-only and RAM-constrained, so pick the
    /// smallest model that reasons well enough. Qwen3-VL comes in 2b (1.9GB),
    /// 4b (3.3GB) and 8b (6.1GB), all with image input and a 256K context. Start
    /// at 4b; move to 8b only if frame selection or the edit prompt come out weak.
    /// Qwen3-VL needs Ollama 0.12.7 or newer on the server.
    ///
    /// Note this model only SEES. It picks frames and writes an edit prompt; it
    /// cannot render an image. Rendering is <see cref="IImageGenerationService"/>.
    /// </summary>
    public string OllamaVisionModel { get; set; } = "";

    /// <summary>
    /// How many snapshot frames may be attached to a single vision request.
    /// Each frame is roughly 20KB of base64 and the host has no GPU, so a small
    /// evenly spread sample is the only practical number.
    /// </summary>
    public int MaxImagesPerRequest { get; set; } = 4;

    /// <summary>Maximum number of tokens the model may generate per call.</summary>
    public int MaxTokens { get; set; } = 512;

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
    /// How long (minutes) the Ollama server keeps the model loaded after a call
    /// (sent as keep_alive). Ollama's default is 5 minutes; past that every
    /// request pays a full multi-GB model reload before generating, which is the
    /// main cause of inference timeouts. 0 = unload immediately, -1 = never unload.
    /// </summary>
    public int ModelKeepAliveMinutes { get; set; } = 60;

    // ---- Local model files are gone: inference goes through the Ollama --------
    // server (OllamaBaseUrl / OllamaModel above). The Qwen2.5-VL GGUF path is
    // intentionally not used any more.
}