// Covered: buttons, slider, tabs, expander, multi-select list, breadcrumbs, stepper, tooltips, action menu.
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

                AutomationElement loading = Find(buttonsCard, "BtnLoading");
                Check("loading button found", loading != null);
                if (loading != null)
                {
                    // The Working… content swap can invalidate a cached UIA peer; re-find each poll.
                    Func<string> loadingState = delegate
                    {
                        try
                        {
                            AutomationElement el = Find(buttonsCard, "BtnLoading");
                            if (el == null) return "missing";
                            return (el.Current.IsEnabled ? "enabled|" : "disabled|") + el.Current.Name;
                        }
                        catch (Exception ex) { return "error:" + ex.GetType().Name; }
                    };
                    string state = null;
                    try { Invoke(loading); }
                    catch (COMException) { }
                    bool working = WaitFor(delegate
                    {
                        state = loadingState();
                        return state == "disabled|Working…";
                    }, 1000);
                    Check("B1 loading disabled + Working…", working, "state=" + state);
                    bool restored = WaitFor(delegate
                    {
                        state = loadingState();
                        return state == "enabled|Loading";
                    }, 2500);
                    Check("B1 loading restored", restored, "state=" + state);
                }
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
                    var select = FastSelectionItem.Of(tabItems[1]);
                    select.Select();
                    Check("tab selection", select.Current.IsSelected);
                }
            }

            // Expander — toggle via ExpandCollapsePattern
            var expander = Find(root, "Expander");
            Check("expander found", expander != null);
            if (expander != null)
            {
                var pattern = FastExpandCollapse.Of(expander);
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
                    var sel = FastSelectionItem.Of(items[1]);
                    sel.Select();
                    Check("multi-select item selected", sel.Current.IsSelected);
                }
            }

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
        }
    }
}
