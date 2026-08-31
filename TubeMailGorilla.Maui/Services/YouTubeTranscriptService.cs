using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace TubeMailGorilla.Maui.Services;

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
                return string.Empty;
            }

            await Task.WhenAll(stdoutTask, stderrTask);
            if (proc.ExitCode != 0)
                return string.Empty;

            var subFile = Directory.GetFiles(tempDir)
                .FirstOrDefault(f =>
                    f.EndsWith(".vtt", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".srt", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

            if (subFile == null)
                return string.Empty;

            var content = await File.ReadAllTextAsync(subFile, Encoding.UTF8);
            return ExtractPlainTextFromCaptions(content);
        }
        catch
        {
            return string.Empty;
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

        await ExtractLock.WaitAsync();
        try
        {
            if (_ytDlpPath != null && File.Exists(_ytDlpPath))
                return;

            var isWindows = OperatingSystem.IsWindows();
            var assetName = isWindows ? WindowsAsset : macOSAsset;
            var targetName = isWindows ? "yt-dlp.exe" : "yt-dlp";
            var targetPath = Path.Combine(FileSystem.AppDataDirectory, targetName);

            using var input = await FileSystem.OpenAppPackageFileAsync(assetName);
            using var output = File.Create(targetPath);
            await input.CopyToAsync(output);
            await output.FlushAsync();

            if (!isWindows)
            {
                try
                {
                    using var chmod = new Process
                    {
                        StartInfo = new ProcessStartInfo("chmod", $"+x \"{targetPath}\"")
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        },
                    };
                    chmod.Start();
                    chmod.WaitForExit();
                }
                catch { }
            }

            _ytDlpPath = targetPath;
        }
        finally
        {
            ExtractLock.Release();
        }
    }

    private static string ExtractPlainTextFromCaptions(string captionContent)
    {
        var lines = captionContent.Split('\n');
        var textLines = new List<string>();

        foreach (var rawLine in lines)
        {
            var trimmed = rawLine.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            if (trimmed.StartsWith("WEBVTT")) continue;
            if (trimmed.StartsWith("Kind:") || trimmed.StartsWith("Language:")) continue;
            if (trimmed.StartsWith("NOTE ")) continue;
            if (trimmed.Contains("-->")) continue;
            if (Regex.IsMatch(trimmed, @"^\d+$")) continue;
            if (Regex.IsMatch(trimmed, @"^\d{2}:\d{2}:\d{2}")) continue;

            var clean = Regex.Replace(trimmed, "<[^>]+>", "").Trim();
            if (!string.IsNullOrEmpty(clean))
                textLines.Add(clean);
        }

        return string.Join(" ", textLines.Distinct());
    }
}
