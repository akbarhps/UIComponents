# WPF dark mode, Phase 1 (walking skeleton), implementer-hard report

Plan: `plan/wpf/09-dark-mode.md`, Phase 1 only. Agent `theme`. Q1-Q4 = plan defaults. Not committed.

## Files changed
- `wpf/ThemeManager.cs` (new): C3 as written (enum, StorageKey/Mode/IsDark/ThemeChanged, Initialize, SetTheme, env `UI_THEME` override that never touches storage, HKCU `Software\UIComponents.Wpf\ui-theme`, System via `AppsUseLightTheme`, `SystemEvents.UserPreferenceChanged` unhooked on `Application.Exit`, title bar attr 20 then 19, Loaded class handler for new windows). The dark dictionary is loaded lazily (first time dark is needed), so light mode builds nothing extra.
- `wpf/Themes/Tokens.Dark.xaml` (new): 22 brushes + `Shadow1`/`Shadow2`, dark values from web `:root` (lines 20-40).
- `wpf/App.xaml.cs`: `ThemeManager.Initialize();` first line of `OnStartup`.
- `wpf/MainWindow.xaml`: masthead chip Border + TextBlock to `DynamicResource`; new C4 toggle (3 `SegmentButton` radios `ThemeSystem/ThemeLight/ThemeDark`). Nothing else touched.
- `wpf/MainWindow.xaml.cs`: `SyncThemeToggle`, `_syncingTheme` guard, `ThemeToggle_Checked`, subscribe to `ThemeChanged` (unsubscribed on `Closed`).
- `wpf/tests/run-hidden.ps1`: `-Theme light|dark|system|none` (default light) sets/clears `UI_THEME` before anything launches.
- `plan/wpf/theme-captures.ps1`: always passes `-Theme $Theme` (removed the Phase 0 "has -Theme?" detection).
- `plan/wpf/theme-lint.mjs` (new): C0-C5 as specified, `--only`, `RESULT theme-lint failed=K`.

## Step 1: web toggle measurement (Edge --dump-dom, 1400x1100)
`.segment` 181.6 x 37 (3px pad, 2px gap, radius 9); buttons 31 high; widths System 66, Light 53.9, Dark 51.8 (padding 7px 12px, 600 13px). C4 uses Width 66/54/52, Height 31, Margin 2,0,0,0 on 2nd/3rd (SegmentButton template untouched, so sizes are set on the RadioButtons). In the web the chip and toggle share a line; WPF masthead is a vertical stack, so the toggle sits under the chip (`Margin 0,0,0,18`). Masthead is not parity-tracked.
`CreateProcess` env arg in run-hidden is `IntPtr.Zero` (line 38): env inherited, confirmed (dark captures prove it).

## Verify results
```
cmd /c "wpf\build.bat theme"                      Built bin\theme\UIComponents.Wpf.exe
node plan/wpf/theme-lint.mjs --only C1,C2         C1 palette violations=0 / C2 keyset violations=0 / RESULT theme-lint failed=0
   (C1 proven non-vacuous: flipping one hex in Tokens.Dark.xaml gave violations=1, reverted)
node plan/wpf/theme-lint.mjs (full, informational) C0=19 (exactly Focus, OnBrand, OnInk, InkHover, OnDanger, DangerHover, BrandLine, SuccessLine, InfoLine, DangerLine, SageSoft, SageLine, Wash, WashStrong, Scrim, Shimmer, FocusRing, Knob, KnobShadow), C3=522, C4=34, C5=57 (all expected pre-Phase 2/3)
theme-captures -Theme light -> p1-light           captured 36, failed 0
Same base p1-light                                diffs=0
run-hidden -Theme dark   Sections                 chip(90,8)=FF3A2820  page(900,300)=FF1C1A17
run-hidden -Theme light  Sections!ThemeDark       chip(90,8)=FF3A2820  page=FF1C1A17, ThemeDark radio shown checked (viewed)
run-hidden -Theme system Sections                 AppsUseLightTheme=0 on this machine -> page FF1C1A17 (dark) as expected
run-hidden -Theme light  Sections                 chip FFF8ECE4, page FFF5F4EF (light unchanged)
build-tests.bat theme forms; run-hidden -Only forms  RESULT  passed=27  failed=0
```
Forms vs base: the 26 PASS/FAIL names under `AREA forms` in `base.log` and the new log are identical (diff=0). DRIVER: `fallback select:<no id> x1`, the same fallback as the baseline (`x2` full-suite across all areas); no new fallback lines.
Full suite (added after review: App.OnStartup and the taller masthead reach every area), full test exe rebuilt, `AREAS compiled:` lists all 9 areas, log `%TEMP%\theme\p1-full.log`:
`RESULT  passed=533  failed=1` = baseline. The 534 PASS/FAIL names equal `base.log` (diff=0); the one failure is the pre-existing `FAIL resume: still present before remaining expires - remaining=1381 sinceLeft=1469` (Tests.Toasts.cs:242), untouched. DRIVER `fast=281 fallback=4` = baseline (`select:<no id> x2`, `toggle:RowCheck x2`): no new fallback lines.
`-Theme system` check only exercised the dark branch (`AppsUseLightTheme=0` on this machine); the light branch of system resolution was not verified.
No registry value was ever created (`HKCU\Software\UIComponents.Wpf` absent): env override path never persists.

## Observations
- Startup-dark already themes most static references because BAML dictionary items are parsed lazily on first access, after the overlay is merged. The runtime switch (`Sections!ThemeDark`) only switches `DynamicResource` users: in p1-rt-dark-all everything except the masthead stays light. That is exactly what Phase 2 fixes. So p1-dark-all vs p1-rt-dark-all differ everywhere below the masthead by design.
- Standalone (`-Section`) mode hides the toggle too (it sits before the first SectionTitle), so per-section captures are unaffected (diffs=0).

## Inventory: p1-dark (36 per-section dark captures in `%TEMP%\theme\p1-dark`) still wrong or invisible
Found by pixel scan (pure white / pure black) plus viewing Buttons-full-page, Forms and File-input. Not every section viewed.
1. White literal text/icons on `Brand` fills (now light terracotta E08A66, white on it is low contrast): primary/small/large/loading buttons (Buttons 45,241-513,522), Cards, Empty-state, Modal-dialog/Modal-form/Loading-modal/Progress-bar primary buttons, Stepper circles (Stepper 42,345-171,462), Forms checkbox tick. Owner: Controls.xaml / Display.xaml / MainWindow.xaml (Phase 2 `OnBrand`).
2. `DarkButton` ("Dark"): `Ink` fill is now near-white with white label, label invisible (Buttons). Phase 2 `OnInk`/`InkHover`.
3. Avatars: white initials on `Ink` avatar (203,185-213,193). Phase 2 `OnInk`.
4. File-input: "Drop a file here" / "Drop images here" titles are black on dark (default SystemColors foreground), `FileInput.cs`/`Pickers.xaml`. Check the web counterpart colour in Phase 2/3; Q2 says tokenise since web has a dropzone.
5. 9-pixel white marks at ~(502,639-646)/(502,758-765)/(502,773-780) in the three Table-with-* sections: likely the header checkbox tick on a `Brand` fill. Phase 2 (`OnBrand`).
6. Known non-token gaps expected per Q2/Q3: page `ScrollViewer` scrollbar (Aero2), legacy `ClaudeInput`, default focus visuals. Not observed in the 36 captures (scrollbar is excluded from captures).

## Deviations from the plan
- `run-hidden.ps1` also removes `UI_THEME` in the final `finally` (next to the existing `UITEST_DRIVE` cleanup) so it cannot leak into an interactive pwsh session. One line beyond the plan.
- `ThemeManager` loads the dark dictionary lazily rather than at Initialize (equivalent behaviour, light graph untouched).
- `theme-lint` C0 prints one extra `missing:` line with all key names (the 10-line cap would otherwise hide 9 of the 19).

## Blocking questions
None.
