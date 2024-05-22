using System.Collections.Generic;
using ZuneModCore.Mods;

namespace ZuneModCore;

public class ModManager
{
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

    private static List<Mod>? _mods;
    public static IReadOnlyList<Mod> GetAvailableMods()
    {
        if (_mods is null)
        {
            _mods = new(ModFactories.Count);
            var services = CommunityToolkit.Mvvm.DependencyInjection.Ioc.Default;

            foreach (var modType in ModFactories)
            {
                var mod = modType.Create(services);
                _mods.Add(mod);
            }
        }

        return _mods;
    }
}
