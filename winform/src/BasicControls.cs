using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UIComponents
{
    public enum ButtonVariant { Primary, Dark, Secondary, Ghost, Danger }

    public class ClaudeButton : Control
    {
        private bool _hover;
        private bool _pressed;
        private float _spin;

        public ButtonVariant Variant { get; set; }
        public bool Loading { get; set; }
        public int Radius { get; set; }

        public ClaudeButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            Variant = ButtonVariant.Primary;
            Radius = 8;
            Font = Theme.Sans(10f, FontStyle.Bold);
            Height = 38;
            MinimumSize = new Size(70, 30);
            Padding = new Padding(12, 0, 12, 0);
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent == null ? Theme.Bg : Parent.BackColor);
            if (Width < 2 || Height < 2) return;

            Color fill, text, border;
            switch (Variant)
            {
                case ButtonVariant.Dark:
                    fill = _hover ? Color.FromArgb(0, 0, 0) : Theme.Ink;
                    text = Color.White; border = fill; break;
                case ButtonVariant.Secondary:
                    fill = _hover ? Theme.Surface2 : Theme.Surface;
                    text = Theme.Ink; border = _hover ? Theme.Ink3 : Theme.LineStrong; break;
                case ButtonVariant.Ghost:
                    fill = _hover ? Theme.BgDeep : Color.Transparent;
                    text = Theme.Ink2; border = Color.Transparent; break;
                case ButtonVariant.Danger:
                    fill = _hover ? Color.FromArgb(152, 47, 39) : Theme.Danger;
                    text = Color.White; border = fill; break;
                default:
                    fill = _hover ? Theme.BrandHover : Theme.Brand;
                    text = Color.White; border = fill; break;
            }
            if (!Enabled) { fill = Theme.Blend(fill, Theme.Surface, .55); text = Theme.Blend(text, Theme.Surface, .55); border = Theme.Blend(border, Theme.Surface, .55); }
            if (_pressed && Enabled) fill = Theme.Blend(fill, Color.Black, .12);

            var rect = new Rectangle(1, 1, Width - 2, Height - 2);
            if (Variant != ButtonVariant.Ghost)
                Draw.RoundedBox(g, rect, Radius, fill, border, Variant == ButtonVariant.Secondary ? 1 : 0);

            if (Loading)
            {
                var r = new Rectangle(rect.X + 10, rect.Y + rect.Height / 2 - 7, 14, 14);
                using (var pen = new Pen(Theme.Blend(text, fill, .55), 2))
                {
                    pen.DashStyle = DashStyle.Solid;
                    g.DrawArc(pen, r, _spin, 280);
                }
                Draw.CenteredText(g, Text, new Rectangle(r.Right + 8, rect.Y, rect.Width - r.Right - 16, rect.Height), Font, text, HorizontalAlignment.Left);
            }
            else
            {
                Draw.CenteredText(g, Text, rect, Font, text);
            }
        }

        public void Tick()
        {
            if (!Loading) return;
            _spin = (_spin + 12) % 360;
            Invalidate();
        }
    }

    public enum BadgeTone { Neutral, Brand, Success, Warning, Danger }

    public class ClaudeBadge : Control
    {
        private bool _dot = true;
        public BadgeTone Tone { get; set; }
        public bool ShowDot { get { return _dot; } set { _dot = value; Invalidate(); } }

        public ClaudeBadge()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            Tone = BadgeTone.Neutral;
            Font = Theme.Sans(8.5f, FontStyle.Bold);
            Height = 24;
            AutoSize = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            Color bg, fg;
            switch (Tone)
            {
                case BadgeTone.Brand: bg = Theme.BrandSoft; fg = Theme.Brand; break;
                case BadgeTone.Success: bg = Theme.SuccessSoft; fg = Theme.Success; break;
                case BadgeTone.Warning: bg = Theme.WarningSoft; fg = Theme.Warning; break;
                case BadgeTone.Danger: bg = Theme.DangerSoft; fg = Theme.Danger; break;
                default: bg = Theme.BgDeep; fg = Theme.Ink2; break;
            }
            Draw.RoundedBox(g, new Rectangle(1, 1, Width - 2, Height - 2), 99, bg, Color.Transparent, 0);
            int x = 9;
            if (ShowDot) { Draw.Dot(g, new Point(x + 3, Height / 2), 6, fg); x += 12; }
            Draw.CenteredText(g, Text, new Rectangle(x, 0, Width - x - 8, Height), Font, fg, HorizontalAlignment.Left);
        }
    }

    public class ClaudeCard : Panel
    {
        public ClaudeCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Surface;
            Padding = new Padding(20);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw.RoundedBox(e.Graphics, new Rectangle(1, 1, Width - 2, Height - 2), 12, Theme.Surface, Theme.Line, 1);
        }
    }

    public class ClaudeDivider : Control
    {
        public ClaudeDivider() { Height = 1; BackColor = Theme.Line; }
    }

    public enum AlertTone { Info, Success, Warning, Danger }

    public class ClaudeAlert : Control
    {
        private readonly Label _title;
        private readonly Label _text;

        public AlertTone Tone
        {
            get { return _tone; }
            set { _tone = value; Apply(); }
        }
        private AlertTone _tone = AlertTone.Info;

        public string Title { get { return _title.Text; } set { _title.Text = value; } }
        public string Message { get { return _text.Text; } set { _text.Text = value; } }

        public ClaudeAlert()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 68;
            _title = new Label { Left = 54, Top = 12, Height = 20, Font = Theme.Sans(10f, FontStyle.Bold), AutoSize = true };
            _text = new Label { Left = 54, Top = 32, Height = 26, Font = Theme.Sans(9f), AutoSize = true, ForeColor = Theme.Ink2 };
            Controls.Add(_title);
            Controls.Add(_text);
            Apply();
        }

        private void Apply()
        {
            Color bg, fg;
            switch (_tone)
            {
                case AlertTone.Success: bg = Theme.SuccessSoft; fg = Theme.Success; break;
                case AlertTone.Warning: bg = Theme.WarningSoft; fg = Theme.Warning; break;
                case AlertTone.Danger: bg = Theme.DangerSoft; fg = Theme.Danger; break;
                default: bg = Theme.InfoSoft; fg = Theme.Info; break;
            }
            BackColor = bg;
            _title.ForeColor = fg;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw.RoundedBox(e.Graphics, new Rectangle(1, 1, Width - 2, Height - 2), 12, BackColor, Color.Transparent, 0);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using (var brush = new SolidBrush(_title.ForeColor))
                e.Graphics.DrawString(_tone == AlertTone.Danger ? "×" : _tone == AlertTone.Success ? "✓" : "i",
                    Theme.Sans(14f, FontStyle.Bold), brush, new Rectangle(14, 14, 26, 26), sf);
        }
    }

    public class ClaudeSwitch : Control
    {
        public bool Checked { get; set; }
        public string Caption { get; set; }

        public ClaudeSwitch()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 26;
            Width = 180;
            Cursor = Cursors.Hand;
        }

        protected override void OnClick(EventArgs e) { Checked = !Checked; Invalidate(); base.OnClick(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            var track = new Rectangle(2, 3, 40, 20);
            Draw.RoundedBox(g, track, 99, Checked ? Theme.Brand : Theme.LineStrong, Color.Transparent, 0);
            int knobX = Checked ? track.Right - 18 : track.Left + 2;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(Color.White))
                g.FillEllipse(brush, knobX, track.Y + 2, 16, 16);
            if (!string.IsNullOrEmpty(Caption))
                Draw.CenteredText(g, Caption, new Rectangle(50, 0, Width - 50, Height), Theme.Sans(10f), Theme.Ink, HorizontalAlignment.Left);
        }
    }

    public class ClaudeProgress : Control
    {
        private int _value;
        public int Value
        {
            get { return _value; }
            set { _value = Math.Max(0, Math.Min(100, value)); Invalidate(); }
        }

        public ClaudeProgress()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 10;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            Draw.RoundedBox(g, new Rectangle(1, 1, Width - 2, Height - 2), 99, Theme.BgDeep, Color.Transparent, 0);
            int w = (int)((Width - 2) * (_value / 100.0));
            if (w > 2) Draw.RoundedBox(g, new Rectangle(1, 1, w, Height - 2), 99, Theme.Brand, Color.Transparent, 0);
        }
    }

    public class ClaudeSkeleton : Control
    {
        private readonly Timer _timer = new Timer { Interval = 40 };
        private float _offset;
        public int Radius { get; set; }

        public ClaudeSkeleton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            Radius = 6;
            _timer.Tick += (s, e) => { _offset += 0.03f; if (_offset > 1.4f) _offset = 0; Invalidate(); };
            _timer.Start();
            Disposed += (s, e) => _timer.Stop();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var rect = new Rectangle(1, 1, Width - 2, Height - 2);
            Draw.RoundedBox(e.Graphics, rect, Radius, Theme.BgDeep, Color.Transparent, 0);
            int shimmerX = (int)(rect.Width * (_offset - 0.2f));
            using (var brush = new LinearGradientBrush(
                new Rectangle(shimmerX, rect.Y, Math.Max(4, rect.Width / 2), rect.Height),
                Color.Transparent, Color.FromArgb(70, 255, 255, 255), LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
        }
    }

    public class ClaudeAvatar : Control
    {
        public string Initials { get; set; }
        public Color Tone { get; set; }
        public Color Fore { get; set; }

        public ClaudeAvatar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            Initials = "AL";
            Tone = Theme.BrandSoft;
            Fore = Theme.Brand;
            Size = new Size(36, 36);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var rect = new Rectangle(2, 2, Width - 4, Height - 4);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(Tone)) e.Graphics.FillEllipse(brush, rect);
            using (var pen = new Pen(Theme.Blend(Tone, Theme.Ink, .12f))) e.Graphics.DrawEllipse(pen, rect);
            Draw.CenteredText(e.Graphics, Initials, rect, Theme.Sans(9f, FontStyle.Bold), Fore);
        }
    }
}
