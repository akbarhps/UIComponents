using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UIComponents
{
    /// <summary>Single select that filters options as you type.</summary>
    public class ClaudeSearchSelect : UserControl
    {
        private readonly TextBox _input = new TextBox();
        private readonly ListBox _list = new ListBox();
        private readonly string[] _options;
        private readonly Label _chevron = new Label();
        private readonly string _placeholder;

        public string SelectedValue { get { return _input.Text == _placeholder ? "" : _input.Text; } }
        public event EventHandler SelectedChanged;

        public ClaudeSearchSelect(string[] options, string placeholder = "Select a value…")
        {
            _options = options ?? new string[0];
            _placeholder = placeholder;
            BackColor = Theme.Surface;
            Height = 120;

            var trigger = new Panel { Height = 38, Dock = DockStyle.Top, BackColor = Theme.Surface };
            trigger.Paint += (s, e) => Draw.RoundedBox(e.Graphics, new Rectangle(1, 1, trigger.Width - 2, trigger.Height - 2), 8, Theme.Surface, Theme.LineStrong);
            _input.Font = Theme.Sans(10f);
            _input.BorderStyle = BorderStyle.None;
            _input.Location = new Point(12, 9);
            _input.Width = 300;
            _input.ReadOnly = true;
            _input.Text = _placeholder;
            _input.ForeColor = Theme.Ink3;
            _chevron.Text = "⌄";
            _chevron.Font = Theme.Sans(11f);
            _chevron.ForeColor = Theme.Ink3;
            _chevron.Location = new Point(trigger.Width - 30, 8);
            _chevron.Click += (s, e) => { _list.Visible = !_list.Visible; if (_list.Visible) _input.Focus(); };
            _input.Click += (s, e) =>
            {
                if (_input.Text == _placeholder) { _input.Text = ""; _input.ForeColor = Theme.Ink; }
                _list.Visible = true;
            };
            _input.TextChanged += (s, e) => Filter(_input.Text);
            _input.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Down && _list.Items.Count > 0) { if (_list.SelectedIndex < _list.Items.Count - 1) _list.SelectedIndex++; e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Up && _list.Items.Count > 0) { if (_list.SelectedIndex > 0) _list.SelectedIndex--; e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Enter && _list.SelectedIndex >= 0) { Choose(_list.SelectedItem.ToString()); e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Escape) _list.Visible = false;
            };
            _list.Dock = DockStyle.Fill;
            _list.Font = Theme.Sans(9.5f);
            _list.BorderStyle = BorderStyle.FixedSingle;
            _list.Visible = false;
            _list.Click += (s, e) => { if (_list.SelectedIndex >= 0) Choose(_list.SelectedItem.ToString()); };

            trigger.Controls.Add(_chevron);
            trigger.Controls.Add(_input);
            Controls.Add(_list);
            Controls.Add(trigger);
            Filter("");
        }

        private void Filter(string query)
        {
            _list.Items.Clear();
            foreach (var o in _options)
                if (o.ToLowerInvariant().Contains(query.ToLowerInvariant())) _list.Items.Add(o);
        }

        private void Choose(string value)
        {
            _input.Text = value;
            _input.ForeColor = Theme.Ink;
            _list.Visible = false;
            if (SelectedChanged != null) SelectedChanged(this, EventArgs.Empty);
        }
    }

    /// <summary>Multiple selection from a fixed list, shown as chips.</summary>
    public class ClaudeMultiSelect : UserControl
    {
        private readonly FlowLayoutPanel _chips = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Theme.Surface, Padding = new Padding(8) };
        private readonly CheckedListBox _list = new CheckedListBox { Dock = DockStyle.Fill, Font = Theme.Sans(9.5f), BorderStyle = BorderStyle.FixedSingle };
        private readonly Panel _dropdown = new Panel { Visible = false };
        private readonly TextBox _search = new TextBox { Font = Theme.Sans(9.5f), Dock = DockStyle.Top, BorderStyle = BorderStyle.FixedSingle };
        private readonly Label _placeholder = new Label();
        public string[] SelectedValues
        {
            get
            {
                var result = new List<string>();
                foreach (var item in _list.CheckedItems) result.Add(item.ToString());
                return result.ToArray();
            }
        }

        public ClaudeMultiSelect(string[] options, string placeholder)
        {
            BackColor = Theme.Surface;
            Height = 46;
            _placeholder.Text = placeholder;
            _placeholder.ForeColor = Theme.Ink3;
            _placeholder.Font = Theme.Sans(10f);
            _placeholder.Location = new Point(10, 12);
            _placeholder.AutoSize = true;

            var trigger = new Panel { Height = 46, Dock = DockStyle.Top, BackColor = Theme.Surface };
            trigger.Paint += (s, e) => Draw.RoundedBox(e.Graphics, new Rectangle(1, 1, trigger.Width - 2, trigger.Height - 2), 8, Theme.Surface, Theme.LineStrong);
            var chevron = new Label { Text = "⌄", Font = Theme.Sans(11f), ForeColor = Theme.Ink3, Dock = DockStyle.Right, Width = 30, TextAlign = ContentAlignment.MiddleCenter };
            chevron.Click += (s, e) => Toggle();
            trigger.Controls.Add(chevron);
            trigger.Controls.Add(_placeholder);
            trigger.Controls.Add(_chips);
            trigger.Click += (s, e) => Toggle();

            foreach (var o in options ?? new string[0]) _list.Items.Add(o);
            _search.TextChanged += (s, e) =>
            {
                // CheckedListBox has no filtering; rebuild from the source list.
                var checkedItems = SelectedValues;
                _list.Items.Clear();
                foreach (var o in options ?? new string[0])
                    if (o.ToLowerInvariant().Contains(_search.Text.Trim().ToLowerInvariant())) _list.Items.Add(o);
                foreach (var item in checkedItems)
                    if (_list.Items.Contains(item)) _list.SetItemChecked(_list.Items.IndexOf(item), true);
            };

            var selectAll = new ClaudeButton { Text = "Select all", Variant = ButtonVariant.Ghost, Dock = DockStyle.Left, Width = 90, Height = 32 };
            selectAll.Click += (s, e) => { for (int i = 0; i < _list.Items.Count; i++) _list.SetItemChecked(i, true); Render(); };
            var clear = new ClaudeButton { Text = "Clear all", Variant = ButtonVariant.Ghost, Dock = DockStyle.Left, Width = 84, Height = 32 };
            clear.Click += (s, e) => { for (int i = 0; i < _list.Items.Count; i++) _list.SetItemChecked(i, false); Render(); };
            var done = new ClaudeButton { Text = "Done", Variant = ButtonVariant.Primary, Dock = DockStyle.Right, Width = 70, Height = 30, Margin = new Padding(6) };
            done.Click += (s, e) => { _dropdown.Visible = false; Height = 46; Render(); };
            var foot = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Theme.Surface };
            foot.Controls.Add(done);
            foot.Controls.Add(clear);
            foot.Controls.Add(selectAll);

            _dropdown.Controls.Add(foot);
            _dropdown.Controls.Add(_list);
            _dropdown.Controls.Add(_search);
            _dropdown.Location = new Point(0, 48);
            _dropdown.Size = new Size(360, 230);
            _dropdown.BackColor = Theme.Surface;

            Controls.Add(_dropdown);
            Controls.Add(trigger);
            Render();
        }

        private void Toggle()
        {
            _dropdown.Visible = !_dropdown.Visible;
            Height = _dropdown.Visible ? 288 : 46;
            if (_dropdown.Visible) _search.Focus();
        }

        private void Render()
        {
            _chips.Controls.Clear();
            var values = SelectedValues;
            _placeholder.Visible = values.Length == 0;
            foreach (var value in values)
            {
                var chip = new ClaudeBadge { Text = value, Tone = BadgeTone.Brand, ShowDot = false, AutoSize = false, Width = TextRenderer.MeasureText(value, Font).Width + 34, Height = 22, Margin = new Padding(2, 2, 2, 0) };
                chip.Click += (s, e) => { _list.SetItemChecked(_list.Items.IndexOf(value), false); Render(); };
                _chips.Controls.Add(chip);
            }
        }
    }

    /// <summary>Full date plus hour / minute / second fields.</summary>
    public class ClaudeDateTimePicker : UserControl
    {
        private readonly DateTimePicker _date = new DateTimePicker();
        private readonly NumericUpDown _hour = new NumericUpDown { Minimum = 0, Maximum = 23 };
        private readonly NumericUpDown _minute = new NumericUpDown { Minimum = 0, Maximum = 59 };
        private readonly NumericUpDown _second = new NumericUpDown { Minimum = 0, Maximum = 59 };

        public DateTime Value
        {
            get { return _date.Value.Date.AddHours((double)_hour.Value).AddMinutes((double)_minute.Value).AddSeconds((double)_second.Value); }
            set { _date.Value = value; _hour.Value = value.Hour; _minute.Value = value.Minute; _second.Value = value.Second; }
        }

        public ClaudeDateTimePicker(bool includeSeconds = false)
        {
            BackColor = Theme.Surface;
            Height = 38;
            _date.Font = Theme.Sans(10f);
            _date.Format = DateTimePickerFormat.Custom;
            _date.CustomFormat = "MMM d, yyyy";
            _date.Location = new Point(0, 4);
            _date.Width = 150;
            Controls.Add(_date);

            int x = 160;
            AddField(_hour, "h", ref x);
            AddField(_minute, "m", ref x);
            if (includeSeconds) AddField(_second, "s", ref x);
        }

        private void AddField(NumericUpDown field, string suffix, ref int x)
        {
            field.Font = Theme.Sans(10f);
            field.Width = 46;
            field.Location = new Point(x, 4);
            var label = new Label { Text = suffix, Font = Theme.Sans(8.5f), ForeColor = Theme.Ink3, Location = new Point(x + 48, 10), AutoSize = true };
            Controls.Add(label);
            Controls.Add(field);
            x += 66;
        }
    }
}
