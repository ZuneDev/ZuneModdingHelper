using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ZuneModCore.Mods
{
    public class VideoSyncMod : Mod
    {
        public override string Title => "Fix Video Sync";

        public override string Description =>
            "Resolves \"Error C00D11CD\" when attempting to sync video to a Zune device using Windows 10 1607 (Anniversary Update) or newer";

        public override string Id => nameof(VideoSyncMod);

        public override AbstractUIElementGroup? OptionsUI => null;

        public override Task<string?> Apply()
        {
            var writer = new StreamWriter(OpenFileInStorageDirectory("1.log"));
            writer.WriteLine("Hello world");
            writer.Flush();
            writer.Close();

            return Task.FromResult<string?>(null);
        }

        public override Task<string?> Reset()
        {
            return Task.FromResult<string?>(null);
        }

        public override IReadOnlyList<Type>? DependentMods => null;
    }
}
