namespace TubeMailGorilla.Maui.Unlocked.Services;

/// <summary>
/// Turns selected video frames into one finished image for the
/// <c>[snapshot_ai]</c> email token.
///
/// This is the seam for image generation. AIService decides WHAT to show (which
/// frames, what edit to describe); this service decides HOW to render it, and
/// the implementation may be a hosted diffusion model, a local one, or nothing
/// at all. Keeping it separate is what lets the app run today with no image
/// model configured, and gain one later without AIService changing.
///
/// Implementations must never throw for an ordinary failure (model down,
/// refused, timed out) - return null so the caller can fall back.
/// </summary>
public interface IImageGenerationService
{
    /// <summary>
    /// True when an actual image model is configured. When false, AIService
    /// skips generation entirely and composes the real frames instead, so the
    /// feature degrades to something honest rather than to nothing.
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Renders one image from the given reference frames.
    ///
    /// <paramref name="referenceImages"/> are base64 JPEG frames from the lead's
    /// own video (no data-URI prefix), already limited to
    /// <see cref="SnapshotImageComposer.MaxFrames"/>. The first is the primary
    /// subject; the rest are supporting references.
    ///
    /// <paramref name="editPrompt"/> is the natural-language edit the LLM wrote,
    /// e.g. "a before-and-after comparison of this clip". Implementations that
    /// cannot honour multi-reference editing should use the first image only.
    ///
    /// Returns the rendered image as base64, or null if it could not be
    /// produced.
    /// </summary>
    Task<string?> GenerateAsync(
        IReadOnlyList<string> referenceImages,
        string editPrompt,
        int width,
        int height,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The stand-in used when no image model is configured.
///
/// It reports itself unconfigured and never generates, which is what makes the
/// rest of the [snapshot_ai] pipeline testable and shippable today: AIService
/// falls back to compositing the real frames. This is the same "optional
/// capability" pattern LlmSettings.OllamaVisionModel already uses.
/// </summary>
public sealed class NullImageGenerationService : IImageGenerationService
{
    public bool IsConfigured => false;

    public Task<string?> GenerateAsync(
        IReadOnlyList<string> referenceImages,
        string editPrompt,
        int width,
        int height,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(null);
}
