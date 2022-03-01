using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ZuneModCore
{
    public class ModDependency
    {
        public string Id { get; set; }

        public ReleaseVersion Version { get; set; }

        public ModDependency(string id, ReleaseVersion? version = null)
        {
            Id = id;
            Version = version ?? ModManager.CurrentVersion;
        }

        /// <summary>
        /// DO NOT USE DIRECTLY. This constructor is only for
        /// JSON de/serialization.
        /// </summary>
        [Obsolete]
        internal ModDependency() { }

        public Task<bool> CheckStatus() => ModManager.CheckStatus(Id);

        public override string ToString() => $"{Id} / {Version}";
    }
}
