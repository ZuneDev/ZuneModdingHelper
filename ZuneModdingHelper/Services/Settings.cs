using OwlCore.ComponentModel;
using OwlCore.Storage;
using System.IO;
using System;
using ZuneModCore.Services;

namespace ZuneModdingHelper.Services;

public class Settings(IModifiableFolder folder) : SettingsBase(folder, SystemTextJsonStreamSerializer.Singleton), IModCoreConfig
{
    static Settings()
    {
        string dir = Path.Combine(AppDataDir, "Settings");
        Directory.CreateDirectory(dir);

        OwlCore.Storage.SystemIO.SystemFolder settingsFolder = new(dir);
        Default = new(settingsFolder);
    }

    public static string AppDataDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZuneModCore");

    public static Settings Default { get; }

    public string ZuneInstallDir
    {
        get => GetSetting(GetDefaultZuneInstallDir);
        set => SetSetting(value);
    }

    public DateTimeOffset? NextDonationRequestTime
    {
        get => GetSetting(() => DateTimeOffset.UtcNow);
        set => SetSetting(value);
    }

    public DonationRequestInterval DonationRequestInterval
    {
        get => GetSetting(() => DonationRequestInterval.OneMonth);
        set => SetSetting(value);
    }

    private static string GetDefaultZuneInstallDir()
    {
        // Check common install locations for Zune
        string[] commonPaths = [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Zune"),
            @"C:\Program Files\Zune",
            @"C:\Program Files (x86)\Zune",
        ];

        foreach (var path in commonPaths)
            if (Directory.Exists(path))
                return path;

        return commonPaths[0];
    }
}

public enum DonationRequestInterval
{
    EveryMod,
    OneWeek,
    TwoWeeks,
    OneMonth,
    ThreeMonths,
    SixMonths,
    OneYear
}
