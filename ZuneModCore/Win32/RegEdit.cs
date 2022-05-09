using Microsoft.Win32;
using System.Diagnostics;

namespace ZuneModCore.Win32
{

#pragma warning disable CA1416 // Validate platform compatibility

    public class RegEdit
    {
        public const string HIVE_CURRENTUSER = "HKEY_CURRENT_USER";
        public const string ZUNE_REG_PATH = @"SOFTWARE\Microsoft\Zune\";

        public static bool CurrentUserSetBoolValue(string key, string name, bool value)
        {
            RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(key, true);
            if (regKey == null)
                regKey = Registry.CurrentUser.CreateSubKey(key, true);

            regKey.SetValue(name, value, RegistryValueKind.DWord);
            regKey.Close();
            regKey.Dispose();

            // Read the key to make sure it was set properly
            if (CurrentUserGetBoolValue(key, name) == null)
                return false;
            return true;
        }

        public static bool? CurrentUserGetBoolValue(string key, string name)
        {
            using RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(key, true);
            if (regKey == null)
                return false;

            // Return null if a boolean value couldn't be read from the key
            int? value = regKey.GetValue(name, false) as int?;
            if (!value.HasValue)
                return null;
            return value != 0;
        }

        public static void CurrentUserDeleteKey(string key)
        {
            using RegistryKey? targetKey = Registry.CurrentUser.OpenSubKey(key, true);
            if (targetKey == null)
                // Key is already deleted
                return;

            // Delete target subkeys
            if (targetKey.SubKeyCount > 0)
                targetKey.DeleteSubKeyTree(key, false);

            // Delete target values
            string[] values = targetKey.GetValueNames();
            foreach (string value in values)
                targetKey.DeleteValue(value, false);

            // Delete target key
            Registry.CurrentUser.DeleteSubKey(key);
        }

        public static void CurrentUserDeleteValue(string key, string name)
        {
            using RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(key, true);
            if (regKey == null)
                return;

            regKey.DeleteValue(name, false);
        }

        public static void ExportKey(string hive, string regKey, string savePath, bool overwrite = true)
        {
            string path = $"\"{savePath}\"";
            string key = $"\"{hive}\\{regKey}\"";

            Process proc = null!;
            try
            {
                string cmd = $"reg export {key} {path}";

                if (overwrite)
                    cmd += " /y";

                proc = Process.Start("cmd.exe", $"/c {cmd}");
                if (proc != null) proc.WaitForExit();
            }
            finally
            {
                if (proc != null) proc.Dispose();
            }
        }

        public static void ImportKey(string savePath)
        {
            string path = "\"" + savePath + "\"";

            Process proc = null!;
            try
            {
                string cmd = $"reg import {path}";

                proc = Process.Start("cmd.exe", $"/c \"{cmd}\"");
                if (proc != null) proc.WaitForExit();
            }
            finally
            {
                if (proc != null) proc.Dispose();
            }
        }
    }

#pragma warning restore CA1416 // Validate platform compatibility

}
