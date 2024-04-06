using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using ZuneModdingHelper.Assets;

namespace ZuneModdingHelper.Services;

internal class LocalizationService : IStringLocalizer
{
    public LocalizedString this[string name]
    {
        get
        {
            var value = Strings.ResourceManager.GetString(name);
            return value is null
                ? new(name, name, true)
                : new(name, value);
        }
    }

    public LocalizedString this[string name, params object[] arguments] => this[name];

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        throw new NotImplementedException();
    }
}
