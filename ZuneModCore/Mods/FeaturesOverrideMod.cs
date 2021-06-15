using System;
using System.Collections.Generic;
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

        public Dictionary<string, bool> AvailableOverrides => new()
        {
            // We don't know what some of these overrides do exactly, so hide them from the user.

            { "Apps", false },
            //{ "Art", false },
            { "Channels", false },
            //{ "FirstLaunchIntroVideo", false },
            { "Games", false },
            { "Marketplace", false },
            //{ "MBRPreview", false },
            //{ "MBRPurchase", false },
            //{ "MBRRental", false },
            { "Music", false },
            { "MusicVideos", false },
            { "Nowplaying", false },
            //{ "NowplayingArt", false },
            { "Picks", false },
            { "Podcasts", false },
            { "QuickMixLocal", false },
            //{ "QuickMixZMP", false },
            { "Quickplay", false },
            //{ "Sign In Available", false },
            { "Social", false },
            //{ "SocialMarketplace", false },
            //{ "SubscriptionFreeTracks", false },
            { "Videos", false },
        };

        public override ReadOnlyCollection<Type>? DependentMods => null;

        public override void Init()
        {
            foreach (AbstractUIElement uiElem in OptionsUI.Items)
            {
                if (uiElem is AbstractBooleanUIElement boolElem)
                {
                    bool? featureOverride = GetFeatureOverride(boolElem.Id);
                    boolElem.ChangeState(featureOverride ?? false);
                }
            }

            return Task.CompletedTask;
        }

        public override async Task<string?> Apply()
        {
            // TODO: Use user choices from AbstractUI
            foreach (AbstractUIElement uiElem in OptionsUI.Items)
            {
                if (uiElem is AbstractBooleanUIElement boolElem)// && boolElem.State)
                {
                    bool isSuccess = SetFeatureOverride(boolElem.Id, true);
                    if (!isSuccess)
                    {
                        string? resetStatus = await Reset();
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

        public override string? Reset()
        {
            for (int i = 0; i < AvailableOverrides.Count; i++)
            {
                string key = AvailableOverrides.Keys.ElementAt(i);
                ResetFeatureOverride(key);
            }
            return null;
        }

        public static bool SetFeatureOverride(string feature, bool value) =>
            RegEdit.CurrentUserSetBoolValue(ZUNE_FEATURESOVERRIDE_REGKEY, feature, value);

        public static bool? GetFeatureOverride(string feature) =>
            RegEdit.CurrentUserGetBoolValue(ZUNE_FEATURESOVERRIDE_REGKEY, feature);

        public static void ResetFeatureOverride(string feature) =>
            RegEdit.CurrentUserDeleteValue(RegEdit.ZUNE_REG_PATH + "FeaturesOverride", feature);
    }
}
