using SkiaSharp;

namespace TubeMailGorilla.Maui.Unlocked.Services;

/// <summary>
/// Builds the single JPEG that the <c>[snapshot_ai]</c> email token embeds, by
/// compositing the frames the vision model selected into one image.
///
/// This deliberately does NOT generate pixels. Nothing in the stack can render
/// what an edit would look like - the VPS runs a text-only model and Ollama's
/// /api/generate returns text, never images. So the image is a montage of REAL
/// frames from the creator's own video: two of them read as a before/after
/// comparison, and a grid of them reads as "these are the clips I touched".
/// That keeps the pitch truthful, because every pixel shown is genuinely from
/// the recipient's video.
///
/// Lives apart from AIService because it is pure, deterministic image work with
/// no LLM involvement, which also makes it straightforward to verify.
/// </summary>
public static class SnapshotImageComposer
{
    /// <summary>Widest image we will produce, in pixels.</summary>
    public const int MaxWidth = 640;

    /// <summary>
    /// Most frames in one montage. Eight is what the "clip 1 to 8" example
    /// describes, and past this the image stops reading on a phone.
    /// </summary>
    public const int MaxFrames = 8;

    private const int Padding = 8;
    private const int CellWidth = 300;
    private const int CellHeight = 169;   // 16:9, matching the captured frames
    private const int JpegQuality = 82;

    /// <summary>
    /// Columns that lay the frames out best: a 2-frame comparison is a single
    /// row, while wider grids stay closer to a phone's width so the cells do not
    /// shrink to unreadable thumbnails.
    /// </summary>
    public static int ColumnsFor(int count) => count switch
    {
        <= 1 => 1,
        2 => 2,
        3 => 3,
        4 => 2,
        _ => 3
    };

    /// <summary>
    /// Composites <paramref name="frames"/> (base64 JPEG, no data-URI prefix)
    /// into one base64 JPEG. Returns null when nothing usable was supplied or
    /// every frame failed to decode - the caller then renders no image at all
    /// rather than shipping a broken one.
    /// </summary>
    public static string? ComposeBase64(IReadOnlyList<string> frames)
    {
        if (frames is null || frames.Count == 0)
            return null;

        var usable = frames
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Take(MaxFrames)
            .Select(Decode)
            .Where(image => image is not null)
            .Take(MaxFrames)
            .Select(image => image!)
            .ToList();

        if (usable.Count == 0)
            return null;

        // A single frame is passed through untouched: recompressing it would
        // only degrade the quality for no benefit.
        if (usable.Count == 1)
        {
            using var single = usable[0];
            return Encode(single);
        }

        var columns = ColumnsFor(usable.Count);
        var rows = (int)Math.Ceiling(usable.Count / (double)columns);

        // Size the cells to fit the width budget FIRST, rather than building an
        // oversized montage and scaling it down afterwards. Scaling afterwards
        // would shrink the padding with it and round the cells off 16:9, which
        // visibly squashes the creator's footage.
        var gutter = Padding * (columns + 1);
        var cellW = Math.Min(CellWidth, (MaxWidth - gutter) / (float)columns);
        var cellH = cellW * CellHeight / CellWidth;

        var width = (int)Math.Round((columns * cellW) + gutter);
        var height = (int)Math.Round((rows * cellH) + (Padding * (rows + 1)));

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        if (surface is null)
            return null;

        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        for (var i = 0; i < usable.Count; i++)
        {
            var row = i / columns;
            var column = i % columns;

            var left = Padding + (column * (cellW + Padding));
            var top = Padding + (row * (cellH + Padding));

            var cell = new SKRect(left, top, left + cellW, top + cellH);

            // Crop to fill rather than stretch, so a frame of an unexpected
            // aspect ratio is never squashed.
            var source = FitSource(usable[i], cellW / cellH);

            // DrawImage(image, source, dest, sampling, paint). SkiaSharp 3 takes
            // SKSamplingOptions rather than the old SKFilterQuality enum.
            canvas.DrawImage(
                usable[i],
                source,
                cell,
                new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear),
                new SKPaint { IsAntialias = true });
        }

        using var image = surface.Snapshot();
        return Encode(image);
    }
    /// <summary>
    /// The largest centred region of the image with the target aspect ratio.
    /// Matches SKCanvas' "fill" behaviour without distorting the frame.
    /// </summary>
    private static SKRect FitSource(SKImage image, float targetAspect)
    {
        var sourceAspect = image.Width / (float)Math.Max(1, image.Height);
        if (sourceAspect > targetAspect)
        {
            var width = image.Height * targetAspect;
            var left = (image.Width - width) / 2f;
            return new SKRect(left, 0, left + width, image.Height);
        }
        else
        {
            var height = image.Width / targetAspect;
            var top = (image.Height - height) / 2f;
            return new SKRect(0, top, image.Width, top + height);
        }
    }

    private static SKImage? Decode(string base64Jpeg)
    {
        try
        {
            var bytes = Convert.FromBase64String(base64Jpeg);
            return SKImage.FromEncodedData(bytes);
        }
        catch
        {
            // A corrupt frame must not sink the whole montage.
            return null;
        }
    }

    /// <summary>Overload for a finished surface snapshot, which is not a bitmap.</summary>
    private static string Encode(SKImage image)
    {
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, JpegQuality);
        return Convert.ToBase64String(data.ToArray());
    }
}
