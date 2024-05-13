using System.Windows.Controls;
using ZuneModCore;

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
    }
}
