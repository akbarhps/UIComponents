# nav final review 3 (nav-fix4 follow-up)

Verdict: CLEAN, 0 blocking. 1 should-fix (test robustness), 1 nit. Suite not run (per instructions).

Verified from code:
- Popover.cs:138-194 grace logic is correct. TipShow stops the pending timer when the host is re-entered or refocused. Re-entering the bubble needs no handler because the Tick re-checks `host.IsKeyboardFocusWithin || host.IsMouseOver || b.IsMouseOver` before hiding. TipHide always stops the timer. Escape (line 134) calls TipHide directly, so it still dismisses with no delay. Switching to another tip hides the old one at once via `_activeTip`. There is one timer per host in a static map, the same lifetime as `Tips`, so nothing leaks. A stale Tick on an already-hidden bubble does nothing.
- The ItemStatus tabstop/notabstop proxy is fully gone from XAML and code. The remaining ItemStatus uses (stepper, breadcrumb, menu trigger) are legitimate.
- No mutation or temp code is left in Popover.cs, Tests.Nav.cs or MainWindow.Navigation.cs.

Findings:
1. wpf/tests/Tests.Nav.cs:109-113: should-fix: "X1 Tab skips other tabs" uses a negative condition (`!Security.focus && !Billing.focus`). It is already true before the posted VK_TAB is processed: PostKey is async, and the UIA read can be serviced before the queued WM_KEYDOWN. So WaitFor can pass on its first poll even with IsTabStop removed. The worker's single mutation FAIL may have been timing luck. Fix: first `WaitFor(!tabProfile.Current.HasKeyboardFocus)`, or WaitFor positive focus on the control after the tab strip, and only then assert Security/Billing are not focused.
2. wpf/tests/Tests.Nav.cs:389-394: nit: "T1 hover bridge grace" only covers the keyboard-blur path. It reads IsOffscreen once and depends on that read landing within 150 ms, so a slow UIA call could flake it. The pointer host->bubble path is untested, which is acceptable given that the hidden desktop cannot move the mouse. Leave it, or note the ceiling in a comment.
