using OwlCore.AbstractUI.Models;
using System.Windows;
using System.Windows.Controls;

namespace ZuneModdingHelper.AbstractUI.Controls
{
    /// <summary>
    /// A control that displays an <see cref="AbstractButton"/>.
    /// </summary>
    public sealed partial class AbstractButtonPresenter : Control
    {
        /// <summary>
        /// Creates a new instance of <see cref="AbstractButtonPresenter"/>.
        /// </summary>
        public AbstractButtonPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AbstractButtonPresenter), new FrameworkPropertyMetadata(typeof(AbstractButtonPresenter)));
        }
    }
}