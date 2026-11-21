using System;
using System.Drawing;
using System.Windows.Forms;

namespace UIComponents
{
    /// <summary>Custom-drawn calendar panel used by the date pickers. No native DateTimePicker chrome.</summary>
    public class ClaudeCalendar : Control
    {
        private static readonly string[] Months =
        {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        };
        private static readonly string[] Days = { "S", "M", "T", "W", "T", "F", "S" };

        private DatePickerMode _mode = DatePickerMode.Date;
        private DateTime _value = DateTime.Today;
        private int _viewYear = DateTime.Today.Year;
        private int _viewMonth = DateTime.Today.Month;
        private Rectangle _prev = Rectangle.Empty, _next = Rectangle.Empty;
        private readonly Rectangle[] _cells = new Rectangle[42];

        public DatePickerMode Mode { get { return _mode; } set { _mode = value; Invalidate(); } }
        public DateTime Value { get { return _value; } set { _value = value; _viewYear = value.Year; _viewMonth = value.Month; Invalidate(); } }
        public event EventHandler ValueChanged;

        public ClaudeCalendar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(280, 250);
            BackColor = Theme.Surface;
            Cursor = Cursors.Hand;
        }

        private void Raise()
        {
            if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(Theme.Surface);
            // header
            _prev = new Rectangle(8, 8, 26, 26);
            _next = new Rectangle(Width - 34, 8, 26, 26);
            Draw.Chevron(g, _prev, Theme.Ink3, 2f);
            Draw.Chevron(g, _next, Theme.Ink3, 2f);
            // rotate chevrons to point left/right
            string title;
            if (_mode == DatePickerMode.Date) title = Months[_viewMonth - 1] + " " + _viewYear;
            else if (_mode == DatePickerMode.MonthYear) title = _viewYear.ToString();
            else title = (_viewYear - 8) + " – " + (_viewYear + 11);
            Draw.CenteredText(g, title, new Rectangle(40, 6, Width - 80, 30), Theme.Sans(10f, FontStyle.Bold), Theme.Ink);

            if (_mode == DatePickerMode.Date) DrawDays(g);
            else if (_mode == DatePickerMode.MonthYear) DrawMonths(g);
            else DrawYears(g);
        }

        private void DrawDays(Graphics g)
        {
            int x0 = 8, y0 = 44, cellW = (Width - 16) / 7, cellH = 28;
            for (int i = 0; i < 7; i++)
                Draw.CenteredText(g, Days[i], new Rectangle(x0 + i * cellW, y0, cellW, 20), Theme.Sans(8f), Theme.Ink3);
            int firstWeekday = (int)new DateTime(_viewYear, _viewMonth, 1).DayOfWeek;
            int days = DateTime.DaysInMonth(_viewYear, _viewMonth);
            for (int i = 0; i < 42; i++) _cells[i] = Rectangle.Empty;
            int index = firstWeekday;
            for (int d = 1; d <= days; d++, index++)
            {
                int col = index % 7, row = index / 7;
                var rect = new Rectangle(x0 + col * cellW, y0 + 22 + row * cellH, cellW - 2, cellH - 2);
                if (index < _cells.Length) _cells[index] = rect;
                bool selected = _value.Year == _viewYear && _value.Month == _viewMonth && _value.Day == d;
                bool today = DateTime.Today.Year == _viewYear && DateTime.Today.Month == _viewMonth && DateTime.Today.Day == d;
                if (selected) Draw.RoundedBox(g, rect, 7, Theme.Brand, Color.Transparent, 0);
                else if (today) Draw.RoundedBox(g, rect, 7, Theme.Surface, Theme.LineStrong, 1);
                Draw.CenteredText(g, d.ToString(), rect, Theme.Sans(9f), selected ? Color.White : Theme.Ink);
            }
        }

        private void DrawMonths(Graphics g)
        {
            int cols = 3, cellW = (Width - 20) / cols, cellH = 42;
            for (int i = 0; i < 12; i++)
            {
                int col = i % cols, row = i / cols;
                var rect = new Rectangle(10 + col * cellW, 44 + row * cellH, cellW - 4, cellH - 4);
                if (i < _cells.Length) _cells[i] = rect;
                bool selected = _value.Year == _viewYear && _value.Month == i + 1;
                if (selected) Draw.RoundedBox(g, rect, 7, Theme.Brand, Color.Transparent, 0);
                Draw.CenteredText(g, Months[i].Substring(0, 3), rect, Theme.Sans(9f, FontStyle.Bold), selected ? Color.White : Theme.Ink);
            }
        }

        private void DrawYears(Graphics g)
        {
            int start = _viewYear - 8, cols = 4, cellW = (Width - 20) / cols, cellH = 30;
            for (int i = 0; i < 20; i++)
            {
                int col = i % cols, row = i / cols;
                var rect = new Rectangle(10 + col * cellW, 44 + row * cellH, cellW - 4, cellH - 4);
                if (i < _cells.Length) _cells[i] = rect;
                int year = start + i;
                bool selected = _value.Year == year;
                if (selected) Draw.RoundedBox(g, rect, 7, Theme.Brand, Color.Transparent, 0);
                Draw.CenteredText(g, year.ToString(), rect, Theme.Sans(9f), selected ? Color.White : Theme.Ink);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            var pt = PointToClient(MousePosition);
            if (_prev.Contains(pt)) { Navigate(-1); return; }
            if (_next.Contains(pt)) { Navigate(1); return; }
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] == Rectangle.Empty || !_cells[i].Contains(pt)) continue;
                if (_mode == DatePickerMode.Date)
                {
                    int first = (int)new DateTime(_viewYear, _viewMonth, 1).DayOfWeek;
                    int day = i - first + 1;
                    if (day >= 1 && day <= DateTime.DaysInMonth(_viewYear, _viewMonth))
                        _value = new DateTime(_viewYear, _viewMonth, day);
                }
                else if (_mode == DatePickerMode.MonthYear)
                    _value = new DateTime(_viewYear, Math.Max(1, Math.Min(12, i + 1)), 1);
                else
                    _value = new DateTime(_viewYear - 8 + i, 1, 1);
                Raise();
                Invalidate();
                return;
            }
        }

        private void Navigate(int step)
        {
            if (_mode == DatePickerMode.Date)
            {
                _viewMonth += step;
                if (_viewMonth < 1) { _viewMonth = 12; _viewYear--; }
                if (_viewMonth > 12) { _viewMonth = 1; _viewYear++; }
            }
            else _viewYear += step * (_mode == DatePickerMode.Year ? 12 : 1);
            Invalidate();
        }
    }
}
