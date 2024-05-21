using OwlCore.AbstractUI.Models;
using OwlCore.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static ZuneModCore.Mods.FeaturesOverrideMod;

namespace ZuneModCore.Mods;

public class WebservicesMod : Mod, IAsyncInit
{
    private const int ZUNESERVICES_ENDPOINTS_BLOCK_OFFSET = 0x14D60;
    private const int ZUNESERVICES_ENDPOINTS_BLOCK_LENGTH = 0x884;
    private const string WEBSERVICE_ICON_TESTING = "\uE72C";
    private const string WEBSERVICE_ICON_SUCCESS = "\uE73E";
    private const string WEBSERVICE_ICON_UNREACHABLE = "\uF384";
    private const string WEBSERVICE_ICON_UNAVAILABLE = "\uE894";
    private const string WEBSERVICE_SUBTITLE_TESTING = "Testing...";
    private const string WEBSERVICE_SUBTITLE_SUCCESS = "OK";
    private const string WEBSERVICE_SUBTITLE_UNREACHABLE = "Unreachable";

    private static readonly string[] _affectedFeatures =
    [
        "Apps", "Channels", "Games", "Marketplace",
        "MBRPreview", "MBRPurchase", "MBRRental",
        "Music", "MusicVideos", "Picks", "Podcasts",
        "Sign In Available", "Social", "Videos",
        "SocialMarketplace", "SubscriptionFreeTracks",
    ];

    private HttpClient _client;
    private CancellationTokenSource _cts = new();

    public override string Id => nameof(WebservicesMod);

    public override string Title => "Community Webservices";

    public override string Description => "Partially restores online features such as the Marketplace by patching the Zune desktop software " +
        "to use the community's recreation of Microsoft's Zune servers at zunes.me (instead of zune.net).";

    public override string Author => "Joshua \"Yoshi\" Askharoun";

    public override AbstractUICollection? GetDefaultOptionsUI()
    {
        AbstractUICollection optionsUi = new(nameof(FeaturesOverrideMod))
        {
            new AbstractTextBox("hostBox", string.Empty, "zune.net")
            {
                Title = "host",
                TooltipText = "The host where the replacement servers are located. Must be the same length as \"zune.net\"."
            },
            new AbstractDataList("hostsTest", new List<AbstractUIMetadata>()
                {
                    GetWebserviceAvailabilityUI("Main website", "www"),
                    GetWebserviceAvailabilityUI("Social website", "social"),
                    GetWebserviceAvailabilityUI("Catalog", "catalog"),
                    GetWebserviceAvailabilityUI("Image catalog", "image.catalog"),
                    GetWebserviceAvailabilityUI("Social", "socialapi"),
                    GetWebserviceAvailabilityUI("Social [Comments]", "comments"),
                    GetWebserviceAvailabilityUI("Social [Inbox]", "inbox"),
                    GetWebserviceAvailabilityUI("Commerce [Sign in]", "commerce"),
                    GetWebserviceAvailabilityUI("Mix", "mix"),
                    GetWebserviceAvailabilityUI("Resources [Firmware]", "resources"),
                    GetWebserviceAvailabilityUI("Statistics", "stats"),
                }
            )
            {
                Subtitle = "TEST AVAILABILITY OF EACH KNOWN WEB SERVICE ON THE NEW HOST.".ToUpper(),
                IsUserEditingEnabled = false,
                PreferredDisplayMode = AbstractDataListPreferredDisplayMode.Grid,
            }
        };
        return optionsUi;
    }

    private static AbstractUIMetadata GetWebserviceAvailabilityUI(string name, string subDomain)
    {
        return new("serviceTestUI_" + subDomain)
        {
            Title = name,
            IconCode = WEBSERVICE_ICON_TESTING
        };
    }

    public override IReadOnlyList<Type>? DependentMods => null;

    public bool IsInitialized { get; private set; }

    public Task InitAsync(CancellationToken token = default)
    {
        if (IsInitialized)
            return Task.CompletedTask;

        _client = new()
        {
            Timeout = TimeSpan.FromSeconds(3)
        };

        AbstractTextBox newHostBox = (AbstractTextBox)OptionsUI![0];
        newHostBox.ValueChanged += OnHostChanged;
        newHostBox.Value = "zunes.me";

        IsInitialized = true;
        return Task.CompletedTask;
    }

    public override async Task<string?> Apply()
    {
        // Verify that ZuneServices.dll exists
        FileInfo zsDllInfo = new(Path.Combine(ZuneInstallDir, "ZuneService.dll"));
        if (!zsDllInfo.Exists)
        {
            return $"The file '{zsDllInfo.FullName}' does not exist.";
        }

        // Make a backup if it doesn't already exist
        FileInfo zsDllBackupInfo = new(Path.Combine(StorageDirectory, "ZuneService.original.dll"));
        if (!zsDllBackupInfo.Exists)
        {
            File.Copy(zsDllInfo.FullName, zsDllBackupInfo.FullName, true);
        }

        try
        {
            // Open the file
            using FileStream zsDll = zsDllInfo.Open(FileMode.Open);
            using BinaryWriter zsDllWriter = new(zsDll);
            using BinaryReader zsDllReader = new(zsDll);

            // Verify that the DLL is from v4.8 (other versions not tested)
            zsDllReader.BaseStream.Position = 0x12C824;
            var versionBytes = zsDllReader.ReadBytes(6);
            if (versionBytes[0] != '4' || versionBytes[2] != '.' || versionBytes[4] != '8')
            {
                return "This mod has not been tested on versions earlier than 4.8.";
            }

            // Get and validate replacement host
            string oldHost = "zune.net";
            string newHost = ((AbstractTextBox)OptionsUI![0]).Value;
            if (newHost.Length != oldHost.Length)
            {
                return $"The new host (\"{newHost}\") must have the same length as \"{oldHost}\".";
            }
            var ping = await new System.Net.NetworkInformation.Ping().SendPingAsync(newHost);
            if (ping.Status != System.Net.NetworkInformation.IPStatus.Success)
            {
                return $"Failed to reach \"{newHost}\". Ping status: {ping.Status}";
            }

            // Patch ZuneServices.dll to use the new host instead of zune.net
            zsDllReader.BaseStream.Position = ZUNESERVICES_ENDPOINTS_BLOCK_OFFSET;
            string endpointBlock = System.Text.Encoding.Unicode.GetString(zsDllReader.ReadBytes(ZUNESERVICES_ENDPOINTS_BLOCK_LENGTH));
            endpointBlock = endpointBlock.Replace("resources." + oldHost, "www.zuneupdate.com");    // Use zuneupdate.com until resources.zunes.me is online
            endpointBlock = endpointBlock.Replace(oldHost, newHost);
            byte[] endpointBytes = System.Text.Encoding.Unicode.GetBytes(endpointBlock);
            if (endpointBytes.Length != ZUNESERVICES_ENDPOINTS_BLOCK_LENGTH)
            {
                return "Failed to safely overwrite strings in DLL.";
            }
            zsDllWriter.Seek(ZUNESERVICES_ENDPOINTS_BLOCK_OFFSET, SeekOrigin.Begin);
            zsDllWriter.Write(endpointBytes);


            // Enable all feature overrides affected by new servers
            bool setOverrideSuccess = true;
            foreach (var feature in _affectedFeatures)
                setOverrideSuccess &= SetFeatureOverride(feature, true);

            if (setOverrideSuccess != true)
                return "Unable to set feature overrides. The mod was successful, but you may not be able to see it in the Zune software.";

            return null;
        }
        catch (IOException)
        {
            return $"Unable to replace '{zsDllInfo.FullName}'. Verify that the Zune software is not running and try again.";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public override async Task<string?> Reset()
    {
        string zsDllPath = Path.Combine(ZuneInstallDir, "ZuneService.dll");
        try
        {
            // Copy backup to application folder
            File.Copy(Path.Combine(StorageDirectory, "ZuneService.original.dll"), zsDllPath, true);

            // Disable all feature overrides affected by new servers
            bool setOverrideSuccess = true;
            foreach (var feature in _affectedFeatures)
                setOverrideSuccess &= SetFeatureOverride(feature, false);

            if (setOverrideSuccess != true)
                return "Unable to reset feature overrides. The mod was successfully removed, but you may still be able to see it in the Zune software.";

            return null;
        }
        catch (IOException)
        {
            return $"Unable to replace '{zsDllPath}'. Verify that the Zune software is not running and try again.";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    async void OnHostChanged(object? sender, string newHost)
    {
        await _cts.CancelAsync();
        _client.CancelPendingRequests();

        _cts = new();

        AbstractDataList list = (AbstractDataList)OptionsUI![1];

        // Reset all statuses
        foreach (AbstractUIMetadata metadata in list.Items)
        {
            metadata.IconCode = WEBSERVICE_ICON_TESTING;
            metadata.Subtitle = WEBSERVICE_SUBTITLE_TESTING;
        }

        // We want to update all the statuses, but we also need to be able
        // to cancel it when the user enters a new host.
        foreach (AbstractUIMetadata metadata in list.Items)
            UpdateStatus(metadata, newHost, _cts.Token);
    }

    private async Task<string?> Ping(string url, CancellationToken token = default)
    {
        try
        {
            var response = await _client.GetAsync(url, token);
            response.EnsureSuccessStatusCode();
            return null;
        }
        catch (HttpRequestException webEx)
        {
            if (webEx.InnerException?.InnerException != null)
                return webEx.InnerException.InnerException.Message;
            return webEx?.Message ?? string.Empty;
        }
        catch (Exception ex)
        {
            return ex?.Message ?? string.Empty;
        }
    }

    private async Task UpdateStatus(AbstractUIMetadata metadata, string newHost, CancellationToken token = default)
    {
        string url = "http://" + (metadata.Id.Split('_')[1] + '.' + newHost).Replace("www.", string.Empty);
        string? pingResult = await Ping(url, token);

        if (pingResult == null)
        {
            metadata.IconCode = WEBSERVICE_ICON_SUCCESS;
            metadata.Subtitle = WEBSERVICE_SUBTITLE_SUCCESS;
        }
        else if (pingResult != string.Empty)
        {
            metadata.IconCode = WEBSERVICE_ICON_UNAVAILABLE;
            metadata.Subtitle = pingResult;
        }
        else
        {
            metadata.IconCode = WEBSERVICE_ICON_UNREACHABLE;
            metadata.Subtitle = WEBSERVICE_SUBTITLE_UNREACHABLE;
        }
    }
}
