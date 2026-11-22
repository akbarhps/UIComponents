# pickers — P1/P2 fixes, P10 isolation, P3–P5 report

## Files changed
- `wpf/CalendarPicker.cs` — all review fixes (300-wide shell, mode-aware selection, body-scoped focus fix-up, Tab to Prev, keyboard focus ring, full-cell today ring, single Loaded focus, safe mouse-press focus, no dead Today time defaults) + P3 time row/inputs, unclamped day-copy, clamped Apply, time retention across Clear.
- `wpf/Themes/Pickers.xaml` — `PickerCell` focus ring + stretched content (full-size today ring); `PickerFileDrop` (dashed 3/2 rectangle, hover/drag/focus states) and `PickerFileItem` styles.
- `wpf/PickerControls.xaml` — swapped Date picker card rows (95.8/73.8); new "Date & time picker" section with 5 pickers; new "File input" section with two `FileInput` controls.
- `wpf/PickerControls.xaml.cs` — removed `Browse_Click` and unused usings.
- `wpf/FileInput.cs` (new) — code-only file control: drop zone, shared image filter, de-dupe, size labels, count, remove rows, drag/drop.
- `wpf/tests/Tests.Pickers.cs` — P1/P2 fixes, self-contained P10, tab-stop checks, Today/reopen and Prev/Next for month/year, full P3 time assertions, P4 idle assertions. `// Covered:` header updated.
- Refs re-captured: `wpf-pickers-date{,-open,-selected,-values}.png`, `wpf-pickers-month-open.png`, `wpf-pickers-year-open.png`, `wpf-pickers-datetime{,-open,-seconds-open,-values}.png`, `wpf-file-input.png`.

## Build / area test results
All builds `wpf\build.bat pickers`; all tests `build-tests.bat pickers pickers` then `run-hidden.ps1 -Agent pickers -Only pickers`.
- After fixes: **53 passed / 0 failed**, 49.6 s.
- After P3: **70 passed / 0 failed**, 66.0 s.
- Final (after P4/P5): **72 passed / 0 failed**, 48.2 s; `DRIVER fast=47 fallback=0`, `PROFILE finds=124 findMs=48060`. No stale dialogs/panels left open.
- Two intervening failures were in `overlays` (build error `OpenFormModal_Click`, then startup `OverlayControls.SetFormError` NRE). Waited ~60 s and rebuilt each time; both resolved by that agent. Never edited their files.

## Captures / measured geometry
- Date picker card 984×232 (web 231.6); triggers 450×46 at rel y 58.8/154.6; panel 300 wide at +52, 316 high (web 316.6); month 261 (262), year 299 (299).
- Date & time card 984×327 (web 327.4); time panel 300×397 (web 398); time row 71.4 at rel y 260.6, inputs 132 (84.7 with seconds) ×34.2 at y 297.8.
- File input card 984×237 (web 237.4); drop zones 450×135.6 with dashed 3/2 radius 12; count hidden at zero.

## Deviations / known gaps
- Real mouse presses cannot be simulated on the hidden desktop: posted `WM_LBUTTONDOWN/UP` (PostMessage and SendMessageTimeout, with/without MK_LBUTTON, client coordinates) never reached the app — verified by instrumenting `Panel_PreviewMouseLeftButtonDown`. The mouse-press focus fix is therefore code-only; a driver `mouse` verb would be needed to test it.
- P10 is self-contained but uses `Drv.Focus(FileSingle_Drop)` (outside-root focusout, same close path) instead of a real outside click. It failed deterministically before the fix and passes alone now.
- W7 selected state remains `ItemStatus="selected"` on a Button (accepted deviation from P1 review).
- FI1–FI5 file dialog, drag/drop, size labels and remove rows are code-only (no seeding hook, per Q5); the area test asserts drop zones, focusability and hidden counts only.
- Month/year `Today` now uses mode-aware selection; `IsSelected` compares Y for year, Y+M for month, Y+M+D for date.
- csproj untouched (wildcards compile `FileInput.cs`); no BOM in any new/edited file.

## Blocking questions
None. Suggestion for the reviewer: add a driver `mouse` verb if real mouse-down behaviour should be asserted for all adorner panels.
