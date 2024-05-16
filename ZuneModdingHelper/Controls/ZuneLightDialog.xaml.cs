using System.Windows;
using System.Windows.Controls;
using ZuneModdingHelper.Messages;

namespace ZuneModdingHelper.Controls
{
    /// <summary>
    /// Interaction logic for ZuneLightDialog.xaml
    /// </summary>
    public partial class ZuneLightDialog : UserControl
    {
        public ZuneLightDialog()
        {
            InitializeComponent();
        }

        public DialogViewModel ViewModel
        {
            get => (DialogViewModel)GetValue(DialogViewModelProperty);
            set => SetValue(DialogViewModelProperty, value);
        }
        public static readonly DependencyProperty DialogViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(DialogViewModel), typeof(ZuneLightDialog), new PropertyMetadata(null));
    }
}
