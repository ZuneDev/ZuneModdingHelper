using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ZuneModCore.Win32
{
    public class TokenManipulator
    {
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);
        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr
        phtok);
        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(string host, string name,
        ref long pluid);
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }
        internal const int SE_PRIVILEGE_DISABLED = 0x00000000;
        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        public static bool AddPrivilege(string privilege)
        {
            try
            {
                bool retVal;
                TokPriv1Luid tp;
                IntPtr hproc = GetCurrentProcess();
                IntPtr htok = IntPtr.Zero;
                retVal = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
                tp.Count = 1;
                tp.Luid = 0;
                tp.Attr = SE_PRIVILEGE_ENABLED;
                retVal = LookupPrivilegeValue(null, privilege, ref tp.Luid);
                retVal = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
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
                TokPriv1Luid tp;
                IntPtr hproc = GetCurrentProcess();
                IntPtr htok = IntPtr.Zero;
                retVal = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
                tp.Count = 1;
                tp.Luid = 0;
                tp.Attr = SE_PRIVILEGE_DISABLED;
                retVal = LookupPrivilegeValue(null, privilege, ref tp.Luid);
                retVal = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
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
            // Activate necessary admin privileges to make changes without NTFS perms
            AddPrivilege("SeRestorePrivilege"); // Necessary to set Owner Permissions
            AddPrivilege("SeBackupPrivilege"); // Necessary to bypass Traverse Checking
            AddPrivilege("SeTakeOwnershipPrivilege"); // Necessary to override FilePermissions

            // Get access control
            FileSecurity security = file.GetAccessControl();
            SecurityIdentifier? cu = WindowsIdentity.GetCurrent().User;

            // Set owner to current user
            security.SetOwner(cu);
            security.SetAccessRule(new FileSystemAccessRule(cu, FileSystemRights.Modify, AccessControlType.Allow));

            // Update the Access Control on the original WMVCORE.dll
            file.SetAccessControl(security);
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
    }
}
