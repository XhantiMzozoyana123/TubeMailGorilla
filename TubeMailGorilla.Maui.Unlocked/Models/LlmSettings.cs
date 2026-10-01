namespace TubeMailGorilla.Maui.Unlocked.Models;

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
    /// Defaults to the TubeMailGorilla VPS.
    /// </summary>
    public string OllamaBaseUrl { get; set; } = "http://46.202.170.203:11434";

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

    /// <summary>Sampling temperature (lower = more deterministic, better for extraction).</summary>
    public float Temperature { get; set; } = 0.6f;

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

    // ---- Local inference (LLamaSharp / llama.cpp) -----------------------------
    // Inference runs on this machine against a local GGUF file. There is no
    // remote server, so nothing here depends on the VPS.

    /// <summary>
    /// Absolute path to the chat GGUF used for text work. An empty value means
    /// "use the default location under the app's data folder", so a normal
    /// install needs no configuration.
    /// </summary>
    public string ChatModelPath { get; set; } = "";

    /// <summary>
    /// Absolute path to the multimodal (vision) GGUF, used by the
    /// <c>[snapshot_ai]</c> token and the video analysis. This is a SEPARATE file
    /// from the chat model - llama.cpp loads a vision projector and its language
    /// model together, so a text-only chat GGUF cannot see images.
    ///
    /// Leave empty to run without vision; the affected features then fall back
    /// to working from the transcript and metadata.
    /// </summary>
    public string VisionModelPath { get; set; } = "";

    /// <summary>
    /// Layers offloaded to the GPU. -1 means every layer the GPU can hold, which
    /// is what you want on a card with enough VRAM (e.g. an RTX A2000 Ada).
    /// Lower it if the model does not fit, or set 0 to run entirely on CPU.
    /// </summary>
    public int GpuLayerCount { get; set; } = -1;

    /// <summary>Context window in tokens.</summary>
    public uint ContextSize { get; set; } = 4096;

    /// <summary>
    /// Where downloaded models are cached, when empty. Defaults to a
    /// "models" folder beside the app's data.
    /// </summary>
    public string ModelDirectory { get; set; } = "";

    /// <summary>
    /// Absolute path to the chat GGUF this app last downloaded, kept so a
    /// first-run fetch does not have to happen again.
    /// </summary>
    public string ModelUrl { get; set; } = string.Empty;

    /// <summary>File name the model is saved under in the model directory.</summary>
    public string ModelFileName { get; set; } = "Llama-3.2-3B-Instruct-Q4_K_M.gguf";
}