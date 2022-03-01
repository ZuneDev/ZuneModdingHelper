using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZuneModCore.Mods;

namespace ZuneModCore
{
    public abstract class Mod
    {
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
                string dir = Path.Combine(ModManager.CoreStorageDir, Id);
                // Create the directory just in case the consumer assumes the folder exists already
                Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public abstract IReadOnlyList<ModDependency>? DependentMods { get; }

        /// <summary>
        /// Initializes any missing depedencies.
        /// </summary>
        public Task InitDependencies() => ForAllDependencies(async mod =>
        {
            bool isDepApplied = await mod.CheckApplied();
            if (!isDepApplied)
                await mod.Init();
        });

        /// <summary>
        /// Applies any missing depedencies.
        /// </summary>
        /// <remarks>
        /// Make sure to call <see cref="InitDependencies"/> first.
        /// </remarks>
        public Task ApplyDependencies() => ForAllDependencies(async mod =>
        {
            bool isDepApplied = await mod.CheckApplied();
            if (!isDepApplied)
                await mod.Apply();
        });

        public Task<bool> CheckApplied() => TrueForAllDependencies(mod => mod.CheckApplied());

        private async Task ForAllDependencies(Func<Mod, Task> asyncAction)
        {
            // Check if there are any dependencies
            if (DependentMods == null)
                return;

            foreach (var dep in DependentMods)
            {
                var depMod = ModManager.AvailableMods.First(m => m.Id == dep.Id);
                if (depMod != null)
                    await asyncAction(depMod);
            }
        }

        private async Task<bool> TrueForAllDependencies(Func<Mod, Task<bool>> asyncAction)
        {
            // Check if there are any dependencies
            if (DependentMods == null)
                return true;

            foreach (var dep in DependentMods)
            {
                var depMod = ModManager.AvailableMods.First(m => m.Id == dep.Id);
                if (depMod != null)
                    if (!await asyncAction(depMod))
                        return false;
            }

            return true;
        }

        public override string ToString() => Title;
    }
}
