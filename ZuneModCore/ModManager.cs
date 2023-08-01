using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
            new VideoSyncMod(),
#if DEBUG
            new Win11DriverMod(),
#endif
            new FeaturesOverrideMod(),
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
        internal static void MarkApplied(string modId) => MarkStatus(modId, true);

        /// <summary>
        /// Marks the specified mod as reset in the status file.
        /// </summary>
        internal static void MarkReset(string modId) => MarkStatus(modId, false);

        /// <summary>
        /// Sets the status of the specified mod in the status file.
        /// </summary>
        private static void MarkStatus(string modId, bool status)
        {
            var model = GetOrCreateStatusModel();

            bool curStatus = model.InstalledMods.ContainsKey(modId);
            if (curStatus && !status)
            {
                model.InstalledMods.Remove(modId);
            }
            else if (!curStatus && status)
            {
                model.InstalledMods.Add(modId, CurrentVersion);
            }

            string json = JsonSerializer.Serialize(model);
            File.WriteAllText(CoreStatusFile, json);
        }

        /// <summary>
        /// Gets the status of the specified mod from the status file.
        /// </summary>
        internal static bool CheckStatus(string modId)
        {
            var model = GetOrCreateStatusModel();
            return model.InstalledMods.ContainsKey(modId);
        }

        private static StatusModel GetOrCreateStatusModel()
        {
            StatusModel? model = null;
            if (File.Exists(CoreStatusFile))
            {
                string json = File.ReadAllText(CoreStatusFile);
                if (!string.IsNullOrWhiteSpace(json))
                    model = JsonSerializer.Deserialize<StatusModel>(json);
            }

            model ??= new()
            {
                Version = CurrentVersion,
                InstalledMods = new()
            };

            Guard.IsNotNull(model, nameof(model));
            return model;
        }
    }
}
