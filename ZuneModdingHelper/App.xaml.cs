using System.Windows;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string Title = "Zune Modding Helper";

        public static readonly ReleaseVersion Version = new(2021, 12, 30, 0, Phase.Alpha);
        public static readonly string VersionStr = Version.ToString();

        public const string DonateLink = "http://josh.askharoun.com/donate";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Set up App Center analytics
            AppCenter.SetCountryCode(System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName);
            AppCenter.Start("24903c19-b3d9-4ab5-b445-b981ca647125", typeof(Analytics), typeof(Crashes));

#if DEBUG
            // Disable crash and event analytics when in debug
            AppCenter.SetEnabledAsync(false);
#endif
        }

        public static void OpenInBrowser(string url)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
        }
    }
}
