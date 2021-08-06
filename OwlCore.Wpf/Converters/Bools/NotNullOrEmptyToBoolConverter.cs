using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace OwlCore.Wpf.Converters.Bools.Visible
{
    /// <summary>
    /// A converter that converts checks if a string is null or empty and returns a <see cref="Visibility"/>.
    /// </summary>
    public sealed class NotNullOrEmptyToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Checks if a string is null or empty, and returns a <see cref="Visibility"/>.
        /// </summary>
        /// <param name="str">The string to null or empty check.</param>
        /// <returns><see cref="Visibility.Visible"/> if not null or empty, <see cref="Visibility.Collapsed"/> if null or empty.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Convert(string? str) => !string.IsNullOrEmpty(str);

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return Convert(str);
            }

            return false;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}