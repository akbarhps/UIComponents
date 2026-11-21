using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace UIComponents
{
    /// <summary>Label + input + hint + error, mirroring the web .field block.</summary>
    public class ClaudeField : UserControl
    {
        private readonly Label _label = new Label();
        private readonly Label _hint = new Label();
        private readonly Label _error = new Label();
        private readonly ClaudeTextBox _box = new ClaudeTextBox();
        private readonly ClaudeSelectBox _select = new ClaudeSelectBox();

        public string LabelText { get { return _label.Text; } set { _label.Text = value; } }
        public string Hint { get { return _hint.Text; } set { _hint.Text = value; } }
        public string ErrorText { get { return _error.Text; } set { _error.Text = value; } }
        public bool HasError
        {
            get { return _error.Visible; }
            set { _error.Visible = value; _box.HasError = value; Invalidate(); }
        }
        public bool Multiline
        {
            get { return _box.Multiline; }
            set { _box.Multiline = value; Arrange(); }
        }
        public bool Password { get { return _box.Password; } set { _box.Password = value; } }
        private bool _isSelect;
        public bool IsSelect
        {
            get { return _isSelect; }
            set { _isSelect = value; _select.Visible = value; _box.Visible = !value; Arrange(); }
        }
        public string[] Options
        {
            set
            {
                _select.SetOptions(value ?? new string[0]);
                if (value != null && value.Length > 0) _select.SelectedValue = value[0];
            }
        }
        public string TextValue
        {
            get { return _isSelect ? _select.SelectedValue : _box.TextValue; }
            set { if (_isSelect) _select.SelectedValue = value; else _box.TextValue = value; }
        }

        public ClaudeField()
        {
            BackColor = Theme.Surface;
            Height = 90;
            _label.Font = Theme.Sans(9.5f, FontStyle.Bold);
            _label.ForeColor = Theme.Ink;
            _label.AutoSize = true;
            _hint.Font = Theme.Sans(8.5f);
            _hint.ForeColor = Theme.Ink3;
            _hint.AutoSize = true;
            _error.Font = Theme.Sans(8.5f);
            _error.ForeColor = Theme.Danger;
            _error.AutoSize = true;
            _error.Visible = false;

            _box.Placeholder = "";
            _select.Visible = false;

            Controls.AddRange(new Control[] { _label, _box, _select, _hint, _error });
            Arrange();
        }

        private void Arrange()
        {
            _label.Location = new Point(0, 0);
            int inputY = 20;
            _box.SetBounds(0, inputY, Math.Max(80, Width), _box.Multiline ? 80 : 38);
            _select.SetBounds(0, inputY, Math.Max(80, Width), 38);
            int below = inputY + (_box.Multiline ? 80 : 38) + 4;
            _hint.Location = new Point(0, below);
            _error.Location = new Point(0, below + (_hint.Visible ? 14 : 0));
            Height = below + 28;
        }

        protected override void OnResize(EventArgs e) { base.OnResize(e); Arrange(); }
    }

    /// <summary>Freeform chips: type then Enter or comma; Backspace removes the last.</summary>
    public class ClaudeTagInput : UserControl
    {
        private readonly FlowLayoutPanel _chips;
        private readonly TextBox _input;
        private readonly Label _hint;
        private readonly List<string> _tags = new List<string>();
        public int MaxTags = 0;

        public string[] Tags { get { return _tags.ToArray(); } }
        public string HintText { get { return _hint.Text; } set { _hint.Text = value; } }

        public ClaudeTagInput(string[] initial = null)
        {
            BackColor = Theme.Surface;
            _chips = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(6), BackColor = Theme.Surface };
            _input = new TextBox { BorderStyle = BorderStyle.None, Width = 130, Font = Theme.Sans(10f), ForeColor = Theme.Ink };
            _hint = new Label { Dock = DockStyle.Bottom, Height = 20, Font = Theme.Sans(8.5f), ForeColor = Theme.Ink3 };
            var border = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = Theme.Surface, Padding = new Padding(1) };
            border.Paint += (s, e) => Draw.RoundedBox(e.Graphics, new Rectangle(1, 1, border.Width - 2, border.Height - 2), 8, Theme.Surface, Theme.LineStrong, 1);
            _input.Location = new Point(8, 12);
            border.Controls.Add(_input);
            Controls.Add(_hint);
            Controls.Add(border);
            Controls.Add(_chips);

            _input.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Oemcomma)
                {
                    e.SuppressKeyPress = true;
                    foreach (var part in _input.Text.Split(',')) Add(part);
                    _input.Clear();
                }
                else if (e.KeyCode == Keys.Back && _input.Text.Length == 0 && _tags.Count > 0)
                {
                    _tags.RemoveAt(_tags.Count - 1);
                    Render();
                }
            };
            if (initial != null) foreach (var t in initial) Add(t);
            Render();
        }

        private void Add(string value)
        {
            value = (value ?? "").Trim();
            if (value.Length == 0) return;
            if (MaxTags > 0 && _tags.Count >= MaxTags) return;
            if (_tags.Any(t => string.Equals(t, value, StringComparison.OrdinalIgnoreCase))) return;
            _tags.Add(value);
            Render();
        }

        private void Render()
        {
            _chips.Controls.Clear();
            _chips.Controls.Add(_input);
            foreach (var tag in _tags)
            {
                var chip = new ClaudeBadge { Text = tag, Tone = BadgeTone.Brand, ShowDot = false, AutoSize = false, Width = TextRenderer.MeasureText(tag, _input.Font).Width + 36, Height = 24, Margin = new Padding(3, 4, 3, 0) };
                chip.Click += (s, e) => { _tags.Remove(tag); Render(); };
                chip.Cursor = Cursors.Hand;
                _chips.Controls.Add(chip);
            }
            _input.Location = new Point(8, 12);
            _hint.Text = _tags.Count + (MaxTags > 0 ? " of " + MaxTags : "") + " tags";
        }
    }

    /// <summary>Plain-Text list with arrow-key and Enter selection.</summary>
    public class ClaudeAutocomplete : UserControl
    {
        private readonly TextBox _input = new TextBox();
        private readonly ListBox _list = new ListBox();
        private readonly string[] _options;
        private readonly bool _multiple;
        private readonly FlowLayoutPanel _chips;
        private readonly List<string> _picked = new List<string>();

        public string TextValue { get { return _input.Text; } }
        public string[] Picked { get { return _picked.ToArray(); } }

        public ClaudeAutocomplete(string[] options, bool multiple = false)
        {
            _options = options ?? new string[0];
            _multiple = multiple;
            BackColor = Theme.Surface;
            Height = multiple ? 110 : 78;

            _input.Font = Theme.Sans(10f);
            _input.BorderStyle = BorderStyle.FixedSingle;
            _input.Dock = DockStyle.Top;
            _input.Location = new Point(0, 0);

            _list.Font = Theme.Sans(9.5f);
            _list.BorderStyle = BorderStyle.FixedSingle;
            _list.Visible = false;
            _list.Height = 100;
            _list.Dock = DockStyle.Top;

            _chips = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = multiple ? 30 : 0, BackColor = Theme.Bg };

            Controls.Add(_chips);
            Controls.Add(_list);
            Controls.Add(_input);

            _input.TextChanged += (s, e) => Filter();
            _input.KeyDown += (s, e) =>
            {
                if (!_list.Visible) return;
                if (e.KeyCode == Keys.Down) { if (_list.SelectedIndex < _list.Items.Count - 1) _list.SelectedIndex++; e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Up) { if (_list.SelectedIndex > 0) _list.SelectedIndex--; e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Enter && _list.SelectedIndex >= 0)
                {
                    e.SuppressKeyPress = true;
                    Choose(_list.SelectedItem.ToString());
                }
                else if (e.KeyCode == Keys.Escape) _list.Visible = false;
            };
            _list.Click += (s, e) => { if (_list.SelectedIndex >= 0) Choose(_list.SelectedItem.ToString()); };
        }

        private void Filter()
        {
            var q = _input.Text.Trim().ToLowerInvariant();
            _list.Items.Clear();
            foreach (var o in _options)
                if (o.ToLowerInvariant().Contains(q) && !_picked.Contains(o)) _list.Items.Add(o);
            _list.Visible = _list.Items.Count > 0;
        }

        private void Choose(string value)
        {
            if (_multiple)
            {
                _picked.Add(value);
                var chip = new ClaudeBadge { Text = value, Tone = BadgeTone.Brand, ShowDot = false, Width = TextRenderer.MeasureText(value, Font).Width + 36, Height = 24, Margin = new Padding(2, 3, 2, 0) };
                _chips.Controls.Add(chip);
                _input.Clear();
            }
            else _input.Text = value;
            _list.Visible = false;
        }
    }

    /// <summary>File picker with a scrollable list of selected files.</summary>
    public class ClaudeFileInput : UserControl
    {
        private readonly ClaudeFileList _list = new ClaudeFileList();
        public string Filter = "All files|*.*";
        public bool MultiSelect = true;

        public string[] Files { get { return _list.Files; } }

        public ClaudeFileInput()
        {
            BackColor = Theme.Surface;
            Height = 160;
            var btn = new ClaudeButton { Text = "Browse files…", Variant = ButtonVariant.Secondary, Dock = DockStyle.Top, Height = 38 };
            btn.Click += (s, e) =>
            {
                var dlg = new OpenFileDialog { Multiselect = MultiSelect, Filter = Filter };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (var f in dlg.FileNames) _list.Add(f);
                }
            };
            var hint = new Label { Text = "or click to browse · selected files are listed below", Dock = DockStyle.Bottom, Height = 22, Font = Theme.Sans(8.5f), ForeColor = Theme.Ink3 };
            _list.Dock = DockStyle.Fill;
            Controls.Add(_list);
            Controls.Add(btn);
            Controls.Add(hint);
        }
    }

    /// <summary>Date / month / year picker — fully custom drawn, no native chrome.</summary>
    public enum DatePickerMode { Date, MonthYear, Year }

    public class ClaudeDatePicker : UserControl
    {
        private readonly ClaudeCalendar _calendar = new ClaudeCalendar();
        private readonly Label _value = new Label();
        private readonly Label _chevron = new Label();
        private DatePickerMode _mode = DatePickerMode.Date;
        private DateTime _selectedDate = DateTime.Today;
        private bool _open;

        public DatePickerMode Mode
        {
            get { return _mode; }
            set { _mode = value; _calendar.Mode = value; UpdateDisplay(); Invalidate(); }
        }
        public DateTime Value { get { return _selectedDate; } set { _selectedDate = value; _calendar.Value = value; UpdateDisplay(); } }
        public string Display { get { return _valueLabel; } }
        private string _valueLabel;

        public ClaudeDatePicker()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Surface;
            Height = 38;
            _value.Font = Theme.Sans(10f);
            _value.ForeColor = Theme.Ink;
            _value.Location = new Point(12, 9);
            _value.AutoSize = true;
            _chevron.Text = "⌄";
            _chevron.Font = Theme.Sans(10f);
            _chevron.ForeColor = Theme.Ink3;
            _chevron.Dock = DockStyle.Right;
            _chevron.Width = 28;
            _chevron.TextAlign = ContentAlignment.MiddleCenter;

            _calendar.Location = new Point(0, 42);
            _calendar.Size = new Size(280, 250);
            _calendar.Visible = false;
            _calendar.ValueChanged += (s, e) =>
            {
                _selectedDate = _calendar.Value;
                UpdateDisplay();
                _calendar.Visible = false;
                _open = false;
                Height = 38;
                Invalidate();
            };

            Controls.Add(_calendar);
            Controls.Add(_chevron);
            Controls.Add(_value);
            Action toggle = () =>
            {
                _open = !_open;
                _calendar.Visible = _open;
                Height = _open ? 296 : 38;
                Invalidate();
            };
            Click += (s, e) => toggle();
            _value.Click += (s, e) => toggle();
            _chevron.Click += (s, e) => toggle();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            _valueLabel = _mode == DatePickerMode.Year ? _selectedDate.Year.ToString()
                : _mode == DatePickerMode.MonthYear ? new DateTime(_selectedDate.Year, _selectedDate.Month, 1).ToString("MMMM yyyy")
                : _selectedDate.ToString("MMM d, yyyy");
            _value.Text = _valueLabel;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw.RoundedBox(e.Graphics, new Rectangle(1, 1, Width - 2, Height - 2), 8, Theme.Surface, Theme.LineStrong, 1);
        }
    }
}
