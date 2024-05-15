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
            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new DialogViewModel
            {
                Title = "RESET"
            }));
        }

        private void ApplyButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new DialogViewModel
            {
                Title = "APPLY"
            }));
        }
    }
}
