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
            CurrentMod = mod;
            if (CurrentMod.OptionsUI != null)
            {
                OptionsViewModel = new(CurrentMod.OptionsUI);
            }

            Title = "Options | " + CurrentMod.Title;
        }

        private void OptionsUINextButton_Click(object sender, RoutedEventArgs e) => Finish(true);

        private void OptionsUICancelButton_Click(object sender, RoutedEventArgs e) => Finish(false);

        private void Finish(bool result)
        {
            DialogResult = result;
            Close();
        }

        public Mod CurrentMod
        {
            get => (Mod)GetValue(CurrentModProperty);
            set => SetValue(CurrentModProperty, value);
        }
        public static readonly DependencyProperty CurrentModProperty = DependencyProperty.Register(
            nameof(CurrentMod), typeof(Mod), typeof(OptionsUIDialog));

        public AbstractUICollectionViewModel OptionsViewModel
        {
            get => (AbstractUICollectionViewModel)GetValue(OptionsViewModelProperty);
            set => SetValue(OptionsViewModelProperty, value);
        }
        public static readonly DependencyProperty OptionsViewModelProperty = DependencyProperty.Register(
            nameof(OptionsViewModel), typeof(AbstractUICollectionViewModel), typeof(OptionsUIDialog));

        public bool HasOptions => CurrentMod.OptionsUI != null;
    }
}
