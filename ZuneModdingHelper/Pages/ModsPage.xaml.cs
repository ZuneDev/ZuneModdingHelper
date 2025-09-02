using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OwlCore.AbstractUI.Models;
using OwlCore.ComponentModel;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ZuneModCore;
using ZuneModCore.Services;
using ZuneModdingHelper.Messages;
using ZuneModdingHelper.Services;

namespace ZuneModdingHelper.Pages
{
    /// <summary>
    /// Interaction logic for ModsPage.xaml
    /// </summary>
    public partial class ModsPage : UserControl
    {
        private const string MOD_MANAGER_TITLE = "MOD MANAGER";

        private readonly Settings _settings;

        public ModsPage(Settings settings)
        {
            _settings = settings;

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
            var mod = await CreateModInstance(modFactory);
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
                OnAction = new AsyncRelayCommand<bool>(ShowDonationRequestDialog)
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

            var mod = await CreateModInstance(modFactory);
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

        private static async Task<Mod> CreateModInstance(IModFactory<Mod> modFactory)
        {
            var mod = modFactory.Create(Ioc.Default);

            if (mod is IAsyncInit initMod)
                await initMod.InitAsync();

            return mod;
        }

        private async Task ShowDonationRequestDialog(bool _)
        {
            // Close success dialog
            WeakReferenceMessenger.Default.Send<CloseDialogMessage>();

            // Don't bother users if they're offline or the site is inaccessible
            try
            {
                var ping = new Ping();
                var result = await ping.SendPingAsync(App.DonateUri.Host);
                if (result.Status is not IPStatus.Success)
                    return;
            }
            catch
            {
                return;
            }

            // Ask for donations every once in a while (but only when mod is successful)
            if (_settings.NextDonationRequestTime is not null && _settings.NextDonationRequestTime <= System.DateTimeOffset.UtcNow)
            {
                // Set next request time
                var nextRequestTime = _settings.NextDonationRequestTime.Value;
                _settings.NextDonationRequestTime = _settings.DonationRequestInterval switch
                {
                    DonationRequestInterval.EveryMod => nextRequestTime,
                    DonationRequestInterval.OneWeek => nextRequestTime.AddDays(7),
                    DonationRequestInterval.TwoWeeks => nextRequestTime.AddMonths(14),
                    DonationRequestInterval.OneMonth => nextRequestTime.AddMonths(1),
                    DonationRequestInterval.ThreeMonths => nextRequestTime.AddMonths(3),
                    DonationRequestInterval.SixMonths => nextRequestTime.AddMonths(6),
                    DonationRequestInterval.OneYear => nextRequestTime.AddYears(1),
                    _ => nextRequestTime.AddMonths(1),
                };

                // Show request dialog
                WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new()
                {
                    Title = "THANK YOU FOR USING ZMH",
                    Description = $"{App.Title} is free software, but it still takes money and a lot of time to write, support, and distribute it.\r\n\r\n"
                        + "If ZMH has been helpful for you, please consider supporting this and other ZuneDev projects with a donation to the author.",
                    AffirmativeText = "DONATE",
                    NegativeText = "REMIND ME LATER",
                    ShowNegativeButton = true,
                    OnAction = new AsyncRelayCommand<bool>(openDonation =>
                    {
                        if (openDonation)
                            App.OpenDonationLink();

                        WeakReferenceMessenger.Default.Send<CloseDialogMessage>();
                        return Task.CompletedTask;
                    })
                }));
            }
        }
    }
}
