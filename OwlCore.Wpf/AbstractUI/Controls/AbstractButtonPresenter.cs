using System.Windows.Controls;
using OwlCore.AbstractUI.Models;

namespace OwlCore.Wpf.AbstractUI.Controls
{
    /// <summary>
    /// A control that displays an <see cref="AbstractButton"/>.
    /// </summary>
    public partial class AbstractButtonPresenter : Control
    {
        /// <summary>
        /// Creates a new instance of <see cref="AbstractButtonPresenter"/>.
        /// </summary>
        public AbstractButtonPresenter()
        {
            this.DefaultStyleKey = typeof(AbstractButtonPresenter);
        }
    }
}