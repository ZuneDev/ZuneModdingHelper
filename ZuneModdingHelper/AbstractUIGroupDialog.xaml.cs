using OwlCore.AbstractUI.Models;
using OwlCore.AbstractUI.ViewModels;
using System.Runtime.InteropServices;
using System;
using System.Windows;
using System.Windows.Interop;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for AbstractUIGroupDialog.xaml
    /// </summary>
    public partial class AbstractUIGroupDialog : Window
    {
        [LibraryImport("DwmApi")]
        private static partial int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // TODO: Move this entire control into a proper dialog.
            var isAtLeastWin10 = Environment.OSVersion.Version > new Version(10, 0);
            if (isAtLeastWin10)
            {
                var handle = new WindowInteropHelper(this).EnsureHandle();

                if (DwmSetWindowAttribute(handle, 19, [1], 4) != 0)
                    _ = DwmSetWindowAttribute(handle, 20, [1], 4);

            }
        }

        public AbstractUIGroupDialog(AbstractUICollectionViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
            InitializeComponent();
        }
        public AbstractUIGroupDialog(AbstractUICollection group) : this(new AbstractUICollectionViewModel(group))
        {

        }

        private void OptionsUINextButton_Click(object sender, RoutedEventArgs e) => Finish(true);

        private void OptionsUICancelButton_Click(object sender, RoutedEventArgs e) => Finish(false);

        private void Finish(bool result)
        {
            DialogResult = result;
            Close();
        }

        public AbstractUICollectionViewModel ViewModel
        {
            get => (AbstractUICollectionViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel), typeof(AbstractUICollectionViewModel), typeof(AbstractUIGroupDialog));
    }
}
