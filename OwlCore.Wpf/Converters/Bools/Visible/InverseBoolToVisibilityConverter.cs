using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace OwlCore.Wpf.Converters.Bools.Visible
{
    /// <summary>
    /// A converter that converts a the inverse of a given <see cref="bool"/> a <see cref="Visibility"/>.
    /// </summary>
    public sealed class InverseBoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets a <see cref="Visibility"/> based on the opposite of a bool.
        /// </summary>
        /// <param name="data">The bool to represented.</param>
        /// <returns><see cref="Visibility.Collapsed"/> if <see cref="true"/>, <see cref="Visibility.Visible"/> if <see cref="false"/></returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Visibility Convert(bool data) => BoolToVisibilityConverter.Convert(!data);

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bValue)
            {
                return Convert(bValue);
            }

            return Visibility.Visible;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}