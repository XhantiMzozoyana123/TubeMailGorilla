using AVFoundation;
using CoreGraphics;
using CoreMedia;
using Foundation;

namespace TubeMailGorilla.Maui.Unlocked.Services;

/// <summary>
/// Decodes video frames on Mac Catalyst using AVFoundation
/// (<see cref="AVAssetImageGenerator"/>), which ships with the OS - so again no
/// ffmpeg install and no bundled binary.
/// </summary>
public sealed class AvFoundationFrameExtractor : IVideoFrameExtractor
{
    private AVAsset? _asset;
    private AVAssetImageGenerator? _generator;

    public TimeSpan Duration { get; private set; }

    public Task<bool> OpenAsync(string videoPath, CancellationToken cancellationToken)
    {
        Dispose();

        try
        {
            if (!File.Exists(videoPath)) return Task.FromResult(false);

            var url = NSUrl.FromFilename(videoPath);
            var asset = AVAsset.FromUrl(url);

            // A zero duration means the container is not something AVFoundation
            // can read, so treat it as an unsupported file.
            var seconds = asset?.Duration.Seconds ?? 0;
            if (asset is null || seconds <= 0) return Task.FromResult(false);

            _asset = asset;
            Duration = TimeSpan.FromSeconds(seconds);
            _generator = new AVAssetImageGenerator(asset)
            {
                // Honour the video's rotation metadata so portrait phone footage
                // does not come out on its side.
                AppliesPreferredTrackTransform = true
            };

            return Task.FromResult(true);
        }
        catch
        {
            Dispose();
            return Task.FromResult(false);
        }
    }

    public Task<byte[]?> ExtractFrameAsync(
        double seconds,
        int width,
        int height,
        CancellationToken cancellationToken)
    {
        if (_generator is null) return Task.FromResult<byte[]?>(null);

        // Clamp inside the clip: frame 0 is usually black and seeking past the
        // end yields no image at all.
        var target = Math.Clamp(seconds, 0.1, Math.Max(0.1, Duration.TotalSeconds - 0.1));

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            _generator.MaximumSize = new CGSize(width, height);

            var time = CMTime.FromSeconds(target, preferredTimeScale: 600);
            using var cgImage = _generator.CopyCGImageAtTime(time, out var error);

            if (cgImage is null || error != null) return Task.FromResult<byte[]?>(null);

            using var bitmap = new NSBitmapImageRep(cgImage);
            using var jpeg = bitmap.RepresentationUsingType(NSBitmapImageFileType.Jpeg, compressionFactor: 0.7f);

            var bytes = jpeg?.ToArray();
            return Task.FromResult(bytes is { Length: > 0 } ? bytes : null);
        }
        catch
        {
            return Task.FromResult<byte[]?>(null);
        }
    }

    public void Dispose()
    {
        _generator?.Dispose();
        _generator = null;
        _asset?.Dispose();
        _asset = null;
    }
}
