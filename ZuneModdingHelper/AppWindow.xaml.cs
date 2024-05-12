using ControlzEx.Behaviors;
using ControlzEx.Controls.Internal;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;

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
    }
}
