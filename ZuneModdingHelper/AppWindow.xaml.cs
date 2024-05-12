using System;
using System.Windows;
using System.Windows.Interop;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for AppWindow.xaml
    /// </summary>
    public partial class AppWindow
    {
        private IntPtr _windowHandle;

        public AppWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _windowHandle = new WindowInteropHelper(this).EnsureHandle();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => SetWindowState(WindowState.Minimized);

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            SetWindowState(WindowState == WindowState.Normal
                ? WindowState.Maximized : WindowState.Normal);

            BorderThickness = new Thickness(WindowState == WindowState.Normal ? 0 : 8);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void SetWindowState(WindowState state)
        {
            // Hack for preserving animations and window sizing (don't cover the taskbar!)
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = state;
            WindowStyle = WindowStyle.None;
        }
    }
}
