using System.Diagnostics;

namespace TubeMailGorilla.Maui.Services;

/// <summary>
/// Locates and prepares the bundled <c>yt-dlp</c> binary for child-process use.
/// Both search and transcript services depend on this helper so the binary is
/// extracted to app data only once per launch.
/// </summary>
public static class YtDlp
{
    private const string WindowsAsset = "yt-dlp.exe";
    private const string macOSAsset = "yt-dlp_macos";

    private static readonly SemaphoreSlim InitLock = new(1, 1);
    private static string? _path;

    /// <summary>
    /// Returns the path to a usable yt-dlp binary, extracting the bundled copy to
    /// the app's data directory on first call (and marking it executable on macOS).
    /// </summary>
    public static async Task<string> GetPathAsync()
    {
        if (_path != null && File.Exists(_path))
            return _path;

        await InitLock.WaitAsync();
        try
        {
            if (_path != null && File.Exists(_path))
                return _path;

            var isWindows = OperatingSystem.IsWindows();
            var assetName = isWindows ? WindowsAsset : macOSAsset;
            var targetName = isWindows ? WindowsAsset : "yt-dlp";
            var targetPath = Path.Combine(FileSystem.AppDataDirectory, targetName);

            using (var input = await FileSystem.OpenAppPackageFileAsync(assetName))
            using (var output = File.Create(targetPath))
            {
                await input.CopyToAsync(output);
                await output.FlushAsync();
            }

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

            _path = targetPath;
            return _path;
        }
        finally
        {
            InitLock.Release();
        }
    }
}
