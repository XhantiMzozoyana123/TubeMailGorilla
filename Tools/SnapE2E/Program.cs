using System.Diagnostics;
using TubeMailGorilla.Maui.Unlocked.Services;

namespace SnapE2E;

/// <summary>
/// Calls the REAL in-app VideoSnapshotService.CaptureAsync (not a copy) and
/// reports the snapshot count, timestamps and JPEG sizes. Exit 0 on success.
/// </summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var url = args.Length > 0
            ? args[0]
            : "https://www.youtube.com/watch?v=x5krSIhV3Hc";

        Console.WriteLine($"CaptureAsync({url})");
        var sw = Stopwatch.StartNew();
        var service = new VideoSnapshotService();
        var snapshots = await service.CaptureAsync(url);
        sw.Stop();

        Console.WriteLine($"captured {snapshots.Count} snapshots in {sw.Elapsed.TotalSeconds:F1}s");
        foreach (var s in snapshots)
        {
            var bytes = Convert.FromBase64String(s.Base64Image);
            var isJpeg = bytes.Length > 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
            Console.WriteLine($"  t={s.Seconds,7:F2}s label={s.TimestampLabel} bytes={bytes.Length} jpeg={isJpeg}");
        }

        Console.WriteLine(snapshots.Count > 0 ? "RESULT: PASS" : "RESULT: FAIL");
        return snapshots.Count > 0 ? 0 : 1;
    }
}
