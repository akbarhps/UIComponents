using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
        private bool _working;

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
            _spinner.Loaded += LoadingSpinner_Loaded;
            panel.Children.Add(_spinner);
            TextBlock label = new TextBlock();
            label.Text = text;
            label.Margin = new Thickness(8, 0, 0, 0);
            label.VerticalAlignment = VerticalAlignment.Center;
            panel.Children.Add(label);
            return panel;
        }

        // The web spinner rotates while idle too; captures stay static via IsCapture.
        private void Spin()
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
            DoubleAnimation animation = new DoubleAnimation(0, 360, TimeSpan.FromMilliseconds(700));
            animation.RepeatBehavior = RepeatBehavior.Forever;
            rotate.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void LoadingSpinner_Loaded(object sender, RoutedEventArgs e)
        {
            ContentControl spinner = sender as ContentControl;
            if (spinner != null) _spinner = spinner;
            Spin();
        }

        // B1: the web uses aria-disabled: keep the button focusable/enabled, ignore extra clicks.
        private void BtnLoading_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || _working) return;
            _working = true;
            button.Opacity = 0.45;
            button.Cursor = Cursors.No;
            button.Tag = null; // hide the shadow sibling
            button.Content = BuildLoadingContent("Working…");
            AutomationProperties.SetName(button, "Working…");
            Spin();
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
            _working = false;
            BtnLoading.ClearValue(OpacityProperty);
            BtnLoading.ClearValue(CursorProperty);
            BtnLoading.ClearValue(TagProperty);
            BtnLoading.Content = BuildLoadingContent("Loading");
            AutomationProperties.SetName(BtnLoading, "Loading");
            Spin();
        }

        // ---- Tabs ----
        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton tab = sender as RadioButton;
            if (tab == null || TabPanel == null || TabProfile == null || TabSecurity == null || TabBilling == null) return;
            if (tab == TabSecurity) TabPanel.Text = "Manage your password and two-factor authentication.";
            else if (tab == TabBilling) TabPanel.Text = "View invoices and update your payment method.";
            else TabPanel.Text = "Update your name, photo and public profile details.";
            TabProfile.IsTabStop = TabProfile.IsChecked == true;
            TabSecurity.IsTabStop = TabSecurity.IsChecked == true;
            TabBilling.IsTabStop = TabBilling.IsChecked == true;
        }

        private void Tab_KeyDown(object sender, KeyEventArgs e)
        {
            RadioButton tab = sender as RadioButton;
            if (tab == null) return;
            int i = tab == TabProfile ? 0 : tab == TabSecurity ? 1 : 2;
            int next;
            if (e.Key == Key.Right) next = (i + 1) % 3;
            else if (e.Key == Key.Left) next = (i + 2) % 3;
            else if (e.Key == Key.Home) next = 0;
            else if (e.Key == Key.End) next = 2;
            else return;
            RadioButton target = next == 0 ? TabProfile : next == 1 ? TabSecurity : TabBilling;
            target.IsChecked = true;
            target.Focus();
            e.Handled = true;
        }

        // ---- Breadcrumbs ----
        private void Crumb_Click(object sender, RoutedEventArgs e)
        {
            Button link = sender as Button;
            ScrollViewer scroll = FindName("ContentScroll") as ScrollViewer;
            StackPanel sections = FindName("Sections") as StackPanel;
            if (link == null || scroll == null || sections == null) return;
            string tag = link.Tag == null ? "" : link.Tag.ToString();
            string title = tag == "Buttons" ? "Buttons" : tag == "Cards" ? "Cards" : "Data table";
            foreach (UIElement child in sections.Children)
            {
                TextBlock heading = child as TextBlock;
                if (heading == null || heading.Text != title) continue;
                if (!heading.IsVisible) return; // standalone mode: the target is collapsed
                Rect r = heading.TransformToVisual(scroll).TransformBounds(
                    new Rect(0, 0, heading.ActualWidth, heading.ActualHeight));
                scroll.ScrollToVerticalOffset(Math.Max(0, scroll.VerticalOffset + r.Top - 24));
                return;
            }
        }

        // ---- Stepper ----
        private int _step;

        private void StepPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_step > 0) { _step--; RenderStepper(); }
        }

        private void StepNext_Click(object sender, RoutedEventArgs e)
        {
            if (_step < 3) { _step++; RenderStepper(); }
        }

        private void RenderStepper()
        {
            for (int i = 0; i < 4; i++)
            {
                Border marker = FindName("Step" + i + "_Marker") as Border;
                TextBlock num = FindName("Step" + i + "_Num") as TextBlock;
                FrameworkElement check = FindName("Step" + i + "_Check") as FrameworkElement;
                TextBlock title = FindName("Step" + i + "_Title") as TextBlock;
                Border line = FindName("Step" + i + "_Line") as Border;
                bool done = i < _step, current = i == _step;
                if (marker != null)
                {
                    marker.Background = done ? (Brush)FindResource("Brand")
                        : current ? (Brush)FindResource("BrandSoft") : (Brush)FindResource("Surface");
                    marker.BorderBrush = done || current ? (Brush)FindResource("Brand") : (Brush)FindResource("LineStrong");
                }
                if (num != null)
                {
                    num.Visibility = done ? Visibility.Collapsed : Visibility.Visible;
                    num.Foreground = current ? (Brush)FindResource("Brand") : (Brush)FindResource("Ink3");
                    AutomationProperties.SetItemStatus(num, done ? "done" : current ? "current" : "todo");
                }
                if (check != null)
                {
                    check.Visibility = done ? Visibility.Visible : Visibility.Collapsed;
                    AutomationProperties.SetItemStatus(check, done ? "done" : current ? "current" : "todo");
                }
                if (title != null) title.Foreground = done || current ? (Brush)FindResource("Ink") : (Brush)FindResource("Ink3");
                if (line != null) line.Background = done ? (Brush)FindResource("Brand") : (Brush)FindResource("Line");
            }
            StepStatus.Text = "Step " + (_step + 1) + " of 4";
            // Web reads the active element before disabling; capture focus first (W17 fix).
            bool prevFocused = StepPrev.IsKeyboardFocused, nextFocused = StepNext.IsKeyboardFocused;
            StepPrev.IsEnabled = _step > 0;
            StepNext.IsEnabled = _step < 3;
            if (!StepPrev.IsEnabled && prevFocused) StepNext.Focus();
            if (!StepNext.IsEnabled && nextFocused) StepPrev.Focus();
        }

        // ---- Accordion ----
        private void Accordion_Changed(object sender, RoutedEventArgs e)
        {
            ToggleButton trigger = sender as ToggleButton;
            if (trigger == null || string.IsNullOrEmpty(trigger.Name)) return;
            int cut = trigger.Name.LastIndexOf('_');
            if (cut < 0) return;
            string panelName = trigger.Name.Substring(0, cut + 1) + "Panel" + trigger.Name.Substring(cut + 1);
            FrameworkElement panel = FindName(panelName) as FrameworkElement;
            if (panel != null) panel.Visibility = trigger.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            if (trigger.Name.StartsWith("AccSingle_") && trigger.IsChecked == true)
            {
                for (int i = 0; i < 3; i++)
                {
                    ToggleButton other = FindName("AccSingle_" + i) as ToggleButton;
                    if (other != null && other != trigger) other.IsChecked = false;
                }
            }
        }

        // ---- Action menu ----
        private GroupBox _openPanel;
        private Button _openTrigger;
        private DropdownOutsideClick _outsideActions;
        private DropdownOutsideClick _outsideMore;

        private void MenuTrigger_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Down) return;
            Button trigger = sender as Button;
            GroupBox panel = trigger == MenuActions ? ProjectMenuPanel : CompactMenuPanel;
            if (_openPanel != panel) OpenMenu(trigger, panel);
            e.Handled = true;
        }

        private void MenuActions_Click(object sender, RoutedEventArgs e)
        {
            if (_openPanel == ProjectMenuPanel) CloseMenu(true);
            else OpenMenu(MenuActions, ProjectMenuPanel);
        }

        private void MenuMore_Click(object sender, RoutedEventArgs e)
        {
            if (_openPanel == CompactMenuPanel) CloseMenu(true);
            else OpenMenu(MenuMore, CompactMenuPanel);
        }

        private void OpenMenu(Button trigger, GroupBox panel)
        {
            if (_openPanel != null) CloseMenu(false);
            _openTrigger = trigger;
            _openPanel = panel;
            AutomationProperties.SetItemStatus(trigger, "expanded");
            if (trigger == MenuActions) MenuChevron.LayoutTransform = new RotateTransform(180);
            Popover.Show(trigger, panel, PopoverSide.BelowRight);
            if (trigger == MenuActions)
            {
                if (_outsideActions == null)
                    _outsideActions = new DropdownOutsideClick(trigger, delegate { return _openPanel; }, delegate { CloseMenu(false); });
            }
            else if (_outsideMore == null)
            {
                _outsideMore = new DropdownOutsideClick(trigger, delegate { return _openPanel; }, delegate { CloseMenu(false); });
            }
            DropdownOutsideClick outside = trigger == MenuActions ? _outsideActions : _outsideMore;
            outside.Hook();
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(delegate
            {
                List<Button> items = MenuItems(panel);
                if (items.Count > 0) items[0].Focus();
            }));
        }

        private void CloseMenu(bool restoreFocus)
        {
            if (_openPanel == null) return;
            GroupBox panel = _openPanel;
            Button trigger = _openTrigger;
            if (_outsideActions != null) _outsideActions.Unhook();
            if (_outsideMore != null) _outsideMore.Unhook();
            Popover.Hide(panel);
            AutomationProperties.SetItemStatus(trigger, "collapsed");
            if (trigger == MenuActions) MenuChevron.LayoutTransform = new RotateTransform(0);
            _openPanel = null;
            _openTrigger = null;
            if (restoreFocus) trigger.Focus();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Button item = sender as Button;
            if (item != null && item.Tag != null) MenuResult.Text = item.Tag.ToString();
            CloseMenu(true);
        }

        private static List<Button> MenuItems(DependencyObject node)
        {
            List<Button> items = new List<Button>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(node, i);
                Button button = child as Button;
                if (button != null && button.IsEnabled) items.Add(button);
                else items.AddRange(MenuItems(child));
            }
            return items;
        }

        private void MenuPanel_KeyDown(object sender, KeyEventArgs e)
        {
            GroupBox panel = sender as GroupBox;
            if (panel == null || panel != _openPanel) return;
            List<Button> items = MenuItems(panel);
            if (items.Count == 0) return;
            if (e.Key == Key.Escape) { CloseMenu(true); e.Handled = true; return; }
            if (e.Key == Key.Tab)
            {
                bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
                Button trigger = _openTrigger;
                CloseMenu(shift);
                if (!shift) trigger.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
                return;
            }
            int index = items.IndexOf(Keyboard.FocusedElement as Button);
            if (e.Key == Key.Down || e.Key == Key.Up)
                index = index < 0 ? 0 : (index + (e.Key == Key.Down ? 1 : items.Count - 1)) % items.Count;
            else if (e.Key == Key.Home) index = 0;
            else if (e.Key == Key.End) index = items.Count - 1;
            else return;
            items[index].Focus();
            e.Handled = true;
        }
    }
}
