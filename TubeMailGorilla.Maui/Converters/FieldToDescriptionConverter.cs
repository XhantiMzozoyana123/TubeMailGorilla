using System.Globalization;
using Microsoft.Maui.Controls;

namespace TubeMailGorilla.Maui.Converters
{
    /// <summary>
    /// Turns an internal parameter field name (e.g. "first-name") into the
    /// plain-English description shown to users on the Settings page, e.g.
    /// "The lead's first name". Keeps the internal field concept invisible.
    /// </summary>
    public class FieldToDescriptionConverter : IValueConverter
    {
        private static readonly Dictionary<string, string> Descriptions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["first-name"] = "The lead's first name",
            ["last-name"] = "The lead's last name",
            ["name"] = "The lead's full name",
            ["email"] = "The lead's email address",
            ["channel-name"] = "Their YouTube channel name",
            ["video-title"] = "Their latest video title",
            ["video-description"] = "Their video description",
            ["icebreaker"] = "AI-written personalized first line"
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is string field && Descriptions.TryGetValue(field, out var description)
                ? description
                : $"Lead data: {value}";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}