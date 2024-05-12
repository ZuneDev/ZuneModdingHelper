using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for AppWindow.xaml
    /// </summary>
    public partial class AppWindow
    {
        private IntPtr _windowHandle;
        private int _selectedPivotIdx = 0;

        public AppWindow()
        {
            Loaded += AppWindow_Loaded;
            InitializeComponent();
        }

        private void AppWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Select MODS tab
            var pivotItem = (ToggleButton)Pivot.Children[_selectedPivotIdx];
            pivotItem.IsChecked = true;
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

        private void PivotButton_Checked(object sender, RoutedEventArgs e)
        {
            var newIdx = Grid.GetColumn((UIElement)sender);
            if (newIdx == _selectedPivotIdx)
                return;

            var pivotItem = (ToggleButton)Pivot.Children[_selectedPivotIdx];
            pivotItem.IsChecked = false;

            _selectedPivotIdx = newIdx;
            pivotItem = (ToggleButton)Pivot.Children[_selectedPivotIdx];
            pivotItem.IsChecked = true;
        }
    }
}
