using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ZuneModdingHelper.Controls;

public class PathButton : Button
{
    static PathButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PathButton), new FrameworkPropertyMetadata(typeof(PathButton)));
    }

    public Geometry PathData
    {
        get => (Geometry)GetValue(PathDataProperty);
        set => SetValue(PathDataProperty, value);
    }
    public static readonly DependencyProperty PathDataProperty = DependencyProperty.Register(
        nameof(PathData), typeof(Geometry), typeof(PathButton), new PropertyMetadata(null));
}
