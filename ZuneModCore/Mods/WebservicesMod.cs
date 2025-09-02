using OwlCore.AbstractUI.Models;
using OwlCore.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static ZuneModCore.Mods.FeaturesOverrideMod;

namespace ZuneModCore.Mods;

public class WebservicesModFactory : DIModFactoryBase<WebservicesMod>
{

    private const string Description = "Partially restores online features such as the Marketplace by patching the Zune desktop software " +
        "to use the community's recreation of Microsoft's Zune servers at zunes.me (instead of zune.net).";

    private const string Author = "Joshua \"Yoshi\" Askharoun";

    public override ModMetadata Metadata => new(nameof(WebservicesMod), "Community Webservices",
        Description, Author, new(1, 1));
}

public partial class WebservicesMod(ModMetadata metadata) : Mod(metadata), IAsyncInit
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

    private static readonly (string description, string subdomain)[] _knownSubdomains =
    [
        ("Main website", "www"),
        ("Social website", "social"),
        ("Catalog", "catalog"),
        ("Image catalog", "image.catalog"),
        ("Social", "socialapi"),
        ("Social [Comments]", "comments"),
        ("Social [Inbox]", "inbox"),
        ("Commerce [Sign in]", "commerce"),
        ("Mix", "mix"),
        ("Resources [Firmware]", "resources"),
        ("Statistics", "stats"),
    ];

    private HttpClient _client;
    private CancellationTokenSource _cts = new();
    private string _methodFile;

    public override AbstractUICollection? GetDefaultOptionsUI()
    {
        AbstractUICollection optionsUi = new(nameof(FeaturesOverrideMod))
        {
            new AbstractTextBox("hostBox", string.Empty, "zune.net")
            {
                Title = "host",
                TooltipText = "The host where the replacement servers are located. Must be the same length as \"zune.net\"."
            },
            new AbstractDataList("hostsTest",
                _knownSubdomains.Select(e => GetWebserviceAvailabilityUI(e.description, e.subdomain)))
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

        _methodFile = Path.Combine(StorageDirectory, "AppliedMethod.txt");

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
        var newHost = ((AbstractTextBox)OptionsUI![0]).Value;
        var errorMessage = "hi";// await PatchZuneServices(newHost);

        if (errorMessage is not null)
        {
            // Fallback to editing hosts file
            errorMessage = await AddEntriesToHostsFile(newHost);

            if (errorMessage is null)
                await RecordAppliedMethod(ApplicationMethod.HostsEntries);
        }
        else
        {
            await RecordAppliedMethod(ApplicationMethod.BinaryPatch);
        }

        return errorMessage;
    }

    public override async Task<string?> Reset()
    {
        string zsDllPath = Path.Combine(ZuneInstallDir, "ZuneService.dll");

        try
        {
            var method = await ReadAppliedMethod();
            switch (method)
            {
                case ApplicationMethod.BinaryPatch:
                    // Restore backup to application folder
                    File.Copy(Path.Combine(StorageDirectory, "ZuneService.original.dll"), zsDllPath, true);
                    break;

                case ApplicationMethod.HostsEntries:
                    // TODO: Remove entries from hosts file
                    return $"Automatic removal of hosts file entries is not yet supported. Please manually remove any entries for zune.net and its subdomains from your hosts file.";
                    break;
            }

            // Clear application method file
            try
            {
                File.Delete(_methodFile);
            }
            catch (FileNotFoundException) { }

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

    private async Task<string?> PatchZuneServices(string newHost)
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
            // Get and validate replacement host
            var oldHost = "zune.net";
            if (newHost.Length != oldHost.Length)
            {
                return $"The new host \"{newHost}\" must have the same length as \"{oldHost}\".";
            }

            var versionInfo = FileVersionInfo.GetVersionInfo(zsDllInfo.FullName);
            if (versionInfo?.FileVersion is null)
            {
                return $"Failed to get version info for '{zsDllInfo.FullName}'.";
            }

            var fileVersion = new Version(versionInfo.ProductMajorPart, versionInfo.ProductMinorPart,
                versionInfo.ProductBuildPart, versionInfo.FilePrivatePart);

            // Open the file and determine the architecture
            using FileStream zsDll = zsDllInfo.OpenRead();

            Machine architecture;
            using (PEReader peReader = new(zsDll, PEStreamOptions.LeaveOpen | PEStreamOptions.PrefetchMetadata))
                architecture = peReader.PEHeaders.CoffHeader.Machine;

            // Select appropriate patch based on version and architecture
            if (fileVersion == new Version(4, 8, 2345, 0) && architecture is Machine.Amd64)
            {
                var patchResult = Patch48Version64Bit(zsDll, oldHost, newHost);
                if (patchResult is not null)
                    return patchResult;
            }
            else
            {
                return $"This mod does not support Zune {fileVersion} on {architecture}.";
            }

            await zsDll.DisposeAsync();

            // Enable all feature overrides affected by new servers
            var setOverrideSuccess = true;
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

    private static string? Patch48Version64Bit(Stream zsDll, string oldHost, string newHost)
    {
        using BinaryWriter zsDllWriter = new(zsDll);
        using BinaryReader zsDllReader = new(zsDll);

        // Read URL block as string
        zsDllReader.BaseStream.Position = ZUNESERVICES_ENDPOINTS_BLOCK_OFFSET;
        var originalEndpointBytes = zsDllReader.ReadBytes(ZUNESERVICES_ENDPOINTS_BLOCK_LENGTH);
        var endpointText = Encoding.Unicode.GetString(originalEndpointBytes);

        // Try to determine previous host
        try
        {
            var firstUrl = endpointText[..endpointText.IndexOf('\0')];
            oldHost = firstUrl.Substring(11, oldHost.Length);
        }
        catch { }

        // Patch ZuneServices.dll to use the new host instead of zune.net
        endpointText = endpointText.Replace(oldHost, newHost);
        var patchedEndpointBytes = Encoding.Unicode.GetBytes(endpointText);
        if (patchedEndpointBytes.Length != originalEndpointBytes.Length)
        {
            return "Failed to safely overwrite strings in DLL.";
        }
        
        zsDllWriter.Seek(ZUNESERVICES_ENDPOINTS_BLOCK_OFFSET, SeekOrigin.Begin);
        zsDllWriter.Write(patchedEndpointBytes);

        return null;
    }

    private static async Task<string?> AddEntriesToHostsFile(string newHost)
    {
        var hostEntry = await Dns.GetHostEntryAsync(newHost);
        var ipAddress = hostEntry?.AddressList?.FirstOrDefault();
        if (ipAddress is null)
        {
            return $"Failed to resolve host '{newHost}' to an IP address.";
        }

        var hostsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");
        var hostLines = (await File.ReadAllLinesAsync(hostsPath)).ToList();

        var domains = _knownSubdomains
            .Select(e => GetDomainName(e.subdomain, "zune.net"))
            .ToHashSet();

        // Update existing entries
        Regex rx = HostsEntryRegex();
        for (int l = 0; l < hostLines.Count; l++)
        {
            var line = hostLines[l].Trim();

            // Ignore comments and empty lines
            if (line.Length <= 0 || line[0] is '#')
                continue;

            var match = rx.Match(line);
            if (!match.Success)
                continue;

            var domain = match.Groups["name"].Value;

            // Ignore entries unrelated to Zune
            if (!domain.EndsWith("zune.net"))
                continue;

            // Reconstruct entry with new IP address
            hostLines[l] = $"{ipAddress} {domain}";

            // Track that we handled this domain
            domains.Remove(domain);
        }

        // Add any entries that didn't already exist
        if (domains.Count > 0)
        {
            foreach (var domain in domains)
                hostLines.Add($"{ipAddress} {domain}");
        }

        await File.WriteAllLinesAsync(hostsPath, hostLines);

        return null;
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
        string url = "http://" + GetDomainName(metadata.Id.Split('_')[1], newHost);
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

    private static string GetDomainName(string subdomain, string host)
    {
        return $"{subdomain}.{host}".Replace("www.", string.Empty);
    }

    private async Task RecordAppliedMethod(ApplicationMethod method)
    {
        await File.WriteAllTextAsync(_methodFile, method.ToString());
    }

    private async Task<ApplicationMethod> ReadAppliedMethod()
    {
        if (!File.Exists(_methodFile))
            return ApplicationMethod.None;

        var methodText = await File.ReadAllTextAsync(_methodFile);
        return Enum.Parse<ApplicationMethod>(methodText);
    }

    [GeneratedRegex(@"(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\s+(?<name>[\w.-]+?)(\s+|$)")]
    private static partial Regex HostsEntryRegex();

    private enum ApplicationMethod { None, BinaryPatch, HostsEntries}
}
