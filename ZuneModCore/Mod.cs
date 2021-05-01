using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using ZuneModCore.Mods;

namespace ZuneModCore
{
    public abstract class Mod
    {
        /// <summary>
        /// A list of all available mods
        /// </summary>
        public static readonly ReadOnlyCollection<Mod> AvailableMods = new List<Mod>
        {
            new FeaturesOverrideMod(),
            new VideoSyncMod(),
            new WebservicesMod(),
        }.AsReadOnly();

        public static string ZuneInstallDir { get; set; } = @"C:\Program Files\Zune\";

        public abstract string Id { get; }

        public abstract string Title { get; }

        public abstract string Description { get; }

        public abstract string Author { get; }

        public virtual void Init() { }

        public abstract string? Apply();

        public abstract string? Reset();

        public string StorageDirectory
        {
            get
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZuneModCore", Id);
                // Create the directory just in case the consumer assumes the folder exists already
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public abstract ReadOnlyCollection<Type>? DependentMods { get; }
    }
}
