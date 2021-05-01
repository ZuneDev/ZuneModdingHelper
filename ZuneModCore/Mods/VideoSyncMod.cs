﻿using System;
using System.Collections.ObjectModel;
using System.IO;

namespace ZuneModCore.Mods
{
    public class VideoSyncMod : Mod
    {
        private const string WMVCORE_PATH = @"C:\Windows\System32\WMVCORE.dll";

        public override string Title => "Fix Video Sync";

        public override string Description =>
            "Resolves \"Error C00D11CD\" when attempting to sync video to a Zune device using Windows 10 1607 (Anniversary Update) or newer";

        public override string Author => "ส็็็Codix#4833";

        public override string Id => nameof(VideoSyncMod);

        public override string? Apply()
        {
            try
            {
                // Make a backup of the file
                File.Copy(WMVCORE_PATH, Path.Combine(StorageDirectory, "WMVCORE.original.dll"), true);

                // NOTE: This is quite dangerous to do blindly, so let's not
                // Kill processes that are using WMVCORE.dll
                //foreach (System.Diagnostics.Process proc in FileUtil.WhoIsLocking(WMVCORE_PATH))
                //{
                //    proc.Kill();
                //}

                // Copy the pre-Anniversary Update WMVCORE.dll
                File.Copy("Resources\\WMVCORE.dll", WMVCORE_PATH, true);

                return null;
            }
            catch (Exception ex)
            {
                return ex.Message!;
            }
        }

        public override string? Reset()
        {
            try
            {
                // Copy backup to application folder
                File.Copy(Path.Combine(StorageDirectory, "WMVCORE.original.dll"), WMVCORE_PATH, true);

                return null;
            }
            catch (Exception ex)
            {
                return ex.Message!;
            }
        }

        public override ReadOnlyCollection<Type>? DependentMods => null;
    }
}
