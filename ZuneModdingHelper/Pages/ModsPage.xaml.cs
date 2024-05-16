using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Controls;
using ZuneModCore;
using ZuneModdingHelper.Messages;

namespace ZuneModdingHelper.Pages
{
    /// <summary>
    /// Interaction logic for ModsPage.xaml
    /// </summary>
    public partial class ModsPage : UserControl
    {
        public ModsPage()
        {
            InitializeComponent();
            ModList.ItemsSource = Mod.AvailableMods;
        }

        private void ResetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!TryGetModFromControl(sender, out var mod))
                return;

            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new ProgressDialogViewModel
            {
                Title = "RESET MOD",
                Description = $"Preparing to reset '{mod.Title}'...",
                ShowPrimaryButton = false,
            }));
        }

        private void ApplyButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new DialogViewModel
            {
                Title = "APPLY"
            }));
        }

        private static bool TryGetModFromControl(object sender, out Mod mod)
        {
            mod = (sender as System.Windows.FrameworkElement)?.DataContext as Mod;
            return mod is not null;
        }
    }
}
