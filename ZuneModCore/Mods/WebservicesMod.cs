using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static ZuneModCore.Mods.FeaturesOverrideMod;

#if DEBUG
using System.Diagnostics;
#endif

namespace ZuneModCore.Mods
{
    public class WebservicesMod : Mod
    {
        private readonly byte[] ZUNE_4_8_VERSION_BYTES =
        {
            0x34, 0x2E, 0x38
        };
        private const int ZUNESERVICES_ENDPOINTS_BLOCK_OFFSET = 0x14D60;
        private const int ZUNESERVICES_ENDPOINTS_BLOCK_LENGTH = 0x884;

        public override string Id => nameof(WebservicesMod);

        public override string Title => "Community Webservices";

        public override string Description => "Partially restores online features such as the Marketplace by patching the Zune desktop software " +
            "to use the community's recreation of Microsoft's Zune servers at zunes.tk (instead of zune.net).";

        public override AbstractUIElementGroup? OptionsUI => null;

        public override IReadOnlyList<Type>? DependentMods => null;

        public override async Task<bool> Apply()
        {
            // Open ZuneServices.dll
            FileInfo zsDllInfo = new(Path.Combine(ZuneInstallDir, "ZuneService.dll"));
            if (!zsDllInfo.Exists)
                return false;
            using FileStream zsDll = zsDllInfo.Open(FileMode.Open);
            using BinaryWriter zsDllWriter = new(zsDll);
            using BinaryReader zsDllReader = new(zsDll);

            // Verify that the DLL is from v4.8 (other versions not tested)
            zsDllReader.BaseStream.Position = 0x12C824;
            var versionBytes = zsDllReader.ReadBytes(ZUNE_4_8_VERSION_BYTES.Length * 2);
            if (versionBytes[0] != '4' || versionBytes[2] != '.' || versionBytes[4] != '8')
                return false;

            // Patch ZuneServices.dll to use zunes.tk instead of zune.net
            zsDllReader.BaseStream.Position = ZUNESERVICES_ENDPOINTS_BLOCK_OFFSET;
            string endpointBlock = System.Text.Encoding.Unicode.GetString(zsDllReader.ReadBytes(ZUNESERVICES_ENDPOINTS_BLOCK_LENGTH));
            endpointBlock = endpointBlock.Replace("zune.net", "zunes.tk");
            byte[] endpointBytes = System.Text.Encoding.Unicode.GetBytes(endpointBlock);
            if (endpointBytes.Length != ZUNESERVICES_ENDPOINTS_BLOCK_LENGTH)
                return false;
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

            return true;
        }

        public override Task<bool> Reset()
        {
            throw new NotImplementedException();
        }
    }
}
