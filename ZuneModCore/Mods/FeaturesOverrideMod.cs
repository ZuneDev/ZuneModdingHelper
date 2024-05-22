using OwlCore.AbstractUI.Models;
using OwlCore.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZuneModCore.Win32;

namespace ZuneModCore.Mods;

public class FeaturesOverrideMod : Mod, IAsyncInit
{
    private const string ZUNE_FEATURESOVERRIDE_REGKEY = RegEdit.ZUNE_REG_PATH + "FeaturesOverride";

    private const string Description = "Re-enables access to some features disabled by Microsoft, such as the Social and Marketplace tabs.\r\n" +
        "Does not restore functionality of those features, but shows them in the software.";

    private const string Author = "Rafael Rivera";

    public override ModMetadata Metadata => new(nameof(FeaturesOverrideMod), "Features Override", Description, Author);

    public override AbstractUICollection? GetDefaultOptionsUI()
    {
        AbstractUICollection optionsUi = new(nameof(FeaturesOverrideMod))
        {
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
        };
        optionsUi.Title = "select features";
        optionsUi.Subtitle = "CHOOSE WHICH ZUNE FEATURES YOU WISH TO ENABLE.";
        return optionsUi;
    }

    public override IReadOnlyList<Type>? DependentMods => null;

    public bool IsInitialized { get; private set; }

    public Task InitAsync(CancellationToken token = default)
    {
        if (IsInitialized) return Task.CompletedTask;

        foreach (AbstractUIElement uiElem in OptionsUI!)
        {
            if (uiElem is AbstractBoolean boolElem)
            {
                bool? featureOverride = GetFeatureOverride(boolElem.Id);
                boolElem.State = featureOverride ?? false;
            }
        }

        IsInitialized = true;
        return Task.CompletedTask;
    }

    public override async Task<string?> Apply() => await Apply(false);

    public async Task<string?> Apply(bool applyAll = false)
    {
        // Use user choices from AbstractUI
        foreach (AbstractUIElement uiElem in OptionsUI!)
        {
            if (uiElem is AbstractBoolean boolElem && (boolElem.State || applyAll))
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
        RegEdit.CurrentUserDeleteKey(ZUNE_FEATURESOVERRIDE_REGKEY);

        return null;
    }

    public static bool SetFeatureOverride(string feature, bool value) =>
        RegEdit.CurrentUserSetBoolValue(ZUNE_FEATURESOVERRIDE_REGKEY, feature, value);

    public static bool? GetFeatureOverride(string feature) =>
        RegEdit.CurrentUserGetBoolValue(ZUNE_FEATURESOVERRIDE_REGKEY, feature);

    public static void ResetFeatureOverride(string feature) =>
        RegEdit.CurrentUserDeleteValue(ZUNE_FEATURESOVERRIDE_REGKEY, feature);
}
