using Windows.Media.Editing;
using Windows.Storage;
using Windows.Storage.Streams;

namespace TubeMailGorilla.Maui.Services;

/// <summary>
/// Decodes video frames on Windows using the built-in Media Foundation stack
/// (<see cref="MediaComposition"/>), which ships with the OS. This means the app
/// needs no ffmpeg install and no bundled binary to take video snapshots.
/// </summary>
public sealed class MediaFoundationFrameExtractor : IVideoFrameExtractor
{
    private MediaClip? _clip;
    private MediaComposition? _composition;

    public TimeSpan Duration { get; private set; }

    public async Task<bool> OpenAsync(string videoPath, CancellationToken cancellationToken)
    {
        Dispose();

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(videoPath);
            cancellationToken.ThrowIfCancellationRequested();

            var clip = await MediaClip.CreateFromFileAsync(file);
            cancellationToken.ThrowIfCancellationRequested();

            // Thumbnails are pulled from a composition, not a bare clip.
            var composition = new MediaComposition();
            composition.Clips.Add(clip);

            _clip = clip;
            _composition = composition;
            Duration = clip.OriginalDuration;

            return true;
        }
        catch
        {
            // An unreadable file or an unsupported codec is not fatal: the
            // caller simply stores a lead without snapshots.
            Dispose();
            return false;
        }
    }

    public async Task<byte[]?> ExtractFrameAsync(
        double seconds,
        int width,
        int height,
        CancellationToken cancellationToken)
    {
        if (_composition is null) return null;

        // MediaComposition throws if asked for a frame past the end, and frame 0
        // is very often black, so the seek is nudged inside both bounds.
        var target = Math.Clamp(seconds, 0.1, Math.Max(0.1, Duration.TotalSeconds - 0.1));

        IRandomAccessStreamWithContentType? thumbnail = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            thumbnail = await _composition.GetThumbnailAsync(
                TimeSpan.FromSeconds(target),
                width,
                height,
                VideoFramePrecision.NearestFrame);

            return await ReadAllBytesAsync(thumbnail, cancellationToken);
        }
        catch
        {
            return null;
        }
        finally
        {
            thumbnail?.Dispose();
        }
    }

    private static async Task<byte[]?> ReadAllBytesAsync(
        IRandomAccessStreamWithContentType stream,
        CancellationToken cancellationToken)
    {
        var size = (int)stream.Size;
        if (size <= 0) return null;

        var reader = new DataReader(stream.GetInputStreamAt(0));
        try
        {
            await reader.LoadAsync((uint)size);
            var buffer = new byte[size];
            reader.ReadBytes(buffer);
            return buffer;
        }
        finally
        {
            reader.Dispose();
        }
    }

    public void Dispose()
    {
        // MediaClip/MediaComposition are WinRT objects without IDisposable.
        // Dropping the references lets the RCW release them.
        _composition = null;
        _clip = null;
    }
}
