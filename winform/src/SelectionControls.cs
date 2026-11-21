using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UIComponents
{
    /// <summary>Single select that filters options as you type — fully custom drawn.</summary>
    public class ClaudeSearchSelect : UserControl
    {
        private readonly Label _value = new Label();
        private readonly ClaudeOptionList _list = new ClaudeOptionList();
        private readonly ClaudeTextBox _search = new ClaudeTextBox();
        private readonly Panel _popup = new Panel();
        private readonly string[] _options;
        private readonly string _placeholder;
        private bool _open;

        public string SelectedValue { get { return _value.Text == _placeholder ? "" : _value.Text; } }
        public event EventHandler SelectedChanged;

        public ClaudeSearchSelect(string[] options, string placeholder = "Select a value…")
        {
            _options = options ?? new string[0];
            _placeholder = placeholder;
            BackColor = Theme.Surface;
            Height = 38;

            _value.Text = placeholder;
            _value.ForeColor = Theme.Ink3;
            _value.Font = Theme.Sans(10f);
            _value.Location = new Point(13, 10);
            _value.AutoSize = true;
            var chevron = new Label { Text = "⌄", Font = Theme.Sans(11f), ForeColor = Theme.Ink3, Dock = DockStyle.Right, Width = 28, TextAlign = ContentAlignment.MiddleCenter };

            _search.Placeholder = "Search…";
            _search.Dock = DockStyle.Top;
            _search.TextChanged += (s, e) => Filter(_search.TextValue);
            _search.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Down) { _list.MoveSelection(1); e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Up) { _list.MoveSelection(-1); e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Enter) { _list.PickCurrent(); e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Escape) { _popup.Visible = false; _open = false; Height = 38; }
            };

            _list.Dock = DockStyle.Fill;
            _list.Picked += (s, v) => Choose(v);

            _popup.Controls.Add(_list);
            _popup.Controls.Add(_search);
            _popup.Location = new Point(0, 42);
            _popup.Size = new Size(300, 200);
            _popup.BackColor = Theme.Surface;
            _popup.Visible = false;

            Controls.Add(_popup);
            Controls.Add(chevron);
            Controls.Add(_value);
            Click += (s, e) => Toggle();
            _value.Click += (s, e) => Toggle();
            chevron.Click += (s, e) => Toggle();
            Filter("");
        }

        private void Toggle()
        {
            _open = !_open;
            _popup.Visible = _open;
            Height = _open ? 244 : 38;
            Invalidate();
        }

        private void Filter(string query)
        {
            var q = (query ?? "").Trim().ToLowerInvariant();
            _list.SetItems(_options.Where(o => o.ToLowerInvariant().Contains(q)));
        }

        private void Choose(string value)
        {
            _value.Text = value;
            _value.ForeColor = Theme.Ink;
            _popup.Visible = false;
            _open = false;
            Height = 38;
            if (SelectedChanged != null) SelectedChanged(this, EventArgs.Empty);
        }
    }

    /// <summary>Multiple selection from a fixed list, shown as chips — fully custom drawn.</summary>
    public class ClaudeMultiSelect : UserControl
    {
        private readonly FlowLayoutPanel _chips = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Theme.Surface, Padding = new Padding(8) };
        private readonly ClaudeCheckList _list = new ClaudeCheckList();
        private readonly Panel _dropdown = new Panel { Visible = false };
        private readonly ClaudeTextBox _search = new ClaudeTextBox();
        private readonly Label _placeholder = new Label();
        private readonly string[] _options;

        public string[] SelectedValues { get { return _list.CheckedValues; } }

        public ClaudeMultiSelect(string[] options, string placeholder)
        {
            _options = options ?? new string[0];
            BackColor = Theme.Surface;
            Height = 38;
            _placeholder.Text = placeholder;
            _placeholder.ForeColor = Theme.Ink3;
            _placeholder.Font = Theme.Sans(10f);
            _placeholder.Location = new Point(13, 10);
            _placeholder.AutoSize = true;

            var trigger = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Theme.Surface };
            trigger.Paint += (s, e) => Draw.RoundedBox(e.Graphics, new Rectangle(1, 1, trigger.Width - 2, trigger.Height - 2), 8, Theme.Surface, Theme.LineStrong);
            var chevron = new Label { Text = "⌄", Font = Theme.Sans(11f), ForeColor = Theme.Ink3, Dock = DockStyle.Right, Width = 28, TextAlign = ContentAlignment.MiddleCenter };
            trigger.Click += (s, e) => Toggle();
            trigger.Controls.Add(chevron);
            trigger.Controls.Add(_placeholder);
            trigger.Controls.Add(_chips);

            _search.Placeholder = "Search…";
            _search.Dock = DockStyle.Top;
            _search.TextChanged += (s, e) => Filter(_search.TextValue);
            _list.Dock = DockStyle.Fill;
            _list.SetItems(_options);

            var selectAll = new ClaudeButton { Text = "Select all", Variant = ButtonVariant.Ghost, Dock = DockStyle.Left, Width = 90, Height = 30 };
            selectAll.Click += (s, e) => { _list.CheckAll(true); Render(); };
            var clear = new ClaudeButton { Text = "Clear all", Variant = ButtonVariant.Ghost, Dock = DockStyle.Left, Width = 84, Height = 30 };
            clear.Click += (s, e) => { _list.CheckAll(false); Render(); };
            var done = new ClaudeButton { Text = "Done", Variant = ButtonVariant.Primary, Dock = DockStyle.Right, Width = 70, Height = 28 };
            done.Click += (s, e) => Toggle();
            var foot = new Panel { Dock = DockStyle.Bottom, Height = 36, BackColor = Theme.Surface };
            foot.Controls.Add(done);
            foot.Controls.Add(clear);
            foot.Controls.Add(selectAll);

            _dropdown.Controls.Add(foot);
            _dropdown.Controls.Add(_list);
            _dropdown.Controls.Add(_search);
            _dropdown.Location = new Point(0, 42);
            _dropdown.Size = new Size(360, 230);
            _dropdown.BackColor = Theme.Surface;

            Controls.Add(_dropdown);
            Controls.Add(trigger);
            Render();
        }

        private void Toggle()
        {
            _dropdown.Visible = !_dropdown.Visible;
            Height = _dropdown.Visible ? 280 : 38;
        }

        private void Filter(string query)
        {
            var q = (query ?? "").Trim().ToLowerInvariant();
            _list.SetItems(_options.Where(o => o.ToLowerInvariant().Contains(q)));
        }

        private void Render()
        {
            _chips.Controls.Clear();
            var values = SelectedValues;
            _placeholder.Visible = values.Length == 0;
            foreach (var value in values)
            {
                var chip = new ClaudeBadge { Text = value, Tone = BadgeTone.Brand, ShowDot = false, AutoSize = false, Width = TextRenderer.MeasureText(value, Font).Width + 34, Height = 22, Margin = new Padding(2, 2, 2, 0) };
                chip.Click += (s, e) => { _list.SetChecked(value, false); Render(); };
                _chips.Controls.Add(chip);
            }
        }
    }

    /// <summary>Full date plus hour / minute / second fields.</summary>
    public class ClaudeDateTimePicker : UserControl
    {
        private readonly ClaudeDatePicker _date = new ClaudeDatePicker();
        private readonly ClaudeNumberBox _hour = new ClaudeNumberBox(0, 23, 9);
        private readonly ClaudeNumberBox _minute = new ClaudeNumberBox(0, 59, 30);
        private readonly ClaudeNumberBox _second = new ClaudeNumberBox(0, 59, 0);

        public DateTime Value
        {
            get { return _date.Value.Date.AddHours((double)_hour.Value).AddMinutes((double)_minute.Value).AddSeconds((double)_second.Value); }
            set { _date.Value = value; _hour.Value = value.Hour; _minute.Value = value.Minute; _second.Value = value.Second; }
        }

        public ClaudeDateTimePicker(bool includeSeconds = false)
        {
            BackColor = Theme.Surface;
            Height = 38;
            _date.Location = new Point(0, 0);
            _date.Width = 170;
            Controls.Add(_date);

            int x = 180;
            AddField(_hour, "h", ref x);
            AddField(_minute, "m", ref x);
            if (includeSeconds) AddField(_second, "s", ref x);
        }

        private void AddField(ClaudeNumberBox field, string suffix, ref int x)
        {
            field.Location = new Point(x, 0);
            var label = new Label { Text = suffix, Font = Theme.Sans(8.5f), ForeColor = Theme.Ink3, Location = new Point(x + 60, 10), AutoSize = true };
            Controls.Add(label);
            Controls.Add(field);
            x += 66;
        }
    }
}
