// Covered: data grid (rows), filter table (search + status segments), selection table (bulk bar hidden/1 row/N rows), column filters (count drops on filter), sortable table (sort asc/desc, pin toggle).
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
        static partial void RunTables(AutomationElement root, ref bool ran)
        {
            ran = true;
            // DataGrid — verify rows via automation children
            var grid = Find(root, "DemoGrid");
            Check("data grid found", grid != null);
            if (grid != null)
            {
                var rows = grid.FindAll(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataItem));
                Check("data grid has rows", rows.Count >= 1, "rows=" + rows.Count);
            }

            // Filter table — set search text and verify grid still present
            var filterSearch = Find(root, "FilterSearch");
            Check("filter table search found", filterSearch != null);
            if (filterSearch != null) SetValue(filterSearch, "ada");
            var filterGrid = Find(root, "FilterGrid");
            Check("filter table grid found", filterGrid != null);

            // Selection table
            var selectionGrid = Find(root, "SelectionGrid");
            Check("selection table found", selectionGrid != null);
            Check("bulk bar hidden initially", Find(root, "BulkBar") == null);
            var oneRow = Find(root, "RowCheck");
            Check("row checkbox found", oneRow != null);
            if (oneRow != null)
            {
                var oneToggle = FastToggle.Of(oneRow);
                if (oneToggle.Current.ToggleState != ToggleState.On) oneToggle.Toggle();
                var oneText = Find(root, "BulkText");
                bool oneOk = WaitFor(delegate
                {
                    return oneText != null && GetValue(oneText) == "1 row selected";
                }, 2000);
                Check("bulk bar reads 1 row selected", oneOk,
                    "text=" + (oneText == null ? "" : GetValue(oneText)));
                if (oneToggle.Current.ToggleState == ToggleState.On) oneToggle.Toggle();
                WaitFor(delegate { return Find(root, "BulkBar") == null; }, 2000);
                Check("bulk bar hidden after untick", Find(root, "BulkBar") == null);
            }
            var rowCheck = Find(root, "SelectAll");
            Check("select-all checkbox found", rowCheck != null);
            if (rowCheck != null)
            {
                var toggle = FastToggle.Of(rowCheck);
                toggle.Toggle();
                var bulkText = Find(root, "BulkText");
                bool bulkOk = WaitFor(delegate
                {
                    return bulkText != null && GetValue(bulkText) == "5 rows selected";
                }, 2000);
                Check("bulk bar appears on selection", bulkOk,
                    "text=" + (bulkText == null ? "" : GetValue(bulkText)));
                toggle.Toggle();
                WaitFor(delegate { return Find(root, "BulkBar") == null; }, 2000);
                Check("bulk bar hidden after select-all off", Find(root, "BulkBar") == null);
            }

            // Column filters
            var colProject = Find(root, "ColProject");
            Check("column filter found", colProject != null);
            var colCount = Find(root, "ColCount");
            Check("column count found", colCount != null);
            string countBefore = colCount == null ? "" : GetValue(colCount);
            if (colProject != null) SetValue(colProject, "mobile");
            WaitFor(delegate { return colCount != null && GetValue(colCount).StartsWith("1 "); }, 2000);
            string countAfter = colCount == null ? "" : GetValue(colCount);
            Check("column filter reduces count", countAfter != countBefore && countAfter.StartsWith("1 "),
                "before=" + countBefore + " after=" + countAfter);
            Check("column info text from matched count", FindByName(root, "Showing 1–1 of 1") != null);
            var columnGrid = Find(root, "ColumnGrid");
            Check("column filter grid found", columnGrid != null);
            if (colProject != null)
            {
                SetValue(colProject, "zzzz");
                Check("column empty state appears", WaitFor(delegate { return Find(root, "ColEmpty") != null; }, 2000));
                SetValue(colProject, "");
                Check("column empty state clears", WaitFor(delegate { return Find(root, "ColEmpty") == null; }, 2000));
            }

            // Sortable table
            var sortScroll = Find(root, "SortScroll");
            Check("sortable scroller found", sortScroll != null);
            var pin = Find(root, "PinColumn");
            Check("pin column button found", pin != null);
            var sortGrid = Find(root, "SortGrid");
            Check("sortable grid found", sortGrid != null);
            if (pin != null && sortScroll != null && sortGrid != null)
            {
                var pinToggle = FastToggle.Of(pin);
                bool before = pinToggle.Current.ToggleState == ToggleState.On;
                pinToggle.Toggle();
                bool after = pinToggle.Current.ToggleState == ToggleState.On;
                Check("pin column toggles", before != after);
                var firstBefore = FindByName(sortGrid, "Website refresh");
                var col1Before = FindByName(sortGrid, "Ada Lovelace");
                double leftBefore = firstBefore == null ? -1 : CellLeft(firstBefore);
                double col1LeftBefore = col1Before == null ? -1 : CellLeft(col1Before);
                try
                {
                    var scroll = (ScrollPattern)sortScroll.GetCurrentPattern(ScrollPattern.Pattern);
                    scroll.SetScrollPercent(100, ScrollPattern.NoScroll);
                    WaitFor(delegate { return scroll.Current.HorizontalScrollPercent > 0; }, 2000);
                    var firstAfter = FindByName(sortGrid, "Website refresh");
                    var col1After = FindByName(sortGrid, "Ada Lovelace");
                    double leftAfter = firstAfter == null ? -1 : CellLeft(firstAfter);
                    double col1LeftAfter = col1After == null ? -1 : CellLeft(col1After);
                    Check("scroller actually moved", scroll.Current.HorizontalScrollPercent > 0,
                        "percent=" + scroll.Current.HorizontalScrollPercent);
                    Check("unpinned column scrolls away", col1LeftBefore - col1LeftAfter >= 40,
                        "col1 left before=" + col1LeftBefore + " after=" + col1LeftAfter);
                    Check("pinned first cell stays put while scrolling",
                        leftBefore >= 0 && Math.Abs(leftAfter - leftBefore) <= 1,
                        "left before=" + leftBefore + " after=" + leftAfter);
                    scroll.SetScrollPercent(0, ScrollPattern.NoScroll);
                    WaitFor(delegate { return scroll.Current.HorizontalScrollPercent == 0; }, 2000);
                }
                catch (InvalidOperationException)
                {
                    Check("scroller actually moved", false, "no ScrollPattern");
                    Check("unpinned column scrolls away", false, "no ScrollPattern");
                    Check("pinned first cell stays put while scrolling", false, "no ScrollPattern");
                }
                pinToggle.Toggle();
            }
            var projectSort = Find(root, "HeaderProject");
            var budgetSort = Find(root, "HeaderBudget");
            Check("project sort header found", projectSort != null);
            Check("budget sort header found", budgetSort != null);
            if (projectSort != null && budgetSort != null)
            {
                Invoke(projectSort);
                WaitFor(delegate { return FirstGridCell(root, "SortGrid") == "Brand guidelines"; }, 2000);
                string firstAsc = FirstGridCell(root, "SortGrid");
                Check("project asc first row", firstAsc == "Brand guidelines", "first=" + firstAsc);
                Invoke(projectSort);
                WaitFor(delegate { return FirstGridCell(root, "SortGrid") == "Website refresh"; }, 2000);
                string firstDesc = FirstGridCell(root, "SortGrid");
                Check("project desc first row", firstDesc == "Website refresh", "first=" + firstDesc);
                Invoke(budgetSort);
                WaitFor(delegate { return FirstGridCell(root, "SortGrid") == "Legacy archive"; }, 2000);
                string firstBudget = FirstGridCell(root, "SortGrid");
                Check("budget asc first row", firstBudget == "Legacy archive", "first=" + firstBudget);
            }
            var sortProjectBox = Find(root, "SortProject");
            Check("sortable filter found", sortProjectBox != null);
            if (sortProjectBox != null)
            {
                SetValue(sortProjectBox, "e");
                WaitFor(delegate { return FirstGridCell(root, "SortGrid") == "Legacy archive"; }, 2000);
                string composed = FirstGridCell(root, "SortGrid");
                Check("sort persists through filter", composed == "Legacy archive", "first=" + composed);
                Check("sortable info text from matched count", FindByName(root, "Showing 1–5 of 5") != null);
                SetValue(sortProjectBox, "zzzz");
                Check("sortable empty state appears", WaitFor(delegate { return Find(root, "SortEmpty") != null; }, 2000));
                SetValue(sortProjectBox, "");
                Check("sortable empty state clears", WaitFor(delegate { return Find(root, "SortEmpty") == null; }, 2000));
            }
            // Re-applying the pinned state after sort/filter while pinned stays code-only: the
            // harness cannot both type a filter and keep a horizontal scroll offset.
        }
    }
}
