﻿using System.Windows;
using System.Windows.Controls;
using OwlCore.AbstractUI.ViewModels;
using OwlCore.AbstractUI.Models;

namespace OwlCore.Wpf.AbstractUI.Controls
{
    /// <summary>
    /// Displays a group of abstract UI elements.
    /// </summary>
    public sealed partial class AbstractUIGroupPresenter : Control
    {
        private bool _dataContextBeingSet;

        /// <summary>
        /// Backing property for <see cref="ViewModel"/>.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(AbstractUICollectionViewModel), typeof(AbstractUIGroupPresenter), new PropertyMetadata(null, (d, e) => ((AbstractUIGroupPresenter)d).OnViewModelChanged()));

        /// <summary>
        /// Backing property for <see cref="TemplateSelector"/>.
        /// </summary>
        public static readonly DependencyProperty TemplateSelectorProperty =
            DependencyProperty.Register(nameof(TemplateSelector), typeof(DataTemplateSelector), typeof(AbstractUIGroupPresenter), new PropertyMetadata(null));

        /// <summary>
        /// The ViewModel for this UserControl.
        /// </summary>
        public AbstractUICollectionViewModel? ViewModel
        {
            get => (AbstractUICollectionViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        /// <summary>
        /// The template selector used to display Abstract UI elements. Use this to define your own custom styles for each control. You may specify the existing, default styles for those you don't want to override.
        /// </summary>
        public DataTemplateSelector? TemplateSelector
        {
            get => (DataTemplateSelector)GetValue(TemplateSelectorProperty);
            set => SetValue(TemplateSelectorProperty, value);
        }

        /// <summary>
        /// Creates a new instance of <see cref="AbstractUIGroupPresenter"/>.
        /// </summary>
        public AbstractUIGroupPresenter()
        {
            this.DefaultStyleKey = typeof(AbstractUIGroupPresenter);

            AttachEvents();
        }

        private void AttachEvents()
        {
            Loaded += OnLoaded;

            DataContextChanged += OnDataContextChanged;
        }

        private void DetachEvents()
        {
            DataContextChanged -= OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_dataContextBeingSet)
                return;

            _dataContextBeingSet = true;

            if (DataContext is AbstractUICollection elementGroup)
                ViewModel = new AbstractUICollectionViewModel(elementGroup);

            if (DataContext is AbstractUICollectionViewModel elementGroupViewModel)
                ViewModel = elementGroupViewModel;

            _dataContextBeingSet = false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= OnUnloaded;

            DetachEvents();
        }

        /// <summary>
        /// Raised when the <see cref="ViewModel"/> changes.
        /// </summary>
        public void OnViewModelChanged()
        {
            if (_dataContextBeingSet)
                return;

            _dataContextBeingSet = true;
            DataContext = ViewModel;
            _dataContextBeingSet = false;
        }
    }
}
