using System.Windows;
using System.Windows.Controls;
using OwlCore.AbstractUI.Models;

namespace ZuneModdingHelper.AbstractUI.Controls
{
    /// <summary>
    /// A control that displays an <see cref="AbstractTextBox"/>.
    /// </summary>
    public sealed partial class AbstractRichTextBlockPresenter : Control
    {
        static AbstractRichTextBlockPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AbstractRichTextBlockPresenter), new FrameworkPropertyMetadata(typeof(AbstractRichTextBlockPresenter)));
        }
    }
}