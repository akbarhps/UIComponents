# nav final-fix report (N1 fixes + N2–N7, then final-review fixes)

## Files changed (all nav-owned)
- `wpf/Themes/Controls.xaml` — lines 5–69 only: disabled `Cursor=No` (N1 fix).
- `wpf/Themes/Navigation.xaml` — spinner centre; icons Edit/Copy/Pin/Trash + Ink3 override usage; menu panel/item/divider, tooltip bubble, accordion, stepper, crumb, tab styles; bubble is hoverable (no `IsHitTestVisible=False`).
- `wpf/Popover.cs` (new) — lazy full-window `PopoverLayer`, menu placement/flip/clamp, `Tip.Text/Side/Dismissed`; handlers attached directly in `OnTipChanged`; hide only when host focus, host hover and bubble hover all end; Escape dismiss cleared on blur/mouseleave.
- `wpf/MainWindow.Navigation.cs` (new) — B1 `aria-disabled` semantics (`_working`, focus kept), idle + working spinner rotation, menu/accordion/stepper/tabs/crumbs handlers; no Window-root `Loaded`, no `EnsureMenus`/`EnsureAccordions`.
- `wpf/MainWindow.xaml` — the seven nav blocks only: Action menu after Buttons, Tooltips split out, Accordion/Stepper/Breadcrumbs/Tabs rewritten. Final review edits: no Window-root `Loaded`; accordion `Checked`/`Unchecked` wired in XAML; accordion Borders have no fixed height; vertical stepper markers `VerticalAlignment=Top` + `Panel.ZIndex=1`; menu item icons `Ink3`; menu triggers/panels wire `KeyDown` in XAML; tab `IsTabStop=False` + ItemStatus; step Num initial ItemStatus; named idle spinner with `Loaded`.
- `wpf/Themes/ExtraControls.xaml` — old `Crumb*` styles deleted; implicit styles kept.
- `wpf/tests/Tests.Nav.cs` — all legacy nav assertions replaced; `// Covered:` header corrected.
- `plan/wpf/ref/wpf-nav-*.png` — all idle/state captures re-taken.

No csproj edits (wildcards), no commits, no BOM, no mutation code left. `MainWindow.xaml.cs`, `App.xaml.cs`, `DropdownPrimitives.cs`, `Tests.Core.cs` untouched.

## Final-review items — all blocking and should-fix addressed
Blocking:
1. Accordion fixed `Height=208.4` removed from both Borders. Toggled card is now **984×370** (was 307 and clipped); idle 984×307.
2. Three vertical stepper markers now top-aligned and above the connector line (`Panel.ZIndex=1`); captures show uncut check/"2"/"3".
3. Menu `KeyDown` handlers wired declaratively on both triggers and both panels; `DropdownOutsideClick` objects created lazily in `OpenMenu`. Added M2 (ArrowDown on an unclicked trigger) test.
4. Non-danger menu item icons now `Foreground={StaticResource Ink3}`; re-captured all menu states.

Should-fix:
1. Window-root `Loaded` removed; six accordion toggles use XAML `Checked`/`Unchecked`; `Navigation_Loaded`/`EnsureAccordions`/`_accordionsWired` deleted.
2. `TabSecurity`/`TabBilling` `IsTabStop=False` in XAML; X1 exposes the roving tab stop via `ItemStatus` (UIA still reports all RadioButtons focusable).
3. Shift+Tab now `CloseMenu(true)` (code-only; Shift+Tab cannot be sent by the harness).
4. Up with nothing focused goes to the first item (same as Down).
5. Step status is also on `Step<i>_Num` with initial XAML values; UIA can now observe "current" on a visible element.
6. Focus is captured before `IsEnabled=false` (web W17 read order).
7. Tooltip hides only when host focus, host hover and bubble hover are all false; bubble is hit-testable; C-level, no test.
8. Accordion tests now assert panel peers appear/disappear and restore `AccSingle_0` to On in `finally`.
9. M5 Tab-close test added; M2 added; header corrected (B1 double-click guard and Shift+Tab marked code-only).
10. Vacuous "B1 second click ignored" check removed and noted as code-only.
11. Idle spinner rotates: named XAML spinner + `Loaded` handler + `Spin()` after the Tick rebuild; the stop branch is gone; captures remain static via `IsCapture`.

Nits applied where trivial: `ClearValue` in the Tick, HelpText kept only in XAML, tip handlers attached directly. Nit "primary button invokes is a smoke test" retained per B2.

## Build / `-Only nav` summary
- `wpf\build.bat nav`: OK (only unrelated `CalendarPicker.cs` CS0649 warnings from another area).
- `cd wpf\tests && .\build-tests.bat nav nav`: OK.
- `pwsh -NoProfile -File wpf/tests/run-hidden.ps1 -Agent nav -Only nav`:
  - `RESULT  passed=70  failed=0`, wall 18.1 s and 25.8 s on two consecutive final runs.
  - `AREAS compiled: nav`, `PROFILE finds=104 findMs=20533`, `DRIVER  fast=25 fallback=0` — no fallback lines.
  - One earlier rapid rerun reported `window not found` (app not up yet on the shared hidden desktop); two immediate reruns passed. No assertion failure.
- NODRIVE sign-off: `DRIVER  off`, `RESULT  passed=70  failed=0`, 68.1 s.
- A build failure in `InputControls.xaml(.cs)` (another agent mid-edit) was waited out (~60 s) and rebuilt; no out-of-area failure appeared in `-Only nav`.

## Mutation check (every new/changed assertion)
Temporarily mutated six things in one build/run:
- `TabSecurity` ItemStatus → `tabstop`: **`X1 single tab stop` FAILED**.
- `Step0_Num` ItemStatus → `todo` and removed the Num status update: **`S1 initial current status` and `S1 current follows step` FAILED**.
- Removed `KeyDown="MenuTrigger_KeyDown"` from `MenuActions`: **`M2 ArrowDown opens` and `M2 Escape closes` FAILED**.
- Removed the accordion panel Visibility assignment: **`A3 multi panel opens`, `A2 single panel opens`, `A2 single closes Account` FAILED**.
- Disabled the Tab close branch: **`M5 Tab closes` FAILED**.
Result of that run: `passed=61 failed=9`. All six mutations were reverted and the clean runs above are green.

## Captures + measured diffs
Re-captured into `plan/wpf/ref/`:
- `wpf-nav-accordion.png` 984×307; **`wpf-nav-accordion-toggled.png` 984×370** (target 370 ±1, was 307); `-accordion-focus.png`.
- `wpf-nav-stepper.png` 984×506; `-stepper-next.png`, `-stepper-last.png`. Rows 69.6/69.6/47.6 and connector lines are intact; markers no longer cut.
- `wpf-nav-menu.png`, `-menu-open.png`, `-menu-down2.png`, `-menu-compact-open.png`, `-menu-picked.png` — icons now Ink3; panel right edge matches the trigger right edge (the accepted ~1.3 px left-edge delta remains).
- Earlier captures unchanged and still valid: buttons, tooltips (incl. Right bubble over Save), breadcrumbs, tabs, progress.
- Card heights vs web: buttons 414 vs 412.4, menu 104, tooltips 104, accordion 307/370, stepper 506 vs 506.1, crumbs 83 vs 82.8, tabs 143 vs 143.4 — all within ±1.

## Deviations / notes
- X1's roving tab stop is asserted through `AutomationProperties.ItemStatus` because UIA reports all `RadioButton`s as `IsKeyboardFocusable` even with `IsTabStop=False`.
- The accordion panel and step check AutomationIds live on chrome-free `GroupBox` wrappers: plain `Panel`/`ContentControl` have no UIA peer.
- Shift+Tab (M5), the B1 double-click guard, tooltip hover bridging and idle spinner motion are code-only/C-level; the harness cannot drive modifier keys or mouse and captures freeze animation.
- The 1.3 px menu-panel left-edge delta is the reviewer-accepted button-width inheritance.
- No blocking questions.

Skipped: full-suite/full-build runs (reviewer-only); re-running toasts/overlays/pickers/tables captures after the global button width change (already reported in N1).
