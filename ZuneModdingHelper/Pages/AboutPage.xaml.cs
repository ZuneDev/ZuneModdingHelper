using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Octokit;
using Syroot.Windows.IO;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using ZuneModCore;
using ZuneModdingHelper.Messages;

namespace ZuneModdingHelper.Pages
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    public partial class AboutPage : UserControl
    {
        const string UPDATES_DIALOG_TITLE = "UPDATES";

        private readonly IGitHubClient? _gitHub = Ioc.Default.GetService<IGitHubClient>();
        private ReleaseAsset _latestAsset;
        private string _downloadedUpdatePath;

        public AboutPage()
        {
            InitializeComponent();
        }

        private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            App.OpenInBrowser(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private async void UpdateCheckButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_gitHub is null)
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
                // Get releases list from GitHub
                // Why not use the `releases/latest` endpoint? Good question: https://github.com/octokit/octokit.net/issues/2916
                var releases = await _gitHub.Repository.Release.GetAll("ZuneDev", "ZuneModdingHelper");
                var latest = releases
                    .Select(r => new { Release = r, Version = ReleaseVersion.Parse(r.TagName) })
                    .OrderByDescending(t => t.Version)
                    .ThenBy(t => t.Release.Prerelease)
                    .First();

                if (ReleaseVersion.TryParse(latest.Release.TagName, out var latestVer) || App.Version >= latestVer)
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
                _latestAsset = latest.Release.Assets[0];

                WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
                WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new DialogViewModel
                {
                    Title = UPDATES_DIALOG_TITLE,
                    Description = $"Release {latest.Release.Name} is available. Would you like to download it now?",
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
                Maximum = 100,
            };
            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(prog));

            // Download new version to AppData
            string downloadedFile = Path.Combine(Path.GetTempPath(), _latestAsset.Name);
            if (File.Exists(downloadedFile)) File.Delete(downloadedFile);
            using (var client = new System.Net.WebClient())
            {
                client.DownloadProgressChanged += (object sender, System.Net.DownloadProgressChangedEventArgs e) =>
                {
                    // Update UI with download progress
                    prog.Progress = e.ProgressPercentage;
                };

                prog.Progress = 0;
                prog.IsIndeterminate = false;
                await client.DownloadFileTaskAsync(new Uri(_latestAsset.BrowserDownloadUrl), downloadedFile);
            }

            // Ask user to save file
            Microsoft.Win32.SaveFileDialog saveFileDialog = new()
            {
                FileName = _latestAsset.Name,
                InitialDirectory = new KnownFolder(KnownFolderType.Downloads).Path
            };
            bool dialogResult = saveFileDialog.ShowDialog() ?? false;
            WeakReferenceMessenger.Default.Send<CloseDialogMessage>();

            if (dialogResult)
            {
                _downloadedUpdatePath = saveFileDialog.FileName;
                File.Copy(downloadedFile, _downloadedUpdatePath, true);

                WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new DialogViewModel
                {
                    Title = UPDATES_DIALOG_TITLE,
                    Description = "Update downloaded. Would you like to install the new version?",
                    AffirmativeText = "YES",
                    NegativeText = "NO",
                    ShowAffirmativeButton = true,
                    ShowNegativeButton = true,
                    OnAction = new AsyncRelayCommand<bool>(OnLaunchUpdateDialogResult)
                }));
            }

            File.Delete(downloadedFile);
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
