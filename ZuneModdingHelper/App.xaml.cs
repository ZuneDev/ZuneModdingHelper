using System;
using System.Windows;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string Title => "Zune Modding Helper";

        public static Version VersionNum => new(2021, 4, 26, 1);
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
    }
}
