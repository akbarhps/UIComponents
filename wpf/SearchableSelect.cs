using System;
using System.Collections.Generic;
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
    // Code-only control (like SheetGrid): the trigger is the only visual child; the panel is
    // built on demand and hosted in an Adorner so it is captured, UIA- and FindById-reachable.
    public class SearchableSelect : Grid
    {
        private sealed class Entry
        {
            public string Label;
            public string Meta;
        }

        private sealed class Row
        {
            public Entry Entry;
            public Button Button;
            public ContentControl Check;
            public TextBlock Label;
        }

        private readonly List<Entry> _entries = new List<Entry>();
        private readonly List<Row> _rows = new List<Row>();
        private readonly List<Row> _visible = new List<Row>();
        private ToggleButton _trigger;
        private TextBlock _value;
        private TextBox _search;
        private FrameworkElement _panel;
        private Adorner _adorner;
        private TextBlock _empty;
        private Row _activeRow;
        private Entry _selected;
        private string _id;
        private DropdownOutsideClick _outsideClick;
        private bool _closing;

        public string Label { get; set; }
        public string Placeholder { get; set; }
        public string SearchPlaceholder { get; set; }
        public string SearchName { get; set; }
        public string Options { get; set; }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _id = AutomationProperties.GetAutomationId(this);
            if (string.IsNullOrEmpty(_id)) _id = Name;
            ParseOptions();
            BuildTrigger();
            Unloaded += Control_Unloaded;
        }

        private static object Res(string key)
        {
            return Application.Current.FindResource(key);
        }

        private void ParseOptions()
        {
            if (string.IsNullOrEmpty(Options)) return;
            foreach (string part in Options.Split(','))
            {
                string[] bits = part.Split('|');
                Entry entry = new Entry();
                entry.Label = bits[0].Trim();
                entry.Meta = bits.Length > 1 ? bits[1].Trim() : "";
                if (entry.Label.Length > 0) _entries.Add(entry);
            }
        }

        private void BuildTrigger()
        {
            _value = new TextBlock();
            _value.Text = Placeholder;
            _value.FontFamily = (FontFamily)Res("Sans");
            _value.FontSize = 15;
            _value.LineHeight = 21;
            _value.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            _value.Foreground = (Brush)Res("Ink3");
            _value.TextTrimming = TextTrimming.CharacterEllipsis;
            _value.TextWrapping = TextWrapping.NoWrap;
            AutomationProperties.SetAutomationId(_value, _id + "_Value");

            _trigger = new ToggleButton();
            _trigger.Style = (Style)Res("DropdownTrigger");
            _trigger.Content = _value;
            AutomationProperties.SetAutomationId(_trigger, _id + "_Trigger");
            AutomationProperties.SetName(_trigger, Label);
            _trigger.Checked += Trigger_Checked;
            _trigger.Unchecked += Trigger_Unchecked;
            _trigger.PreviewKeyDown += Trigger_PreviewKeyDown;
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

        // S3: opening from the keyboard highlights the first option; suppress the ToggleButton's own toggle.
        private void Trigger_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_trigger.IsChecked == true) return;
            if (e.Key != Key.Down && e.Key != Key.Return && e.Key != Key.Space) return;
            e.Handled = true;
            _trigger.IsChecked = true;
            SetActive(_visible.Count > 0 ? _visible[0] : null);
        }

        private void Open()
        {
            if (_adorner != null) return;
            BuildPanel();
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(_trigger);
            if (layer == null) { _trigger.IsChecked = false; return; }
            _adorner = new DropdownAdorner(_trigger, _panel);
            layer.Add(_adorner);
            _search.Text = "";
            ApplyFilter("");
            _outsideClick = new DropdownOutsideClick(this, delegate { return (DependencyObject)_panel; },
                delegate { Close(false); });
            _outsideClick.Hook();
            TextBox search = _search;
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(delegate { search.Focus(); }));
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
                _panel = null;
                _search = null;
                _empty = null;
                _rows.Clear();
                _visible.Clear();
                _activeRow = null;
                if (_trigger.IsChecked == true) _trigger.IsChecked = false;
                if (restoreFocus) _trigger.Focus();
            }
            finally { _closing = false; }
        }

        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void BuildPanel()
        {
            Grid root = new Grid();
            Border shadow = new Border();
            shadow.Style = (Style)Res("DropdownPanelShadow");
            root.Children.Add(shadow);

            Border border = new Border();
            border.Style = (Style)Res("DropdownPanel");
            Grid content = new Grid();
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Border searchArea = new Border();
            searchArea.BorderBrush = (Brush)Res("Line");
            searchArea.BorderThickness = new Thickness(0, 0, 0, 1);
            searchArea.Padding = new Thickness(10);
            Grid searchGrid = new Grid();
            _search = new TextBox();
            _search.Style = (Style)Res("DropdownSearch");
            _search.Tag = SearchPlaceholder;
            AutomationProperties.SetAutomationId(_search, _id + "_Search");
            AutomationProperties.SetName(_search, SearchName);
            _search.TextChanged += Search_TextChanged;
            _search.PreviewKeyDown += Search_KeyDown; // preview: TextBox marks arrow/Enter KeyDown handled
            _search.LostKeyboardFocus += Search_LostKeyboardFocus;
            searchGrid.Children.Add(_search);
            ContentControl icon = new ContentControl();
            icon.Style = (Style)Res("SearchIcon");
            icon.Width = 15;
            icon.Height = 15;
            icon.Margin = new Thickness(11, 0, 0, 0);
            icon.HorizontalAlignment = HorizontalAlignment.Left;
            icon.VerticalAlignment = VerticalAlignment.Center;
            searchGrid.Children.Add(icon);
            searchArea.Child = searchGrid;
            Grid.SetRow(searchArea, 0);
            content.Children.Add(searchArea);

            ScrollViewer scroller = new ScrollViewer();
            scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            scroller.MaxHeight = 220;
            scroller.Resources[typeof(ScrollBar)] = (Style)Res("DropdownScrollBar");
            StackPanel list = new StackPanel();
            list.Margin = new Thickness(6);
            for (int i = 0; i < _entries.Count; i++)
            {
                Row row = BuildOption(_entries[i], i);
                _rows.Add(row);
                list.Children.Add(row.Button);
            }
            _empty = new TextBlock();
            _empty.Style = (Style)Res("DropdownEmpty");
            _empty.Text = "No matches";
            _empty.Visibility = Visibility.Collapsed;
            AutomationProperties.SetAutomationId(_empty, _id + "_Empty");
            list.Children.Add(_empty);
            scroller.Content = list;
            Grid.SetRow(scroller, 1);
            content.Children.Add(scroller);

            border.Child = content;
            root.Children.Add(border);
            _panel = root;
        }

        private Row BuildOption(Entry entry, int index)
        {
            Button button = new Button();
            button.Style = (Style)Res("DropdownOption");
            button.Focusable = false; // web mousedown guard: keep focus in the search input
            button.Click += Option_Click;
            AutomationProperties.SetAutomationId(button, _id + "_Option" + index);
            AutomationProperties.SetName(button, entry.Label);

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            ContentControl check = new ContentControl();
            check.Style = (Style)Res("DropdownCheck");
            check.Visibility = Visibility.Collapsed;
            Grid.SetColumn(check, 0);
            grid.Children.Add(check);

            TextBlock label = new TextBlock();
            label.Text = entry.Label;
            label.FontFamily = (FontFamily)Res("Sans");
            label.FontSize = 14;
            label.LineHeight = 22.4;
            label.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            label.VerticalAlignment = VerticalAlignment.Center;
            // VerticalAlignment=Center with a -5 top margin shifts this line's ink up ~2.5px to web ink y.
            label.Margin = new Thickness(0, -5, 0, 0);
            label.TextTrimming = TextTrimming.CharacterEllipsis;
            Grid.SetColumn(label, 2);
            grid.Children.Add(label);

            TextBlock meta = new TextBlock();
            meta.Text = entry.Meta;
            meta.FontFamily = (FontFamily)Res("Sans");
            meta.FontSize = 12;
            meta.LineHeight = 19.2;
            meta.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            meta.Foreground = (Brush)Res("Ink3");
            meta.VerticalAlignment = VerticalAlignment.Center;
            meta.Margin = new Thickness(10, -1, 0, 0); // web meta ink sits ~1px higher
            Grid.SetColumn(meta, 3);
            grid.Children.Add(meta);

            button.Content = grid;
            return new Row { Entry = entry, Button = button, Check = check, Label = label };
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(_search.Text);
        }

        // S5/S6/S7: wrap over visible options; Up from none lands on the LAST (web fix), Enter chooses, Escape closes.
        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                e.Handled = true;
                if (_visible.Count == 0) return;
                int current = ActiveIndex();
                int next = e.Key == Key.Down
                    ? (current + 1) % _visible.Count
                    : (current < 0 ? _visible.Count - 1 : (current - 1 + _visible.Count) % _visible.Count);
                Row row = _visible[next];
                SetActive(row);
                row.Button.BringIntoView();
            }
            else if (e.Key == Key.Return && _activeRow != null && _visible.Contains(_activeRow))
            {
                e.Handled = true;
                Choose(_activeRow);
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close(true);
            }
            else if (e.Key == Key.Tab)
            {
                // Options are not focusable, so forward Tab always leaves the combo (web fix S10).
                // Shift+Tab lands on the trigger and keeps the panel open, as on the web.
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0) Close(false);
            }
        }

        private int ActiveIndex()
        {
            for (int i = 0; i < _visible.Count; i++)
                if (_visible[i] == _activeRow) return i;
            return -1;
        }

        // Web fix S10: focusout (Tab included) closes the panel without stealing focus back.
        private void Search_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_panel == null || e.NewFocus == null) return;
            DependencyObject next = e.NewFocus as DependencyObject;
            if (DropdownTree.IsWithin(next, this) || DropdownTree.IsWithin(next, _panel)) return;
            Close(false);
        }

        private void ApplyFilter(string query)
        {
            string q = (query ?? "").Trim().ToLowerInvariant();
            _visible.Clear();
            for (int i = 0; i < _rows.Count; i++)
            {
                Row row = _rows[i];
                string text = (row.Entry.Label + row.Entry.Meta).ToLowerInvariant();
                bool show = text.Contains(q);
                row.Button.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                if (show) _visible.Add(row);
            }
            _empty.Visibility = _visible.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
            SetActive(null); // Web fix S4: filtering clears the highlight and aria-activedescendant.
        }

        private void SetActive(Row row)
        {
            _activeRow = row;
            for (int i = 0; i < _rows.Count; i++) UpdateRow(_rows[i]);
        }

        private void UpdateRow(Row row)
        {
            bool selected = _selected == row.Entry;
            bool active = _activeRow == row;
            string status = (active ? "active" : "") + (selected ? (active ? " " : "") + "selected" : "");
            AutomationProperties.SetItemStatus(row.Button, status);
            row.Button.Background = active ? (Brush)Res("BgDeep") : Brushes.Transparent;
            row.Label.Foreground = selected ? (Brush)Res("Brand") : (Brush)Res("Ink");
            row.Label.FontWeight = selected ? FontWeights.SemiBold : FontWeights.Normal;
            row.Check.Visibility = selected ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Option_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;
            for (int i = 0; i < _rows.Count; i++)
            {
                if (_rows[i].Button == button)
                {
                    Choose(_rows[i]);
                    return;
                }
            }
        }

        private void Choose(Row row)
        {
            _selected = row.Entry;
            _value.Text = row.Entry.Label;
            _value.Foreground = (Brush)Res("Ink");
            AutomationProperties.SetItemStatus(_value, "selected");
            for (int i = 0; i < _rows.Count; i++) UpdateRow(_rows[i]);
            Close(true);
        }
    }
}
