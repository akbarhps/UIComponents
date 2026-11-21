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

    /// <summary>Custom-drawn file list with per-item remove buttons.</summary>
    public class ClaudeFileList : Control
    {
        private readonly List<string> _files = new List<string>();
        private int _hover = -1;
        public event EventHandler<string> Removed;

        public string[] Files { get { return _files.ToArray(); } }

        public ClaudeFileList()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Surface;
            Cursor = Cursors.Hand;
        }

        public void Add(string file)
        {
            if (!_files.Contains(file)) { _files.Add(file); Invalidate(); }
        }

        private int RowAt(int y)
        {
            int row = (y - 6) / 40;
            return row >= 0 && row < _files.Count ? row : -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int row = RowAt(e.Y);
            if (row != _hover) { _hover = row; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = -1;
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            int row = RowAt(PointToClient(MousePosition).Y);
            if (row < 0) return;
            // Right edge of the row is the remove target.
            int x = PointToClient(MousePosition).X;
            if (x > Width - 40)
            {
                string file = _files[row];
                _files.RemoveAt(row);
                if (Removed != null) Removed(this, file);
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(Theme.Surface);
            for (int i = 0; i < _files.Count; i++)
            {
                var rect = new Rectangle(6, 6 + i * 40, Width - 12, 36);
                Draw.RoundedBox(g, rect, 8, i == _hover ? Theme.Surface2 : Theme.Surface, Theme.Line, 1);
                string name = System.IO.Path.GetFileName(_files[i]);
                Draw.CenteredText(g, name, new Rectangle(rect.X + 34, rect.Y, rect.Width - 80, rect.Height), Theme.Sans(9.5f), Theme.Ink, HorizontalAlignment.Left);
                var size = new System.IO.FileInfo(_files[i]).Length;
                string sizeText = size < 1024 ? size + " B" : size < 1024 * 1024 ? (size / 1024) + " KB" : (size / 1024 / 1024) + " MB";
                Draw.CenteredText(g, sizeText, new Rectangle(rect.Right - 110, rect.Y, 70, rect.Height), Theme.Sans(8.5f), Theme.Ink3, HorizontalAlignment.Right);
                Draw.CenteredText(g, "×", new Rectangle(rect.Right - 34, rect.Y, 28, rect.Height), Theme.Sans(12f), Theme.Ink3);
            }
        }
    }

    /// <summary>Custom-drawn option list with hover and keyboard navigation.</summary>
    public class ClaudeOptionList : Control
    {
        private readonly List<string> _items = new List<string>();
        private int _selected = -1;
        private int _hover = -1;
        public event EventHandler<string> Picked;

        public int Count { get { return _items.Count; } }

        public ClaudeOptionList()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Surface;
            Cursor = Cursors.Hand;
        }

        public void SetItems(IEnumerable<string> items)
        {
            _items.Clear();
            if (items != null) _items.AddRange(items);
            _selected = -1;
            _hover = -1;
            Invalidate();
        }

        public void MoveSelection(int delta)
        {
            if (_items.Count == 0) return;
            _selected = Math.Max(0, Math.Min(_items.Count - 1, _selected + delta));
            Invalidate();
        }

        public void PickCurrent()
        {
            if (_selected >= 0 && _selected < _items.Count && Picked != null) Picked(this, _items[_selected]);
        }

        private int RowAt(int y)
        {
            int row = (y - 4) / 34;
            return row >= 0 && row < _items.Count ? row : -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int row = RowAt(e.Y);
            if (row != _hover) { _hover = row; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = -1;
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            int row = RowAt(PointToClient(MousePosition).Y);
            if (row >= 0 && Picked != null) Picked(this, _items[row]);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(Theme.Surface);
            for (int i = 0; i < _items.Count; i++)
            {
                var rect = new Rectangle(4, 4 + i * 34, Width - 8, 32);
                if (i == _selected || i == _hover)
                    Draw.RoundedBox(g, rect, 7, i == _selected ? Theme.BrandSoft : Theme.BgDeep, Color.Transparent, 0);
                Draw.CenteredText(g, _items[i], new Rectangle(rect.X + 10, rect.Y, rect.Width - 14, rect.Height), Theme.Sans(9.5f), Theme.Ink, HorizontalAlignment.Left);
            }
            if (_items.Count == 0)
                Draw.CenteredText(g, "No matches", new Rectangle(0, 12, Width, 28), Theme.Sans(9f), Theme.Ink3);
        }
    }

    /// <summary>Custom-drawn checkbox list for multi-select.</summary>
    public class ClaudeCheckList : Control
    {
        private readonly List<string> _items = new List<string>();
        private readonly List<bool> _checked = new List<bool>();
        private int _hover = -1;

        public string[] CheckedValues
        {
            get
            {
                var result = new List<string>();
                for (int i = 0; i < _items.Count; i++) if (_checked[i]) result.Add(_items[i]);
                return result.ToArray();
            }
        }

        public ClaudeCheckList()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Surface;
            Cursor = Cursors.Hand;
        }

        public void SetItems(IEnumerable<string> items)
        {
            _items.Clear(); _checked.Clear();
            if (items != null) foreach (var i in items) { _items.Add(i); _checked.Add(false); }
            Invalidate();
        }

        public void SetChecked(string value, bool on)
        {
            int index = _items.IndexOf(value);
            if (index >= 0) { _checked[index] = on; Invalidate(); }
        }

        public bool IsChecked(string value)
        {
            int index = _items.IndexOf(value);
            return index >= 0 && _checked[index];
        }

        public void CheckAll(bool on)
        {
            for (int i = 0; i < _checked.Count; i++) _checked[i] = on;
            Invalidate();
        }

        private int RowAt(int y)
        {
            int row = (y - 4) / 34;
            return row >= 0 && row < _items.Count ? row : -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int row = RowAt(e.Y);
            if (row != _hover) { _hover = row; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = -1;
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            int row = RowAt(PointToClient(MousePosition).Y);
            if (row < 0) return;
            _checked[row] = !_checked[row];
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(Theme.Surface);
            for (int i = 0; i < _items.Count; i++)
            {
                var rect = new Rectangle(4, 4 + i * 34, Width - 8, 32);
                if (i == _hover) Draw.RoundedBox(g, rect, 7, Theme.BgDeep, Color.Transparent, 0);
                var box = new Rectangle(rect.X + 10, rect.Y + 8, 16, 16);
                Draw.RoundedBox(g, box, 4, _checked[i] ? Theme.Brand : Theme.Surface, _checked[i] ? Theme.Brand : Theme.LineStrong, 1);
                if (_checked[i]) Draw.Check(g, new Rectangle(box.X + 3, box.Y + 3, 10, 10), Color.White, 2f);
                Draw.CenteredText(g, _items[i], new Rectangle(rect.X + 36, rect.Y, rect.Width - 42, rect.Height), Theme.Sans(9.5f), Theme.Ink, HorizontalAlignment.Left);
            }
        }
    }

    /// <summary>Custom-drawn number stepper replacing NumericUpDown chrome.</summary>
    public class ClaudeNumberBox : UserControl
    {
        private readonly Label _value = new Label();
        private readonly Label _up = new Label();
        private readonly Label _down = new Label();
        private int _min, _max, _current;
        private bool _focused;

        public int Value
        {
            get { return _current; }
            set { _current = Math.Max(_min, Math.Min(_max, value)); _value.Text = _current.ToString("00"); }
        }

        public ClaudeNumberBox(int min, int max, int initial)
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            _min = min; _max = max; _current = initial;
            BackColor = Theme.Surface;
            Size = new Size(58, 38);

            _value.Font = Theme.Sans(10f);
            _value.ForeColor = Theme.Ink;
            _value.Location = new Point(8, 9);
            _value.AutoSize = true;
            _up.Text = "▲"; _down.Text = "▼";
            _up.Font = Theme.Sans(6f); _down.Font = Theme.Sans(6f);
            _up.ForeColor = Theme.Ink3; _down.ForeColor = Theme.Ink3;
            _up.Location = new Point(38, 5); _down.Location = new Point(38, 20);
            _up.Size = new Size(14, 12); _down.Size = new Size(14, 12);
            _up.Click += (s, e) => Value = _current + 1;
            _down.Click += (s, e) => Value = _current - 1;

            Controls.Add(_down);
            Controls.Add(_up);
            Controls.Add(_value);
            Click += (s, e) => { _focused = true; Invalidate(); };
            Value = initial;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw.RoundedBox(e.Graphics, new Rectangle(1, 1, Width - 2, Height - 2), 8, Theme.Surface, _focused ? Theme.Brand : Theme.LineStrong, 1);
        }
    }

    /// <summary>Custom-drawn select with a popup list and no native combo chrome.</summary>
    public class ClaudeSelectBox : UserControl
    {
        private readonly Label _value = new Label();
        private readonly Label _chevron = new Label();
        private readonly ClaudeOptionList _list = new ClaudeOptionList();
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

            _list.Location = new Point(0, 40);
            _list.Width = 260;
            _list.Height = 140;
            _list.Visible = false;
            _list.Picked += (s, v) => Pick(v);

            Controls.Add(_list);
            Controls.Add(_chevron);
            Controls.Add(_value);
            Action toggle = () =>
            {
                _focused = true;
                _list.Visible = !_list.Visible;
                Invalidate();
            };
            Click += (s, e) => toggle();
            _value.Click += (s, e) => toggle();
            _chevron.Click += (s, e) => toggle();
        }

        public void SetOptions(IEnumerable<string> options)
        {
            _options.Clear();
            _options.AddRange(options);
            _list.SetItems(_options);
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
