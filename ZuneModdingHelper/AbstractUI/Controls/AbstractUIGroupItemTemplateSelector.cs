using System.Windows;
using System.Windows.Controls;
using OwlCore.AbstractUI.ViewModels;
using Microsoft.Toolkit.Diagnostics;
using OwlCore.AbstractUI.Models;

namespace ZuneModdingHelper.AbstractUI.Controls
{
    /// <summary>
    /// The template selector used to display Abstract UI elements. Use this to define your own custom styles for each control. You may specify the existing, default styles for those you don't want to override.
    /// </summary>
    public class AbstractUIGroupItemTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Creates a new instance of <see cref="AbstractUIGroupItemTemplateSelector"/>.
        /// </summary>
        public AbstractUIGroupItemTemplateSelector()
        {
            object textBoxTemplate = new Themes.AbstractTextBoxStyle()["DefaultAbstractTextBoxTemplate"];
            if (textBoxTemplate == null)
                ThrowHelper.ThrowArgumentNullException<DataTemplate>(nameof(textBoxTemplate));

            object dataListTemplate = new Themes.AbstractDataListStyle()["DefaultAbstractDataListTemplate"];
            if (dataListTemplate == null)
                ThrowHelper.ThrowArgumentNullException<DataTemplate>(nameof(dataListTemplate));

            object mutableDataListTemplate = new Themes.AbstractMutableDataListStyle()["DefaultAbstractMutableDataListTemplate"];
            if (mutableDataListTemplate == null)
                ThrowHelper.ThrowArgumentNullException<DataTemplate>(nameof(mutableDataListTemplate));

            object buttonTemplate = new Themes.AbstractButtonStyle()["DefaultAbstractButtonTemplate"];
            if (buttonTemplate == null)
                ThrowHelper.ThrowArgumentNullException<DataTemplate>(nameof(buttonTemplate));

            object multiChoiceTemplate = new Themes.AbstractMultiChoiceUIElementStyle()["DefaultAbstractMultipleChoiceTemplate"];
            if (multiChoiceTemplate == null)
                ThrowHelper.ThrowArgumentNullException<DataTemplate>(nameof(multiChoiceTemplate));

            object booleanTemplate = new Themes.AbstractBooleanUIElementStyle()["DefaultAbstractBooleanUIElementTemplate"];
            if (booleanTemplate == null)
                ThrowHelper.ThrowArgumentNullException<DataTemplate>(nameof(booleanTemplate));

            object progressTemplate = new Themes.AbstractProgressUIElementStyle()["DefaultAbstractProgressUIElementTemplate"];
            if (progressTemplate == null)
                ThrowHelper.ThrowArgumentNullException<DataTemplate>(nameof(progressTemplate));

            TextBoxTemplate = (DataTemplate)textBoxTemplate;
            DataListTemplate = (DataTemplate)dataListTemplate;
            ButtonTemplate = (DataTemplate)buttonTemplate;
            MutableDataListTemplate = (DataTemplate)mutableDataListTemplate;
            MultiChoiceTemplate = (DataTemplate)multiChoiceTemplate;
            BooleanTemplate = (DataTemplate)booleanTemplate;
            ProgressTemplate = (DataTemplate)progressTemplate;
        }

        /// <summary>
        /// The data template used to display an <see cref="AbstractTextBox"/>.
        /// </summary>
        public DataTemplate TextBoxTemplate { get; set; }

        /// <summary>
        /// The data template used to display an <see cref="AbstractDataList"/>.
        /// </summary>
        public DataTemplate DataListTemplate { get; set; }

        /// <summary>
        /// The data template used to display an <see cref="AbstractMutableDataList"/>.
        /// </summary>
        public DataTemplate MutableDataListTemplate { get; set; }

        /// <summary>
        /// The data template used to display an <see cref="AbstractButton"/>.
        /// </summary>
        public DataTemplate ButtonTemplate { get; set; }

        /// <summary>
        /// The data template used to display an <see cref="AbstractBooleanUIElement"/>.
        /// </summary>
        public DataTemplate BooleanTemplate { get; set; }

        /// <summary>
        /// The data template used to display an <see cref="AbstractProgressUIElement"/>.
        /// </summary>
        public DataTemplate ProgressTemplate { get; set; }

        /// <summary>
        /// The data template used to display an <see cref="AbstractMultiChoiceUIElement"/>.
        /// </summary>
        public DataTemplate MultiChoiceTemplate { get; set; }

        /// <inheritdoc />
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            System.Diagnostics.Debug.WriteLine($"{item.GetType()} template selecting for {nameof(AbstractUIGroupItemTemplateSelector)}");

            return item switch
            {
                AbstractTextBoxViewModel _ => TextBoxTemplate,
                AbstractDataListViewModel _ => DataListTemplate,
                AbstractButtonViewModel _ => ButtonTemplate,
                AbstractMutableDataListViewModel _ => MutableDataListTemplate,
                AbstractMultiChoiceUIElementViewModel _ => MultiChoiceTemplate,
                AbstractBooleanViewModel _ => BooleanTemplate,
                AbstractProgressUIElement _ => ProgressTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
}