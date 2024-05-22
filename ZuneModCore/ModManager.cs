using System;
using System.Collections.Generic;
using ZuneModCore.Mods;

namespace ZuneModCore;

public class ModManager
{
    private static List<Mod>? _mods;

    /// <summary>
    /// Factories for all available mods.
    /// </summary>
    public static readonly IReadOnlyList<IModFactory<Mod>> ModFactories = new List<IModFactory<Mod>>
    {
        new FeaturesOverrideModFactory(),
        new VideoSyncModFactory(),
        new WebservicesModFactory(),
        new BackgroundImageModFactory(),
        new MbidLocatorModFactory(),
    }.AsReadOnly();

    /// <summary>
    /// Creates instances of each mod using the available factories
    /// using the default MVVM IoC instance.
    /// </summary>
    public static IReadOnlyList<Mod> GetAvailableMods() => GetAvailableMods(CommunityToolkit.Mvvm.DependencyInjection.Ioc.Default);

    /// <summary>
    /// Creates instances of each mod using the available factories.
    /// </summary>
    public static IReadOnlyList<Mod> GetAvailableMods(IServiceProvider services)
    {
        if (_mods is null)
        {
            _mods = new(ModFactories.Count);

            foreach (var modType in ModFactories)
            {
                var mod = modType.Create(services);
                _mods.Add(mod);
            }
        }

        return _mods;
    }
}
