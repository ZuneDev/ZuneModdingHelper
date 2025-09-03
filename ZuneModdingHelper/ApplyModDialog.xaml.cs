using OwlCore.AbstractUI.Models;
using OwlCore.AbstractUI.ViewModels;
using System.Runtime.InteropServices;
using System;
using System.Windows;
using System.Windows.Interop;
using ZuneModCore;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for ApplyModDialog.xaml
    /// </summary>
    public partial class ApplyModDialog : Window
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

        public ApplyModDialog(ApplyModDialogViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        private void OptionsUINextButton_Click(object sender, RoutedEventArgs e) => Finish(true);

        private void OptionsUICancelButton_Click(object sender, RoutedEventArgs e) => Finish(false);

        private void Finish(bool result)
        {
            DialogResult = result;
            Close();
        }

        public ApplyModDialogViewModel ViewModel => (ApplyModDialogViewModel)DataContext;
    }

    public record ApplyModDialogViewModel(AbstractUICollectionViewModel OptionsUI, ModMetadata Metadata)
    {
        public ApplyModDialogViewModel(Mod mod)
            : this(new AbstractUICollectionViewModel(mod.OptionsUI), mod.Metadata)
        {
        }

        public ApplyModDialogViewModel() : this(null, null) { }

        public string FullDescription => GetFullDescription();

        private string GetFullDescription()
        {
            if (Metadata.ExtendedDescription is null)
                return Metadata.Description;

            return Metadata.Description + "\r\n\r\n" + Metadata.ExtendedDescription;
        }
    }
}
