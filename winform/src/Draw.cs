using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UIComponents
{
    /// <summary>Shared GDI+ primitives so every control uses the same shape language.</summary>
    public static class Draw
    {
        public static GraphicsPath Rounded(Rectangle rect, int radius)
        {
            int r = Math.Max(0, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2));
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
            path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
            path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void RoundedBox(Graphics g, Rectangle rect, int radius, Color fill, Color border, int borderWidth = 1)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var path = Rounded(rect, radius))
            {
                using (var brush = new SolidBrush(fill)) g.FillPath(brush, path);
                if (borderWidth > 0 && border.A > 0)
                {
                    using (var pen = new Pen(border, borderWidth)) g.DrawPath(pen, path);
                }
            }
        }

        public static void CenteredText(Graphics g, string text, Rectangle rect, Font font, Color color,
            HorizontalAlignment align = HorizontalAlignment.Center)
        {
            if (string.IsNullOrEmpty(text)) return;
            var stringAlign = align == HorizontalAlignment.Center ? StringAlignment.Center
                : align == HorizontalAlignment.Right ? StringAlignment.Far : StringAlignment.Near;
            var sf = new StringFormat { Alignment = stringAlign, LineAlignment = StringAlignment.Center };
            using (var brush = new SolidBrush(color))
                g.DrawString(text, font, brush, rect, sf);
        }

        public static void Dot(Graphics g, Point center, int diameter, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(center.X - diameter / 2, center.Y - diameter / 2, diameter, diameter);
            using (var brush = new SolidBrush(color)) g.FillEllipse(brush, rect);
        }

        public static void Chevron(Graphics g, Rectangle rect, Color color, float width = 1.8f)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(color, width))
            {
                int cx = rect.X + rect.Width / 2;
                int cy = rect.Y + rect.Height / 2;
                int half = Math.Min(rect.Width, rect.Height) / 5;
                g.DrawLine(pen, cx - half, cy - half / 2, cx, cy + half / 2);
                g.DrawLine(pen, cx, cy + half / 2, cx + half, cy - half / 2);
            }
        }

        public static void Check(Graphics g, Rectangle rect, Color color, float width = 2f)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(color, width))
            {
                int x = rect.X + rect.Width / 4;
                int y = rect.Y + rect.Height / 2;
                g.DrawLine(pen, x, y, x + rect.Width / 5, y + rect.Height / 5);
                g.DrawLine(pen, x + rect.Width / 5, y + rect.Height / 5, x + rect.Width * 3 / 4, y - rect.Height / 4);
            }
        }

        public static void GridHeader(Graphics g, Rectangle bounds, string value, Font font)
        {
            using (var brush = new SolidBrush(Theme.Surface2)) g.FillRectangle(brush, bounds);
            g.DrawLine(new Pen(Theme.Line), bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
            g.DrawLine(new Pen(Theme.Line), bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom);
            CenteredText(g, value ?? "", new Rectangle(bounds.X + 10, bounds.Y, bounds.Width - 14, bounds.Height), font, Theme.Ink2, System.Windows.Forms.HorizontalAlignment.Left);
        }

        public static void GridCell(Graphics g, Rectangle bounds, string value, bool selected, Font font, Color textColor)
        {
            Color background = selected ? Theme.BrandSoft : Theme.Surface;
            using (var brush = new SolidBrush(background)) g.FillRectangle(brush, bounds);
            g.DrawLine(new Pen(Theme.Line), bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
            g.DrawLine(new Pen(Theme.Line), bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom);
            CenteredText(g, value ?? "", new Rectangle(bounds.X + 10, bounds.Y, bounds.Width - 14, bounds.Height), font, textColor, System.Windows.Forms.HorizontalAlignment.Left);
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Theme.Surface;
            grid.GridColor = Theme.Line;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.EnableHeadersVisualStyles = false;
            grid.Font = Theme.Sans(9.5f);
            grid.ForeColor = Theme.Ink;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            grid.ColumnHeadersHeight = 34;
            grid.RowTemplate.Height = 38;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.Surface2;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Ink2;
            grid.ColumnHeadersDefaultCellStyle.Font = Theme.Sans(9f, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.DefaultCellStyle.BackColor = Theme.Surface;
            grid.DefaultCellStyle.SelectionBackColor = Theme.BrandSoft;
            grid.DefaultCellStyle.SelectionForeColor = Theme.Ink;
            grid.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);
        }
    }
}
