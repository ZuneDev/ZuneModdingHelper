using OwlCore.AbstractUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ZuneModCore;

public abstract class Mod(ModMetadata metadata)
{
    public static string DefaultZuneInstallDir { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Zune");

    public ModMetadata Metadata { get; } = metadata;

    public string ZuneInstallDir { get; set; } = DefaultZuneInstallDir;

    public virtual AbstractUICollection? GetDefaultOptionsUI() => null;

    public abstract Task<string?> Apply();

    public abstract Task<string?> Reset();

    private AbstractUICollection? _OptionsUI;
    public AbstractUICollection? OptionsUI
    {
        get
        {
            return _OptionsUI ??= GetDefaultOptionsUI();
        }
        set => _OptionsUI = value;
    }

    public string StorageDirectory
    {
        get
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZuneModCore", Metadata.Id);
            // Create the directory just in case the consumer assumes the folder exists already
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    public abstract IReadOnlyList<Type>? DependentMods { get; }
}
