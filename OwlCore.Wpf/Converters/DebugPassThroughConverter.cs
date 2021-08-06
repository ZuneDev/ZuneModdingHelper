using System;
using System.Globalization;
using System.Windows.Data;

namespace OwlCore.Wpf.Converters
{
    /// <summary>
    /// A converter used for debugging the data passed by a binding.
    /// </summary>
    public sealed class DebugPassThroughConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Debug.Write($"Debug passthrough: Type is {value?.GetType().ToString() ?? "null"}");
            return value;
        }

        /// <inheritdoc/>
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
