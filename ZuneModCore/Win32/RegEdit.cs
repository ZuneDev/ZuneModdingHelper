using Microsoft.Win32;

namespace ZuneModCore.Win32;

public class RegEdit
{
    public const string ZUNE_REG_PATH = @"SOFTWARE\Microsoft\Zune\";

    public static bool CurrentUserSetStringValue(string key, string name, string value)
    {
        RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(key, true);
        regKey ??= Registry.CurrentUser.CreateSubKey(key, true);

        regKey.SetValue(name, value, RegistryValueKind.String);
        regKey.Close();
        regKey.Dispose();

        // Read the key to make sure it was set properly
        return CurrentUserGetStringValue(key, name) == value;
    }

    public static string? CurrentUserGetStringValue(string key, string name)
    {
        using RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(key, true);
        if (regKey == null)
            return null;

        // Return null if a string value couldn't be read from the key
        if (regKey.GetValue(name, false) is string value)
            return value;
        return null;
    }

    public static bool CurrentUserSetBoolValue(string key, string name, bool value)
    {
        RegistryKey? regKey = Registry.CurrentUser.OpenSubKey(key, true);
        regKey ??= Registry.CurrentUser.CreateSubKey(key, true);

        regKey.SetValue(name, value, RegistryValueKind.DWord);
        regKey.Close();
        regKey.Dispose();

        // Read the key to make sure it was set properly
        return CurrentUserGetBoolValue(key, name) == value;
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
}
