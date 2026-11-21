using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UIComponents.Tests
{
    /// <summary>Headless harness: instantiates each component and exercises its logic with assertions.</summary>
    internal static class ComponentTests
    {
        private static int _passed;
        private static int _failed;
        private static Form _host;

        private static void Check(string name, bool condition, string detail = "")
        {
            if (condition) { _passed++; Console.WriteLine("PASS  " + name); }
            else { _failed++; Console.WriteLine("FAIL  " + name + (detail.Length > 0 ? " — " + detail : "")); }
        }

        private static object Call(object target, string method, params object[] args)
        {
            var type = target.GetType();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var info = type.GetMethod(method, flags, null, args.Select(a => a == null ? typeof(object) : a.GetType()).ToArray(), null);
            if (info == null) throw new MissingMethodException(type.Name, method);
            return info.Invoke(target, args);
        }

        private static void Set(object target, string field, object value)
        {
            var info = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (info != null) info.SetValue(target, value);
        }

        private static object Get(object target, string field)
        {
            var info = target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return info == null ? null : info.GetValue(target);
        }

        [STAThread]
        private static int Main()
        {
            Application.EnableVisualStyles();
            _host = new Form { Size = new Size(1000, 800) };
            _host.Show();

            TestButtons();
            TestBadgesAndAlerts();
            TestInputs();
            TestSelectors();
            TestGrids();
            TestTables();
            TestNavigation();
            TestOverlays();

            _host.Close();
            Console.WriteLine();
            Console.WriteLine("RESULT  passed=" + _passed + "  failed=" + _failed);
            return _failed == 0 ? 0 : 1;
        }

        private static void TestButtons()
        {
            Console.WriteLine("\n[Buttons]");
            var b = new ClaudeButton { Text = "Save" };
            Check("button default variant primary", b.Variant == ButtonVariant.Primary);
            Check("button radius 8", b.Radius == 8);
            b.Variant = ButtonVariant.Danger;
            Check("button variant set", b.Variant == ButtonVariant.Danger);
            b.Loading = true;
            b.Tick();
            Check("button loading tick safe", b.Loading);
            b.Loading = false;
            bool clicked = false;
            b.Click += (s, e) => clicked = true;
            Call(b, "OnClick", EventArgs.Empty);
            Check("button click raises event", clicked);
        }

        private static void TestBadgesAndAlerts()
        {
            Console.WriteLine("\n[Badges & alerts]");
            var badge = new ClaudeBadge { Text = "Active", Tone = BadgeTone.Success };
            Check("badge tone", badge.Tone == BadgeTone.Success && badge.ShowDot);
            badge.ShowDot = false;
            Check("badge dot toggle", !badge.ShowDot);

            var alert = new ClaudeAlert { Tone = AlertTone.Danger, Title = "Oops", Message = "Try again" };
            Check("alert content", alert.Title == "Oops" && alert.Message == "Try again" && alert.Tone == AlertTone.Danger);

            var card = new ClaudeCard { Width = 200, Height = 100 };
            Check("card surface", card.BackColor == Theme.Surface);
            var divider = new ClaudeDivider();
            Check("divider line", divider.Height == 1 && divider.BackColor == Theme.Line);

            var sw = new ClaudeSwitch { Checked = true, Caption = "On" };
            Call(sw, "OnClick", EventArgs.Empty);
            Check("switch toggles", !sw.Checked);

            var progress = new ClaudeProgress { Value = 150 };
            Check("progress clamps high", progress.Value == 100);
            progress.Value = -10;
            Check("progress clamps low", progress.Value == 0);

            var avatar = new ClaudeAvatar { Initials = "MK" };
            Check("avatar initials", avatar.Initials == "MK" && avatar.Tone == Theme.BrandSoft);

            var skeleton = new ClaudeSkeleton { Width = 100, Height = 10 };
            skeleton.Dispose();
            Check("skeleton dispose safe", true);
        }

        private static void TestInputs()
        {
            Console.WriteLine("\n[Inputs]");
            var field = new ClaudeField { LabelText = "Name", TextValue = "Ada", Hint = "Hint" };
            Check("field text", field.TextValue == "Ada" && field.LabelText == "Name");
            field.HasError = true;
            Check("field error", field.HasError);
            field.HasError = false;
            var select = new ClaudeField { IsSelect = true, Options = new[] { "A", "B" } };
            Check("field select options", select.IsSelect && select.TextValue == "A");
            var area = new ClaudeField { Multiline = true };
            Check("field multiline", area.Multiline);

            var tags = new ClaudeTagInput(new[] { "design" }) { MaxTags = 2 };
            Call(tags, "Add", "research");
            Check("tag add", tags.Tags.Length == 2);
            Call(tags, "Add", "DESIGN");
            Check("tag duplicate rejected", tags.Tags.Length == 2);
            Call(tags, "Add", "third");
            Check("tag max", tags.Tags.Length == 2);

            var auto = new ClaudeAutocomplete(new[] { "React", "Vue" });
            Call(auto, "Filter");
            Check("autocomplete options", auto.Controls.Count > 0);
            Call(auto, "Choose", "React");
            Check("autocomplete choose", auto.TextValue == "React");

            var dates = new ClaudeDatePicker { Mode = DatePickerMode.MonthYear };
            Check("datepicker month mode", dates.Mode == DatePickerMode.MonthYear && dates.Display.Contains("2026"), "display=" + dates.Display);
            dates.Mode = DatePickerMode.Year;
            Check("datepicker year mode", dates.Display.Length == 4 && dates.Display.StartsWith("20"), "display=" + dates.Display);

            var dt = new ClaudeDateTimePicker(true);
            dt.Value = new DateTime(2026, 11, 21, 14, 5, 9);
            Check("datetime value", dt.Value.Year == 2026 && dt.Value.Hour == 14 && dt.Value.Minute == 5 && dt.Value.Second == 9);

            var files = new ClaudeFileInput();
            Check("file input empty", files.Files.Length == 0);
        }

        private static void TestSelectors()
        {
            Console.WriteLine("\n[Selectors]");
            var search = new ClaudeSearchSelect(new[] { "Ada", "Jonas" }, "Select…");
            Check("search placeholder", search.SelectedValue == "");
            Call(search, "Filter", "ada");
            Check("search filter", search.Controls.Count > 0);
            Call(search, "Choose", "Ada");
            Check("search choose", search.SelectedValue == "Ada");

            var multi = new ClaudeMultiSelect(new[] { "React", "Vue" }, "Select…");
            var list = (CheckedListBox)Get(multi, "_list");
            list.SetItemChecked(0, true);
            Call(multi, "Render");
            Check("multi select values", multi.SelectedValues.Length == 1 && multi.SelectedValues[0] == "React");
            var all = new ClaudeButton { Text = "Select all" };
            Call(multi, "Toggle");
            Check("multi dropdown opens", multi.Height > 46);
        }

        private static void TestGrids()
        {
            Console.WriteLine("\n[Grids]");
            var columns = new[]
            {
                new GridColumnDef("Name", GridColumnType.Text),
                new GridColumnDef("Category", GridColumnType.Select, new[] { "A", "B" }),
                new GridColumnDef("Active", GridColumnType.Checkbox)
            };
            var rows = new string[,] { { "One", "A", "true" }, { "Two", "B", "false" } };
            var grid = new ClaudeGrid(columns, rows);
            var view = (DataGridView)Get(grid, "_grid");
            Check("grid columns", view.Columns.Count == 3);
            Check("grid rows", view.Rows.Count == 2);
            Check("grid select column type", view.Columns[1] is DataGridViewComboBoxColumn);
            Check("grid checkbox column type", view.Columns[2] is DataGridViewCheckBoxColumn);
            view.Rows.Add();
            Check("grid add row", view.Rows.Count == 3);

            var locked = new ClaudeGrid(columns, rows, false);
            Check("grid no add column", ((Panel)locked.Controls.Cast<Control>().First(c => c.Dock == DockStyle.Top)).Controls.Count == 3);

        }

        private static DataTable SampleTable()
        {
            var t = new DataTable();
            t.Columns.Add("Project", typeof(string));
            t.Columns.Add("Owner", typeof(string));
            t.Columns.Add("Status", typeof(string));
            t.Rows.Add("Website", "Ada", "Active");
            t.Rows.Add("Mobile", "Jonas", "Pending");
            t.Rows.Add("Brand", "Mia", "Archived");
            return t;
        }

        private static void TestTables()
        {
            Console.WriteLine("\n[Tables]");
            var filter = new ClaudeTable(SampleTable(), new TableOptions { ShowFilter = true, ShowPagination = true, PageSize = 1 });
            var view = (DataGridView)filter.Controls.OfType<DataGridView>().First();
            Check("table pagination rows", view.Rows.Count == 1);
            Check("table page info", ((Label)Get(filter, "_pageInfo")).Text.Contains("of 3"));
            var search = (TextBox)Get(filter, "_search");
            search.Text = "mobile";
            Check("table search", ((Label)Get(filter, "_pageInfo")).Text.Contains("of 1"));

            var cols = new ClaudeTable(SampleTable(), new TableOptions { ShowColumnFilters = true });
            var panels = cols.Controls.OfType<Panel>().ToList();
            var textBoxes = panels[0].Controls.OfType<TextBox>().ToList();
            var ownerFilter = textBoxes[1];
            ownerFilter.Text = "ada";
            Check("table column filter", ((Label)Get(cols, "_pageInfo")).Text.Contains("of 1"),
                "info=" + ((Label)Get(cols, "_pageInfo")).Text + " boxes=" + textBoxes.Count);

            var sticky = new ClaudeTable(SampleTable(), new TableOptions { StickyFirst = true, Sortable = true });
            var stickyView = sticky.Controls.OfType<DataGridView>().First();
            Check("table sticky first", stickyView.Columns[0].Frozen);
            Check("table sortable", stickyView.Columns[0].SortMode == DataGridViewColumnSortMode.Automatic);

            var selection = new ClaudeTable(SampleTable(), new TableOptions { ShowSelection = true, ShowPagination = true });
            var selectionView = selection.Controls.OfType<DataGridView>().First();
            Check("table no default selection", selectionView.SelectedRows.Count == 0);
            selectionView.Rows[0].Selected = true;
            Call(selection, "UpdateBulk");
            Check("table bulk bar", ((Panel)Get(selection, "_bulkBar")).Visible);
        }

        private static void TestNavigation()
        {
            Console.WriteLine("\n[Navigation]");
            var tabs = new ClaudeTabs();
            tabs.Add("One"); tabs.Add("Two");
            tabs.SelectedIndex = 1;
            Check("tabs selection", tabs.SelectedIndex == 1);

            var acc = new ClaudeAccordion();
            acc.Add("A", "Body A", true);
            acc.Add("B", "Body B");
            Check("accordion expanded", acc.Controls.Count > 2);

            var stepper = new ClaudeStepper();
            stepper.Add("1"); stepper.Add("2"); stepper.Add("3");
            stepper.Current = 5;
            Check("stepper clamps", stepper.Current == 2);

            var crumbs = new ClaudeBreadcrumbs();
            crumbs.Add("Home"); crumbs.Add("Page");
            Check("breadcrumbs", crumbs.Controls.Count == 0 && crumbs.Height == 28);

            var sidebar = new ClaudeSidebar();
            sidebar.AddGroup("Basics", "Buttons", "Badges");
            Check("sidebar groups", sidebar.Controls.Count > 2);
            Call(sidebar, "ApplyCollapse");
            Check("sidebar collapse safe", true);

            var empty = new ClaudeEmptyState("▢", "Nothing", "Message", "Add");
            Check("empty state", empty.Controls.Count == 4);

            var toast = new ClaudeToastHost();
            toast.Show("Saved", "Done", AlertTone.Success, true);
            Check("toast persistent", toast.Controls.Count == 1);
        }

        private static void TestOverlays()
        {
            Console.WriteLine("\n[Overlays]");
            var modal = new ClaudeModalForm();
            modal.Build();
            Check("modal built", modal.Controls.Count == 5);

            var palette = new ClaudeCommandPalette();
            bool ran = false;
            palette.Add(new CommandEntry("Actions", "Do it", () => ran = true));
            Call(palette, "Filter");
            Check("palette filter", ((ListBox)Get(palette, "_list")).Items.Count == 1);
            Call(palette, "Run", ((List<CommandEntry>)Get(palette, "_filtered"))[0]);
            Check("palette runs command", ran);

            var loading = new ClaudeLoadingDialog("Please wait");
            Check("loading message", loading.Message == "Please wait");
            loading.Close();

            var drawer = new ClaudeDrawerForm();
            drawer.Show();
            var confirm = (Panel)Get(drawer, "_confirm");
            Check("drawer confirm hidden", !confirm.Visible);
            confirm.Visible = true;
            Check("drawer confirm can show", confirm.Visible);
            confirm.Visible = false;
            var delete = drawer.Controls.OfType<ClaudeButton>().First(b => b.Text == "Delete");
            bool clickFired = false;
            delete.Click += (s, e) => clickFired = true;
            delete.SimulateClick();
            Check("drawer delete click fires", clickFired);
            Check("drawer confirm shows", confirm.Visible, "confirmVisible=" + confirm.Visible + " buttons=" + string.Join(",", drawer.Controls.OfType<ClaudeButton>().Select(b => b.Text)));
            drawer.Close();
        }
    }
}
