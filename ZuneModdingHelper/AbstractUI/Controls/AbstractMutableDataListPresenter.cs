using System.Windows;
using System.Windows.Controls;
using OwlCore.AbstractUI.Models;

namespace ZuneModdingHelper.AbstractUI.Controls
{
    /// <summary>
    /// A control that displays an <see cref="AbstractMutableDataList"/>.
    /// </summary>
    public sealed partial class AbstractMutableDataListPresenter : Control
    {
        static AbstractMutableDataListPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AbstractMutableDataListPresenter), new FrameworkPropertyMetadata(typeof(AbstractMutableDataListPresenter)));
        }
    }
}