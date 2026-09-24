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

    /// <summary>The resolved binary path (or bare name for a PATH lookup).</summary>
    public static string ResolvedPath => _path ?? (OperatingSystem.IsWindows() ? WindowsExecutable : UnixExecutable);

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

    /// <summary>
    /// Returns the path to a usable yt-dlp binary. Kept async - and named
    /// identically to the desktop edition - so every call site ports unchanged.
    /// </summary>
    public static Task<string> GetPathAsync() => Task.FromResult(ResolvedPath);
}
