using System;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace ZuneModdingHelper.Controls;

public abstract class TextTransformerConverter : DependencyObject, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => TransformText(value?.ToString());

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value?.ToString();

    public abstract string TransformText(string value);
}

public sealed class LowercaseConverter : TextTransformerConverter
{
    public override string TransformText(string value) => value.ToLower();
}

public sealed class UppercaseConverter : TextTransformerConverter
{
    public override string TransformText(string value) => value.ToUpper();
}
