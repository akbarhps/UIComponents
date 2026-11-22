// Covered: toasts.
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
            // Toasts T1/T2. T5 live regions are code-only (this harness references UIAutomationClient 3.0).
            Func<AutomationElement, string, double> rectPart = delegate(AutomationElement el, string part)
            {
                try
                {
                    object rect = el.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
                    return (double)rect.GetType().GetProperty(part).GetValue(rect, null);
                }
                catch (ElementNotAvailableException) { return 0; }
            };
            // Name-/type-scoped FindAll: walking the whole tree costs seconds and lets 3.6s toasts expire.
            Func<string, AutomationElement> findToast = delegate(string toastName)
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
                    catch (ElementNotAvailableException) { }   // a toast may dismiss mid-enumeration
                }
                return null;
            };
            Func<List<AutomationElement>> collectToasts = delegate()
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
            Func<string, int, bool> waitGone = delegate(string toastName, int maxMs)
            {
                int start = Environment.TickCount;
                while (Environment.TickCount - start < maxMs)
                {
                    if (findToast(toastName) == null) return true;
                    Thread.Sleep(80);
                }
                return findToast(toastName) == null;
            };
            Func<string, int, bool> waitPresent = delegate(string toastName, int maxMs)
            {
                int start = Environment.TickCount;
                while (Environment.TickCount - start < maxMs)
                {
                    if (findToast(toastName) != null) return true;
                    Thread.Sleep(80);
                }
                return findToast(toastName) != null;
            };
            Action<int> waitMs = delegate(int ms)
            {
                int start = Environment.TickCount;
                while (Environment.TickCount - start < ms) Thread.Sleep(50);
            };
            double windowRight = rectPart(root, "Left") + rectPart(root, "Width");
            double windowBottom = rectPart(root, "Top") + rectPart(root, "Height");

            // T1: raise and measure one toast at a time (Invoke is slow under four concurrent suites,
            // so an earlier toast can expire before a whole batch has been raised).
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
                var saved = findToast("Saved");
                Check("success toast shown", saved != null);
                if (saved != null)
                {
                    Check("toast id numbered", saved.Current.AutomationId.StartsWith("Toast"), "id=" + saved.Current.AutomationId);
                    Check("toast width 360", Math.Abs(rectPart(saved, "Width") - 360) <= 1, "w=" + rectPart(saved, "Width").ToString("0.0"));
                    Check("toast height 73.5", Math.Abs(rectPart(saved, "Height") - 73.5) <= 1, "h=" + rectPart(saved, "Height").ToString("0.0"));
                    // Window UIA rect includes the 8px frame: 24px layer inset => 32px here.
                    Check("toast right inset 24", Math.Abs(windowRight - rectPart(saved, "Left") - rectPart(saved, "Width") - 32) <= 1,
                        "gap=" + (windowRight - rectPart(saved, "Left") - rectPart(saved, "Width")).ToString("0.0"));
                }
                Check("plain toast auto-dismisses by 4.5s", waitGone("Saved", 4600 - (Environment.TickCount - shownAt)));
            }
            if (bInfo != null)
            {
                Invoke(bInfo);
                int infoShownAt = Environment.TickCount;
                var headsEl = FindByName(root, "Heads up");
                Check("info toast shown", headsEl != null);
                int to3s = 3000 - (Environment.TickCount - infoShownAt);
                if (to3s > 0) waitMs(to3s);
                try { Check("plain toast still present at 3.0s", headsEl != null && headsEl.Current.Name == "Heads up"); }
                catch (ElementNotAvailableException) { Check("plain toast still present at 3.0s", false, "expired before 3.0s"); }
                Check("info toast auto-dismisses by 4.5s", waitGone("Heads up", 4600 - (Environment.TickCount - infoShownAt)));
            }
            if (bError != null)
            {
                Invoke(bError);
                var failed = findToast("Upload failed");
                Check("error toast shown", failed != null);
                if (failed != null)
                    Check("toast bottom inset 24", Math.Abs(windowBottom - rectPart(failed, "Top") - rectPart(failed, "Height") - 32) <= 1,
                        "gap=" + (windowBottom - rectPart(failed, "Top") - rectPart(failed, "Height")).ToString("0.0"));
                Check("error toast auto-dismisses by 4.5s", waitGone("Upload failed", 4600));
            }

            // T2: variants, one at a time.
            var bDismiss = Find(root, "ToastDismissible");
            var bAction = Find(root, "ToastWithAction");
            var bPersist = Find(root, "ToastPersistent");
            Check("dismissible button found", bDismiss != null);
            Check("with-action button found", bAction != null);
            Check("persistent button found", bPersist != null);
            if (bPersist != null)
            {
                Invoke(bPersist);
                var pers = findToast("Connection lost");
                Check("persistent toast shown", pers != null);
                if (pers != null)
                {
                    Check("persistent height 97.5", Math.Abs(rectPart(pers, "Height") - 97.5) <= 1, "h=" + rectPart(pers, "Height").ToString("0.0"));
                    var close = Find(root, pers.Current.AutomationId + "_Close");
                    Check("persistent close found", close != null);
                    if (close != null)
                    {
                        double cx = rectPart(close, "Left") - rectPart(pers, "Left");
                        double cy = rectPart(close, "Top") - rectPart(pers, "Top");
                        Check("close at (325,15)", Math.Abs(cx - 325) <= 1 && Math.Abs(cy - 15) <= 1,
                            "rel=" + cx.ToString("0.0") + "," + cy.ToString("0.0"));
                    }
                    waitMs(4500);
                    Check("persistent toast survives 4.5s", findToast("Connection lost") != null);
                    if (close != null) Invoke(close);
                    Check("_Close removes toast", waitGone("Connection lost", 1000));
                }
            }
            if (bAction != null)
            {
                Invoke(bAction);
                var act = findToast("Message sent");
                Check("action toast shown", act != null);
                if (act != null)
                {
                    Check("action height 100.5", Math.Abs(rectPart(act, "Height") - 100.5) <= 1, "h=" + rectPart(act, "Height").ToString("0.0"));
                    var title = act.FindFirst(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.NameProperty, "Message sent"));
                    Check("body width 262", title != null && Math.Abs(rectPart(title, "Width") - 262) <= 1,
                        "w=" + (title == null ? "-" : rectPart(title, "Width").ToString("0.0")));
                    var action = Find(root, act.Current.AutomationId + "_Action");
                    Check("action button found", action != null);
                    if (action != null)
                    {
                        double ax = rectPart(action, "Left") - rectPart(act, "Left");
                        double ay = rectPart(action, "Top") - rectPart(act, "Top");
                        Check("action at (52,64.5)", Math.Abs(ax - 52) <= 1 && Math.Abs(ay - 64.5) <= 1,
                            "rel=" + ax.ToString("0.0") + "," + ay.ToString("0.0"));
                    }
                    waitMs(4500);
                    Check("action toast survives 4.5s (W10)", findToast("Message sent") != null);
                    if (action != null) Invoke(action);
                    Check("_Action raises Done toast", waitPresent("Done", 1000));
                    Check("_Action removes original toast", waitGone("Message sent", 1000));
                }
            }
            if (bDismiss != null)
            {
                Invoke(bDismiss);
                var disp = findToast("New update available");
                Check("dismissible toast shown", disp != null);
                if (disp != null)
                    Check("dismissible height 73.5", Math.Abs(rectPart(disp, "Height") - 73.5) <= 1,
                        "h=" + rectPart(disp, "Height").ToString("0.0"));
                Check("dismissible auto-dismisses by 4.5s", waitGone("New update available", 4600));
            }

            // Keyboard focus pauses the timer; leaving resumes it.
            if (bDismiss != null)
            {
                Invoke(bDismiss);
                var t = findToast("New update available");
                var c = t == null ? null : t.FindFirst(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.NameProperty, "Dismiss"));
                Check("focus pause toast shown", t != null && c != null);
                if (c != null)
                {
                    SetForegroundWindow(new IntPtr(root.Current.NativeWindowHandle));
                    try { Drv.Focus(c); }
                    catch (InvalidOperationException) { }   // toast may have dismissed before focus landed
                    if (!c.Current.HasKeyboardFocus)
                        Console.WriteLine("CODE-ONLY  keyboard-focus pause/resume: UIA SetFocus cannot focus a control on the hidden desktop");
                    else
                    {
                        waitMs(4000);
                        Check("keyboard focus pauses auto-dismiss", findToast("New update available") != null);
                        Drv.Focus(bDismiss);
                        Check("auto-dismiss resumes when focus leaves", waitGone("New update available", 1600));
                    }
                }
            }

            // Dismissing a focused toast moves focus to the immediately next toast's close button.
            if (bDismiss != null)
            {
                Invoke(bDismiss); Invoke(bDismiss);
                var dups = new List<AutomationElement>();
                foreach (AutomationElement e in collectToasts())
                {
                    try { if (e.Current.Name == "New update available") dups.Add(e); }
                    catch (ElementNotAvailableException) { }
                }
                Check("two toasts for focus restore", dups.Count >= 2, "count=" + dups.Count);
                if (dups.Count >= 2)
                {
                    double gap = rectPart(dups[1], "Top") - rectPart(dups[0], "Top") - rectPart(dups[0], "Height");
                    Check("newest toast at bottom", gap >= 9 && gap <= 11, "gap=" + gap.ToString("0.0"));
                    Check("toasts 10 apart", Math.Abs(gap - 10) <= 1, "gap=" + gap.ToString("0.0"));
                    var firstClose = dups[0].FindFirst(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.NameProperty, "Dismiss"));
                    string secondCloseId = dups[1].Current.AutomationId + "_Close";
                    if (firstClose != null)
                    {
                        SetForegroundWindow(new IntPtr(root.Current.NativeWindowHandle));
                        try { Drv.Focus(firstClose); }
                        catch (InvalidOperationException) { }
                        if (!firstClose.Current.HasKeyboardFocus)
                            Console.WriteLine("CODE-ONLY  focus restore after dismiss: UIA SetFocus cannot focus a control on the hidden desktop");
                        else
                        {
                            Invoke(firstClose);
                            Thread.Sleep(100);
                            var secondClose = Find(root, secondCloseId);
                            Check("focus restore to next close", secondClose != null && secondClose.Current.HasKeyboardFocus,
                                "expected=" + secondCloseId + " found=" + (secondClose != null));
                        }
                    }
                }
            }
        }
    }
}
