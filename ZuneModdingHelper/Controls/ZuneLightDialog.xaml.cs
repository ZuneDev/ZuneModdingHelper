using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
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
            nameof(ViewModel), typeof(DialogViewModel), typeof(ZuneLightDialog), new PropertyMetadata(ViewModelChanged));

        private static void ViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ZuneLightDialog dialog || e.NewValue is not DialogViewModel vm)
                return;

            dialog.DataContext = vm;

            if (vm.Height is not null)
                dialog.Height = vm.Height.Value;

            if (vm is ProgressDialogViewModel)
            {
                var progressBar = new ProgressBar
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Width = double.NaN,
                };

                Binding progValueBinding = new()
                {
                    Source = vm,
                    Path = new PropertyPath(nameof(ProgressDialogViewModel.Progress)),
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                progressBar.SetBinding(ProgressBar.ValueProperty, progValueBinding);

                Binding progIndetBinding = new()
                {
                    Source = vm,
                    Path = new PropertyPath(nameof(ProgressDialogViewModel.IsIndeterminate)),
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                progressBar.SetBinding(ProgressBar.IsIndeterminateProperty, progIndetBinding);

                dialog.InnerPresenter.Content = progressBar;
            }
        }
    }
}
