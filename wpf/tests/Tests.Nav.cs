// Covered: full-size buttons (B1 names/focus; the _working double-click guard is code-only),
// action menu (M1, M2, M3 Down/Up/End/wrap, M4, M5 Tab-close, M6; Shift+Tab is code-only),
// tooltips (T1/T4), accordion (A1-A3), stepper (S1/S2), breadcrumbs (C1), tabs (X1/X2).
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
        static partial void RunNav(AutomationElement root, ref bool ran)
        {
            ran = true;
            // Buttons (06 N1): scoped to ButtonsCard; one walk for the 13 ids; B1 with WaitFor.
            var buttonsCard = Find(root, "ButtonsCard");
            Check("buttons card found", buttonsCard != null);
            if (buttonsCard != null)
            {
                string[] buttonIds = {
                    "BtnPrimary", "BtnDark", "BtnSecondary", "BtnGhost", "BtnDanger",
                    "BtnNewItem", "BtnSearch", "BtnContinue",
                    "BtnSmall", "BtnDefault", "BtnLarge", "BtnDisabled", "BtnLoading"
                };
                var present = new HashSet<string>();
                foreach (AutomationElement el in buttonsCard.FindAll(TreeScope.Descendants, Condition.TrueCondition))
                {
                    string id = el.Current.AutomationId;
                    if (id != null) present.Add(id);
                }
                int found = 0;
                foreach (string id in buttonIds) if (present.Contains(id)) found++;
                Check("buttons 13 ids found", found == buttonIds.Length, "found=" + found + "/13");

                var primary = Find(buttonsCard, "BtnPrimary");
                if (primary != null) { Invoke(primary); Check("primary button invokes", true); }

                var disabled = Find(buttonsCard, "BtnDisabled");
                Check("BtnDisabled is disabled", disabled != null && !disabled.Current.IsEnabled);

                AutomationElement loading = Find(buttonsCard, "BtnLoading");
                Check("loading button found", loading != null);
                if (loading != null)
                {
                    // The Working… content swap can invalidate a cached UIA peer; re-find each poll.
                    Func<string> loadingState = delegate
                    {
                        AutomationElement el = Find(buttonsCard, "BtnLoading");
                        if (el == null) return "missing";
                        return (el.Current.IsEnabled ? "enabled|" : "disabled|") + el.Current.Name;
                    };
                    Drv.Focus(loading);
                    Check("B1 focus before click", WaitFor(delegate { return loading.Current.HasKeyboardFocus; }, 1000));
                    bool invoked = false;
                    for (int attempt = 0; attempt < 2 && !invoked; attempt++)
                    {
                        try { Invoke(loading); invoked = true; }
                        catch (COMException) { loading = Find(buttonsCard, "BtnLoading"); }
                    }
                    Check("B1 invoke delivered", invoked);
                    string state = null;
                    bool working = WaitFor(delegate
                    {
                        state = loadingState();
                        return state == "enabled|Working…";
                    }, 1000);
                    Check("B1 working name (aria-disabled)", working, "state=" + state);
                    Check("B1 keeps focus", loading != null && loading.Current.HasKeyboardFocus);
                    // The _working double-click guard is code-only: re-invoking rebuilds the same name.
                    bool restored = WaitFor(delegate
                    {
                        state = loadingState();
                        return state == "enabled|Loading";
                    }, 2500);
                    Check("B1 loading restored", restored, "state=" + state);
                }
            }

            // Tabs (06 N6)
            var tabsCard = Find(root, "TabsCard");
            Check("tabs card found", tabsCard != null);
            if (tabsCard != null)
            {
                var tabProfile = Find(tabsCard, "TabProfile");
                var tabSecurity = Find(tabsCard, "TabSecurity");
                var tabBilling = Find(tabsCard, "TabBilling");
                var tabPanel = Find(tabsCard, "TabPanel");
                Check("tab controls found", tabProfile != null && tabSecurity != null && tabBilling != null && tabPanel != null);
                if (tabProfile != null && tabSecurity != null && tabBilling != null && tabPanel != null)
                {
                    Check("X1 initial tab", tabPanel.Current.Name == "Update your name, photo and public profile details.");
                    // Real keyboard traversal: from the control before the tab strip, Tab enters only
                    // TabProfile, and the next Tab must not stop on Security/Billing.
                    IntPtr tabHwnd = new IntPtr(root.Current.NativeWindowHandle);
                    var beforeTabs = Find(root, "ProgressReset");
                    Check("X1 pre-tab control found", beforeTabs != null);
                    if (beforeTabs != null)
                    {
                        Drv.Focus(beforeTabs);
                        PostKey(tabHwnd, 0x09);
                        Check("X1 Tab enters first tab", WaitFor(delegate
                        {
                            return tabProfile.Current.HasKeyboardFocus;
                        }, 1000));
                        PostKey(tabHwnd, 0x09);
                        // PostKey is async: wait for focus to actually leave Profile before reading
                        // the other tabs, otherwise the negative check can pass before the Tab arrives.
                        bool leftProfile = WaitFor(delegate
                        {
                            return !tabProfile.Current.HasKeyboardFocus;
                        }, 1000);
                        Check("X1 Tab leaves first tab", leftProfile);
                        Check("X1 Tab skips other tabs", leftProfile
                            && !tabSecurity.Current.HasKeyboardFocus && !tabBilling.Current.HasKeyboardFocus);
                    }
                    FastSelectionItem.Of(tabSecurity).Select();
                    Check("X1 select updates panel", WaitFor(delegate
                    {
                        return tabPanel.Current.Name == "Manage your password and two-factor authentication.";
                    }, 1000));
                    Drv.Focus(tabSecurity);
                    PostKey(tabHwnd, 0x27); // Right -> Billing
                    Check("X2 Right selects Billing", WaitFor(delegate
                    {
                        return tabBilling.Current.HasKeyboardFocus
                            && tabPanel.Current.Name == "View invoices and update your payment method.";
                    }, 1000));
                    PostKey(tabHwnd, 0x27); // wraps -> Profile
                    Check("X2 wraps to Profile", WaitFor(delegate
                    {
                        return tabProfile.Current.HasKeyboardFocus
                            && tabPanel.Current.Name == "Update your name, photo and public profile details.";
                    }, 1000));
                    PostKey(tabHwnd, 0x23); // End -> Billing
                    Check("X2 End", WaitFor(delegate { return tabBilling.Current.HasKeyboardFocus; }, 1000));
                    PostKey(tabHwnd, 0x24); // Home -> Profile
                    Check("X2 Home", WaitFor(delegate { return tabProfile.Current.HasKeyboardFocus; }, 1000));
                }
            }

            // Accordion (06 N4)
            var accordionCard = Find(root, "AccordionCard");
            Check("accordion card found", accordionCard != null);
            if (accordionCard != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    Check("accordion multi trigger " + i, Find(accordionCard, "AccMulti_" + i) != null);
                    Check("accordion single trigger " + i, Find(accordionCard, "AccSingle_" + i) != null);
                }
                Func<string, AutomationElement> accPanel = delegate(string id) { return Find(accordionCard, id); };
                // UIA reports empty rects for panels scrolled out of the viewport: assert toggle state
                // and the presence/absence of the panel's UIA peer in Accordion_Changed.
                Func<string, bool> accOn = delegate(string id)
                {
                    AutomationElement trigger = Find(accordionCard, id);
                    return trigger != null && FastToggle.Of(trigger).Current.ToggleState == ToggleState.On;
                };
                Check("A1 first panels open", accOn("AccMulti_0") && accOn("AccSingle_0")
                    && accPanel("AccMulti_Panel0") != null && accPanel("AccSingle_Panel0") != null,
                    "multi0=" + accOn("AccMulti_0") + " single0=" + accOn("AccSingle_0")
                    + " p0=" + (accPanel("AccMulti_Panel0") != null) + " sp0=" + (accPanel("AccSingle_Panel0") != null));
                Check("A1 other panels closed", !accOn("AccMulti_1") && !accOn("AccSingle_1")
                    && accPanel("AccMulti_Panel1") == null && accPanel("AccSingle_Panel1") == null);
                var multi1 = Find(accordionCard, "AccMulti_1");
                var single1 = Find(accordionCard, "AccSingle_1");
                var single0 = Find(accordionCard, "AccSingle_0");
                if (multi1 != null && single1 != null && single0 != null)
                {
                    try
                    {
                        FastToggle.Of(multi1).Toggle();
                        Check("A3 multi panel opens", WaitFor(delegate
                        {
                            return accOn("AccMulti_1") && accPanel("AccMulti_Panel1") != null;
                        }, 1000));
                        Check("A3 multi stays independent", accOn("AccMulti_0"));
                        FastToggle.Of(single1).Toggle();
                        Check("A2 single panel opens", WaitFor(delegate
                        {
                            return accOn("AccSingle_1") && accPanel("AccSingle_Panel1") != null;
                        }, 1000));
                        Check("A2 single closes Account", WaitFor(delegate
                        {
                            return !accOn("AccSingle_0") && accPanel("AccSingle_Panel0") == null;
                        }, 1000));
                    }
                    finally
                    {
                        if (FastToggle.Of(multi1).Current.ToggleState == ToggleState.On) FastToggle.Of(multi1).Toggle();
                        if (FastToggle.Of(single1).Current.ToggleState == ToggleState.On) FastToggle.Of(single1).Toggle();
                        if (FastToggle.Of(single0).Current.ToggleState == ToggleState.Off) FastToggle.Of(single0).Toggle();
                    }
                    Check("A2 state restored", WaitFor(delegate
                    {
                        return accOn("AccSingle_0") && accPanel("AccMulti_Panel1") == null
                            && accPanel("AccSingle_Panel1") == null;
                    }, 1000));
                }
            }

            // Breadcrumbs (06 N6)
            var crumbsCard = Find(root, "CrumbsCard");
            Check("crumbs card found", crumbsCard != null);
            if (crumbsCard != null)
            {
                Check("C1 both trails found", Find(crumbsCard, "CrumbWorkspace") != null
                    && Find(crumbsCard, "Crumb2Workspace") != null);
                var projects = Find(crumbsCard, "CrumbProjects");
                var current = Find(crumbsCard, "CrumbCurrent");
                Check("C1 links and current found", projects != null && current != null
                    && Find(crumbsCard, "CrumbWorkspace") != null && Find(crumbsCard, "CrumbRefresh") != null
                    && Find(crumbsCard, "Crumb2Workspace") != null && Find(crumbsCard, "Crumb2Refresh") != null
                    && Find(crumbsCard, "Crumb2Current") != null);
                if (projects != null && current != null)
                {
                    Check("C1 current status", current.Current.ItemStatus == "current", "status=" + current.Current.ItemStatus);
                    var scroll = Find(root, "ContentScroll") ?? FindByName(root, "ContentScroll");
                    if (scroll == null)
                    {
                        var panes = root.FindAll(TreeScope.Children,
                            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Pane));
                        foreach (AutomationElement pane in panes)
                            if (pane.Current.Name == "ContentScroll") { scroll = pane; break; }
                    }
                    if (scroll != null)
                    {
                        object value = scroll.GetCurrentPattern(ScrollPattern.Pattern);
                        ScrollPattern sp = value as ScrollPattern;
                        if (sp != null)
                        {
                            sp.SetScrollPercent(sp.Current.HorizontalScrollPercent, 0);
                            double offsetBefore = sp.Current.VerticalScrollPercent;
                            Invoke(projects);
                            double offsetAfter = WaitFor(delegate
                            {
                                return sp.Current.VerticalScrollPercent > offsetBefore;
                            }, 1000) ? sp.Current.VerticalScrollPercent : offsetBefore;
                            Check("C1 project crumb scrolls", offsetAfter > offsetBefore,
                                "before=" + offsetBefore + " after=" + offsetAfter);
                            sp.SetScrollPercent(sp.Current.HorizontalScrollPercent, 0);
                        }
                        else Check("C1 project crumb scrolls", false, "no ScrollPattern");
                    }
                }
            }

            // Stepper (06 N5)
            var stepperCard = Find(root, "StepperCard");
            Check("stepper card found", stepperCard != null);
            if (stepperCard != null)
            {
                var stepPrev = Find(stepperCard, "StepPrev");
                var stepNext = Find(stepperCard, "StepNext");
                var stepStatus = Find(stepperCard, "StepStatus");
                Check("stepper controls found", stepPrev != null && stepNext != null && stepStatus != null);
                if (stepPrev != null && stepNext != null && stepStatus != null)
                {
                    Func<string> stepText = delegate { return stepStatus.Current.Name; };
                    Check("S1 initial state", stepText() == "Step 1 of 4" && !stepPrev.Current.IsEnabled && stepNext.Current.IsEnabled,
                        "status=" + stepText());
                    var num0 = Find(stepperCard, "Step0_Num");
                    Check("S1 initial current status", num0 != null && num0.Current.ItemStatus == "current",
                        "status=" + (num0 == null ? "missing" : num0.Current.ItemStatus));
                    Invoke(stepNext);
                    Check("S2 first Next", WaitFor(delegate { return stepText() == "Step 2 of 4"; }, 1000), "status=" + stepText());
                    AutomationElement check0 = Find(stepperCard, "Step0_Check") ?? FindByName(stepperCard, "Step0_Check");
                    Check("S1 done marker state", check0 != null && check0.Current.ItemStatus == "done",
                        "status=" + (check0 == null ? "missing" : check0.Current.ItemStatus));
                    var num1 = Find(stepperCard, "Step1_Num");
                    Check("S1 current follows step", num1 != null && num1.Current.ItemStatus == "current",
                        "status=" + (num1 == null ? "missing" : num1.Current.ItemStatus));
                    for (int i = 0; i < 2; i++) Invoke(stepNext);
                    Check("S2 last step", WaitFor(delegate { return stepText() == "Step 4 of 4"; }, 1000), "status=" + stepText());
                    Check("S1 Next disabled at end", !stepNext.Current.IsEnabled);
                    for (int i = 0; i < 3; i++) Invoke(stepPrev);
                    Check("S2 back to first", WaitFor(delegate { return stepText() == "Step 1 of 4"; }, 1000), "status=" + stepText());
                    Check("S1 Previous disabled at start", !stepPrev.Current.IsEnabled);
                }
            }

            // Action menu (06 N2)
            var menuCard = Find(root, "MenuCard");
            Check("menu card found", menuCard != null);
            if (menuCard != null)
            {
                var menuTrigger = Find(menuCard, "MenuActions");
                var moreTrigger = Find(menuCard, "MenuMore");
                var menuResult = Find(menuCard, "MenuResult");
                Check("menu controls found", menuTrigger != null && moreTrigger != null && menuResult != null);
                IntPtr menuHwnd = new IntPtr(root.Current.NativeWindowHandle);
                Func<string, AutomationElement> menuItem = delegate(string id) { return Find(root, id); };
                Func<string, bool> focused = delegate(string id)
                {
                    AutomationElement el = menuItem(id);
                    return el != null && el.Current.HasKeyboardFocus;
                };
                try
                {
                    if (menuTrigger != null)
                    {
                        // M2: ArrowDown on an unclicked trigger opens and focuses the first item.
                        Drv.Focus(menuTrigger);
                        PostKey(menuHwnd, 0x28);
                        Check("M2 ArrowDown opens", WaitFor(delegate
                        {
                            return menuTrigger.Current.ItemStatus == "expanded" && focused("ProjectMenu_Item0");
                        }, 1000));
                        PostKey(menuHwnd, 0x1B);
                        Check("M2 Escape closes", WaitFor(delegate
                        {
                            return menuTrigger.Current.ItemStatus == "collapsed";
                        }, 1000));

                        Invoke(menuTrigger);
                        Check("M1 panel opens", WaitFor(delegate
                        {
                            return menuTrigger.Current.ItemStatus == "expanded" && menuItem("ProjectMenu_Item0") != null;
                        }, 1000));
                        Check("M1 first item focused", WaitFor(delegate { return focused("ProjectMenu_Item0"); }, 1000));
                        PostKey(menuHwnd, 0x28); PostKey(menuHwnd, 0x28); // Down x2
                        Check("M3 down x2 -> Export as CSV", WaitFor(delegate { return focused("ProjectMenu_Item2"); }, 1000));
                        PostKey(menuHwnd, 0x23); // End
                        Check("M3 End -> Delete project", WaitFor(delegate { return focused("ProjectMenu_Item5"); }, 1000));
                        PostKey(menuHwnd, 0x28); // Down wraps
                        Check("M3 wraps -> Edit project", WaitFor(delegate { return focused("ProjectMenu_Item0"); }, 1000));
                        PostKey(menuHwnd, 0x1B); // Escape
                        Check("M4 escape closes", WaitFor(delegate
                        {
                            return menuTrigger.Current.ItemStatus == "collapsed";
                        }, 1000));
                        Check("M4 escape restores focus", menuTrigger.Current.HasKeyboardFocus);

                        // M5: Tab closes without moving focus back to the trigger.
                        Invoke(menuTrigger);
                        WaitFor(delegate
                        {
                            return menuTrigger.Current.ItemStatus == "expanded" && focused("ProjectMenu_Item0");
                        }, 1000);
                        PostKey(menuHwnd, 0x09);
                        Check("M5 Tab closes", WaitFor(delegate
                        {
                            return menuTrigger.Current.ItemStatus == "collapsed" && !menuTrigger.Current.HasKeyboardFocus;
                        }, 1000));
                    }
                    if (moreTrigger != null)
                    {
                        Invoke(moreTrigger);
                        Check("M1 compact opens", WaitFor(delegate { return menuItem("CompactMenu_Item0") != null; }, 1000));
                        var pick = menuItem("CompactMenu_Item1");
                        if (pick != null) Invoke(pick);
                        Check("M6 click sets result", WaitFor(delegate
                        {
                            return menuResult != null && menuResult.Current.Name == "Make a copy";
                        }, 1000));
                    }
                }
                finally
                {
                    if (menuTrigger != null && menuTrigger.Current.ItemStatus == "expanded") { Invoke(menuTrigger); }
                    if (moreTrigger != null && moreTrigger.Current.ItemStatus == "expanded") { Invoke(moreTrigger); }
                }
            }

            // Tooltips (06 N3)
            var tipsCard = Find(root, "TipsCard");
            Check("tips card found", tipsCard != null);
            if (tipsCard != null)
            {
                var top = Find(tipsCard, "TipTop");
                var bottom = Find(tipsCard, "TipBottom");
                Check("tip buttons found", top != null && bottom != null
                    && Find(tipsCard, "TipLeft") != null && Find(tipsCard, "TipRight") != null
                    && Find(tipsCard, "TipSave") != null);
                if (top != null && bottom != null)
                {
                    Check("T4 help text", top.Current.HelpText == "Top tooltip", "help=" + top.Current.HelpText);
                    Drv.Focus(top);
                    Check("T1 bubble on focus", WaitFor(delegate
                    {
                        AutomationElement bubble = Find(root, "TipBubble");
                        return bubble != null && bubble.Current.Name == "Top tooltip";
                    }, 1000));
                    Drv.Focus(bottom);
                    Check("T1 bubble follows focus", WaitFor(delegate
                    {
                        AutomationElement bubble = Find(root, "TipBubble");
                        return bubble != null && bubble.Current.Name == "Bottom tooltip";
                    }, 1000));
                    AutomationElement bubbleEl = Find(root, "TipBubble");
                    Drv.Focus(Find(root, "BtnPrimary"));
                    bool stillVisible = false;
                    try { stillVisible = bubbleEl != null && !bubbleEl.Current.IsOffscreen; }
                    catch (Exception) { stillVisible = false; }
                    Check("T1 hover bridge grace", stillVisible);
                    Check("T1 bubble hides on blur", WaitFor(delegate
                    {
                        AutomationElement bubble = Find(root, "TipBubble");
                        return bubble == null || bubble.Current.IsOffscreen;
                    }, 1000));
                }
            }
        }
    }
}
