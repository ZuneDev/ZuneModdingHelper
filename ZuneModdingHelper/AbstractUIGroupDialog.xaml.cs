using OwlCore.AbstractUI.Models;
using OwlCore.AbstractUI.ViewModels;
using System.Windows;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for AbstractUIGroupDialog.xaml
    /// </summary>
    public partial class AbstractUIGroupDialog : Window
    {
        public AbstractUIGroupDialog(AbstractUICollectionViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }
        public AbstractUIGroupDialog(AbstractUICollection group) : this(new AbstractUICollectionViewModel(group))
        {

        }

        private void OptionsUINextButton_Click(object sender, RoutedEventArgs e) => Finish(true);

        private void OptionsUICancelButton_Click(object sender, RoutedEventArgs e) => Finish(false);

        private void Finish(bool result)
        {
            DialogResult = result;
            Close();
        }

        public AbstractUICollectionViewModel ViewModel
        {
            get => (AbstractUICollectionViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(AbstractUICollectionViewModel), typeof(AbstractUIGroupDialog));
    }
}
