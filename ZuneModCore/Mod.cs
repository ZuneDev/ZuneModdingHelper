using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ZuneModCore.Mods;

namespace ZuneModCore
{
    public abstract class Mod
    {
        /// <summary>
        /// A list of all available mods
        /// </summary>
        public static readonly IReadOnlyList<Mod> AvailableMods = new List<Mod>
        {
            new FeaturesOverrideMod(),
            new VideoSyncMod(),
            new WebservicesMod(),
        }.AsReadOnly();

        public static string ZuneInstallDir { get; set; } = @"C:\Program Files\Zune\";

        public abstract string Id { get; }

        public abstract string Title { get; }

        public abstract string Description { get; }

        public virtual Task Init() => Task.CompletedTask;

        public abstract Task<bool> Apply();

        public abstract Task<bool> Reset();

        public abstract AbstractUIElementGroup? OptionsUI { get; }

        public string StorageDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ZuneModCore", Id);

        public abstract IReadOnlyList<Type>? DependentMods { get; }
    }
}
