using MahApps.Metro.Controls;
using OwlCore.AbstractUI.Models;
using OwlCore.AbstractUI.ViewModels;
using OwlCore.Wpf.AbstractUI.Controls;
using System.Windows;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for AbstractUIGroupDialog.xaml
    /// </summary>
    public partial class AbstractUIGroupDialog : MetroWindow
    {
        public AbstractUIGroupDialog(AbstractUIElementGroupViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }
        public AbstractUIGroupDialog(AbstractUIElementGroup group) : this(new AbstractUIElementGroupViewModel(group))
        {

        }

        private void OptionsUINextButton_Click(object sender, RoutedEventArgs e) => Finish(true);

        private void OptionsUICancelButton_Click(object sender, RoutedEventArgs e) => Finish(false);

        private void Finish(bool result)
        {
            DialogResult = result;
            Close();
        }

        public AbstractUIElementGroupViewModel ViewModel
        {
            get => (AbstractUIElementGroupViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(AbstractUIElementGroupViewModel), typeof(AbstractUIGroupDialog));
    }
}
