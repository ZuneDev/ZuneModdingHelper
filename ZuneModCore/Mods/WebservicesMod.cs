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
            "to use the community's recreation of Microsoft's Zune servers at zunes.tk (instead of zune.net).";

        public override string Author => "Joshua \"Yoshi\" Askharoun";

        public override AbstractUIElementGroup? OptionsUI => null;

        public override IReadOnlyList<Type>? DependentMods => null;

        public override Task<string?> Apply()
        {
            // Verify that ZuneServices.dll exists
            FileInfo zsDllInfo = new(Path.Combine(ZuneInstallDir, "ZuneService.dll"));
            if (!zsDllInfo.Exists)
            {
                return Task.FromResult<string?>($"The file '{zsDllInfo.FullName}' does not exist.");
            }

            // Make a backup of the file
            File.Copy(zsDllInfo.FullName, Path.Combine(StorageDirectory, "ZuneService.original.dll"), true);

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
                    return Task.FromResult<string?>("This mod has not been tested on versions earlier than 4.8.");
                }

                // Patch ZuneServices.dll to use zunes.tk instead of zune.net
                zsDllReader.BaseStream.Position = ZUNESERVICES_ENDPOINTS_BLOCK_OFFSET;
                string endpointBlock = System.Text.Encoding.Unicode.GetString(zsDllReader.ReadBytes(ZUNESERVICES_ENDPOINTS_BLOCK_LENGTH));
                endpointBlock = endpointBlock.Replace("zune.net", "zunes.tk");
                byte[] endpointBytes = System.Text.Encoding.Unicode.GetBytes(endpointBlock);
                if (endpointBytes.Length != ZUNESERVICES_ENDPOINTS_BLOCK_LENGTH)
                {
                    return Task.FromResult<string?>("Failed to safely overwrite strings in DLL.");
                }
                zsDllWriter.Seek(ZUNESERVICES_ENDPOINTS_BLOCK_OFFSET, SeekOrigin.Begin);
                zsDllWriter.Write(endpointBytes);


                // Enable all feature overrides affected by new servers
                SetFeatureOverride("Apps", true);
                SetFeatureOverride("Channels", true);
                SetFeatureOverride("Games", true);
                SetFeatureOverride("Marketplace", true);
                SetFeatureOverride("Music", true);
                SetFeatureOverride("MusicVideos", true);
                SetFeatureOverride("Podcasts", true);
                SetFeatureOverride("Social", true);
                SetFeatureOverride("Videos", true);

                return Task.FromResult<string?>(null);
            }
            catch (IOException)
            {
                return Task.FromResult<string?>($"Unable to replace '{zsDllInfo.FullName}'. Verify that the Zune software is not running and try again.");
            }
            catch (Exception ex)
            {
                return Task.FromResult<string?>(ex.Message);
            }
        }

        public override Task<string?> Reset()
        {
            string zsDllPath = Path.Combine(ZuneInstallDir, "ZuneService.dll");
            try
            {
                // Copy backup to application folder
                File.Copy(Path.Combine(StorageDirectory, "ZuneService.original.dll"), zsDllPath, true);

                // Disable all feature overrides affected by new servers
                SetFeatureOverride("Apps", false);
                SetFeatureOverride("Channels", false);
                SetFeatureOverride("Games", false);
                SetFeatureOverride("Marketplace", false);
                SetFeatureOverride("Music", false);
                SetFeatureOverride("MusicVideos", false);
                SetFeatureOverride("Podcasts", false);
                SetFeatureOverride("Social", false);
                SetFeatureOverride("Videos", false);

                return Task.FromResult<string?>(null);
            }
            catch (IOException)
            {
                return Task.FromResult<string?>($"Unable to replace '{zsDllPath}'. Verify that the Zune software is not running and try again.");
            }
            catch (Exception ex)
            {
                return Task.FromResult<string?>(ex.Message);
            }
        }
    }
}
