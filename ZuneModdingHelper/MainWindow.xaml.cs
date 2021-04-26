using MahApps.Metro.Controls;
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
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ModList.ItemsSource = Mod.AvailableMods;
            ZuneInstallDirBox.Text = Mod.ZuneInstallDir;
        }

        private async void InstallModsButton_Click(object sender, RoutedEventArgs e)
        {
            ProgressCloseButton.Visibility = Visibility.Collapsed;
            ProgressBorder.Visibility = Visibility.Visible;
            ModList.IsEnabled = false;
            InstallModsButton.IsEnabled = false;
            Mod.ZuneInstallDir = ZuneInstallDirBox.Text;
            var selectedMods = ModList.SelectedItems.Cast<Mod>();

            double progTotal = selectedMods.Count() * 2;
            double progCompleted = 0;

            foreach (Mod mod in selectedMods)
            {
                ProgressDesc.Text = $"Setting up '{mod.Title}'...";
                await mod.Init();
                Progress.Value = ++progCompleted / progTotal * 100;

                // TODO: Implement AbstractUI display for options
                //if (mod.OptionsUI != null)
                //{
                //    var optionsDialog = new AbstractUIGroupDialog();
                //    optionsDialog.OptionsUIPresenter.ViewModel = new AbstractUIElementGroupViewModel(mod.OptionsUI);
                //    optionsDialog.ShowDialog();
                //}

                ProgressDesc.Text = $"Applying '{mod.Title}'...";
                string applyResult = await mod.Apply();
                if (applyResult != null)
                {
                    ProgressDesc.Text = $"Failed to apply '{mod.Title}':\r\n{applyResult}";
                    await Task.Delay(15000);
                    continue;
                }
                Progress.Value = ++progCompleted / progTotal * 100;
            }

            ProgressDesc.Text = "Completed";
            ProgressCloseButton.Visibility = Visibility.Visible;
            ModList.IsEnabled = true;
            InstallModsButton.IsEnabled = true;
        }

        private void ProgressCloseButton_Click(object sender, RoutedEventArgs e)
        {
            ProgressCloseButton.Visibility = Visibility.Collapsed;
            ProgressBorder.Visibility = Visibility.Collapsed;
            ProgressDesc.Text = "Preparing...";
            Progress.Value = 0;
        }
    }
}
