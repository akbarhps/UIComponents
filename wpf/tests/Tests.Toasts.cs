// Covered: toasts T0-T3: global ToastLayer/Show, W10 auto-dismiss/pause/focus-restore,
// variants (dismissible/action/persistent), F1-F3 call sites (grid copy, upload complete, bulk bar).
// T3 notes: 46 assertions, ~43 s driver-on / ~72 s driver-off (-Only toasts); toast waits are real 3.6 s timers.
// Stack uses 5/5 per toast + stack bottom 19, equivalent to the plan's Margin 0,0,24,24 with 10px gaps.
// Accepted difference: close/action buttons use FocusVisualStyle=null like every other ported button,
// so no web-style :focus-visible ring; focus is still detectable (HasKeyboardFocus) and pauses the timer.
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
        static partial void RunToasts(AutomationElement root, ref bool ran)
        {
            ran = true;
            // The window UIA rect includes the 8px frame; the layer inset is the spec's 24px.
            const double windowFrame = 8;

            Func<AutomationElement, string, double> ToastsRect = delegate(AutomationElement el, string part)
            {
                try
                {
                    object rect = el.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
                    return (double)rect.GetType().GetProperty(part).GetValue(rect, null);
                }
                catch (ElementNotAvailableException) { return 0; }
            };
            // Name-scoped lookup: never walk the whole tree from the window root for a toast.
            Func<string, AutomationElement> ToastsFind = delegate(string toastName)
            {
                var matches = root.FindAll(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.NameProperty, toastName));
                foreach (AutomationElement e in matches)
                {
                    try
                    {
                        string id = e.Current.AutomationId;
                        if (id != null && id.StartsWith("Toast")) return e;
                    }
                    catch (ElementNotAvailableException) { }
                }
                return null;
            };
            Func<List<AutomationElement>> ToastsCollect = delegate()
            {
                var toastList = new List<AutomationElement>();
                var groups = root.FindAll(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Group));
                foreach (AutomationElement e in groups)
                {
                    try { string id = e.Current.AutomationId; if (id != null && id.StartsWith("Toast")) toastList.Add(e); }
                    catch (ElementNotAvailableException) { }
                }
                return toastList;
            };
            Func<AutomationElement, int> ToastsNumber = delegate(AutomationElement el)
            {
                string id = el.Current.AutomationId;
                if (id == null || !id.StartsWith("Toast")) return 0;
                int i = 5;
                while (i < id.Length && id[i] >= '0' && id[i] <= '9') i++;
                int n;
                return i > 5 && int.TryParse(id.Substring(5, i - 5), out n) ? n : 0;
            };
            Func<string, int, bool> ToastsWaitGone = delegate(string toastName, int maxMs)
            {
                return WaitFor(delegate { return ToastsFind(toastName) == null; }, maxMs);
            };
            Action<int> ToastsWaitMs = delegate(int ms)
            {
                int start = Environment.TickCount;
                while (Environment.TickCount - start < ms) Thread.Sleep(50);
            };
            double windowRight = ToastsRect(root, "Left") + ToastsRect(root, "Width");
            double windowBottom = ToastsRect(root, "Top") + ToastsRect(root, "Height");

            // T1: three plain toasts, one at a time. Invoke is async: WaitFor before the first read.
            var bSuccess = Find(root, "ToastSuccess");
            var bInfo = Find(root, "ToastInfo");
            var bError = Find(root, "ToastError");
            Check("toast success button found", bSuccess != null);
            Check("toast info button found", bInfo != null);
            Check("toast error button found", bError != null);
            if (bSuccess != null)
            {
                Invoke(bSuccess);
                int shownAt = Environment.TickCount;
                WaitFor(delegate { return ToastsFind("Saved") != null; }, 1000);
                var saved = ToastsFind("Saved");
                Check("success toast shown", saved != null);
                if (saved != null)
                {
                    Check("toast id numbered", ToastsNumber(saved) > 0, "id=" + saved.Current.AutomationId);
                    Check("toast width 360", Math.Abs(ToastsRect(saved, "Width") - 360) <= 1,
                        "w=" + ToastsRect(saved, "Width").ToString("0.0"));
                    Check("toast height 73.5", Math.Abs(ToastsRect(saved, "Height") - 73.5) <= 1,
                        "h=" + ToastsRect(saved, "Height").ToString("0.0"));
                    Check("toast right inset 24",
                        Math.Abs(windowRight - ToastsRect(saved, "Left") - ToastsRect(saved, "Width") - (24 + windowFrame)) <= 1,
                        "gap=" + (windowRight - ToastsRect(saved, "Left") - ToastsRect(saved, "Width")).ToString("0.0"));
                }
                Check("plain toast auto-dismisses by 4.5s", ToastsWaitGone("Saved", 4600 - (Environment.TickCount - shownAt)));
            }
            if (bInfo != null)
            {
                Invoke(bInfo);
                int infoShownAt = Environment.TickCount;
                WaitFor(delegate { return ToastsFind("Heads up") != null; }, 1000);
                var heads = ToastsFind("Heads up");
                Check("info toast shown", heads != null);
                int to3s = 3000 - (Environment.TickCount - infoShownAt);
                if (to3s > 0) ToastsWaitMs(to3s);
                // Cached peer read: a fresh tree walk costs ~600ms and pushes past the 3.6s expiry.
                try { Check("plain toast still present at 3.0s", heads != null && heads.Current.Name == "Heads up"); }
                catch (ElementNotAvailableException) { Check("plain toast still present at 3.0s", false, "peer gone"); }
                Check("info toast auto-dismisses by 4.5s", ToastsWaitGone("Heads up", 4600 - (Environment.TickCount - infoShownAt)));
            }
            if (bError != null)
            {
                Invoke(bError);
                WaitFor(delegate { return ToastsFind("Upload failed") != null; }, 1000);
                var failed = ToastsFind("Upload failed");
                Check("error toast shown", failed != null);
                if (failed != null)
                    Check("toast bottom inset 24",
                        Math.Abs(windowBottom - ToastsRect(failed, "Top") - ToastsRect(failed, "Height") - (24 + windowFrame)) <= 1,
                        "gap=" + (windowBottom - ToastsRect(failed, "Top") - ToastsRect(failed, "Height")).ToString("0.0"));
                Check("error toast auto-dismisses by 4.5s", ToastsWaitGone("Upload failed", 4600));
            }

            // T2: variants. Persistent (never expires) is used for close geometry and focus restore.
            var bDismiss = Find(root, "ToastDismissible");
            var bAction = Find(root, "ToastWithAction");
            var bPersist = Find(root, "ToastPersistent");
            Check("dismissible button found", bDismiss != null);
            Check("with-action button found", bAction != null);
            Check("persistent button found", bPersist != null);
            if (bPersist != null)
            {
                Invoke(bPersist);
                WaitFor(delegate { return ToastsFind("Connection lost") != null; }, 1000);
                var pers = ToastsFind("Connection lost");
                Check("persistent toast shown", pers != null);
                if (pers != null)
                {
                    Check("persistent height 97.5", Math.Abs(ToastsRect(pers, "Height") - 97.5) <= 1,
                        "h=" + ToastsRect(pers, "Height").ToString("0.0"));
                    var close = Find(root, pers.Current.AutomationId + "_Close");
                    Check("persistent close found", close != null);
                    if (close != null)
                    {
                        double cx = ToastsRect(close, "Left") - ToastsRect(pers, "Left");
                        double cy = ToastsRect(close, "Top") - ToastsRect(pers, "Top");
                        Check("close at (325,15)", Math.Abs(cx - 325) <= 1 && Math.Abs(cy - 15) <= 1,
                            "rel=" + cx.ToString("0.0") + "," + cy.ToString("0.0"));
                        ToastsWaitMs(4500);
                        Check("persistent toast survives 4.5s", ToastsFind("Connection lost") != null);
                        Invoke(close);
                        Check("_Close removes toast", ToastsWaitGone("Connection lost", 1000));
                    }
                }
            }
            if (bAction != null)
            {
                Invoke(bAction);
                WaitFor(delegate { return ToastsFind("Message sent") != null; }, 1000);
                var act = ToastsFind("Message sent");
                Check("action toast shown", act != null);
                if (act != null)
                {
                    Check("action height 100.5", Math.Abs(ToastsRect(act, "Height") - 100.5) <= 1,
                        "h=" + ToastsRect(act, "Height").ToString("0.0"));
                    var title = act.FindFirst(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.NameProperty, "Message sent"));
                    Check("body width 262", title != null && Math.Abs(ToastsRect(title, "Width") - 262) <= 1,
                        "w=" + (title == null ? "-" : ToastsRect(title, "Width").ToString("0.0")));
                    var action = Find(root, act.Current.AutomationId + "_Action");
                    Check("action button found", action != null);
                    if (action != null)
                    {
                        double ax = ToastsRect(action, "Left") - ToastsRect(act, "Left");
                        double ay = ToastsRect(action, "Top") - ToastsRect(act, "Top");
                        Check("action at (52,64.5)", Math.Abs(ax - 52) <= 1 && Math.Abs(ay - 64.5) <= 1,
                            "rel=" + ax.ToString("0.0") + "," + ay.ToString("0.0"));
                        ToastsWaitMs(4500);
                        Check("action toast survives 4.5s (W10)", ToastsFind("Message sent") != null);
                        Invoke(action);
                        Check("_Action raises Done toast", WaitFor(delegate { return ToastsFind("Done") != null; }, 1000));
                        Check("_Action removes original toast", ToastsWaitGone("Message sent", 1000));
                    }
                }
            }
            if (bDismiss != null)
            {
                Invoke(bDismiss);
                WaitFor(delegate { return ToastsFind("New update available") != null; }, 1000);
                var disp = ToastsFind("New update available");
                Check("dismissible toast shown", disp != null);
                if (disp != null)
                    Check("dismissible height 73.5", Math.Abs(ToastsRect(disp, "Height") - 73.5) <= 1,
                        "h=" + ToastsRect(disp, "Height").ToString("0.0"));
                Check("dismissible auto-dismisses by 4.5s", ToastsWaitGone("New update available", 4600));
            }

            // Focus pauses the timer; leaving resumes it with the REMAINING time (web 3465).
            if (bDismiss != null)
            {
                Invoke(bDismiss);
                int shownAt = Environment.TickCount;
                WaitFor(delegate { return ToastsFind("New update available") != null; }, 1000);
                var t = ToastsFind("New update available");
                var closeMatches = t == null ? null : t.FindAll(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.NameProperty, "Dismiss"));
                var c = closeMatches != null && closeMatches.Count > 0 ? closeMatches[0] : null;
                Check("focus pause toast shown", t != null && c != null);
                if (c != null)
                {
                    SetForegroundWindow(new IntPtr(root.Current.NativeWindowHandle));
                    Drv.Focus(c);
                    int focusAt = Environment.TickCount;
                    Check("close button takes keyboard focus", c.Current.HasKeyboardFocus,
                        "hasFocus=" + c.Current.HasKeyboardFocus);
                    if (c.Current.HasKeyboardFocus)
                    {
                        int remaining = 3600 - (focusAt - shownAt);
                        // Past the ORIGINAL expiry: this can only pass if focus stopped the timer.
                        ToastsWaitMs(Math.Max(0, focusAt + remaining + 600 - Environment.TickCount));
                        Check("keyboard focus pauses auto-dismiss", ToastsFind("New update available") != null);
                        Drv.Focus(bDismiss);
                        int leftAt = Environment.TickCount;
                        // Lower bound: the restarted timer still has most of its remaining time.
                        // Live tree lookup, not a cached peer name: a removed toast must fail this.
                        // Allow ~600ms for the lookup, so the deadline is remaining - 1000.
                        string focusToastId = t.Current.AutomationId;
                        ToastsWaitMs(Math.Max(0, leftAt + remaining - 1000 - Environment.TickCount));
                        Check("resume: still present before remaining expires", Find(root, focusToastId) != null,
                            "remaining=" + remaining + " sinceLeft=" + (Environment.TickCount - leftAt));
                        Check("auto-dismiss resumes when focus leaves",
                            ToastsWaitGone("New update available", remaining + 800 - (Environment.TickCount - leftAt)));
                    }
                }
            }

            // Focus restore: two PERSISTENT toasts (they never expire), new ids only.
            if (bPersist != null)
            {
                int highestBefore = 0;
                foreach (AutomationElement e in ToastsCollect())
                    highestBefore = Math.Max(highestBefore, ToastsNumber(e));
                Invoke(bPersist); Invoke(bPersist);
                WaitFor(delegate
                {
                    int n = 0;
                    foreach (AutomationElement e in ToastsCollect()) if (ToastsNumber(e) > highestBefore) n++;
                    return n >= 2;
                }, 1000);
                var dups = new List<AutomationElement>();
                foreach (AutomationElement e in ToastsCollect())
                    if (ToastsNumber(e) > highestBefore) dups.Add(e);
                dups = dups.OrderBy(e => ToastsNumber(e)).ToList();
                Check("two toasts for focus restore", dups.Count >= 2,
                    "count=" + dups.Count + " before=" + highestBefore);
                if (dups.Count >= 2)
                {
                    WaitFor(delegate { return ToastsRect(dups[0], "Height") > 0 && ToastsRect(dups[1], "Height") > 0; }, 1000);
                    double gap = ToastsRect(dups[1], "Top") - ToastsRect(dups[0], "Top") - ToastsRect(dups[0], "Height");
                    Check("toasts 10 apart", Math.Abs(gap - 10) <= 1, "gap=" + gap.ToString("0.0"));
                    Check("newest toast at bottom", ToastsNumber(dups[1]) > ToastsNumber(dups[0]),
                        "ids=" + ToastsNumber(dups[0]) + "," + ToastsNumber(dups[1]));
                    var firstClose = dups[0].FindFirst(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.NameProperty, "Dismiss"));
                    string secondCloseId = dups[1].Current.AutomationId + "_Close";
                    Check("first persistent close found", firstClose != null);
                    if (firstClose != null)
                    {
                        SetForegroundWindow(new IntPtr(root.Current.NativeWindowHandle));
                        Drv.Focus(firstClose);
                        Check("restore: first close has focus", firstClose.Current.HasKeyboardFocus,
                            "hasFocus=" + firstClose.Current.HasKeyboardFocus);
                        if (firstClose.Current.HasKeyboardFocus)
                        {
                            string firstId = dups[0].Current.AutomationId;
                            Invoke(firstClose);
                            WaitFor(delegate { return Find(root, firstId) == null; }, 1000);
                            Check("first toast removed", Find(root, firstId) == null, "id=" + firstId);
                            WaitFor(delegate { return Find(root, secondCloseId) != null; }, 1000);
                            var secondClose = Find(root, secondCloseId);
                            Check("focus restore to next close", secondClose != null && secondClose.Current.HasKeyboardFocus,
                                "found=" + (secondClose != null) + " expected=" + secondCloseId);
                        }
                    }
                    // Persistent toasts do not self-dismiss: close anything left from this block.
                    foreach (AutomationElement e in ToastsCollect())
                    {
                        if (ToastsNumber(e) <= highestBefore) continue;
                        var cl = e.FindFirst(TreeScope.Descendants,
                            new PropertyCondition(AutomationElement.NameProperty, "Dismiss"));
                        if (cl != null) Invoke(cl);
                    }
                }
            }

            // F1: the grid's Copy button raises "Copied" with the selection count.
            var copyColumn = Find(root, "InputGrid_col0");
            var copyButton = Find(root, "InputGrid_Copy");
            if (copyColumn != null && copyButton != null)
            {
                IDataObject savedClipboard;
                bool snapshotOk = SnapshotClipboard(out savedClipboard);
                try
                {
                    Invoke(copyColumn);
                    WaitFor(delegate
                    {
                        var info = Find(root, "InputGrid_Info");
                        return info != null && GetValue(info).Contains("cells selected");
                    }, 1000);
                    Invoke(copyButton);
                    // The button's exact text is set in SheetGrid; here we verify the call site fires.
                    Check("F1 grid Copy raises Copied toast",
                        WaitFor(delegate { return ToastsFind("Copied") != null; }, 1500));
                }
                finally { if (snapshotOk) RestoreClipboard(savedClipboard); }
                var clearGrid = Find(root, "InputGrid_Clear");
                if (clearGrid != null) Invoke(clearGrid);
            }

            // F2: progress completion raises "Upload complete".
            var progressStart = Find(root, "ProgressStart");
            var progressReset = Find(root, "ProgressReset");
            if (progressStart != null && progressReset != null)
            {
                Invoke(progressStart);
                bool complete = WaitFor(delegate
                {
                    var label = Find(root, "ProgressLabel");
                    return label != null && GetValue(label) == "Upload complete";
                }, 15000);
                Check("F2 progress reaches Upload complete", complete);
                Check("F2 Upload complete toast", ToastsFind("Upload complete") != null);
                Invoke(progressReset);
            }

            // F3: the tables bulk bar raises Export started / Rows deleted and clears the selection.
            var selectAll = Find(root, "SelectAll");
            if (selectAll != null)
            {
                var allToggle = FastToggle.Of(selectAll);
                if (allToggle.Current.ToggleState != ToggleState.On) allToggle.Toggle();
                WaitFor(delegate { return Find(root, "BulkBar") != null; }, 1000);
                // The buttons live in the collapsed bulk bar: find them only after it is shown.
                var bulkExport = Find(root, "BulkExport");
                var bulkDelete = Find(root, "BulkDelete");
                Check("F3 bulk bar buttons found", bulkExport != null && bulkDelete != null);
                Func<int> rowCount = delegate
                {
                    var grid = Find(root, "SelectionGrid");
                    if (grid == null) return -1;
                    return grid.FindAll(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.AutomationIdProperty, "RowCheck")).Count;
                };
                if (bulkExport != null && bulkDelete != null)
                {
                    int rowsBefore = rowCount();
                    Invoke(bulkExport);
                    Check("F3 Export started toast", WaitFor(delegate { return ToastsFind("Export started") != null; }, 1000));
                    Invoke(bulkDelete);
                    Check("F3 Rows deleted toast", WaitFor(delegate { return ToastsFind("Rows deleted") != null; }, 1000));
                    int rowsAfter = rowCount();
                    Check("F3 rows removed", rowsBefore > 0 && rowsAfter == 0,
                        "before=" + rowsBefore + " after=" + rowsAfter);
                    var saAfter = Find(root, "SelectAll");
                    Check("F3 select-all unchecked after delete",
                        saAfter != null && FastToggle.Of(saAfter).Current.ToggleState == ToggleState.Off);
                }
            }
        }
    }
}
