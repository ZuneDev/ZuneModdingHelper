using System.Windows;
using System.Windows.Controls;
using OwlCore.AbstractUI.Models;

namespace ZuneModdingHelper.AbstractUI.Controls
{
    /// <summary>
    /// A control that displays an <see cref="AbstractMultiChoiceUIElement"/>.
    /// </summary>
    public sealed partial class AbstractMultiChoicePresenter : Control
    {
        static AbstractMultiChoicePresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AbstractMultiChoicePresenter), new FrameworkPropertyMetadata(typeof(AbstractMultiChoicePresenter)));
        }
    }
}