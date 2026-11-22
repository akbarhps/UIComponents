using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace UIComponents.Wpf
{
    public partial class OverlayControls : UserControl
    {
        private DispatcherTimer _loadingTimer;

        public OverlayControls()
        {
            InitializeComponent();
        }

        private void OpenModal_Click(object sender, RoutedEventArgs e)
        {
            OverlayHost.Open(ModalDialog, ModalClose);
        }

        private void ModalClose_Click(object sender, RoutedEventArgs e)
        {
            OverlayHost.Close();
        }

        private void ModalCancel_Click(object sender, RoutedEventArgs e)
        {
            OverlayHost.Close();
        }

        private void ModalConfirm_Click(object sender, RoutedEventArgs e)
        {
            OverlayHost.Close();
            Toasts.Show("success", "Deleted", "The project was removed.");
        }

        private void OpenLoading_Click(object sender, RoutedEventArgs e)
        {
            OverlayHost.Open(LoadingDialog, LoadingDialog, true);
            // Authored XAML is already in the tree; focus now, not only on the deferred pass.
            LoadingDialog.UpdateLayout();
            LoadingDialog.Focus();
            StartLoadingMotion();
            _loadingTimer = new DispatcherTimer();
            _loadingTimer.Interval = TimeSpan.FromMilliseconds(3000);
            _loadingTimer.Tick += LoadingTimer_Tick;
            _loadingTimer.Start();
        }

        private void LoadingTimer_Tick(object sender, EventArgs e)
        {
            if (_loadingTimer == null) return;
            _loadingTimer.Stop();
            _loadingTimer = null;
            StopLoadingMotion();
            OverlayHost.Close();
            Toasts.Show("success", "Ready", "Your workspace is ready.");
        }

        // Decision 9: motion is functional, but never run it for --capture or reduced motion.
        private static bool MotionAllowed()
        {
            foreach (string arg in Environment.GetCommandLineArgs())
                if (arg == "--capture") return false;
            return SystemParameters.ClientAreaAnimation;
        }

        private void StartLoadingMotion()
        {
            if (!MotionAllowed()) return;
            DoubleAnimation spin = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(0.8));
            spin.RepeatBehavior = RepeatBehavior.Forever;
            SpinnerRotate.BeginAnimation(RotateTransform.AngleProperty, spin);

            DoubleAnimation slide = new DoubleAnimation(-114.094, 445.0, TimeSpan.FromSeconds(1.2));
            slide.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut };
            slide.RepeatBehavior = RepeatBehavior.Forever;
            LoadingBarShift.BeginAnimation(TranslateTransform.XProperty, slide);
        }

        private void StopLoadingMotion()
        {
            SpinnerRotate.BeginAnimation(RotateTransform.AngleProperty, null);
            LoadingBarShift.BeginAnimation(TranslateTransform.XProperty, null);
            SpinnerRotate.Angle = 0;
            LoadingBarShift.X = 0;
        }
    }
}
