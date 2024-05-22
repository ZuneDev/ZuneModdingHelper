using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using OwlCore.AbstractUI.Models;
using OwlCore.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ZuneModCore;
using ZuneModCore.Services;
using ZuneModdingHelper.Messages;

namespace ZuneModdingHelper.Pages
{
    /// <summary>
    /// Interaction logic for ModsPage.xaml
    /// </summary>
    public partial class ModsPage : UserControl
    {
        private const string MOD_MANAGER_TITLE = "MOD MANAGER";

        private readonly IModCoreConfig _modConfig;

        public ModsPage(IModCoreConfig modConfig)
        {
            _modConfig = modConfig;

            InitializeComponent();
            ModList.ItemsSource = ModManager.ModFactories;
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetModFromControl(sender, out var modFactory))
                return;

            var modTitle = modFactory.Metadata.Title;
            ProgressDialogViewModel progDialog = new()
            {
                Title = MOD_MANAGER_TITLE,
                Description = $"Preparing to apply '{modTitle}'...",
                ShowAffirmativeButton = false,
                IsIndeterminate = true,
                Maximum = 3,
            };
            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(progDialog));

            // Stage 0: Initialize mod
            var mod = modFactory.Create(Ioc.Default);
            mod.ZuneInstallDir = _modConfig.ZuneInstallDir;
            if (mod is IAsyncInit initMod)
                await initMod.InitAsync();
            ++progDialog.Progress;

            // Stage 1: Display AbstractUI for options
            progDialog.Description = $"Awaiting options for '{modTitle}'...";
            if (mod.OptionsUI != null)
            {
                var optionsDialog = new AbstractUIGroupDialog(mod.OptionsUI);
                optionsDialog.Title = optionsDialog.Title + " | " + modTitle;
                bool? optionsResult = optionsDialog.ShowDialog();
                if (!(optionsResult.HasValue && optionsResult.Value))
                {
                    WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
                    return;
                }
                mod.OptionsUI = (AbstractUICollection)optionsDialog.ViewModel.Model;
            }
            ++progDialog.Progress;

            // Stage 2: Apply mod
            progDialog.Description = $"Applying '{modTitle}'...";
            string applyResult = await mod.Apply();
            if (applyResult != null)
            {
                WeakReferenceMessenger.Default.Send<CloseDialogMessage>();

                DialogViewModel errorDialog = new()
                {
                    Title = MOD_MANAGER_TITLE,
                    Description = $"Failed to apply '{modTitle}'.\r\n{applyResult}",
                };
                WeakReferenceMessenger.Default.Send(new ShowDialogMessage(errorDialog));

                return;
            }
            ++progDialog.Progress;

            WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new()
            {
                Title = MOD_MANAGER_TITLE,
                Description = $"Successfully applied '{modTitle}'",
            }));
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetModFromControl(sender, out var modFactory))
                return;

            var modTitle = modFactory.Metadata.Title;
            ProgressDialogViewModel progDialog = new()
            {
                Title = MOD_MANAGER_TITLE,
                Description = $"Preparing to reset '{modTitle}'...",
                ShowAffirmativeButton = false,
                IsIndeterminate = true,
                Maximum = 2,
            };
            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(progDialog));

            var mod = modFactory.Create(Ioc.Default);
            mod.ZuneInstallDir = _modConfig.ZuneInstallDir;
            if (mod is IAsyncInit initMod)
                await initMod.InitAsync();
            ++progDialog.Progress;

            // TODO: Implement AbstractUI display for reset options
            //if (mod.OptionsUI != null)
            //{
            //    var optionsDialog = new AbstractUIGroupDialog();
            //    optionsDialog.OptionsUIPresenter.ViewModel = new AbstractUICollectionViewModel(mod.OptionsUI);
            //    optionsDialog.ShowDialog();
            //}

            progDialog.Description = $"Resetting '{modTitle}'...";
            string resetResult = await mod.Reset();
            if (resetResult != null)
            {
                WeakReferenceMessenger.Default.Send<CloseDialogMessage>();

                DialogViewModel errorDialog = new()
                {
                    Title = MOD_MANAGER_TITLE,
                    Description = $"Failed to reset '{modTitle}'.\r\n{resetResult}",
                };
                WeakReferenceMessenger.Default.Send(new ShowDialogMessage(errorDialog));

                return;
            }

            ++progDialog.Progress;

            WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new()
            {
                Title = MOD_MANAGER_TITLE,
                Description = $"Successfully reset '{modTitle}'",
            }));
        }

        private static bool TryGetModFromControl(object sender, out IModFactory<Mod> modFactory)
        {
            modFactory = (sender as FrameworkElement)?.DataContext as IModFactory<Mod>;
            return modFactory is not null;
        }
    }
}
