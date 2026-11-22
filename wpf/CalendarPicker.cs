using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace UIComponents.Wpf
{
    // Code-only control (like SearchableSelect): the trigger is the only visual child; the
    // panel is built on demand and hosted in an Adorner so it is capturable and UIA-reachable.
    public class CalendarPicker : Grid
    {
        private static readonly string[] Months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        private static readonly string[] MonthsFull = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        private static readonly string[] Weekdays = { "S", "M", "T", "W", "T", "F", "S" };

        private sealed class Selection
        {
            public int Year;
            public int Month;
            public int Day;
            // ponytail: hour/minute/second are P3's time mode; date-only modes ignore them.
            public int Hour;
            public int Minute;
            public int Second;
        }

        private string _id;
        private DateTime _today;
        private int _viewYear;
        private int _viewMonth;
        private Selection _selected;
        private ToggleButton _trigger;
        private TextBlock _value;
        private GroupBox _panelRoot;
        private TextBlock _title;
        private Grid _bodyHost;
        private Adorner _adorner;
        private DropdownOutsideClick _outsideClick;
        private readonly Dictionary<Button, Selection> _cellMap = new Dictionary<Button, Selection>();
        private Button _focusCellOnOpen;
        private bool _closing;

        public string Mode { get; set; }
        public bool WithTime { get; set; }
        public bool WithSeconds { get; set; }
        public string Icon { get; set; }
        public string Placeholder { get; set; }
        public string Label { get; set; }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _id = AutomationProperties.GetAutomationId(this);
            if (string.IsNullOrEmpty(_id)) _id = Name;
            _today = DateTime.Today;
            _viewYear = _today.Year;
            _viewMonth = _today.Month - 1;
            BuildTrigger();
            Unloaded += Control_Unloaded;
        }

        private static object Res(string key)
        {
            return Application.Current.FindResource(key);
        }

        // 1px border + 13px padding + 16px icon + 9px gap = value at +39; the 46px trigger
        // centres a 21px line, putting the icon at y 15 and the value's line box at y 12.5.
        private void BuildTrigger()
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(9) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            ContentControl icon = new ContentControl();
            icon.Style = (Style)Res(Icon == "Clock" ? "IconClock" : "IconCalendar");
            icon.Width = 16;
            icon.Height = 16;
            icon.Foreground = (Brush)Res("Ink3");
            icon.IsHitTestVisible = false;
            grid.Children.Add(icon);

            _value = new TextBlock();
            _value.Text = Placeholder;
            _value.FontFamily = (FontFamily)Res("Sans");
            _value.FontSize = 15;
            _value.LineHeight = 21;
            _value.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            _value.Foreground = (Brush)Res("Ink3");
            _value.TextTrimming = TextTrimming.CharacterEllipsis;
            _value.TextWrapping = TextWrapping.NoWrap;
            _value.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(_value, 2);
            AutomationProperties.SetAutomationId(_value, _id + "_Value");
            grid.Children.Add(_value);

            _trigger = new ToggleButton();
            _trigger.Style = (Style)Res("PickerTrigger");
            _trigger.Content = grid;
            AutomationProperties.SetAutomationId(_trigger, _id + "_Trigger");
            AutomationProperties.SetName(_trigger, Label);
            _trigger.Checked += Trigger_Checked;
            _trigger.Unchecked += Trigger_Unchecked;
            _trigger.KeyDown += Picker_KeyDown;
            _trigger.LostKeyboardFocus += Picker_LostKeyboardFocus;
            Children.Add(_trigger);
        }

        private void Trigger_Checked(object sender, RoutedEventArgs e)
        {
            Open();
        }

        private void Trigger_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_closing) return;
            Close(true); // web: closing from the trigger restores focus
        }

        private void Open()
        {
            if (_adorner != null) return;
            BuildPanel();
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(_trigger);
            if (layer == null) { _trigger.IsChecked = false; return; }
            _adorner = new DropdownAdorner(_trigger, _panelRoot);
            layer.Add(_adorner);
            _outsideClick = new DropdownOutsideClick(this, delegate { return (DependencyObject)_panelRoot; },
                delegate { Close(false); });
            _outsideClick.Hook();
            // Web fix: the panel stays open while focus is anywhere inside the picker root.
            // focusout (Tab included) closes only when focus leaves root; no focus restore.
            Button cell = FindSelectedCell();
            if (cell != null)
            {
                // The adorner isn't connected until layout, and UIA can set focus after the
                // Checked handler returns. Focus on LayoutUpdated and once more on a short timer.
                _focusCellOnOpen = cell;
                Button focused = cell;
                DispatcherTimer focusTimer = new DispatcherTimer();
                focusTimer.Interval = TimeSpan.FromMilliseconds(100);
                focusTimer.Tick += delegate(object sender2, EventArgs e2)
                {
                    ((DispatcherTimer)sender2).Stop();
                    if (_adorner != null && !focused.IsKeyboardFocused) focused.Focus();
                };
                focusTimer.Start();
            }
        }

        private void Close(bool restoreFocus)
        {
            if (_closing) return;
            _closing = true;
            try
            {
                if (_outsideClick != null) { _outsideClick.Unhook(); _outsideClick = null; }
                if (_adorner != null)
                {
                    AdornerLayer layer = AdornerLayer.GetAdornerLayer(_trigger);
                    if (layer != null) layer.Remove(_adorner);
                    _adorner = null;
                }
                _panelRoot = null;
                _title = null;
                _bodyHost = null;
                _focusCellOnOpen = null;
                _cellMap.Clear();
                if (_trigger.IsChecked == true) _trigger.IsChecked = false;
                if (restoreFocus) _trigger.Focus();
            }
            finally { _closing = false; }
        }

        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private Button FindSelectedCell()
        {
            foreach (KeyValuePair<Button, Selection> pair in _cellMap)
                if (IsSelected(pair.Value)) return pair.Key;
            return null;
        }

        private bool IsSelected(Selection value)
        {
            return _selected != null && value.Year == _selected.Year && value.Month == _selected.Month && value.Day == _selected.Day;
        }

        private bool IsToday(int day)
        {
            return _today.Year == _viewYear && _today.Month == _viewMonth + 1 && _today.Day == day;
        }

        private string DialogLabel()
        {
            if (Mode == "month") return "Choose a month";
            if (Mode == "year") return "Choose a year";
            return WithTime ? "Choose a date and time" : "Choose a date";
        }

        private void BuildPanel()
        {
            _panelRoot = new GroupBox();
            _panelRoot.Style = (Style)Res("PickerPanelRoot");
            AutomationProperties.SetAutomationId(_panelRoot, _id + "_Panel");
            AutomationProperties.SetName(_panelRoot, DialogLabel());
            _panelRoot.KeyDown += Picker_KeyDown;
            _panelRoot.LostKeyboardFocus += Picker_LostKeyboardFocus;
            _panelRoot.PreviewMouseLeftButtonDown += Panel_PreviewMouseLeftButtonDown;
            _panelRoot.LayoutUpdated += Panel_LayoutUpdated;

            Grid shell = new Grid();
            Border shadow = new Border();
            shadow.Style = (Style)Res("DropdownPanelShadow");
            shell.Children.Add(shadow);

            Border panel = new Border();
            panel.Style = (Style)Res("DropdownPanel");
            panel.Width = 300;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Padding = new Thickness(12);

            Grid rows = new Grid();
            rows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32) });
            rows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
            rows.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
            rows.RowDefinitions.Add(new RowDefinition { Height = new GridLength(43) });

            Grid head = new Grid();
            head.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
            head.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            head.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
            head.Children.Add(BuildNav(-1, 90, true));
            _title = new TextBlock();
            _title.Style = (Style)Res("PickerTitle");
            Grid.SetColumn(_title, 1);
            AutomationProperties.SetAutomationId(_title, _id + "_Title");
            head.Children.Add(_title);
            head.Children.Add(BuildNav(1, -90, false));
            Grid.SetRow(head, 0);
            rows.Children.Add(head);

            _bodyHost = new Grid();
            Grid.SetRow(_bodyHost, 2);
            rows.Children.Add(_bodyHost);

            Border foot = new Border();
            foot.BorderBrush = (Brush)Res("Line");
            foot.BorderThickness = new Thickness(0, 1, 0, 0);
            foot.Padding = new Thickness(0, 10, 0, 0);
            foot.Height = 43;
            Grid footGrid = new Grid();
            footGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56) });
            footGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60.9) });

            Button clear = new Button();
            clear.Style = (Style)Res("GhostButtonSm");
            clear.Content = "Clear";
            clear.Width = 56;
            clear.Height = 32;
            clear.HorizontalAlignment = HorizontalAlignment.Left;
            clear.Click += Clear_Click;
            AutomationProperties.SetAutomationId(clear, _id + "_Clear");
            footGrid.Children.Add(clear);

            Button today = new Button();
            today.Style = (Style)Res("PrimaryButtonSm");
            today.Content = WithTime ? "Apply" : "Today";
            today.Width = WithTime ? 60.4 : 60.9;
            today.Height = 32;
            today.HorizontalAlignment = HorizontalAlignment.Right;
            // ponytail: P3 turns this into Apply and adds the hour/minute/second inputs.
            if (WithTime) today.Click += Apply_Click; else today.Click += Today_Click;
            AutomationProperties.SetAutomationId(today, WithTime ? _id + "_Apply" : _id + "_Today");
            Grid.SetColumn(today, 2);
            footGrid.Children.Add(today);

            foot.Child = footGrid;
            Grid.SetRow(foot, 4);
            rows.Children.Add(foot);

            panel.Child = rows;
            shell.Children.Add(panel);
            _panelRoot.Content = shell;
            Render();
        }

        private Button BuildNav(int step, double angle, bool previous)
        {
            Button button = new Button();
            button.Style = (Style)Res("IconButton");
            button.Click += delegate { Navigate(step); };
            ContentControl icon = new ContentControl();
            icon.Style = (Style)Res("IconChevron");
            icon.Width = 18;
            icon.Height = 18;
            icon.RenderTransform = new RotateTransform(angle, 9, 9);
            button.Content = icon;
            AutomationProperties.SetAutomationId(button, previous ? _id + "_Prev" : _id + "_Next");
            AutomationProperties.SetName(button, previous ? "Previous" : "Next");
            if (!previous) Grid.SetColumn(button, 2);
            return button;
        }

        private void Navigate(int step)
        {
            if (Mode == "month") _viewYear += step;
            else if (Mode == "year") _viewYear += step * 20;
            else
            {
                _viewMonth += step;
                if (_viewMonth < 0) { _viewMonth = 11; _viewYear--; }
                if (_viewMonth > 11) { _viewMonth = 0; _viewYear++; }
            }
            Render();
        }

        private void Render()
        {
            if (_title == null || _bodyHost == null) return;
            // Re-rendering replaces the focused cell. Move focus back into the picker first so
            // focusout never sees a null/outside target and closes the panel (review fix 1).
            IInputElement focused = Keyboard.FocusedElement;
            if (focused != null && _panelRoot != null && DropdownTree.IsWithin(focused as DependencyObject, _panelRoot))
                _trigger.Focus();

            if (Mode == "month") _title.Text = _viewYear.ToString(CultureInfo.InvariantCulture);
            else if (Mode == "year")
            {
                int start = _viewYear - 8;
                _title.Text = start.ToString(CultureInfo.InvariantCulture) + "–" + (start + 19).ToString(CultureInfo.InvariantCulture);
            }
            else _title.Text = MonthsFull[_viewMonth] + " " + _viewYear.ToString(CultureInfo.InvariantCulture);

            _bodyHost.Children.Clear();
            _cellMap.Clear();
            if (Mode == "month") BuildMonthGrid();
            else if (Mode == "year") BuildYearGrid();
            else BuildDateGrid();
        }

        private void BuildMonthGrid()
        {
            Grid grid = BuildCellGrid(3, 88, 5, 12, 4, delegate(int i)
            {
                Selection value = new Selection { Year = _viewYear, Month = i, Day = 1 };
                Button cell = BuildCell(Months[i], value, _id + "_Month" + i);
                _cellMap[cell] = value;
                return cell;
            });
            _bodyHost.Children.Add(grid);
        }

        private void BuildYearGrid()
        {
            int start = _viewYear - 8;
            Grid grid = BuildCellGrid(4, 64.8, 5, 20, 5, delegate(int i)
            {
                int year = start + i;
                Selection value = new Selection { Year = year, Month = 0, Day = 1 };
                Button cell = BuildCell(year.ToString(CultureInfo.InvariantCulture), value, _id + "_Year" + year);
                _cellMap[cell] = value;
                return cell;
            });
            _bodyHost.Children.Add(grid);
        }

        private Grid BuildCellGrid(int columns, double cellWidth, double gap, int count, int rows, Func<int, Button> factory)
        {
            Grid grid = new Grid();
            for (int c = 0; c < columns; c++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(cellWidth) });
                if (c < columns - 1) grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(gap) });
            }
            for (int r = 0; r < rows; r++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32) });
                if (r < rows - 1) grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(gap) });
            }
            for (int i = 0; i < count; i++)
            {
                Button cell = factory(i);
                Grid.SetRow(cell, (i / columns) * 2); // odd rows are the 5px gaps
                Grid.SetColumn(cell, (i % columns) * 2);
                grid.Children.Add(cell);
            }
            return grid;
        }

        private Button BuildCell(string text, Selection value, string automationId)
        {
            bool selected = IsSelected(value);
            Button cell = new Button();
            cell.Style = (Style)Res("PickerCell");
            cell.Height = 32;
            cell.Tag = selected ? "selected" : null;
            if (selected) cell.Background = (Brush)Res("Brand");
            TextBlock label = new TextBlock();
            label.Text = text;
            label.FontFamily = (FontFamily)Res("Sans");
            label.FontSize = 13;
            label.FontWeight = selected ? FontWeights.SemiBold : FontWeights.Medium;
            label.Foreground = selected ? Brushes.White : (Brush)Res("Ink");
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            cell.Content = label;
            cell.Click += Cell_Click;
            AutomationProperties.SetAutomationId(cell, automationId);
            AutomationProperties.SetName(cell, text);
            AutomationProperties.SetItemStatus(cell, selected ? "selected" : "");
            return cell;
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            Button cell = sender as Button;
            if (cell == null || !_cellMap.ContainsKey(cell)) return;
            // ponytail: P3 uses this branch to copy the time inputs, re-render and stay open.
            _selected = _cellMap[cell];
            Commit();
        }

        private void BuildDateGrid()
        {
            int leading = (int)new DateTime(_viewYear, _viewMonth + 1, 1).DayOfWeek;
            int days = DateTime.DaysInMonth(_viewYear, _viewMonth + 1);
            int rowCount = (leading + days + 6) / 7;

            Grid grid = new Grid();
            for (int c = 0; c < 7; c++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(37.4) });
                if (c < 6) grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2) });
            }
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(27.6) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2) });
            for (int r = 0; r < rowCount; r++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32) });
                if (r < rowCount - 1) grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2) });
            }

            for (int c = 0; c < 7; c++)
            {
                TextBlock dow = new TextBlock();
                dow.Style = (Style)Res("PickerDow");
                dow.Text = Weekdays[c];
                Grid.SetColumn(dow, c * 2);
                grid.Children.Add(dow);
            }

            for (int d = 1; d <= days; d++)
            {
                int position = leading + d - 1;
                Selection value = new Selection { Year = _viewYear, Month = _viewMonth, Day = d };
                Button cell = BuildDay(d, position / 7, position % 7, value);
                grid.Children.Add(cell);
                _cellMap[cell] = value;
            }
            _bodyHost.Children.Add(grid);
        }

        private Button BuildDay(int day, int row, int column, Selection value)
        {
            bool selected = IsSelected(value);
            bool today = IsToday(day);
            Button cell = new Button();
            cell.Style = (Style)Res("PickerCell");
            cell.Height = 32;
            cell.Tag = selected ? "selected" : null;
            if (selected)
            {
                cell.Background = (Brush)Res("Brand");
                cell.Foreground = Brushes.White;
                cell.FontWeight = FontWeights.SemiBold;
            }
            Grid content = new Grid();
            if (today)
            {
                Border ring = new Border();
                ring.BorderBrush = (Brush)Res("LineStrong");
                ring.BorderThickness = new Thickness(1);
                ring.CornerRadius = new CornerRadius(7);
                ring.IsHitTestVisible = false;
                content.Children.Add(ring);
            }
            TextBlock label = new TextBlock();
            label.Text = day.ToString(CultureInfo.InvariantCulture);
            label.FontFamily = (FontFamily)Res("Sans");
            label.FontSize = 13;
            label.FontWeight = selected ? FontWeights.SemiBold : FontWeights.Medium;
            label.Foreground = selected ? Brushes.White : (Brush)Res("Ink");
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            content.Children.Add(label);

            cell.Content = content;
            cell.Click += Day_Click;
            Grid.SetRow(cell, row * 2 + 2);
            Grid.SetColumn(cell, column * 2);
            AutomationProperties.SetAutomationId(cell, _id + "_Day" + day);
            AutomationProperties.SetName(cell, MonthsFull[_viewMonth] + " " + day + ", " + _viewYear.ToString(CultureInfo.InvariantCulture));
            string status = (selected ? "selected" : "") + (selected && today ? " " : "") + (today ? "today" : "");
            AutomationProperties.SetItemStatus(cell, status);
            return cell;
        }

        private void Day_Click(object sender, RoutedEventArgs e)
        {
            Button cell = sender as Button;
            if (cell == null || !_cellMap.ContainsKey(cell)) return;
            // ponytail: P3 copies the hour/minute/second inputs, re-renders and stays open.
            _selected = _cellMap[cell];
            if (!WithTime) Commit(); else Render();
        }

        private void Today_Click(object sender, RoutedEventArgs e)
        {
            // ponytail: P3 replaces the 09:30 defaults with the live time inputs.
            _selected = new Selection
            {
                Year = _today.Year,
                Month = _today.Month - 1,
                Day = _today.Day,
                Hour = 9,
                Minute = 30,
                Second = 0
            };
            Commit();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null) return;
            Commit();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            // W6: the time inputs keep their values (there are none in date mode).
            _selected = null;
            _value.Text = Placeholder;
            _value.Foreground = (Brush)Res("Ink3");
            Close(false);
        }

        private string Format()
        {
            if (_selected == null) return "";
            string year = _selected.Year.ToString(CultureInfo.InvariantCulture);
            if (Mode == "year") return year;
            if (Mode == "month") return Months[_selected.Month] + " " + year;
            string date = Months[_selected.Month] + " " + _selected.Day.ToString(CultureInfo.InvariantCulture) + ", " + year;
            if (!WithTime) return date;
            string time = _selected.Hour.ToString("00", CultureInfo.InvariantCulture) + ":" +
                          _selected.Minute.ToString("00", CultureInfo.InvariantCulture);
            if (WithSeconds) time += ":" + _selected.Second.ToString("00", CultureInfo.InvariantCulture);
            return date + ", " + time;
        }

        private void Commit()
        {
            _value.Text = Format();
            _value.Foreground = (Brush)Res("Ink");
            Close(false);
        }

        private void Picker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape || _adorner == null) return;
            e.Handled = true;
            Close(true);
        }

        private void Panel_LayoutUpdated(object sender, EventArgs e)
        {
            Button cell = _focusCellOnOpen;
            if (cell == null || _adorner == null) return;
            _focusCellOnOpen = null;
            cell.Focus();
        }

        // Web: mousedown on the panel calls preventDefault, so buttons and cells stay tab stops
        // but a mouse press must not move focus. Never mark the event Handled: that kills Click.
        private void Panel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IInputElement saved = Keyboard.FocusedElement;
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(delegate
            {
                if (_adorner == null) return;
                Visual visual = saved as Visual;
                if (saved != null && visual != null && PresentationSource.FromVisual(visual) != null) saved.Focus();
                else _trigger.Focus();
            }));
        }

        private void Picker_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_closing || _panelRoot == null) return;
            // The web ignores focus going to document.body / Alt-Tab (null NewFocus).
            if (e.NewFocus == null) return;
            DependencyObject next = e.NewFocus as DependencyObject;
            if (next != null && (DropdownTree.IsWithin(next, this) || DropdownTree.IsWithin(next, _panelRoot))) return;
            Close(false);
        }
    }
}
