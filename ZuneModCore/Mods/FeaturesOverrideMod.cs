using OwlCore.AbstractUI.Models;
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

        public override AbstractUIElementGroup OptionsUI => new(nameof(FeaturesOverrideMod))
        {
            Title = "Select features:",
            Items =
            {
                // We don't know what some of these overrides do exactly, so hide them from the user.
                // The ID is the name of the registry key, the label is the display name

                new AbstractBooleanUIElement("Apps", "Apps"),
                new AbstractBooleanUIElement("Art", "Art"),
                new AbstractBooleanUIElement("Channels", "Channels"),
                new AbstractBooleanUIElement("FirstLaunchIntroVideo", "First Launch Intro Video"),
                new AbstractBooleanUIElement("Games", "Games"),
                new AbstractBooleanUIElement("Marketplace", "Marketplace"),
                new AbstractBooleanUIElement("MBRPreview", "[Marketplace] Media Preview"),
                new AbstractBooleanUIElement("MBRPurchase", "[Marketplace] Media Purchase"),
                new AbstractBooleanUIElement("MBRRental", "[Marketplace] Media Rental"),
                new AbstractBooleanUIElement("Music", "Music"),
                new AbstractBooleanUIElement("MusicVideos", "Music Videos"),
                new AbstractBooleanUIElement("Nowplaying", "Now Playing"),
                new AbstractBooleanUIElement("NowplayingArt", "Now Playing Art"),
                new AbstractBooleanUIElement("Picks", "Picks"),
                new AbstractBooleanUIElement("Podcasts", "Podcasts"),
                new AbstractBooleanUIElement("QuickMixLocal", "Quick Mix (Local)"),
                new AbstractBooleanUIElement("QuickMixZMP", "Quick Mix (ZMP)"),
                new AbstractBooleanUIElement("Quickplay", "Quickplay"),
                new AbstractBooleanUIElement("Sign In Available", "Sign In"),
                new AbstractBooleanUIElement("Social", "Social"),
                new AbstractBooleanUIElement("SocialMarketplace", "Social Marketplace"),
                new AbstractBooleanUIElement("SubscriptionFreeTracks", "Subscription Free Tracks"),
                new AbstractBooleanUIElement("Videos", "Videos"),
            }
        };

        public override IReadOnlyList<Type>? DependentMods => null;

        public override Task Init()
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

        public override async Task<string?> Apply() => await Apply(false);

        public async Task<string?> Apply(bool applyAll = false)
        {
            // Use user choices from AbstractUI
            foreach (AbstractUIElement uiElem in OptionsUI.Items)
            {
                if (uiElem is AbstractBooleanUIElement boolElem && (boolElem.State || applyAll))
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

        public override async Task<string?> Reset()
        {
            foreach (AbstractUIElement uiElem in OptionsUI.Items)
                if (uiElem is AbstractBooleanUIElement boolElem)
                    ResetFeatureOverride(boolElem.Id);

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
