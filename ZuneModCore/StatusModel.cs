using System.Collections.Generic;

namespace ZuneModCore
{
    internal class StatusModel
    {
        /// <summary>
        /// The version of ZuneModCore that created this model.
        /// </summary>
        public ReleaseVersion Version { get; set; }

        /// <summary>
        /// A list of mods that are currently installed.
        /// <para>
        /// <c>Key</c> is mod ID and <c>Value</c> is installed version.
        /// </para>
        /// </summary>
        public Dictionary<string, ReleaseVersion> InstalledMods { get; set; }
    }
}
