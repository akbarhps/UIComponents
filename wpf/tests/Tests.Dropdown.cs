// Covered: tag input (T1-T4, T6, T7, wrap), autocomplete (A1-A7, A8 focusout), searchable select (S1-S8, S10), multi-select (M1-M7).
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
        static partial void RunDropdown(AutomationElement root, ref bool ran)
        {
            ran = true;
            // Tag input — T1-T7. The in-process driver lets these run in milliseconds.
            var tagCard = Find(root, "TagCard");
            Check("T1 tag card found", tagCard != null);
            if (tagCard != null)
            {
                IntPtr tagHwnd = new IntPtr(root.Current.NativeWindowHandle);
                var tagInput = Find(tagCard, "TagTags_Input");
                var tagHint = Find(tagCard, "TagTags_Hint");
                var skillsInput = Find(tagCard, "TagSkills_Input");
                var skillsHint = Find(tagCard, "TagSkills_Hint");
                Check("T1 tag input and hint found", tagInput != null && tagHint != null);
                Check("T1 initial tags and hint", tagHint != null && tagHint.Current.Name == "2 of 6 tags"
                    && Find(tagCard, "TagTags_Chip0") != null
                    && Find(tagCard, "TagTags_Chip0").Current.Name == "design");
                Check("T1 skills initial hint", skillsHint != null && skillsHint.Current.Name == "1 of 4 tags");

                // T2: Enter splits on commas, trims, skips duplicates, always clears.
                Drv.Focus(tagInput);
                SetValue(tagInput, "a, b ,A");
                PostKey(tagHwnd, 0x0D);
                WaitFor(() => tagHint.Current.Name == "4 of 6 tags", 2000);
                Check("T2 Enter adds comma-separated unique tags", tagHint.Current.Name == "4 of 6 tags",
                    "hint=" + tagHint.Current.Name);
                Check("T2 field cleared", GetValue(tagInput) == "");

                // T3: Backspace on an empty field removes the last chip.
                PostKey(tagHwnd, 0x08);
                WaitFor(() => tagHint.Current.Name == "3 of 6 tags", 2000);
                Check("T3 Backspace removes last tag", tagHint.Current.Name == "3 of 6 tags",
                    "hint=" + tagHint.Current.Name);

                // Shift+Tab to a remove button is a keyboard blur: the pending text must still be
                // added (the mouse-only bypass must not trigger). Drv.Focus is the focus-move proxy.
                var remove0 = Find(tagCard, "TagTags_Remove0");
                Drv.Focus(tagInput);
                SetValue(tagInput, "q, r");
                if (remove0 != null) Drv.Focus(remove0);
                WaitFor(() => tagHint.Current.Name == "4 of 6 tags", 2000);
                Check("T4 keyboard focus to remove adds pending text", tagHint.Current.Name == "4 of 6 tags",
                    "hint=" + tagHint.Current.Name);
                var tabChip = Find(tagCard, "TagTags_Chip3");
                Check("T4 keyboard blur keeps whole value", tabChip != null && tabChip.Current.Name == "q, r",
                    tabChip == null ? "missing" : tabChip.Current.Name);

                // T4: blur adds the whole value without a comma split.
                Drv.Focus(tagInput);
                SetValue(tagInput, "x, y");
                Drv.Focus(skillsInput);
                WaitFor(() => tagHint.Current.Name == "5 of 6 tags", 2000);
                Check("T4 blur adds pending text", tagHint.Current.Name == "5 of 6 tags",
                    "hint=" + tagHint.Current.Name);
                var blurredChip = Find(tagCard, "TagTags_Chip4");
                Check("T4 no comma split on blur", blurredChip != null && blurredChip.Current.Name == "x, y",
                    blurredChip == null ? "missing" : blurredChip.Current.Name);

                // T6: each chip has its own remove button.
                if (remove0 != null) Invoke(remove0);
                WaitFor(() => tagHint.Current.Name == "4 of 6 tags", 2000);
                Check("T6 chip remove", tagHint.Current.Name == "4 of 6 tags",
                    "hint=" + tagHint.Current.Name);

                // T7: at max the field stays enabled and further adds are ignored.
                Drv.Focus(skillsInput);
                SetValue(skillsInput, "vue,node,go,x");
                PostKey(tagHwnd, 0x0D);
                WaitFor(() => skillsHint.Current.Name == "4 of 4 tags", 2000);
                Check("T7 skills cap at 4", skillsHint.Current.Name == "4 of 4 tags",
                    "hint=" + skillsHint.Current.Name);
                Check("T7 field enabled at max", skillsInput.Current.IsEnabled);

                // Wrapping: field top and the hint below the box both move down by one 27.2+6 row.
                double topBefore = RectPart(tagInput, "Top");
                double hintTopBefore = RectPart(tagHint, "Top");
                Drv.Focus(tagInput);
                SetValue(tagInput, "user interviews");
                Drv.Focus(skillsInput);
                WaitFor(() => tagHint.Current.Name == "5 of 6 tags", 2000);
                Drv.Focus(tagInput);
                SetValue(tagInput, "accessibility audit");
                Drv.Focus(skillsInput);
                WaitFor(() => tagHint.Current.Name == "6 of 6 tags", 2000);
                double topAfter = RectPart(tagInput, "Top");
                Check("tag field wraps to a second row", topAfter - topBefore > 25 && topAfter - topBefore < 40,
                    "delta=" + (topAfter - topBefore));
                double hintTopAfter = RectPart(tagHint, "Top");
                Check("tag box grows so hint moves down on wrap", hintTopAfter - hintTopBefore > 25
                    && hintTopAfter - hintTopBefore < 40, "delta=" + (hintTopAfter - hintTopBefore));
                // Container height is measured from the capture (a plain Border has no UIA peer).
            }

            // Autocomplete — A1-A8.
            var acCard = Find(root, "AcCard");
            Check("A1 autocomplete card found", acCard != null);
            if (acCard != null)
            {
                IntPtr acHwnd = new IntPtr(root.Current.NativeWindowHandle);
                var fwInput = Find(acCard, "AcFramework_Input");
                var techInput = Find(acCard, "AcTech_Input");
                var techHint = Find(acCard, "AcTech_Hint");
                int total, onscreen, offscreen;

                // A1/A6: focus opens all 7; hint is shown ("0 selected").
                Drv.Focus(fwInput);
                WaitFor(() => Find(root, "AcFramework_Option0") != null, 2000);
                CountComboOptions(root, "AcFramework_Option", out total, out onscreen, out offscreen);
                Check("A1 Framework focus shows 7", onscreen == 7, "onscreen=" + onscreen);
                Check("A6 Technologies hint shown", techHint != null && techHint.Current.Name == "0 selected",
                    techHint == null ? "missing" : techHint.Current.Name);

                // A1 filter; empty row; 8-item cap in Multiple mode.
                SetValue(fwInput, "s");
                CountComboOptions(root, "AcFramework_Option", out total, out onscreen, out offscreen);
                Check("A1 's' leaves 3", onscreen == 3, "onscreen=" + onscreen);
                SetValue(fwInput, "zzz");
                CountComboOptions(root, "AcFramework_Option", out total, out onscreen, out offscreen);
                Check("A1 'zzz' shows empty row", onscreen == 0 && Find(root, "AcFramework_Empty") != null,
                    "onscreen=" + onscreen);

                // A4: Escape hides but keeps focus in the input.
                PostKey(acHwnd, 0x1B);
                WaitFor(() => Find(root, "AcFramework_Option0") == null, 2000);
                Check("A4 Escape hides list", Find(root, "AcFramework_Option0") == null);
                WaitFor(() => fwInput.Current.HasKeyboardFocus, 2000);
                Check("A4 focus stays in input", fwInput.Current.HasKeyboardFocus);

                // A2: Down highlights first; Up clamps at first.
                SetValue(fwInput, "");
                PostKey(acHwnd, 0x28);
                var fwActive = FindActiveComboOption(root, "AcFramework_Option");
                Check("A2 Down highlights first", fwActive != null && fwActive.Current.Name == "React",
                    fwActive == null ? "missing" : fwActive.Current.Name);
                PostKey(acHwnd, 0x26);
                fwActive = FindActiveComboOption(root, "AcFramework_Option");
                Check("A2 Up clamps at first", fwActive != null && fwActive.Current.Name == "React",
                    fwActive == null ? "missing" : fwActive.Current.Name);

                // A5: clicking an option chooses it and hides the list. The button must not take
                // focus on mouse-down (that would hide the list before Click fires).
                var fwOption0 = Find(root, "AcFramework_Option0");
                Check("A5 option not focusable", fwOption0 != null && !fwOption0.Current.IsKeyboardFocusable);
                if (fwOption0 != null) Invoke(fwOption0);
                WaitFor(() => Find(root, "AcFramework_Option0") == null, 2000);
                Check("A5 option sets text", GetValue(fwInput) == "React", GetValue(fwInput));
                Check("A5 list hidden after choose", Find(root, "AcFramework_Option0") == null);

                // A8: focusout hides the list.
                SetValue(fwInput, "Vue");
                WaitFor(() => Find(root, "AcFramework_Option0") != null, 2000);
                Drv.Focus(techInput);
                WaitFor(() => Find(root, "AcFramework_Option0") == null, 2000);
                Check("A8 focusout hides list", Find(root, "AcFramework_Option0") == null);

                // A1 cap and A3 duplicate check in Multiple mode.
                Drv.Focus(techInput);
                WaitFor(() => Find(root, "AcTech_Option0") != null, 2000);
                CountComboOptions(root, "AcTech_Option", out total, out onscreen, out offscreen);
                Check("A1 tech focus shows 8", onscreen == 8, "onscreen=" + onscreen);
                PostKey(acHwnd, 0x1B);
                SetValue(techInput, "React");
                PostKey(acHwnd, 0x0D);
                WaitFor(() => techHint.Current.Name == "1 selected", 2000);
                Check("A3 typed value adds a tag", techHint.Current.Name == "1 selected", techHint.Current.Name);
                var acRemove0 = Find(acCard, "AcTech_Remove0");
                Check("A3 chip remove not focusable", acRemove0 != null && !acRemove0.Current.IsKeyboardFocusable);
                SetValue(techInput, "react");
                PostKey(acHwnd, 0x0D);
                WaitFor(() => GetValue(techInput) == "", 2000);
                Check("A3 duplicate on Enter ignored", techHint.Current.Name == "1 selected", techHint.Current.Name);
                Check("A3 duplicate clears field", GetValue(techInput) == "", GetValue(techInput));
                SetValue(techInput, "NewLang");
                PostKey(acHwnd, 0x0D);
                WaitFor(() => techHint.Current.Name == "2 selected", 2000);
                Check("A3 new value adds second tag", techHint.Current.Name == "2 selected", techHint.Current.Name);

                // A7: chip removal.
                var acRemove = Find(acCard, "AcTech_Remove0");
                if (acRemove != null) Invoke(acRemove);
                WaitFor(() => techHint.Current.Name == "1 selected", 2000);
                Check("A7 chip remove", techHint.Current.Name == "1 selected", techHint.Current.Name);
                PostKey(acHwnd, 0x1B);
            }

            // Searchable select — Phase 1 shell + Phase 2 behaviour (U and K via PostKey).
            var comboTrigger = Find(root, "ComboAssignee_Trigger");
            Check("combo trigger found", comboTrigger != null);
            if (comboTrigger != null)
            {
                int total, onscreen, offscreen;
                IntPtr comboHwnd = new IntPtr(root.Current.NativeWindowHandle);
                var togglePattern = FastToggle.Of(comboTrigger);
                Check("combo search absent when closed", Find(root, "ComboAssignee_Search") == null);

                // S1: open by trigger.
                togglePattern.Toggle();
                Thread.Sleep(200);
                var comboSearch = Find(root, "ComboAssignee_Search");
                Check("S1 search reachable from UIA root and focused",
                    comboSearch != null && comboSearch.Current.HasKeyboardFocus);
                CountComboOptions(root, "ComboAssignee_Option", out total, out onscreen, out offscreen);
                Check("S1 six options visible", onscreen == 6,
                    "total=" + total + " onscreen=" + onscreen + " offscreen=" + offscreen);

                Check("S1 search found", comboSearch != null);

                // S4: substring of label+meta; highlight cleared by filtering; empty row when nothing matches.
                SetValue(comboSearch, "eng"); Thread.Sleep(150);
                CountComboOptions(root, "ComboAssignee_Option", out total, out onscreen, out offscreen);
                Check("S4 filter eng leaves 1", onscreen == 1, "onscreen=" + onscreen);
                SetValue(comboSearch, "design"); Thread.Sleep(150);
                CountComboOptions(root, "ComboAssignee_Option", out total, out onscreen, out offscreen);
                Check("S4 filter design leaves 2", onscreen == 2, "onscreen=" + onscreen);
                PostKey(comboHwnd, 0x28); Thread.Sleep(150); // highlight something before filtering again
                Check("S4 an option is active before filter", FindActiveComboOption(root, "ComboAssignee_Option") != null);
                SetValue(comboSearch, "xyz"); Thread.Sleep(150);
                CountComboOptions(root, "ComboAssignee_Option", out total, out onscreen, out offscreen);
                Check("S4 filter xyz leaves 0", onscreen == 0, "onscreen=" + onscreen);
                Check("S4 empty row shown", Find(root, "ComboAssignee_Empty") != null);
                Check("S4 highlight cleared by filter", FindActiveComboOption(root, "ComboAssignee_Option") == null);
                SetValue(comboSearch, ""); Thread.Sleep(150);
                Check("S4 empty row hidden again", Find(root, "ComboAssignee_Empty") == null);

                // S5: Up from no active lands on the LAST visible option; both directions wrap.
                PostKey(comboHwnd, 0x26); Thread.Sleep(150); // VK_UP
                var active = FindActiveComboOption(root, "ComboAssignee_Option");
                Check("S5 Up from none lands on Priya Shah", active != null && active.Current.Name == "Priya Shah",
                    active == null ? "missing" : active.Current.Name);
                PostKey(comboHwnd, 0x28); Thread.Sleep(150); // VK_DOWN
                active = FindActiveComboOption(root, "ComboAssignee_Option");
                Check("S5 Down wraps to Ada Lovelace", active != null && active.Current.Name == "Ada Lovelace",
                    active == null ? "missing" : active.Current.Name);
                PostKey(comboHwnd, 0x26); Thread.Sleep(150); // VK_UP from first
                active = FindActiveComboOption(root, "ComboAssignee_Option");
                Check("S5 Up from first wraps to Priya Shah", active != null && active.Current.Name == "Priya Shah",
                    active == null ? "missing" : active.Current.Name);

                // S6: Enter with no active option does nothing.
                SetValue(comboSearch, "xyz"); Thread.Sleep(100);
                SetValue(comboSearch, ""); Thread.Sleep(100);
                var beforeEnter = Find(root, "ComboAssignee_Value");
                PostKey(comboHwnd, 0x0D); Thread.Sleep(150);
                var afterEnter = Find(root, "ComboAssignee_Value");
                Check("S6 Enter with no active does nothing",
                    afterEnter != null && beforeEnter != null && afterEnter.Current.Name == beforeEnter.Current.Name
                        && Find(root, "ComboAssignee_Search") != null);

                // S6: Enter with an active option chooses it.
                PostKey(comboHwnd, 0x28); Thread.Sleep(150);
                PostKey(comboHwnd, 0x0D); Thread.Sleep(150); // VK_RETURN
                var chosen = Find(root, "ComboAssignee_Value");
                Check("S6 Enter chooses active option", chosen != null && chosen.Current.Name == "Ada Lovelace",
                    chosen == null ? "missing" : chosen.Current.Name);
                Check("S6 panel closed after Enter", Find(root, "ComboAssignee_Search") == null);
                Check("S6 trigger focused after Enter", comboTrigger.Current.HasKeyboardFocus);

                // S3: Down, Enter and Space on the closed trigger all open and highlight the first option.
                Drv.Focus(comboTrigger);
                PostKey(comboHwnd, 0x28); Thread.Sleep(200);
                Check("S3 Down opens combo", Find(root, "ComboAssignee_Search") != null);
                var first = FindActiveComboOption(root, "ComboAssignee_Option");
                Check("S3 first option highlighted", first != null && first.Current.Name == "Ada Lovelace",
                    first == null ? "missing" : first.Current.Name);

                // S7: Escape closes and focuses the trigger.
                comboSearch = Find(root, "ComboAssignee_Search");
                Drv.Focus(comboSearch);
                PostKey(comboHwnd, 0x1B); Thread.Sleep(150); // VK_ESCAPE
                Check("S7 Escape closes panel", Find(root, "ComboAssignee_Search") == null);
                Check("S7 trigger focused after Escape", comboTrigger.Current.HasKeyboardFocus);

                // S3: Enter and Space also open from the trigger.
                Drv.Focus(comboTrigger); PostKey(comboHwnd, 0x0D); Thread.Sleep(150);
                Check("S3 Enter opens combo", Find(root, "ComboAssignee_Search") != null);
                PostKey(comboHwnd, 0x1B); Thread.Sleep(150);
                Drv.Focus(comboTrigger); PostKey(comboHwnd, 0x20); Thread.Sleep(150); // VK_SPACE
                Check("S3 Space opens combo", Find(root, "ComboAssignee_Search") != null);
                PostKey(comboHwnd, 0x1B); Thread.Sleep(150);

                // S10 (web fix): Tab out of the search closes the panel.
                togglePattern.Toggle(); Thread.Sleep(150);
                comboSearch = Find(root, "ComboAssignee_Search");
                Drv.Focus(comboSearch);
                PostKey(comboHwnd, 0x09); Thread.Sleep(150); // VK_TAB
                Check("S10 Tab closes panel", Find(root, "ComboAssignee_Search") == null);

                // S2: toggle close removes the panel and restores focus.
                togglePattern.Toggle(); Thread.Sleep(150);
                togglePattern.Toggle(); Thread.Sleep(150);
                Check("S2 toggle close removes panel", Find(root, "ComboAssignee_Search") == null);
                Check("S2 trigger focused after toggle close", comboTrigger.Current.HasKeyboardFocus);

                // S8: click-to-choose a single selection.
                togglePattern.Toggle(); Thread.Sleep(150);
                var option1 = Find(root, "ComboAssignee_Option1");
                if (option1 != null) Invoke(option1);
                Thread.Sleep(150);
                var value = Find(root, "ComboAssignee_Value");
                Check("S8 choose sets value text", value != null && value.Current.Name == "Jonas Silva",
                    value == null ? "missing" : value.Current.Name);
                Check("S8 choose marks value selected", value != null && value.Current.ItemStatus == "selected",
                    value == null ? "missing" : value.Current.ItemStatus);
                Check("S8 search gone after choose", Find(root, "ComboAssignee_Search") == null);
            }

            // Multi-select — M1-M7 with UIA and PostKey (M8/M9 are code-only).
            var multiTrigger = Find(root, "MultiSkills_Trigger");
            Check("M1 multi trigger found", multiTrigger != null);
            if (multiTrigger != null)
            {
                var multiToggle = FastToggle.Of(multiTrigger);
                Check("M1 multi search absent when closed", Find(root, "MultiSkills_Search") == null);
                multiToggle.Toggle(); Thread.Sleep(200);
                var multiSearch = Find(root, "MultiSkills_Search");
                Check("M1 multi search focused on open", multiSearch != null && multiSearch.Current.HasKeyboardFocus);
                Check("M1 multi placeholder visible", Find(root, "MultiSkills_Placeholder") != null);
                int total, onscreen, offscreen;
                CountComboOptions(root, "MultiSkills_Option", out total, out onscreen, out offscreen);
                Check("M1 six options shown", onscreen == 6, "onscreen=" + onscreen);

                // Pre-check React and TypeScript so select-all has hidden options to add.
                var opt0 = Find(root, "MultiSkills_Option0");
                var opt1 = Find(root, "MultiSkills_Option1");
                var opt3 = Find(root, "MultiSkills_Option3");
                if (opt0 != null) (FastToggle.Of(opt0)).Toggle();
                if (opt1 != null) (FastToggle.Of(opt1)).Toggle();
                Thread.Sleep(150);

                // M2 filter; M5 select all visible only (2 + React/TypeScript = 4), clear all includes hidden.
                SetValue(multiSearch, "design"); Thread.Sleep(150);
                CountComboOptions(root, "MultiSkills_Option", out total, out onscreen, out offscreen);
                Check("M2 design leaves 2", onscreen == 2, "onscreen=" + onscreen);
                var selectAll = Find(root, "MultiSkills_SelectAll");
                if (selectAll != null) Invoke(selectAll);
                Thread.Sleep(150);
                var count = Find(root, "MultiSkills_Count");
                Check("M5 select all checks visible only", count != null && count.Current.Name == "4 selected",
                    count == null ? "missing" : count.Current.Name);
                var clearAll = Find(root, "MultiSkills_Clear");
                if (clearAll != null) Invoke(clearAll);
                Thread.Sleep(150);
                count = Find(root, "MultiSkills_Count");
                Check("M5 clear all unchecks everything", count != null && count.Current.Name == "0 selected",
                    count == null ? "missing" : count.Current.Name);
                Check("M3 placeholder returns at zero", Find(root, "MultiSkills_Placeholder") != null);

                // M3 chips follow option order, not click order (click Option3 then Option1).
                SetValue(multiSearch, ""); Thread.Sleep(150);
                if (opt3 != null) (FastToggle.Of(opt3)).Toggle();
                if (opt1 != null) (FastToggle.Of(opt1)).Toggle();
                Thread.Sleep(150);
                count = Find(root, "MultiSkills_Count");
                Check("M3 two checked gives 2 selected", count != null && count.Current.Name == "2 selected",
                    count == null ? "missing" : count.Current.Name);
                var chip0 = Find(root, "MultiSkills_Chip0");
                var chip1 = Find(root, "MultiSkills_Chip1");
                Check("M3 chips in option order", chip0 != null && chip0.Current.Name == "TypeScript"
                    && chip1 != null && chip1.Current.Name == "Accessibility",
                    (chip0 == null ? "missing0" : chip0.Current.Name) + "/" + (chip1 == null ? "missing1" : chip1.Current.Name));

                // M4 chip removal while the panel is open, and after close/reopen selection persists.
                var remove0 = Find(root, "MultiSkills_Remove0");
                if (remove0 != null) Invoke(remove0);
                Thread.Sleep(150);
                count = Find(root, "MultiSkills_Count");
                Check("M4 chip remove unchecks option", count != null && count.Current.Name == "1 selected",
                    count == null ? "missing" : count.Current.Name);
                Check("M4 chip remove keeps panel open", Find(root, "MultiSkills_Search") != null);
                if (opt0 != null) (FastToggle.Of(opt0)).Toggle();
                Thread.Sleep(150);

                // M6 Done closes and focuses the trigger.
                var done = Find(root, "MultiSkills_Done");
                if (done != null) Invoke(done);
                Thread.Sleep(150);
                Check("M6 Done closes panel", Find(root, "MultiSkills_Search") == null);
                Check("M6 trigger focused after Done", multiTrigger.Current.HasKeyboardFocus);
                chip0 = Find(root, "MultiSkills_Chip0");
                Check("M6 selection persists after Done", chip0 != null && chip0.Current.Name == "React",
                    chip0 == null ? "missing" : chip0.Current.Name);

                // Reopen: checkboxes and chips are rebuilt from the model; chip removal works while closed.
                multiToggle.Toggle(); Thread.Sleep(150);
                count = Find(root, "MultiSkills_Count");
                Check("M1 selection survives reopen", count != null && count.Current.Name == "2 selected",
                    count == null ? "missing" : count.Current.Name);
                chip0 = Find(root, "MultiSkills_Chip0");
                Check("M1 chips survive reopen", chip0 != null && chip0.Current.Name == "React",
                    chip0 == null ? "missing" : chip0.Current.Name);
                var checkedOpt0 = Find(root, "MultiSkills_Option0");
                Check("M1 checkbox state survives reopen", checkedOpt0 != null
                    && (FastToggle.Of(checkedOpt0)).Current.ToggleState == ToggleState.On);
                multiToggle.Toggle(); Thread.Sleep(150); // close without Done
                var closedRemove = Find(root, "MultiSkills_Remove0");
                if (closedRemove != null) Invoke(closedRemove);
                Thread.Sleep(150);
                chip0 = Find(root, "MultiSkills_Chip0");
                Check("M4 chip removable while closed", chip0 != null && chip0.Current.Name == "Accessibility"
                    && Find(root, "MultiSkills_Chip1") == null,
                    chip0 == null ? "missing" : chip0.Current.Name);
                // M7 Escape anywhere closes and focuses the trigger.
                multiToggle.Toggle();
                WaitFor(() => Find(root, "MultiSkills_Search") != null, 2000);
                PostKey(new IntPtr(root.Current.NativeWindowHandle), 0x1B);
                WaitFor(() => Find(root, "MultiSkills_Search") == null, 2000);
                Check("M7 Escape closes multi", Find(root, "MultiSkills_Search") == null);
                WaitFor(() => multiTrigger.Current.HasKeyboardFocus, 2000);
                Check("M7 trigger focused after Escape", multiTrigger.Current.HasKeyboardFocus);
            }
        }
    }
}
