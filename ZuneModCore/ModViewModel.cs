using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OwlCore.AbstractUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZuneModCore
{
    public class ModViewModel : ObservableObject
    {
        public ModViewModel()
        {
            _loadCommand = new AsyncRelayCommand(LoadAsync);
        }

        public ModViewModel(Mod mod) : this()
        {
            UpdateMod(mod);
        }

        private Mod _mod;
        private AbstractUICollectionViewModel? _optionsVm;
        private IAsyncRelayCommand _loadCommand;
        private IAsyncRelayCommand? _actionCommand;
        private bool _hasDependencies;
        private string _actionButtonText;
        private IAsyncRelayCommand _applyCommand;
        private IAsyncRelayCommand _resetCommand;

        public Mod Mod
        {
            get => _mod;
            set
            {
                UpdateMod(value);
                SetProperty(ref _mod, value);
            }
        }

        public AbstractUICollectionViewModel? OptionsViewModel
        {
            get => _optionsVm;
            set => SetProperty(ref _optionsVm, value);
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

        public bool HasDependencies
        {
            get => _hasDependencies;
            set => SetProperty(ref _hasDependencies, value);
        }

        private IReadOnlyList<ModDependency>? _Dependencies;
        public IReadOnlyList<ModDependency>? Dependencies
        {
            get => _Dependencies;
            set => SetProperty(ref _Dependencies, value);
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

        public void Dispose()
        {
            Mod.StatusChanged -= OnStatusChanged;
        }

        private void UpdateMod(Mod mod)
        {
            if (mod == null) return;

            _mod = mod;
            _applyCommand = new AsyncRelayCommand(Mod.ApplyWithDependencies);
            _resetCommand = new AsyncRelayCommand(Mod.Reset);

            Dependencies = Mod.DependentMods;
            HasDependencies = Dependencies != null && Dependencies.Count > 0;

            if (Mod.OptionsUI != null)
                OptionsViewModel = new(Mod.OptionsUI);

            Mod.StatusChanged += OnStatusChanged;
        }
    }
}
