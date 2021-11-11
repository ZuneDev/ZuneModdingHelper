using System;
using System.Windows;
using System.Windows.Controls;
using OwlCore.AbstractUI.ViewModels;
using Microsoft.Toolkit.Diagnostics;
using OwlCore.AbstractUI.Models;

namespace OwlCore.Wpf.AbstractUI.Themes
{
    /// <summary>
    /// Selects the template that is used for an <see cref="AbstractDataList"/> based on the <see cref="AbstractDataList.PreferredDisplayMode"/>.
    /// </summary>
    public class AbstractMultiChoiceTypeTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// The data template used to display an <see cref="AbstractTextBox"/>.
        /// </summary>
        public DataTemplate? ComboBoxTemplate { get; set; }

        /// <summary>
        /// The data template used to display an <see cref="AbstractDataList"/>.
        /// </summary>
        public DataTemplate? RadioButtonTemplate { get; set; }

        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is AbstractMultiChoiceViewModel viewModel)
            {
                return viewModel.PreferredDisplayMode switch
                {
                    AbstractMultiChoicePreferredDisplayMode.Dropdown => ComboBoxTemplate ?? ThrowHelper.ThrowArgumentNullException<DataTemplate>(),
                    AbstractMultiChoicePreferredDisplayMode.RadioButtons => RadioButtonTemplate ?? ThrowHelper.ThrowArgumentNullException<DataTemplate>(),
                    _ => throw new NotImplementedException(),
                };
            }

            return null!;
        }
    }
}