using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static ZuneModCore.Mods.FeaturesOverrideMod;

namespace ZuneModCore.Mods
{
    public class WebservicesMod : Mod
    {
        private const int ZUNESERVICES_ENDPOINTS_BLOCK_OFFSET = 0x14D60;
        private const int ZUNESERVICES_ENDPOINTS_BLOCK_LENGTH = 0x884;

        public override string Id => nameof(WebservicesMod);

        public override string Title => "Community Webservices";

        public override string Description => "Partially restores online features such as the Marketplace by patching the Zune desktop software " +
            "to use the community's recreation of Microsoft's Zune servers at zunes.me (instead of zune.net).";

        public override string Author => "Joshua \"Yoshi\" Askharoun";

        public override AbstractUIElementGroup? GetDefaultOptionsUI()
        {
            return new(nameof(FeaturesOverrideMod))
            {
                Title = string.Empty,
                Items =
                {
                    new AbstractTextBox("hostBox", "zunes.me", "zune.net")
                    {
                        Title = "Host",
                        TooltipText = "The host where the replacement servers are located. Must be the same length as \"zune.net\"."
                    }
                }
            };
        }

        public override IReadOnlyList<Type>? DependentMods => null;

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
            if (zsDllBackupInfo.Exists)
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
                string newHost = ((AbstractTextBox)OptionsUI!.Items[0]).Value;
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
                setOverrideSuccess &= SetFeatureOverride("Apps", true);
                setOverrideSuccess &= SetFeatureOverride("Channels", true);
                setOverrideSuccess &= SetFeatureOverride("Games", true);
                setOverrideSuccess &= SetFeatureOverride("Marketplace", true);
                setOverrideSuccess &= SetFeatureOverride("MBRPreview", true);
                setOverrideSuccess &= SetFeatureOverride("MBRPurchase", true);
                setOverrideSuccess &= SetFeatureOverride("MBRRental", true);
                setOverrideSuccess &= SetFeatureOverride("Music", true);
                setOverrideSuccess &= SetFeatureOverride("MusicVideos", true);
                setOverrideSuccess &= SetFeatureOverride("Picks", true);
                setOverrideSuccess &= SetFeatureOverride("Podcasts", true);
                setOverrideSuccess &= SetFeatureOverride("Sign In Available", true);
                setOverrideSuccess &= SetFeatureOverride("Social", true);
                setOverrideSuccess &= SetFeatureOverride("SocialMarketplace", true);
                setOverrideSuccess &= SetFeatureOverride("SubscriptionFreeTracks", true);
                setOverrideSuccess &= SetFeatureOverride("Videos", true);
                if (setOverrideSuccess != true)
                {
                    return "Unable to set feature overrides. The mod was successful, but you may not be able to see it in the Zune Software.";
                }

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
                setOverrideSuccess &= SetFeatureOverride("Apps", false);
                setOverrideSuccess &= SetFeatureOverride("Channels", false);
                setOverrideSuccess &= SetFeatureOverride("Games", false);
                setOverrideSuccess &= SetFeatureOverride("Marketplace", false);
                setOverrideSuccess &= SetFeatureOverride("MBRPreview", false);
                setOverrideSuccess &= SetFeatureOverride("MBRPurchase", false);
                setOverrideSuccess &= SetFeatureOverride("MBRRental", false);
                setOverrideSuccess &= SetFeatureOverride("Music", false);
                setOverrideSuccess &= SetFeatureOverride("MusicVideos", false);
                setOverrideSuccess &= SetFeatureOverride("Picks", false);
                setOverrideSuccess &= SetFeatureOverride("Podcasts", false);
                setOverrideSuccess &= SetFeatureOverride("Sign In Available", false);
                setOverrideSuccess &= SetFeatureOverride("Social", false);
                setOverrideSuccess &= SetFeatureOverride("SocialMarketplace", false);
                setOverrideSuccess &= SetFeatureOverride("SubscriptionFreeTracks", false);
                setOverrideSuccess &= SetFeatureOverride("Videos", false);
                if (setOverrideSuccess != true)
                {
                    return "Unable to reset feature overrides. The mod was successfully removed, but you may still be able to see it in the Zune Software.";
                }

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
    }
}
