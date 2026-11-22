# overlays: O1fix+O2 verdict = fix-first, then O3
Review: plan/wpf/handoff/overlays-o2-review.md (read it). Rules unchanged: agent "overlays", build.bat overlays, run-hidden.ps1 only, never commit, never kill by name. Minimise UIA actions. Re-open UIAutomationTests.cs after your final run.

## Fixes
1. BLOCKING OverlayControls.xaml LoadingBar Border: add ClipToBounds="True" (fill slides x -114..445 and draws outside the 326 track when motion is on; captures hide it).
2. BLOCKING OverlayHost.cs:46-58 scrim proxy Button uses the default Aero2 template: hover/press would tint the whole window. And it is test-only code in product (unnamed UIA button over the window while a dialog is open, announced by screen readers). REMOVE the proxy Button; keep the scrim's MouseLeftButtonUp as the only handler. Test the backdrop click with the existing PostMouseClick helper (tests:156) posting a real click on the scrim. Update tests:702-708.
3. LoadingDialog: FocusVisualStyle="{x:Null}" (W2 ring regression when opened with Enter/Space).
4. Timer/motion lifecycle: stop and replace any existing timer when OpenLoading runs; Tick closes only if the loading dialog is the open one; stop timer + animations on every close path (a Closed hook). Under --capture do not run the 3 s close timer.
5. L1 test: post Escape right after Invoke (margin vs 3 s timer is thin after a 2 s Find); add the backdrop-ignored-while-loading case (needs a longer test-only timer) or print CODE-ONLY explicitly.
Nits: IconButtonGlyph/IconPlus caps on both ends; drop redundant UpdateLayout()/Focus() in OpenLoading_Click; TabNavigation on LoadingDialog; HelpText for aria-describedby; README Covered count.
UIA focus pattern: assert element.Current.HasKeyboardFocus (works), not the global AutomationElement.FocusedElement (reports the adorner Pane).

## Heads-up
nav's button restyle (Controls.xaml 5-69) shifts full-size button widths +2px; don't edit it.

## Then O3 (modal form) of plan/wpf/07-overlays-pickers.md
Reuses Open(dialog, focus, blocking) and Close(). Check Themes/Dropdown.xaml for the Team select before the Tab test (ComboBox + inner toggles add Tab stops). W9 + error row are yours. Stop after O3, report in README format.

## Applies after task 08 (tests split) is done
Read handoff/tests-split.md first (tooling change). Ignore the 'Re-open UIAutomationTests.cs' line above: that file is gone. The 'README Covered count' nit is done by the `// Covered:` header of `Tests.Overlays.cs`: do not edit the README.
Area `overlays`, file `wpf/tests/Tests.Overlays.cs` (Modal dialog, Loading modal, Dialog launchers). Fix 2's backdrop test uses `PostMouseClick`/`RectPart` from Core. The O0 probe moved to `Tests.Forms.cs`: leave it. Coordinate AdvancedControls' 'Dialogs' removal (O4-O6) **after** tables round 5, not at the same time (both edit `AdvancedControls.xaml(.cs)`).
