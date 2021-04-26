using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using OwlCore.AbstractUI.ViewModels;
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
using ZuneModCore;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
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
            var dialog = await this.ShowProgressAsync("Getting ready...", "Preparing to apply mods");
            ModList.IsEnabled = false;
            InstallModsButton.IsEnabled = false;
            Mod.ZuneInstallDir = ZuneInstallDirBox.Text;
            var selectedMods = ModList.SelectedItems.Cast<Mod>();

            dialog.Maximum = selectedMods.Count() * 2;
            int numCompleted = 0;

            dialog.SetTitle("Installing mods...");
            foreach (Mod mod in selectedMods)
            {
                dialog.SetMessage($"Setting up '{mod.Title}'");
                await mod.Init();
                dialog.SetProgress(++numCompleted);

                // TODO: Implement AbstractUI display for options
                //if (mod.OptionsUI != null)
                //{
                //    var optionsDialog = new AbstractUIGroupDialog();
                //    optionsDialog.OptionsUIPresenter.ViewModel = new AbstractUIElementGroupViewModel(mod.OptionsUI);
                //    optionsDialog.ShowDialog();
                //}

                dialog.SetMessage($"Applying '{mod.Title}'");
                string applyResult = await mod.Apply();
                if (applyResult != null)
                {
                    dialog.SetMessage($"Failed to apply '{mod.Title}':\r\n{applyResult}");
                    await Task.Delay(15000);
                    continue;
                }
                dialog.SetProgress(++numCompleted);
            }

            dialog.SetTitle("Completed");
            dialog.SetMessage("Finished installing selected mods");
            await Task.Delay(5000);
            dialog.CloseAsync();
            ModList.IsEnabled = true;
            InstallModsButton.IsEnabled = true;
        }
    }
}
