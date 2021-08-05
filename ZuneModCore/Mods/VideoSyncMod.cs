using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using ZuneModCore.Win32;

namespace ZuneModCore.Mods
{
    public class VideoSyncMod : Mod
    {
        private readonly string WMVCORE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WMVCORE.dll");

        public override string Title => "Fix Video Sync";

        public override string Description =>
            "Resolves \"Error C00D11CD\" when attempting to sync video to a Zune device using Windows 10 1607 (Anniversary Update) or newer";

        public override string Author => "ส็็็Codix#4833";

        public override string Id => nameof(VideoSyncMod);

        public override AbstractUIElementGroup? OptionsUI => null;

#pragma warning disable CA1416 // Validate platform compatibility
        public override Task<string?> Apply()
        {
            // Make a backup of the original file
            FileVersionInfo wmvDllVersionInfo = FileVersionInfo.GetVersionInfo(WMVCORE_PATH);
            if (Version.Parse(wmvDllVersionInfo.ProductVersion!) != new Version(12, 0, 10586, 0))
                File.Copy(WMVCORE_PATH, Path.Combine(StorageDirectory, "WMVCORE.original.dll"), true);

            // Get the working WMVCORE.dll
            string sourcePath = Path.Combine(AppContext.BaseDirectory, "Resources\\WMVCORE.dll");

            // Get the original WMVCORE.dll
            FileInfo wmvDllInfo = new(WMVCORE_PATH);

            try
            {
                // Activate necessary admin privileges to make changes without NTFS perms
                TokenManipulator.AddPrivilege("SeRestorePrivilege"); // Necessary to set Owner Permissions
                TokenManipulator.AddPrivilege("SeBackupPrivilege"); // Necessary to bypass Traverse Checking
                TokenManipulator.AddPrivilege("SeTakeOwnershipPrivilege"); // Necessary to override FilePermissions

                // Get access control
                FileSecurity security = wmvDllInfo.GetAccessControl();
                SecurityIdentifier? cu = WindowsIdentity.GetCurrent().User;
                if (cu == null)
                    return Task.FromResult<string?>("Failed to set permissions on WMVCORE.dll, current user was null");

                // Set owner to current user
                security.SetOwner(cu);
                security.SetAccessRule(new FileSystemAccessRule(cu, FileSystemRights.Modify, AccessControlType.Allow));

                // Update the Access Control on the original WMVCORE.dll
                wmvDllInfo.SetAccessControl(security);

                // Copy the pre-Anniversary Update WMVCORE.dll
                File.Copy(sourcePath, WMVCORE_PATH, true);

                return Task.FromResult<string?>(null);
            }
            catch (IOException)
            {
                return Task.FromResult<string?>($"Unable to replace '{wmvDllInfo.FullName}'. Verify that Zune and Windows Media Player are not running and try again.");
            }
            catch (Exception ex)
            {
                return Task.FromResult(ex.Message)!;
            }
        }
#pragma warning restore CA1416 // Validate platform compatibility

        public override Task<string?> Reset()
        {
            try
            {
                // Copy backup to application folder
                File.Copy(Path.Combine(StorageDirectory, "WMVCORE.original.dll"), WMVCORE_PATH, true);

                return Task.FromResult<string?>(null);
            }
            catch (Exception ex)
            {
                return Task.FromResult(ex.Message)!;
            }
        }

        public override IReadOnlyList<Type>? DependentMods => null;
    }
}
