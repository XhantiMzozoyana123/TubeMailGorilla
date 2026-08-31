namespace TubeMailGorilla.Maui.Models;

/// <summary>
/// Configuration for the local LLamaSharp-powered LLM used for data extraction.
/// The model is downloaded once (on first use) from <see cref="ModelUrl"/> and cached
/// in the machine's local application data folder so the app works fully offline.
/// </summary>
public class LlmSettings
{
    /// <summary>
    /// Download location of the Llama 3 GGUF model (HuggingFace "resolve/main" URL).
    /// Defaults to Llama 3.2 3B Instruct, Q4_K_M quantization (~1.9 GB) - a Llama 3
    /// family model that runs well on CPU and follows extraction instructions reliably.
    /// </summary>
    public string ModelUrl { get; set; } =
        "https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q4_K_M.gguf";

    /// <summary>Local file name the model is stored under (must match the URL's file).</summary>
    public string ModelFileName { get; set; } = "Llama-3.2-3B-Instruct-Q4_K_M.gguf";

    /// <summary>Maximum prompt context, in tokens, used when loading the model.</summary>
    public uint ContextSize { get; set; } = 4096;

    /// <summary>Layers offloaded to GPU (0 = run purely on CPU).</summary>
    public int GpuLayerCount { get; set; } = 0;

    /// <summary>Maximum number of tokens the model may generate per call.</summary>
    public int MaxTokens { get; set; } = 512;

    /// <summary>Sampling temperature (lower = more deterministic, better for extraction).</summary>
    public float Temperature { get; set; } = 0.6f;

    /// <summary>
    /// Hard cap on the prompt length (characters) sent to the model, so a long video
    /// transcript never overflows the fixed context window.
    /// </summary>
    public int MaxInputCharacters { get; set; } = 8000;
}