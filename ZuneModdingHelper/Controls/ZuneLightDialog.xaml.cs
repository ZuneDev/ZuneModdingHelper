using System.Windows;
using System.Windows.Controls;
using ZuneModdingHelper.Messages;

namespace ZuneModdingHelper.Controls;

public class ZuneLightDialog : ContentControl
{
    static ZuneLightDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ZuneLightDialog), new FrameworkPropertyMetadata(typeof(ZuneLightDialog)));
    }

    public DialogViewModel ViewModel
    {
        get => (DialogViewModel)GetValue(DialogViewModelProperty);
        set => SetValue(DialogViewModelProperty, value);
    }
    public static readonly DependencyProperty DialogViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel), typeof(DialogViewModel), typeof(ZuneLightDialog), new PropertyMetadata(null));
}
