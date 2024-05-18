using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Windows.Controls;
using ZuneModdingHelper.Services;

namespace ZuneModdingHelper.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        public SettingsPage(Settings settings)
        {
            ViewModel = new()
            {
                Settings = settings,
                FolderPickerVisibility = CommonFileDialog.IsPlatformSupported
                    ? Visibility.Visible : Visibility.Collapsed,
            };
            DataContext = ViewModel;

            InitializeComponent();
        }

        private SettingsViewModel ViewModel { get; }

        private async void SettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.Settings.SaveAsync();
        }

        private void LocateZuneButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new()
            {
                IsFolderPicker = true,
                DefaultDirectory = ViewModel.Settings.ZuneInstallDir
            };
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                ViewModel.Settings.ZuneInstallDir = dialog.FileName;
            }
        }
    }

    internal class SettingsViewModel
    {
        public Settings Settings { get; init; }

        public Visibility FolderPickerVisibility { get; init; }
    }
}
