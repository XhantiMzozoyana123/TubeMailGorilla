namespace TubeMailGorilla.Maui.Unlocked.Services;

/// <summary>
/// Pulls still frames out of a video file that is already on disk.
///
/// Downloading a video is one problem; turning it into pictures is another, and
/// the decoder is inherently platform-specific. This interface is the seam:
/// each platform supplies its own native decoder, so the snapshot service never
/// has to know whether it is running on Windows or macOS.
/// </summary>
public interface IVideoFrameExtractor : IDisposable
{
    /// <summary>
    /// Loads a video and prepares it for seeking. Returns false if the file
    /// cannot be opened or its codec is not supported on this platform.
    /// </summary>
    Task<bool> OpenAsync(string videoPath, CancellationToken cancellationToken);

    /// <summary>Length of the loaded video. Only valid after a successful open.</summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// Decodes the frame at <paramref name="seconds"/> as JPEG bytes, or null if
    /// that position cannot be decoded (e.g. it is past the end).
    /// </summary>
    Task<byte[]?> ExtractFrameAsync(double seconds, int width, int height, CancellationToken cancellationToken);
}

/// <summary>
/// Creates the frame extractor for the platform the app is running on.
/// </summary>
public static class VideoFrameExtractorFactory
{
    public static IVideoFrameExtractor Create() =>
#if WINDOWS
        new MediaFoundationFrameExtractor();
#elif MACCATALYST
        new AvFoundationFrameExtractor();
#else
        throw new PlatformNotSupportedException(
            "Video snapshots are not supported on this platform: there is no built-in video decoder to use.");
#endif
}
