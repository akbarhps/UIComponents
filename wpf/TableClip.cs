using System;
using System.Windows;
using System.Windows.Media;

namespace UIComponents.Wpf
{
    // Rounded clip for table frames. WPF's ClipToBounds on a rounded Border still clips
    // the children as a rectangle, so apply an explicit rounded geometry to the content.
    public static class TableClip
    {
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(TableClip),
                new PropertyMetadata(false, OnEnabledChanged));

        public static void SetEnabled(DependencyObject obj, bool value) { obj.SetValue(EnabledProperty, value); }
        public static bool GetEnabled(DependencyObject obj) { return (bool)obj.GetValue(EnabledProperty); }

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement fe = d as FrameworkElement;
            if (fe == null) return;
            if ((bool)e.NewValue) fe.SizeChanged += OnSizeChanged;
            else fe.SizeChanged -= OnSizeChanged;
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)sender;
            if (fe.ActualWidth <= 0 || fe.ActualHeight <= 0) return;
            // Whole-pixel clip: a fractional bottom edge cuts the last table line mid-pixel.
            double w = Math.Round(fe.ActualWidth);
            // Keep the bottom outside the clip: the frame Border paints that line itself.
            double h = Math.Round(fe.ActualHeight) + 1;
            fe.Clip = new RectangleGeometry(new Rect(0, 0, w, h), 11, 11);
        }
    }
}
