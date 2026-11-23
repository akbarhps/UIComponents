using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UIComponents.Wpf
{
    // Code-only multi-select: trigger with wrapping chips, adorner-hosted panel with checkbox
    // options, search, empty row and a select-all / clear-all / count / Done foot.
    public class MultiSelect : Grid
    {
        private sealed class Entry
        {
            public string Label;
            public string Meta;
        }

        private readonly List<Entry> _entries = new List<Entry>();
        private bool[] _selected = new bool[0]; // survives close/reopen; the web keeps input.checked
        private readonly List<CheckBox> _checks = new List<CheckBox>();
        private readonly Dictionary<CheckBox, Path> _ticks = new Dictionary<CheckBox, Path>();
        private readonly Dictionary<CheckBox, Border> _boxes = new Dictionary<CheckBox, Border>();
        private ToggleButton _trigger;
        private ChipWrapPanel _chipHost;
        private TextBlock _placeholder;
        private Viewbox _chevron;
        private TextBox _search;
        private FrameworkElement _panel;
        private Adorner _adorner;
        private TextBlock _empty;
        private TextBlock _count;
        private DropdownOutsideClick _outsideClick;
        private Window _window;
        private KeyEventHandler _windowKeyDown;
        private string _id;
        private bool _closing;
        private bool _building;

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
            _selected = new bool[_entries.Count];
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
            _placeholder = new TextBlock();
            _placeholder.Text = Placeholder;
            _placeholder.Style = (Style)Res("MultiPlaceholder");
            AutomationProperties.SetAutomationId(_placeholder, _id + "_Placeholder");

            _chipHost = new ChipWrapPanel();
            _chipHost.GapX = 6;
            _chipHost.GapY = 6;
            _chipHost.Margin = new Thickness(0, 0, 8, 0); // 402 chip host + 8 gap to chevron

            _chevron = new Viewbox();
            _chevron.Width = 16;
            _chevron.Height = 16;
            _chevron.Stretch = Stretch.Uniform;
            _chevron.HorizontalAlignment = HorizontalAlignment.Right;
            _chevron.VerticalAlignment = VerticalAlignment.Top;
            Path chevronPath = new Path
            {
                Data = Geometry.Parse("M6,9 L12,15 L18,9"),
                StrokeThickness = 2, Fill = Brushes.Transparent,
                StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round
            };
            chevronPath.SetResourceReference(Shape.StrokeProperty, "Ink3");
            _chevron.Child = new Canvas
            {
                Width = 24, Height = 24,
                Children = { chevronPath }
            };

            Grid content = new Grid();
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            Grid.SetColumn(_placeholder, 0);
            Grid.SetColumn(_chipHost, 0);
            Grid.SetColumn(_chevron, 1);
            content.Children.Add(_placeholder);
            content.Children.Add(_chipHost);
            content.Children.Add(_chevron);

            _trigger = new ToggleButton();
            _trigger.Style = (Style)Res("MultiTriggerChrome");
            _trigger.Content = content;
            AutomationProperties.SetAutomationId(_trigger, _id + "_Trigger");
            AutomationProperties.SetName(_trigger, Label);
            _trigger.Checked += Trigger_Checked;
            _trigger.Unchecked += Trigger_Unchecked;
            Children.Add(_trigger);
            UpdateTrigger();
        }

        private void Trigger_Checked(object sender, RoutedEventArgs e)
        {
            Open();
        }

        private void Trigger_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_closing) return;
            Close(false); // web: closing from the trigger does not move focus
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
            SetChevron(true);
            _outsideClick = new DropdownOutsideClick(this, delegate { return (DependencyObject)_panel; },
                delegate { Close(false); });
            _outsideClick.Hook();
            _window = Window.GetWindow(this);
            if (_window != null)
            {
                _windowKeyDown = Window_PreviewKeyDown;
                _window.AddHandler(Keyboard.PreviewKeyDownEvent, _windowKeyDown, true);
            }
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
                if (_window != null && _windowKeyDown != null)
                {
                    _window.RemoveHandler(Keyboard.PreviewKeyDownEvent, _windowKeyDown);
                    _window = null;
                    _windowKeyDown = null;
                }
                if (_adorner != null)
                {
                    AdornerLayer layer = AdornerLayer.GetAdornerLayer(_trigger);
                    if (layer != null) layer.Remove(_adorner);
                    _adorner = null;
                }
                _panel = null;
                _search = null;
                _empty = null;
                _count = null;
                _checks.Clear();
                _ticks.Clear();
                _boxes.Clear();
                SetChevron(false);
                if (_trigger.IsChecked == true) _trigger.IsChecked = false;
                if (restoreFocus)
                {
                    _trigger.Focus();
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate { _trigger.Focus(); }));
                }
            }
            finally { _closing = false; }
        }

        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            Close(false);
        }

        private void SetChevron(bool open)
        {
            _chevron.RenderTransform = open
                ? new RotateTransform(180, 8, 8)
                : new RotateTransform(0, 8, 8);
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
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Border searchArea = new Border();
            searchArea.SetResourceReference(Border.BorderBrushProperty, "Line");
            searchArea.BorderThickness = new Thickness(0, 0, 0, 1);
            searchArea.Padding = new Thickness(10);
            Grid searchGrid = new Grid();
            _search = new TextBox();
            _search.Style = (Style)Res("DropdownSearch");
            _search.Tag = SearchPlaceholder;
            AutomationProperties.SetAutomationId(_search, _id + "_Search");
            AutomationProperties.SetName(_search, SearchName);
            _search.TextChanged += Search_TextChanged;
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
                CheckBox check = BuildOption(_entries[i], i);
                _checks.Add(check);
                list.Children.Add(check);
            }
            _building = true;
            for (int i = 0; i < _checks.Count; i++) _checks[i].IsChecked = _selected[i];
            _building = false;
            _empty = new TextBlock();
            _empty.Style = (Style)Res("DropdownEmpty");
            _empty.Text = "No matches";
            _empty.Visibility = Visibility.Collapsed;
            AutomationProperties.SetAutomationId(_empty, _id + "_Empty");
            list.Children.Add(_empty);
            scroller.Content = list;
            Grid.SetRow(scroller, 1);
            content.Children.Add(scroller);

            Border foot = new Border();
            foot.Style = (Style)Res("MultiFoot");
            Grid footGrid = new Grid();
            footGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            footGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            StackPanel left = new StackPanel { Orientation = Orientation.Horizontal };
            Button selectAll = new Button { Content = "Select all", Style = (Style)Res("GhostButtonSm") };
            Button clear = new Button { Content = "Clear all", Style = (Style)Res("GhostButtonSm") };
            clear.Margin = new Thickness(4, 0, 0, 0);
            AutomationProperties.SetAutomationId(selectAll, _id + "_SelectAll");
            AutomationProperties.SetAutomationId(clear, _id + "_Clear");
            selectAll.Click += SelectAll_Click;
            clear.Click += Clear_Click;
            left.Children.Add(selectAll);
            left.Children.Add(clear);
            StackPanel right = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            _count = new TextBlock();
            _count.FontFamily = (FontFamily)Res("Sans");
            _count.FontSize = 12;
            _count.LineHeight = 19.2;
            _count.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            _count.VerticalAlignment = VerticalAlignment.Center;
            _count.Margin = new Thickness(0, 0, 10, 0);
            _count.SetResourceReference(TextBlock.ForegroundProperty, "Ink3");
            AutomationProperties.SetAutomationId(_count, _id + "_Count");
            Button done = new Button { Content = "Done", Style = (Style)Res("PrimaryButtonSm"), Width = 57.6, Height = 32 };
            AutomationProperties.SetAutomationId(done, _id + "_Done");
            done.Click += Done_Click;
            right.Children.Add(_count);
            right.Children.Add(done);
            Grid.SetColumn(left, 0);
            Grid.SetColumn(right, 1);
            footGrid.Children.Add(left);
            footGrid.Children.Add(right);
            foot.Child = footGrid;
            Grid.SetRow(foot, 2);
            content.Children.Add(foot);

            border.Child = content;
            root.Children.Add(border);
            _panel = root;
            UpdateTrigger();
        }

        private CheckBox BuildOption(Entry entry, int index)
        {
            CheckBox check = new CheckBox();
            check.Style = (Style)Res("MultiOption");
            check.HorizontalAlignment = HorizontalAlignment.Stretch;
            AutomationProperties.SetAutomationId(check, _id + "_Option" + index);
            AutomationProperties.SetName(check, entry.Label);
            check.Tag = index;

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(19) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Border box = new Border();
            box.Width = 19; box.Height = 19; box.CornerRadius = new CornerRadius(5);
            box.BorderThickness = new Thickness(1);
            box.SetResourceReference(Border.BorderBrushProperty, "LineStrong");
            box.SetResourceReference(Border.BackgroundProperty, "Surface");
            box.VerticalAlignment = VerticalAlignment.Top;
            Path tick = new Path();
            tick.Data = Geometry.Parse("M5,12 L10,17 L19,7");
            tick.SetResourceReference(Shape.StrokeProperty, "OnBrand");
            tick.StrokeThickness = 2.4;
            tick.Fill = Brushes.Transparent;
            tick.Stretch = Stretch.Uniform;
            tick.Margin = new Thickness(4);
            tick.Visibility = Visibility.Collapsed;
            box.Child = tick;
            Grid.SetColumn(box, 0);
            grid.Children.Add(box);
            _ticks[check] = tick;
            _boxes[check] = box;

            TextBlock label = new TextBlock();
            label.Text = entry.Label;
            label.FontFamily = (FontFamily)Res("Sans");
            label.FontSize = 14;
            label.SetResourceReference(TextBlock.ForegroundProperty, "Ink");
            label.LineHeight = 22.4;
            label.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            label.Margin = new Thickness(0, -5, 0, 0);
            label.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetColumn(label, 2);
            grid.Children.Add(label);

            TextBlock meta = new TextBlock();
            meta.Text = entry.Meta;
            meta.FontFamily = (FontFamily)Res("Sans");
            meta.FontSize = 12;
            meta.LineHeight = 19.2;
            meta.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            meta.SetResourceReference(TextBlock.ForegroundProperty, "Ink3");
            meta.VerticalAlignment = VerticalAlignment.Top;
            meta.Margin = new Thickness(10, 0, 0, 0);
            Grid.SetColumn(meta, 3);
            grid.Children.Add(meta);

            check.Content = grid;
            check.Checked += Option_Checked;
            check.Unchecked += Option_Checked;
            return check;
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter(_search.Text);
        }

        private void ApplyFilter(string query)
        {
            string q = (query ?? "").Trim().ToLowerInvariant();
            int shown = 0;
            for (int i = 0; i < _checks.Count; i++)
            {
                Entry entry = _entries[i];
                string text = (entry.Label + entry.Meta).ToLowerInvariant();
                bool show = text.Contains(q);
                _checks[i].Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                if (show) shown++;
            }
            _empty.Visibility = shown > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Option_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            if (check != null && _ticks.ContainsKey(check))
            {
                bool on = check.IsChecked == true;
                _selected[(int)check.Tag] = on;
                _ticks[check].Visibility = on ? Visibility.Visible : Visibility.Collapsed;
                _boxes[check].SetResourceReference(Border.BackgroundProperty, on ? "Brand" : "Surface");
                _boxes[check].SetResourceReference(Border.BorderBrushProperty, on ? "Brand" : "LineStrong");
            }
            if (!_building) UpdateTrigger();
        }

        private void UpdateTrigger()
        {
            _chipHost.Children.Clear();
            int count = 0;
            for (int i = 0; i < _entries.Count; i++)
            {
                if (!_selected[i]) continue;
                count++;
                Border chip = new Border();
                chip.Style = (Style)Res("MultiChip");
                StackPanel inner = new StackPanel { Orientation = Orientation.Horizontal };
                TextBlock text = new TextBlock();
                text.Text = _entries[i].Label;
                text.FontFamily = (FontFamily)Res("Sans");
                text.FontSize = 12;
                text.FontWeight = FontWeights.SemiBold;
                text.LineHeight = 16.8;
                text.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                text.SetResourceReference(TextBlock.ForegroundProperty, "Brand");
                text.VerticalAlignment = VerticalAlignment.Center;
                AutomationProperties.SetAutomationId(text, _id + "_Chip" + (count - 1));
                Button remove = new Button();
                remove.Style = (Style)Res("ChipRemove");
                remove.Margin = new Thickness(5, 0, 0, 0);
                AutomationProperties.SetAutomationId(remove, _id + "_Remove" + (count - 1));
                AutomationProperties.SetName(remove, "Remove " + _entries[i].Label);
                remove.Tag = count - 1;
                remove.Click += Remove_Click;
                inner.Children.Add(text);
                inner.Children.Add(remove);
                chip.Child = inner;
                _chipHost.Children.Add(chip);
            }
            _placeholder.Visibility = count == 0 ? Visibility.Visible : Visibility.Collapsed;
            _chipHost.Visibility = count == 0 ? Visibility.Collapsed : Visibility.Visible;
            _chevron.Margin = new Thickness(0, count == 0 ? 6.5 : 3, 0, 0);
            if (_count != null) _count.Text = count + " selected";
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Button button = sender as Button;
            if (button == null || !(button.Tag is int)) return;
            int index = (int)button.Tag;
            int seen = -1;
            for (int i = 0; i < _selected.Length; i++)
            {
                if (!_selected[i]) continue;
                seen++;
                if (seen != index) continue;
                _selected[i] = false;
                if (i < _checks.Count) _checks[i].IsChecked = false; // fires Option_Checked while open
                else UpdateTrigger(); // closed: no checkbox to fire it
                break;
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < _checks.Count; i++)
                if (_checks[i].Visibility == Visibility.Visible) _checks[i].IsChecked = true; // fires Option_Checked
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < _checks.Count; i++) _checks[i].IsChecked = false; // fires Option_Checked
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            Close(true);
        }

        // Web fix: Escape restores focus to the trigger; outside click (M8) does not.
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _adorner != null)
            {
                e.Handled = true;
                Close(true);
            }
        }
    }
}
