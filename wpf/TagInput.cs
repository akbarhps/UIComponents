using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace UIComponents.Wpf
{
    // Code-only tag input: wrapping chips plus a bare text field, with the dynamic hint below.
    public class TagInput : Grid
    {
        private readonly List<string> _tags = new List<string>();
        private Border _box;
        private Border _focusRing;
        private ChipWrapPanel _chipHost;
        private TextBox _input;
        private TextBlock _hint;
        private string _id;
        private bool _removePress;

        public string Label { get; set; }
        public string Placeholder { get; set; }
        public string InitialTags { get; set; }
        public int Max { get; set; }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _id = AutomationProperties.GetAutomationId(this);
            if (string.IsNullOrEmpty(_id)) _id = Name;
            if (!string.IsNullOrEmpty(InitialTags))
                foreach (string part in InitialTags.Split(','))
                {
                    string tag = part.Trim();
                    if (tag.Length > 0) _tags.Add(tag);
                }
            BuildTree();
            Render();
        }

        private static object Res(string key)
        {
            return Application.Current.FindResource(key);
        }

        private void BuildTree()
        {
            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid shell = new Grid();
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

            _input = new TextBox();
            _input.Style = (Style)Res("BareInput");
            _input.Tag = Placeholder;
            AutomationProperties.SetAutomationId(_input, _id + "_Input");
            AutomationProperties.SetName(_input, Label);
            _input.PreviewKeyDown += Input_PreviewKeyDown;
            _input.PreviewTextInput += Input_PreviewTextInput;
            _input.LostKeyboardFocus += Input_LostKeyboardFocus;
            _chipHost.Children.Add(_input);

            _box = new Border();
            _box.Style = (Style)Res("TagBox");
            _box.Child = _chipHost;
            _box.IsKeyboardFocusWithinChanged += Box_FocusChanged;
            _box.PreviewMouseDown += Box_PreviewMouseDown;
            AutomationProperties.SetAutomationId(_box, _id + "_Box");

            shell.Children.Add(_focusRing);
            shell.Children.Add(_box);

            _hint = new TextBlock();
            _hint.Style = (Style)Res("FieldHint");
            AutomationProperties.SetAutomationId(_hint, _id + "_Hint");
            Grid.SetRow(_hint, 1);

            Children.Add(shell);
            Children.Add(_hint);
        }

        private void Box_FocusChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _focusRing.Visibility = _box.IsKeyboardFocusWithin ? Visibility.Visible : Visibility.Collapsed;
        }

        // Web T5: clicking the container focuses the field, but a chip-remove click must not.
        private void Box_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject target = e.OriginalSource as DependencyObject;
            while (target != null && target != _box)
            {
                Button remove = target as Button;
                if (remove != null && AutomationProperties.GetAutomationId(remove) != null
                    && AutomationProperties.GetAutomationId(remove).Contains("_Remove"))
                {
                    // Only a real mouse press suppresses blur-add; keyboard focus must still add.
                    _removePress = true;
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(delegate { _removePress = false; }));
                    return;
                }
                target = VisualTreeHelper.GetParent(target);
            }
            _input.Focus();
        }

        private void Input_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Layout-aware comma, like the web's e.key === ','.
            if (e.Text == ",")
            {
                e.Handled = true;
                AddFromField();
            }
        }

        private void Input_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                e.Handled = true;
                AddFromField();
            }
            else if (e.Key == Key.Back && _input.Text.Length == 0 && _tags.Count > 0)
            {
                e.Handled = true;
                _tags.RemoveAt(_tags.Count - 1);
                Render();
            }
        }

        private void Input_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // Web T5: a mouse press on a remove button must not blur-add; Shift+Tab still does.
            if (_removePress)
            {
                _removePress = false;
                return;
            }
            // Web T4: blur adds the whole value without a comma split.
            if (_input.Text.Trim().Length > 0)
            {
                Add(_input.Text); // no split
                _input.Text = "";
            }
        }

        private void AddFromField()
        {
            string text = _input.Text;
            _input.Text = "";
            foreach (string part in text.Split(',')) Add(part);
        }

        private void Add(string value)
        {
            string tag = (value ?? "").Trim();
            if (tag.Length == 0 || _tags.Count >= Max) return;
            foreach (string existing in _tags)
                if (string.Equals(existing, tag, StringComparison.OrdinalIgnoreCase)) return;
            _tags.Add(tag);
            Render();
        }

        private void Render()
        {
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
            _hint.Text = _tags.Count + " of " + Max + " tags";
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Button button = sender as Button;
            if (button == null || !(button.Tag is int)) return;
            int index = (int)button.Tag;
            if (index < 0 || index >= _tags.Count) return;
            bool hadFocus = _box.IsKeyboardFocusWithin;
            _tags.RemoveAt(index);
            Render();
            if (hadFocus) _input.Focus();
        }
    }
}
