using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ZuneModCore.Mods;

namespace ZuneModCore
{
    public static class ModManager
    {
        /// <summary>
        /// A list of all available mods.
        /// </summary>
        public static readonly IReadOnlyList<Mod> AvailableMods = new List<Mod>
        {
            new FeaturesOverrideMod(),
            new VideoSyncMod(),
            new WebservicesMod(),
            new BackgroundImageMod(),
            new MbidLocatorMod(),
        };

        public static string ZuneInstallDir { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Zune");

        public static readonly ReleaseVersion CurrentVersion = new(2021, 12, 30, 0, Phase.Alpha);

        internal static readonly string CoreStorageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZuneModCore");

        private static readonly string CoreStatusFile = Path.Combine(CoreStorageDir, "ModStatus.json");

        /// <summary>
        /// Marks the specified mod as applied in the status file.
        /// </summary>
        internal static Task MarkApplied(string modId) => MarkStatus(modId, true);

        /// <summary>
        /// Marks the specified mod as reset in the status file.
        /// </summary>
        internal static Task MarkReset(string modId) => MarkStatus(modId, false);

        /// <summary>
        /// Sets the status of the specified mod in the status file.
        /// </summary>
        private static async Task MarkStatus(string modId, bool status)
        {
            using var stream = File.Open(CoreStatusFile, FileMode.OpenOrCreate);
            var model = await JsonSerializer.DeserializeAsync<StatusModel>(stream);
            Guard.IsNotNull(model, nameof(model));

            bool curStatus = model.InstalledMods.ContainsKey(modId);
            if (curStatus && !status)
            {
                model.InstalledMods.Remove(modId);
            }
            else if (!curStatus && status)
            {
                model.InstalledMods.Add(modId, CurrentVersion);
            }

            await JsonSerializer.SerializeAsync(stream, model);
        }

        /// <summary>
        /// Gets the status of the specified mod from the status file.
        /// </summary>
        internal static async Task<bool> CheckStatus(string modId)
        {
            if (!File.Exists(CoreStatusFile))
                return false;

            using var stream = File.OpenRead(CoreStatusFile);
            var model = await JsonSerializer.DeserializeAsync<StatusModel>(stream);
            Guard.IsNotNull(model, nameof(model));

            return model.InstalledMods.ContainsKey(modId);
        }
    }
}
