using System.Windows;
using System.Windows.Controls;
using OwlCore.AbstractUI.Models;

namespace ZuneModdingHelper.AbstractUI.Controls
{
    /// <summary>
    /// A control that displays an <see cref="AbstractProgressUIElement"/>.
    /// </summary>
    public sealed partial class AbstractProgessUIElementPresenter : Control
    {
        static AbstractProgessUIElementPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AbstractProgessUIElementPresenter), new FrameworkPropertyMetadata(typeof(AbstractProgessUIElementPresenter)));
        }
    }
}
