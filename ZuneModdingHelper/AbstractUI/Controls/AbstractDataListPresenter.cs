using System.Windows;
using System.Windows.Controls;
using OwlCore.AbstractUI.Models;

namespace ZuneModdingHelper.AbstractUI.Controls
{
    /// <summary>
    /// A control that displays an <see cref="AbstractDataList"/>.
    /// </summary>
    public sealed partial class AbstractDataListPresenter : Control
    {
        static AbstractDataListPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AbstractDataListPresenter), new FrameworkPropertyMetadata(typeof(AbstractDataListPresenter)));
        }
    }
}