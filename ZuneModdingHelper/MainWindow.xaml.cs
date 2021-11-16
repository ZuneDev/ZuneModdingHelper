using ControlzEx.Theming;
using Flurl.Http;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.AppCenter.Analytics;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json.Linq;
using OwlCore.AbstractUI.Models;
using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
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
            AffirmativeButtonText = "OK"
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

        private async void ModInstallButton_Click(object sender, RoutedEventArgs e)
        {
            var progDialog = await this.ShowProgressAsync("Getting ready...", "Preparing to apply mod", settings: defaultMetroDialogSettings);
            Mod mod = (Mod)((FrameworkElement)sender).DataContext;
            Mod.ZuneInstallDir = ZuneInstallDirBox.Text;

            progDialog.Maximum = 3;
            int numCompleted = 0;

            // Stage 0: Initialize mod
            progDialog.SetTitle($"Installing '{mod.Title}'");
            progDialog.SetMessage("Preparing to install...");
            await mod.Init();
            progDialog.SetProgress(++numCompleted);

            // Stage 1: Display AbstractUI for options
            progDialog.SetMessage("Awaiting options...");
            if (mod.OptionsUI != null)
            {
                var optionsDialog = new AbstractUIGroupDialog(mod.OptionsUI);
                bool? optionsResult = optionsDialog.ShowDialog();
                if (!(optionsResult.HasValue && optionsResult.Value))
                {
                    await progDialog.CloseAsync();
                    return;
                }
                mod.OptionsUI = (AbstractUICollection)optionsDialog.ViewModel.Model;
            }
            progDialog.SetProgress(++numCompleted);

            // Stage 2: Apply mod
            progDialog.SetMessage("Applying mod...");
            string applyResult = await mod.Apply();
            if (applyResult != null)
            {
                Analytics.TrackEvent("Failed to apply mod", new Dictionary<string, string> {
                    { "ModID", mod.Id },
                    { "ErrorMessage", applyResult }
                });

                await progDialog.CloseAsync();
                await this.ShowMessageAsync($"Failed to apply '{mod.Title}'", applyResult, settings: defaultMetroDialogSettings);

                return;
            }
            progDialog.SetProgress(++numCompleted);

            Analytics.TrackEvent("Applied mod", new Dictionary<string, string> {
                { "ModID", mod.Id },
            });

            await progDialog.CloseAsync();
            await this.ShowMessageAsync("Completed", $"Installed '{mod.Title}'", settings: defaultMetroDialogSettings);
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
                //    optionsDialog.OptionsUIPresenter.ViewModel = new AbstractUICollectionViewModel(mod.OptionsUI);
                //    optionsDialog.ShowDialog();
                //}

                progDialog.SetMessage($"Resetting '{mod.Title}'");
                string resetResult = await mod.Reset();
                if (resetResult != null)
                {
                    Analytics.TrackEvent("Failed to reset mod", new Dictionary<string, string> {
                        { "ModID", mod.Id },
                        { "ErrorMessage", resetResult }
                    });

                    await progDialog.CloseAsync();
                    await this.ShowMessageAsync("Completed", $"Failed to reset '{mod.Title}':\r\n{resetResult}", settings: defaultMetroDialogSettings);
                    return;
                }

                progDialog.SetProgress(++numCompleted);

                Analytics.TrackEvent("Reset mod", new Dictionary<string, string> {
                    { "ModID", mod.Id },
                });

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
            App.OpenInBrowser(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private async void UpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            var checkDialog = await this.ShowProgressAsync("Checking for updates...", "Please wait.", settings: defaultMetroDialogSettings);
            checkDialog.SetIndeterminate();

            try
            {
                // Get releases list from GitHub
                List<JObject> releases = await "https://api.github.com/repos/ZuneDev/ZuneModdingHelper/releases"
                    .WithHeader("User-Agent", App.Title.Replace(" ", "") + "/" + App.VersionStr)
                    .GetJsonAsync<List<JObject>>();
                JObject latest = releases[0];
                string latestVerStr = latest["tag_name"].Value<string>();
                if (!ReleaseVersion.TryParse(latestVerStr, out var latestVer) || App.Version >= latestVer)
                {
                    // Already up-to-date

                    Analytics.TrackEvent("Checked for updates", new Dictionary<string, string> {
                        { "UpdatesFound", bool.FalseString },
                    });

                    await checkDialog.CloseAsync();
                    await this.ShowMessageAsync("No updates available", "You're already using the latest version.", settings: defaultMetroDialogSettings);
                    return;
                }

                // Newer version available, prompt user to download
                MetroDialogSettings promptSettings = new()
                {
                    ColorScheme = MetroDialogColorScheme.Accented,
                    AnimateShow = true,
                    AnimateHide = true,
                    AffirmativeButtonText = "Download",
                    NegativeButtonText = "Later"
                };
                await checkDialog.CloseAsync();
                var promptResult = await this.ShowMessageAsync("Update available", $"Release {latest["name"]} is available. Would you like to download it now?",
                    MessageDialogStyle.AffirmativeAndNegative, promptSettings);
                bool acceptedUpdate = promptResult == MessageDialogResult.Affirmative;

                if (acceptedUpdate)
                {
                    var progDialog = await this.ShowProgressAsync("Downloading update...", "This may take a few minutes.", settings: defaultMetroDialogSettings);
                    progDialog.Maximum = 100;
                    progDialog.SetIndeterminate();

                    // Download new version to AppData
                    JObject asset = latest["assets"].ToObject<List<JObject>>()[0];
                    string assetName = asset["name"].Value<string>();
                    string downloadedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), assetName);
                    if (File.Exists(downloadedFile)) File.Delete(downloadedFile);
                    using (var client = new System.Net.WebClient())
                    {
                        client.DownloadProgressChanged += (object sender, System.Net.DownloadProgressChangedEventArgs e) => {
                            // Update UI with download progress
                            progDialog.SetProgress(e.ProgressPercentage);
                        };

                        progDialog.SetProgress(0);
                        await client.DownloadFileTaskAsync(new Uri(asset["browser_download_url"].Value<string>()), downloadedFile);
                    }

                    // Ask user to save file
                    Microsoft.Win32.SaveFileDialog saveFileDialog = new()
                    {
                        FileName = assetName,
                        InitialDirectory = new KnownFolder(KnownFolderType.Downloads).Path
                    };
                    bool dialogResult = saveFileDialog.ShowDialog() ?? false;
                    await progDialog.CloseAsync();
                    if (dialogResult)
                    {
                        File.Copy(downloadedFile, saveFileDialog.FileName, true);
                        await this.ShowMessageAsync("Update complete", "You may now exit this program and open the new version.", settings: defaultMetroDialogSettings);
                    }
                }

                Analytics.TrackEvent("Checked for updates", new Dictionary<string, string> {
                    { "UpdatesFound", bool.TrueString },
                    { "Accepted", acceptedUpdate.ToString() },
                });
            }
            catch
            {
                App.OpenInBrowser("https://github.com/ZuneDev/ZuneModdingHelper/releases");
            }
            finally
            {
                if (checkDialog.IsOpen)
                    await checkDialog.CloseAsync();
            }
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e) => App.OpenInBrowser(App.DonateLink);

        private void LocateZuneButton_Click(object sender, RoutedEventArgs e)
        {
            if (CommonFileDialog.IsPlatformSupported)
            {
                CommonOpenFileDialog dialog = new()
                {
                    IsFolderPicker = true,
                    DefaultDirectory = Mod.ZuneInstallDir
                };
                CommonFileDialogResult result = dialog.ShowDialog();
                if (result == CommonFileDialogResult.Ok)
                {
                    ZuneInstallDirBox.Text = dialog.FileName;
                }
            }
            else
            {
                // TODO: Fallback to pre-Vista dialog
                this.ShowMessageAsync("Error",
                    "Please use File Explorer to locate and copy the path.",
                    settings: defaultMetroDialogSettings);
            }
        }
    }
}
