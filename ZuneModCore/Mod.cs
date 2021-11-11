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
            new BackgroundImageMod(),
            new MbidLocatorMod(),
        }.AsReadOnly();

        public static string ZuneInstallDir { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Zune");

        public abstract string Id { get; }

        public abstract string Title { get; }

        public abstract string Description { get; }

        public abstract string Author { get; }

        public virtual AbstractUICollection? GetDefaultOptionsUI() => null;

        public virtual Task Init() => Task.CompletedTask;

        public abstract Task<string?> Apply();

        public abstract Task<string?> Reset();

        private AbstractUICollection? _OptionsUI;
        public AbstractUICollection? OptionsUI
        {
            get
            {
                if (_OptionsUI == null)
                    _OptionsUI = GetDefaultOptionsUI();
                return _OptionsUI;
            }
            set => _OptionsUI = value;
        }

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
    }
}
