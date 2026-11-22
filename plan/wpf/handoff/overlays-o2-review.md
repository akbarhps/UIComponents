Phase review: overlays O1 fixes + O2 (Loading modal). Diff/code read only, no build or test run.
VERDICT: fix-first (3 blocking, all small).

## Blocking

1. wpf/OverlayControls.xaml:~110 (`LoadingBar` Border): the animated fill is not clipped. The plan says "clipped to the track (ClipToBounds)"; the web has `overflow:hidden`. A WPF Border does not clip its child, so with motion on the 114 px fill slides from x=-114 to x=445 over a 326 px track. It draws over the card padding, past the card edge and onto the scrim (right limit 326+119 > 352). The captures never show it because motion is off under `--capture`. Fix: `ClipToBounds="True"` on `LoadingBar` (rounded corners 2 px are negligible), and verify once with motion on.

2. wpf/OverlayHost.cs:46-58 (`scrimClick` proxy Button): it is a default-template Button and there is no implicit Button style in App resources (grep: none). The Aero2 template sets Background `#FFBEE6FD` on IsMouseOver and `#FFC4E5F6` on IsPressed (those triggers override `Background=Transparent`). Hovering the scrim would turn the whole window an opaque light blue. The hidden desktop has no mouse, so the tests cannot see it. Please hover it once to confirm. Fix: drop the Button (see 3), or give it a flat template (`ControlTemplate` with only a transparent Border).

3. wpf/OverlayHost.cs:46-58 and tests:702-708: the proxy is test-only code in product code. While any dialog is open it adds an unnamed, control-view Button (AutomationId `OverlayScrimClick`, no Name) that covers the window to the UIA tree. Narrator and other screen readers will announce an unlabelled "button" over the page. It is not a tab stop (`Focusable=false`), so it does not break keyboard use. It is also the only path a real click takes: ButtonBase marks the real MouseLeftButtonUp Handled, so the scrim's own handler only fires through the synthetic event raised in `Click`. Fix: remove the Button and raise nothing synthetic. The scrim's `MouseLeftButtonUp` stays the sole handler. The test already has a real-message helper, `PostMouseClick` (tests:156, used at ~1189 by the grid worker): post WM_LBUTTONDOWN/UP at a point on the scrim outside the card (e.g. the window's top-left area plus 10,10; `BoundingRectangle` of the root gives it). That exercises the shipping path with no a11y surface. If that proves not to raise MouseLeftButtonUp in WPF on the hidden desktop, keep the in-process fallback but build it in test code only, not in the product (for example `OverlayHost.Close` called via an internal method). Do not keep an unnamed Button in the layer. If it does stay for a stop-gap, give it `AutomationProperties.AccessibilityView="Raw"` (or `IsHitTestVisible` plus a flat template) so it is not in the control view.

## Should-fix

4. wpf/OverlayControls.xaml:~87 (`LoadingDialog` GroupBox): `FocusVisualStyle` is not nulled. The card takes programmatic focus. If the user opens the loading modal by keyboard (Enter/Space on "Start loading") WPF draws the default dashed focus rectangle around the whole dialog element, which is layer-sized. That is the W2 outline regression, and the web has `.loading-modal:focus{outline:none}`. The test only runs by UIA Invoke, so it cannot see it. Fix: `FocusVisualStyle="{x:Null}"` on `LoadingDialog` (and on `ModalDialog` for its fallback focus path).

5. wpf/OverlayControls.xaml.cs:41-65 (timer): the timer is only stopped inside its own Tick.
   - Another dialog opened while loading (UIA Invoke or, later, Ctrl+K) does `Close(false)` on the loading dialog. The 3 s Tick then calls the global `OverlayHost.Close()`, which closes whatever dialog is open at that moment, and shows "Ready". The web only closes `loadingOverlay`.
   - Re-invoking `OpenLoading` while it is open (`Open` returns early) still creates a second DispatcherTimer. The first Tick stops the field, not `sender`, so the orphan keeps ticking every 3 s for the app's lifetime.
   - Spinner and bar animations are not stopped on any path except the Tick.
   Fix: in `OpenLoading_Click`, stop and replace any existing timer. In the Tick, stop `sender` and close only if the loading dialog is the open one (add `OverlayHost.IsOpen(FrameworkElement)`, or `Close(FrameworkElement)`). Ideally `OverlayHost` raises a `Closed` event, or `OverlayControls` hooks `LoadingDialog.IsVisibleChanged`, to stop the timer and motion on every close path. O3-O5 will want the same hook.

6. wpf/tests/UIAutomationTests.cs:735-753 (L1 Escape check): timing margin. The timer starts inside `Invoke(openLoading)`. The test then spends one `Find` (~2 s per the comments), `AutomationElement.FocusedElement` and several `.Current.*` cross-process reads before posting Escape. On a slow machine the dialog may already be closed, which fails "loading dialog shown", or Escape arrives at about 3 s and the check cannot discriminate. The blocking behaviour (Escape and backdrop ignored while loading) is the main O2 contract. Fix: make `Check` run first (post Escape immediately after `Invoke`, before any `Find`), or lengthen the timer through a test-only knob such as an `--loading-ms` command-line argument. The L1 backdrop-ignored case is not tested and not even printed as CODE-ONLY; add it (it needs a 2nd action, so it needs the longer timer).

## Nits

7. wpf/Themes/Overlays.xaml:127 (`IconButtonGlyph`) and 81 (`IconPlus`): only `StrokeStartLineCap="Round"`. The web symbol has `stroke-linecap:round` (both ends), so the inner line's far end is flat (0.9 px short). Add `StrokeEndLineCap="Round"`. All other icons (Table, Panel, Calendar, Clock, More) now match the web symbols: rect 3..21, calendar y 5..21, clock and more round caps.

8. wpf/OverlayControls.xaml.cs:36-38: the explicit `LoadingDialog.UpdateLayout(); LoadingDialog.Focus();` after `Open` duplicates what `Open`'s BeginInvoke now does (ApplyTemplate + UpdateLayout + focus). Delete both lines and the comment, or keep them only if the opener-focus capture needs them.

9. wpf/OverlayControls.xaml: `ModalDialog` has `KeyboardNavigation.TabNavigation="None"` but `LoadingDialog` does not. The window-level Tab handler makes both irrelevant. Remove from `ModalDialog` or add to both.

10. wpf/OverlayControls.xaml.cs:68-73 (`MotionAllowed`): under `--capture` the 3 s close-timer still runs, so a slow capture of `!OpenLoading` can race the close. Not a defect today (the measured captures succeeded); mention to the capture owner.

11. Loading dialog a11y: the web has `aria-describedby="loadingText"`; the WPF dialog only sets Name and ItemStatus="busy". Optional: `AutomationProperties.HelpText="Please wait a moment while we set things up for you."`.

12. wpf/README.md:32: the "Covered" paragraph still ends "toasts and the dialog launchers — 265 assertions". Counts moved with O1/O2 (305 reported); update if README totals are meant to stay accurate.

## O1-fix checklist (overlays-o1fix-o2.md)

- Backdrop click: done in `Install` (`MouseLeftButtonUp` on the scrim, blocking-aware). Mechanism is the problem, see 1-3.
- Icon paths: done and match the web sprite (see 7 for caps).
- FirstFocusable template: done (`ApplyTemplate` + `UpdateLayout` inside `BeginInvoke`).
- wpf/README DialogWindow row: deleted. `LoadingWindow` and `DialogWindow` have no remaining references (grep clean). csproj has the Overlay entries and no `*Window` entries for the removed classes.
- UIAutomationTests:617 comment: the block was rewritten; no stale `Cycle` comment remains.
- `TabNavigation` restore: done (ReadLocalValue / ClearValue; also restored by the `Close(false)` switch path).
- Open-while-open focus: done (`_previousFocus` captured only when `_openDialog == null`, before the switch). Tab handler walks up from inner elements to the enclosing item: done.

## O2 acceptance

- Card 380, spinner 36, title, 2-line body, bar 326x4 with a 114.094 fill, no focus ring in the capture: matches the web ref by eye (wpf-overlays-loading-open.png vs web-overlays-loading-open.png); the sidebar difference is another worker's area.
- Closes in 3 s plus the "Ready" toast: implemented and tested ("L2 closes within 4s", "L2 Ready toast"). The "Ready" lookup is a whole-tree `FindByName`; "Ready" does not occur elsewhere in the app XAML or code, so it is not vacuous.
- Escape ignored while loading: implemented (`_blocking`), tested with the caveat in 6.
- Test preconditions are not vacuous: `loadingCard` is found after `Invoke` and checked shown and focused, and the closed state is read from the same cached element.
- Not scope creep: AdvancedControls only lost the Loading launcher (the Command palette and Drawer buttons stay for O4/O5); `LoadingWindow` and `DialogWindow` are deleted; no C# 5 violations (no `?.`, `nameof`, interpolation, `=>` in the overlay files); the files are ASCII with no BOM.

## What O3 (modal form) depends on

- `Open(dialog, focus, blocking)` and `Close()` are reused as is. O3 needs the first-field focus: `FirstFocusable` only accepts `ButtonBase` and `TextBox`, which matches the web's `input, button` for `FmName`. A ComboBox is not a candidate for default focus, which matches the web.
- The Tab cycle (`CollectFocusable`) lists ButtonBase, TextBox, ComboBox and PasswordBox. The form's `FieldSelect` (Team) and any template-internal ToggleButton or editable TextBox inside a ComboBox will appear as extra stops (ComboBox + inner element) unless those inner elements have `Focusable=False`. Check this against `Themes/Dropdown.xaml` before O3's Tab test. The element-to-item walk-up in the Tab handler covers focus on an inner element only when the ComboBox itself is in the list.
- W9 (focus the first invalid field, error ARIA) is O3's own job. An `OverlayHost.Close` hook (item 5) would let O3-O5 reset the form state and stop timers on every close path.
- Textarea per Q6 and the error row (26.2 px shift) are in O3's own XAML; nothing in the host blocks them.
