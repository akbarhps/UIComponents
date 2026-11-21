using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace UIComponents
{
    public class ClaudeModalForm : Form
    {
        public ClaudeModalForm()
        {
            Text = "Create project";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(440, 330);
            BackColor = Theme.Bg;
            Padding = new Padding(1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw.RoundedBox(e.Graphics, new Rectangle(1, 1, Width - 2, Height - 2), 16, Theme.Surface, Theme.Line, 1);
        }

        public void Build()
        {
            var title = new Label { Text = "Create a project", Font = Theme.SerifBold(16f), ForeColor = Theme.Ink, Location = new Point(26, 22), AutoSize = true };
            var name = new ClaudeField { LabelText = "Project name", Hint = "As it should appear in your account.", Location = new Point(26, 62), Width = 386 };
            var owner = new ClaudeField { LabelText = "Owner email", Location = new Point(26, 148), Width = 386 };
            var cancel = new ClaudeButton { Text = "Cancel", Variant = ButtonVariant.Secondary, Location = new Point(230, 274), Size = new Size(82, 38) };
            cancel.Click += (s, e) => Close();
            var submit = new ClaudeButton { Text = "Create", Variant = ButtonVariant.Primary, Location = new Point(322, 274), Size = new Size(90, 38) };
            submit.Click += (s, e) =>
            {
                name.HasError = string.IsNullOrWhiteSpace(name.TextValue);
                owner.HasError = !owner.TextValue.Contains("@");
                if (!name.HasError && !owner.HasError) Close();
            };
            Controls.AddRange(new Control[] { submit, cancel, owner, name, title });
        }
    }

    public class DemoForm : Form
    {
        private readonly Panel _main = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Bg, AutoScroll = true, Padding = new Padding(28, 24, 28, 60) };
        private readonly ClaudeToastHost _toasts = new ClaudeToastHost();
        private readonly ClaudeSidebar _sidebar = new ClaudeSidebar();
        private readonly System.Collections.Generic.Dictionary<string, int> _anchors = new System.Collections.Generic.Dictionary<string, int>();
        private ClaudeButton _loadingButton;
        private ClaudeProgress _progress;
        private Timer _progressTimer;
        private ClaudeStepper _stepper;

        public DemoForm()
        {
            Text = "Claude UI — WinForms components";
            Size = new Size(1180, 820);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.Bg;
            Font = Theme.Sans(10f);
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.K)
                {
                    e.SuppressKeyPress = true;
                    ShowCommandPalette();
                }
            };

            Controls.Add(_main);
            BuildContent();
            BuildSidebar();
            Controls.Add(_sidebar);

            _toasts.Location = new Point(1180 - 350, 820 - 200);
            Controls.Add(_toasts);
            Resize += (s, e) => _toasts.Location = new Point(Width - 350, Height - 210);
        }

        private Panel Section(string title, int y, int height, out int nextY)
        {
            var heading = new Label { Text = title, Font = Theme.Serif(18f), ForeColor = Theme.Ink, Location = new Point(0, y), AutoSize = true, UseMnemonic = false };
            _main.Controls.Add(heading);
            _anchors[title] = y;
            int cardY = y + 36;
            var card = new ClaudeCard { Location = new Point(0, cardY), Size = new Size(900, height + 44), Padding = new Padding(22) };
            var panel = new Panel { Location = new Point(22, 22), Size = new Size(856, height), BackColor = Theme.Surface };
            card.Controls.Add(panel);
            _main.Controls.Add(card);
            nextY = cardY + height + 44 + 28;
            return panel;
        }

        private void BuildSidebar()
        {
            _sidebar.AddGroup("Basics", "Buttons", "Badges & alerts", "Progress & skeleton", "Empty state");
            _sidebar.AddGroup("Forms", "Fields", "Tag input", "Autocomplete", "Date & file", "Selects", "Date & time", "Grid input", "Typed grid", "Modal form");
            _sidebar.AddGroup("Navigation", "Tabs & accordion", "Stepper & breadcrumbs", "Tooltips", "Command palette");
            _sidebar.AddGroup("Data", "Tables", "Filter table", "Column filters", "Selection table", "Overlays", "Toasts");
            _sidebar.ItemSelected += (s, item) =>
            {
                int y;
                if (_anchors.TryGetValue(item, out y)) _main.AutoScrollPosition = new Point(0, Math.Max(0, y - 20));
            };
        }

        private void BuildContent()
        {
            int y;

            // Buttons
            var buttons = Section("Buttons", 0, 120, out y);
            var primary = new ClaudeButton { Text = "Primary action", Location = new Point(0, 0), Size = new Size(130, 38) };
            var dark = new ClaudeButton { Text = "Dark", Variant = ButtonVariant.Dark, Location = new Point(142, 0), Size = new Size(80, 38) };
            var secondary = new ClaudeButton { Text = "Secondary", Variant = ButtonVariant.Secondary, Location = new Point(234, 0), Size = new Size(100, 38) };
            var ghost = new ClaudeButton { Text = "Ghost", Variant = ButtonVariant.Ghost, Location = new Point(346, 0), Size = new Size(80, 38) };
            var danger = new ClaudeButton { Text = "Danger", Variant = ButtonVariant.Danger, Location = new Point(438, 0), Size = new Size(90, 38) };
            _loadingButton = new ClaudeButton { Text = "Loading", Location = new Point(0, 54), Size = new Size(110, 38) };
            _loadingButton.Click += (s, e) => { _loadingButton.Loading = true; Timer t = new Timer { Interval = 1500 }; int ticks = 0; t.Tick += (a, b) => { _loadingButton.Tick(); if (++ticks > 10) { t.Stop(); _loadingButton.Loading = false; } }; t.Start(); };
            var modalBtn = new ClaudeButton { Text = "Open modal form", Variant = ButtonVariant.Secondary, Location = new Point(122, 54), Size = new Size(140, 38) };
            modalBtn.Click += (s, e) => { var m = new ClaudeModalForm(); m.Build(); m.ShowDialog(this); };
            buttons.Controls.AddRange(new Control[] { danger, ghost, secondary, dark, primary, _loadingButton, modalBtn });

            // Badges & alerts
            var feedback = Section("Badges & alerts", y, 150, out y);
            var b1 = new ClaudeBadge { Text = "Active", Tone = BadgeTone.Success, Location = new Point(0, 0), Size = new Size(80, 24) };
            var b2 = new ClaudeBadge { Text = "In review", Tone = BadgeTone.Brand, Location = new Point(90, 0), Size = new Size(90, 24) };
            var b3 = new ClaudeBadge { Text = "Pending", Tone = BadgeTone.Warning, Location = new Point(190, 0), Size = new Size(86, 24) };
            var b4 = new ClaudeBadge { Text = "Overdue", Tone = BadgeTone.Danger, Location = new Point(286, 0), Size = new Size(86, 24) };
            var alert = new ClaudeAlert { Tone = AlertTone.Info, Title = "Good to know", Message = "Your changes save automatically as you type.", Location = new Point(0, 40), Size = new Size(420, 68) };
            feedback.Controls.AddRange(new Control[] { alert, b4, b3, b2, b1 });

            // Progress & skeleton
            var loading = Section("Progress & skeleton", y, 120, out y);
            var plabel = new Label { Text = "Uploading files…", Font = Theme.Sans(9.5f), ForeColor = Theme.Ink2, Location = new Point(0, 0), AutoSize = true };
            _progress = new ClaudeProgress { Location = new Point(0, 22), Size = new Size(420, 10) };
            var start = new ClaudeButton { Text = "Start upload", Location = new Point(0, 52), Size = new Size(110, 34) };
            start.Click += (s, e) =>
            {
                int value = 0;
                _progressTimer = new Timer { Interval = 250 };
                _progressTimer.Tick += (a, b) =>
                {
                    value = Math.Min(100, value + 9);
                    _progress.Value = value;
                    plabel.Text = value >= 100 ? "Upload complete" : "Uploading files… " + value + "%";
                    if (value >= 100) { _progressTimer.Stop(); _toasts.Show("Upload complete", "All files were uploaded."); }
                };
                _progressTimer.Start();
            };
            var sk1 = new ClaudeSkeleton { Location = new Point(470, 0), Size = new Size(200, 14) };
            var sk2 = new ClaudeSkeleton { Location = new Point(470, 22), Size = new Size(150, 14) };
            var sk3 = new ClaudeSkeleton { Location = new Point(470, 44), Size = new Size(180, 14) };
            loading.Controls.AddRange(new Control[] { sk3, sk2, sk1, start, _progress, plabel });

            // Empty state
            var empty = Section("Empty state", y, 170, out y);
            var emptyCard = new ClaudeEmptyState("▢", "No projects yet", "Projects you create will appear here. Start with a blank project.", "New project");
            emptyCard.Location = new Point(0, 0);
            empty.Controls.Add(emptyCard);

            // Fields
            var fields = Section("Fields", y, 200, out y);
            var nameField = new ClaudeField { LabelText = "Full name", Hint = "As it should appear on your account.", Location = new Point(0, 0), Width = 400 };
            var emailField = new ClaudeField { LabelText = "Email address", Location = new Point(420, 0), Width = 400 };
            var selectField = new ClaudeField { LabelText = "Role", IsSelect = true, Options = new[] { "Designer", "Engineer", "Product manager" }, Location = new Point(0, 100), Width = 400 };
            var areaField = new ClaudeField { LabelText = "Message", Multiline = true, Location = new Point(420, 100), Width = 400 };
            var check = new ClaudeSwitch { Caption = "Enable notifications", Checked = true, Location = new Point(0, 196), Width = 220 };
            fields.Controls.AddRange(new Control[] { check, areaField, selectField, emailField, nameField });
            fields.Height = 240;

            // Tag input & autocomplete
            var inputs = Section("Tag input & autocomplete", y, 150, out y);
            var tags = new ClaudeTagInput(new[] { "design", "research" }) { Location = new Point(0, 0), Width = 400, MaxTags = 6 };
            var auto = new ClaudeAutocomplete(new[] { "React", "Vue", "Angular", "Svelte", "Solid" }) { Location = new Point(420, 0), Width = 400 };
            inputs.Controls.Add(auto);
            inputs.Controls.Add(tags);

            // Date & file
            var dates = Section("Date & file", y, 170, out y);
            var date = new ClaudeDatePicker { Location = new Point(0, 0), Size = new Size(220, 38) };
            var month = new ClaudeDatePicker { Mode = DatePickerMode.MonthYear, Location = new Point(240, 0), Size = new Size(220, 38) };
            var year = new ClaudeDatePicker { Mode = DatePickerMode.Year, Location = new Point(480, 0), Size = new Size(140, 38) };
            var file = new ClaudeFileInput { Location = new Point(0, 50), Size = new Size(420, 120) };
            dates.Controls.AddRange(new Control[] { file, year, month, date });

            // Tabs & accordion
            var nav = Section("Tabs & accordion", y, 180, out y);
            var tabs = new ClaudeTabs { Location = new Point(0, 0), Size = new Size(380, 40) };
            tabs.Add("Profile"); tabs.Add("Security"); tabs.Add("Billing");
            var tabBody = new Label { Text = "Update your name, photo and public profile details.", Font = Theme.Sans(9.5f), ForeColor = Theme.Ink2, Location = new Point(0, 50), AutoSize = true };
            var accordion = new ClaudeAccordion { Location = new Point(440, 0), Width = 400 };
            accordion.Add("Project overview", "Track scope, timeline and ownership in one place.", true);
            accordion.Add("Team members", "Invite people and manage their access.");
            nav.Controls.AddRange(new Control[] { accordion, tabBody, tabs });

            // Stepper & breadcrumbs
            var steps = Section("Stepper & breadcrumbs", y, 130, out y);
            _stepper = new ClaudeStepper { Location = new Point(0, 0), Size = new Size(500, 66) };
            _stepper.Add("Account"); _stepper.Add("Workspace"); _stepper.Add("Review"); _stepper.Add("Done");
            var prev = new ClaudeButton { Text = "Previous", Variant = ButtonVariant.Secondary, Location = new Point(0, 82), Size = new Size(90, 34) };
            var next = new ClaudeButton { Text = "Next step", Location = new Point(100, 82), Size = new Size(100, 34) };
            prev.Click += (s, e) => { _stepper.Current = Math.Max(0, _stepper.Current - 1); };
            next.Click += (s, e) => { _stepper.Current = Math.Min(3, _stepper.Current + 1); };
            var crumbs = new ClaudeBreadcrumbs { Location = new Point(540, 20), Size = new Size(320, 28) };
            crumbs.Add("Workspace"); crumbs.Add("Projects"); crumbs.Add("Website refresh"); crumbs.Add("Settings");
            steps.Controls.AddRange(new Control[] { crumbs, next, prev, _stepper });

            // Tables
            var tables = Section("Tables", y, 220, out y);
            var grid = new DataGridView
            {
                Location = new Point(0, 0),
                Size = new Size(810, 210)
            };
            Draw.StyleGrid(grid);
            grid.Columns.Add("Project", "Project");
            grid.Columns.Add("Owner", "Owner");
            grid.Columns.Add("Status", "Status");
            grid.Columns.Add("Updated", "Updated");
            grid.Columns.Add("Budget", "Budget");
            grid.Columns[0].Width = 220;
            grid.Columns[1].Width = 160;
            grid.Columns[2].Width = 120;
            grid.Columns[3].Width = 120;
            grid.Columns[4].Width = 120;
            grid.Rows.Add("Website refresh", "Ada Lovelace", "Active", "Today", "$12,400");
            grid.Rows.Add("Mobile app", "Jonas Silva", "Active", "Yesterday", "$28,900");
            grid.Rows.Add("Brand guidelines", "Mia Chen", "Pending", "3 days ago", "$4,200");
            grid.Rows.Add("Help center", "Ada Lovelace", "Active", "Last week", "$8,750");
            grid.Rows.Add("Legacy archive", "Sam Patel", "Draft", "2 weeks ago", "$1,100");
            tables.Controls.Add(grid);

            // Toasts
            var toasts = Section("Toasts", y, 90, out y);
            var okToast = new ClaudeButton { Text = "Success toast", Variant = ButtonVariant.Secondary, Location = new Point(0, 0), Size = new Size(120, 36) };
            okToast.Click += (s, e) => _toasts.Show("Saved", "Your changes have been saved.");
            var infoToast = new ClaudeButton { Text = "Info toast", Variant = ButtonVariant.Secondary, Location = new Point(130, 0), Size = new Size(110, 36) };
            infoToast.Click += (s, e) => _toasts.Show("Heads up", "A new version is available.", AlertTone.Info);
            var errToast = new ClaudeButton { Text = "Error toast", Variant = ButtonVariant.Secondary, Location = new Point(250, 0), Size = new Size(110, 36) };
            errToast.Click += (s, e) => _toasts.Show("Upload failed", "The file could not be uploaded.", AlertTone.Danger);
            toasts.Controls.AddRange(new Control[] { errToast, infoToast, okToast });

            // Selects
            var selects = Section("Selects", y, 300, out y);
            var searchSelect = new ClaudeSearchSelect(new[] { "Ada Lovelace", "Jonas Silva", "Mia Chen", "Sam Patel", "Alex Rivera" }) { Location = new Point(0, 0), Size = new Size(410, 110) };
            var multiSelect = new ClaudeMultiSelect(new[] { "React", "TypeScript", "Design systems", "Accessibility", "Testing" }, "Select skills…") { Location = new Point(430, 0), Size = new Size(400, 290) };
            var selectedLabel = new Label { Text = "Selected: —", Font = Theme.Sans(9f), ForeColor = Theme.Ink2, Location = new Point(0, 120), AutoSize = true };
            searchSelect.SelectedChanged += (s, e) => selectedLabel.Text = "Selected: " + searchSelect.SelectedValue;
            selects.Controls.Add(selectedLabel);
            selects.Controls.Add(multiSelect);
            selects.Controls.Add(searchSelect);

            // Date & time
            var dateTime = Section("Date & time", y, 100, out y);
            var dt1 = new ClaudeDateTimePicker { Location = new Point(0, 0), Size = new Size(360, 38) };
            var dt2 = new ClaudeDateTimePicker(true) { Location = new Point(0, 50), Size = new Size(420, 38) };
            var dtLabel = new Label { Text = "", Font = Theme.Sans(9f), ForeColor = Theme.Ink3, Location = new Point(440, 8), AutoSize = true };
            dateTime.Controls.Add(dtLabel);
            dateTime.Controls.Add(dt2);
            dateTime.Controls.Add(dt1);

            // Grid input
            var gridSection = Section("Grid input", y, 270, out y);
            gridSection.Controls.Add(new ClaudeGrid(
                new[] { new GridColumnDef("Width", GridColumnType.Text), new GridColumnDef("Height", GridColumnType.Text), new GridColumnDef("Depth", GridColumnType.Text) },
                new string[,] { { "Small", "10", "20" }, { "Medium", "14", "24" }, { "Large", "18", "30" } }
            ) { Dock = DockStyle.Fill });

            // Typed grid
            var typedSection = Section("Typed grid", y, 270, out y);
            typedSection.Controls.Add(new ClaudeGrid(
                new[]
                {
                    new GridColumnDef("Variant", GridColumnType.Text),
                    new GridColumnDef("Category", GridColumnType.Select, new[] { "Standard", "Pro", "Custom" }),
                    new GridColumnDef("IsActive", GridColumnType.Checkbox)
                },
                new string[,] { { "Small", "Standard", "true" }, { "Large", "Custom", "false" } },
                false
            ) { Dock = DockStyle.Fill });

            // Filter table
            var filterSection = Section("Filter table", y, 300, out y);
            filterSection.Controls.Add(new ClaudeTable(BuildTable(), new TableOptions { ShowFilter = true, ShowPagination = true, Sortable = true, PageSize = 10 }) { Dock = DockStyle.Fill });

            // Column filters
            var columnSection = Section("Column filters", y, 300, out y);
            columnSection.Controls.Add(new ClaudeTable(BuildTable(), new TableOptions { ShowColumnFilters = true, ShowPagination = true, Sortable = true }) { Dock = DockStyle.Fill });

            // Selection table
            var selectionSection = Section("Selection table", y, 300, out y);
            selectionSection.Controls.Add(new ClaudeTable(BuildTable(), new TableOptions { ShowSelection = true, ShowPagination = true, Sortable = true, StickyFirst = true }) { Dock = DockStyle.Fill });

            // Overlays
            var overlays = Section("Overlays", y, 100, out y);
            var paletteBtn = new ClaudeButton { Text = "Command palette (Ctrl+K)", Location = new Point(0, 0), Size = new Size(190, 38) };
            paletteBtn.Click += (s, e) => ShowCommandPalette();
            var loadingBtn = new ClaudeButton { Text = "Loading modal", Variant = ButtonVariant.Secondary, Location = new Point(200, 0), Size = new Size(130, 38) };
            loadingBtn.Click += (s, e) =>
            {
                var dialog = new ClaudeLoadingDialog("Please wait a moment while we set things up.");
                var timer = new Timer { Interval = 2200 };
                timer.Tick += (a, b) => { timer.Stop(); dialog.Close(); _toasts.Show("Ready", "Your workspace is ready."); };
                dialog.Shown += (a, b) => timer.Start();
                dialog.ShowDialog(this);
            };
            var drawerBtn = new ClaudeButton { Text = "Drawer form", Variant = ButtonVariant.Secondary, Location = new Point(340, 0), Size = new Size(120, 38) };
            drawerBtn.Click += (s, e) =>
            {
                var drawer = new ClaudeDrawerForm();
                drawer.Location = new Point(Right - drawer.Width - 20, Top + 60);
                drawer.ShowDialog(this);
            };
            var tip = new ToolTip { BackColor = Theme.Ink, ForeColor = Theme.Surface, IsBalloon = false };
            tip.SetToolTip(paletteBtn, "Search and run an action");
            tip.SetToolTip(loadingBtn, "Blocks the UI until work finishes");
            tip.SetToolTip(drawerBtn, "Edit a member with delete confirmation");
            overlays.Controls.AddRange(new Control[] { drawerBtn, loadingBtn, paletteBtn });
        }

        private void ShowCommandPalette()
        {
            var palette = new ClaudeCommandPalette();
            palette.Add(new CommandEntry("Actions", "Create a project", () => _toasts.Show("Create project", "A new project draft was started.")));
            palette.Add(new CommandEntry("Actions", "Invite a member", () => _toasts.Show("Invite member", "Opening the invite form.", AlertTone.Info)));
            palette.Add(new CommandEntry("Navigation", "Go to Buttons", () => _main.AutoScrollPosition = new Point(0, 0)));
            palette.Add(new CommandEntry("Navigation", "Go to Tables", () => { int v; if (_anchors.TryGetValue("Tables", out v)) _main.AutoScrollPosition = new Point(0, Math.Max(0, v - 20)); }));
            palette.ShowDialog(this);
        }

        private static DataTable BuildTable()
        {
            var table = new DataTable();
            table.Columns.Add("Project", typeof(string));
            table.Columns.Add("Owner", typeof(string));
            table.Columns.Add("Status", typeof(string));
            table.Columns.Add("Updated", typeof(string));
            table.Columns.Add("Budget", typeof(string));
            table.Rows.Add("Website refresh", "Ada Lovelace", "Active", "Today", "$12,400");
            table.Rows.Add("Mobile app", "Jonas Silva", "Active", "Yesterday", "$28,900");
            table.Rows.Add("Brand guidelines", "Mia Chen", "Pending", "3 days ago", "$4,200");
            table.Rows.Add("Help center", "Ada Lovelace", "Active", "Last week", "$8,750");
            table.Rows.Add("Q3 campaign", "Sam Patel", "Pending", "Last week", "$15,600");
            table.Rows.Add("Legacy archive", "Mia Chen", "Archived", "2 weeks ago", "$1,100");
            return table;
        }
    }
}
