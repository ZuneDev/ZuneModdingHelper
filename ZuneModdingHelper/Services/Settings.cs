using OwlCore.ComponentModel;
using OwlCore.Storage;
using System.IO;
using System;
using ZuneModCore;
using ZuneModCore.Services;

namespace ZuneModdingHelper.Services;

public class Settings(IModifiableFolder folder) : SettingsBase(folder, SystemTextJsonStreamSerializer.Singleton), IModCoreConfig
{
    static Settings()
    {
        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZuneModCore", "Settings");
        Directory.CreateDirectory(dir);

        OwlCore.Storage.SystemIO.SystemFolder settingsFolder = new(dir);
        Default = new(settingsFolder);
    }

    public static Settings Default { get; }

    public string ZuneInstallDir
    {
        get => GetSetting(() => Mod.DefaultZuneInstallDir);
        set => SetSetting(value);
    }
}
