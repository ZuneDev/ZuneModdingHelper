using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ZuneModCore.Mods
{
    public class VideoSyncMod : Mod
    {
        private const string WMVCORE_PATH = @"C:\Windows\System32\WMVCORE.dll";

        public override string Title => "Fix Video Sync";

        public override string Description =>
            "Resolves \"Error C00D11CD\" when attempting to sync video to a Zune device using Windows 10 1607 (Anniversary Update) or newer";

        public override string Id => nameof(VideoSyncMod);

        public override AbstractUIElementGroup? OptionsUI => null;

        public override Task<string?> Apply()
        {
            try
            {
                // Make a backup of the file
                File.Copy(WMVCORE_PATH, Path.Combine(StorageDirectory, "WMVCORE.original.dll"), true);

                // Copy the pre-Anniversary Update WMVCORE.dll
                File.Copy("Resources\\WMVCORE.dll", WMVCORE_PATH, true);

                return Task.FromResult<string?>(null);
            }
            catch (Exception ex)
            {
                return Task.FromResult(ex.Message)!;
            }
        }

        public override Task<string?> Reset()
        {
            // Copy backup to application folder
            File.Copy(Path.Combine(StorageDirectory, "WMVCORE.original.dll"), WMVCORE_PATH, true);

            return Task.FromResult<string?>(null);
        }

        public override IReadOnlyList<Type>? DependentMods => null;
    }
}
