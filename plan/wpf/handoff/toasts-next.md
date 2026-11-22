# toasts: T1/T2 verdict = fix-first (tests partly vacuous), then T3
Review: plan/wpf/handoff/toasts-t2-review.md (read it). Rules unchanged: agent "toasts", build.bat toasts, run-hidden.ps1 only, never commit, never kill by name. Minimise UIA actions (README test speed rule). Re-open UIAutomationTests.cs after your final run (shared file gets overwritten by stale copies).

## Fixes (tests unless noted)
1. BLOCKING UIAutomationTests.cs:1675-1689 and 1711-1726: when HasKeyboardFocus is false after SetFocus the code only prints CODE-ONLY, no Check runs, so pause-on-focus / resume / focus-restore can never fail. Your claim that UIA focus is unreliable on the hidden desktop is wrong: this file asserts HasKeyboardFocus after SetFocus in 9 other places (S1, S6, S7, M1, M6, P9, O0). Likely cause is a race: the dismissible toast expires at 3.6 s while the slow FindFirst/SetFocus run under 4 suites. Run focus restore on TWO PERSISTENT toasts (never expire) and fail loudly if focus does not land. Pause test: cheap name-scoped FindAll.
2. BLOCKING :1687 resume timing waitGone(...,1600) is wrong: after pause the remaining time is 3600 minus time before focus landed. Wait 3600+800 ms and add a lower bound proving the timer was paused.
3. :1579-1584 "still present at 3.0s" probably cannot fail (whole-tree FindByName + Current.Name on a removed peer often still returns). Use findToast("Heads up") != null.
4. Themes/Toasts.xaml:60,87 FocusVisualStyle=null on close/action buttons leaves no keyboard focus ring; web has global :focus-visible 2px outline (index.html:50). Add a ring or record as accepted difference in the report.
5. Toasts.cs:168 Schedule: add the Timer.IsEnabled guard like the web.
Nits: hard-coded "- 32" frame constant; "newest at bottom" duplicates "10 apart" (assert order); root.Tag as close handle; record that stack margin 19 + toast 5/5 is equivalent to the plan's Margin 0,0,24,24.

## Heads-up
nav's button restyle (Controls.xaml 5-69, nav's file) shifts full-size button widths +2px toward the web in captures with full-size buttons. Do not edit Controls.xaml 5-69.

## Then T3 (only after the fixes land)
README coverage list (wpf/README.md:32; record wall-time delta, toast tests spend ~4.5 s four times on waitMs); F1 SheetGrid.cs:684 "Copied" toasts; F2 MainWindow.xaml.cs:39-40 "Upload complete" (remove the ponytail: note); F3 "Rows deleted"/"Export started" has no WPF call site found: port it or report. Existing callers OverlayControls.xaml.cs:37,60. Stop after T3, report in README format.

## Applies after task 08 (tests split) is done
Read handoff/tests-split.md first (tooling change). Ignore the 'Re-open UIAutomationTests.cs' line above: that file is gone.
Area `toasts`, file `wpf/tests/Tests.Toasts.cs`. Fixes 1-3 and the nits are there. Also apply `handoff/driver-review.md` #7: resume wait = `3600 - (focusAt - shownAt) + 800` plus a 'not gone after remaining-400' lower bound; the dups block keeps only toasts with an id number above the highest `ToastN` seen before the two Invokes; `WaitFor(..., 1000)` before the first read after each `Invoke`. Also #15: reword CODE-ONLY to '(driver off)'. F2 now edits `MainWindow.Display.cs` (`ProgressTimer_Tick`), not `MainWindow.xaml.cs`.
