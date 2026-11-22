# overlays report2 — final-review fixes

## Blocking fixes
1. Palette search row `BorderThickness` right→bottom (`0,0,0,1`); footer left→top (`0,1,0,0`). Separators now visible in `wpf-overlays-cmdk-open.png`.
2. `CmdkChip` padding `3,6`→`6,3` (WPF Thickness L/R,T/B). "ESC" and "Ctrl N" chips no longer clipped.
3. `CmdkInput` template gained a Tag-bound placeholder TextBlock with a `Text=""` trigger, like `FieldInputTemplate`. Placeholder "Search for an action…" is visible in the capture; UIA cannot expose TextBox template children, so this is printed CODE-ONLY in the tests and verified by capture.
4. `GoToSection` now searches only `ContentScroll` and only `SectionTitle`-styled TextBlocks; "Go to Buttons" targets the section title, not the sidebar label.
5. Loading L1 backdrop: removed the no-op `PostMouseClick` and the fake pass; prints `CODE-ONLY  backdrop ignored while loading (no mouse on hidden desktop)`.

## Should-fix
6. L1 Escape is now posted **after** the root is shown and the card is focused; the assertion is `!WaitFor(() => loadingDialog.Current.IsOffscreen, 300)`. Mutation (removing `_blocking` from `OverlayLayer_KeyDown`) makes it fail.
7. Added K3 Down (`Down` → item 1 active), K4 Enter (Enter runs "Invite a member"), and K6 ("Go to Buttons" changes the content scroll offset). All wait for posted keys before asserting.
8. F3 now asserts the toast body `“Atlas” is ready.` and the form reset after a valid submit. Mutation (changing the toast text) makes it fail.
9. Added the form-modal Tab cycle (FmName → FmEmail → FmRole → FmDesc → Cancel → Submit → Close → wrap) and the confirm's scoped cycle (Keep member → Delete member → wrap). Disabling the window Tab handler fails 9 Tab assertions.
10. Nits: `EmailOk` reused in `SubmitFormModal`; Ctrl+K sets `Handled` once; `LoadingBar` gets a rounded `RectangleGeometry` clip; report wording corrected.

## Files changed
- `wpf/OverlayControls.xaml`, `wpf/OverlayControls.xaml.cs`, `wpf/Themes/Overlays.xaml`, `wpf/tests/Tests.Overlays.cs`. No shared files owned by other agents were edited.

## Build / test
- `wpf\build.bat overlays` OK; `cd wpf\tests && .\build-tests.bat overlays overlays` OK; C# 5, no BOM, no mutation/debug text left.
- Final `-Only overlays`: **RESULT passed=95 failed=0** (124.2 s, `finds=152`, `DRIVER fast=52 fallback=0`).
- Final real-UIA check (`UITEST_NODRIVE=1`): **RESULT passed=95 failed=0** (235.3 s).
- Code-only lines: modal backdrop click, loading backdrop, palette placeholder (capture-verified).

## Mutations run (each reverted; clean build re-run afterwards)
- Removed `_blocking` guard in `OverlayLayer_KeyDown` → `L1 Escape does not close loading` + `L2 Ready toast shown` FAIL.
- Disabled `OverlayWindow_PreviewKeyDown` (early `return`) → 9 assertions FAIL (modal Tab ×3, form Tab ×7, confirm Tab ×2 subset).
- Replaced `CmdkInput.AddHandler(..., handledEventsToo:true)` with `+=` → K3 Down, K3 Up and K4 Enter FAIL.
- Disabled `target.BringIntoView()` in `GoToSection` → `K6 Go to Buttons` FAIL.
- Changed the created toast text → `F3 toast text` FAIL.
- A stale incremental build briefly kept the mutant toast string after a restore; forced a rebuild by touching `OverlayControls.xaml.cs`. Clean final builds confirmed.

## Captures
- Re-captured all palette refs: `wpf-overlays-cmdk.png`, `-open.png`, `-down2.png`, `-filter.png`, `-empty.png`. Open ref shows the placeholder, both horizontal separators, unclipped ESC/Ctrl N chips, 560×436.6 card at 12vh, 8 items and the active first row. Other O1–O4 refs unchanged from report 1.

## Blocking questions
- None. Known non-ownership: pickers `P2 selected cell focused on reopen` in full-suite runs.
