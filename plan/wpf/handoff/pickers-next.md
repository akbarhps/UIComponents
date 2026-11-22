# pickers: P1fix+P2 verdict = fix-first, then P3
Review: plan/wpf/handoff/pickers-p2-review.md (read it, it has file:line and P3 dependencies). Rules unchanged: agent "pickers", build.bat pickers, run-hidden.ps1 only, never commit, never kill by name. Re-open UIAutomationTests.cs after your final run.

## Blocking
1. InputControls.xaml:140-141 date picker card rows swapped (73.8 then 95.8; web 95.8 then 73.8): swap, re-capture (Year-only label sits 22px too high).
2. CalendarPicker.cs:223-228 shadow Border stretches to 450 while the panel is 300: shell.Width=300, HorizontalAlignment=Left, re-capture (also a hit-test dead zone counted as inside by DropdownOutsideClick).
3. CalendarPicker.cs:195-198,534-543 IsSelected compares Y/M/D in every mode: make mode-aware (year: Y; month: Y+M); test Today then reopen in month and year mode.
4. CalendarPicker.cs:331-338 Render moves focus to the trigger when focus is anywhere in _panelRoot (Enter on Next throws focus out): test _bodyHost instead.
5. CalendarPicker.cs:599-609 focus restore at Input priority from PreviewMouseLeftButtonDown (before mouse-up) can drop the Click on real mouse presses: set Focusable=false on the pressed button until mouse-up/capture-lost, or restore on button-up. Add a PostMouseClick test on PickDate_Day14 (tests:156).
## Should-fix
6. Trigger KeyDown: with no selection Tab leaves the picker and closes it: handle Tab on the open trigger and focus Prev; add IsKeyboardFocusable assertions for tab stops.
7. Themes/Pickers.xaml:79-105 PickerCell focus ring (web global :focus-visible 2px ring): IsKeyboardFocused trigger.
8. Today ring shrinks to the digits (~16px) instead of 37.4x32 radius 7 (fails P1 acceptance 3): stretch ContentPresenter/content Grid.
9. LayoutUpdated never unhooked and _focusCellOnOpen cleared even if Focus() fails: unhook after use and in Close(); replace LayoutUpdated + 100 ms timer with one Dispatcher.BeginInvoke(Loaded, ...).
10. Tests: "P11 view month survives reopen" is vacuous (Prev then Next returns to today's month): pick a day in another month and reopen. Month/year coverage lacks Prev/Next (+-1 year, +-20), selected marker + focus on reopen, Today, Clear.
Nits: month cell Name = MonthsFull[i]; delete Today_Click's hardcoded 09:30 + wrong ponytail note (Today never exists in time mode, web 2650-2663); confirm FontWeights.Medium face.
UIA focus pattern: your HasKeyboardFocus approach is right (poll with WaitFor; for negative checks also assert where focus is).

## Heads-up
nav's button restyle (Controls.xaml 5-69) shifts full-size button widths +2px; don't edit it.

## Then P3 (time modes)
Per the review: 71.4 time row in the 5-row BuildPanel grid (panel height 398); WithTime true whenever WithSeconds; Day_Click copies and Apply clamps time (0-23/0-59/0-59); time inputs are fields so Clear keeps them (W6); Tab order Prev, Next, cells, time inputs, Clear, Apply; Render focus scope must not pull focus out of time inputs; mouse-press handler skips TextBox. Stop after P3, report in README format.

## Added by split Phase 1 (isolation)
"P10 outside click closes" passes in the full run but FAILS with `-Only pickers` alone (deterministic, reproduced): it relies on state left by an earlier area. Make P10 self-contained in Tests.Pickers.cs (open the picker itself, set up its own state). Every area must pass when run alone: `-Only pickers`.

## Applies after task 08 (tests split) is done
Read handoff/tests-split.md first (tooling change). Ignore the 'Re-open UIAutomationTests.cs' line above: that file is gone.
Area `pickers`, file `wpf/tests/Tests.Pickers.cs`. Fixes 3 (Today reopen asserts), 5 (`PostMouseClick` on `PickDate_Day14`), 6 (`IsKeyboardFocusable`) and 10 are there. Fix 1's rows are now in `PickerControls.xaml`, not `InputControls.xaml:140-141`. `Browse_Click` is in `PickerControls.xaml.cs`. P3's Date & time picker section goes into `PickerControls.xaml`. P10 (above) must be self-contained: it is the only known `-Only pickers` failure.
