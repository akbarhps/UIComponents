# Dropdown Phase 3 fix round: review (diff-only, nothing built or run)

Verdict: **fix-first** (one test-only item blocks; product code is correct).

## Blocking

1. `wpf/tests/UIAutomationTests.cs:1043-1056`: the M7 Escape check is now vacuous, and the stated reason for dropping the focus assertion looks wrong. Line 1043 closes the panel with `multiToggle.Toggle()` and the chip-remove check follows. The `PostKey(..., 0x1B)` at line 1052 is sent with the panel already closed, even though the comment says "panel already reopened". `Find(MultiSkills_Search) == null` was true before the key, so "M7 Escape closes multi" cannot fail. That also explains why UIA saw a Pane. The trigger was never focused because Escape had nothing to close, and focus was left wherever the UIA `Toggle` and chip-remove calls put it. It is not a UIA limitation. M6 (line 1027) uses the same `Close(true)` path and `HasKeyboardFocus` passes there, and the combo S6/S7/S2 checks assert focus the same way. Fix: after the chip-removed-while-closed check, `multiToggle.Toggle()` to reopen (poll until `MultiSkills_Search` exists), `PostKey 0x1B`, then `WaitFor(Search == null)` and `WaitFor(multiTrigger.Current.HasKeyboardFocus)`. Delete the "UIA reports Pane" comment. If the focus assertion still fails after a real reopen, report that as a product bug in `Close(true)`. Do not remove the check. No app-side focus hook is needed.

## Should-fix

2. `plan/wpf/ref/wpf-dropdown-multi-chips.png` and `wpf-dropdown-multi-open.png` are dated 08:02. `MultiSelect.cs` was last written at 09:20 (model, 8px chip gap). The fix prompt required re-capturing after the fix, and the chip column is the item being verified. Re-capture both and measure the chip host (402 + 8 gap) and the MultiFoot top divider pixel row.
3. `wpf/MultiSelect.cs:268,413,418,473-474,482-483,488-489`: redundant `UpdateTrigger()` calls rebuild every chip each time. Setting `IsChecked` already fires `Option_Checked`, which calls `UpdateTrigger`. Select all, Clear all and the `BuildPanel` init loop (268) therefore rebuild the chips N times, and 483, 489 and 320 rebuild them once more. Remove the trailing calls in `SelectAll_Click` and `Clear_Click`. Keep the one in `Remove_Click` (it is needed while closed), but drop the `_checks[i].IsChecked = false` branch's double call by only calling `UpdateTrigger` when `_checks` is empty. In `BuildPanel`, set the initial state before attaching the handlers, or keep a `_building` flag. This is also churn for UIA: chip elements are replaced on every toggle.
4. `wpf/MultiSelect.cs:77`: `_selected` is only allocated inside `ParseOptions` after the early return on empty `Options`. It is null for an option-less control, so `UpdateTrigger` is safe only because `_entries.Count == 0`. Initialise it at the declaration or in `OnInitialized` (`new bool[0]`). This is cheap insurance, and Phase 4 will likely copy the pattern.

## Nits

5. `wpf/SearchableSelect.cs:320-325`: forward Tab calls `Close(false)` in KeyDown before focus moves, and `Search_LostKeyboardFocus` (336-342) already implements the "close only if the new focus is outside" rule. The KeyDown branch is a second mechanism. It is defensible because the options are `Focusable=false` and the adorner may not support tab traversal, and it matches the web (S10 closes on forward Tab, Shift+Tab to the trigger stays open). Only Tab close is tested (line 934); the Shift+Tab-stays-open claim has no assertion and is unverified. Either add a Shift+Tab assertion or note it as untested.
6. `wpf/MultiSelect.cs`: M9 (Tab walks search, then checkboxes, then buttons) is untested and may not work, because the panel lives in an adorner outside the logical tree. The web multi has no focusout close, and WPF matches that (no Tab handling). Keep it as it is. Record it as a known unverified item.
7. `wpf/MultiSelect.cs:421-458`: `UpdateTrigger` builds the chips, count text and placeholder in one place, and `ChipWrapPanel`, `MultiChip` and `ChipRemove` are shared styles. Phase 4 (tag input) should reuse the styles and the `_Chip{n}` / `_Remove{n}` id scheme (index among the visible chips). It should not copy this method. Note that `Remove_Click` finds the model index by counting selected entries. A tag input has no such model, so it should use the chip's index directly.

## Checked and fine

- The model (`bool[] _selected`) drives chips, count and checkboxes. It survives Done, Escape and the close/reopen cycle, and chip removal works while closed (`_checks` is empty, so `i < _checks.Count` guards it).
- Select all applies to visible options only; Clear all applies to every option. Chips follow option order, and the count text is "N selected". All of this matches the web's `render()` and handlers.
- Escape is handled at window level with focus restored. An outside click closes without restoring focus, as web `close(false)` does (plan M8 is stale there). Done restores focus.
- `MultiFoot` is `0,1,0,0` with height 53. The dead `MultiChip` Margin setter is gone, and no `Margin(0)` override remains in the code.
- The chip host has an 8px right margin, with the chevron in its own 16px column.
- No C# 6+ syntax in `MultiSelect.cs`, `SearchableSelect.cs` or `DropdownPrimitives.cs`. The csproj lists `Themes\Dropdown.xaml`, `DropdownPrimitives.cs`, `SearchableSelect.cs` and `MultiSelect.cs`. All changes sit in owned files.
- M5, M3, M4 and persistence tests: the M5 numbers (4, then 0) and the M3 option order (TypeScript, Accessibility) match the plan, and the reopen and closed-removal tests are sound. The only test defect is item 1.
