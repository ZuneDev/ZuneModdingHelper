﻿using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace OwlCore.Wpf.Converters.Bools.Visible
{
    /// <summary>
    /// A converter that checks if an object is not null and returns a <see cref="Visibility"/>.
    /// </summary>
    public sealed class NotNullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Checks if an object is null, and returns a <see cref="Visibility"/>.
        /// </summary>
        /// <param name="obj">The object to null check.</param>
        /// <returns><see cref="Visibility.Visible"/> if not null, <see cref="Visibility.Collapsed"/> if null.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Visibility Convert(object? obj) => BoolToVisibilityConverter.Convert(obj != null);

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