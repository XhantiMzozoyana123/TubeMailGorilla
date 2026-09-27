using System.Diagnostics;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Locates the <c>yt-dlp</c> binary used by the YouTube search and transcript
/// services.
///
/// Desktop (MAUI) edition: the binary ships inside the app package and is
/// extracted to the app data directory on first use.
///
/// Web (MVC) edition: there is no app package, so the executable is resolved
/// once at startup (see <see cref="Configure"/>):
///   1. <c>YtDlp:Path</c> - an absolute path, or just "yt-dlp" for a PATH lookup.
///   2. &lt;content root&gt;/Tools/yt-dlp(.exe) - dropped in during deployment.
///   3. "yt-dlp" / "yt-dlp.exe" - resolved from the system PATH.
/// </summary>
public static class YtDlp
{
    private const string WindowsExecutable = "yt-dlp.exe";
    private const string UnixExecutable = "yt-dlp";

    private static string? _path;
    private static string _expectedPath = string.Empty;

    /// <summary>The resolved binary path (or bare name for a PATH lookup).</summary>
    public static string ResolvedPath => _path ?? (OperatingSystem.IsWindows() ? WindowsExecutable : UnixExecutable);

    /// <summary>
    /// Absolute path the app looked in first. Surfaced in the UI error so the
    /// message names a real folder the user can drop the binary into, rather
    /// than a relative "Tools/yt-dlp" that resolves somewhere unexpected.
    /// </summary>
    public static string ExpectedPath => _expectedPath;

    /// <summary>True when a usable binary is actually present (absolute path or on PATH).</summary>
    public static bool IsAvailable
    {
        get
        {
            if (string.IsNullOrEmpty(_path))
                return false;

            return Path.GetFileName(_path) != _path ? File.Exists(_path) : FindOnPath(_path) is not null;
        }
    }

    /// <summary>
    /// Resolves the yt-dlp location. Called once from Program.cs, so the static
    /// helpers below never have to guess per request.
    /// </summary>
    public static void Configure(string? configuredPath, string contentRoot)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            _path = configuredPath.Trim();
            _expectedPath = configuredPath.Trim();
            return;
        }

        var exeName = OperatingSystem.IsWindows() ? WindowsExecutable : UnixExecutable;

        // Candidate folders, in order:
        //   <contentRoot>/Tools       - the project's own Tools folder, and where a
        //                               deployed copy would be placed
        //   <contentRoot>/../Tools     - the REPO Tools folder. The MVC content root
        //                               is the project directory, so a developer
        //                               following the repo's top-level Tools
        //                               convention would otherwise be invisible
        //                               here, which is exactly the confusion this
        //                               extra probe removes.
        var candidates = new[]
        {
            Path.Combine(contentRoot, "Tools", exeName),
            Path.Combine(contentRoot, "..", "Tools", exeName)
        };

        // Remember the primary location so the UI can name it in the error.
        _expectedPath = Path.GetFullPath(candidates[0]);

        foreach (var candidate in candidates)
        {
            var full = Path.GetFullPath(candidate);
            if (File.Exists(full))
            {
                _path = full;
                return;
            }
        }

        // Prefer whatever PATH resolves to, else the bare name (a PATH lookup at
        // spawn time, so a system-wide install still works).
        _path = FindOnPath(exeName) ?? exeName;
    }

    private static string? FindOnPath(string fileName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path))
            return null;

        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var candidate = Path.Combine(directory.Trim('"'), fileName);
                if (File.Exists(candidate))
                    return candidate;
            }
            catch
            {
                // Malformed PATH entry - skip it.
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the path to a usable yt-dlp binary. Kept async - and named
    /// identically to the desktop edition - so every call site ports unchanged.
    /// </summary>
    public static Task<string> GetPathAsync() => Task.FromResult(ResolvedPath);
}

/// <summary>
/// Locates the <c>ffmpeg</c> binary used to decode video frames for lead
/// snapshots.
///
/// The desktop edition pulls still frames out of a video with the platform's own
/// decoder (Windows Media Foundation / macOS AVFoundation). This site runs on a
/// Linux VPS in a container where neither exists, so ffmpeg is the decoder here.
/// It is installed in the runtime image (see the Dockerfile), which is why this
/// resolves the same way <see cref="YtDlp"/> does rather than shipping a binary:
///   1. <c>Ffmpeg:Path</c> - an absolute path, or just "ffmpeg" for a PATH lookup.
///   2. &lt;content root&gt;/Tools/ffmpeg(.exe) - dropped in during deployment.
///   3. "ffmpeg" / "ffmpeg.exe" - resolved from the system PATH.
/// </summary>
public static class Ffmpeg
{
    private const string WindowsExecutable = "ffmpeg.exe";
    private const string UnixExecutable = "ffmpeg";

    private static string? _path;
    private static string _expectedPath = string.Empty;

    /// <summary>The resolved binary path (or bare name for a PATH lookup).</summary>
    public static string ResolvedPath => _path ?? (OperatingSystem.IsWindows() ? WindowsExecutable : UnixExecutable);

    /// <summary>
    /// Absolute path the app looked in first. Surfaced in the UI error so the
    /// message names a real folder the user can drop the binary into, rather
    /// than a relative "Tools/yt-dlp" that resolves somewhere unexpected.
    /// </summary>
    public static string ExpectedPath => _expectedPath;

    /// <summary>True when a usable binary is actually present (absolute path or on PATH).</summary>
    public static bool IsAvailable
    {
        get
        {
            if (string.IsNullOrEmpty(_path))
                return false;

            return Path.GetFileName(_path) != _path ? File.Exists(_path) : FindOnPath(_path) is not null;
        }
    }

    /// <summary>
    /// Resolves the ffmpeg location. Called once from Program.cs, so the static
    /// helpers below never have to guess per request.
    /// </summary>
    public static void Configure(string? configuredPath, string contentRoot)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            _path = configuredPath.Trim();
            _expectedPath = configuredPath.Trim();
            return;
        }

        var exeName = OperatingSystem.IsWindows() ? WindowsExecutable : UnixExecutable;
        var bundled = Path.Combine(contentRoot, "Tools", exeName);

        // Prefer the deployment-provided copy, then whatever PATH resolves to.
        _path = File.Exists(bundled) ? bundled : FindOnPath(exeName) ?? exeName;
    }

    private static string? FindOnPath(string fileName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path))
            return null;

        foreach (var directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                var candidate = Path.Combine(directory.Trim('"'), fileName);
                if (File.Exists(candidate))
                    return candidate;
            }
            catch
            {
                // Malformed PATH entry - skip it.
            }
        }

        return null;
    }
}
