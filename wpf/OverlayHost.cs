using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace UIComponents.Wpf
{
    // One in-window overlay layer: scrim, Escape, focus save/restore and page blur.
    // The layer always exists (never Collapsed) so --capture resolves it before any ! step.
    public static class OverlayHost
    {
        private static Grid _layer;
        private static Window _window;
        private static Border _scrim;
        private static FrameworkElement _openDialog;
        private static FrameworkElement _tabScope;
        private static IInputElement _previousFocus;
        private static object _savedTabNav = DependencyProperty.UnsetValue;
        private static bool _blocking;
        private static readonly Dictionary<FrameworkElement, Effect> _savedEffects =
            new Dictionary<FrameworkElement, Effect>();

        public static void Install(Window window, Panel dialogStore)
        {
            _layer = new Grid();
            _layer.Name = "OverlayLayer";
            _layer.SetValue(AutomationProperties.AutomationIdProperty, "OverlayLayer");
            Grid.SetColumnSpan(_layer, 2);
            Panel.SetZIndex(_layer, 100);
            // Background stays null when closed so clicks reach the page.

            _scrim = new Border();
            _scrim.Name = "OverlayScrim";
            _scrim.SetValue(AutomationProperties.AutomationIdProperty, "OverlayScrim");
            _scrim.Background = new SolidColorBrush(Color.FromArgb(0x6B, 0x1E, 0x1A, 0x14));
            _scrim.Visibility = Visibility.Collapsed;
            // O3: a mouse-up on the scrim itself closes. The scrim is a sibling of the
            // dialog, so a click on the card never reaches it. No proxy control: that
            // would add an unnamed button to the UIA control view and a11y announcements.
            _scrim.MouseLeftButtonUp += delegate { if (!_blocking) Close(); };
            _layer.Children.Add(_scrim);

            // Move the authored dialogs in once, synchronously: x:Name fields stay valid,
            // and there is no dependency on Loaded.
            while (dialogStore.Children.Count > 0)
            {
                UIElement child = dialogStore.Children[0];
                dialogStore.Children.RemoveAt(0);
                _layer.Children.Add(child);
            }

            _window = window;
            ((Panel)window.Content).Children.Add(_layer);
            // Bubbling, not Preview: the edit drawer's confirm needs to handle Escape first (O4).
            _layer.KeyDown += OverlayLayer_KeyDown;
            // Tab at the window's Preview (handledEventsToo): WPF's own navigation runs on
            // inner elements, so intercept at the root before it moves focus out of the dialog.
            _window.AddHandler(Keyboard.PreviewKeyDownEvent,
                new KeyEventHandler(OverlayWindow_PreviewKeyDown), true);
        }

        public static void Open(FrameworkElement dialog, IInputElement focus, bool blocking)
        {
            // Keep the opener's focus even when one dialog replaces another.
            if (_openDialog == null) _previousFocus = Keyboard.FocusedElement;
            if (_openDialog != null && _openDialog != dialog) Close(false);
            if (_openDialog == dialog) return;

            _openDialog = dialog;
            _blocking = blocking;
            // Disable WPF's window-wide Tab navigation while a dialog owns the keyboard.
            _savedTabNav = _window.ReadLocalValue(KeyboardNavigation.TabNavigationProperty);
            _window.SetValue(KeyboardNavigation.TabNavigationProperty, KeyboardNavigationMode.None);
            dialog.Visibility = Visibility.Visible;
            _scrim.Visibility = Visibility.Visible;
            SetPageBlur(true);

            dialog.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(delegate
            {
                // A GroupBox-rooted dialog has not been measured while Collapsed: apply
                // its template and lay it out before looking for a default focus target.
                dialog.ApplyTemplate();
                dialog.UpdateLayout();
                FrameworkElement target = focus as FrameworkElement;
                if (target == null) target = FirstFocusable(dialog);
                if (target == null) target = dialog;
                target.Focus();
            }));
        }

        public static void Open(FrameworkElement dialog, IInputElement focus)
        {
            Open(dialog, focus, false);
        }

        // O4: the edit drawer's confirm scopes Tab to its own two buttons while visible.
        public static void SetTabScope(FrameworkElement scope)
        {
            _tabScope = scope;
        }

        public static bool IsAnyOpen
        {
            get { return _openDialog != null; }
        }

        public static bool IsOpen(FrameworkElement dialog)
        {
            return _openDialog == dialog;
        }

        public static void Close()
        {
            Close(true);
        }

        private static void Close(bool restoreFocus)
        {
            if (_openDialog == null) return;
            FrameworkElement dialog = _openDialog;
            _openDialog = null;
            _tabScope = null;
            _blocking = false;
            if (_savedTabNav == DependencyProperty.UnsetValue)
                _window.ClearValue(KeyboardNavigation.TabNavigationProperty);
            else
                _window.SetValue(KeyboardNavigation.TabNavigationProperty, _savedTabNav);
            _savedTabNav = DependencyProperty.UnsetValue;
            dialog.Visibility = Visibility.Collapsed;
            _scrim.Visibility = Visibility.Collapsed;
            SetPageBlur(false);

            if (restoreFocus)
            {
                FrameworkElement previous = _previousFocus as FrameworkElement;
                if (previous != null && PresentationSource.FromVisual(previous) != null) previous.Focus();
                _previousFocus = null;
            }
        }

        private static void OverlayLayer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape || _openDialog == null || _blocking) return;
            e.Handled = true;
            Close();
        }

        // Web 2119-2139: cycle Tab inside the open dialog (Shift+Tab is code-only).
        private static void OverlayWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Tab || _openDialog == null) return;
            e.Handled = true;
            List<FrameworkElement> items = new List<FrameworkElement>();
            FrameworkElement scope = _tabScope != null && _tabScope.IsVisible ? _tabScope : _openDialog;
            CollectFocusable(scope, items);
            if (items.Count == 0) { _openDialog.Focus(); return; }

            // Focus can sit on a template inner element (e.g. an editable ComboBox text
            // box). Walk up to the nearest item in the cycle instead of falling back to 0.
            int index = -1;
            DependencyObject node = Keyboard.FocusedElement as DependencyObject;
            while (node != null && index < 0)
            {
                for (int i = 0; i < items.Count; i++)
                    if (items[i] == node) { index = i; break; }
                if (index < 0) node = VisualTreeHelper.GetParent(node);
            }
            bool back = (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.None;
            int next = index < 0 ? (back ? items.Count - 1 : 0)
                : (index + (back ? items.Count - 1 : 1)) % items.Count;
            items[next].Focus();
        }

        private static void CollectFocusable(DependencyObject node, List<FrameworkElement> items)
        {
            int count = VisualTreeHelper.GetChildrenCount(node);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(node, i);
                FrameworkElement fe = child as FrameworkElement;
                // Web's focusable selector: buttons and inputs, not decorative ContentControls.
                if (fe != null && fe.Focusable && fe.IsEnabled && fe.IsVisible &&
                    (fe is ButtonBase || fe is TextBox || fe is ComboBox || fe is PasswordBox)) items.Add(fe);
                CollectFocusable(child, items);
            }
        }

        private static FrameworkElement FirstFocusable(DependencyObject node)
        {
            int count = VisualTreeHelper.GetChildrenCount(node);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(node, i);
                FrameworkElement fe = child as FrameworkElement;
                if (fe != null && fe.Focusable && fe.IsEnabled && (fe is ButtonBase || fe is TextBox))
                    return fe;
                FrameworkElement found = FirstFocusable(child);
                if (found != null) return found;
            }
            return null;
        }

        // Q1: blur the page, never the layer. Restore any effect the page had before.
        private static void SetPageBlur(bool on)
        {
            Panel root = _layer.Parent as Panel;
            if (root == null) return;
            foreach (UIElement child in root.Children)
            {
                if (child == _layer) continue;
                FrameworkElement fe = child as FrameworkElement;
                if (fe == null || AutomationProperties.GetAutomationId(fe) == "ToastLayer") continue;
                if (on)
                {
                    if (!_savedEffects.ContainsKey(fe)) _savedEffects[fe] = fe.Effect;
                    fe.Effect = new BlurEffect { Radius = 9 };
                }
                else
                {
                    Effect saved;
                    if (_savedEffects.TryGetValue(fe, out saved)) fe.Effect = saved;
                    _savedEffects.Remove(fe);
                }
            }
        }
    }
}
