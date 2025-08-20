using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Syroot.Windows.IO;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using ZuneModdingHelper.Messages;
using ZuneModdingHelper.Services;

namespace ZuneModdingHelper.Pages
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    public partial class AboutPage : UserControl
    {
        const string UPDATES_DIALOG_TITLE = "UPDATES";

        private readonly IUpdateService? _updateService = Ioc.Default.GetService<IUpdateService>();
        private UpdateAvailableInfo _updateInfo;
        private string _downloadedUpdatePath;

        public AboutPage()
        {
            DataContext = this;
            InitializeComponent();
        }

        public bool EnableCheckForUpdates => _updateService is not null;

        private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            App.OpenInBrowser(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private async void UpdateCheckButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_updateService is null)
                return;

            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new ProgressDialogViewModel
            {
                Title = UPDATES_DIALOG_TITLE,
                Description = "Checking for updates, please wait...",
                IsIndeterminate = true,
                ShowAffirmativeButton = false,
            }));

            try
            {
                _updateInfo = await _updateService.FetchAvailableUpdate();
                if (_updateInfo is null)
                {
                    WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
                    WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new DialogViewModel
                    {
                        Title = UPDATES_DIALOG_TITLE,
                        Description = "No updates available.\r\n\r\nYou're already using the latest version.",
                        ShowAffirmativeButton = true,
                    }));
                    return;
                }

                // Newer version available, prompt user to download

                WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
                WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new DialogViewModel
                {
                    Title = UPDATES_DIALOG_TITLE,
                    Description = $"Release {_updateInfo.Name} is available. Would you like to download it now?",
                    AffirmativeText = "YES",
                    NegativeText = "NO",
                    ShowAffirmativeButton = true,
                    ShowNegativeButton = true,
                    OnAction = new AsyncRelayCommand<bool>(OnUpdateDialogResult)
                }));
            }
            catch
            {
                App.OpenInBrowser("https://github.com/ZuneDev/ZuneModdingHelper/releases");
                WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
            }
        }

        private async Task OnUpdateDialogResult(bool accepted)
        {
            WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
            if (!accepted)
                return;

            ProgressDialogViewModel prog = new()
            {
                Title = UPDATES_DIALOG_TITLE,
                Description = "Downloading update...\r\nThis may take a few minutes.",
                IsIndeterminate = true,
                ShowAffirmativeButton = false,
                ShowNegativeButton = false,
                Maximum = 1.0,
            };
            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(prog));

            // Ask user to save file
            Microsoft.Win32.SaveFileDialog saveFileDialog = new()
            {
                FileName = _updateInfo.Name,
                InitialDirectory = new KnownFolder(KnownFolderType.Downloads).Path
            };
            bool dialogResult = saveFileDialog.ShowDialog() ?? false;
            WeakReferenceMessenger.Default.Send<CloseDialogMessage>();

            if (dialogResult)
            {
                _downloadedUpdatePath = saveFileDialog.FileName;

                // Download new version to requested folder with progress updates
                Progress<double> progress = new(p => prog.Progress = p);
                prog.Progress = 0;
                prog.IsIndeterminate = false;
                await _updateService.DownloadUpdate(_updateInfo, _downloadedUpdatePath, progress);

                WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new DialogViewModel
                {
                    Title = UPDATES_DIALOG_TITLE,
                    Description = "Update downloaded. Would you like to launch the new version?",
                    AffirmativeText = "YES",
                    NegativeText = "NO",
                    ShowAffirmativeButton = true,
                    ShowNegativeButton = true,
                    OnAction = new AsyncRelayCommand<bool>(OnLaunchUpdateDialogResult)
                }));
            }
        }

        private async Task OnLaunchUpdateDialogResult(bool accepted)
        {
            WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
            if (!accepted)
                return;

            // In the future, this should launch an installer
            System.Diagnostics.Process.Start("explorer", _downloadedUpdatePath);
        }
    }
}
