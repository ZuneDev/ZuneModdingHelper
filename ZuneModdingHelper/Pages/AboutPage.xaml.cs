using System.Windows.Controls;
using System.Windows.Navigation;
using ZuneModdingHelper.Services;

namespace ZuneModdingHelper.Pages
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    public partial class AboutPage : UserControl
    {
        public AboutPage()
        {
            DataContext = this;
            InitializeComponent();
        }

        public bool EnableCheckForUpdates => UpdateHelper.Instance.Enabled;

        private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            App.OpenInBrowser(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private async void UpdateCheckButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await UpdateHelper.Instance.CheckForUpdatesAsync(true);
        }
    }
}
