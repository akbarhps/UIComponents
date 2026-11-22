// Covered: data grid (rows), filter table (whole-row search + status segments + count), selection table (bulk bar hidden/1 row/N rows), column filters (count drops on filter + Project/Team search + scoped pager), sortable table (sort asc/desc, stable ties, sort+filter composition, pin toggle, pin-scroll, re-pin after filter, scoped pager info text, empty state).
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
        private static List<string> TablesFirstColumn(AutomationElement grid)
        {
            var names = new List<string>();
            var rows = grid.FindAll(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataItem));
            foreach (AutomationElement row in rows)
            {
                var texts = row.FindAll(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text));
                if (texts.Count > 0) names.Add(GetValue(texts[0]));
            }
            return names;
        }

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
            if (filterSearch != null)
            {
                var filterCountLabel = Find(root, "FilterCount");
                // "ada" above also gives 2 matches: clear first so the wait cannot pass on stale text.
                SetValue(filterSearch, "");
                Check("filter search cleared", WaitFor(delegate
                {
                    return filterCountLabel != null && GetValue(filterCountLabel) == "6 of 6 projects";
                }, 2000), "count=" + (filterCountLabel == null ? "" : GetValue(filterCountLabel)));
                SetValue(filterSearch, "design");
                bool teamMatch = WaitFor(delegate
                {
                    return filterCountLabel != null && GetValue(filterCountLabel) == "2 of 6 projects";
                }, 2000);
                Check("filter search matches team text", teamMatch,
                    "count=" + (filterCountLabel == null ? "" : GetValue(filterCountLabel)));
                SetValue(filterSearch, "");
            }

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
            var columnPager = Find(root, "ColumnPagination");
            Check("column pager found", columnPager != null);
            Check("column info text from matched count",
                columnPager != null && FindByName(columnPager, "Showing 1–1 of 1") != null);
            var columnGrid = Find(root, "ColumnGrid");
            Check("column filter grid found", columnGrid != null);
            if (colProject != null)
            {
                SetValue(colProject, "zzzz");
                Check("column empty state appears", WaitFor(delegate { return Find(root, "ColEmpty") != null; }, 2000));
                SetValue(colProject, "");
                Check("column empty state clears", WaitFor(delegate { return Find(root, "ColEmpty") == null; }, 2000));
                SetValue(colProject, "operations");
                bool teamMatch = WaitFor(delegate
                {
                    return colCount != null && GetValue(colCount) == "1 of 6 projects";
                }, 2000);
                Check("column filter matches team text", teamMatch,
                    "count=" + (colCount == null ? "" : GetValue(colCount)));
                SetValue(colProject, "");
                WaitFor(delegate { return colCount != null && GetValue(colCount) == "6 of 6 projects"; }, 2000);
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
            // Stable ties: under Budget asc the two "Last week" rows are Help center then Q3
            // campaign. Updated asc must keep that prior order instead of swapping them.
            var updatedSort = Find(root, "HeaderUpdated");
            Check("updated sort header found", updatedSort != null);
            if (updatedSort != null)
            {
                Invoke(updatedSort);
                WaitFor(delegate { return FirstGridCell(root, "SortGrid") == "Website refresh"; }, 2000);
                var order = TablesFirstColumn(sortGrid);
                int q3 = order.IndexOf("Q3 campaign");
                int help = order.IndexOf("Help center");
                Check("stable sort keeps tied rows in prior order",
                    q3 >= 0 && help >= 0 && help < q3, "order=" + string.Join(",", order.ToArray()));
                Invoke(budgetSort);
                WaitFor(delegate { return FirstGridCell(root, "SortGrid") == "Legacy archive"; }, 2000);
            }
            var sortProjectBox = Find(root, "SortProject");
            Check("sortable filter found", sortProjectBox != null);
            if (sortProjectBox != null)
            {
                // "o" matches Mobile app, Help center and Legacy archive (3 rows); no other
                // pager shows 1–3 of 3, so the count assertion cannot pass on a stale pager.
                SetValue(sortProjectBox, "o");
                WaitFor(delegate { return FirstGridCell(root, "SortGrid") == "Legacy archive"; }, 2000);
                string composed = FirstGridCell(root, "SortGrid");
                Check("sort persists through filter", composed == "Legacy archive", "first=" + composed);
                var sortPager = Find(root, "SortPagination");
                Check("sort pager found", sortPager != null);
                Check("sortable info text from matched count",
                    sortPager != null && FindByName(sortPager, "Showing 1–3 of 3") != null);
                SetValue(sortProjectBox, "zzzz");
                Check("sortable empty state appears", WaitFor(delegate { return Find(root, "SortEmpty") != null; }, 2000));
                SetValue(sortProjectBox, "");
                Check("sortable empty state clears", WaitFor(delegate { return Find(root, "SortEmpty") == null; }, 2000));
            }
            // Re-pin, scroll, then filter: RefreshSortGrid must re-apply the pinned style and
            // transform to the freshly created cells without losing the scroll offset.
            if (pin != null && sortScroll != null && sortProjectBox != null)
            {
                var pinAgain = FastToggle.Of(pin);
                if (pinAgain.Current.ToggleState != ToggleState.On) pinAgain.Toggle();
                var scrollAgain = (ScrollPattern)sortScroll.GetCurrentPattern(ScrollPattern.Pattern);
                scrollAgain.SetScrollPercent(100, ScrollPattern.NoScroll);
                WaitFor(delegate { return scrollAgain.Current.HorizontalScrollPercent > 0; }, 2000);
                var cellBeforeFilter = FindByName(sortGrid, "Website refresh");
                double leftPinnedScrolled = cellBeforeFilter == null ? -1 : CellLeft(cellBeforeFilter);
                SetValue(sortProjectBox, "o");
                // Wait for the re-applied pin on the new cells, not just for the row text.
                bool rePinned = WaitFor(delegate
                {
                    var cell = FindByName(sortGrid, "Legacy archive");
                    return cell != null && Math.Abs(CellLeft(cell) - leftPinnedScrolled) <= 1 &&
                           scrollAgain.Current.HorizontalScrollPercent > 0;
                }, 2000);
                var cellAfterFilter = FindByName(sortGrid, "Legacy archive");
                double leftAfterFilter = cellAfterFilter == null ? -1 : CellLeft(cellAfterFilter);
                Check("pinned cell survives filter while scrolled",
                    rePinned && leftPinnedScrolled >= 0 && Math.Abs(leftAfterFilter - leftPinnedScrolled) <= 1,
                    "left before=" + leftPinnedScrolled + " after=" + leftAfterFilter);
                Check("pinned filter keeps scroll offset", scrollAgain.Current.HorizontalScrollPercent > 0,
                    "percent=" + scrollAgain.Current.HorizontalScrollPercent);
                SetValue(sortProjectBox, "");
                scrollAgain.SetScrollPercent(0, ScrollPattern.NoScroll);
                if (pinAgain.Current.ToggleState == ToggleState.On) pinAgain.Toggle();
            }
        }
    }
}
