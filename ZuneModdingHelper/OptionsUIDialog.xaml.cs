using MahApps.Metro.Controls;
using OwlCore.AbstractUI.Models;
using OwlCore.AbstractUI.ViewModels;
using OwlCore.Wpf.AbstractUI.Controls;
using System.Windows;
using ZuneModCore;

namespace ZuneModdingHelper
{
    /// <summary>
    /// A dialog that displays the options and dependencies for the given <see cref="CurrentMod"/>.
    /// </summary>
    public partial class OptionsUIDialog : MetroWindow
    {
        public OptionsUIDialog(Mod mod)
        {
            InitializeComponent();

            DataContext = this;
            ViewModel = new(mod);

            Title = "Options | " + ViewModel.Mod.Title;
        }

        private void OptionsUINextButton_Click(object sender, RoutedEventArgs e) => Finish(true);

        private void OptionsUICancelButton_Click(object sender, RoutedEventArgs e) => Finish(false);

        private void Finish(bool result)
        {
            DialogResult = result;
            Close();
        }

        public ModViewModel ViewModel
        {
            get => (ModViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(ModViewModel), typeof(OptionsUIDialog));

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadAsync();
        }
    }
}
