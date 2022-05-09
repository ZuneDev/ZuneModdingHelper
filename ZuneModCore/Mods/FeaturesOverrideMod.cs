using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ZuneModCore.Win32;

namespace ZuneModCore.Mods
{
    public class FeaturesOverrideMod : Mod
    {
        private const string ZUNE_FEATURESOVERRIDE_REGKEY = RegEdit.ZUNE_REG_PATH + "FeaturesOverride";

        public override string Id => nameof(FeaturesOverrideMod);

        public override string Title => "Features Override";

        public override string Description => "Re-enables access to some features disabled by Microsoft, such as the Social and Marketplace tabs.\r\n" +
            "Does not restore functionality of those features, but shows them in the software.";

        public override string Author => "Rafael Rivera";

        public override AbstractUICollection? GetDefaultOptionsUI()
        {
            return new(nameof(FeaturesOverrideMod))
            {
                Title = "Select features:",
                Items = new List<AbstractUIElement>
                {
                    // We don't know what some of these overrides do exactly, so hide them from the user.
                    // The ID is the name of the registry key, the label is the display name

                    new AbstractBoolean("Apps", "Apps"),
                    new AbstractBoolean("Art", "Art"),
                    new AbstractBoolean("Channels", "Channels"),
                    new AbstractBoolean("FirstLaunchIntroVideo", "First Launch Intro Video"),
                    new AbstractBoolean("Games", "Games"),
                    new AbstractBoolean("Marketplace", "Marketplace"),
                    new AbstractBoolean("MBRPreview", "[Marketplace] Media Preview"),
                    new AbstractBoolean("MBRPurchase", "[Marketplace] Media Purchase"),
                    new AbstractBoolean("MBRRental", "[Marketplace] Media Rental"),
                    new AbstractBoolean("Music", "Music"),
                    new AbstractBoolean("MusicVideos", "Music Videos"),
                    new AbstractBoolean("Nowplaying", "Now Playing"),
                    new AbstractBoolean("NowplayingArt", "Now Playing Art"),
                    new AbstractBoolean("Picks", "Picks"),
                    new AbstractBoolean("Podcasts", "Podcasts"),
                    new AbstractBoolean("QuickMixLocal", "Quick Mix (Local)"),
                    new AbstractBoolean("QuickMixZMP", "Quick Mix (ZMP)"),
                    new AbstractBoolean("Quickplay", "Quickplay"),
                    new AbstractBoolean("Sign In Available", "Sign In"),
                    new AbstractBoolean("Social", "Social"),
                    new AbstractBoolean("SocialMarketplace", "Social Marketplace"),
                    new AbstractBoolean("SubscriptionFreeTracks", "Subscription Free Tracks"),
                    new AbstractBoolean("Videos", "Videos"),
                }
            };
        }

        public override IReadOnlyList<ModDependency>? DependentMods => null;

        public override Task Init()
        {
            foreach (AbstractUIElement uiElem in OptionsUI.Items)
            {
                if (uiElem is AbstractBoolean boolElem)
                {
                    bool? featureOverride = GetFeatureOverride(boolElem.Id);
                    boolElem.State = featureOverride ?? false;
                }
            }

            return Task.CompletedTask;
        }

        protected override async Task<string?> ApplyCore() => await Apply(false);

        public async Task<string?> Apply(bool applyAll = false)
        {
            // Save backup of current values
            ExportFeatureOverrides(Path.Combine(StorageDirectory, "FeatureOverrides.reg"));

            // Use user choices from AbstractUI
            foreach (AbstractUIElement uiElem in OptionsUI!.Items)
            {
                if (uiElem is AbstractBoolean boolElem && (boolElem.State || applyAll))
                {
                    bool isSuccess = SetFeatureOverride(boolElem.Id, true);
                    if (!isSuccess)
                    {
                        string? resetStatus = await ResetCore();
                        if (resetStatus != null)
                        {
                            // The reset failed as well, return both errors
                            return "Failed to set registry keys. Unable to clean up partial overrides:\r\n" + resetStatus;
                        }
                        else
                        {
                            return "Failed to set registry keys. Automatically cleaned up partial changes.";
                        }
                    }
                }
            }

            return null;
        }

        protected override async Task<string?> ResetCore()
        {
            // Delete all values first, so if a backup exists it has a clean slate
            RegEdit.CurrentUserDeleteKey(ZUNE_FEATURESOVERRIDE_REGKEY);

            FileInfo reg = new(Path.Combine(StorageDirectory, "FeatureOverrides.reg"));
            if (reg.Exists)
            {
                // Load backup of original values
                RegEdit.ImportKey(reg.FullName);
            }

            return null;
        }

        public static void ExportFeatureOverrides(string path) =>
            RegEdit.ExportKey(RegEdit.HIVE_CURRENTUSER, ZUNE_FEATURESOVERRIDE_REGKEY, path);

        public static bool SetFeatureOverride(string feature, bool value) =>
            RegEdit.CurrentUserSetBoolValue(ZUNE_FEATURESOVERRIDE_REGKEY, feature, value);

        public static bool? GetFeatureOverride(string feature) =>
            RegEdit.CurrentUserGetBoolValue(ZUNE_FEATURESOVERRIDE_REGKEY, feature);

        public static void ResetFeatureOverride(string feature) =>
            RegEdit.CurrentUserDeleteValue(ZUNE_FEATURESOVERRIDE_REGKEY, feature);
    }
}
