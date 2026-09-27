using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Extracts YouTube video transcripts/subtitles by invoking the standalone
/// <c>yt-dlp</c> binary, exactly mirroring the proven
/// <c>yt-transcript-service</c> (FastAPI microservice) logic.
///
/// Bundle layout (Resources/Raw):
///   - Windows: yt-dlp.exe
///   - macOS:   yt-dlp_macos
/// </summary>
public class YouTubeTranscriptService
{
    private const string WindowsAsset = "yt-dlp.exe";
    private const string macOSAsset = "yt-dlp_macos";
    private const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
        "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
    private const int ProcessTimeoutMs = 120000;

    private static readonly SemaphoreSlim ExtractLock = new(1, 1);
    private string? _ytDlpPath;

    /// <summary>
    /// Download (auto-generated) subtitles for a YouTube video and return
    /// them as plain text. Returns an empty string if none could be fetched.
    /// </summary>
    public async Task<string> ExtractTranscriptAsync(string videoUrl, string lang = "en")
    {
        var cues = await ExtractTranscriptCuesAsync(videoUrl, lang);
        if (cues.Count == 0) return string.Empty;

        return string.Join(" ", cues.Select(c => c.Text).Where(t => !string.IsNullOrEmpty(t)).Distinct());
    }

    /// <summary>
    /// Same download as <see cref="ExtractTranscriptAsync"/>, but keeps the
    /// WebVTT/SRT timing information instead of flattening it to plain text.
    /// The cue start times are what the snapshot extractor seeks to.
    /// </summary>
    public async Task<List<TranscriptCue>> ExtractTranscriptCuesAsync(string videoUrl, string lang = "en")
    {
        await EnsureYtDlpAsync();

        await ExtractLock.WaitAsync();
        var tempDir = Path.Combine(Path.GetTempPath(), "ytdlp_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _ytDlpPath!,
                WorkingDirectory = tempDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            psi.ArgumentList.Add("--write-auto-sub");
            psi.ArgumentList.Add("--sub-lang");
            psi.ArgumentList.Add(lang);
            psi.ArgumentList.Add("--skip-download");
            psi.ArgumentList.Add("--quiet");
            psi.ArgumentList.Add("--no-warnings");
            psi.ArgumentList.Add("--extractor-args");
            psi.ArgumentList.Add("youtube:player_client=android");
            psi.ArgumentList.Add("--user-agent");
            psi.ArgumentList.Add(UserAgent);
            psi.ArgumentList.Add("--output");
            psi.ArgumentList.Add("video");
            psi.ArgumentList.Add(videoUrl);

            using var proc = new Process { StartInfo = psi };
            proc.Start();

            var stdoutTask = proc.StandardOutput.ReadToEndAsync();
            var stderrTask = proc.StandardError.ReadToEndAsync();

            if (!proc.WaitForExit(ProcessTimeoutMs))
            {
                try { proc.Kill(entireProcessTree: true); } catch { }
                return new List<TranscriptCue>();
            }

            await Task.WhenAll(stdoutTask, stderrTask);
            if (proc.ExitCode != 0)
                return new List<TranscriptCue>();

            var subFile = Directory.GetFiles(tempDir)
                .FirstOrDefault(f =>
                    f.EndsWith(".vtt", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".srt", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".srt", StringComparison.OrdinalIgnoreCase));

            if (subFile == null)
                return new List<TranscriptCue>();

            var content = await File.ReadAllTextAsync(subFile, Encoding.UTF8);
            return ParseCues(content);
        }
        catch
        {
            return new List<TranscriptCue>();
        }
        finally
        {
            ExtractLock.Release();
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    private async Task EnsureYtDlpAsync()
    {
        if (_ytDlpPath != null && File.Exists(_ytDlpPath))
            return;

        // The desktop edition extracts a bundled binary from the app package.
        // The web edition has no app package - the path comes from configuration
        // (YtDlp:Path, <content root>/Tools/yt-dlp, or the system PATH), resolved
        // once in Program.cs.
        _ytDlpPath = await YtDlp.GetPathAsync();
    }

    /// <summary>
    /// Parses WebVTT (and SRT) into timestamped cues.
    ///
    /// YouTube auto-captions repeat each line with a rolling window of
    /// (<c>00:00:01.000 --&gt; 00:00:04.000</c>), so consecutive cues often
    /// share text. Identical text is only kept when its start time moved, which
    /// keeps one snapshot per spoken sentence instead of one per word.
    /// </summary>
    private static List<TranscriptCue> ParseCues(string captionContent)
    {
        var cues = new List<TranscriptCue>();
        var lines = captionContent.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        double? start = null, end = null;
        var text = new List<string>();

        void Flush()
        {
            if (start is null) return;

            var joined = string.Join(" ", text)
                .Replace("<c>", "").Replace("</c>", "")
                .Trim();

            if (joined.Length > 0)
            {
                var previous = cues.Count > 0 ? cues[^1] : null;
                if (previous is null || previous.Text != joined || start.Value - previous.StartSeconds >= 1.0)
                    cues.Add(new TranscriptCue(start.Value, end ?? start.Value, joined));
            }

            start = null;
            end = null;
            text.Clear();
        }

        foreach (var raw in lines)
        {
            var trimmed = raw.Trim();

            if (trimmed.Length == 0)
            {
                Flush();
                continue;
            }

            if (trimmed.StartsWith("WEBVTT", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("Kind:", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("Language:", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("NOTE", StringComparison.Ordinal))
            {
                continue;
            }

            if (TryParseTimestampLine(trimmed, out var cueStart, out var cueEnd))
            {
                Flush();
                start = cueStart;
                end = cueEnd;
                continue;
            }

            // A bare cue index (SRT) - it carries no text, so drop it.
            if (Regex.IsMatch(trimmed, @"^\d+$")) continue;

            // YouTube's VTT header line: "Kind: captions" / "Language: en" is
            // already skipped above, so anything left is caption text.
            text.Add(Regex.Replace(trimmed, "<[^>]+>", ""));
        }

        Flush();

        return cues;
    }

    /// <summary>
    /// Parses "00:00:01.000 --&gt; 00:00:04.000" (WebVTT) and
    /// "00:00:01,000 --&gt; 00:00:04,000" (SRT) into seconds.
    /// </summary>
    private static bool TryParseTimestampLine(string line, out double start, out double end)
    {
        start = 0;
        end = 0;

        var parts = line.Split("-->", StringSplitOptions.TrimEntries);
        if (parts.Length != 2) return false;

        if (!TryParseTimestamp(parts[0], out start)) return false;
        if (!TryParseTimestamp(parts[1], out end)) return false;

        return true;
    }

    private static bool TryParseTimestamp(string value, out double seconds)
    {
        seconds = 0;

        // Strip any trailing cue settings (WebVTT: "align:start position:0%").
        var token = value.Split(' ')[0].Replace(',', '.');
        var parts = token.Split(':');
        if (parts.Length is < 2 or > 3) return false;

        if (!double.TryParse(parts[^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var s)) return false;
        if (!double.TryParse(parts[^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var m)) return false;
        var h = 0d;
        if (parts.Length == 3 && !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out h))
            return false;

        seconds = h * 3600 + m * 60 + s;
        return true;
    }
}

/// <summary>
/// One timed caption line: the seek position a video snapshot is taken from,
/// plus the words spoken there.
/// </summary>
public class TranscriptCue
{
    public TranscriptCue(double startSeconds, double endSeconds, string text)
    {
        StartSeconds = startSeconds;
        EndSeconds = endSeconds;
        Text = text;
    }

    public double StartSeconds { get; }
    public double EndSeconds { get; }
    public string Text { get; }

    /// <summary>"00:00:01" - shown next to the snapshot in the lead editor.</summary>
    public string TimestampLabel => FormatTimestamp(StartSeconds);

    /// <summary>Formats a seek position as the "hh:mm:ss" label shown in the UI.</summary>
    public static string FormatTimestamp(double seconds)
    {
        var total = (int)Math.Max(0, seconds);
        return $"{total / 3600:00}:{total / 60 % 60:00}:{total % 60:00}";
    }
}