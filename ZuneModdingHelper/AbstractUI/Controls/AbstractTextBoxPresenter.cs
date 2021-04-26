using System.Windows;
using System.Windows.Controls;
using OwlCore.AbstractUI.Models;

namespace ZuneModdingHelper.AbstractUI.Controls
{
    /// <summary>
    /// A control that displays an <see cref="AbstractTextBox"/>.
    /// </summary>
    public sealed partial class AbstractTextBoxPresenter : Control
    {
        static AbstractTextBoxPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AbstractTextBoxPresenter), new FrameworkPropertyMetadata(typeof(AbstractTextBoxPresenter)));
        }
    }
}