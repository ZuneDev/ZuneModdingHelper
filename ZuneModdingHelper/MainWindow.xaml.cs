using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using ZuneModCore;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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

        private void InstallModsButton_Click(object sender, RoutedEventArgs e)
        {
            //var progDialog = await this.ShowProgressAsync("Getting ready...", "Preparing to apply mods", settings: defaultMetroDialogSettings);
            Mod.ZuneInstallDir = ZuneInstallDirBox.Text;
            var selectedMods = ModList.SelectedItems.Cast<Mod>();

            //progDialog.Maximum = selectedMods.Count() * 2;
            int numCompleted = 0;

            //progDialog.SetTitle("Installing mods...");
            foreach (Mod mod in selectedMods)
            {
                //progDialog.SetMessage($"Setting up '{mod.Title}'");
                mod.Init();
                ++numCompleted;
                //progDialog.SetProgress(numCompleted);

                // TODO: Implement AbstractUI display for options
                //if (mod.OptionsUI != null)
                //{
                //    var optionsDialog = new AbstractUIGroupDialog();
                //    optionsDialog.OptionsUIPresenter.ViewModel = new AbstractUIElementGroupViewModel(mod.OptionsUI);
                //    optionsDialog.ShowDialog();
                //}

                //progDialog.SetMessage($"Applying '{mod.Title}'");
                string applyResult = mod.Apply();
                if (applyResult != null)
                {
                    //progDialog.SetMessage($"Failed to apply '{mod.Title}':\r\n{applyResult}");
                    continue;
                }
                //progDialog.SetProgress(++numCompleted);
            }

            //await progDialog.CloseAsync();
            //await this.ShowMessageAsync("Completed", "Finished installing selected mods", settings: defaultMetroDialogSettings);
        }

        private void ModResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elem && elem.DataContext is Mod mod)
            {
                //var progDialog = await this.ShowProgressAsync("Getting ready...", "Preparing to reset mod", settings: defaultMetroDialogSettings);
                Mod.ZuneInstallDir = ZuneInstallDirBox.Text;

                //progDialog.Maximum = 2;
                int numCompleted = 0;

                //progDialog.SetTitle("Resetting mod...");
                //progDialog.SetMessage($"Setting up '{mod.Title}'");
                mod.Init();
                ++numCompleted;
                //progDialog.SetProgress(numCompleted);

                // TODO: Implement AbstractUI display for options
                //if (mod.OptionsUI != null)
                //{
                //    var optionsDialog = new AbstractUIGroupDialog();
                //    optionsDialog.OptionsUIPresenter.ViewModel = new AbstractUIElementGroupViewModel(mod.OptionsUI);
                //    optionsDialog.ShowDialog();
                //}

                //progDialog.SetMessage($"Resetting '{mod.Title}'");
                string applyResult = mod.Reset();
                if (applyResult != null)
                {
                    //await progDialog.CloseAsync();
                    //await this.ShowMessageAsync("Completed", $"Failed to reset '{mod.Title}':\r\n{applyResult}", settings: defaultMetroDialogSettings);
                    return;
                }

                //progDialog.SetProgress(++numCompleted);
                //await progDialog.CloseAsync();
                //await this.ShowMessageAsync("Completed", $"Successfully reset '{mod.Title}'", settings: defaultMetroDialogSettings);
            }
        }

        private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            App.OpenInBrowser(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void UpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            //ProgressDialogController checkDialog = await this.ShowProgressAsync("Checking for updates...", "Please wait.", settings: defaultMetroDialogSettings);
            //checkDialog.SetIndeterminate();

            using var client = new System.Net.WebClient();

            // Get releases list from GitHub
            string releasesUrl = "http://api.github.com/repos/yoshiask/ZuneModdingHelper/releases";
            List<JObject> releases = JsonConvert.DeserializeObject<List<JObject>>(client.DownloadString(releasesUrl));

            JObject latest = releases[0];
            if (!App.CheckIfNewerVersion(latest["tag_name"].Value<string>()))
            {
                // Already up-to-date
                //await checkDialog.CloseAsync();
                //await this.ShowMessageAsync("No updates available", "You're already using the latest version.", settings: defaultMetroDialogSettings);
                return;
            }

            // Newer version available, prompt user to download
            //await checkDialog.CloseAsync();
            //var promptResult = await this.ShowMessageAsync("Update available", $"Relase {latest["name"]} is available. Would you like to download it now?",
            //    MessageDialogStyle.AffirmativeAndNegative, promptSettings);

            if (true)//promptResult == MessageDialogResult.Affirmative)
            {
                //var progDialog = await this.ShowProgressAsync("Downloading update...", "This may take a few minutes.", settings: defaultMetroDialogSettings);
                //progDialog.SetIndeterminate();

                // Download new version to AppData
                JObject asset = latest["assets"].ToObject<List<JObject>>()[0];
                string assetName = asset["name"].Value<string>();
                string downloadedFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), assetName);
                if (File.Exists(downloadedFile)) File.Delete(downloadedFile);
                client.DownloadFile(new Uri(asset["browser_download_url"].Value<string>()), downloadedFile);

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
                var promptResult = await this.ShowMessageAsync("Update available", $"Relase {latest["name"]} is available. Would you like to download it now?",
                    MessageDialogStyle.AffirmativeAndNegative, promptSettings);
                bool acceptedUpdate = promptResult == MessageDialogResult.Affirmative;

                    //await progDialog.CloseAsync();
                    //await this.ShowMessageAsync("Update complete", "You may now exit this program and open the new version.", settings: defaultMetroDialogSettings);
                }

                Analytics.TrackEvent("Checked for updates", new Dictionary<string, string> {
                    { "UpdatesFound", bool.TrueString },
                    { "Accepted", acceptedUpdate.ToString() },
                });
            }
            catch
            {
                App.OpenInBrowser("https://github.com/ZuneDev/ZuneModdingHelper/releases");
                if (checkDialog.IsOpen)
                    await checkDialog.CloseAsync();
            }
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e) => App.OpenInBrowser(App.DonateLink);
    }
}
