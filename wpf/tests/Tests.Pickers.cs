// Covered: file input, date picker.
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
        static partial void RunPickers(AutomationElement root, ref bool ran)
        {
            ran = true;
            // File input
            var browse = Find(root, "BtnBrowse");
            Check("file browse button found", browse != null);
            var fileList = Find(root, "FileList");
            Check("file list found", fileList != null);

            // Date picker (07 P1) — find the section once, then search inside the cached scopes.
            var dateSection = Find(root, "DatePickerCard");
            Check("P date section found", dateSection != null);
            if (dateSection != null)
            {
                var pickTrigger = Find(dateSection, "PickDate_Trigger");
                Check("P1 trigger found", pickTrigger != null);
                if (pickTrigger != null)
                {
                    var pickValue = Find(dateSection, "PickDate_Value");
                    Check("P1 placeholder", pickValue != null && GetValue(pickValue) == "Pick a date…",
                        pickValue == null ? "missing" : GetValue(pickValue));
                    Check("P1 panel absent when closed", Find(root, "PickDate_Panel") == null);
                    var pickToggle = FastToggle.Of(pickTrigger);
                    Func<AutomationElement> openPanel = delegate { return Find(root, "PickDate_Panel"); };
                    string expectedDate = "";

                    pickToggle.Toggle();
                    Check("P2 open shows panel", WaitFor(delegate { return openPanel() != null; }, 2000));
                    var panel = openPanel();
                    if (panel != null)
                    {
                        Check("W7 panel name", panel.Current.Name == "Choose a date", panel.Current.Name);
                        var title = Find(panel, "PickDate_Title");
                        Check("P5 initial title", title != null && GetValue(title) == DateTime.Today.ToString("MMMM yyyy"),
                            title == null ? "missing" : GetValue(title));
                        var todayCell = Find(panel, "PickDate_Day" + DateTime.Today.Day);
                        Check("W7 today name and marker", todayCell != null
                            && todayCell.Current.Name == DateTime.Today.ToString("MMMM d, yyyy")
                            && todayCell.Current.ItemStatus.Contains("today"),
                            todayCell == null ? "missing" : todayCell.Current.Name + " / " + todayCell.Current.ItemStatus);

                        var prev = Find(panel, "PickDate_Prev");
                        var next = Find(panel, "PickDate_Next");
                        if (prev != null && next != null)
                        {
                            Invoke(prev);
                            string before = DateTime.Today.AddMonths(-1).ToString("MMMM yyyy");
                            Check("P5 prev month", WaitFor(delegate
                            {
                                var t = Find(panel, "PickDate_Title");
                                return t != null && GetValue(t) == before;
                            }, 2000), "expected " + before);
                            Invoke(next);
                            string current = DateTime.Today.ToString("MMMM yyyy");
                            Check("P5 next month", WaitFor(delegate
                            {
                                var t = Find(panel, "PickDate_Title");
                                return t != null && GetValue(t) == current;
                            }, 2000), "expected " + current);
                        }

                        var day14 = Find(panel, "PickDate_Day14");
                        Check("P4 day cell found", day14 != null);
                        if (day14 != null)
                        {
                            Check("W7 full-date cell name", day14.Current.Name == DateTime.Today.ToString("MMMM") + " 14, " + DateTime.Today.Year,
                                day14.Current.Name);
                            Invoke(day14);
                            Check("P4 date pick closes panel", WaitFor(delegate { return openPanel() == null; }, 2000));
                            expectedDate = DateTime.Today.ToString("MMM") + " 14, " + DateTime.Today.Year;
                            Check("P3 date format", WaitFor(delegate
                            {
                                var v = Find(dateSection, "PickDate_Value");
                                return v != null && GetValue(v) == expectedDate;
                            }, 2000), "expected=[" + expectedDate + "] actual=[" + GetValue(pickValue) + "]");
                        }
                    }

                    pickToggle.Toggle();
                    Check("P7 reopened", WaitFor(delegate { return openPanel() != null; }, 2000));
                    panel = openPanel();
                    if (expectedDate.Length > 0 && panel != null)
                    {
                        // UIA reports the adorner layer as FocusedElement here, so ask the cell itself.
                        var reopenedDay = Find(panel, "PickDate_Day14");
                        Check("P2 selected cell focused on reopen", WaitFor(delegate
                        {
                            var cell = Find(root, "PickDate_Day14");
                            return cell != null && cell.Current.HasKeyboardFocus;
                        }, 2000), "hasFocus=" + (reopenedDay == null ? "missing"
                            : reopenedDay.Current.HasKeyboardFocus.ToString()));
                        Check("P11 selection survives reopen", GetValue(pickValue) == expectedDate
                            && reopenedDay != null && reopenedDay.Current.ItemStatus.Contains("selected"),
                            "value=" + GetValue(pickValue) + " status=" + (reopenedDay == null ? "missing" : reopenedDay.Current.ItemStatus));
                        var reopenedTitle = Find(panel, "PickDate_Title");
                        Check("P11 view month survives reopen", reopenedTitle != null
                            && GetValue(reopenedTitle) == DateTime.Today.ToString("MMMM yyyy"),
                            reopenedTitle == null ? "missing" : GetValue(reopenedTitle));
                        var reopenedNext = Find(panel, "PickDate_Next");
                        if (reopenedNext != null) Invoke(reopenedNext);
                        string nextTitle = DateTime.Today.AddMonths(1).ToString("MMMM yyyy");
                        Check("finding-1 Next keeps panel open", WaitFor(delegate
                        {
                            var t = Find(root, "PickDate_Title");
                            return t != null && GetValue(t) == nextTitle;
                        }, 2000) && openPanel() != null, "title=" + nextTitle);
                    }
                    var todayButton = Find(root, "PickDate_Today");
                    if (todayButton != null) Invoke(todayButton);
                    Check("P7 Today commits", WaitFor(delegate { return openPanel() == null; }, 2000)
                        && GetValue(pickValue) == DateTime.Today.ToString("MMM d, yyyy"), GetValue(pickValue));

                    pickToggle.Toggle();
                    WaitFor(delegate { return openPanel() != null; }, 2000);
                    panel = openPanel();
                    var pickClear = Find(panel, "PickDate_Clear");
                    if (pickClear != null) Invoke(pickClear);
                    Check("P8 Clear resets to placeholder", WaitFor(delegate { return openPanel() == null; }, 2000)
                        && GetValue(pickValue) == "Pick a date…", GetValue(pickValue));

                    pickToggle.Toggle();
                    WaitFor(delegate { return openPanel() != null; }, 2000);
                    Drv.Focus(pickTrigger);
                    PostKey(new IntPtr(root.Current.NativeWindowHandle), 0x1B);
                    Check("P9 Escape closes and keeps focus", WaitFor(delegate { return openPanel() == null; }, 2000)
                        && pickTrigger.Current.HasKeyboardFocus);
                    if (openPanel() != null) pickToggle.Toggle(); // never leave a panel open for other areas

                    // P10: a mouse message outside trigger and panel; focus is not moved first.
                    pickToggle.Toggle();
                    WaitFor(delegate { return openPanel() != null; }, 2000);
                    object windowRect = root.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
                    double windowX = (double)windowRect.GetType().GetProperty("X").GetValue(windowRect, null);
                    double windowY = (double)windowRect.GetType().GetProperty("Y").GetValue(windowRect, null);
                    PostMouseClick(new IntPtr(root.Current.NativeWindowHandle),
                        RectPart(dateSection, "X") + RectPart(dateSection, "Width") - 60,
                        RectPart(dateSection, "Y") + 20, windowX, windowY);
                    Check("P10 outside click closes", WaitFor(delegate { return openPanel() == null; }, 2000));
                }
            }

            // Date picker (07 P2) — month and year modes, same card.
            var monthTrigger = Find(dateSection, "PickMonth_Trigger");
            Check("P2 month trigger found", monthTrigger != null);
            if (monthTrigger != null)
            {
                var monthValue = Find(dateSection, "PickMonth_Value");
                var monthToggle = FastToggle.Of(monthTrigger);
                monthToggle.Toggle();
                Check("P2 month panel opens", WaitFor(delegate { return Find(root, "PickMonth_Panel") != null; }, 2000));
                var monthPanel = Find(root, "PickMonth_Panel");
                Check("W7 month panel name", monthPanel != null && monthPanel.Current.Name == "Choose a month",
                    monthPanel == null ? "missing" : monthPanel.Current.Name);
                var monthTitle = Find(monthPanel, "PickMonth_Title");
                Check("P5 month title", monthTitle != null && GetValue(monthTitle) == DateTime.Today.Year.ToString(),
                    monthTitle == null ? "missing" : GetValue(monthTitle));
                var jan = Find(monthPanel, "PickMonth_Month0");
                if (jan != null) Invoke(jan);
                Check("P4 month commits", WaitFor(delegate { return Find(root, "PickMonth_Panel") == null; }, 2000)
                    && monthValue != null && GetValue(monthValue) == "Jan " + DateTime.Today.Year,
                    monthValue == null ? "missing" : GetValue(monthValue));
            }
            var yearTrigger = Find(dateSection, "PickYear_Trigger");
            Check("P2 year trigger found", yearTrigger != null);
            if (yearTrigger != null)
            {
                var yearValue = Find(dateSection, "PickYear_Value");
                var yearToggle = FastToggle.Of(yearTrigger);
                yearToggle.Toggle();
                Check("P2 year panel opens", WaitFor(delegate { return Find(root, "PickYear_Panel") != null; }, 2000));
                var yearPanel = Find(root, "PickYear_Panel");
                Check("W7 year panel name", yearPanel != null && yearPanel.Current.Name == "Choose a year",
                    yearPanel == null ? "missing" : yearPanel.Current.Name);
                int y = DateTime.Today.Year;
                var yearTitle = Find(yearPanel, "PickYear_Title");
                Check("P5 year title", yearTitle != null && GetValue(yearTitle) == (y - 8) + "–" + (y + 11),
                    yearTitle == null ? "missing" : GetValue(yearTitle));
                var thisYear = Find(yearPanel, "PickYear_Year" + y);
                if (thisYear != null) Invoke(thisYear);
                Check("P4 year commits", WaitFor(delegate { return Find(root, "PickYear_Panel") == null; }, 2000)
                    && yearValue != null && GetValue(yearValue) == y.ToString(),
                    yearValue == null ? "missing" : GetValue(yearValue));
            }
        }
    }
}
