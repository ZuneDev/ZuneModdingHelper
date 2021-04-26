using OwlCore.AbstractUI.Models;
using System.Windows;
using System.Windows.Controls;

namespace ZuneModdingHelper.AbstractUI.Controls
{
    /// <summary>
    /// A control that displays an <see cref="AbstractBooleanUIElement"/>.
    /// </summary>
    public sealed partial class AbstractBooleanUIElementPresenter : Control
    {
        static AbstractBooleanUIElementPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AbstractBooleanUIElementPresenter), new FrameworkPropertyMetadata(typeof(AbstractBooleanUIElementPresenter)));
        }
    }
}