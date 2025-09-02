using System;
using System.IO;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        public static readonly ReleaseVersion Version = new(2025, 8, 20, 0, Phase.Alpha);
        public static readonly string VersionStr = Version.ToString();
        public static readonly Uri RepoUri = new($"https://github.com/ZuneDev/ZuneModdingHelper");
        public static readonly Uri VersionUri = new($"{RepoUri}/releases/tag/{VersionStr}");

        public const string DonateLink = "http://josh.askharoun.com/donate";
        public static readonly Uri DonateUri = new(DonateLink);

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

        public static void OpenDonationLink() => OpenInBrowser(DonateLink);

        public static void OpenIssueReport(ModMetadata modMetadata, string errorMessage)
        {
            StringBuilder issueBodyBuilder = new();
            issueBodyBuilder.AppendLine("# Description");
            issueBodyBuilder.AppendLine("<!-- Please replace this text with a clear and concise description of the issue you are experiencing. -->");
            issueBodyBuilder.AppendLine();
            issueBodyBuilder.AppendLine("# Details");
            issueBodyBuilder.AppendLine($"**Mod:** {modMetadata.Id} v{modMetadata.Version}");
            issueBodyBuilder.AppendLine(errorMessage);

            var escapedIssueBody = Uri.EscapeDataString(issueBodyBuilder.ToString());
            var issueUrl = $"{RepoUri}/issues/new?title=&body={escapedIssueBody}";
            OpenInBrowser(issueUrl);
        }

        private static void ConfigureServices()
        {
            ServiceCollection services = [];

            services.AddLogging(builder =>
            {
                var logDir = Path.Combine(Settings.AppDataDir, "Logs");
                Directory.CreateDirectory(logDir);
                builder.AddProvider(new FileLoggerProvider(logDir));
            });

            Octokit.IGitHubClient github = new Octokit.GitHubClient(new Octokit.ProductHeaderValue(Title.Replace(" ", ""), VersionStr));
            services.AddSingleton(github);

            services.AddSingleton<IUpdateService, GitHubReleasesUpdateService>();

            services.AddSingleton<IModCoreConfig>(Settings.Default);
            services.AddSingleton(Settings.Default);

            Ioc.Default.ConfigureServices(services.BuildServiceProvider());
        }
    }
}
