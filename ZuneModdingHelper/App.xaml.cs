using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ZuneModCore;
using ZuneModCore.Services;
using ZuneModdingHelper.Services;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string Title = "Zune Modding Helper";

        public static readonly ReleaseVersion Version = new(2024, 5, 18, 0, Phase.Alpha);
        public static readonly string VersionStr = Version.ToString();
        public static readonly System.Uri VersionUri = new($"https://github.com/ZuneDev/ZuneModdingHelper/releases/tag/{VersionStr}");

        public const string DonateLink = "http://josh.askharoun.com/donate";
        public static readonly System.Uri DonateUri = new(DonateLink);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConfigureServices();
        }

        public static void OpenInBrowser(string url)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
        }

        private static void ConfigureServices()
        {
            ServiceCollection services = new();

            Octokit.IGitHubClient github = new Octokit.GitHubClient(new Octokit.ProductHeaderValue(Title.Replace(" ", ""), VersionStr));
            services.AddSingleton(github);

            services.AddSingleton<IModCoreConfig>(Settings.Default);
            services.AddSingleton(Settings.Default);

            Ioc.Default.ConfigureServices(services.BuildServiceProvider());
        }
    }
}
