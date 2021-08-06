using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace OwlCore.Wpf.Converters.Bools
{
    /// <summary>
    /// A converter that converts checks null checks an object.
    /// </summary>
    public sealed class NotNullToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Checks if an object is null.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>True if not null, false if null</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Convert(object? obj) => obj != null;

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}