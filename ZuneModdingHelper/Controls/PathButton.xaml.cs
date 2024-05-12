using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
