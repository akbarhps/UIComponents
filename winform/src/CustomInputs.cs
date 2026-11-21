using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace UIComponents
{
    /// <summary>Custom-drawn text input: no native border, focus ring and placeholder like the web .input.</summary>
    public class ClaudeTextBox : UserControl
    {
        private readonly TextBox _box = new TextBox();
        private readonly Label _placeholder = new Label();
        private bool _focused;

        public string TextValue
        {
            get { return _box.Text; }
            set { _box.Text = value; UpdatePlaceholder(); }
        }
        public string Placeholder
        {
            get { return _placeholder.Text; }
            set { _placeholder.Text = value; UpdatePlaceholder(); }
        }
        public bool Multiline
        {
            get { return _box.Multiline; }
            set { _box.Multiline = value; _box.AcceptsReturn = value; Arrange(); }
        }
        public bool Password { get { return _box.UseSystemPasswordChar; } set { _box.UseSystemPasswordChar = value; } }
        public bool HasError { get; set; }

        public ClaudeTextBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Surface;
            Height = 38;
            _box.Font = Theme.Sans(10f);
            _box.ForeColor = Theme.Ink;
            _box.BorderStyle = BorderStyle.None;
            _box.BackColor = Theme.Surface;
            _box.Dock = DockStyle.Fill;
            _box.Enter += (s, e) => { _focused = true; Invalidate(); };
            _box.Leave += (s, e) => { _focused = false; Invalidate(); };
            _box.TextChanged += (s, e) => UpdatePlaceholder();
            _placeholder.Font = Theme.Sans(10f);
            _placeholder.ForeColor = Theme.Ink3;
            _placeholder.Location = new Point(13, 9);
            _placeholder.AutoSize = true;
            _placeholder.Visible = false;
            Controls.Add(_box);
            Controls.Add(_placeholder);
            Arrange();
        }

        private void UpdatePlaceholder()
        {
            _placeholder.Visible = string.IsNullOrEmpty(_box.Text) && !string.IsNullOrEmpty(_placeholder.Text);
            _placeholder.BringToFront();
        }

        private void Arrange()
        {
            _box.Margin = Padding.Empty;
            _box.Multiline = Multiline;
            Height = Multiline ? 80 : 38;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Color border = HasError ? Theme.Danger : _focused ? Theme.Brand : Theme.LineStrong;
            Draw.RoundedBox(e.Graphics, new Rectangle(1, 1, Width - 2, Height - 2), 8, Theme.Surface, border, 1);
            if (_focused && !HasError)
                Draw.RoundedBox(e.Graphics, new Rectangle(3, 3, Width - 6, Height - 6), 7, Color.Transparent, Theme.BrandSoft, 2);
        }
    }

    /// <summary>Custom-drawn select with a popup list and no native combo chrome.</summary>
    public class ClaudeSelectBox : UserControl
    {
        private readonly Label _value = new Label();
        private readonly Label _chevron = new Label();
        private readonly ListBox _list = new ListBox();
        private readonly List<string> _options = new List<string>();
        private readonly Label _placeholder = new Label();
        private bool _focused;

        public string SelectedValue
        {
            get { return _value.Text == Placeholder ? "" : _value.Text; }
            set { _value.Text = string.IsNullOrEmpty(value) ? Placeholder : value; _value.ForeColor = string.IsNullOrEmpty(value) ? Theme.Ink3 : Theme.Ink; }
        }
        public string Placeholder { get; set; }

        public ClaudeSelectBox(string placeholder = "Select…")
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            Placeholder = placeholder;
            BackColor = Theme.Surface;
            Height = 38;

            _value.Text = placeholder;
            _value.ForeColor = Theme.Ink3;
            _value.Font = Theme.Sans(10f);
            _value.Location = new Point(13, 9);
            _value.AutoSize = true;

            _chevron.Text = "⌄";
            _chevron.Font = Theme.Sans(11f);
            _chevron.ForeColor = Theme.Ink3;
            _chevron.Dock = DockStyle.Right;
            _chevron.Width = 30;
            _chevron.TextAlign = ContentAlignment.MiddleCenter;

            _list.Font = Theme.Sans(10f);
            _list.BorderStyle = BorderStyle.FixedSingle;
            _list.Location = new Point(0, 40);
            _list.Width = 260;
            _list.Height = 140;
            _list.Visible = false;
            _list.Click += (s, e) => { if (_list.SelectedIndex >= 0) Pick(_list.SelectedItem.ToString()); };

            Controls.Add(_list);
            Controls.Add(_chevron);
            Controls.Add(_value);
            Click += (s, e) =>
            {
                _focused = true;
                _list.Visible = !_list.Visible;
                Invalidate();
            };
        }

        public void SetOptions(IEnumerable<string> options)
        {
            _options.Clear();
            _options.AddRange(options);
            _list.Items.Clear();
            foreach (var o in _options) _list.Items.Add(o);
        }

        private void Pick(string value)
        {
            SelectedValue = value;
            _list.Visible = false;
            _focused = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Color border = _focused ? Theme.Brand : Theme.LineStrong;
            Draw.RoundedBox(e.Graphics, new Rectangle(1, 1, Width - 2, Height - 2), 8, Theme.Surface, border, 1);
        }
    }
}
