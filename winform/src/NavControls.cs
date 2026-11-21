using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace UIComponents
{
    public class ClaudeTabs : Control
    {
        private readonly List<string> _items = new List<string>();
        private int _selected;

        public int SelectedIndex { get { return _selected; } set { _selected = Math.Max(0, Math.Min(_items.Count - 1, value)); Invalidate(); } }
        public event EventHandler SelectedIndexChanged;

        public ClaudeTabs()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 40;
            Cursor = Cursors.Hand;
        }

        public void Add(string text) { _items.Add(text); Invalidate(); }

        protected override void OnClick(EventArgs e)
        {
            var pt = PointToClient(MousePosition);
            int x = 10;
            for (int i = 0; i < _items.Count; i++)
            {
                int w = TextRenderer.MeasureText(_items[i], Theme.Sans(10f, FontStyle.Bold)).Width + 22;
                if (pt.X >= x && pt.X <= x + w) { SelectedIndex = i; if (SelectedIndexChanged != null) SelectedIndexChanged(this, EventArgs.Empty); break; }
                x += w + 6;
            }
            base.OnClick(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(Parent == null ? Theme.Bg : Parent.BackColor);
            g.DrawLine(new Pen(Theme.Line), 0, Height - 1, Width, Height - 1);
            int x = 10;
            for (int i = 0; i < _items.Count; i++)
            {
                int w = TextRenderer.MeasureText(_items[i], Theme.Sans(10f, FontStyle.Bold)).Width + 22;
                bool active = i == _selected;
                Draw.CenteredText(g, _items[i], new Rectangle(x, 0, w, Height - 2), Theme.Sans(10f, FontStyle.Bold), active ? Theme.Brand : Theme.Ink3);
                if (active) g.FillRectangle(new SolidBrush(Theme.Brand), x, Height - 3, w, 3);
                x += w + 6;
            }
        }
    }

    public class ClaudeAccordion : UserControl
    {
        private class Entry { public string Title; public string Body; public bool Open; }
        private readonly List<Entry> _entries = new List<Entry>();

        public void Add(string title, string body, bool open = false)
        {
            _entries.Add(new Entry { Title = title, Body = body, Open = open });
            Render();
        }

        private void Render()
        {
            Controls.Clear();
            int y = 0;
            foreach (var entry in _entries)
            {
                var head = new ClaudeButton
                {
                    Text = entry.Title,
                    Variant = ButtonVariant.Ghost,
                    Dock = DockStyle.None,
                    Location = new Point(0, y),
                    Size = new Size(Math.Max(100, Width), 42)
                };
                var captured = entry;
                head.Click += (s, e) => { captured.Open = !captured.Open; Render(); };
                Controls.Add(head);
                y += 42;
                if (entry.Open)
                {
                    var body = new Label
                    {
                        Text = entry.Body,
                        Location = new Point(14, y + 4),
                        Size = new Size(Math.Max(80, Width - 28), 34),
                        Font = Theme.Sans(9.5f),
                        ForeColor = Theme.Ink2
                    };
                    Controls.Add(body);
                    y += 42;
                }
            }
            Height = Math.Max(42, y);
        }

        protected override void OnResize(EventArgs e) { base.OnResize(e); Render(); }
    }

    public class ClaudeStepper : Control
    {
        private readonly List<string> _steps = new List<string>();
        private int _current;
        public int Current { get { return _current; } set { _current = Math.Max(0, Math.Min(_steps.Count - 1, value)); Invalidate(); } }

        public ClaudeStepper()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 66;
        }

        public void Add(string title) { _steps.Add(title); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(Parent == null ? Theme.Bg : Parent.BackColor);
            for (int i = 0; i < _steps.Count; i++)
            {
                int cx = 18 + i * (Width - 36) / Math.Max(1, _steps.Count - 1);
                if (i < _steps.Count - 1)
                {
                    int nextX = 18 + (i + 1) * (Width - 36) / Math.Max(1, _steps.Count - 1);
                    g.DrawLine(new Pen(i < _current ? Theme.Brand : Theme.LineStrong, 2), cx + 16, 16, nextX - 16, 16);
                }
                bool done = i < _current, active = i == _current;
                var circle = new Rectangle(cx, 3, 28, 28);
                Draw.RoundedBox(g, circle, 99,
                    done ? Theme.Brand : active ? Theme.BrandSoft : Theme.Surface,
                    done ? Theme.Brand : active ? Theme.Brand : Theme.LineStrong, active || done ? 0 : 2);
                if (done) Draw.Check(g, new Rectangle(cx + 7, 11, 14, 12), Color.White);
                else Draw.CenteredText(g, (i + 1).ToString(), circle, Theme.Sans(9f, FontStyle.Bold), active ? Theme.Brand : Theme.Ink3);
                Draw.CenteredText(g, _steps[i], new Rectangle(cx - 30, 36, 88, 24), Theme.Sans(9f, FontStyle.Bold), active || done ? Theme.Ink : Theme.Ink3);
            }
        }
    }

    public class ClaudeBreadcrumbs : Control
    {
        private readonly List<string> _items = new List<string>();
        public ClaudeBreadcrumbs()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 28;
        }
        public void Add(string item) { _items.Add(item); Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(Parent == null ? Theme.Bg : Parent.BackColor);
            int x = 0;
            for (int i = 0; i < _items.Count; i++)
            {
                bool last = i == _items.Count - 1;
                var font = Theme.Sans(9.5f, last ? FontStyle.Bold : FontStyle.Regular);
                int w = TextRenderer.MeasureText(_items[i], font).Width;
                Draw.CenteredText(g, _items[i], new Rectangle(x, 0, w, Height), font, last ? Theme.Ink : Theme.Ink2, HorizontalAlignment.Left);
                x += w;
                if (!last)
                {
                    Draw.CenteredText(g, "›", new Rectangle(x, 0, 18, Height), Theme.Sans(11f), Theme.Ink3);
                    x += 16;
                }
            }
        }
    }

    public class ClaudeSidebar : UserControl
    {
        private readonly Dictionary<string, List<string>> _groups = new Dictionary<string, List<string>>();
        private readonly Panel _brand;
        private bool _collapsed;

        public event EventHandler<string> ItemSelected;
        public bool Collapsed { get { return _collapsed; } }

        public ClaudeSidebar()
        {
            BackColor = Theme.BgDeep;
            Width = 224;
            Dock = DockStyle.Left;

            _brand = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = Theme.BgDeep };
            var mark = new Label { Text = "✦", Font = Theme.Serif(20f), ForeColor = Theme.BrandBright, Location = new Point(18, 10), Size = new Size(40, 32) };
            var name = new Label { Text = "Claude UI", Font = Theme.SerifBold(13f), ForeColor = Theme.Ink, Location = new Point(52, 14), Size = new Size(140, 22) };
            var sub = new Label { Text = "Component library", Font = Theme.Sans(8f), ForeColor = Theme.Ink3, Location = new Point(52, 36), Size = new Size(140, 18) };
            var toggle = new ClaudeButton { Text = "‹", Variant = ButtonVariant.Ghost, Location = new Point(180, 16), Size = new Size(30, 30), Radius = 6 };
            toggle.Click += (s, e) => { _collapsed = !_collapsed; toggle.Text = _collapsed ? "›" : "‹"; Width = _collapsed ? 64 : 224; ApplyCollapse(); };
            _brand.Controls.Add(sub);
            _brand.Controls.Add(name);
            _brand.Controls.Add(mark);
            _brand.Controls.Add(toggle);
            Controls.Add(_brand);
        }

        public void AddGroup(string name, params string[] items)
        {
            _groups[name] = new List<string>(items);
            Render();
        }

        private void ApplyCollapse()
        {
            foreach (Control c in Controls)
            {
                if (c == _brand) continue;
                c.Visible = !_collapsed;
            }
        }

        private void Render()
        {
            for (int i = Controls.Count - 1; i >= 0; i--)
                if (Controls[i] != _brand) Controls.RemoveAt(i);

            int y = 84;
            foreach (var group in _groups)
            {
                var label = new Label { Text = group.Key, Font = Theme.Sans(8.5f, FontStyle.Bold), ForeColor = Theme.Ink3, Location = new Point(16, y), Size = new Size(Math.Max(40, Width - 32), 20), UseMnemonic = false };
                Controls.Add(label);
                y += 24;
                foreach (var item in group.Value)
                {
                    var btn = new ClaudeButton { Text = item, Variant = ButtonVariant.Ghost, Location = new Point(8, y), Size = new Size(Math.Max(48, Width - 16), 34), Radius = 7 };
                    var captured = item;
                    btn.Click += (s, e) => { if (ItemSelected != null) ItemSelected(this, captured); };
                    Controls.Add(btn);
                    y += 36;
                }
                y += 12;
            }
        }

        protected override void OnResize(EventArgs e) { base.OnResize(e); if (!_collapsed) Render(); }
    }

    public class ClaudeEmptyState : UserControl
    {
        public ClaudeEmptyState(string glyph, string title, string message, string actionLabel)
        {
            BackColor = Theme.Surface;
            var icon = new Label { Text = glyph, Font = Theme.Serif(26f), ForeColor = Theme.Brand, AutoSize = true, Location = new Point(16, 18) };
            var head = new Label { Text = title, Font = Theme.SerifBold(14f), ForeColor = Theme.Ink, AutoSize = true, Location = new Point(52, 24) };
            var text = new Label { Text = message, Font = Theme.Sans(9.5f), ForeColor = Theme.Ink2, Location = new Point(18, 60), Size = new Size(Math.Max(120, Width - 36), 40) };
            var action = new ClaudeButton { Text = actionLabel, Variant = ButtonVariant.Primary, Location = new Point(18, 106), Size = new Size(110, 34) };
            Controls.Add(action);
            Controls.Add(text);
            Controls.Add(head);
            Controls.Add(icon);
            Size = new Size(280, 156);
        }
    }

    /// <summary>Stacks transient toasts in the bottom-right of its host panel.</summary>
    public class ClaudeToastHost : UserControl
    {
        public ClaudeToastHost()
        {
            BackColor = Color.Transparent;
            Width = 330;
        }

        public void Show(string title, string message, AlertTone tone = AlertTone.Success, bool persistent = false)
        {
            var toast = new ClaudeAlert { Tone = tone, Title = title, Message = message, Width = 310, Height = 72 };
            toast.Location = new Point(10, Controls.Count * 78);
            Controls.Add(toast);
            toast.BringToFront();
            if (!persistent)
            {
                var timer = new Timer { Interval = 3600 };
                timer.Tick += (s, e) => { timer.Stop(); Controls.Remove(toast); toast.Dispose(); Reposition(); };
                timer.Start();
            }
        }

        private void Reposition()
        {
            for (int i = 0; i < Controls.Count; i++) Controls[i].Location = new Point(10, i * 78);
        }
    }
}
