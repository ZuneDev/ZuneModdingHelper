using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using ZuneModCore;
using ZuneModdingHelper.Controls;
using ZuneModdingHelper.Messages;
using ZuneModdingHelper.Pages;

namespace ZuneModdingHelper
{
    /// <summary>
    /// Interaction logic for AppWindow.xaml
    /// </summary>
    public partial class AppWindow : IRecipient<ShowDialogMessage>, IRecipient<CloseDialogMessage>
    {
        private IntPtr _windowHandle;
        private int _selectedPivotIdx = -1;
        private int _lastClosedDialog = 0;

        private readonly Type[] _pages = [typeof(ModsPage), typeof(SettingsPage), typeof(AboutPage)];
        private readonly Storyboard _dialogExitAnimation;

        public AppWindow()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
            Pivot.Loaded += Pivot_Loaded;

            _dialogExitAnimation = (Storyboard)Resources["DialogExitAnimation"];

            WeakReferenceMessenger.Default.Register<ShowDialogMessage>(this);
            WeakReferenceMessenger.Default.Register<CloseDialogMessage>(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Show a warning if Zune is running
            Process[] procs = Process.GetProcessesByName("Zune");
            string zuneExePath = System.IO.Path.Combine(Mod.DefaultZuneInstallDir, "Zune.exe");
            if (procs.Length > 0 && procs.Any(p => p.MainModule.FileName == zuneExePath))
            {
                WeakReferenceMessenger.Default.Send(new ShowDialogMessage(new()
                {
                    Description = "The Zune software is currently running. You may run into issues applying or resetting mods.",
                }));
            }
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

            ContentFrame.Content = ActivatorUtilities.CreateInstance(Ioc.Default, _pages[_selectedPivotIdx]);
        }

        void IRecipient<ShowDialogMessage>.Receive(ShowDialogMessage message)
        {
            DialogPresenter.Content = new ZuneLightDialog
            {
                ViewModel = message.ViewModel
            };

            DialogPresenter.IsHitTestVisible = true;
        }

        void IRecipient<CloseDialogMessage>.Receive(CloseDialogMessage message)
        {
            DialogPresenter.IsHitTestVisible = false;

            _dialogExitAnimation.Completed += DialogExitAnimation_Completed;

            _lastClosedDialog = DialogPresenter.Content.GetHashCode();
            (DialogPresenter.Content as FrameworkElement)?.BeginStoryboard(_dialogExitAnimation);
        }

        private void DialogExitAnimation_Completed(object sender, EventArgs e)
        {
            _dialogExitAnimation.Completed -= DialogExitAnimation_Completed;

            if (_lastClosedDialog != DialogPresenter.Content?.GetHashCode())
                return;

            DialogPresenter.Content = null;
        }
    }
}
