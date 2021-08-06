using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Globalization;

namespace OwlCore.Wpf.Converters.Bools
{
    /// <summary>
    /// A converter that converts a given <see cref="ICollection"/> to an bool based on the lack of presence of any items in the <see cref="ICollection"/>.
    /// </summary>
    public sealed class CollectionNotAnyToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Gets whether or not a collection is empty.
        /// </summary>
        /// <param name="collection">The collection to check.</param>
        /// <returns>Whether or not the colleciton contains no elements</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Convert(ICollection collection) => collection.Count <= 0;

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ICollection collection)
            {
                return Convert(collection);
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
