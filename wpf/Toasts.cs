using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace UIComponents.Wpf
{
    // Global toast host: one in-window layer at z 200, newest toast at the bottom.
    public static class Toasts
    {
        private sealed class ToastState
        {
            public DispatcherTimer Timer;
            public double Remaining = 3600;
            public DateTime Started;
            public FrameworkElement PreviousFocus;
        }

        private static StackPanel _stack;
        private static int _nextId = 1;
        private static readonly Dictionary<GroupBox, ToastState> _states = new Dictionary<GroupBox, ToastState>();

        public static void Install(Window window)
        {
            Grid layer = new Grid();
            layer.Name = "ToastLayer";
            layer.SetValue(AutomationProperties.AutomationIdProperty, "ToastLayer");
            Grid.SetColumnSpan(layer, 2);
            Panel.SetZIndex(layer, 200);

            _stack = new StackPanel();
            _stack.Name = "ToastStack";
            _stack.SetValue(AutomationProperties.AutomationIdProperty, "ToastStack");
            _stack.Width = 360;
            _stack.HorizontalAlignment = HorizontalAlignment.Right;
            _stack.VerticalAlignment = VerticalAlignment.Bottom;
            // Children carry 5px top/bottom (10px between toasts); 19 + 5 = 24 from the layer edge.
            _stack.Margin = new Thickness(0, 0, 24, 19);

            layer.Children.Add(_stack);
            ((Panel)window.Content).Children.Add(layer);
        }

        public static Border Show(string type, string title, string text)
        {
            return Show(type, title, text, false, null, false);
        }

        public static Border Show(string type, string title, string text, bool dismissible, string action, bool persistent)
        {
            string accentKey = type == "error" ? "Danger" : (type == "info" ? "Info" : "Success");
            string bgKey = type == "error" ? "DangerSoft" : (type == "info" ? "InfoSoft" : "SuccessSoft");
            string borderKey = type == "error" ? "DangerLine" : (type == "info" ? "InfoLine" : "SuccessLine");
            string iconKey = type == "error" ? "IconClose" : (type == "info" ? "IconInfo" : "IconCheck");

            ResourceDictionary res = Application.Current.Resources;
            string id = "Toast" + _nextId++;

            GroupBox root = new GroupBox();
            root.Style = (Style)res["ToastRoot"];
            root.SetValue(AutomationProperties.AutomationIdProperty, id);
            root.SetValue(AutomationProperties.NameProperty, title);
            root.SetValue(AutomationProperties.LiveSettingProperty,
                type == "error" ? AutomationLiveSetting.Assertive : AutomationLiveSetting.Polite);
            root.Margin = new Thickness(0, 5, 0, 5);

            Grid card = new Grid { Width = 360 };
            Border shadow = new Border { Style = (Style)res["ToastShadow"] };
            shadow.SetResourceReference(Border.BackgroundProperty, bgKey);
            Border content = new Border
            {
                Style = (Style)res["ToastCard"],
                Background = Brushes.Transparent
            };
            content.SetResourceReference(Border.BorderBrushProperty, borderKey);

            StackPanel row = new StackPanel { Orientation = Orientation.Horizontal };
            Border circle = new Border { Style = (Style)res["ToastIconCircle"] };
            ContentControl icon = new ContentControl
            {
                Style = (Style)res[iconKey], Width = 14, Height = 14
            };
            icon.SetResourceReference(Control.ForegroundProperty, accentKey);
            circle.Child = icon;
            row.Children.Add(circle);

            StackPanel body = new StackPanel
            {
                Width = (dismissible || persistent) ? 262 : 291,
                Margin = new Thickness(11, 0, 0, 0)
            };
            TextBlock titleText = new TextBlock { Style = (Style)res["ToastTitle"], Text = title };
            titleText.SetResourceReference(TextBlock.ForegroundProperty, accentKey);
            body.Children.Add(titleText);
            body.Children.Add(new TextBlock { Style = (Style)res["ToastText"], Text = text });
            if (action != null)
            {
                Button actionButton = new Button
                {
                    Style = (Style)res["ToastAction"],
                    Content = action,
                    Margin = new Thickness(0, 6, 0, 0)
                };
                actionButton.SetValue(AutomationProperties.AutomationIdProperty, id + "_Action");
                actionButton.Click += delegate { Show("success", "Done", "The action was completed."); Dismiss(root); };
                body.Children.Add(actionButton);
            }
            row.Children.Add(body);

            Button closeButton = null;
            if (dismissible || persistent)
            {
                closeButton = new Button { Style = (Style)res["ToastClose"], Margin = new Thickness(11, 0, 0, 0) };
                ContentControl closeIcon = new ContentControl
                {
                    Style = (Style)res["IconClose"], Width = 14, Height = 14
                };
                closeIcon.SetResourceReference(Control.ForegroundProperty, "Ink3");
                closeButton.Content = closeIcon;
                closeButton.SetValue(AutomationProperties.AutomationIdProperty, id + "_Close");
                closeButton.SetValue(AutomationProperties.NameProperty, "Dismiss");
                closeButton.Click += delegate { Dismiss(root); };
                row.Children.Add(closeButton);
            }

            content.Child = row;
            card.Children.Add(shadow);
            card.Children.Add(content);
            root.Content = card;
            root.Tag = closeButton;
            _stack.Children.Add(root);

            ToastState state = new ToastState();
            state.PreviousFocus = Keyboard.FocusedElement as FrameworkElement;
            _states[root] = state;

            root.MouseEnter += delegate { Pause(root); };
            root.MouseLeave += delegate { Schedule(root); };
            root.GotKeyboardFocus += delegate { Pause(root); };
            // IsKeyboardFocusWithin is still true during the event; re-check after focus settles.
            root.LostKeyboardFocus += delegate
            {
                root.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(delegate
                {
                    if (!root.IsKeyboardFocusWithin) Schedule(root);
                }));
            };

            // W10: action toasts persist; only plain and persistent-less toasts auto-dismiss.
            if (!persistent && action == null)
            {
                state.Timer = new DispatcherTimer();
                state.Timer.Tick += delegate { Dismiss(root); };
                Schedule(root);
            }

            root.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(delegate
            {
                AutomationPeer peer = UIElementAutomationPeer.FromElement(root) ?? UIElementAutomationPeer.CreatePeerForElement(root);
                peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
            }));

            return content;
        }

        private static void Schedule(GroupBox root)
        {
            ToastState state;
            if (!_states.TryGetValue(root, out state) || state.Timer == null || state.Timer.IsEnabled) return;
            if (root.IsMouseOver || root.IsKeyboardFocusWithin) return;
            state.Timer.Interval = TimeSpan.FromMilliseconds(state.Remaining);
            state.Started = DateTime.Now;
            state.Timer.Start();
        }

        private static void Pause(GroupBox root)
        {
            ToastState state;
            if (!_states.TryGetValue(root, out state) || state.Timer == null || !state.Timer.IsEnabled) return;
            state.Timer.Stop();
            state.Remaining = Math.Max(0, state.Remaining - (DateTime.Now - state.Started).TotalMilliseconds);
        }

        private static void Dismiss(GroupBox root)
        {
            ToastState state;
            if (!_states.TryGetValue(root, out state)) return;
            if (state.Timer != null) state.Timer.Stop();
            if (root.IsKeyboardFocusWithin) RestoreFocus(root, state);
            _stack.Children.Remove(root);
            _states.Remove(root);
        }

        // web 3441-3444: the immediately next sibling's close button, then the previous focus.
        private static void RestoreFocus(GroupBox root, ToastState state)
        {
            int index = _stack.Children.IndexOf(root);
            FrameworkElement target = null;
            if (index >= 0 && index + 1 < _stack.Children.Count)
            {
                GroupBox next = _stack.Children[index + 1] as GroupBox;
                if (next != null) target = next.Tag as Button;
            }
            if (target == null && state.PreviousFocus != null && PresentationSource.FromVisual(state.PreviousFocus) != null)
                target = state.PreviousFocus;
            if (target != null) target.Focus();
            else Keyboard.ClearFocus();
        }
    }
}
