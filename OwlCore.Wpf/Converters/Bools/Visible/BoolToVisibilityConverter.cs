﻿using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace OwlCore.Wpf.Converters.Bools.Visible
{
    /// <summary>
    /// A converter that converts a given <see cref="bool"/> a <see cref="Visibility"/>.
    /// </summary>
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Returns visible if the boolean is true.
        /// </summary>
        /// <param name="data">boolean to check.</param>
        /// <returns>Collapsed if false, otherwise Visible.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Visibility Convert(bool data) => data ? Visibility.Visible : Visibility.Collapsed;

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value is bool bValue && bValue);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}