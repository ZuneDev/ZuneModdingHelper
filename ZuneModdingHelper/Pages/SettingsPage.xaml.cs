using System.Windows;
using System.Windows.Controls;
using ZuneModdingHelper.Services;

namespace ZuneModdingHelper.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();
            DataContext = Settings.Default;
        }

        private async void SettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            await Settings.Default.SaveAsync();
        }
    }
}
