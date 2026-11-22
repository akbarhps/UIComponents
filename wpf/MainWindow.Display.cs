using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace UIComponents.Wpf
{
    public partial class MainWindow
    {
        private DispatcherTimer _progressTimer;
        private readonly Random _progressRandom = new Random();
        private int _progressValue;

        // Progress bar (05-display)
        private void ProgressStart_Click(object sender, RoutedEventArgs e)
        {
            if (_progressTimer != null && _progressTimer.IsEnabled) return;
            ProgressLabel.Text = "Uploading files…";
            // Web keeps the old bar until the first tick: no visual reset here.
            _progressValue = 0;
            _progressTimer = new DispatcherTimer();
            _progressTimer.Interval = TimeSpan.FromMilliseconds(350);
            _progressTimer.Tick += ProgressTimer_Tick;
            _progressTimer.Start();
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            int value = Math.Min(100, _progressValue + _progressRandom.Next(13) + 4);
            _progressValue = value;
            SetProgress(value);
            if (value >= 100)
            {
                _progressTimer.Stop();
                _progressTimer = null;
                ProgressLabel.Text = "Upload complete";
                Toasts.Show("success", "Upload complete", "All files were uploaded.");
            }
        }

        private void ProgressReset_Click(object sender, RoutedEventArgs e)
        {
            if (_progressTimer != null)
            {
                _progressTimer.Stop();
                _progressTimer = null;
            }
            ProgressLabel.Text = "Uploading files…";
            _progressValue = 0;
            SetProgress(0);
        }

        // The only writer of Value: a held animation masks a direct assignment.
        private void SetProgress(int value)
        {
            double ms = SystemParameters.ClientAreaAnimation ? 250 : 0;
            DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
            SplineDoubleKeyFrame frame = new SplineDoubleKeyFrame(
                value, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(ms)));
            frame.KeySpline = new KeySpline(0.25, 0.1, 0.25, 1);
            animation.KeyFrames.Add(frame);
            ProgressBar.BeginAnimation(RangeBase.ValueProperty, animation);
            ProgressValue.Text = value + "%";
        }

        // Skeleton (05-display): B6 shows the motion cue when animations are off.
        private void ApplyReducedMotion()
        {
            if (!SystemParameters.ClientAreaAnimation)
                LoadingCue.Visibility = Visibility.Visible;
        }
    }
}

