using System.Globalization;
using Microsoft.Maui.Controls;

namespace TubeMailGorilla.Maui.Converters
{
    /// <summary>
    /// Inverts a boolean value. Mainly used to bind <c>IsVisible</c>/<c>IsEnabled</c>
    /// to the negation of a flag (e.g. show a control while <c>IsLoading</c> is
    /// false, or show the Login form while <c>IsRegisterMode</c> is false) without
    /// adding duplicate properties on every view model.
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            !(value is bool b) || !b;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            !(value is bool b) || !b;
    }
}
