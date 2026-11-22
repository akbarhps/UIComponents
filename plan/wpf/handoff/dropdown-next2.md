# dropdown: Phase 3 verdict = fix-first (one test defect), then Phase 4
(dropdown-next.md is old; this file supersedes it.) Review: plan/wpf/handoff/dropdown-p3-review.md (read it). Rules unchanged: agent "dropdown", build.bat dropdown, run-hidden.ps1 only, never commit, never kill by name. Minimise UIA actions (README test speed rule).

## Phase 3 fixes
1. BLOCKING UIAutomationTests.cs:1043-1056: the M7 Escape check is vacuous (panel was closed by multiToggle.Toggle() before Escape, so nothing closes and the "UIA reports Pane" reason was wrong: the trigger was never focused). Reopen the panel, poll until MultiSkills_Search exists, send Escape, WaitFor(Search == null) and WaitFor(multiTrigger.HasKeyboardFocus). Delete the "UIA reports Pane" comment. M6 uses the same Close(true) path and its focus assertion passes. If focus still fails after a real reopen it is a product bug in Close(true): fix it there.
2. Re-capture wpf-dropdown-multi-chips.png and wpf-dropdown-multi-open.png (dated 08:02, MultiSelect.cs changed 09:20); re-measure the 402+8 chip column and the MultiFoot divider row.
3. MultiSelect.cs:268,320,483,489,474 redundant UpdateTrigger(): setting IsChecked already fires Option_Checked -> UpdateTrigger. Drop the trailing calls; set initial state before attaching handlers (or a _building flag).
4. MultiSelect.cs:77 allocate _selected at its declaration (not after the early return).
Nits: add an assertion for Shift+Tab keeps the panel open if cheap.

## Heads-up
nav's button restyle (Controls.xaml 5-69, nav's file) shifts full-size button widths +2px toward the web in your captures. Do not edit Controls.xaml 5-69.

## Then Phase 4 (tag input) of plan/wpf/03-dropdown.md
Reuse ChipWrapPanel, MultiChip, ChipRemove styles and the _Chip{n}/_Remove{n} id scheme; do NOT copy UpdateTrigger or Remove_Click's bool[] mapping (specific to multi-select). Stop after Phase 4, report in README format.

## Applies after task 08 (tests split) is done
Read handoff/tests-split.md first (tooling change).
Area `dropdown`, file `wpf/tests/Tests.Dropdown.cs` (yours alone). Fix 1 (M7 vacuous Escape) and the Shift+Tab nit are in its Multi-select / Searchable select blocks. Phase 4 tag-input tests go in the same file (Tag input block). `InputControls.xaml` now holds only your four sections; File input and Date picker moved to `PickerControls.xaml`.
