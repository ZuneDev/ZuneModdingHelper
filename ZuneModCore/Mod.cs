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

        public delegate void StatusChangedHandler(Mod sender, bool status);
        public event StatusChangedHandler? StatusChanged;

        public virtual AbstractUICollection? GetDefaultOptionsUI() => null;

        public virtual Task Init() => Task.CompletedTask;

        protected abstract Task<string?> ApplyCore();

        protected abstract Task<string?> ResetCore();

        protected virtual void FireStatusChanged(bool status) => StatusChanged?.Invoke(this, status);

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

        /// <summary>
        /// Gets a directory that mods can use to store mod-specific information,
        /// such as backups for resetting.
        /// </summary>
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
        /// Applies this mod, without any of its dependencies.
        /// </summary>
        /// <returns>
        /// The error message if not successful.
        /// </returns>
        public async Task<string?> Apply()
        {
            string? error = await ApplyCore();
            if (error == null)
            {
                ModManager.MarkApplied(Id);
                FireStatusChanged(true);
            }
            return error;
        }

        /// <summary>
        /// Initializes any missing depedencies.
        /// </summary>
        public Task InitDependencies() => ForAllDependenciesAsync(async mod =>
        {
            bool isDepApplied = mod.CheckApplied();
            if (!isDepApplied)
                await mod.Init();
        });

        /// <summary>
        /// Applies any missing depedencies.
        /// </summary>
        /// <remarks>
        /// Make sure to call <see cref="InitDependencies"/> first.
        /// </remarks>
        public Task ApplyDependencies() => ForAllDependenciesAsync(async mod =>
        {
            bool isDepApplied = mod.CheckApplied();
            if (!isDepApplied)
                await mod.Apply();
        });

        /// <summary>
        /// Applies this mod and all its dependencies.
        /// </summary>
        /// <returns>
        /// The error message if not successful.
        /// </returns>
        public async Task<string?> ApplyWithDependencies()
        {
            await ApplyDependencies();
            return await Apply();
        }

        /// <summary>
        /// Resets this mod, without resetting any dependencies.
        /// </summary>
        public async Task<string?> Reset()
        {
            string? error = await ResetCore();
            if (error == null)
            {
                ModManager.MarkReset(Id);
                FireStatusChanged(false);
            }
            return error;
        }

        public bool CheckApplied() => ModManager.CheckStatus(Id);

        public bool CheckDependenciesApplied() => TrueForAllDependencies(mod => mod.CheckApplied());

        private void ForAllDependencies(Action<Mod> action)
        {
            // Check if there are any dependencies
            if (DependentMods == null)
                return;

            foreach (var dep in DependentMods)
            {
                var depMod = ModManager.AvailableMods.First(m => m.Id == dep.Id);
                if (depMod != null)
                    action(depMod);
            }
        }

        private async Task ForAllDependenciesAsync(Func<Mod, Task> asyncAction)
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

        private bool TrueForAllDependencies(Func<Mod, bool> action)
        {
            // Check if there are any dependencies
            if (DependentMods == null)
                return true;

            foreach (var dep in DependentMods)
            {
                var depMod = ModManager.AvailableMods.First(m => m.Id == dep.Id);
                if (depMod != null)
                    if (!action(depMod))
                        return false;
            }

            return true;
        }

        public override string ToString() => Title;
    }
}
