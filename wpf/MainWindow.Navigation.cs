using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace UIComponents.Wpf
{
    public partial class MainWindow
    {
        // ponytail: private capture/reduced-motion guard; lift into a shared spinner guard when overlays lands one
        private static readonly bool IsCapture = IsCaptureMode();
        private DispatcherTimer _loadingTimer;
        private ContentControl _spinner;

        private static bool IsCaptureMode()
        {
            foreach (string arg in Environment.GetCommandLineArgs())
                if (arg == "--capture") return true;
            return !SystemParameters.ClientAreaAnimation;
        }

        private object BuildLoadingContent(string text)
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            _spinner = new ContentControl();
            _spinner.Style = (Style)FindResource("ButtonSpinner");
            _spinner.VerticalAlignment = VerticalAlignment.Center;
            panel.Children.Add(_spinner);
            TextBlock label = new TextBlock();
            label.Text = text;
            label.Margin = new Thickness(8, 0, 0, 0);
            label.VerticalAlignment = VerticalAlignment.Center;
            panel.Children.Add(label);
            return panel;
        }

        private void Spin(bool running)
        {
            if (_spinner == null || IsCapture) return;
            _spinner.ApplyTemplate();
            Grid rotor = _spinner.Template.FindName("Rotor", _spinner) as Grid;
            if (rotor == null) return;
            // A template's transform is shared and frozen: give this spinner its own.
            RotateTransform rotate = rotor.RenderTransform as RotateTransform;
            if (rotate == null || rotate.IsFrozen)
            {
                rotate = new RotateTransform();
                rotor.RenderTransform = rotate;
            }
            if (running)
            {
                DoubleAnimation animation = new DoubleAnimation(0, 360, TimeSpan.FromMilliseconds(700));
                animation.RepeatBehavior = RepeatBehavior.Forever;
                rotate.BeginAnimation(RotateTransform.AngleProperty, animation);
            }
            else
            {
                rotate.BeginAnimation(RotateTransform.AngleProperty, null);
                rotate.Angle = 0;
            }
        }

        // B1: the loading button is a one-shot 1600ms disabled state, with a live UIA name.
        private void BtnLoading_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || !button.IsEnabled) return;
            button.IsEnabled = false;
            button.Content = BuildLoadingContent("Working…");
            AutomationProperties.SetName(button, "Working…");
            Spin(true);
            if (_loadingTimer == null)
            {
                _loadingTimer = new DispatcherTimer();
                _loadingTimer.Interval = TimeSpan.FromMilliseconds(1600);
                _loadingTimer.Tick += new EventHandler(LoadingTimer_Tick);
            }
            _loadingTimer.Start();
        }

        private void LoadingTimer_Tick(object sender, EventArgs e)
        {
            _loadingTimer.Stop();
            Spin(false);
            BtnLoading.Content = BuildLoadingContent("Loading");
            AutomationProperties.SetName(BtnLoading, "Loading");
            BtnLoading.IsEnabled = true;
        }
    }
}
