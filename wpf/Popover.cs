using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace UIComponents.Wpf
{
    public enum PopoverSide { BelowRight, Top, Bottom, Left, Right }

    // A lazily created in-window layer: real children (not Popups), so --capture and UIA see them.
    public static class Popover
    {
        private static Canvas _layer;
        private static readonly Dictionary<FrameworkElement, FrameworkElement> Anchors = new Dictionary<FrameworkElement, FrameworkElement>();
        private static readonly Dictionary<FrameworkElement, PopoverSide> Sides = new Dictionary<FrameworkElement, PopoverSide>();
        private static readonly Dictionary<Button, GroupBox> Tips = new Dictionary<Button, GroupBox>();
        // A short grace covers the 9px gap between a host and its hoverable bubble.
        private static readonly Dictionary<Button, DispatcherTimer> HideTimers = new Dictionary<Button, DispatcherTimer>();
        private static Button _activeTip;

        public static Canvas Layer
        {
            get
            {
                if (_layer != null) return _layer;
                Window win = Application.Current.MainWindow;
                Grid root = win == null ? null : win.Content as Grid;
                if (root == null) return null;
                Canvas layer = new Canvas();
                layer.Name = "PopoverLayer";
                Grid.SetColumnSpan(layer, 2);
                Panel.SetZIndex(layer, 50);
                layer.ClipToBounds = true;
                root.Children.Add(layer);
                layer.SizeChanged += delegate { RepositionAll(); };
                ScrollViewer scroll = win.FindName("ContentScroll") as ScrollViewer;
                if (scroll != null) scroll.ScrollChanged += delegate { RepositionAll(); };
                _layer = layer;
                return layer;
            }
        }

        public static void Show(FrameworkElement anchor, FrameworkElement content, PopoverSide side)
        {
            Canvas layer = Layer;
            if (layer == null || anchor == null || content == null) return;
            if (content.Parent != null)
            {
                Panel parent = content.Parent as Panel;
                if (parent != null) parent.Children.Remove(content);
            }
            content.Visibility = Visibility.Visible;
            if (!layer.Children.Contains(content)) layer.Children.Add(content);
            Anchors[content] = anchor;
            Sides[content] = side;
            layer.UpdateLayout();
            Position(content);
        }

        public static void Hide(FrameworkElement content)
        {
            if (content == null) return;
            content.Visibility = Visibility.Collapsed;
            Anchors.Remove(content);
            Sides.Remove(content);
        }

        private static void RepositionAll()
        {
            if (_layer == null) return;
            foreach (FrameworkElement content in _layer.Children)
                if (content.Visibility == Visibility.Visible && Anchors.ContainsKey(content)) Position(content);
        }

        private static void Position(FrameworkElement content)
        {
            FrameworkElement anchor;
            PopoverSide side;
            if (!Anchors.TryGetValue(content, out anchor) || !Sides.TryGetValue(content, out side)) return;
            Canvas layer = _layer;
            Rect a = anchor.TransformToVisual(layer).TransformBounds(
                new Rect(0, 0, anchor.ActualWidth, anchor.ActualHeight));
            double w = content.ActualWidth, h = content.ActualHeight;
            double width = layer.ActualWidth > 0 ? layer.ActualWidth : Application.Current.MainWindow.ActualWidth;
            double height = layer.ActualHeight > 0 ? layer.ActualHeight : Application.Current.MainWindow.ActualHeight;
            double x, y;
            if (side == PopoverSide.BelowRight)
            {
                x = a.Right - w;
                y = a.Bottom + 6;
                if (y + h > height - 8 && a.Top - 6 - h > 8) y = a.Top - 6 - h;
            }
            else
            {
                if (side == PopoverSide.Top) { x = a.Left + (a.Width - w) / 2; y = a.Top - 9 - h; }
                else if (side == PopoverSide.Bottom) { x = a.Left + (a.Width - w) / 2; y = a.Bottom + 9; }
                else if (side == PopoverSide.Left) { x = a.Left - w - 9; y = a.Top + (a.Height - h) / 2; }
                else { x = a.Right + 9; y = a.Top + (a.Height - h) / 2; }
            }
            x = Math.Max(8, Math.Min(x, width - w - 8));
            y = Math.Max(8, Math.Min(y, height - h - 8));
            Canvas.SetLeft(content, x);
            Canvas.SetTop(content, y);
        }

        // ---- Tooltip attached properties (aria-describedby equivalent via HelpText) ----
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text", typeof(string), typeof(Popover), new PropertyMetadata(null, OnTipChanged));
        public static readonly DependencyProperty SideProperty = DependencyProperty.RegisterAttached(
            "Side", typeof(PopoverSide), typeof(Popover), new PropertyMetadata(PopoverSide.Top));
        public static readonly DependencyProperty DismissedProperty = DependencyProperty.RegisterAttached(
            "Dismissed", typeof(bool), typeof(Popover), new PropertyMetadata(false));

        public static void SetText(DependencyObject o, string value) { o.SetValue(TextProperty, value); }
        public static string GetText(DependencyObject o) { return (string)o.GetValue(TextProperty); }
        public static void SetSide(DependencyObject o, PopoverSide value) { o.SetValue(SideProperty, value); }
        public static PopoverSide GetSide(DependencyObject o) { return (PopoverSide)o.GetValue(SideProperty); }
        public static void SetDismissed(DependencyObject o, bool value) { o.SetValue(DismissedProperty, value); }
        public static bool GetDismissed(DependencyObject o) { return (bool)o.GetValue(DismissedProperty); }

        private static void OnTipChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Button host = o as Button;
            if (host == null || e.NewValue == null) return;
            host.MouseEnter += delegate { TipShow(host); };
            host.MouseLeave += delegate { TipMaybeHide(host); };
            host.GotKeyboardFocus += delegate { TipShow(host); };
            host.LostKeyboardFocus += delegate { TipMaybeHide(host); };
            host.KeyDown += delegate(object sender, KeyEventArgs args)
            {
                if (args.Key == Key.Escape) { SetDismissed(host, true); TipHide(host, false); args.Handled = true; }
            };
        }

        private static void TipShow(Button host)
        {
            if (GetDismissed(host)) return;
            DispatcherTimer pending;
            if (HideTimers.TryGetValue(host, out pending)) pending.Stop();
            if (_activeTip != null && _activeTip != host) TipHide(_activeTip, false);
            GroupBox bubble;
            if (!Tips.TryGetValue(host, out bubble))
            {
                bubble = new GroupBox();
                bubble.Style = (Style)host.FindResource("TipBubble");
                bubble.SetValue(AutomationProperties.AutomationIdProperty, "TipBubble");
                bubble.SetValue(AutomationProperties.NameProperty, GetText(host));
                bubble.Content = new TextBlock { Text = GetText(host), Style = (Style)host.FindResource("TipBubbleText") };
                bubble.Visibility = Visibility.Collapsed;
                bubble.MouseLeave += delegate { TipMaybeHide(host); };
                Tips[host] = bubble;
            }
            Show(host, bubble, GetSide(host));
            _activeTip = host;
        }

        // Hide only when neither the host nor the (hoverable) bubble still owns pointer/focus.
        // The timer is the 9px hover bridge: pointer travel through the gap cancels the hide.
        private static void TipMaybeHide(Button host)
        {
            GroupBox bubble;
            if (!Tips.TryGetValue(host, out bubble)) return;
            if (host.IsKeyboardFocusWithin || host.IsMouseOver || bubble.IsMouseOver) return;
            DispatcherTimer timer;
            if (!HideTimers.TryGetValue(host, out timer))
            {
                timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(150);
                timer.Tick += delegate(object sender, EventArgs e)
                {
                    ((DispatcherTimer)sender).Stop();
                    GroupBox b;
                    if (Tips.TryGetValue(host, out b)
                        && (host.IsKeyboardFocusWithin || host.IsMouseOver || b.IsMouseOver)) return;
                    TipHide(host, true);
                };
                HideTimers[host] = timer;
            }
            timer.Stop();
            timer.Start();
        }

        private static void TipHide(Button host, bool clearDismissed)
        {
            DispatcherTimer pending;
            if (HideTimers.TryGetValue(host, out pending)) pending.Stop();
            if (clearDismissed) SetDismissed(host, false);
            GroupBox bubble;
            if (Tips.TryGetValue(host, out bubble)) Hide(bubble);
            if (_activeTip == host) _activeTip = null;
        }
    }
}
