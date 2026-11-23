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
    // Code-only autocomplete. Multiple mode reuses the tag-box chrome; single mode reuses the
    // Forms search look. Both host their suggestion list in an Adorner so capture/UIA see it.
    public class Autocomplete : Grid
    {
        private readonly List<string> _options = new List<string>();
        private readonly List<string> _tags = new List<string>();
        private readonly List<string> _matches = new List<string>();
        private readonly List<Button> _optionButtons = new List<Button>();
        private FrameworkElement _anchor;
        private Border _box;
        private Border _focusRing;
        private ChipWrapPanel _chipHost;
        private TextBox _input;
        private TextBlock _hint;
        private FrameworkElement _panel;
        private Adorner _adorner;
        private TextBlock _empty;
        private DropdownOutsideClick _outsideClick;
        private string _id;
        private int _active = -1;
        private bool _suppress;

        public string Label { get; set; }
        public string Placeholder { get; set; }
        public string SearchName { get; set; }
        public string Options { get; set; }
        public string HintText { get; set; }
        public bool Multiple { get; set; }
        public bool ShowHint { get; set; }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _id = AutomationProperties.GetAutomationId(this);
            if (string.IsNullOrEmpty(_id)) _id = Name;
            if (!string.IsNullOrEmpty(Options))
                foreach (string part in Options.Split(','))
                {
                    string option = part.Trim();
                    if (option.Length > 0) _options.Add(option);
                }
            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            BuildInput();
            BuildHint();
            RenderTags();
            UpdateHint();
            Unloaded += Control_Unloaded;
        }

        private static object Res(string key)
        {
            return Application.Current.FindResource(key);
        }

        private void BuildInput()
        {
            _input = new TextBox();
            _input.Style = (Style)Res("BareInput");
            _input.Tag = Placeholder;
            AutomationProperties.SetAutomationId(_input, _id + "_Input");
            AutomationProperties.SetName(_input, Label);
            _input.PreviewKeyDown += Input_PreviewKeyDown;
            _input.TextChanged += Input_TextChanged;
            _input.GotKeyboardFocus += Input_GotFocus;
            _input.LostKeyboardFocus += Input_LostFocus;

            if (Multiple)
            {
                _focusRing = new Border();
                _focusRing.BorderThickness = new Thickness(3);
                _focusRing.SetResourceReference(Border.BorderBrushProperty, "FocusRing");
                _focusRing.CornerRadius = new CornerRadius(11);
                _focusRing.Margin = new Thickness(-3);
                _focusRing.Visibility = Visibility.Collapsed;
                _focusRing.IsHitTestVisible = false;
                _chipHost = new ChipWrapPanel();
                _chipHost.GapX = 6;
                _chipHost.GapY = 6;
                _chipHost.FillLast = true;
                _chipHost.Children.Add(_input);
                _box = new Border();
                _box.Style = (Style)Res("TagBox");
                _box.Child = _chipHost;
                _box.IsKeyboardFocusWithinChanged += Box_FocusChanged;
                Grid shell = new Grid();
                shell.Children.Add(_focusRing);
                shell.Children.Add(_box);
                Children.Add(shell);
                _anchor = _box;
                RenderTags();
            }
            else
            {
                Grid search = new Grid();
                _input.Style = (Style)Res("FieldInputSearch");
                search.Children.Add(_input);
                ContentControl icon = new ContentControl();
                icon.Style = (Style)Res("SearchIcon");
                icon.Margin = new Thickness(13, 0, 0, 0);
                icon.HorizontalAlignment = HorizontalAlignment.Left;
                icon.VerticalAlignment = VerticalAlignment.Center;
                search.Children.Add(icon);
                Children.Add(search);
                _anchor = _input;
            }
        }

        private void BuildHint()
        {
            if (!ShowHint) return;
            _hint = new TextBlock();
            _hint.Style = (Style)Res("FieldHint");
            AutomationProperties.SetAutomationId(_hint, _id + "_Hint");
            Children.Add(_hint);
            Grid.SetRow(_hint, 1);
        }

        private void Box_FocusChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _focusRing.Visibility = _box.IsKeyboardFocusWithin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RenderTags()
        {
            if (_chipHost == null) return;
            // Keep the field attached: clearing all children detaches it and fires the blur handler.
            for (int i = _chipHost.Children.Count - 1; i >= 0; i--)
                if (_chipHost.Children[i] != _input) _chipHost.Children.RemoveAt(i);
            for (int i = 0; i < _tags.Count; i++)
            {
                Border chip = new Border();
                chip.Style = (Style)Res("MultiChip");
                StackPanel inner = new StackPanel { Orientation = Orientation.Horizontal };
                TextBlock text = new TextBlock();
                text.Text = _tags[i];
                text.FontFamily = (FontFamily)Res("Sans");
                text.FontSize = 12;
                text.FontWeight = FontWeights.SemiBold;
                text.LineHeight = 19.2;
                text.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
                text.SetResourceReference(TextBlock.ForegroundProperty, "Brand");
                text.VerticalAlignment = VerticalAlignment.Center;
                AutomationProperties.SetAutomationId(text, _id + "_Chip" + i);
                Button remove = new Button();
                remove.Style = (Style)Res("ChipRemove");
                remove.Focusable = false; // keep focus in the field on a chip-remove click
                remove.Margin = new Thickness(5, 0, 0, 0);
                remove.Tag = i;
                AutomationProperties.SetAutomationId(remove, _id + "_Remove" + i);
                AutomationProperties.SetName(remove, "Remove " + _tags[i]);
                remove.Click += Remove_Click;
                inner.Children.Add(text);
                inner.Children.Add(remove);
                chip.Child = inner;
                _chipHost.Children.Insert(i, chip);
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Button button = sender as Button;
            if (button == null || !(button.Tag is int)) return;
            int index = (int)button.Tag;
            if (index < 0 || index >= _tags.Count) return;
            _tags.RemoveAt(index);
            RenderTags();
            UpdateHint();
        }

        private void UpdateHint()
        {
            if (_hint == null) return;
            _hint.Text = Multiple ? _tags.Count + " selected" : HintText;
        }

        private void Input_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!_suppress) Show();
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_suppress) Show();
        }

        private void Input_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // Web fix A8: losing focus (Tab included) hides the list.
            if (_adorner != null) Hide();
        }

        private void Input_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                e.Handled = true;
                if (_adorner == null || _matches.Count == 0) { Show(); return; }
                _active = e.Key == Key.Down
                    ? Math.Min(_active + 1, _matches.Count - 1)
                    : Math.Max(_active - 1, 0);
                Highlight();
            }
            else if (e.Key == Key.Return)
            {
                if (_adorner != null && _active >= 0 && _active < _matches.Count)
                {
                    e.Handled = true;
                    Choose(_matches[_active]);
                }
                else if (Multiple && _input.Text.Trim().Length > 0)
                {
                    e.Handled = true;
                    Choose(_input.Text.Trim()); // web fix: duplicate check inside Choose
                }
            }
            else if (e.Key == Key.Escape)
            {
                if (_adorner != null) { e.Handled = true; Hide(); }
            }
        }

        private void Show()
        {
            if (_adorner != null) Hide(); // rebuild the hosted panel for the new match set
            string q = _input.Text.Trim().ToLowerInvariant();
            _matches.Clear();
            foreach (string option in _options)
            {
                if (!option.ToLowerInvariant().Contains(q)) continue;
                if (Multiple)
                {
                    bool used = false;
                    foreach (string tag in _tags)
                        if (string.Equals(tag, option, StringComparison.OrdinalIgnoreCase)) { used = true; break; }
                    if (used) continue;
                }
                _matches.Add(option);
                if (_matches.Count == 8) break;
            }
            _active = -1;
            BuildPanel();
            if (_adorner == null)
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(_anchor);
                if (layer == null) return;
                _adorner = new DropdownAdorner(_anchor, _panel);
                layer.Add(_adorner);
                _outsideClick = new DropdownOutsideClick(this, delegate { return (DependencyObject)_panel; },
                    delegate { Hide(); });
                _outsideClick.Hook();
            }
            Highlight();
        }

        private void BuildPanel()
        {
            Grid root = new Grid();
            Border shadow = new Border();
            shadow.Style = (Style)Res("DropdownPanelShadow");
            root.Children.Add(shadow);
            Border border = new Border();
            border.Style = (Style)Res("DropdownPanel");

            ScrollViewer scroller = new ScrollViewer();
            scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            scroller.MaxHeight = 218;
            scroller.Resources[typeof(ScrollBar)] = (Style)Res("DropdownScrollBar");
            StackPanel list = new StackPanel();
            list.Margin = new Thickness(6);
            _optionButtons.Clear();
            for (int i = 0; i < _matches.Count; i++)
            {
                Button option = new Button();
                option.Style = (Style)Res("AcOption");
                option.Focusable = false; // a real click must not move focus out of the input
                option.Content = _matches[i];
                option.Tag = i;
                AutomationProperties.SetAutomationId(option, _id + "_Option" + i);
                AutomationProperties.SetName(option, _matches[i]);
                option.Click += Option_Click;
                _optionButtons.Add(option);
                list.Children.Add(option);
            }
            if (_matches.Count == 0)
            {
                _empty = new TextBlock();
                _empty.Style = (Style)Res("AcEmpty");
                _empty.Text = "No suggestions";
                AutomationProperties.SetAutomationId(_empty, _id + "_Empty");
                list.Children.Add(_empty);
            }
            scroller.Content = list;
            border.Child = scroller;
            root.Children.Add(border);
            _panel = root;
        }

        private void Highlight()
        {
            for (int i = 0; i < _optionButtons.Count; i++)
            {
                bool active = i == _active;
                if (active) _optionButtons[i].SetResourceReference(Control.BackgroundProperty, "BgDeep");
                else _optionButtons[i].Background = Brushes.Transparent;
                AutomationProperties.SetItemStatus(_optionButtons[i], active ? "active" : "");
                if (active) _optionButtons[i].BringIntoView();
            }
        }

        private void Option_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || !(button.Tag is int)) return;
            int index = (int)button.Tag;
            if (index < 0 || index >= _matches.Count) return;
            Choose(_matches[index]);
        }

        private void Choose(string value)
        {
            _suppress = true;
            try
            {
                if (Multiple)
                {
                    bool duplicate = false;
                    foreach (string tag in _tags)
                        if (string.Equals(tag, value, StringComparison.OrdinalIgnoreCase)) { duplicate = true; break; }
                    if (!duplicate)
                    {
                        _tags.Add(value);
                        RenderTags();
                        UpdateHint();
                    }
                    _input.Text = ""; // web choose() clears even on a duplicate
                }
                else _input.Text = value;
                Hide();
                _input.Focus();
            }
            finally { _suppress = false; }
        }

        private void Hide()
        {
            if (_outsideClick != null) { _outsideClick.Unhook(); _outsideClick = null; }
            if (_adorner != null)
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(_anchor);
                if (layer != null) layer.Remove(_adorner);
                _adorner = null;
            }
            _panel = null;
            _optionButtons.Clear();
            _active = -1;
        }

        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
