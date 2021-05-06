using System;
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
        public static string Title => "Zune Modding Helper";

        public static Version VersionNum => new(2021, 5, 6, 0);
        public static string VersionStatus => "alpha";
        public static string Version => VersionNum.ToString() + (VersionStatus != string.Empty ? "-" + VersionStatus : string.Empty);

        public static bool CheckIfNewerVersion(string otherStr)
        {
            int idxSplit = otherStr.IndexOf('-');
            Version otherNum = new(otherStr[..idxSplit]);
            string otherStatus = otherStr[(idxSplit + 1)..];

            // TODO: This assumes that the VersionStatus is "alpha"
            bool isNotAlpha = otherStatus != VersionStatus;
            bool isNewer = otherNum > VersionNum;
            return isNotAlpha || isNewer;
        }

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
    }
}
