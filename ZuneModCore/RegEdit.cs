using Microsoft.Win32;

namespace ZuneModCore
{

#pragma warning disable CA1416 // Validate platform compatibility

    public class RegEdit
    {
        public const string ZUNE_REG_PATH = @"SOFTWARE\Microsoft\Zune\";

        public static void CurrentUserSetBoolValue(string key, string name, bool value)
        {
            RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(key, true);
            if (regKey == null)
                regKey = Registry.CurrentUser.CreateSubKey(key, true);

            regKey.SetValue(name, value, RegistryValueKind.DWord);
            regKey.Close();
            regKey.Dispose();
        }

        public static bool CurrentUserGetBoolValue(string key, string name)
        {
            using RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(key, true);
            if (regKey == null)
                return false;

            int? value = regKey.GetValue(name, false) as int?;
            return value.HasValue && value != 0;
        }

        public static void CurrentUserDeleteKey(string key)
        {
            string[] targetKeySplit = key.Split('\\', System.StringSplitOptions.RemoveEmptyEntries);
            using RegistryKey? targetKey = Registry.CurrentUser.OpenSubKey(key, true);
            if (targetKey == null)
                // Key is already deleted
                return;

            // Delete all subkeys
            RegistryKey? hdr = targetKey.OpenSubKey(targetKeySplit[^1], true);
            if (hdr != null)
                foreach (string subKey in hdr.GetSubKeyNames())
                    hdr.DeleteSubKey(subKey);
            hdr?.Close();

            // Delete target key
            targetKey.DeleteSubKeyTree(targetKeySplit[^1]);
        }

        public static void CurrentUserDeleteValue(string key, string name)
        {
            using RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(key, true);
            if (regKey == null)
                return;

            regKey.DeleteValue(name, false);
        }
    }

#pragma warning restore CA1416 // Validate platform compatibility

}
