using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using ZuneModdingHelper.Pages;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for AppWindow.xaml
    /// </summary>
    public partial class AppWindow
    {
        private IntPtr _windowHandle;
        private int _selectedPivotIdx = -1;

        private readonly Type[] _pages = [typeof(ModsPage), typeof(SettingsPage), typeof(AboutPage)];

        public AppWindow()
        {
            InitializeComponent();
            Pivot.Loaded += Pivot_Loaded;
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in Pivot.Children.OfType<ToggleButton>())
                item.Checked += PivotButton_Checked;

            // Select MODS tab
            var pivotItem = (ToggleButton)Pivot.Children[0];
            pivotItem.IsChecked = true;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            _windowHandle = new WindowInteropHelper(this).EnsureHandle();
            ContentEntranceAnimation.Begin(ContentFrame);
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

            if (_selectedPivotIdx >= 0 && _selectedPivotIdx < Pivot.Children.Count)
            {
                var oldPivotItem = (ToggleButton)Pivot.Children[_selectedPivotIdx];
                oldPivotItem.IsChecked = false;
            }

            _selectedPivotIdx = newIdx;
            var newPivotItem = (ToggleButton)Pivot.Children[_selectedPivotIdx];
            newPivotItem.IsChecked = true;

            ContentFrame.Content = Activator.CreateInstance(_pages[_selectedPivotIdx]);
        }
    }
}
