// Covered: file input, date picker (date/month/year), date & time picker (time/seconds, W6/P6), focus/tab order, outside-click behaviour and real presses (M0-M3, Drv.Click).
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
        // Bounds by reflection: the test assembly's Rect exposes X/Y/Width/Height only (see Tests.Core CellLeft).
        private static double PickersBox(AutomationElement el, string name)
        {
            object rect = el.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
            return (double)rect.GetType().GetProperty(name).GetValue(rect, null);
        }

        private static bool PickersBoxHas(AutomationElement el, double x, double y)
        {
            double left = PickersBox(el, "X"), top = PickersBox(el, "Y");
            return x >= left && x < left + PickersBox(el, "Width") && y >= top && y < top + PickersBox(el, "Height");
        }

        static partial void RunPickers(AutomationElement root, ref bool ran)
        {
            ran = true;
            // File input (07 P4) — no seeding hook, so only the idle structure is assertable.
            var fileCard = Find(root, "FileCard");
            Check("file input card found", fileCard != null);
            if (fileCard != null)
            {
                var singleDrop = Find(fileCard, "FileSingle_Drop");
                var multiDrop = Find(fileCard, "FileMulti_Drop");
                Check("F1 drop zones found", singleDrop != null && multiDrop != null
                    && singleDrop.Current.Name == "Drop a file here" && multiDrop.Current.Name == "Drop images here");
                Check("F1 drop zones focusable", singleDrop != null && multiDrop != null
                    && singleDrop.Current.IsKeyboardFocusable && multiDrop.Current.IsKeyboardFocusable);
                Check("F5 count hidden at zero files", Find(fileCard, "FileSingle_Count") == null
                    && Find(fileCard, "FileMulti_Count") == null);
            }

            // Date picker (07 P1/P2) — find the section once, then search inside the cached scopes.
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
                        // Keyboard reachability: every control in the panel is a tab stop.
                        var prev = Find(panel, "PickDate_Prev");
                        var next = Find(panel, "PickDate_Next");
                        var clearBtn = Find(panel, "PickDate_Clear");
                        var todayBtn = Find(panel, "PickDate_Today");
                        Check("tab stops present", prev != null && next != null && clearBtn != null && todayBtn != null
                            && todayCell != null && prev.Current.IsKeyboardFocusable && next.Current.IsKeyboardFocusable
                            && clearBtn.Current.IsKeyboardFocusable && todayBtn.Current.IsKeyboardFocusable
                            && todayCell.Current.IsKeyboardFocusable);

                        // Tab from the open trigger must land on Previous, not leave the picker.
                        Drv.Focus(pickTrigger);
                        PostKey(new IntPtr(root.Current.NativeWindowHandle), 0x09);
                        Check("P12 Tab from trigger reaches Prev", WaitFor(delegate
                        {
                            var p = Find(root, "PickDate_Prev");
                            return p != null && p.Current.HasKeyboardFocus;
                        }, 2000) && openPanel() != null);

                        if (prev != null && next != null)
                        {
                            // Tab from Prev must reach Next (the panel KeyDown must not redirect).
                            Drv.Focus(prev);
                            PostKey(new IntPtr(root.Current.NativeWindowHandle), 0x09);
                            bool reached = WaitFor(delegate
                            {
                                var n = Find(root, "PickDate_Next");
                                return n != null && n.Current.HasKeyboardFocus;
                            }, 2000) && openPanel() != null;
                            Check("tab order Prev -> Next", reached, "prevBefore=" + prev.Current.HasKeyboardFocus
                                + " nextAfter=" + next.Current.HasKeyboardFocus + " panel=" + (openPanel() != null));

                            // Focus is on Prev; re-rendering the body must not pull focus out.
                            Drv.Focus(prev);
                            Invoke(prev);
                            string before = DateTime.Today.AddMonths(-1).ToString("MMMM yyyy");
                            Check("P5 prev month + focus kept", WaitFor(delegate
                            {
                                var t = Find(root, "PickDate_Title");
                                var p = Find(root, "PickDate_Prev");
                                return t != null && GetValue(t) == before && p != null && p.Current.HasKeyboardFocus;
                            }, 2000) && openPanel() != null, "expected " + before);
                            Invoke(next);
                            string current = DateTime.Today.ToString("MMMM yyyy");
                            Check("P5 next month", WaitFor(delegate
                            {
                                var t = Find(root, "PickDate_Title");
                                return t != null && GetValue(t) == current;
                            }, 2000), "expected " + current);
                        }

                        var day14 = Find(panel, "PickDate_Day14");
                        Check("P4 day cell found", day14 != null);
                        if (day14 != null)
                        {
                            Check("W7 full-date cell name", day14.Current.Name == DateTime.Today.ToString("MMMM") + " 14, " + DateTime.Today.Year,
                                day14.Current.Name);
                            // Invoke, not a press: a real press on a day cell is M2 below.
                            Invoke(day14);
                            bool committed = WaitFor(delegate
                            {
                                var v = Find(dateSection, "PickDate_Value");
                                return v != null && GetValue(v) == DateTime.Today.ToString("MMM") + " 14, " + DateTime.Today.Year;
                            }, 2000) && openPanel() == null;
                            Check("P4 day pick commits value", committed,
                                "value=" + GetValue(pickValue) + " panel=" + (openPanel() != null));
                        }
                    }

                    // P11 non-vacuous: navigate away, pick in the new month, reopen and expect that month.
                    pickToggle.Toggle();
                    Check("P7 reopened", WaitFor(delegate { return openPanel() != null; }, 2000));
                    panel = openPanel();
                    if (panel != null)
                    {
                        var reopenedDay = Find(panel, "PickDate_Day14");
                        Check("P2 selected cell focused on reopen", WaitFor(delegate
                        {
                            var cell = Find(root, "PickDate_Day14");
                            return cell != null && cell.Current.HasKeyboardFocus;
                        }, 2000), "hasFocus=" + (reopenedDay == null ? "missing" : reopenedDay.Current.HasKeyboardFocus.ToString()));
                        Check("P11 selection survives reopen", reopenedDay != null
                            && reopenedDay.Current.ItemStatus.Contains("selected"));

                        var next2 = Find(panel, "PickDate_Next");
                        if (next2 != null) Invoke(next2);
                        string moved = DateTime.Today.AddMonths(1).ToString("MMMM yyyy");
                        Check("finding-1 Next keeps panel open", WaitFor(delegate
                        {
                            var t = Find(root, "PickDate_Title");
                            return t != null && GetValue(t) == moved;
                        }, 2000) && openPanel() != null, "title=" + moved);
                        var otherDay = Find(root, "PickDate_Day14");
                        if (otherDay != null) Invoke(otherDay);
                        pickToggle.Toggle();
                        Check("P11 view survives reopen", WaitFor(delegate { return openPanel() != null; }, 2000));
                        panel = openPanel();
                        // Guarded so a closed panel FAILs these checks instead of aborting the area (NullReferenceException).
                        var movedTitle = panel == null ? null : Find(panel, "PickDate_Title");
                        Check("P11 navigated month not reset on reopen", movedTitle != null && GetValue(movedTitle) == moved,
                            movedTitle == null ? "missing" : GetValue(movedTitle));
                        var movedDay = panel == null ? null : Find(panel, "PickDate_Day14");
                        Check("selected marker after moved pick", movedDay != null
                            && movedDay.Current.ItemStatus.Contains("selected"),
                            movedDay == null ? "missing" : movedDay.Current.ItemStatus);
                    }

                    var todayButton = Find(root, "PickDate_Today");
                    if (todayButton != null) Invoke(todayButton);
                    Check("P7 Today commits", WaitFor(delegate { return openPanel() == null; }, 2000)
                        && GetValue(pickValue) == DateTime.Today.ToString("MMM d, yyyy"), GetValue(pickValue));

                    pickToggle.Toggle();
                    WaitFor(delegate { return openPanel() != null; }, 2000);
                    var pickClear = Find(root, "PickDate_Clear");
                    if (pickClear != null) Invoke(pickClear);
                    Check("P8 Clear resets to placeholder", WaitFor(delegate { return openPanel() == null; }, 2000)
                        && GetValue(pickValue) == "Pick a date…", GetValue(pickValue));

                    pickToggle.Toggle();
                    WaitFor(delegate { return openPanel() != null; }, 2000);
                    Drv.Focus(pickTrigger);
                    PostKey(new IntPtr(root.Current.NativeWindowHandle), 0x1B);
                    Check("P9 Escape closes and keeps focus", WaitFor(delegate { return openPanel() == null; }, 2000)
                        && pickTrigger.Current.HasKeyboardFocus);
                    if (openPanel() != null) pickToggle.Toggle();

                    // P10 self-contained: focus an outside control with no help from other areas.
                    // Focus-based: exercises the outside-root close path (focusout) without a press.
                    // The real-press variant is M3 below.
                    pickToggle.Toggle();
                    Check("P10 setup panel open", WaitFor(delegate { return openPanel() != null; }, 2000));
                    var outside = Find(root, "FileSingle_Drop");
                    if (outside != null) Drv.Focus(outside);
                    Check("P10 outside interaction closes", outside != null
                        && WaitFor(delegate { return openPanel() == null; }, 2000)
                        && outside.Current.HasKeyboardFocus);
                }
            }

            // Date picker (07 P2) — month and year modes, same card.
            var monthTrigger = Find(dateSection, "PickMonth_Trigger");
            Check("P2 month trigger found", monthTrigger != null);
            if (monthTrigger != null)
            {
                var monthValue = Find(dateSection, "PickMonth_Value");
                var monthToggle = FastToggle.Of(monthTrigger);
                Func<AutomationElement> monthPanelNow = delegate { return Find(root, "PickMonth_Panel"); };
                monthToggle.Toggle();
                Check("P2 month panel opens", WaitFor(delegate { return monthPanelNow() != null; }, 2000));
                var monthPanel = monthPanelNow();
                if (monthPanel != null)
                {
                    Check("W7 month panel name", monthPanel.Current.Name == "Choose a month", monthPanel.Current.Name);
                    var monthTitle = Find(monthPanel, "PickMonth_Title");
                    Check("P5 month title", monthTitle != null && GetValue(monthTitle) == DateTime.Today.Year.ToString(),
                        monthTitle == null ? "missing" : GetValue(monthTitle));
                    var monthNext = Find(monthPanel, "PickMonth_Next");
                    if (monthNext != null) Invoke(monthNext);
                    Check("P5 month Next is +1 year", WaitFor(delegate
                    {
                        var t = Find(root, "PickMonth_Title");
                        return t != null && GetValue(t) == (DateTime.Today.Year + 1).ToString();
                    }, 2000) && monthPanelNow() != null);
                    var monthPrev = Find(root, "PickMonth_Prev");
                    if (monthPrev != null) Invoke(monthPrev);
                    Check("P5 month Prev is -1 year", WaitFor(delegate
                    {
                        var t = Find(root, "PickMonth_Title");
                        return t != null && GetValue(t) == DateTime.Today.Year.ToString();
                    }, 2000));
                    var jan = Find(root, "PickMonth_Month0");
                    Check("month cell Name is full month", jan != null && jan.Current.Name == "January",
                        jan == null ? "missing" : jan.Current.Name);
                    if (jan != null) Invoke(jan);
                    Check("P4 month commits", WaitFor(delegate { return monthPanelNow() == null; }, 2000)
                        && GetValue(monthValue) == "Jan " + DateTime.Today.Year, GetValue(monthValue));
                    // Today stores today's month/day; reopening must still mark the selected month.
                    monthToggle.Toggle();
                    WaitFor(delegate { return monthPanelNow() != null; }, 2000);
                    var monthToday = Find(root, "PickMonth_Today");
                    if (monthToday != null) Invoke(monthToday);
                    Check("month Today commits", WaitFor(delegate { return monthPanelNow() == null; }, 2000)
                        && GetValue(monthValue) == DateTime.Today.ToString("MMM yyyy"), GetValue(monthValue));
                    monthToggle.Toggle();
                    Check("P2 month reopened", WaitFor(delegate { return monthPanelNow() != null; }, 2000));
                    var pickedMonth = Find(root, "PickMonth_Month" + (DateTime.Today.Month - 1));
                    Check("month selected marker on reopen", pickedMonth != null
                        && WaitFor(delegate
                        {
                            var c = Find(root, "PickMonth_Month" + (DateTime.Today.Month - 1));
                            return c != null && c.Current.ItemStatus.Contains("selected") && c.Current.HasKeyboardFocus;
                        }, 2000));
                    var monthClear = Find(root, "PickMonth_Clear");
                    if (monthClear != null) Invoke(monthClear);
                    Check("month Clear resets", WaitFor(delegate { return monthPanelNow() == null; }, 2000)
                        && GetValue(monthValue) == "Pick a month…", GetValue(monthValue));
                }
            }
            var yearTrigger = Find(dateSection, "PickYear_Trigger");
            Check("P2 year trigger found", yearTrigger != null);
            if (yearTrigger != null)
            {
                var yearValue = Find(dateSection, "PickYear_Value");
                var yearToggle = FastToggle.Of(yearTrigger);
                Func<AutomationElement> yearPanelNow = delegate { return Find(root, "PickYear_Panel"); };
                yearToggle.Toggle();
                Check("P2 year panel opens", WaitFor(delegate { return yearPanelNow() != null; }, 2000));
                var yearPanel = yearPanelNow();
                if (yearPanel != null)
                {
                    Check("W7 year panel name", yearPanel.Current.Name == "Choose a year", yearPanel.Current.Name);
                    int y = DateTime.Today.Year;
                    var yearTitle = Find(yearPanel, "PickYear_Title");
                    Check("P5 year title", yearTitle != null && GetValue(yearTitle) == (y - 8) + "–" + (y + 11),
                        yearTitle == null ? "missing" : GetValue(yearTitle));
                    var yearNext = Find(yearPanel, "PickYear_Next");
                    if (yearNext != null) Invoke(yearNext);
                    Check("P5 year Next is +20", WaitFor(delegate
                    {
                        var t = Find(root, "PickYear_Title");
                        return t != null && GetValue(t) == (y + 12) + "–" + (y + 31);
                    }, 2000) && yearPanelNow() != null);
                    var yearPrev = Find(root, "PickYear_Prev");
                    if (yearPrev != null) Invoke(yearPrev);
                    Check("P5 year Prev is -20", WaitFor(delegate
                    {
                        var t = Find(root, "PickYear_Title");
                        return t != null && GetValue(t) == (y - 8) + "–" + (y + 11);
                    }, 2000));
                    var thisYear = Find(root, "PickYear_Year" + y);
                    if (thisYear != null) Invoke(thisYear);
                    Check("P4 year commits", WaitFor(delegate { return yearPanelNow() == null; }, 2000)
                        && GetValue(yearValue) == y.ToString(), GetValue(yearValue));
                    yearToggle.Toggle();
                    WaitFor(delegate { return yearPanelNow() != null; }, 2000);
                    var yearToday = Find(root, "PickYear_Today");
                    if (yearToday != null) Invoke(yearToday);
                    Check("year Today commits", WaitFor(delegate { return yearPanelNow() == null; }, 2000)
                        && GetValue(yearValue) == y.ToString(), GetValue(yearValue));
                    yearToggle.Toggle();
                    Check("P2 year reopened", WaitFor(delegate { return yearPanelNow() != null; }, 2000));
                    Check("year selected marker on reopen", WaitFor(delegate
                    {
                        var c = Find(root, "PickYear_Year" + y);
                        return c != null && c.Current.ItemStatus.Contains("selected") && c.Current.HasKeyboardFocus;
                    }, 2000));
                    var yearClear = Find(root, "PickYear_Clear");
                    if (yearClear != null) Invoke(yearClear);
                    Check("year Clear resets", WaitFor(delegate { return yearPanelNow() == null; }, 2000)
                        && GetValue(yearValue) == "Pick a year…", GetValue(yearValue));
                }
            }

            // Date & time picker (07 P3) — time row, uncommitted pick, Apply clamp, Clear (W6).
            var dtSection = Find(root, "DateTimeCard");
            Check("P3 date-time section found", dtSection != null);
            if (dtSection != null)
            {
                var dtTrigger = Find(dtSection, "PickDateTime_Trigger");
                var dtValue = Find(dtSection, "PickDateTime_Value");
                Check("P3 date-time trigger found", dtTrigger != null && dtValue != null);
                if (dtTrigger != null)
                {
                    var dtToggle = FastToggle.Of(dtTrigger);
                    Func<AutomationElement> dtPanel = delegate { return Find(root, "PickDateTime_Panel"); };
                    dtToggle.Toggle();
                    Check("P3 time panel opens", WaitFor(delegate { return dtPanel() != null; }, 2000));
                    var timePanel = dtPanel();
                    if (timePanel != null)
                    {
                        Check("W7 time panel name", timePanel.Current.Name == "Choose a date and time", timePanel.Current.Name);
                        var hour = Find(timePanel, "PickDateTime_Hour");
                        var minute = Find(timePanel, "PickDateTime_Minute");
                        Check("P3 time inputs", hour != null && minute != null && GetValue(hour) == "09" && GetValue(minute) == "30");
                        Check("P3 no Second input", Find(timePanel, "PickDateTime_Second") == null);
                        Drv.Focus(hour);
                        PostKey(new IntPtr(root.Current.NativeWindowHandle), 0x09);
                        Check("tab order Hour -> Minute", WaitFor(delegate
                        {
                            var m = Find(root, "PickDateTime_Minute");
                            return m != null && m.Current.HasKeyboardFocus;
                        }, 2000) && dtPanel() != null);
                        var dtDay = Find(timePanel, "PickDateTime_Day14");
                        if (dtDay != null) Invoke(dtDay);
                        Check("P4 time pick stays open and uncommitted", dtPanel() != null
                            && GetValue(dtValue) == "Pick a date and time…", GetValue(dtValue));

                        // W6/P11: Escape keeps the uncommitted selection and the placeholder.
                        Drv.Focus(dtTrigger);
                        PostKey(new IntPtr(root.Current.NativeWindowHandle), 0x1B);
                        Check("P11 Escape closes time picker", WaitFor(delegate { return dtPanel() == null; }, 2000));
                        dtToggle.Toggle();
                        WaitFor(delegate { return dtPanel() != null; }, 2000);
                        var pendingDay = Find(root, "PickDateTime_Day14");
                        Check("P11 uncommitted selection survives reopen", pendingDay != null
                            && pendingDay.Current.ItemStatus.Contains("selected")
                            && GetValue(dtValue) == "Pick a date and time…",
                            pendingDay == null ? "missing" : pendingDay.Current.ItemStatus);
                        SetValue(Find(root, "PickDateTime_Hour"), "30");
                        SetValue(Find(root, "PickDateTime_Minute"), "5");
                        var pendingApply = Find(root, "PickDateTime_Apply");
                        if (pendingApply != null) Invoke(pendingApply);
                        Check("P6 hour clamps to 23", WaitFor(delegate { return dtPanel() == null; }, 2000)
                            && GetValue(dtValue).EndsWith(", 23:05"), GetValue(dtValue));
                        dtToggle.Toggle();
                        WaitFor(delegate { return dtPanel() != null; }, 2000);
                        timePanel = dtPanel();
                        SetValue(Find(timePanel, "PickDateTime_Hour"), "9");
                        SetValue(Find(timePanel, "PickDateTime_Minute"), "5");
                        var apply = Find(timePanel, "PickDateTime_Apply");
                        if (apply != null) Invoke(apply);
                        Check("P6 Apply commits with zero padding", WaitFor(delegate { return dtPanel() == null; }, 2000)
                            && GetValue(dtValue) == DateTime.Today.ToString("MMM") + " 14, " + DateTime.Today.Year + ", 09:05",
                            GetValue(dtValue));
                    }

                    // W6: Clear keeps the time inputs; reopen shows the same hour.
                    dtToggle.Toggle();
                    WaitFor(delegate { return dtPanel() != null; }, 2000);
                    var hour2 = Find(root, "PickDateTime_Hour");
                    SetValue(hour2, "12");
                    var clear2 = Find(root, "PickDateTime_Clear");
                    if (clear2 != null) Invoke(clear2);
                    Check("P8 Clear closes time picker", WaitFor(delegate { return dtPanel() == null; }, 2000)
                        && GetValue(dtValue) == "Pick a date and time…", GetValue(dtValue));
                    dtToggle.Toggle();
                    WaitFor(delegate { return dtPanel() != null; }, 2000);
                    Check("W6 time inputs survive Clear", GetValue(Find(root, "PickDateTime_Hour")) == "12",
                        GetValue(Find(root, "PickDateTime_Hour")));
                    var day2 = Find(root, "PickDateTime_Day14");
                    if (day2 != null) Invoke(day2);
                    SetValue(Find(root, "PickDateTime_Minute"), "99");
                    var apply2 = Find(root, "PickDateTime_Apply");
                    if (apply2 != null) Invoke(apply2);
                    Check("P6 Apply clamps minutes", WaitFor(delegate { return dtPanel() == null; }, 2000)
                        && GetValue(dtValue).EndsWith("12:59"), GetValue(dtValue));
                }

                var secTrigger = Find(dtSection, "PickDateTimeSec_Trigger");
                if (secTrigger != null)
                {
                    var secToggle = FastToggle.Of(secTrigger);
                    secToggle.Toggle();
                    Check("P3 seconds panel opens", WaitFor(delegate { return Find(root, "PickDateTimeSec_Panel") != null; }, 2000));
                    Check("P3 Second input", GetValue(Find(root, "PickDateTimeSec_Second")) == "00");
                    var secDay = Find(root, "PickDateTimeSec_Day14");
                    if (secDay != null) Invoke(secDay);
                    Check("P4 seconds pick stays open", Find(root, "PickDateTimeSec_Panel") != null);
                    var secApply = Find(root, "PickDateTimeSec_Apply");
                    if (secApply != null) Invoke(secApply);
                    Check("P6 seconds format", WaitFor(delegate { return Find(root, "PickDateTimeSec_Panel") == null; }, 2000)
                        && GetValue(Find(dtSection, "PickDateTimeSec_Value")).EndsWith(", 09:30:00"),
                        GetValue(Find(dtSection, "PickDateTimeSec_Value")));
                }
                var dateOnly = Find(dtSection, "PickDateOnly_Trigger");
                Check("W4 date-only trigger found", dateOnly != null);
                var month2 = Find(dtSection, "PickMonth2_Trigger");
                var year2 = Find(dtSection, "PickYear2_Trigger");
                Check("P3 month/year date-only triggers found", month2 != null && year2 != null);
            }

            // Real presses (Drv.Click / Drv.ClickAt): the app hit-tests the point and routes the press like an OS press
            // (App.xaml.cs ClickScreenPoint). Driver-only: UITEST_NODRIVE has no press path, so these print CODE-ONLY.
            var pressSection = Find(root, "DateTimeCard");
            if (!Drv.Enabled) Console.WriteLine("CODE-ONLY  real-press picker checks M0-M3 (UITEST_NODRIVE: no driver press)");
            else if (pressSection != null)
            {
                var pressTrigger = Find(pressSection, "PickDateTime_Trigger");
                Check("M0 time trigger found", pressTrigger != null);
                if (pressTrigger != null)
                {
                    var pressToggle = FastToggle.Of(pressTrigger);
                    Func<AutomationElement> pressPanel = delegate { return Find(root, "PickDateTime_Panel"); };
                    // Opens the time panel with the driver toggle (not a press) when an earlier check left it closed.
                    Func<bool> openTimePanel = delegate
                    {
                        if (pressPanel() == null) pressToggle.Toggle();
                        return WaitFor(delegate { return pressPanel() != null; }, 2000);
                    };

                    // M1 blank panel space: the centre of the 8px gap between the header row and the body row holds no
                    // focusable control, and a press there keeps the panel open.
                    if (openTimePanel())
                    {
                        var mp = pressPanel();
                        var headPrev = Find(mp, "PickDateTime_Prev");
                        var headNext = Find(mp, "PickDateTime_Next");
                        Check("M1 header buttons found", headPrev != null && headNext != null);
                        if (headPrev != null && headNext != null)
                        {
                            double headBottom = Math.Max(PickersBox(headPrev, "Y") + PickersBox(headPrev, "Height"),
                                PickersBox(headNext, "Y") + PickersBox(headNext, "Height"));
                            double gapY = headBottom + 4;
                            double gapX = PickersBox(mp, "X") + PickersBox(mp, "Width") / 2;
                            bool blank = PickersBoxHas(mp, gapX, gapY);
                            foreach (AutomationElement d in mp.FindAll(TreeScope.Descendants, Condition.TrueCondition))
                            {
                                if (d.Current.AutomationId == "PickDateTime_Panel") continue;
                                if (d.Current.IsKeyboardFocusable && PickersBoxHas(d, gapX, gapY)) blank = false;
                            }
                            Check("M1 gap point is blank panel space", blank);
                            int press = Drv.ClickAt(mp, PickersBox(mp, "Width") / 2, gapY - PickersBox(mp, "Y"));
                            bool closed = WaitFor(delegate { return pressPanel() == null; }, 400);
                            Check("M1 press in blank panel space keeps it open", (press == 1 || press == 4) && !closed && pressPanel() != null,
                                "press=" + press + " (1 element hit, 4 fell through to the window)");
                        }
                    }

                    // M2 a press on a day cell lands on the cell and keeps the panel open. A synthetic press does not
                    // raise Button.Click (ButtonBase needs real IsMouseOver), so the day's selection is checked by Invoke
                    // in P4/P11 above, not here.
                    if (openTimePanel())
                    {
                        var mDay = Find(pressPanel(), "PickDateTime_Day21");
                        Check("M2 day cell found", mDay != null);
                        if (mDay != null)
                        {
                            int press = Drv.ClickAt(mDay, PickersBox(mDay, "Width") / 2, PickersBox(mDay, "Height") / 2);
                            bool closed = WaitFor(delegate { return pressPanel() == null; }, 400);
                            Check("M2 press on a day cell lands on it and keeps the panel open", press == 1 && !closed && pressPanel() != null,
                                "press=" + press + " open=" + (pressPanel() != null));
                        }
                    }

                    // M3 a real press outside the panel closes it: the window-level outside-click hook must see the press.
                    if (openTimePanel())
                    {
                        int press = Drv.ClickAt(pressSection, 4, 4);
                        bool outClosed = WaitFor(delegate { return pressPanel() == null; }, 2000);
                        Check("M3 real press outside closes the time panel", (press == 1 || press == 4) && outClosed,
                            "press=" + press + " closed=" + outClosed);
                    }
                    if (pressPanel() != null) pressToggle.Toggle();
                }
            }
        }
    }
}
