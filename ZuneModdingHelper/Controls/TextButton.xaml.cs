using System.Windows;
using System.Windows.Controls.Primitives;

namespace ZuneModdingHelper.Controls;

public class TextButton : ToggleButton
{
    static TextButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TextButton), new FrameworkPropertyMetadata(typeof(TextButton)));
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(TextButton), new PropertyMetadata(string.Empty));

    public bool AllowUncheck
    {
        get => (bool)GetValue(AllowUncheckProperty);
        set => SetValue(AllowUncheckProperty, value);
    }
    public static readonly DependencyProperty AllowUncheckProperty = DependencyProperty.Register(
        nameof(AllowUncheck), typeof(bool), typeof(TextButton), new PropertyMetadata(true));

    protected override void OnChecked(RoutedEventArgs e)
    {
        if (!AllowUncheck)
            IsEnabled = false;

        base.OnChecked(e);
    }

    protected override void OnUnchecked(RoutedEventArgs e)
    {
        IsEnabled = true;
        base.OnUnchecked(e);
    }
}
