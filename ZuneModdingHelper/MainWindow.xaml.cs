using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using OwlCore.AbstractUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ZuneModCore;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly MetroDialogSettings defaultMetroDialogSettings = new()
        {
            ColorScheme = MetroDialogColorScheme.Accented,
            AnimateShow = true,
            AnimateHide = true,
            AffirmativeButtonText = "Close"
        };

        public MainWindow()
        {
            InitializeComponent();

            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ModList.ItemsSource = Mod.AvailableMods;
            ZuneInstallDirBox.Text = Mod.ZuneInstallDir;
        }

        private async void InstallModsButton_Click(object sender, RoutedEventArgs e)
        {
            var progDialog = await this.ShowProgressAsync("Getting ready...", "Preparing to apply mods", settings: defaultMetroDialogSettings);
            Mod.ZuneInstallDir = ZuneInstallDirBox.Text;
            var selectedMods = ModList.SelectedItems.Cast<Mod>();

            progDialog.Maximum = selectedMods.Count() * 2;
            int numCompleted = 0;

            progDialog.SetTitle("Installing mods...");
            foreach (Mod mod in selectedMods)
            {
                progDialog.SetMessage($"Setting up '{mod.Title}'");
                await mod.Init();
                progDialog.SetProgress(++numCompleted);

                // TODO: Implement AbstractUI display for options
                //if (mod.OptionsUI != null)
                //{
                //    var optionsDialog = new AbstractUIGroupDialog();
                //    optionsDialog.OptionsUIPresenter.ViewModel = new AbstractUIElementGroupViewModel(mod.OptionsUI);
                //    optionsDialog.ShowDialog();
                //}

                progDialog.SetMessage($"Applying '{mod.Title}'");
                string applyResult = await mod.Apply();
                if (applyResult != null)
                {
                    progDialog.SetMessage($"Failed to apply '{mod.Title}':\r\n{applyResult}");
                    await Task.Delay(15000);
                    continue;
                }
                progDialog.SetProgress(++numCompleted);
            }

            await progDialog.CloseAsync();
            await this.ShowMessageAsync("Completed", "Finished installing selected mods", settings: defaultMetroDialogSettings);
        }

        private async void ModResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is Mod mod)
            {
                var progDialog = await this.ShowProgressAsync("Getting ready...", "Preparing to reset mod", settings: defaultMetroDialogSettings);
                Mod.ZuneInstallDir = ZuneInstallDirBox.Text;

                progDialog.Maximum = 2;
                int numCompleted = 0;

                progDialog.SetTitle("Resetting mod...");
                progDialog.SetMessage($"Setting up '{mod.Title}'");
                await mod.Init();
                progDialog.SetProgress(++numCompleted);

                // TODO: Implement AbstractUI display for options
                //if (mod.OptionsUI != null)
                //{
                //    var optionsDialog = new AbstractUIGroupDialog();
                //    optionsDialog.OptionsUIPresenter.ViewModel = new AbstractUIElementGroupViewModel(mod.OptionsUI);
                //    optionsDialog.ShowDialog();
                //}

                progDialog.SetMessage($"Resetting '{mod.Title}'");
                string applyResult = await mod.Reset();
                if (applyResult != null)
                {
                    await progDialog.CloseAsync();
                    await this.ShowMessageAsync("Completed", $"Failed to reset '{mod.Title}':\r\n{applyResult}", settings: defaultMetroDialogSettings);
                    return;
                }

                progDialog.SetProgress(++numCompleted);
                await progDialog.CloseAsync();
                await this.ShowMessageAsync("Completed", $"Successfully reset '{mod.Title}'", settings: defaultMetroDialogSettings);
            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            AboutFlyout.Width = Math.Max(Math.Min(500, ActualWidth), ActualWidth / 3);
            AboutFlyout.IsOpen = true;
        }

        private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
            e.Handled = true;
        }
    }
}
