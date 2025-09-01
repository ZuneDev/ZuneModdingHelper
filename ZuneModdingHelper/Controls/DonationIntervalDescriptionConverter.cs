using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ZuneModdingHelper.Services;

namespace ZuneModdingHelper.Controls;

public class DonationIntervalDescriptionConverter : DependencyObject, IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            DonationRequestInterval.EveryMod => "After every successful mod",
            DonationRequestInterval.OneWeek => "Once a week",
            DonationRequestInterval.TwoWeeks => "Once every two weeks",
            DonationRequestInterval.OneMonth => "Once a month",
            DonationRequestInterval.ThreeMonths => "Once a quarter",
            DonationRequestInterval.SixMonths => "Once every six months",
            DonationRequestInterval.OneYear => "Once a year",
            _ => value?.ToString(),
        };
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
