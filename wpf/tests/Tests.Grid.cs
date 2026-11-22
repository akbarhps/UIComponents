// Covered: grid input (cell values, row-header selection, propagation, clear, copy TSV, add row/column), typed grid (no add column, checkbox toggle, select choice).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;

namespace UIComponents.Wpf.Tests
{
    internal static partial class UIAutomationTests
    {
        static partial void RunGrid(AutomationElement root, ref bool ran)
        {
            ran = true;
            // Grid input — Phase 6 data operations
            Check("grid input found", Find(root, "InputGrid") != null);
            var c00 = Find(root, "InputGrid_r0_c0");
            var c23 = Find(root, "InputGrid_r2_c3");
            Check("grid cell r0c0 value", c00 != null && GetValue(c00) == "Small", c00 == null ? "missing" : GetValue(c00));
            Check("grid cell r2c3 value", c23 != null && GetValue(c23) == "12", c23 == null ? "missing" : GetValue(c23));

            var row1 = Find(root, "InputGrid_row1");
            if (row1 != null) Invoke(row1);
            var info = Find(root, "InputGrid_Info");
            Check("grid row header selects 4", info != null && GetValue(info) == "4 cells selected", info == null ? "missing" : GetValue(info));

            var c10 = Find(root, "InputGrid_r1_c0");
            var c12 = Find(root, "InputGrid_r1_c2");
            if (c10 != null) SetValue(c10, "99");
            Check("grid typing propagates", c12 != null && GetValue(c12) == "99", c12 == null ? "missing" : GetValue(c12));

            var clear = Find(root, "InputGrid_Clear");
            if (clear != null) Invoke(clear);
            bool cleared = WaitFor(delegate { return c10 != null && GetValue(c10) == ""; }, 2000);
            Check("grid clear selected", cleared, c10 == null ? "missing" : GetValue(c10));

            IDataObject savedClipboard;
            bool snapshotOk = SnapshotClipboard(out savedClipboard);
            string expectedCopy = "";
            string copied = null;
            var col0 = Find(root, "InputGrid_col0");
            var copy = Find(root, "InputGrid_Copy");
            try
            {
                if (col0 != null) Invoke(col0);
                for (int r = 0; r < 3; r++)
                {
                    var cell = Find(root, "InputGrid_r" + r + "_c0");
                    if (r > 0) expectedCopy += "\n";
                    expectedCopy += cell == null ? "" : GetValue(cell);
                }
                if (copy != null) Invoke(copy);
                for (int i = 0; i < 20; i++)
                {
                    copied = ReadClipboardText();
                    if (copied == expectedCopy) break;
                    Thread.Sleep(100);
                }
            }
            finally
            {
                if (snapshotOk) RestoreClipboard(savedClipboard);
            }
            var infoAfterCol = Find(root, "InputGrid_Info");
            Check("grid copy column TSV", copied == expectedCopy,
                "info=" + (infoAfterCol == null ? "missing" : GetValue(infoAfterCol)) +
                " expected=[" + expectedCopy + "] actual=[" + copied + "]");

            var addRow = Find(root, "InputGrid_AddRow");
            if (addRow != null) Invoke(addRow);
            Check("grid add row", Find(root, "InputGrid_r3_c0") != null);
            var addCol = Find(root, "InputGrid_AddCol");
            if (addCol != null) Invoke(addCol);
            var col4 = Find(root, "InputGrid_col4");
            Check("grid add column", col4 != null && col4.Current.Name == "Column 5", col4 == null ? "missing" : col4.Current.Name);

            // Typed grid — no add column, checkbox and select patterns
            Check("typed grid found", Find(root, "TypedGrid") != null);
            Check("typed grid has no add column", Find(root, "TypedGrid_AddCol") == null);
            var checkCell = Find(root, "TypedGrid_r0_c4");
            bool checkOn = false, checkToggled = false;
            if (checkCell != null)
            {
                var toggle = FastToggle.Of(checkCell);
                checkOn = toggle.Current.ToggleState == ToggleState.On;
                toggle.Toggle();
                checkToggled = toggle.Current.ToggleState == ToggleState.Off;
            }
            Check("typed checkbox initially on", checkOn);
            Check("typed checkbox toggles off", checkToggled);

            var selectCell = Find(root, "TypedGrid_r0_c3");
            bool selectedCustom = false;
            if (selectCell != null)
            {
                var expand = FastExpandCollapse.Of(selectCell);
                expand.Expand();
                var custom = selectCell.FindFirst(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.NameProperty, "Custom"));
                if (custom != null)
                {
                    var selection = FastSelectionItem.Of(custom);
                    selection.Select();
                    var comboSelection = (SelectionPattern)selectCell.GetCurrentPattern(SelectionPattern.Pattern);
                    var chosen = comboSelection.Current.GetSelection();
                    selectedCustom = chosen.Length > 0 && chosen[0].Current.Name == "Custom";
                }
                try { expand.Collapse(); } catch (InvalidOperationException) { }
            }
            Check("typed select chooses Custom", selectedCustom, selectCell == null ? "missing" : "selection mismatch");
        }
    }
}
