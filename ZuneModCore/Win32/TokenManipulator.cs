using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using Vanara.PInvoke;

namespace ZuneModCore.Win32
{
    public class TokenManipulator
    {
        public static bool AddPrivilege(string privilege)
        {
            try
            {
                bool retVal;
                var hproc = Kernel32.GetCurrentProcess();

                retVal = AdvApi32.OpenProcessToken(hproc,
                    AdvApi32.TokenAccess.TOKEN_ADJUST_PRIVILEGES | AdvApi32.TokenAccess.TOKEN_QUERY, out var htok);

                retVal = AdvApi32.LookupPrivilegeValue(null, privilege, out var tpLuid);
                AdvApi32.TOKEN_PRIVILEGES tp = new(tpLuid, AdvApi32.PrivilegeAttributes.SE_PRIVILEGE_ENABLED);

                AdvApi32.AdjustTokenPrivileges(htok, false, in tp, out _).ThrowIfFailed();

                return retVal;
            }
            catch
            {
                throw;
            }
        }
        public static bool RemovePrivilege(string privilege)
        {
            try
            {
                bool retVal;
                var hproc = Kernel32.GetCurrentProcess();

                retVal = AdvApi32.OpenProcessToken(hproc,
                    AdvApi32.TokenAccess.TOKEN_ADJUST_PRIVILEGES | AdvApi32.TokenAccess.TOKEN_QUERY, out var htok);

                retVal = AdvApi32.LookupPrivilegeValue(null, privilege, out var tpLuid);
                AdvApi32.TOKEN_PRIVILEGES tp = new(tpLuid, AdvApi32.PrivilegeAttributes.SE_PRIVILEGE_DISABLED);

                AdvApi32.AdjustTokenPrivileges(htok, false, in tp, out _).ThrowIfFailed();

                return retVal;
            }
            catch
            {
                throw;
            }
        }

#pragma warning disable CA1416 // Validate platform compatibility
        public static void TakeOwnership(FileInfo file)
        {
            EscalateToAdmin();

            // Get access control
            FileSecurity security = file.GetAccessControl();
            SecurityIdentifier? cu = WindowsIdentity.GetCurrent().User;

            // Set owner to current user
            security.SetOwner(cu);
            security.SetAccessRule(new FileSystemAccessRule(cu, FileSystemRights.Modify, AccessControlType.Allow));

            // Update the Access Control on the original file
            file.SetAccessControl(security);
        }

        public static void TakeOwnership(DirectoryInfo dir)
        {
            EscalateToAdmin();

            // Get access control
            DirectorySecurity security = dir.GetAccessControl();
            SecurityIdentifier? cu = WindowsIdentity.GetCurrent().User;

            // Set owner to current user
            security.SetOwner(cu);
            security.SetAccessRule(new FileSystemAccessRule(cu, FileSystemRights.Modify, AccessControlType.Allow));

            // Update the Access Control on the original file
            dir.SetAccessControl(security);
        }

        public static void TakeOwnership(FileSystemInfo info)
        {
            if (info is FileInfo file)
                TakeOwnership(file);
            else if (info is DirectoryInfo dir)
                TakeOwnership(dir);
        }
#pragma warning restore CA1416 // Validate platform compatibility

        public static bool TryTakeOwnership(FileInfo file, out Exception? exception)
        {
            try
            {
                TakeOwnership(file);

                exception = null;
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static void EscalateToAdmin()
        {
            // Activate necessary admin privileges to make changes without NTFS perms
            AddPrivilege("SeRestorePrivilege"); // Necessary to set Owner Permissions
            AddPrivilege("SeBackupPrivilege"); // Necessary to bypass Traverse Checking
            AddPrivilege("SeTakeOwnershipPrivilege"); // Necessary to override FilePermissions
        }
    }
}
