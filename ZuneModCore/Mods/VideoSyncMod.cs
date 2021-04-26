using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ZuneModCore.Mods
{
    public class VideoSyncMod : Mod
    {
        public VideoSyncMod()
        {
            testButton.Clicked += TestButton_Clicked;
        }

        private void TestButton_Clicked(object? sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Howdy, Zune! How are ya?");
        }

        public override string Title => "Fix Video Sync";

        public override string Description =>
            "Resolves \"Error C00D11CD\" when attempting to sync video to a Zune device using Windows 10 1607 (Anniversary Update) or newer";

        public override string Id => nameof(VideoSyncMod);

        public override AbstractUIElementGroup OptionsUI => new(nameof(VideoSyncMod))
        {
            Title = Title,
            Items =
            {
                new AbstractBooleanUIElement("test", "Is this is test?"),
                testButton
            }
        };

        private readonly AbstractButton testButton = new(nameof(testButton), "Howdy Zune!", type: AbstractButtonType.Generic);

        public override Task<bool> Apply()
        {
            var writer = File.CreateText(Path.Combine(StorageDirectory, "1.log"));
            writer.WriteLine("Hello world");
            writer.Flush();
            writer.Close();
            return Task.FromResult(true);
        }

        public override Task<bool> Reset()
        {
            return Task.FromResult(true);
        }

        public override IReadOnlyList<Type>? DependentMods => null;
    }
}
