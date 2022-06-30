using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OwlCore.AbstractUI.Models;
using OwlCore.AbstractUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZuneModCore
{
    public class ModViewModel : ObservableObject
    {
        public ModViewModel(Mod mod)
        {
            Mod = mod;
            _loadCommand = new AsyncRelayCommand(LoadAsync);
            _applyCommand = new AsyncRelayCommand(Mod.ApplyWithDependencies);
            _resetCommand = new AsyncRelayCommand(Mod.Reset);

            if (Mod.OptionsUI != null)
                OptionsViewModel = new(Mod.OptionsUI);

            Mod.StatusChanged += OnStatusChanged;
        }

        private Mod _mod;
        private AbstractUICollectionViewModel? _optionsVm;
        private IAsyncRelayCommand _loadCommand;
        private IAsyncRelayCommand? _actionCommand;
        private bool _hasOptions;
        private bool _hasDependencies;
        private string _actionButtonText;
        private readonly IAsyncRelayCommand _applyCommand;
        private readonly IAsyncRelayCommand _resetCommand;

        public Mod Mod
        {
            get => _mod;
            set
            {
                HasDependencies = _mod?.DependentMods != null && _mod.DependentMods.Count > 0;
                SetProperty(ref _mod, value);
            }
        }

        public AbstractUICollectionViewModel? OptionsViewModel
        {
            get => _optionsVm;
            set
            {
                HasOptions = _optionsVm != null;
                SetProperty(ref _optionsVm, value);
            }
        }
        
        public IAsyncRelayCommand LoadCommand
        {
            get => _loadCommand;
            set => SetProperty(ref _loadCommand, value);
        }
        
        public IAsyncRelayCommand? ActionCommand
        {
            get => _actionCommand;
            set => SetProperty(ref _actionCommand, value);
        }

        public bool HasOptions
        {
            get => _hasOptions;
            set => SetProperty(ref _hasOptions, value);
        }

        public bool HasDependencies
        {
            get => _hasDependencies;
            set => SetProperty(ref _hasDependencies, value);
        }

        public string ActionButtonText
        {
            get => _actionButtonText;
            set => SetProperty(ref _actionButtonText, value);
        }

        public async Task LoadAsync()
        {
            OnStatusChanged(_mod, _mod.CheckApplied());
        }

        private void OnStatusChanged(Mod mod, bool status)
        {
            if (status)
            {
                ActionButtonText = "Reset";
                ActionCommand = _resetCommand;
            }
            else
            {
                ActionButtonText = "Apply";
                ActionCommand = _applyCommand;
            }
        }
    }
}
