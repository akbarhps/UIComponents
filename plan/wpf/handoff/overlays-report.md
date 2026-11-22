# overlays report — O2 fixes + O3-O6 (agent `overlays`)

## Files changed
- `wpf/OverlayHost.cs` — removed the scrim proxy Button (test-only code / a11y surface); scrim keeps `MouseLeftButtonUp` only. Added `IsOpen`/`IsAnyOpen`/`SetTabScope`; fixed focus capture on dialog switches; saved/restored `TabNavigation`; Tab cycle walks up from template inner elements and honours the confirm scope; focus target resolved in `BeginInvoke` with `ApplyTemplate`/`UpdateLayout`.
- `wpf/OverlayControls.xaml(.cs)` — all five sections and six dialogs. O2: `ClipToBounds` on `LoadingBar`, `FocusVisualStyle={x:Null}` on both base dialogs, `HelpText`, tab-scope on `LoadingDialog`, timer stopped/replaced on open and on every close path, tick closes only if that dialog is open, no timer under `--capture`. O3: form modal with independent errors, W9 first-invalid focus and `ItemStatus`/`HelpText`, reset, Enter-to-submit. O4: invite/edit drawers with confirm step and scoped Escape/Tab. O5: palette with 8 commands, groups, filter/wrap/Enter/click, Ctrl+K, clip and 12vh position. Removed duplicate 7px field margins (FieldLabel/FieldHint already provide them).
- `wpf/Themes/Overlays.xaml` — modal/form/error/palette styles; sprite icons corrected to the web paths and round caps; `IconPlus`/`IconButtonGlyph` caps on both ends.
- `wpf/AdvancedControls.xaml(.cs)` — deleted the three OS-window launcher buttons and their handlers.
- Deleted `DialogWindow`, `LoadingWindow`, `DrawerWindow`, `CommandPaletteWindow` (.xaml/.cs); no csproj entries (wildcards).
- `wpf/tests/Tests.Overlays.cs` — `// Covered:` header now lists all five sections; O1-O5 assertions rewritten for the in-process driver (`Invoke`, `SetValue`, `FastToggle`, `Drv.Focus`, `WaitFor`; no fixed sleeps, no `AutomationElement.FocusedElement`, no new fallbacks). Backdrop-click prints CODE-ONLY.
- Did not edit: `Tests.Core.cs`, `Controls.xaml`, App/MainWindow partials owned by others, README (header replaces counts).

## Build / test
- `wpf\build.bat overlays` OK; `cd wpf\tests && .\build-tests.bat overlays overlays` OK; C# 5, no BOM.
- Final `run-hidden.ps1 -Agent overlays -Only overlays`: **81 passed, 0 failed**, 106.6 s, `PROFILE finds=121 findMs=104228`, `DRIVER fast=44 fallback=0`.
- Required real-UIA check (`UITEST_NODRIVE=1`): **81 passed, 0 failed**, 179.6 s, no fallbacks.
- Quoted failures: none in this area. Backdrop click is `CODE-ONLY  backdrop click closes modal (no mouse on hidden desktop)`; `PostMouseClick` produced no WPF down/up at all (verified with a temporary probe, removed).
- One concurrent picker half-edit (`FileInput.cs`/`PickerControls.xaml.cs`) broke the build; waited ~60 s and rebuilt successfully.

## Captures (measured vs `web-*` refs)
- `wpf-overlays-modal*.png`: outer card 460×207 vs web 460×206.8; centred within 0.5 px; Danger button 125×42 at rel (307,137) vs (306,136); X ring rel (396,22)–(435,60) vs (396,22)–(435,61); scrim samples page `(155,153,147)`, top `(161,159,156)`, sidebar `(151,147,138)` identical; escaped state `Bg (245,244,239)`, no dialog.
- `wpf-overlays-loading*.png`: card 380×213 vs web 380×214; bar fill rel (27,182) 113×3 vs web 112×4; spinner Brand pixel count 54 in both, arc 24×6 vs 24×7; no focus outline.
- `wpf-overlays-form-modal*.png`: card 540×516 vs web 540×515.8 after removing the doubled 7px field gaps; error capture shows both rows and the 26.2px downward shift.
- `wpf-overlays-drawer*.png`: drawer 440 (439 white interior) flush right, matching web; invite/edit/confirm states captured and checked by eye; D10 Escape capture confirms the drawer stays open.
- `wpf-overlays-cmdk*.png`: card 560×436.6 (white interior 560×435 vs web 558×435), top at 12% of layer (128px at 1061 vs 133px at 1100); open/down2/filter/empty states captured.

## Deviations
- Backdrop click and Shift+Tab and Ctrl+K are code-only (no mouse/modifier state on the hidden desktop). The scrim handler is the shipping path; the earlier UIA proxy was removed.
- Tab trap uses the window `PreviewKeyDown` handler with `handledEventsToo`, not `TabNavigation=Cycle`, because WPF's window-level navigation otherwise moves focus to a generic Pane. `TabNavigation=None` on dialogs is belt-and-braces.
- The palette input registers KeyDown via `AddHandler(..., true)` because TextBox marks arrow keys handled in its class handler.
- Spinner/bar motion is guarded by `--capture`/reduced motion and the bar is clipped; the 3s close timer is disabled under `--capture`.
- Palette position follows the fixed W1 web (top-aligned 12vh), not the plan's older centred spec.

## Blocking questions
- None. Known non-ownership: pickers `P2 selected cell focused on reopen` failed in an earlier full run; all current area runs are green.
