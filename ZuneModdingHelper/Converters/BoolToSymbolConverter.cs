using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ZuneModdingHelper.Converters
{
    internal class BoolToSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool val)
                return null;

            var resultsStr = parameter.ToString().Split(',');
            object trueReturnVal = null;
            object falseReturnVal = null;

            if (targetType == typeof(string))
            {
                trueReturnVal = resultsStr[0];
                falseReturnVal = resultsStr[1];
            }
            else if (targetType == typeof(Brush))
            {
                var brushConverter = new BrushConverter();
                trueReturnVal = brushConverter.ConvertFrom(resultsStr[0]);
                falseReturnVal = brushConverter.ConvertFrom(resultsStr[1]);
            }

            return val ? trueReturnVal : falseReturnVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var results = parameter.ToString().Split(',');
            return value.ToString() == results[0];
        }
    }
}
