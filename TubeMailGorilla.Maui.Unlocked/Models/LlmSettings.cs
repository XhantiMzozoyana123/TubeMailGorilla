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
    /// error.
    /// </summary>
    public int InferenceTimeoutSeconds { get; set; } = 120;

    // ---- Legacy on-device (LLamaSharp) settings, kept so old appsettings.json
    // files still bind without errors. They are ignored by the Ollama LLMService. ----
    public string ModelUrl { get; set; } =
        "https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q4_K_M.gguf";
    public string ModelFileName { get; set; } = "Llama-3.2-3B-Instruct-Q4_K_M.gguf";
    public uint ContextSize { get; set; } = 4096;
    public int GpuLayerCount { get; set; } = 0;
    public int ModelLoadTimeoutSeconds { get; set; } = 300;
}