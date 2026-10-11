namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Latest video-review analysis per lead. The review runs on a worker, so the
/// result must survive the request: the worker saves it here, the details
/// page reads it back. Cap of 200 leads keeps memory bounded.
/// </summary>
public static class VideoReviewStore
{
    private const int MaxEntries = 200;
    private static readonly Dictionary<int, (string Advice, DateTime At)> Entries = new();
    private static readonly object Gate = new();

    public static void Save(int contactId, string advice)
    {
        lock (Gate)
        {
            Entries[contactId] = (advice, DateTime.UtcNow);
            if (Entries.Count > MaxEntries)
            {
                var oldest = Entries.OrderBy(kv => kv.Value.At).First().Key;
                Entries.Remove(oldest);
            }
        }
    }

    public static string? Get(int contactId)
    {
        lock (Gate)
        {
            return Entries.TryGetValue(contactId, out var entry) ? entry.Advice : null;
        }
    }
}
