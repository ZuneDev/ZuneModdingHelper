﻿using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace OwlCore.Wpf.Converters.Bools.Visible
{
    /// <summary>
    /// A simple converter that converts a given <see cref="ICollection"/> to an <see cref="Visibility"/> based on the presence of any items in the <see cref="ICollection"/>.
    /// </summary>
    public sealed class CollectionAnyToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts an <see cref="ICollection"/> an <see cref="Visibility"/> based on the presence of any items in the <see cref="ICollection"/>.
        /// </summary>
        /// <param name="value">The <see cref="ICollection"/>.</param>
        /// <returns>A <see cref="Visibility"/>.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Visibility Convert(ICollection value) => BoolToVisibilityConverter.Convert(CollectionAnyToBoolConverter.Convert(value));

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection collection)
            {
                return Convert(collection);
            }

            return Visibility.Collapsed;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
