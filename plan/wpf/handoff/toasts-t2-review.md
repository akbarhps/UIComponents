# toasts T1 fixes + T2 review (diff-only, nothing built or run)

Verdict: fix-first (tests only; production code is clean).

Checked: Toasts.cs, Themes/Toasts.xaml, ToastControls.xaml(.cs), the Toasts block of wpf/tests/UIAutomationTests.cs (1484-1728) against web/index.html 536-555, 716-722, 3441-3508 and plan 07 (toast spec, T table, T2 acceptance). Compared web-overlays-toast-variants-shown.png with wpf-overlays-toast-variants-shown.png visually: layout, colours, icon circle, close, Undo all match.

## Blocking

1. wpf/tests/UIAutomationTests.cs:1675-1689 and 1711-1726: the focus-pause/resume and the focus-restore assertions are vacuous. If `HasKeyboardFocus` is false after `SetFocus`, the test only prints "CODE-ONLY" and no `Check` runs, so a regression in W10 pause-on-focus, resume or focus restore cannot fail. The worker's claim that UIA focus is unreliable on the hidden desktop does not hold: the same file asserts `HasKeyboardFocus` after `SetFocus` in S1, S6, S7, M1, M6, P9 and O0 (lines 330-336, 849-850, 905, 965, 1027, 1178-1179) and the suite passes them. The likely real cause is a race, not the platform: the toast is a 3.6 s toast, and the slow `FindFirst`/`SetForegroundWindow` under 4 concurrent suites can let it expire before focus lands (hence the `InvalidOperationException` catch). Fix: run both tests on persistent toasts, which never expire. Focus restore: raise "Persistent" twice, `SetFocus` the first `_Close`, `Invoke` it, then assert the second `_Close` has focus. If `SetFocus` still fails, `Check(..., false, "SetFocus did not land")` instead of printing. Pause/resume: keep the dismissible toast but fetch the close button with one cheap name-scoped `FindAll` (not `Find(root, id)`), and fail loudly if focus did not land.

2. wpf/tests/UIAutomationTests.cs:1687: if the focus branch ever runs, "auto-dismiss resumes when focus leaves" will fail or flake. After the pause, `Remaining` is 3600 minus the time before focus landed (roughly 0.3-2 s), so the resume needs about 1.6-3.3 s after `bDismiss.SetFocus()`, and `waitGone(..., 1600)` only passes when 2 s or more were already used. Fix: wait up to 3600 + 800 ms, and add a lower bound that proves the timer was paused/restarted, for example assert it is still present 500 ms after refocusing the button.

## Should-fix

3. wpf/tests/UIAutomationTests.cs:1579-1584: the "plain toast still present at 3.0s" lower bound can never fail if the element stays queryable after removal. `headsEl` comes from the whole-tree `FindByName`, which may return the title TextBlock rather than the GroupBox, and `Current.Name` on a removed WPF peer often still returns the cached name instead of throwing `ElementNotAvailableException`. Fix: use `findToast("Heads up") != null` at 3.0 s (the cheap name-scoped lookup the other checks use), and drop the `FindByName` whole-tree walk, which the comment at 1494 says costs seconds.

4. wpf/Themes/Toasts.xaml:60 and 87: `FocusVisualStyle=null` on the close and action buttons removes any keyboard focus indicator. Web index.html:50 gives every control `:focus-visible { outline:2px solid var(--focus); outline-offset:2px; border-radius:4px }`, so keyboard users get a ring on `.toast__close` and `.toast__action`. This also matters for W10, since focus is what pauses the toast. Fix: add a 2px `Focus` ring (or reuse the shared FocusVisualStyle if one exists), and confirm what the other ported buttons do first. If the project accepts "no focus ring" elsewhere, list it as an accepted difference in the T2 report.

5. wpf/Toasts.cs:168-176: `Schedule` has no `if (state.Timer.IsEnabled) return;`, which the web has (`if(... || timer ...) return`). A second call while the timer runs (MouseLeave plus the deferred LostKeyboardFocus in the same turn) restarts the timer from `Started = Now` without subtracting the elapsed time, so the toast can live slightly longer than 3.6 s. Fix: add the guard.

## Nit

6. wpf/tests/UIAutomationTests.cs:1570, 1593: the `- 32` (24 + 8 px window frame) is correct for the current window but is a magic constant tied to the frame. Define it once from the layer or `ToastStack` rect, or name the constant. The "newest toast at bottom" check (1706) is the same assertion as "10 apart" and does not prove newest-last; compare the numeric part of the AutomationIds (`Toast<n>` increasing top to bottom) if that is wanted.

7. wpf/Toasts.cs:132 and 204: `root.Tag` carries the close button for sibling focus restore. It works; a field on `ToastState` (`_states[next].Close`) would be cleaner and avoids the cast. Not required.

8. wpf/Toasts.cs:42 and plan Decision 2: the plan says stack `Margin 0,0,24,24`; the code uses bottom 19 plus 5 on each toast (uniform margin, effective 24, 10 px gaps). Equivalent and measured, and it fixes the stray-10 bug. Just record it in the plan or README so nobody "fixes" it back.

## Verified correct (no action)

- W10: action and persistent toasts get no timer; error is `Assertive`, others `Polite`; mouse and focus pause with remaining-time accounting; Dismiss is idempotent and stops the timer; no handler or timer leak (timer stopped on dismiss, `_states` entry removed, deferred `BeginInvoke` after dismiss exits through the failed `TryGetValue`).
- Focus restore (Toasts.cs:197-210) matches web 3472-3474: the immediate next sibling's close button, else the previous focus when it is still in the visual tree, else clear. The action toast's "Done" sibling has no close button, so it falls back to the previous focus, same as the web.
- Peer fallback (`FromElement ?? CreatePeerForElement`) is right. The LostKeyboardFocus deferral is right (focus-within is still true inside the event).
- Geometry: 24 + 11 + 262 + 11 + 18 = 326 = 360 - 2 - 32, so close x = 325 and body 262/291 hold; VerticalAlignment=Top gives y = 15.
- C# 5: no `?.`, `nameof`, string interpolation or expression-bodied members; csproj has Page/Compile entries for Toasts.xaml, ToastControls.xaml(.cs) and Toasts.cs. No edits outside owned files in this diff. Old AdvancedControls toast block is gone (no ShowToast/ToastHost references in `.cs` or `.xaml`).
- T5 live region is code-only, as the plan allows (marked C).

## What T3 depends on

- README: wpf/README.md line 32 is the "Covered:" coverage list. Add toasts: global host (`Toasts.Install`/`Show`), W10 behaviour, the stack margin 5/5 + 19 detail, and the focus-ring status. Record the wall-time delta (the toast tests now spend about 4.5 s x 4 on `waitMs`).
- F1-F3 call sites, using the existing public `Toasts.Show(type, title, text)`: F1 wpf/SheetGrid.cs:684 (`// ponytail: no shared toast yet`, "Copied" / "N cells copied. Paste into Excel." and "Cells copied. Paste into Excel."); F2 wpf/MainWindow.xaml.cs:39-40 ("Upload complete" / "All files were uploaded.", remove the `ponytail:` note); F3 tables bulk bar (no WPF call site found by grep, so it is still unported: "Rows deleted" / "N row(s) removed." and info "Export started" / "Preparing N row(s) for export."). Existing callers: OverlayControls.xaml.cs:37 and 60.
- T3 should not finish until blocking 1-2 are fixed, otherwise the README would claim W10 coverage the tests do not give.
