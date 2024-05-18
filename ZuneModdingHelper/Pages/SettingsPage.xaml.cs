using CommunityToolkit.Mvvm.Messaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ZuneModdingHelper.Messages;
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

        private void KillZuneButton_Click(object sender, RoutedEventArgs e)
        {
            var curProc = Process.GetCurrentProcess();
            var zuneProcs = Process.GetProcesses()
                .Where(p =>
                    p.ProcessName.Contains("zune", System.StringComparison.OrdinalIgnoreCase)
                    && p.Id != curProc.Id
                );

            StringBuilder sb = new("The following processes were killed:\r\n");
            bool anyKilled = false;

            foreach (var proc in zuneProcs)
            {
                var id = proc.Id;
                var name = proc.ProcessName;
                var path = OwlCore.Flow.Catch(() => proc.MainModule.FileName);

                try
                {
                    proc.Kill(true);
                    
                    sb.AppendFormat("    • ({0}) {1}", id, name);
                    if (path is not null)
                        sb.AppendFormat(" @ {0}", path);
                    sb.AppendLine();

                    anyKilled = true;
                }
                catch { }
            }

            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new()
            {
                Description = anyKilled ? sb.ToString() : "No processes were killed.",
            }));
        }
    }

    internal class SettingsViewModel
    {
        public Settings Settings { get; init; }

        public Visibility FolderPickerVisibility { get; init; }
    }
}
