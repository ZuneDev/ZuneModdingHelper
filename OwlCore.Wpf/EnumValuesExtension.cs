using System;
using System.Windows.Markup;

namespace OwlCore.Wpf;

public class EnumValuesExtension : MarkupExtension
{
    public Type Enum { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) => System.Enum.GetValues(Enum);
}
