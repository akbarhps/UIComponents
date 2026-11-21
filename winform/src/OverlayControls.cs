using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace UIComponents
{
    public class CommandEntry
    {
        public string Group;
        public string Label;
        public Action Action;
        public CommandEntry(string group, string label, Action action) { Group = group; Label = label; Action = action; }
    }

    /// <summary>Ctrl+K style search-driven action launcher.</summary>
    public class ClaudeCommandPalette : Form
    {
        private readonly TextBox _search = new TextBox();
        private readonly ListBox _list = new ListBox();
        private readonly List<CommandEntry> _commands = new List<CommandEntry>();
        private readonly List<CommandEntry> _filtered = new List<CommandEntry>();

        public ClaudeCommandPalette()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(520, 420);
            BackColor = Theme.Surface;
            Padding = new Padding(1);
            KeyPreview = true;

            var header = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Theme.Surface };
            header.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Theme.Line), 0, 57, header.Width, 57);
            var icon = new Label { Text = "⌕", Font = Theme.Sans(18f), ForeColor = Theme.Ink3, Location = new Point(18, 14), Size = new Size(26, 30) };
            _search.Font = Theme.Sans(12f);
            _search.BorderStyle = BorderStyle.None;
            _search.Location = new Point(50, 16);
            _search.Width = 400;
            _search.TextChanged += (s, e) => Filter();
            _search.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Down && _list.Items.Count > 0) { if (_list.SelectedIndex < _list.Items.Count - 1) _list.SelectedIndex++; e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Up && _list.Items.Count > 0) { if (_list.SelectedIndex > 0) _list.SelectedIndex--; e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Enter && _list.SelectedIndex >= 0) { Run(_filtered[_list.SelectedIndex]); e.SuppressKeyPress = true; }
                else if (e.KeyCode == Keys.Escape) Close();
            };
            header.Controls.Add(_search);
            header.Controls.Add(icon);

            _list.Font = Theme.Sans(10f);
            _list.BorderStyle = BorderStyle.None;
            _list.Dock = DockStyle.Fill;
            _list.Click += (s, e) => { if (_list.SelectedIndex >= 0) Run(_filtered[_list.SelectedIndex]); };

            var footer = new Label { Text = "↑↓ to navigate    ↵ to select    esc to close", Dock = DockStyle.Bottom, Height = 32, Font = Theme.Sans(8.5f), ForeColor = Theme.Ink3, TextAlign = ContentAlignment.MiddleCenter };
            footer.Paint += (s, e) => e.Graphics.DrawLine(new Pen(Theme.Line), 0, 0, footer.Width, 0);

            Controls.Add(_list);
            Controls.Add(header);
            Controls.Add(footer);
            Shown += (s, e) => { Filter(); _search.Focus(); };
        }

        public void Add(CommandEntry entry) { _commands.Add(entry); }

        private void Filter()
        {
            string q = _search.Text.Trim().ToLowerInvariant();
            _filtered.Clear();
            _list.Items.Clear();
            foreach (var c in _commands)
                if (c.Label.ToLowerInvariant().Contains(q)) { _filtered.Add(c); _list.Items.Add(c.Group + "  ·  " + c.Label); }
            if (_filtered.Count > 0) _list.SelectedIndex = 0;
        }

        private void Run(CommandEntry entry)
        {
            Close();
            if (entry.Action != null) entry.Action();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw.RoundedBox(e.Graphics, new Rectangle(1, 1, Width - 2, Height - 2), 16, Theme.Surface, Theme.Line);
        }
    }

    /// <summary>Blocking modal for work that must finish before the user continues.</summary>
    public class ClaudeLoadingDialog : Form
    {
        private readonly Timer _timer = new Timer { Interval = 40 };
        private float _angle;
        public string Message { get; set; }

        public ClaudeLoadingDialog(string message)
        {
            Message = message;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(380, 220);
            BackColor = Theme.Surface;
            _timer.Tick += (s, e) => { _angle = (_angle + 12) % 360; Invalidate(); };
        }

        protected override void OnShown(EventArgs e) { base.OnShown(e); _timer.Start(); }
        protected override void OnFormClosed(FormClosedEventArgs e) { _timer.Stop(); base.OnFormClosed(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw.RoundedBox(e.Graphics, new Rectangle(1, 1, Width - 2, Height - 2), 16, Theme.Surface, Theme.Line);
            var rect = new Rectangle(Width / 2 - 18, 36, 36, 36);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(Theme.Brand, 3)) e.Graphics.DrawArc(pen, rect, _angle, 280);
            Draw.CenteredText(e.Graphics, "Preparing your workspace", new Rectangle(20, 86, Width - 40, 32), Theme.SerifBold(14f), Theme.Ink);
            Draw.CenteredText(e.Graphics, Message, new Rectangle(30, 120, Width - 60, 32), Theme.Sans(9.5f), Theme.Ink2);
            Draw.RoundedBox(e.Graphics, new Rectangle(30, 164, Width - 60, 5), 99, Theme.BgDeep, Color.Transparent, 0);
            int w = (int)((Width - 60) * 0.35);
            Draw.RoundedBox(e.Graphics, new Rectangle(30, 164, w, 5), 99, Theme.Brand, Color.Transparent, 0);
        }
    }

    /// <summary>Side-panel edit form with Delete / Cancel / Save and an inline delete confirmation.</summary>
    public class ClaudeDrawerForm : Form
    {
        private readonly Panel _confirm = new Panel { Visible = false };
        private readonly ClaudeField _name = new ClaudeField { LabelText = "Full name", TextValue = "Jamie Rivera" };
        private readonly ClaudeField _email = new ClaudeField { LabelText = "Email address", TextValue = "jamie@example.com" };
        private readonly ClaudeField _role = new ClaudeField { LabelText = "Role", IsSelect = true, Options = new[] { "Member", "Admin", "Viewer" } };
        private readonly ClaudeSwitch _active = new ClaudeSwitch { Caption = "Active in workspace", Checked = true };

        public ClaudeDrawerForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            BackColor = Theme.Surface;
            Size = new Size(420, 560);

            var title = new Label { Text = "Edit member", Font = Theme.SerifBold(16f), ForeColor = Theme.Ink, Location = new Point(26, 22), AutoSize = true };
            var close = new ClaudeButton { Text = "×", Variant = ButtonVariant.Ghost, Location = new Point(370, 18), Size = new Size(28, 28) };
            close.Click += (s, e) => Close();
            _name.Location = new Point(26, 70);
            _name.Width = 368;
            _email.Location = new Point(26, 156);
            _email.Width = 368;
            _role.Location = new Point(26, 242);
            _role.Width = 368;
            _active.Location = new Point(26, 340);

            var delete = new ClaudeButton { Text = "Delete", Variant = ButtonVariant.Danger, Location = new Point(26, 496), Size = new Size(80, 38) };
            delete.Click += (s, e) => { _confirm.Visible = true; _confirm.BringToFront(); };
            var cancel = new ClaudeButton { Text = "Cancel", Variant = ButtonVariant.Secondary, Location = new Point(220, 496), Size = new Size(82, 38) };
            cancel.Click += (s, e) => Close();
            var save = new ClaudeButton { Text = "Save changes", Location = new Point(312, 496), Size = new Size(96, 38) };
            save.Click += (s, e) => { _email.HasError = !_email.TextValue.Contains("@"); if (!_email.HasError) Close(); };

            _confirm.Dock = DockStyle.Fill;
            _confirm.BackColor = Theme.Surface;
            var confirmTitle = new Label { Text = "Delete this member?", Font = Theme.SerifBold(16f), ForeColor = Theme.Danger, Location = new Point(30, 170), AutoSize = true };
            var confirmText = new Label { Text = "Jamie will lose access immediately. This can't be undone.", Font = Theme.Sans(9.5f), ForeColor = Theme.Ink2, Location = new Point(30, 210), Size = new Size(340, 50) };
            var keep = new ClaudeButton { Text = "Keep member", Variant = ButtonVariant.Secondary, Location = new Point(100, 280), Size = new Size(100, 38) };
            keep.Click += (s, e) => { _confirm.Visible = false; };
            var confirmDelete = new ClaudeButton { Text = "Delete member", Variant = ButtonVariant.Danger, Location = new Point(210, 280), Size = new Size(110, 38) };
            confirmDelete.Click += (s, e) => Close();
            _confirm.Controls.Add(confirmDelete);
            _confirm.Controls.Add(keep);
            _confirm.Controls.Add(confirmText);
            _confirm.Controls.Add(confirmTitle);

            Controls.Add(_confirm);
            Controls.Add(save);
            Controls.Add(cancel);
            Controls.Add(delete);
            Controls.Add(_active);
            Controls.Add(_role);
            Controls.Add(_email);
            Controls.Add(_name);
            Controls.Add(close);
            Controls.Add(title);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawLine(new Pen(Theme.Line), 0, 0, 1, Height);
        }
    }
}
