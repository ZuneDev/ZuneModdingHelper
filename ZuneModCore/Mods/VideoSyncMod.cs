using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ZuneModCore.Mods;

public class VideoSyncMod : Mod
{
    private const int ZUNEENCENG_WMVCORA_OFFSET = 0x161E;

    private const string Description =
        "Resolves \"Error C00D11CD\" when attempting to sync video to a Zune device using Windows 10 1607 (Anniversary Update) or newer";

    private const string Author = "sylvathemoth";

    public override ModMetadata Metadata => new(nameof(VideoSyncMod), "Fix Video Sync", Description, Author);

    public override async Task<string?> Apply()
    {
        // Verify that ZuneEncEng.dll exists
        FileInfo zeeDllInfo = new(Path.Combine(ZuneInstallDir, "ZuneEncEng.dll"));
        if (!zeeDllInfo.Exists)
            return $"The file '{zeeDllInfo.FullName}' does not exist.";

        // Make a backup if it doesn't already exist
        FileInfo zeeDllBackupInfo = new(Path.Combine(StorageDirectory, "ZuneEncEng.original.dll"));
        if (!zeeDllBackupInfo.Exists)
            File.Copy(zeeDllInfo.FullName, zeeDllBackupInfo.FullName, true);

        try
        {
            // Verify that the DLL is from v4.8 (other versions not tested)
            var zeeDllVersion = FileVersionInfo.GetVersionInfo(zeeDllInfo.FullName);
            if (zeeDllVersion is null || zeeDllVersion.FileMajorPart != 4 || zeeDllVersion.FileMinorPart != 8)
                return "This mod has not been tested on versions earlier than 4.8.";

            // Open the file
            using (FileStream zeeDll = zeeDllInfo.Open(FileMode.Open))
            using (BinaryWriter zeeDllWriter = new(zeeDll))
            {
                // Patch ZuneEncEng.dll to point to the local WMVCORE.DLL
                zeeDllWriter.Seek(ZUNEENCENG_WMVCORA_OFFSET, SeekOrigin.Begin);
                zeeDllWriter.Write((byte)'a');

                zeeDllWriter.Flush();
            }

            // Get the working WMVCORE.dll
            byte[] wmvDllAniv = ModResources.WMVCORE;

            // Copy the WMVCore files to the Zune directory.
            // Only ZuneEncEng.dll searches in System32 first; all other dependent
            // DLLs will search the Zune folder.
            await File.WriteAllBytesAsync(Path.Combine(ZuneInstallDir, "wmvcora.dll"), wmvDllAniv);
            await File.WriteAllBytesAsync(Path.Combine(ZuneInstallDir, "wmvcore.dll"), wmvDllAniv);

            return null;
        }
        catch (IOException)
        {
            return $"Unable to replace '{zeeDllInfo.FullName}'. Verify that the Zune software is not running and try again.";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public override Task<string?> Reset()
    {
        var zeeDllPath = Path.Combine(ZuneInstallDir, "ZuneEncEng.dll");
        try
        {
            // Copy backup to application folder
            File.Copy(Path.Combine(StorageDirectory, "ZuneEncEng.original.dll"), zeeDllPath, true);

            // Delete WMVCore files
            File.Delete(Path.Combine(ZuneInstallDir, "wmvcora.dll"));
            File.Delete(Path.Combine(ZuneInstallDir, "wmvcore.dll"));

            return Task.FromResult<string?>(null);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ex.Message)!;
        }
    }

    public override IReadOnlyList<Type>? DependentMods => null;
}
