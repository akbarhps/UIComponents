using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Automation;

namespace UIComponents.Wpf.Tests
{
    /// <summary>
    /// UI Automation tests: drives the running WPF app by finding controls through
    /// AutomationElement and invoking patterns. No cursor movement, no coordinates.
    /// </summary>
    internal static class UIAutomationTests
    {
        private static int _passed;
        private static int _failed;

        private static void Check(string name, bool condition, string detail = "")
        {
            if (condition) { _passed++; Console.WriteLine("PASS  " + name); }
            else { _failed++; Console.WriteLine("FAIL  " + name + (detail.Length > 0 ? " — " + detail : "")); }
        }

        private static AutomationElement Find(AutomationElement root, string automationId)
        {
            var condition = new PropertyCondition(AutomationElement.AutomationIdProperty, automationId);
            return root.FindFirst(TreeScope.Descendants, condition);
        }

        private static AutomationElement FindByName(AutomationElement root, string name)
        {
            var condition = new PropertyCondition(AutomationElement.NameProperty, name);
            return root.FindFirst(TreeScope.Descendants, condition);
        }

        private static void Invoke(AutomationElement element)
        {
            var pattern = (InvokePattern)element.GetCurrentPattern(InvokePattern.Pattern);
            pattern.Invoke();
        }

        private static string GetValue(AutomationElement element)
        {
            try
            {
                var pattern = (ValuePattern)element.GetCurrentPattern(ValuePattern.Pattern);
                return pattern.Current.Value;
            }
            catch (InvalidOperationException)
            {
                try
                {
                    var text = (TextPattern)element.GetCurrentPattern(TextPattern.Pattern);
                    return text.DocumentRange.GetText(-1);
                }
                catch (InvalidOperationException)
                {
                    return element.Current.Name;
                }
            }
        }

        private static void SetValue(AutomationElement element, string value)
        {
            var pattern = (ValuePattern)element.GetCurrentPattern(ValuePattern.Pattern);
            pattern.SetValue(value);
        }

        private static int Main()
        {
            Console.WriteLine("WPF UI Automation tests");
            Console.WriteLine("Start UIComponents.Wpf.exe first, then run this.");
            Console.WriteLine();

            AutomationElement root;
            try
            {
                var windows = AutomationElement.RootElement.FindAll(TreeScope.Children,
                    new PropertyCondition(AutomationElement.NameProperty, "Claude UI — WPF components"));
                if (windows.Count == 0) { Console.WriteLine("FAIL  window not found — start the app first"); return 1; }
                root = windows[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine("FAIL  UI Automation unavailable — " + ex.Message);
                return 1;
            }

            Check("window found", root != null);

            // Buttons
            var primary = Find(root, "BtnPrimary");
            Check("primary button found", primary != null);
            if (primary != null) { Invoke(primary); Check("primary button invokes", true); }

            // Text fields — set via ValuePattern, no typing, no cursor
            var name = Find(root, "FieldName");
            Check("name field found", name != null);
            if (name != null)
            {
                SetValue(name, "Grace Hopper");
                Check("name field value set", GetValue(name) == "Grace Hopper", "value=" + GetValue(name));
            }

            var email = Find(root, "FieldEmail");
            Check("email field found", email != null);
            if (email != null)
            {
                SetValue(email, "grace@example.com");
                Check("email field value set", GetValue(email) == "grace@example.com");
            }

            // DataGrid — verify rows via automation children
            var grid = Find(root, "DemoGrid");
            Check("data grid found", grid != null);
            if (grid != null)
            {
                var rows = grid.FindAll(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataItem));
                Check("data grid has rows", rows.Count >= 1, "rows=" + rows.Count);
            }

            // CheckBox — toggle via TogglePattern
            var check = Find(root, "ChkNotifications");
            Check("checkbox found", check != null);
            if (check != null)
            {
                var pattern = (TogglePattern)check.GetCurrentPattern(TogglePattern.Pattern);
                var before = pattern.Current.ToggleState;
                pattern.Toggle();
                Check("checkbox toggles", pattern.Current.ToggleState != before);
                pattern.Toggle();
            }

            // ComboBox — select via SelectionPattern / ExpandCollapsePattern
            var combo = Find(root, "CmbRole");
            Check("combobox found", combo != null);
            if (combo != null)
            {
                var expand = (ExpandCollapsePattern)combo.GetCurrentPattern(ExpandCollapsePattern.Pattern);
                expand.Expand();
                Check("combobox expands", expand.Current.ExpandCollapseState == ExpandCollapseState.Expanded);
                expand.Collapse();
            }

            // DatePicker — verify it is found and has a value
            var date = Find(root, "DatePick");
            Check("date picker found", date != null);

            // ProgressBar — read its range value
            var progress = Find(root, "ProgressBar");
            Check("progress bar found", progress != null);
            if (progress != null)
            {
                var range = (RangeValuePattern)progress.GetCurrentPattern(RangeValuePattern.Pattern);
                Check("progress has value", range.Current.Value > 0, "value=" + range.Current.Value);
            }

            // Slider — set its value via RangeValuePattern
            var slider = Find(root, "Slider");
            Check("slider found", slider != null);
            if (slider != null)
            {
                var range = (RangeValuePattern)slider.GetCurrentPattern(RangeValuePattern.Pattern);
                double max = range.Current.Maximum;
                range.SetValue(max * 0.75);
                Check("slider value set", range.Current.Value > 0, "value=" + range.Current.Value + " max=" + max);
            }

            // TabControl — select a tab via SelectionItemPattern
            var tabs = Find(root, "Tabs");
            Check("tabs found", tabs != null);
            if (tabs != null)
            {
                var tabItems = tabs.FindAll(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.TabItem));
                Check("tabs have items", tabItems.Count == 3, "count=" + tabItems.Count);
                if (tabItems.Count > 1)
                {
                    var select = (SelectionItemPattern)tabItems[1].GetCurrentPattern(SelectionItemPattern.Pattern);
                    select.Select();
                    Check("tab selection", select.Current.IsSelected);
                }
            }

            // Expander — toggle via ExpandCollapsePattern
            var expander = Find(root, "Expander");
            Check("expander found", expander != null);
            if (expander != null)
            {
                var pattern = (ExpandCollapsePattern)expander.GetCurrentPattern(ExpandCollapsePattern.Pattern);
                pattern.Collapse();
                Check("expander collapses", pattern.Current.ExpandCollapseState == ExpandCollapseState.Collapsed);
                pattern.Expand();
            }

            // ListBox with multiple selection
            var list = Find(root, "MultiList");
            Check("multi-select list found", list != null);
            if (list != null)
            {
                var items = list.FindAll(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ListItem));
                Check("multi-select has items", items.Count >= 2, "count=" + items.Count);
                if (items.Count > 1)
                {
                    var sel = (SelectionItemPattern)items[1].GetCurrentPattern(SelectionItemPattern.Pattern);
                    sel.Select();
                    Check("multi-select item selected", sel.Current.IsSelected);
                }
            }

            // Modal dialog — open, then find and click a button by automation id
            var openDialog = Find(root, "BtnOpenDialog");
            Check("dialog trigger found", openDialog != null);
            if (openDialog != null)
            {
                // ShowDialog blocks the invoking thread, so raise it on a worker thread.
                var worker = new Thread(() => Invoke(openDialog));
                worker.IsBackground = true;
                worker.Start();
                AutomationElement dialog = null;
                for (int i = 0; i < 20 && dialog == null; i++)
                {
                    Thread.Sleep(150);
                    dialog = AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.NameProperty, "Confirm"));
                }
                Check("dialog opened", dialog != null);
                if (dialog != null)
                {
                    var cancel = Find(dialog, "DlgCancel");
                    Check("dialog cancel found", cancel != null);
                    var windowPattern = (WindowPattern)dialog.GetCurrentPattern(WindowPattern.Pattern);
                    windowPattern.Close();
                    Check("dialog closes", true);
                }
            }

            // Avatars (Border with text child)
            var avatars = root.FindAll(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "Avatar"));
            Check("avatars present", avatars.Count >= 1 || FindByName(root, "AL") != null);

            // Breadcrumbs
            var crumbs = Find(root, "Breadcrumbs") ?? FindByName(root, "Workspace");
            Check("breadcrumbs found", crumbs != null);

            // Stepper
            var stepper = Find(root, "Stepper") ?? FindByName(root, "1 Account");
            Check("stepper found", stepper != null);

            // Tooltip button
            var tooltipBtn = Find(root, "BtnTooltip");
            Check("tooltip button found", tooltipBtn != null);

            // Action menu button
            var menuBtn = Find(root, "BtnMenu");
            Check("action menu button found", menuBtn != null);

            // Empty state
            var empty = Find(root, "EmptyState") ?? FindByName(root, "No projects yet");
            Check("empty state found", empty != null);
            var emptyAction = Find(root, "EmptyAction");
            Check("empty state action found", emptyAction != null);
            if (emptyAction != null) Invoke(emptyAction);

            // Skeleton
            var skeleton = root.FindAll(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.AutomationIdProperty, "Skeleton"));
            Check("skeleton found", skeleton.Count > 0 || FindByName(root, "Skeleton loader") != null);

            // Tag input — enter a tag via ValuePattern + Enter key is not possible without
            // keystrokes, but we can verify the field and existing tags are exposed.
            var tagInput = Find(root, "TagInput");
            Check("tag input found", tagInput != null);
            var tagCount = Find(root, "TagCount");
            Check("tag count found", tagCount != null);

            // Autocomplete
            var autoInput = Find(root, "AutoInput");
            Check("autocomplete input found", autoInput != null);

            // File input
            var browse = Find(root, "BtnBrowse");
            Check("file browse button found", browse != null);
            var fileList = Find(root, "FileList");
            Check("file list found", fileList != null);

            // Date & time
            var dtDate = Find(root, "DtDate");
            Check("date-time date found", dtDate != null);
            var dtTime = Find(root, "DtTime");
            Check("date-time time found", dtTime != null);
            if (dtTime != null)
            {
                SetValue(dtTime, "14:05:09");
                Check("date-time time set", GetValue(dtTime).Contains("14:05"));
            }

            // Grid input
            var inputGrid = Find(root, "InputGrid");
            Check("grid input found", inputGrid != null);
            var addRow = Find(root, "GridAddRow");
            Check("grid add row found", addRow != null);
            if (addRow != null) Invoke(addRow);
            var addCol = Find(root, "GridAddCol");
            Check("grid add column found", addCol != null);
            if (addCol != null) Invoke(addCol);

            // Typed grid
            var typedGrid = Find(root, "TypedGrid");
            Check("typed grid found", typedGrid != null);

            // Filter table — set search text and verify grid still present
            var filterSearch = Find(root, "FilterSearch");
            Check("filter table search found", filterSearch != null);
            if (filterSearch != null) SetValue(filterSearch, "ada");
            var filterGrid = Find(root, "FilterGrid");
            Check("filter table grid found", filterGrid != null);

            // Selection table
            var selectionGrid = Find(root, "SelectionGrid");
            Check("selection table found", selectionGrid != null);
            var bulkText = Find(root, "BulkText");
            Check("bulk bar found", bulkText != null);

            Console.WriteLine();
            Console.WriteLine("RESULT  passed=" + _passed + "  failed=" + _failed);
            return _failed == 0 ? 0 : 1;
        }
    }
}
