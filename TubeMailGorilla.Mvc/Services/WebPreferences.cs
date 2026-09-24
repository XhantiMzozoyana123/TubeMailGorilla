using System.Text.Json;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Web equivalent of MAUI's <c>Preferences.Default</c>.
///
/// The desktop edition stores these small settings in the platform preference
/// store; the web server has no such store, so they are kept in a single JSON
/// file under App_Data (path configured once at startup). The public surface
/// mirrors <c>Preferences</c> exactly - Get/Set/Remove with a default value -
/// so <see cref="SendSettings"/> and every call site port unchanged.
/// </summary>
public static class WebPreferences
{
    private static readonly object Gate = new();
    private static Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);
    private static string? _filePath;
    private static bool _loaded;

    /// <summary>Points the store at its JSON file and loads existing values.</summary>
    public static void Configure(string filePath)
    {
        lock (Gate)
        {
            _filePath = filePath;
            _loaded = false;
            _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            EnsureLoaded();
        }
    }

    public static bool Get(string key, bool defaultValue)
        => bool.TryParse(GetRaw(key), out var value) ? value : defaultValue;

    public static int Get(string key, int defaultValue)
        => int.TryParse(GetRaw(key), out var value) ? value : defaultValue;

    public static string Get(string key, string defaultValue)
        => GetRaw(key) ?? defaultValue;

    public static void Set(string key, bool value) => SetRaw(key, value ? "true" : "false");

    public static void Set(string key, int value) => SetRaw(key, value.ToString());

    public static void Set(string key, string value) => SetRaw(key, value);

    public static void Remove(string key)
    {
        lock (Gate)
        {
            EnsureLoaded();
            if (_values.Remove(key))
                Save();
        }
    }

    private static string? GetRaw(string key)
    {
        lock (Gate)
        {
            EnsureLoaded();
            return _values.TryGetValue(key, out var value) ? value : null;
        }
    }

    private static void SetRaw(string key, string value)
    {
        lock (Gate)
        {
            EnsureLoaded();
            _values[key] = value;
            Save();
        }
    }

    private static void EnsureLoaded()
    {
        if (_loaded || string.IsNullOrEmpty(_filePath))
            return;

        _loaded = true;
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _values = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                          ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }
        catch
        {
            // A corrupt/unreadable file must never take the site down - fall back
            // to defaults and rewrite on the next Set.
            _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static void Save()
    {
        if (string.IsNullOrEmpty(_filePath))
            return;

        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(_filePath, JsonSerializer.Serialize(_values));
        }
        catch
        {
            // Best effort - settings are convenience state, not business data.
        }
    }
}
