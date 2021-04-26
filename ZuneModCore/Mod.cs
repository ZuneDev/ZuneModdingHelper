﻿using OwlCore.AbstractUI.Models;
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

        public abstract Task<string?> Apply();

        public abstract Task<string?> Reset();

        public abstract AbstractUIElementGroup? OptionsUI { get; }

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

        public abstract IReadOnlyList<Type>? DependentMods { get; }

        internal FileStream OpenFileInStorageDirectory(string filename)
        {
            FileInfo info = new(Path.Combine(StorageDirectory, filename));
            return File.Create(info.FullName);
        }
    }
}
