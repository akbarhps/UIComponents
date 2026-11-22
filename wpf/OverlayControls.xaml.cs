using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Text.RegularExpressions;

namespace UIComponents.Wpf
{
    public partial class OverlayControls : UserControl
    {
        private DispatcherTimer _loadingTimer;

        private sealed class Cmd
        {
            public string Group;
            public string Label;
            public string Icon;
            public string Shortcut;
            public Action Run;
        }
        private List<Cmd> _cmds;
        private List<Cmd> _cmdkMatches = new List<Cmd>();
        private int _cmdkActive;

        public OverlayControls()
        {
            InitializeComponent();
            LoadingDialog.IsVisibleChanged += delegate
            {
                if (!LoadingDialog.IsVisible) { StopLoadingTimer(); StopLoadingMotion(); }
            };
            // ClipToBounds alone clips to a rectangle: round the animated bar's ends too.
            LoadingBar.SizeChanged += delegate(object s, SizeChangedEventArgs e)
            {
                LoadingBar.Clip = new System.Windows.Media.RectangleGeometry(
                    new Rect(0, 0, e.NewSize.Width, e.NewSize.Height), 2, 2);
            };
            CmdkCard.SizeChanged += delegate(object s, SizeChangedEventArgs e)
            {
                CmdkCard.Clip = new System.Windows.Media.RectangleGeometry(
                    new Rect(0, 0, e.NewSize.Width, e.NewSize.Height), 15, 15);
            };
            _cmds = BuildCommands();
            // TextBox marks arrow keys handled in its class handler; handledEventsToo keeps
            // the palette's navigation working.
            CmdkInput.AddHandler(Keyboard.KeyDownEvent, new KeyEventHandler(CmdkInput_KeyDown), true);
            Application.Current.MainWindow.PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        private List<Cmd> BuildCommands()
        {
            var list = new List<Cmd>();
            list.Add(new Cmd { Group = "Actions", Label = "Create a project", Icon = "IconPlus", Shortcut = "Ctrl N",
                Run = delegate { Toasts.Show("success", "Create project", "A new project draft was started."); } });
            list.Add(new Cmd { Group = "Actions", Label = "Invite a member", Icon = "IconMore",
                Run = delegate { Toasts.Show("info", "Invite member", "Opening the invite form."); } });
            list.Add(new Cmd { Group = "Actions", Label = "Export workspace", Icon = "IconDownload",
                Run = delegate { Toasts.Show("info", "Export", "Preparing your export."); } });
            string[] navTitles = { "Buttons", "Data table", "Grid input", "Date picker" };
            for (int i = 0; i < navTitles.Length; i++)
            {
                string title = navTitles[i];
                list.Add(new Cmd { Group = "Navigation", Label = "Go to " + title,
                    Icon = i == 0 ? "IconButtonGlyph" : (i == 3 ? "IconCalendar" : "IconTable"),
                    Run = delegate { GoToSection(title); } });
            }
            // Q3: WPF has no collapsible sidebar; keep the item as a no-op (web parity).
            list.Add(new Cmd { Group = "Appearance", Label = "Toggle sidebar", Icon = "IconPanel", Shortcut = "Ctrl \\",
                Run = delegate { } });
            return list;
        }

        private void GoToSection(string title)
        {
            // Only the content column: the sidebar has a label with the same text.
            FrameworkElement scroll = Application.Current.MainWindow.FindName("ContentScroll") as FrameworkElement;
            FrameworkElement target = FindByTitle(scroll ?? Application.Current.MainWindow, title);
            if (target != null) target.BringIntoView();
        }

        private FrameworkElement FindByTitle(DependencyObject node, string title)
        {
            Style titleStyle = TryFindResource("SectionTitle") as Style;
            TextBlock text = node as TextBlock;
            if (text != null && text.Text == title && (titleStyle == null || text.Style == titleStyle)) return text;
            int count = VisualTreeHelper.GetChildrenCount(node);
            for (int i = 0; i < count; i++)
            {
                FrameworkElement found = FindByTitle(VisualTreeHelper.GetChild(node, i), title);
                if (found != null) return found;
            }
            return null;
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.K || Keyboard.Modifiers != ModifierKeys.Control) return;
            e.Handled = true;
            if (OverlayHost.IsAnyOpen) return;
            OpenCmdk();
        }

        private void OpenCmdk_Click(object sender, RoutedEventArgs e)
        {
            OpenCmdk();
        }

        private void OpenCmdk()
        {
            CmdkInput.Text = "";
            RenderCmdk(); // empty -> empty does not raise TextChanged
            OverlayHost.Open(CmdkDialog, CmdkInput);
        }

        private void CmdkInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CmdkItems == null) return; // XAML load
            RenderCmdk();
        }

        private void RenderCmdk()
        {
            string query = CmdkInput == null ? "" : CmdkInput.Text.Trim().ToLower();
            _cmdkMatches.Clear();
            foreach (Cmd c in _cmds)
                if (c.Label.ToLower().Contains(query)) _cmdkMatches.Add(c);
            _cmdkActive = 0;
            CmdkItems.Children.Clear();
            double content = 16; // list padding
            if (_cmdkMatches.Count == 0)
            {
                var empty = new TextBlock();
                empty.Text = "No matching commands";
                empty.Style = (Style)TryFindResource("ModalBody");
                empty.Height = 78.4;
                empty.Padding = new Thickness(16, 28, 16, 28);
                empty.TextAlignment = TextAlignment.Center;
                empty.SetValue(AutomationProperties.AutomationIdProperty, "CmdkEmpty");
                CmdkItems.Children.Add(empty);
                content += 78.4;
            }
            else
            {
                string group = null;
                for (int i = 0; i < _cmdkMatches.Count; i++)
                {
                    Cmd c = _cmdkMatches[i];
                    if (c.Group != group)
                    {
                        group = c.Group;
                        var head = new TextBlock();
                        head.Text = group;
                        head.Style = (Style)TryFindResource("CmdkGroup");
                        head.SetValue(AutomationProperties.AutomationIdProperty, "CmdkGroup" + i);
                        CmdkItems.Children.Add(head);
                        content += 29.6;
                    }
                    Button item = new Button();
                    item.Style = (Style)TryFindResource("CmdkItem");
                    item.Height = c.Shortcut == null ? 39 : 41;
                    item.SetValue(AutomationProperties.AutomationIdProperty, "CmdkItem" + i);
                    item.SetValue(AutomationProperties.NameProperty, c.Label);
                    item.SetValue(AutomationProperties.ItemStatusProperty, i == 0 ? "active" : "");
                    if (i == 0) item.Tag = "active";
                    var row = new Grid();
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(11) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    var icon = new ContentControl { Style = (Style)TryFindResource(c.Icon),
                        Foreground = (Brush)TryFindResource("Ink3"), Width = 16, Height = 16 };
                    Grid.SetColumn(icon, 0);
                    var label = new TextBlock { Text = c.Label, FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
                    Grid.SetColumn(label, 2);
                    row.Children.Add(icon);
                    row.Children.Add(label);
                    if (c.Shortcut != null)
                    {
                        var chip = new Border { Style = (Style)TryFindResource("CmdkChip"),
                            VerticalAlignment = VerticalAlignment.Center };
                        chip.Child = new TextBlock { Text = c.Shortcut, FontSize = 11,
                            Foreground = (Brush)TryFindResource("Ink3") };
                        Grid.SetColumn(chip, 3);
                        chip.HorizontalAlignment = HorizontalAlignment.Right;
                        row.Children.Add(chip);
                    }
                    item.Content = row;
                    Cmd selected = c;
                    item.Click += delegate { RunCmdk(selected); };
                    CmdkItems.Children.Add(item);
                    content += item.Height;
                }
            }
            double listHeight = Math.Min(340, content);
            CmdkList.MaxHeight = 340;
            CmdkCard.Height = CmdkCardRoot.Height = 96.6 + listHeight;
            UpdateActive();
        }

        private void UpdateActive()
        {
            for (int i = 0; i < CmdkItems.Children.Count; i++)
            {
                Button b = CmdkItems.Children[i] as Button;
                if (b == null) continue;
                int index = -1;
                string id = b.GetValue(AutomationProperties.AutomationIdProperty) as string;
                if (id != null && id.StartsWith("CmdkItem")) int.TryParse(id.Substring(8), out index);
                bool active = index == _cmdkActive;
                b.Tag = active ? "active" : null;
                b.SetValue(AutomationProperties.ItemStatusProperty, active ? "active" : "");
            }
        }

        private void CmdkInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                e.Handled = true;
                if (_cmdkMatches.Count == 0) return;
                int step = e.Key == Key.Down ? 1 : _cmdkMatches.Count - 1;
                _cmdkActive = (_cmdkActive + step) % _cmdkMatches.Count;
                UpdateActive();
                string id = "CmdkItem" + _cmdkActive;
                FrameworkElement item = FindById(CmdkDialog, id);
                if (item != null) item.BringIntoView();
            }
            else if (e.Key == Key.Return)
            {
                e.Handled = true;
                if (_cmdkMatches.Count > 0) RunCmdk(_cmdkMatches[_cmdkActive]);
            }
        }

        private void RunCmdk(Cmd c)
        {
            OverlayHost.Close();
            c.Run();
        }

        private static FrameworkElement FindById(DependencyObject node, string id)
        {
            FrameworkElement fe = node as FrameworkElement;
            if (fe != null && (AutomationProperties.GetAutomationId(fe) == id || fe.Name == id)) return fe;
            int count = VisualTreeHelper.GetChildrenCount(node);
            for (int i = 0; i < count; i++)
            {
                FrameworkElement found = FindById(VisualTreeHelper.GetChild(node, i), id);
                if (found != null) return found;
            }
            return null;
        }

        private static bool EmailOk(string value)
        {
            return Regex.IsMatch(value.Trim(), @"^[^\s@]+@[^\s@]+\.[^\s@]+$");
        }

        // D1-D3: invite drawer.
        private void OpenDrawer_Click(object sender, RoutedEventArgs e)
        {
            DrEmail.Text = "";
            DrRole.SelectedIndex = 0;
            DrNote.Text = "";
            DrSendEmail.IsChecked = true;
            SetFormError(DrEmail, DrEmailErrorRow, false);
            OverlayHost.Open(DrawerInvite, DrEmail);
        }

        private void DrEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetFormError(DrEmail, DrEmailErrorRow, false);
        }

        private void DrEmail_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;
            e.Handled = true;
            SubmitInvite();
        }

        private void DrawerSubmit_Click(object sender, RoutedEventArgs e)
        {
            SubmitInvite();
        }

        private void SubmitInvite()
        {
            string email = DrEmail.Text.Trim();
            SetFormError(DrEmail, DrEmailErrorRow, !EmailOk(email));
            if (!EmailOk(email)) { DrEmail.Focus(); return; }
            OverlayHost.Close();
            Toasts.Show("success", "Invite sent", "An invitation is on its way to " + email + ".");
            DrEmail.Text = "";
            DrRole.SelectedIndex = 0;
            DrNote.Text = "";
            DrSendEmail.IsChecked = true;
        }

        private void DrawerClose_Click(object sender, RoutedEventArgs e) { OverlayHost.Close(); }
        private void DrawerCancel_Click(object sender, RoutedEventArgs e) { OverlayHost.Close(); }

        // D4-D11: edit drawer and its confirm step.
        private void OpenEditDrawer_Click(object sender, RoutedEventArgs e)
        {
            EdName.Text = "Jamie Rivera";
            EdEmail.Text = "jamie@example.com";
            EdRole.SelectedIndex = 0;
            EdActive.IsChecked = true;
            SetFormError(EdEmail, EdEmailErrorRow, false);
            EditConfirm.Visibility = Visibility.Collapsed;
            OverlayHost.SetTabScope(null);
            OverlayHost.Open(DrawerEdit, EdName);
        }

        private void EdEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetFormError(EdEmail, EdEmailErrorRow, false);
        }

        private void EdEmail_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;
            e.Handled = true;
            SaveEdit();
        }

        private void EditSave_Click(object sender, RoutedEventArgs e) { SaveEdit(); }

        private void SaveEdit()
        {
            string email = EdEmail.Text.Trim();
            SetFormError(EdEmail, EdEmailErrorRow, !EmailOk(email));
            if (!EmailOk(email)) { EdEmail.Focus(); return; }
            OverlayHost.Close();
            Toasts.Show("success", "Changes saved", "Jamie's details have been updated.");
        }

        private void EditDelete_Click(object sender, RoutedEventArgs e)
        {
            EditConfirm.Visibility = Visibility.Visible;
            OverlayHost.SetTabScope(EditConfirm);
            KeepMember.Focus();
        }

        private void KeepMember_Click(object sender, RoutedEventArgs e)
        {
            EditConfirm.Visibility = Visibility.Collapsed;
            OverlayHost.SetTabScope(null);
            EditDelete.Focus();
        }

        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            EditConfirm.Visibility = Visibility.Collapsed;
            OverlayHost.SetTabScope(null);
            OverlayHost.Close();
            Toasts.Show("success", "Member deleted", "Jamie no longer has access.");
        }

        // D10: Escape on the confirm hides only the confirm, before the layer sees it.
        private void DrawerEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape || EditConfirm.Visibility != Visibility.Visible) return;
            e.Handled = true;
            EditConfirm.Visibility = Visibility.Collapsed;
            OverlayHost.SetTabScope(null);
            EditDelete.Focus();
        }

        private void EditDrawerClose_Click(object sender, RoutedEventArgs e) { OverlayHost.Close(); }
        private void EditCancel_Click(object sender, RoutedEventArgs e) { OverlayHost.Close(); }

        private void OpenFormModal_Click(object sender, RoutedEventArgs e)
        {
            ResetFormModal();
            OverlayHost.Open(FormModalDialog, FmName);
        }

        private void ResetFormModal()
        {
            FmName.Text = "";
            FmEmail.Text = "";
            FmRole.SelectedIndex = 0;
            FmDesc.Text = "";
            SetFormError(FmName, FmNameErrorRow, false);
            SetFormError(FmEmail, FmEmailErrorRow, false);
        }

        private void SetFormError(TextBox input, StackPanel row, bool on)
        {
            // XAML sets Text on EdEmail during load, before its error-row field is assigned.
            if (row == null) return;
            row.Visibility = on ? Visibility.Visible : Visibility.Collapsed;
            input.Style = (Style)(on ? TryFindResource("FieldInputError") : TryFindResource("FieldInput"));
            input.SetValue(AutomationProperties.ItemStatusProperty, on ? "invalid" : "");
            input.SetValue(AutomationProperties.HelpTextProperty,
                on ? (input == FmName ? "Give the project a name." : "Enter a valid email address.") : "");
        }

        private void FmField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender == FmName) SetFormError(FmName, FmNameErrorRow, false);
            else if (sender == FmEmail) SetFormError(FmEmail, FmEmailErrorRow, false);
        }

        private void FmField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;
            e.Handled = true;
            SubmitFormModal();
        }

        private void FormModalSubmit_Click(object sender, RoutedEventArgs e)
        {
            SubmitFormModal();
        }

        private void SubmitFormModal()
        {
            string name = FmName.Text.Trim();
            string email = FmEmail.Text.Trim();
            bool nameOk = name.Length > 0;
            bool emailOk = EmailOk(email);
            SetFormError(FmName, FmNameErrorRow, !nameOk);
            SetFormError(FmEmail, FmEmailErrorRow, !emailOk);
            // W9: focus the first invalid field.
            if (!nameOk) { FmName.Focus(); return; }
            if (!emailOk) { FmEmail.Focus(); return; }
            OverlayHost.Close();
            Toasts.Show("success", "Project created", "“" + name + "” is ready.");
            ResetFormModal();
        }

        private void FormModalClose_Click(object sender, RoutedEventArgs e)
        {
            OverlayHost.Close();
        }

        private void FormModalCancel_Click(object sender, RoutedEventArgs e)
        {
            OverlayHost.Close();
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
            StopLoadingTimer();
            OverlayHost.Open(LoadingDialog, LoadingDialog, true);
            StartLoadingMotion();
            // The 3s close is functional, but a capture must not race it.
            if (IsCapture()) return;
            _loadingTimer = new DispatcherTimer();
            _loadingTimer.Interval = TimeSpan.FromMilliseconds(3000);
            _loadingTimer.Tick += LoadingTimer_Tick;
            _loadingTimer.Start();
        }

        private void StopLoadingTimer()
        {
            if (_loadingTimer == null) return;
            _loadingTimer.Stop();
            _loadingTimer.Tick -= LoadingTimer_Tick;
            _loadingTimer = null;
        }

        private void LoadingTimer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer timer = sender as DispatcherTimer;
            if (timer != null) timer.Stop();
            if (_loadingTimer == timer) StopLoadingTimer();
            if (!OverlayHost.IsOpen(LoadingDialog)) return;
            StopLoadingMotion();
            OverlayHost.Close();
            Toasts.Show("success", "Ready", "Your workspace is ready.");
        }

        private static bool IsCapture()
        {
            foreach (string arg in Environment.GetCommandLineArgs())
                if (arg == "--capture") return true;
            return false;
        }

        // Decision 9: motion is functional, but never run it for --capture or reduced motion.
        private static bool MotionAllowed()
        {
            return !IsCapture() && SystemParameters.ClientAreaAnimation;
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
