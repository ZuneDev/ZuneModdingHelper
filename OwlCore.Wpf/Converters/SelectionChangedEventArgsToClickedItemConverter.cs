using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;

namespace OwlCore.Wpf.Converters
{
    /// <summary>
    /// A converter that returns the first element of <see cref="SelectionChangedEventArgs.AddedItems"/>
    /// from a <see cref="SelectionChangedEventArgs"/>.
    /// </summary>
    public sealed class SelectionChangedEventArgsToClickedItemConverter : IValueConverter
    {
        /// <summary>
        /// Gets the <see cref="ItemClickEventArgs.ClickedItem"/> from a <see cref="ItemClickEventArgs"/> .
        /// </summary>
        /// <param name="args">The event args to check.</param>
        /// <returns>The clicked item, cast to <see cref="object"/>.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Convert(SelectionChangedEventArgs args) => args.AddedItems[0];

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SelectionChangedEventArgs args)
                return Convert(args);

            return false;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}