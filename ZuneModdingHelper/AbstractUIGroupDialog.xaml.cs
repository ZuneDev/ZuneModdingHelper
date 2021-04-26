using System.Windows;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for AbstractUIGroupDialog.xaml
    /// </summary>
    public partial class AbstractUIGroupDialog : Window
    {
        public AbstractUIGroupDialog()
        {
            InitializeComponent();
        }

        private void OptionsUINextButton_Click(object sender, RoutedEventArgs e) => Close();
    }
}
