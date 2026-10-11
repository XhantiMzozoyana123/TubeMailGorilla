using System.IO.Compression;
using System.Text;
using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Packs a lead's video snapshots into a ZIP archive the user can download.
/// Same layout as the MAUI Unlocked edition, so an archive produced on either
/// side is identical: snapshot_NN_HH-MM-SS.jpg frames plus a README manifest.
/// </summary>
public static class SnapshotZipService
{
    /// <summary>
    /// Builds the archive. Returns null when the lead has no readable frames, so
    /// the caller can show a single clear message rather than a corrupt file.
    /// </summary>
    public static (byte[] Data, string FileName)? Build(EmailContact contact)
    {
        var images = contact.VideoSnapshot;
        var stamps = contact.VideoSnapshotTimestamps;

        using var buffer = new MemoryStream();

        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            var written = 0;

            for (var i = 0; i < images.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(images[i])) continue;

                var seconds = i < stamps.Count ? stamps[i] : 0d;

                var entry = archive.CreateEntry(
                    string.Format("snapshot_{0:D2}_{1}.jpg", i, Flatten(TranscriptCue.FormatTimestamp(seconds))),
                    CompressionLevel.Fastest);

                byte[] jpeg;
                try
                {
                    // Stored as bare base64, but tolerate a data-URI prefix.
                    var payload = images[i].StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                        ? images[i][(images[i].IndexOf(',') + 1)..]
                        : images[i];
                    jpeg = Convert.FromBase64String(payload);
                }
                catch
                {
                    // One corrupt frame must not sink the whole archive.
                    continue;
                }

                using var stream = entry.Open();
                stream.Write(jpeg, 0, jpeg.Length);
                written++;
            }

            if (written == 0) return null;

            // Manifest, so the archive still makes sense once it leaves the app.
            var manifest = archive.CreateEntry("README.txt", CompressionLevel.Fastest);
            using (var writer = new StreamWriter(manifest.Open(), Encoding.UTF8))
            {
                writer.WriteLine("TubeMailGorilla - lead video snapshots");
                writer.WriteLine();
                writer.WriteLine($"Lead:     {contact.DisplayName} <{contact.Email}>");
                writer.WriteLine($"Channel:  {contact.Channel}");
                writer.WriteLine($"Video:    {contact.VideoTitle}");
                writer.WriteLine($"Captured: {contact.ExtractedAt:g}");
                if (!string.IsNullOrWhiteSpace(contact.VideoUrl))
                    writer.WriteLine($"Source:   {contact.VideoUrl}");
                writer.WriteLine();
                writer.WriteLine("Frames (one per 10 seconds of the video):");
                for (var i = 0; i < written; i++)
                {
                    var seconds = i < stamps.Count ? stamps[i] : 0d;
                    writer.WriteLine(
                        $"  snapshot_{i:D2}_{Flatten(TranscriptCue.FormatTimestamp(seconds))}.jpg");
                }
            }
        }

        return (buffer.ToArray(), BuildFileName(contact));
    }

    /// <summary>"00:01:24" -&gt; "00-01-24", for use inside a filename.</summary>
    private static string Flatten(string timestamp) => timestamp.Replace(':', '-');

    /// <summary>
    /// A safe, readable file name. Channel names come from scraped YouTube
    /// metadata, so anything illegal in a filename has to go.
    /// </summary>
    private static string BuildFileName(EmailContact contact)
    {
        var source = !string.IsNullOrWhiteSpace(contact.Channel)
            ? contact.Channel
            : contact.Email;

        var safe = new string(source
            .Select(ch => char.IsLetterOrDigit(ch) || ch is ' ' or '-' or '_' ? ch : '-')
            .ToArray())
            .Trim('-', ' ');

        if (safe.Length == 0) safe = "lead";
        if (safe.Length > 60) safe = safe[..60].Trim('-', ' ');

        return $"{safe}-snapshots.zip";
    }
}
