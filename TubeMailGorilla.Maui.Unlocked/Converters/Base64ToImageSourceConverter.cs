using System.Globalization;
using Microsoft.Maui.Controls;

namespace TubeMailGorilla.Maui.Unlocked.Converters
{
    /// <summary>
    /// Turns one of the base64 JPEG snapshots stored on a lead into an
    /// <see cref="ImageSource"/> so it can be bound straight to an
    /// <c>Image.Source</c> in the contact editor.
    ///
    /// The decoded BYTES are cached, but a new <see cref="ImageSource"/> is
    /// created on every conversion. That distinction matters: on Windows
    /// <c>ImageSource.FromStream</c> builds a WPF BitmapImage that can only be
    /// materialised once, so handing the same instance back to a recycled
    /// CarouselView item produces a silently blank image. Caching the bytes
    /// still avoids re-decoding the base64 on every swipe, which is the
    /// expensive part.
    /// </summary>
    public class Base64ToImageSourceConverter : IValueConverter
    {
        private const int MaxCachedImages = 64;

        private static readonly Dictionary<string, byte[]> Cache = new(StringComparer.Ordinal);
        private static readonly object Gate = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string base64 || string.IsNullOrWhiteSpace(base64))
                return null!;

            // Tolerate a full data URI as well as bare base64.
            var payload = base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                ? base64[(base64.IndexOf(',') + 1)..]
                : base64;

            byte[] bytes;

            lock (Gate)
            {
                if (!Cache.TryGetValue(payload, out bytes!))
                {
                    try
                    {
                        // Fully qualified: this class has its own Convert method,
                        // which would otherwise shadow System.Convert here.
                        bytes = System.Convert.FromBase64String(payload);
                    }
                    catch
                    {
                        // A corrupt snapshot must not take the contact page down.
                        return null!;
                    }

                    // A bounded cache: the editor only ever shows one lead's
                    // snapshots at a time, so evicting the oldest is enough.
                    if (Cache.Count >= MaxCachedImages)
                        Cache.Remove(Cache.Keys.First());

                    Cache[payload] = bytes;
                }
            }

            if (bytes.Length == 0) return null!;

            // A fresh stream AND a fresh ImageSource for every bind.
            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
