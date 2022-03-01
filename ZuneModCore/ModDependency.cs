using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ZuneModCore
{
    public class ModDependency
    {
        public ModDependency(string id, ReleaseVersion? version = null)
        {
            Id = id;
            Version = version ?? ModManager.CurrentVersion;
        }

        public string Id { get; set; }

        public ReleaseVersion Version { get; set; }

        public bool Status => CheckStatus();

        /// <summary>
        /// DO NOT USE DIRECTLY. This constructor is only for
        /// JSON de/serialization.
        /// </summary>
        [Obsolete]
        internal ModDependency() { }

        public Task<bool> CheckStatusAsync() => ModManager.CheckStatus(Id);

        private bool CheckStatus() => CheckStatusAsync().Result;

        public override string ToString() => $"{Id} / {Version}";
    }
}
