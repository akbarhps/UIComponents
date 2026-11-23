using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace UIComponents.Wpf
{
    // B7: a gradient band that sweeps across each skeleton shape, in phase for all of them.
    // The overlay is a rounded Border, so its background clips to the shape without a
    // RectangleGeometry. It is collapsed for --capture and for reduced motion.
    public static class Shimmer
    {
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(Shimmer),
                new PropertyMetadata(false, OnEnabledChanged));

        public static void SetEnabled(DependencyObject element, bool value)
        {
            element.SetValue(EnabledProperty, value);
        }

        public static bool GetEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(EnabledProperty);
        }

        // One unfrozen brush: a frozen dictionary brush throws when animated, and a
        // shared transform keeps every band in phase.
        private static readonly LinearGradientBrush OverlayBrush = CreateBrush();
        private static readonly bool IsCapture = IsCaptureMode();
        private static bool animationStarted;   // the shared transform is animated once, not once per band

        private static bool IsCaptureMode()
        {
            foreach (string arg in Environment.GetCommandLineArgs())
                if (arg == "--capture") return true;
            return false;
        }

        static Shimmer()
        {
            // The brush is unfrozen and animated, so it cannot hold a DynamicResource: re-read the token on a theme switch.
            ThemeManager.ThemeChanged += delegate { OverlayBrush.GradientStops[1].Color = BandColor(); };
        }

        private static Color BandColor()
        {
            // No Application (designer, WinForms host) or no Shimmer key in the consumer's tokens: fall back to the light value.
            Application app = Application.Current;
            SolidColorBrush token = app == null ? null : app.TryFindResource("Shimmer") as SolidColorBrush;   // theme-lint: allow
            return token != null ? token.Color : Color.FromArgb(0xA6, 0xFF, 0xFF, 0xFF);   // theme-lint: allow
        }

        private static LinearGradientBrush CreateBrush()
        {
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(1, 0);
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF), 0));   // theme-lint: allow (transparent end)
            brush.GradientStops.Add(new GradientStop(BandColor(), 0.5));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF), 1));   // theme-lint: allow (transparent end)
            brush.RelativeTransform = new TranslateTransform();
            return brush;
        }

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Border border = d as Border;
            if (border == null || !(bool)e.NewValue) return;
            border.Loaded += delegate { Apply(border); };
        }

        private static void Apply(Border border)
        {
            if (border.Child != null) return;   // a second Loaded must not restart the shared clock
            bool reduced = !SystemParameters.ClientAreaAnimation;
            if (reduced)
            {
                border.SetResourceReference(Border.BackgroundProperty, "Line");
            }

            Border overlay = new Border();
            overlay.CornerRadius = border.CornerRadius;
            overlay.Background = OverlayBrush;
            overlay.IsHitTestVisible = false;
            overlay.HorizontalAlignment = HorizontalAlignment.Stretch;
            overlay.VerticalAlignment = VerticalAlignment.Stretch;
            if (IsCapture || reduced)
            {
                overlay.Visibility = Visibility.Collapsed;
            }
            else if (!animationStarted)
            {
                animationStarted = true;
                TranslateTransform transform = (TranslateTransform)OverlayBrush.RelativeTransform;
                transform.X = -1;
                DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
                SplineDoubleKeyFrame frame = new SplineDoubleKeyFrame(
                    1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1400)));
                frame.KeySpline = new KeySpline(0.25, 0.1, 0.25, 1);
                animation.KeyFrames.Add(frame);
                animation.RepeatBehavior = RepeatBehavior.Forever;
                transform.BeginAnimation(TranslateTransform.XProperty, animation);
            }
            border.Child = overlay;
        }
    }
}
